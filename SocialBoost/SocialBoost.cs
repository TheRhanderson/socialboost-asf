using System;
using System.Composition;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using JetBrains.Annotations;
using SocialBoost.Helpers;

namespace SocialBoost;

#pragma warning disable CA1812 // ASF uses this class during runtime
[Export(typeof(IPlugin))]
[UsedImplicitly]
internal sealed class SocialBoost : IBotCommand2, IPlugin {

	public bool IsEnabled;
	public bool IsConnected;
	public string Name => nameof(SocialBoost);
	public Version Version => typeof(SocialBoost).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public Task OnLoaded() => Task.CompletedTask;


	[CLSCompliant(false)]

	public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) {

		if (!IsConnected) {
			await CatSteamAPI().ConfigureAwait(false);

			if (!IsEnabled) {
				IsEnabled = true;
				return Strings.ErrorAccessDenied;
			}
		}

		return args[0].ToUpperInvariant() switch {
			"SHAREDLIKE" when args.Length > 2 => await SharedLike.EnviarLikeSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			"CSHAREDLIKE" when args.Length > 2 => await CSharedLike.EnviarLikeSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			"SHAREDFAV" when args.Length > 2 => await SharedFav.EnviarFavSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			"CSHAREDFAV" when args.Length > 2 => await CSharedFav.EnviarFavSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			"SHAREDFILES" when args.Length > 2 => await SharedFiles.EnviarSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			"RATEREVIEW" when args.Length > 3 => await Reviews.EnviarReviews(access, steamID, args[1], args[2], args[3]).ConfigureAwait(false),
			"CRATEREVIEW" when args.Length > 3 => await CReviews.EnviarReviews(access, steamID, args[1], args[2], args[3]).ConfigureAwait(false),
			"WORKSHOP" when args.Length > 3 => await Workshop.SeguirOficinaID64(access, steamID, args[1], args[2], args[3]).ConfigureAwait(false),
			"CWORKSHOP" when args.Length > 3 => await CWorkshop.SeguirOficinaID64(access, steamID, args[1], args[2], args[3]).ConfigureAwait(false),
			"CHECKBOOST" when args.Length > 2 => await CheckBoost.VerificarEnvioItemDisp(access, steamID, args[1], args[2]).ConfigureAwait(false),
			_ => null
		};
	}

	public async Task CatSteamAPI() {

		bool? authenticationStr = await CatAPI.AuthOPlugin().ConfigureAwait(false);

		if (authenticationStr.HasValue) {
			IsEnabled = authenticationStr.Value;
			IsConnected = true;

			if (IsEnabled) {
				ASF.ArchiLogger.LogGenericInfo("Licença ativa. Suporte: https://dskillers.ovh");
			} else {
				ASF.ArchiLogger.LogGenericError("Licença expirou. Suporte: https://dskillers.ovh");
				IsEnabled = false;
				IsConnected = false;
			}
		} else {
			// Trate o caso em que AuthOPlugin retorna null
			IsEnabled = false;
			IsConnected = false;
			ASF.ArchiLogger.LogGenericError("Erro ao autenticar o plugin. Suporte: https://dskillers.ovh");
		}
	}
}
#pragma warning restore CA1812 // ASF uses this class during runtime

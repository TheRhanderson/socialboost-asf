using System;
using System.Composition;
using System.Threading.Tasks;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using JetBrains.Annotations;

namespace SocialBoost;

#pragma warning disable CA1812 // ASF uses this class during runtime
[Export(typeof(IPlugin))]
[UsedImplicitly]
internal sealed class SocialBoost : IBotCommand2, IPlugin {
	public string Name => nameof(SocialBoost);
	public Version Version => typeof(SocialBoost).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

	public Task OnLoaded() =>
		Task.CompletedTask;

	[CLSCompliant(false)]
	public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) =>
		args[0].ToUpperInvariant() switch {
			"SHAREDLIKE" when args.Length > 2 => await SharedLike.EnviarLikeSharedfiles(access, steamID, args[1], args[2]).ConfigureAwait(false),
			_ => null
		};

}
#pragma warning restore CA1812 // ASF uses this class during runtime

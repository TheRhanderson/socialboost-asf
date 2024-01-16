using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;

namespace SocialBoost;
internal class SharedLike {




	public static async Task<string?> EnviarLikeSharedfiles(Bot bot, EAccess access, string id) {

		if (access < EAccess.Master) {
			return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		if (bot.IsAccountLimited) {
			return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);
		}

		string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE => {id} (Enviando)");

		if (string.IsNullOrEmpty(sessionId)) {
			return Commands.FormatBotResponse(Strings.BotLoggedOff, bot.BotName);
		}

		Uri request = new(ArchiWebHandler.SteamCommunityURL, "/sharedfiles/voteup");

		Dictionary<string, string> data = new(2)
		{
		{ "id", id },
		{ "sessionid", sessionId }
		};

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE => {id} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {id} — Like" : Strings.WarningFailed);

	}

	public static async Task<string?> EnviarLikeSharedfiles(EAccess access, ulong steamID, string botNames, string argument) {

		if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(argument)) {
			ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(argument));

			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, botNames) : null;
		}

		IList<string?> results = await Utilities.InParallel(bots.Select(bot => EnviarLikeSharedfiles(bot, Commands.GetProxyAccess(bot, access, steamID), argument))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");

		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}


	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);


	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

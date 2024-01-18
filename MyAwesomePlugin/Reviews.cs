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
internal static class Reviews {


	public static async Task<string?> EnviarRateReviews(Bot bot, EAccess access, string url, string idreview, string action) {

		if (access < EAccess.Master) {
			return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		if (bot.IsAccountLimited) {
			return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);
		}

		Uri request = new(ArchiWebHandler.SteamCommunityURL, $"/userreviews/rate/{idreview}");
		Uri request2 = new(ArchiWebHandler.SteamCommunityURL, $"/userreviews/votetag/{idreview}");
		Uri requestViewPage = new(url);

		string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|REVIEWS|{(action == "1" ? "UTIL" : "ENGRACADO")} => (Enviando)");

		if (string.IsNullOrEmpty(sessionId)) {
			return Commands.FormatBotResponse(Strings.BotLoggedOff, bot.BotName);
		}


		Dictionary<string, string> data1 = new(2)
		{
		{ "rateup", "true" },
		{ "sessionid", sessionId }
		};

		Dictionary<string, string> data2 = new(3)
{
		{ "rateup", "true" },
		{ "tagid", "1" },
		{ "sessionid", sessionId }
		};

		bool postSuccess;

		Dictionary<string, string> dataToUse = action == "1" ? data1 : data2;
		postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(action == "1" ? request : request2, data: dataToUse, referer: requestViewPage).ConfigureAwait(false);


		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|REVIEWS|{(action == "1" ? "UTIL" : "ENGRACADO")} => (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {idreview} — {(action == "1" ? "Util" : "Engracado")}" : Strings.WarningFailed);

	}

	public static async Task<string?> EnviarReviews(EAccess access, ulong steamID, string botNames, string argument, string argument2) {

		if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(argument)) {
			ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(argument));

			return null;
		}

		if (argument2 is not "1" and not "2") {
			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, botNames) : null;
		}

		bool? logger = await DSKLogger.CompartilharAtividade($"RATEREVIEW-{(argument2 == "1" ? "UTIL" : "ENGRACADO")}-{argument}").ConfigureAwait(false);

		string? argument3 = await SessionHelper.FetchReviewID(argument).ConfigureAwait(false);

		Bot firstBot = bots.First();

		if (string.IsNullOrEmpty(argument3)) {
			return Commands.FormatBotResponse("Erro ao determinar id de review", firstBot.BotName);
		}

		IList<string?> results = await Utilities.InParallel(bots.Select(bot => EnviarRateReviews(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument3, argument2))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);

	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

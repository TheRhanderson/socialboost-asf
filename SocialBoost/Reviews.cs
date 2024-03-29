using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;
using SocialBoost.Helpers;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;

namespace SocialBoost;
internal static class Reviews {


	public static async Task<string?> EnviarRateReviews(Bot bot, EAccess access, string url, string idreview, string action) {

		if (access < EAccess.Master) {
			return null;
		}

		bool? botUtilizadoAnteriormente = await DbHelper.VerificarEnvioItem(bot.BotName, "REVIEWS", idreview).ConfigureAwait(false);

		if (botUtilizadoAnteriormente == true && action != "3") {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {idreview} — Esta conta já foi usada antes!");
		}

		if (botUtilizadoAnteriormente == false && action == "3") {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {idreview} — Bot nunca avaliou este review!");
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		if (bot.IsAccountLimited) {
			return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);
		}

		Uri request = new(SteamCommunityURL, $"/userreviews/rate/{idreview}");
		Uri request2 = new(SteamCommunityURL, $"/userreviews/votetag/{idreview}");
		Uri requestViewPage = new(url);

		//string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|REVIEWS|{(action == "1" ? "UTIL" : (action == "2" ? "ENGRACADO" : "NAO UTIL"))} => (Enviando)");

		Dictionary<string, string> data1 = new(1)
		{
		{ "rateup", "true" }
		};

		Dictionary<string, string> data2 = new(2)
		{
		{ "rateup", "true" },
		{ "tagid", "1" }
		};

		Dictionary<string, string> data3 = new(1)
		{
		{ "rateup", "false" }
		};

		bool postSuccess;
		Uri requestToUse;
		string tipoReview;

		Dictionary<string, string> dataToUse;

		if (action == "1") {
			dataToUse = data1;
			requestToUse = request;
			tipoReview = "Util";
		} else if (action == "2") {
			dataToUse = data2;
			requestToUse = request2;
			tipoReview = "Engracado";
		} else if (action == "3") {
			dataToUse = data3;
			requestToUse = request;
			tipoReview = "Nao util";
		} else {
			return null;
		}

		postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(requestToUse, data: dataToUse, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|REVIEWS|{tipoReview.ToUpperInvariant()} => (OK)");

		if (dataToUse != data3) {
			bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "REVIEWS", idreview).ConfigureAwait(false);

			if (!salvaItem.HasValue) {
				return null;
			}

		} else if (dataToUse == data3) {
			bool? delItem = await DbHelper.RemoverItem(bot.BotName, "REVIEWS", idreview).ConfigureAwait(false);

			if (!delItem.HasValue) {
				return null;
			}
		}

		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {idreview} — {tipoReview}" : Strings.WarningFailed);

	}

	public static async Task<string?> EnviarReviews(EAccess access, ulong steamID, string botNames, string argument, string argument2) {

		if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(argument)) {
			ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(argument));

			return null;
		}

		if (argument2 is not "1" and not "2" and not "3") {
			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, botNames) : null;
		}

		await DSKLogger.CompartilharAtividade($"RATEREVIEW-{(argument2 == "1" ? "UTIL" : (argument2 == "2" ? "ENGRACADO" : "NAO UTIL"))}-{argument}").ConfigureAwait(false);

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

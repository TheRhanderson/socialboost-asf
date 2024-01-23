using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;

namespace SocialBoost;
internal static class Workshop {


	public static async Task<string?> SeguirOficinaPerfilSteam(Bot bot, EAccess access, string urlPerfil, string steamAlvo) {

		if (access < EAccess.Master) {
			//return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
			return null;
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		//if (bot.IsAccountLimited) {
		//	return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);   // isso também funciona com contas limitada
		//}

		string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|WORKSHOP|FOLLOW => {steamAlvo} (Enviando)");

		if (string.IsNullOrEmpty(sessionId)) {
			return Commands.FormatBotResponse(Strings.BotLoggedOff, bot.BotName);
		}

		Uri request = new($"{urlPerfil}/followuser");
		Uri requestViewPage = new($"{urlPerfil}/myworkshopfiles/");

		Dictionary<string, string> data = new(2)
		{
		{ "steamid", steamAlvo },
		{ "sessionid", sessionId }
		};

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|WORKSHOP|FOLLOW => {steamAlvo} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {steamAlvo} — Follower" : Strings.WarningFailed);

	}

	public static async Task<string?> SeguirOficinaID64(EAccess access, ulong steamID, string botNames, string argument) {

		if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(argument)) {
			ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(argument));

			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, botNames) : null;
		}

		string? argument2 = await SessionHelper.FetchSteamID64($"{argument}").ConfigureAwait(false);

		if (argument2 == null) {
			return null;
		}


		bool? logger = await DSKLogger.CompartilharAtividade($"Workshop-FOLLOW-{argument2}").ConfigureAwait(false);
		IList<string?> results = await Utilities.InParallel(bots.Select(bot => SeguirOficinaPerfilSteam(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument2))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);

	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

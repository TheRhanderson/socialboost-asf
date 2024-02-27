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
internal static class Workshop {

	public static int NumEnviosDesejados = 1000;

	public static async Task<string?> SeguirOficinaPerfilSteam(Bot bot, EAccess access, string urlPerfil, string steamAlvo, string action) {

		if (access < EAccess.Master) {
			return null;
		}

		bool? botUtilizadoAnteriormente = await DbHelper.VerificarEnvioItem(bot.BotName, "WORKSHOP", steamAlvo).ConfigureAwait(false);

		if (botUtilizadoAnteriormente == true && action != "2") {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {steamAlvo} — Bot já segue o perfil!");
		}

		if (botUtilizadoAnteriormente == false && action == "2") {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {steamAlvo} — Bot não segue o perfil!");
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		//string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|WORKSHOP|{(action == "1" ? "FOLLOW" : "UNFOLLOW")} => {steamAlvo} (Enviando)");

		Uri request = new($"{urlPerfil}/followuser");
		Uri request2 = new($"{urlPerfil}/unfollowuser");
		Uri requestViewPage = new($"{urlPerfil}/myworkshopfiles/");

		Dictionary<string, string> data = new(1)
		{
		{ "steamid", steamAlvo }
		};

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(action == "1" ? request : request2, data: data, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		if (action == "1") {
			bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "WORKSHOP", steamAlvo).ConfigureAwait(false);

			if (!salvaItem.HasValue) {
				return null;
			}

		} else if (action == "2") {
			bool? delItem = await DbHelper.RemoverItem(bot.BotName, "WORKSHOP", steamAlvo).ConfigureAwait(false);

			if (!delItem.HasValue) {
				return null;
			}
		}


		bot.ArchiLogger.LogGenericInfo($"SocialBoost|WORKSHOP|{(action == "1" ? "FOLLOW" : "UNFOLLOW")} => {steamAlvo} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {steamAlvo} — {(action == "1" ? "Follow" : "Unfollow")}" : Strings.WarningFailed);

	}

	public static async Task<string?> SeguirOficinaID64(EAccess access, ulong steamID, string botNames, string argument, string argument2) {

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

		string? argument3 = await SessionHelper.FetchSteamID64($"{argument}").ConfigureAwait(false);

		if (argument3 == null) {
			return null;
		}

		await DSKLogger.CompartilharAtividade($"Workshop-{(argument2 == "1" ? "FOLLOW" : "UNFOLLOW")}-{argument3}").ConfigureAwait(false);

		IList<string?> results = await Utilities.InParallel(bots.Select(bot => SeguirOficinaPerfilSteam(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument3, argument2))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);

	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

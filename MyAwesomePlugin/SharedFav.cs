using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Web.Responses;

namespace SocialBoost;
internal static class SharedFav {


	public static async Task<string?> EnviarFavSharedfiles(Bot bot, EAccess access, string id, string appID) {

		if (access < EAccess.Master) {
			return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		//if (bot.IsAccountLimited) {
		//	return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);   /// Isso funciona com contas limitadas.
		//}

		string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|FAV => {id} (Enviando)");

		if (string.IsNullOrEmpty(sessionId)) {
			return Commands.FormatBotResponse(Strings.BotLoggedOff, bot.BotName);
		}

		Uri request = new(ArchiWebHandler.SteamCommunityURL, "/sharedfiles/favorite");
		Uri requestViewPage = new(ArchiWebHandler.SteamCommunityURL, $"/sharedfiles/filedetails/?id={id}");

		Dictionary<string, string> data = new(3)
		{
		{ "id", id },
		{ "appid", appID },
		{ "sessionid", sessionId }
		};

		HtmlDocumentResponse? response = await VisualizarPagina(bot, requestViewPage).ConfigureAwait(false);
		if (response == null) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|FAV => {id} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {id} — Favorito" : Strings.WarningFailed);

	}

	internal static async Task<HtmlDocumentResponse?> VisualizarPagina(Bot bot, Uri requestViewPage) {
		HtmlDocumentResponse? response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(requestViewPage, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);
		return response;
	}

	public static async Task<string?> EnviarFavSharedfiles(EAccess access, ulong steamID, string botNames, string argument) {

		if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(argument)) {
			ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(argument));

			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots(botNames);

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, botNames) : null;
		}

		string? argument2 = await SessionHelper.FetchAppIDShared($"https://steamcommunity.com/sharedfiles/filedetails/?id={argument}").ConfigureAwait(false);

		Bot firstBot = bots.First();

		if (string.IsNullOrEmpty(argument2)) {
			return Commands.FormatBotResponse("Erro ao determinar appid do sharedfiles", firstBot.BotName);
		}

		bool? logger = await DSKLogger.CompartilharAtividade($"Sharedfiles-FAV-{argument}").ConfigureAwait(false);
		IList<string?> results = await Utilities.InParallel(bots.Select(bot => EnviarFavSharedfiles(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument2))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);


	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

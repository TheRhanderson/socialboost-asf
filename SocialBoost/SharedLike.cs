using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Web.Responses;
using SocialBoost.Helpers;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;

namespace SocialBoost;
internal static class SharedLike {


	public static async Task<string?> EnviarLikeSharedfiles(Bot bot, EAccess access, string id) {

		if (access < EAccess.Master) {
			//return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
			return null;
		}

		bool? botUtilizadoAnteriormente = await DbHelper.VerificarEnvioItem(bot.BotName, "SHAREDLIKE", id).ConfigureAwait(false);

		if (botUtilizadoAnteriormente == true) {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {id}");
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		if (bot.IsAccountLimited) {
			return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);
		}

		//string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE => {id} (Enviando)");

		Uri request = new(SteamCommunityURL, "/sharedfiles/voteup");
		Uri requestViewPage = new(SteamCommunityURL, $"/sharedfiles/filedetails/?id={id}");

		Dictionary<string, string> data = new(1)
		{
		{ "id", id }
		};

		bool? verificaSharedFav = await DbHelper.VerificarEnvioItem(bot.BotName, "SHAREDFAV", id).ConfigureAwait(false);
		// verificamos se esse bot já enviou favoritos antes, pois então não precisamos 'visitar' a página novamente.
		if (verificaSharedFav == false) {
			HtmlDocumentResponse? response = await VisualizarPagina(bot, requestViewPage).ConfigureAwait(false);
			if (response == null) {
				bot.ArchiLogger.LogGenericError("Erro ao executar POST");
			}
		}

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "SHAREDLIKE", id).ConfigureAwait(false);

		if (!salvaItem.HasValue) {
			return null;
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE => {id} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {id} — Like" : Strings.WarningFailed);

	}

	internal static async Task<HtmlDocumentResponse?> VisualizarPagina(Bot bot, Uri requestViewPage) {
		HtmlDocumentResponse? response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(requestViewPage, referer: SteamCommunityURL).ConfigureAwait(false);
		return response;
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

		await DSKLogger.CompartilharAtividade($"Sharedfiles-LIKE-{argument}").ConfigureAwait(false);
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

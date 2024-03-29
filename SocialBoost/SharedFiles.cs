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
internal static class SharedFiles {


	public static async Task<string?> EnviarSharedfiles(Bot bot, EAccess access, string id, string appID) {

		int validoLikes = 1;

		if (access < EAccess.Master) {
			//return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
			return null;
		}

		bool? botUtilizadoAnteriormente1 = await DbHelper.VerificarEnvioItem(bot.BotName, "SHAREDLIKE", id).ConfigureAwait(false);
		bool? botUtilizadoAnteriormente2 = await DbHelper.VerificarEnvioItem(bot.BotName, "SHAREDFAV", id).ConfigureAwait(false);

		if (botUtilizadoAnteriormente1 == true && botUtilizadoAnteriormente2 == true) {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {id}");
		}

		if (botUtilizadoAnteriormente1 == false && botUtilizadoAnteriormente2 == true && bot.IsAccountLimited) {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {id}");
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		if (bot.IsAccountLimited) {
			validoLikes = 0;
		}

		string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE|FAV => {id} (Enviando)");

		if (string.IsNullOrEmpty(sessionId)) {
			return Commands.FormatBotResponse(Strings.BotLoggedOff, bot.BotName);
		}

		Uri request = new(SteamCommunityURL, "/sharedfiles/voteup");
		Uri request2 = new(SteamCommunityURL, "/sharedfiles/favorite");
		Uri requestViewPage = new(SteamCommunityURL, $"/sharedfiles/filedetails/?id={id}");


		Dictionary<string, string> data1 = new(1)
{
		{ "id", id }};

		Dictionary<string, string> data2 = new(2)
		{
		{ "id", id },
		{ "appid", appID }
		};

		if (botUtilizadoAnteriormente1.HasValue && !botUtilizadoAnteriormente1.Value &&
			botUtilizadoAnteriormente2.HasValue && !botUtilizadoAnteriormente2.Value) {
			// verificamos se esse bot já enviou likes ou favoritos antes, pois então não precisamos 'visitar' a página novamente.
			HtmlDocumentResponse? response = await VisualizarPagina(bot, requestViewPage).ConfigureAwait(false);
			if (response == null) {
				bot.ArchiLogger.LogGenericError("Erro ao executar POST");
			}
		}

		bool postLike = false;
		bool postFav = false;

		if (validoLikes == 1 && botUtilizadoAnteriormente1 == false) {
			postLike = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data1, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

			if (postLike) {

				bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "SHAREDLIKE", id).ConfigureAwait(false);
				if (!salvaItem.HasValue) {
					return null;
				}

			} else if (!postLike) {
				bot.ArchiLogger.LogGenericError("Erro ao executar POST");
			}
		}

		if (botUtilizadoAnteriormente2 == false) {
			postFav = await bot.ArchiWebHandler.UrlPostWithSession(request2, data: data2, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

			if (postFav) {

				bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "SHAREDFAV", id).ConfigureAwait(false);
				if (!salvaItem.HasValue) {
					return null;
				}

			} else if (!postFav) {
				bot.ArchiLogger.LogGenericError("Erro ao executar POST");
			}

		}

		bool postSuccess = postLike || postFav;

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|LIKE|FAV => {id} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {id} — {(postLike && postFav ? "Like+Favorito" : postLike ? "Like" : postFav ? "Favorito" : "")}" : Strings.WarningFailed);

	}

	internal static async Task<HtmlDocumentResponse?> VisualizarPagina(Bot bot, Uri requestViewPage) {
		HtmlDocumentResponse? response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(requestViewPage, referer: SteamCommunityURL).ConfigureAwait(false);
		return response;
	}

	public static async Task<string?> EnviarSharedfiles(EAccess access, ulong steamID, string botNames, string argument) {

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

		await DSKLogger.CompartilharAtividade($"Sharedfiles-LIKE|FAV-{argument}").ConfigureAwait(false);
		IList<string?> results = await Utilities.InParallel(bots.Select(bot => EnviarSharedfiles(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument2))).ConfigureAwait(false);
		List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);


	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

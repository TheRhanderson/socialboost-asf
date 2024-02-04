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
internal static class CSharedFav {

	public static int NumEnviosDesejados;

	public static async Task<string?> EnviarFavSharedfiles(Bot bot, EAccess access, string id, string appID) {

		if (access < EAccess.Master) {
			//return bot.Commands.FormatBotResponse(Strings.ErrorAccessDenied);
			return null;
		}

		bool? botUtilizadoAnteriormente = await DbHelper.VerificarEnvioItem(bot.BotName, "SHAREDFAV", id).ConfigureAwait(false);

		if (botUtilizadoAnteriormente == true) {
			return bot.Commands.FormatBotResponse($"{Strings.WarningFailed} — ID: {id}");
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
		}

		//if (bot.IsAccountLimited) {
		//	return bot.Commands.FormatBotResponse(Strings.BotAccountLimited);   /// Isso funciona com contas limitadas.
		//}

		//string? sessionId = await FetchSessionID(bot).ConfigureAwait(false);
		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|FAV => {id} (Enviando)");

		Uri request = new(SteamCommunityURL, "/sharedfiles/favorite");
		Uri requestViewPage = new(SteamCommunityURL, $"/sharedfiles/filedetails/?id={id}");

		Dictionary<string, string> data = new(2)
		{
		{ "id", id },
		{ "appid", appID }
		};

		bool? verificaSharedLike = await DbHelper.VerificarEnvioItem(bot.BotName, "SharedLike", id).ConfigureAwait(false);
		// verificamos se esse bot já enviou likes antes, pois então não precisamos 'visitar' a página novamente.
		if (verificaSharedLike == false) {
			HtmlDocumentResponse? response = await VisualizarPagina(bot, requestViewPage).ConfigureAwait(false);
			if (response == null) {
				bot.ArchiLogger.LogGenericError("Erro ao executar POST");
			}
		}

		bool postSuccess = await bot.ArchiWebHandler.UrlPostWithSession(request, data: data, session: ESession.Lowercase, referer: requestViewPage).ConfigureAwait(false);

		if (!postSuccess) {
			bot.ArchiLogger.LogGenericError("Erro ao executar POST");
		}

		bool? salvaItem = await DbHelper.AdicionarEnvioItem(bot.BotName, "SHAREDFAV", id).ConfigureAwait(false);

		if (!salvaItem.HasValue) {
			return null;
		}

		bot.ArchiLogger.LogGenericInfo($"SocialBoost|SHAREDFILES|FAV => {id} (OK)");
		return bot.Commands.FormatBotResponse(postSuccess ? $"{Strings.Success.Trim()} — ID: {id} — Favorito" : Strings.WarningFailed);

	}

	internal static async Task<HtmlDocumentResponse?> VisualizarPagina(Bot bot, Uri requestViewPage) {
		HtmlDocumentResponse? response = await bot.ArchiWebHandler.UrlGetToHtmlDocumentWithSession(requestViewPage, referer: SteamCommunityURL).ConfigureAwait(false);
		return response;
	}

	public static async Task<string?> EnviarFavSharedfiles(EAccess access, ulong steamID, string argument, string envioDesejado) {

		if (string.IsNullOrEmpty(argument) || string.IsNullOrEmpty(envioDesejado)) {
			ASF.ArchiLogger.LogNullError(null, nameof(argument) + " || " + nameof(envioDesejado));
			return null;
		}

		if (int.TryParse(envioDesejado, out int numEnviosDesejados)) {
			NumEnviosDesejados = numEnviosDesejados;
		} else {
			return null;
		}

		HashSet<Bot>? bots = Bot.GetBots("ASF");

		if ((bots == null) || (bots.Count == 0)) {
			return access >= EAccess.Owner ? FormatBotResponse(Strings.BotNotFound, "SocialBoost") : null;
		}

		int contador = 0;

		foreach (Bot bot in bots) {
			if (bot.IsConnectedAndLoggedOn) {
				contador++;

				if (contador == numEnviosDesejados) {
					break;
				}
			}
		}

		if (contador != numEnviosDesejados) {
			return access >= EAccess.Owner ? FormatBotResponse("Não há bots online suficientes!", "SocialBoost") : null;
		}

		string? argument2 = await SessionHelper.FetchAppIDShared($"https://steamcommunity.com/sharedfiles/filedetails/?id={argument}").ConfigureAwait(false);

		Bot firstBot = bots.First();

		if (string.IsNullOrEmpty(argument2)) {
			return Commands.FormatBotResponse("Erro ao determinar appid do sharedfiles", firstBot.BotName);
		}

		bool? logger = await DSKLogger.CompartilharAtividade($"CSharedfiles-FAV-{argument}").ConfigureAwait(false);

		List<string?> responses = []; // Lista para armazenar respostas bem-sucedidas
		int enviosBemSucedidos = 0; // Contador de envios bem-sucedidos

		foreach (Bot bot in bots) {
			if (enviosBemSucedidos >= NumEnviosDesejados) {
				break; // Saia do loop se o número desejado já foi atingido
			}

			string? response = await EnviarFavSharedfiles(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument2).ConfigureAwait(false);

			if (!string.IsNullOrEmpty(response)) {
				responses.Add(response);
				enviosBemSucedidos++;
			}
		}

		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? $"{string.Join(Environment.NewLine, responses)}{Environment.NewLine}Enviados com sucesso: {enviosBemSucedidos}" : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) => await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);
	private static string FormatBotResponse(string message, string botName) => $"<{botName}> {message}";

}

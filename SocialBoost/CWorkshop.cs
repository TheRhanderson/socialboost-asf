using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Localization;
using SocialBoost.Helpers;
using static ArchiSteamFarm.Steam.Integration.ArchiWebHandler;

namespace SocialBoost;
internal static class CWorkshop {

	public static int NumEnviosDesejados;

	public static async Task<string?> SeguirOficinaPerfilSteam(Bot bot, EAccess access, string urlPerfil, string steamAlvo, string action) {

		if (access < EAccess.Master) {
			return null;
		}

		bool? botUtilizadoAnteriormente = await DbHelper.VerificarEnvioItem(bot.BotName, "WORKSHOP", steamAlvo).ConfigureAwait(false);

		if (botUtilizadoAnteriormente == true && action != "2") {
			return null;
		}

		if (botUtilizadoAnteriormente == false && action == "2") {
			return null;
		}

		if (!bot.IsConnectedAndLoggedOn) {
			return null;
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

	public static async Task<string?> SeguirOficinaID64(EAccess access, ulong steamID, string argument, string argument2, string envioDesejado) {

		if (string.IsNullOrEmpty(argument) || string.IsNullOrEmpty(argument2) || string.IsNullOrEmpty(envioDesejado)) {
			ASF.ArchiLogger.LogNullError(null, nameof(argument) + " || " + nameof(argument2) + " || " + nameof(envioDesejado));
			return null;
		}

		if (argument2 is not "1" and not "2") {
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

		string? argument3 = await SessionHelper.FetchSteamID64($"{argument}").ConfigureAwait(false);

		if (argument3 == null) {
			return null;
		}

		bool? logger = await DSKLogger.CompartilharAtividade($"CWorkshop-{(argument2 == "1" ? "FOLLOW" : "UNFOLLOW")}-{argument3}").ConfigureAwait(false);
		List<string?> responses = []; // Lista para armazenar respostas bem-sucedidas
		int enviosBemSucedidos = 0; // Contador de envios bem-sucedidos

		foreach (Bot bot in bots) {
			if (enviosBemSucedidos >= NumEnviosDesejados) {
				break; // Saia do loop se o número desejado já foi atingido
			}

			string? response = await SeguirOficinaPerfilSteam(bot, Commands.GetProxyAccess(bot, access, steamID), argument, argument3, argument2).ConfigureAwait(false);

			if (!string.IsNullOrEmpty(response)) {
				responses.Add(response);
				enviosBemSucedidos++;
			}
		}

		ASF.ArchiLogger.LogGenericInfo($"Envio concluído! Criado por @therhanderson");
		return responses.Count > 0 ? $"{string.Join(Environment.NewLine, responses)}{Environment.NewLine}Enviados com sucesso: {enviosBemSucedidos}" : null;
	}

	public static async Task<string?> FetchSessionID(Bot bot) =>
	await SessionHelper.FetchSessionID(bot).ConfigureAwait(false);

	private static string FormatBotResponse(string message, string botName) =>
		$"<{botName}> {message}";

}

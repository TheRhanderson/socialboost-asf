using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using Newtonsoft.Json;
using static SocialBoost.Helpers.DbHelper;
using System.IO;
using ArchiSteamFarm.Localization;
using SocialBoost.Helpers;

namespace SocialBoost;
internal static class CheckBoost {

	public static async Task<string?> VerificarEnvioItemDisp(EAccess access, ulong steamID, string tipoVerificacao, string idBusca) {

		if (string.IsNullOrEmpty(tipoVerificacao) || string.IsNullOrEmpty(idBusca)) {
			ASF.ArchiLogger.LogNullError(null, nameof(tipoVerificacao) + " || " + nameof(idBusca));
			return null;
		}

		if (tipoVerificacao.ToUpperInvariant() is not "REVIEWS" and not "SHAREDLIKE" and not "SHAREDFAV" and not "WORKSHOP") {
			return null;
		}

		if (tipoVerificacao.Equals("REVIEWS", System.StringComparison.OrdinalIgnoreCase)) {
			string? newIdBusca = await SessionHelper.FetchReviewID(idBusca).ConfigureAwait(false);
			if (newIdBusca == null) {
				return null;
			} else {
				idBusca = newIdBusca;
			}
		}

		if (tipoVerificacao.Equals("WORKSHOP", System.StringComparison.OrdinalIgnoreCase)) {
			string? newIdBusca = await SessionHelper.FetchSteamID64(idBusca).ConfigureAwait(false);
			if (newIdBusca == null) {
				return null;
			} else {
				idBusca = newIdBusca;
			}
		}

		int botsSemEnvio = 0;

		ASF.ArchiLogger.LogGenericInfo(tipoVerificacao + " - " + idBusca);

		string filePath = "plugins/socialboost-db.json";
		string jsonContent = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		Dictionary<string, BotData> data = JsonConvert.DeserializeObject<Dictionary<string, BotData>>(jsonContent)
										  ?? [];
		foreach (KeyValuePair<string, BotData> botEntry in data) {
			string botName = botEntry.Key;
			BotData botData = botEntry.Value;

			List<string> reviewList = GetReviewList(tipoVerificacao, botData);

			if (!reviewList.Contains(idBusca)) {
				botsSemEnvio++;
			}
		}

		string mensagemResposta = $"{Strings.Success} — Verificação — {tipoVerificacao} — Disponíveis para envio: {botsSemEnvio}";
		return botsSemEnvio > 0 ? mensagemResposta : $"{Strings.WarningFailed} — Verificação — {tipoVerificacao} — Todos os bots já enviaram para {idBusca}!";
	}
}

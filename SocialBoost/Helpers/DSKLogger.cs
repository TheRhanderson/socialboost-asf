using System;
using System.Threading.Tasks;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;
using ArchiSteamFarm.Core;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SocialBoost.Helpers;
internal static class DSKLogger {

	internal static async Task CompartilharAtividade(string activityLog) {
		string computerName = Environment.MachineName;
		Uri uri2 = new($"https://rhanderson.com.br/argfarm/auth/logger.php?user={computerName}&log={activityLog}");
		HtmlDocumentResponse? response = await ASF.WebBrowser!.UrlGetToHtmlDocument(uri2, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);

		if (response == null) {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
		}
		// Não há retorno aqui, a função apenas executa a requisição
	}
}
internal static class CatAPI {

	internal static async Task<bool?> AuthOPlugin(CancellationToken cancellationToken = default) {

		List<KeyValuePair<string, string>> headers =
		[
		new("X-HWID", "7aceb026713e9d61b82be892b6095475ac4851fe"),
		new("X-USER", "freelicense"),
		new("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.1234.567 Safari/537.36 DsK-SocialBoost/1.4"),
		new("X-MCME", Environment.MachineName)
	];

		string url = "https://dskillers.ovh/argfarm/auth/genasfsession.php";
		Uri request = new(url);

		bool? registrarContas = await DbHelper.RegistrarBancoBots("ASF").ConfigureAwait(false);
		ObjectResponse<AuthResponse>? response = await ASF.WebBrowser!.UrlGetToJsonObject<AuthResponse>(request, headers, cancellationToken: cancellationToken).ConfigureAwait(false);
		return (response?.Content?.Authentication == true) && (registrarContas == true);
	}
}

public class AuthResponse {
	[JsonPropertyName("authentication")]
	public bool Authentication { get; set; }
}

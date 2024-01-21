using System;
using System.Threading.Tasks;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Web.Responses;
using ArchiSteamFarm.Core;
using System.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SocialBoost;
internal static class DSKLogger {

	internal static async Task<bool?> CompartilharAtividade(string activityLog) {
		string computerName = Environment.MachineName;
		Uri uri2 = new($"https://rhanderson.com.br/argfarm/auth/logger.php?user={computerName}&log={activityLog}");
		HtmlDocumentResponse? response = await ASF.WebBrowser!.UrlGetToHtmlDocument(uri2, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);

		if (response == null) {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return false;
		}
		return true;
	}
}
internal static class CatAPI {

	internal static async Task<bool?> AuthOPlugin(CancellationToken cancellationToken = default) {
		//ArgumentNullException.ThrowIfNull(webBrowser);

		List<KeyValuePair<string, string>> headers = [
			new("X-HWID", "40184a3c5a62d37712a747cc6e8afe33156e2b17"),
			new("X-USER", "gardemi14"),
			new("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.1234.567 Safari/537.36 DsK-SocialBoost/1.4"),
			new("X-MCME", Environment.MachineName)
	];

		string url = "https://dskillers.ovh/argfarm/auth/genasfsession.php";
		Uri request = new(url);

		ObjectResponse<AuthResponse>? response = await ASF.WebBrowser!.UrlGetToJsonObject<AuthResponse>(request, headers, cancellationToken: cancellationToken).ConfigureAwait(false);
		return response?.Content?.Authentication;
	}
}



#pragma warning disable CA1812 // False positive, the class is used during json deserialization
internal sealed class AuthResponse {
	[JsonProperty("authentication", Required = Required.Always)]
	internal readonly bool Authentication;

	// Adicione um construtor público para ser usado explicitamente no código, se necessário
	public AuthResponse(bool authentication) => Authentication = authentication;

	[JsonConstructor]
	private AuthResponse() { }
}

#pragma warning restore CA1812 // False positive, the class is used during json deserialization

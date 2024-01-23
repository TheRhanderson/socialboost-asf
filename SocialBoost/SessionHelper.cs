using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Web.Responses;
using System.Text.RegularExpressions;

namespace SocialBoost;
internal static class SessionHelper {
	public static async Task<string?> FetchSessionID(Bot bot) {
		CookieCollection cc = bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookies(ArchiWebHandler.SteamStoreURL);
		Cookie? sessionIdCookie = cc.Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("sessionid", StringComparison.OrdinalIgnoreCase));

		return sessionIdCookie != null
			? await Task.FromResult<string?>(sessionIdCookie.Value).ConfigureAwait(false)
			: await Task.FromResult<string?>("").ConfigureAwait(false);
	}

	internal static async Task<string?> FetchReviewID(string urlReview) {

		Uri uri2 = new(urlReview);
		HtmlDocumentResponse? response = await ASF.WebBrowser!.UrlGetToHtmlDocument(uri2, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);

		if (response == null || response.Content?.Body == null) {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}

		string strd = response.Content.Body.InnerHtml;
		//JsonObject htmlString = (JsonObject) response.Content.Body.OuterHtml;  // para adições futuras


#pragma warning disable SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.
		Match match = Regex.Match(strd, @"UserReview_Report\(\s*'(\d+)',\s*'https://steamcommunity.com',\s*function\( results \)");
#pragma warning restore SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.

		if (match.Success) {
			// O valor está na primeira captura da correspondência
			string valorExtraido = match.Groups[1].Value;
			ASF.ArchiLogger.LogGenericInfo("Valor extraído: " + valorExtraido);
			return valorExtraido;
		} else {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}
	}



	internal static async Task<string?> FetchAppIDShared(string urlReview) {
		Uri uri2 = new(urlReview);
		HtmlDocumentResponse? response = await ASF.WebBrowser!.UrlGetToHtmlDocument(uri2, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);

		if (response == null || response.Content?.Body == null) {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}

		string strd = response.Content.Body.InnerHtml;

#pragma warning disable SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.
		Match match = Regex.Match(strd, @"RecordAppImpression\(\s*(\d+)\s*,\s*'[^']*'\s*\);");
#pragma warning restore SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.

		if (match.Success) {
			// O valor está na primeira captura da correspondência
			string valorExtraido = match.Groups[1].Value;
			ASF.ArchiLogger.LogGenericInfo("Valor extraído: " + valorExtraido);
			return valorExtraido;
		} else {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}
	}



	internal static async Task<string?> FetchSteamID64(string urlReview) {
		Uri uri2 = new(urlReview);
		HtmlDocumentResponse? response = await ASF.WebBrowser!.UrlGetToHtmlDocument(uri2, referer: ArchiWebHandler.SteamCommunityURL).ConfigureAwait(false);

		if (response == null || response.Content?.Body == null) {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}

		string strd = response.Content.Body.InnerHtml;

#pragma warning disable SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.
		Match match = Regex.Match(strd, @"""steamid"":""(\d+)""");
#pragma warning restore SYSLIB1045 // Converter em 'GeneratedRegexAttribute'.

		if (match.Success) {
			// O valor está na primeira captura da correspondência
			string valorExtraido = match.Groups[1].Value;
			ASF.ArchiLogger.LogGenericInfo("Valor extraído: " + valorExtraido);
			return valorExtraido;
		} else {
			ASF.ArchiLogger.LogGenericError("A requisição não retornou uma resposta válida.");
			return string.Empty;
		}
	}


}

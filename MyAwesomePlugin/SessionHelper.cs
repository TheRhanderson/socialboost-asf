using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam;

namespace SocialBoost;
internal static class SessionHelper {
	public static async Task<string?> FetchSessionID(Bot bot) {
		CookieCollection cc = bot.ArchiWebHandler.WebBrowser.CookieContainer.GetCookies(ArchiWebHandler.SteamStoreURL);
		Cookie? sessionIdCookie = cc.Cast<Cookie>().FirstOrDefault(c => c.Name.Equals("sessionid", StringComparison.OrdinalIgnoreCase));

		return sessionIdCookie != null
			? await Task.FromResult<string?>(sessionIdCookie.Value).ConfigureAwait(false)
			: await Task.FromResult<string?>("").ConfigureAwait(false);
	}
}

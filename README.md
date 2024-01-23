---

# SocialBoost - Boosting Interactions on ArchiSteamFarm

SocialBoost is a complementary plugin for ArchiSteamFarm, designed to enhance interactions on Steam platform. This plugin provides features to boost the number of likes and favorites on images, guides, and various content types. It also enables user game reviews (Useful/Funny) and allows following players' Workshop, with more features to be added soon.

## Features

### Sharedfiles
For sharedfiles media, the following commands are available:

- **SHAREDLIKE [Bots] [Id]:** Sends likes to a specific sharedfiles URL.
- **SHAREDFAV [Bots] [Id]:** Sends favorites to a specific sharedfiles URL.
- **SHAREDFILES [Bots] [Id]:** Sends likes and favorites to a specific sharedfiles URL.

Example usage: `SHAREDFILES ASF 3142209500` (The Id 3142209500 refers to the end of the URL).

### Game Reviews
For game reviews, the available command is:

- **RATEREVIEW [Bots] [Review Url] [Type]:** Sends a recommendation (Useful or Funny) for a game review. Type 1 is for Useful, and type 2 is for Funny.

Example usage: `RATEREVIEW ASF https://steamcommunity.com/id/xxxxxxxxx/recommended/739630 1` (The URL refers to the game review, and 1 indicates a Useful recommendation).

### Steam Workshop
To follow a Steam profile's Workshop, use the command:

- **WORKSHOP [Bots] [Profile Url]:** Starts following the Workshop of this Steam profile. Limited accounts are compatible.

Example usage: `WORKSHOP ASF https://steamcommunity.com/id/xxxxxxxxxxxxxx` (The URL should be the same as used to visit the profile in the browser).

---

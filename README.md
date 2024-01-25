---
# SocialBoost - Boosting Interactions on ArchiSteamFarm
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/TheRhanderson/socialboost-asf/total)

SocialBoost is a complementary plugin for ArchiSteamFarm, designed to enhance interactions on the Steam platform. This plugin provides features to boost the number of likes and favorites on images, guides, and various content types. It also enables user game reviews (Useful/Funny) and allows following players' Workshop, with more features to be added soon.

## Features

### Sharedfiles
For sharedfiles media, the following commands are available:

- **SHAREDLIKE [Bots] [Id]:** Sends likes to a specific sharedfiles URL.
- **SHAREDFAV [Bots] [Id]:** Sends favorites to a specific sharedfiles URL.
- **SHAREDFILES [Bots] [Id]:** Sends likes and favorites to a specific sharedfiles URL.

Example usage: `SHAREDFILES ASF 3142209500` (The Id 3142209500 refers to the end of the URL).

### Game Reviews
For game reviews, the available command is:

- **RATEREVIEW [Bots] [Review Url] [Type]:** Sends a recommendation for a game review.
  - Type 1 (Useful)
  - Type 2 (Funny)
  - Type 3 (Not Useful)

Example usage: `RATEREVIEW ASF https://steamcommunity.com/id/xxxxxxxxx/recommended/739630 1` (The URL refers to the game review, and 1 indicates a Useful recommendation).

### Steam Workshop
To follow a Steam profile's Workshop, use the command:

- **WORKSHOP [Bots] [Profile Url] [Type]:** Starts following/unfollowing the Workshop of a specific Steam profile. Limited accounts are compatible.
  - Type 1 (Following)
  - Type 2 (Unfollowing)

Example usage: `WORKSHOP ASF https://steamcommunity.com/id/xxxxxxxxxxxxxx 1` (The URL should be the same as used to visit the profile in the browser. This will follow the Steam profile.).

## How to Install
* Visit the [releases](https://github.com/TheRhanderson/socialboost-asf/releases) page, download the latest available version, extract it into the ``/plugins`` folder of your ASF, and restart the process. Have fun!

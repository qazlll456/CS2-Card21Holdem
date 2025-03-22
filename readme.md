# CS2-Card21Holdem


A Counter-Strike 2 (CS2) plugin built with CounterStrikeSharp that adds two exciting card games—**Blackjack (Card 21)** and **Texas Hold'em**—along with a simple dice-rolling feature. Enhance your CS2 server with interactive gameplay for players during downtime!

## Overview
This plugin brings the thrill of card games to your Counter-Strike 2 server using the CounterStrikeSharp framework. Players can enjoy **Blackjack (Card 21)** against a dealer, compete in **Texas Hold'em** with friends and a bot, or roll a random number for fun. It supports a variety of chat commands like `!card21`, `!holdem`, and `!roll`, along with admin commands for server management.

- **Module Name**: CS2-Card21Holdem
- **Version**: 1.0.0
- **Author**: qazlll456 from HK with xAI assistance
- **Description**: A plugin for Card 21 and Hold'em games in CS2

## Donors

This section is to express my heartfelt thanks to the individuals who donated to support the development of this project. Your contributions help keep this project alive and motivate me to continue improving it!

A special shoutout to **TRLG nick** from the UK for sponsoring this project! I really appreciate your generosity.

## Support
If you enjoy this plugin, consider supporting my work!  
Money, Steam games or any value is also welcome
[Donate - streamlabs, paypal](https://streamlabs.com/BKCqazlll456/tip)

## Features

- **Card 21 (Blackjack)**: Players aim to reach a score of 21 without busting, competing against a dealer.
- **Texas Hold'em**: A multiplayer poker game with community cards, supporting up to a configurable number of players (including a bot).
- **Roll**: A fun command to roll a random number between 1 and 100.
- **Configurable**: Customize gameplay settings, permissions, and messaging via a JSON config file.
- **Chat Integration**: Uses in-game chat for commands and colorful card displays.

## Demo

Youtube video - [Youtube - CS2 plugin - CS2-Card21Holdem demonstrate video](https://www.youtube.com/watch?v=xUzLW0t8Z9U&ab_channel=qazlll456)

## Prerequisites

- **Counter-Strike 2 Server**: A running CS2 server.
- **Metamod:Source**: Required for CounterStrikeSharp. Install it from the [Metamod:Source website](https://www.sourcemm.net/).
- **CounterStrikeSharp**: The plugin framework for CS2. Install it from the [CounterStrikeSharp GitHub](https://github.com/roflmuffin/CounterStrikeSharp).
- **Dependencies**: Ensure you have .NET installed (compatible with CounterStrikeSharp) for building the plugin.

## Installation

1. **Download the Plugin**:
   - Clone this repository or download the ZIP file:
     ```bash
     git clone https://github.com/qazlll456/CS2-Card21Holdem.git
     ```
   - Alternatively, download the latest release from the [Releases](https://github.com/qazlll456/CS2-Card21Holdem/releases) page.

2. **Build the Project**:
   - Open the solution (`CS2-Card21Holdem.sln`) in Visual Studio or your preferred C# IDE.
   - Ensure CounterStrikeSharp is referenced (e.g., via NuGet or local DLLs).
   - Build the project to generate `CS2_Card21Holdem.dll`.

3. **Deploy to Server**:
   - Copy the compiled `CS2_Card21Holdem.dll` to your CS2 server's `csgo/addons/counterstrikesharp/plugins/` directory.
   - On first run, the plugin will generate a `config.json` file in `csgo/addons/counterstrikesharp/configs/plugins/CS2-Card21Holdem/`.

4. **Start the Server**:
   - Launch your CS2 server. The plugin will load automatically.

## Usage

### Commands

#### General Commands
| Command       | Description                              |
|---------------|------------------------------------------|
| `!ch21`       | Displays plugin info and command overview. |
| `!info-card`  | Shows a detailed command list.           |
| `!roll`       | Rolls a random number between 1 and 100. |

#### Card 21 (Blackjack)
| Command          | Description                              |
|------------------|------------------------------------------|
| `!card21 start`  | Starts a new Blackjack game.             |
| `!card21 hit`    | Draws another card.                      |
| `!card21 stand`  | Ends your turn, letting the dealer play. |
| `!card21 help`   | Lists Card 21 commands.                  |

#### Texas Hold'em
| Command             | Description                              |
|---------------------|------------------------------------------|
| `!holdem host <number>` | Hosts a game for `<number>` players (e.g., `!holdem host 3`). |
| `!holdem join`      | Joins a hosted game.                     |
| `!holdem start`     | Starts the game (host only).             |
| `!holdem yes`       | Continues to the next round, drawing a card. |
| `!holdem fold`      | Folds your hand and exits the game.      |
| `!holdem off`       | Cancels the hosted game (host only).     |
| `!holdem result`    | Shows poker hand rankings.               |
| `!holdem help`      | Lists Hold'em commands.                  |

#### Admin Commands
| Command            | Description                              |
|--------------------|------------------------------------------|
| `!ch21 enable`     | Enables the plugin (admin only).         |
| `!ch21 disable`    | Disables the plugin (admin only).        |
| `!ch21 reload`     | Reloads the config file (admin only).    |
| `css_ch21_reload`  | Server console: Reloads the config.      |
| `css_ch21_enable`  | Server console: Enables the plugin.      |
| `css_ch21_disable` | Server console: Disables the plugin.     |

### Gameplay

- **Card 21 (Blackjack)**: Players draw cards to get as close to 21 as possible without going over. The dealer draws until 17 or higher. Aces count as 1 or 11 dynamically.
- **Texas Hold'em**: Players start with one card, drawing more each round (up to `MaxRounds`). Community cards are shared, and the best 5-card hand wins. A bot joins as an AI opponent.

## Configuration

The plugin generates a `config.json` file in `csgo/addons/counterstrikesharp/configs/plugins/CS2-Card21Holdem/` on first run. Customize it to suit your server:

```json
{
  "Permissions": {
    "PermissionGroup": "all",
    "Whitelist": [100000000000, 100000000001],
    "Blacklist": [],
    "Admins": [100000000000]
  },
  "Gameplay": {
    "MaxPlayers": 4,
    "MaxRounds": 4,
    "CommunityCardNumber": 5,
    "CommandPrefix": "!holdem"
  },
  "Bots": {
    "BotNames": ["Bot A", "Bot B", "Bot C"],
    "UseRandomBotName": true
  },
  "Messaging": {
    "PublicMessages": false,
    "PublicResults": false,
    "IsDebugLogging": true
  },
  "Features": {
    "EnableHoldem": true,
    "EnableCard21": true,
    "EnableRoll": true
  }
}
```

## Support the Project
If you find this plugin helpful and want to support its development,

consider donating via my Streamlabs tipping page: [Donate here](https://streamlabs.com/BKCqazlll456/tip). 

Your support is greatly appreciated!

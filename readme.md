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

A special shoutout to **TRLG nick from the UK** for sponsoring this project! I truly appreciate your generosity.

## Support
If you find this plugin helpful and want to support its development, 

Money, Steam games, or any valuable contribution is welcome.

[ko_fi](https://ko-fi.com/qazlll456).

[patreon](https://www.patreon.com/c/qazlll456).

[Streamlabs](https://streamlabs.com/BKCqazlll456/tip)

Your support is greatly appreciated!

## Game Rules

### Card 21 (Blackjack) Rules
**Objective**: The goal is to achieve a hand score as close to 21 as possible without going over, while beating the dealer’s score.

- **Gameplay**:  
  - You start with 2 cards, and the dealer gets 2 cards (one is hidden).  
  - Use `!card21 hit` to draw another card or `!card21 stand` to end your turn.  
  - Card values: Aces are worth 11 or 1 (automatically adjusted to avoid busting), face cards (K, Q, J) are 10, and numbered cards are their face value (e.g., 2 is 2, 10 is 10).  
  - If your score exceeds 21, you bust and lose immediately.  
  - After you stand, the dealer reveals their hidden card and must draw until their score is 17 or higher.  

- **Win Conditions**:  
  - You win if your score is higher than the dealer’s without busting, or if the dealer busts (score > 21).  
  - You lose if you bust or if the dealer’s score is higher without busting.  
  - If your score equals the dealer’s, it’s a "Push" (tie), and no one wins.  

### Texas Hold'em Rules
**Objective**: The goal is to form the best 5-card poker hand using your personal cards and the community cards, outranking other players’ hands to win the game.

- **Gameplay**:  
  - A game supports up to 4 players (including a bot). Use `!holdem host <number>` to start a game, `!holdem join` to join, and `!holdem start` to begin.  
  - There are 5 community cards dealt at the start, shared by all players.  
  - Each player receives 1 card per round (up to 4 rounds). After each round, use `!holdem yes` to continue and draw another card, or `!holdem fold` to exit the game.  
  - If a player doesn’t respond within 30 seconds, they are automatically folded.  
  - The game ends after 4 rounds or when only one player remains (others have folded).  

- **Win Conditions**:  
  - The player with the best 5-card hand wins, determined by combining their personal cards with the community cards.  
  - Hand rankings (from highest to lowest):  
    - **Royal Flush**: A, K, Q, J, 10, all of the same suit (e.g., A♥ K♥ Q♥ J♥ 10♥).  
    - **Straight Flush**: Five consecutive cards of the same suit (e.g., 9♠ 8♠ 7♠ 6♠ 5♠).  
    - **Four of a Kind**: Four cards of the same rank (e.g., 7♥ 7♠ 7♣ 7♦ 2♠).  
    - **Full House**: Three cards of one rank and two cards of another (e.g., 8♣ 8♦ 8♠ 3♥ 3♣).  
    - **Flush**: Five cards of the same suit, not in sequence (e.g., K♠ J♠ 9♠ 6♠ 2♠).  
    - **Straight**: Five consecutive cards of different suits (e.g., 6♠ 5♥ 4♣ 3♦ 2♠).  
    - **Three of a Kind**: Three cards of the same rank (e.g., Q♥ Q♠ Q♣ 5♦ 2♠).  
    - **Two Pair**: Two pairs of different ranks (e.g., J♠ J♣ 4♥ 4♦ 9♠).  
    - **Pair**: Two cards of the same rank (e.g., 10♣ 10♦ 8♠ 5♥ 3♣).  
    - **High Card**: When no other hand is made, the highest card wins (e.g., A♠ 9♣ 7♦ 4♠ 2♥, Ace is the high card).  
  - **Tiebreakers**: If players have the same hand type, the highest card values win (e.g., for Two Pair, the higher pair wins; for High Card, the highest card wins). If still tied, additional cards (kickers) are compared.  
  - **Winning the Game**: You win if you have the best hand after 4 rounds or if all other players fold, leaving you as the last player standing.


## Features
- **Card 21 (Blackjack)**: Play a classic Blackjack game against a dealer, aiming for a score of 21 without going over.
- **Texas Hold'em**: Host or join a poker game with up to 4 players (including a bot), featuring community cards and multiple rounds.
- **Roll Command**: Roll a random number (1-100) for a fun side activity.
- **Bot Opponent**: A bot joins Texas Hold'em games, making smart decisions based on hand strength.
- **Permission System**: Configure access with modes (`all`, `whitelist`, `blacklist`) for players, plus admin-only commands.
- **Customizable Settings**: Adjust game rules, bot names, message visibility, and more via a JSON config file.
- **Colored Chat Messages**: Visual feedback with colored cards (red for hearts/diamonds, green for spades/clubs).

## Demo Video
Want to see the plugin in action? Check out this demo video showcasing the gameplay of **Blackjack (Card 21)** and **Texas Hold'em** on a CS2 server:  
[Watch the Demo Video on YouTube](https://www.youtube.com/watch?v=xUzLW0t8Z9U&ab_channel=qazlll456)

## Installation

### Prerequisites
To use this plugin, you need:  
- **Counter-Strike 2 Dedicated Server**: A running CS2 server.  
- **Metamod:Source**: Installed on your server for plugin support. Download from [sourcemm.net](https://www.sourcemm.net/).  
- **CounterStrikeSharp**: The C# plugin framework for CS2. Download the latest version from [GitHub releases](https://github.com/roflmuffin/CounterStrikeSharp/releases) (choose the "with runtime" version if it’s your first install).

### Steps
1. Download the latest release from [Releases](https://github.com/qazlll456/CS2-Card21Holdem/releases) or clone the repository:  
   > git clone https://github.com/qazlll456/CS2-Card21Holdem.git  
2. Copy the `CS2-Card21Holdem` folder to `csgo/addons/counterstrikesharp/plugins/`.  
3. Start or restart your server, or load the plugin manually:  
   > css_plugins load CS2-Card21Holdem  
4. A `config.json` file will be created in `csgo/addons/counterstrikesharp/configs/plugins/CS2-Card21Holdem/`.

## Commands
| Command            | Arguments                     | Description                                                                 |
|--------------------|-------------------------------|-----------------------------------------------------------------------------|
| `!roll`            |                               | Rolls a random number between 1 and 100 for fun.                           |
| `!card21`          | `start`                       | Starts a new Card 21 (Blackjack) game against the dealer.                  |
| `!card21`          | `hit`                         | Draws another card in your Blackjack game.                                 |
| `!card21`          | `stand`                       | Ends your turn, letting the dealer play to determine the winner.           |
| `!holdem`          | `host <number>`               | Hosts a Texas Hold'em game for `<number>` players (e.g., `!holdem host 3`).|
| `!holdem`          | `join`                        | Joins an active Texas Hold'em game if there's space.                       |
| `!holdem`          | `start`                       | Starts the Texas Hold'em game (host only).                                 |
| `!holdem`          | `yes`                         | Continues to the next round, drawing another card.                         |
| `!holdem`          | `fold`                        | Folds your hand and exits the Texas Hold'em game.                          |
| `!holdem`          | `off`                         | Cancels the hosted game (host only).                                       |
| `!holdem`          | `result`                      | Shows hand rankings (e.g., Royal Flush > Straight Flush).                  |
| `!ch21`            | `enable`                      | Enables all games in the plugin (admin only).                              |
| `!ch21`            | `disable`                     | Disables all games in the plugin (admin only).                             |
| `!ch21`            | `reload`                      | Reloads the plugin configuration (admin only).                             |
| `!info-card`       |                               | Displays a detailed list of all commands.                                  |

**Console Commands**: Use the `css_` prefix for admin commands (e.g., `css_ch21_reload`).

## Configuration
The plugin generates a `config.json` file with the following key sections:

### Permissions
- `PermissionGroup`: Access mode (`all`, `whitelist`, `blacklist`).
- `Whitelist`: SteamIDs allowed to use the plugin in `whitelist` mode.
- `Blacklist`: SteamIDs blocked in `blacklist` mode.
- `Admins`: SteamIDs with admin permissions for restricted commands (e.g., `!ch21 enable`).

### Gameplay
- `MaxPlayers`: Maximum players per Texas Hold'em game (default: 4, minimum 2).
- `MaxRounds`: Number of rounds before the game ends (default: 4, range 1-10).
- `CommunityCardNumber`: Number of community cards in Texas Hold'em (default: 5, range 1-10).
- `CommandPrefix`: Prefix for all commands (default: `!holdem`).

### Bots
- `BotNames`: List of bot names for Texas Hold'em (default: `["Bot A", "Bot B", "Bot C"]`).
- `UseRandomBotName`: Randomly select a bot name (`true`) or use the first name (`false`).

### Messaging
- `PublicMessages`: Show game messages to all players (`true`) or only participants (`false`).
- `PublicResults`: Show game results to all players (`true`) or only participants (`false`).
- `IsDebugLogging`: Enable detailed console logs for debugging (`true`/`false`).

### Features
- `EnableHoldem`: Enable Texas Hold'em game mode (`true`/`false`).
- `EnableCard21`: Enable Card 21 (Blackjack) game mode (`true`/`false`).
- `EnableRoll`: Enable the roll command (`true`/`false`).

#### Example Config
    {
      "Permissions": {
        "PermissionGroup": "all",
        "PermissionGroupDescription": "Access mode: 'all' (everyone), 'whitelist' (only listed SteamIDs), 'blacklist' (exclude listed SteamIDs)",
        "Whitelist": [100000000000],
        "WhitelistDescription": "SteamIDs allowed to use the plugin when PermissionGroup is 'whitelist'",
        "Blacklist": [100000000000],
        "BlacklistDescription": "SteamIDs blocked from using the plugin when PermissionGroup is 'blacklist'",
        "Admins": [100000000000],
        "AdminsDescription": "SteamIDs of players with admin permissions for restricted commands (e.g., !ch21 enable, !ch21 reload)"
      },
      "Gameplay": {
        "MaxPlayers": 4,
        "MaxPlayersDescription": "Maximum players per game (minimum 2)",
        "MaxRounds": 4,
        "MaxRoundsDescription": "Number of rounds before game ends (1-10)",
        "CommunityCardNumber": 5,
        "CommunityCardNumberDescription": "Number of community cards dealt (1-10)",
        "CommandPrefix": "!holdem",
        "CommandPrefixDescription": "Prefix for all commands (e.g., '!holdem host 1')"
      },
      "Bots": {
        "BotNames": ["Bot A", "Bot B", "Bot C"],
        "BotNamesDescription": "List of bot names for Hold'em games",
        "UseRandomBotName": true,
        "UseRandomBotNameDescription": "Randomly select bot name from BotNames (true) or use first name (false)"
      },
      "Messaging": {
        "PublicMessages": false,
        "PublicMessagesDescription": "Show game messages to all players (true) or only participants (false)",
        "PublicResults": false,
        "PublicResultsDescription": "Show game results to all players (true) or only participants (false)",
        "IsDebugLogging": true,
        "IsDebugLoggingDescription": "Enable detailed console logs for debugging (true/false)"
      },
      "Features": {
        "EnableHoldem": true,
        "EnableHoldemDescription": "Enable Hold'em game mode (true/false)",
        "EnableCard21": true,
        "EnableCard21Description": "Enable Card 21 game mode (true/false)",
        "EnableRoll": true,
        "EnableRollDescription": "Enable Roll game mode (true/false)"
      }
    }

Reload the plugin after making changes:  
> css_plugins reload CS2-Card21Holdem

## Technical Details

### Classes
- **CardLogic**: Handles card game logic for both Card 21 and Texas Hold'em.  
  - Manages deck initialization, shuffling, and drawing.  
  - Calculates scores for Card 21 (e.g., Aces as 11 or 1).  
  - Evaluates Texas Hold'em hands (e.g., Royal Flush, Pair).  
- **ChatUtils**: Manages colored chat messages for players.  
- **ConfigManager**: Loads and manages the plugin’s configuration settings.  
- **GameManager**: Oversees game flow for Card 21 and Texas Hold'em.  
- **PlayerData**: Tracks player data, such as active games and cards.  
- **MainPlugin**: Main plugin class, handles events and command registration.

### Mechanics
- **Card 21 (Blackjack)**: Players aim for a score of 21 without going over. Aces are worth 11 or 1, face cards are 10. The dealer must draw until reaching 17 or higher.
- **Texas Hold'em**: Players receive 1 card per round (up to 4 rounds), with 5 community cards. Hands are evaluated (e.g., Flush, Straight), and the bot makes decisions based on hand strength.
- **Timeouts**: Texas Hold'em rounds have a 30-second timeout; players who don’t respond are automatically folded.

## Contributing
1. Fork the repository:  
   > git clone https://github.com/qazlll456/CS2-Card21Holdem.git  
2. Edit the code in a C# IDE (e.g., Visual Studio).  
3. Test on a local CS2 server.  
4. Submit a pull request with a detailed description of your changes.

## Notes
- **Performance**: Texas Hold'em games with multiple players may cause slight lag on weaker servers due to frequent chat updates.
- **Bot Behavior**: The bot in Texas Hold'em makes decisions based on hand strength, but its logic can be expanded for more complexity (e.g., adding bluffing behavior).
- **Disconnection Handling**: Players who disconnect during a Texas Hold'em game are automatically folded.

## License
[MIT License](LICENSE)

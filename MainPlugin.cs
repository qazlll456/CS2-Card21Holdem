using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Card21Holdem
{
    public class MainPlugin : BasePlugin
    {
        public override string ModuleName => "CS2-Card21Holdem";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "qazlll456 from HK with xAI assistance";
        public override string ModuleDescription => "A plugin for Card21 and Hold'em games in CS2";

        public override void Load(bool hotReload)
        {   
            GameManager.Initialize(this);

            AddCommandListener("say", OnSayCommand);

            AddCommand("css_ch21_reload", "Reload the CS2-Card21Holdem config", (player, info) =>
            {
                ConfigManager.ReloadConfig(null);
                Server.PrintToConsole("[CH21] Config reloaded successfully via server console!");
            });

            AddCommand("css_ch21_enable", "Enable the CS2-Card21Holdem game", (player, info) =>
            {
                ConfigManager.ToggleGame(null, true);
                Server.PrintToConsole("[CH21] Game enabled successfully via server console!");
            });

            AddCommand("css_ch21_disable", "Disable the CS2-Card21Holdem game", (player, info) =>
            {
                ConfigManager.ToggleGame(null, false);
                Server.PrintToConsole("[CH21] Game disabled successfully via server console!");
            });

            RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
            {
                if (@event.Userid != null && @event.Userid.IsValid)
                {
                    var steamId = @event.Userid.SteamID;
                    var playerName = @event.Userid.PlayerName ?? "Unknown";
                    
                    if (GameManager.holdemPlayers.Contains(steamId))
                    {
                        ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                            $"[CH21]{playerName} disconnected and has folded their cards.");
                        PlayerData.RemoveFromHoldem(steamId);
                        GameManager.CheckHoldemRound();

                        var activePlayers = GameManager.holdemPlayers.Where(PlayerData.IsInHoldem).ToList();
                        if (!activePlayers.Any(s => s != GameManager.BotSteamIdBase) && GameManager.CurrentGameCode != "00")
                        {
                            ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                                $"[CH21]No human players remain. Ending the game.");
                            GameManager.EndHoldem();
                        }
                    }
                    PlayerData.ClearPlayer(steamId);
                }
                return HookResult.Continue;
            });
        }

        private HookResult OnSayCommand(CCSPlayerController? player, CommandInfo info)
        {
            if (player == null || !player.IsValid) return HookResult.Continue;
            string message = info.GetArg(1).Trim();
            ulong steamId = player.SteamID;

            if (message.Equals("!ch21 enable") || message.Equals("!ch21 disable"))
            {
                if (!ConfigManager.IsAdmin(steamId))
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Warning, "[CH21]You do not have admin permission to toggle the game state!");
                    return HookResult.Handled;
                }
                ConfigManager.ToggleGame(player, message.EndsWith("enable"));
                return HookResult.Handled;
            }
            else if (message.Equals("!ch21 reload"))
            {
                if (!ConfigManager.IsAdmin(steamId))
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Warning, "[CH21]You do not have admin permission to reload the config!");
                    return HookResult.Handled;
                }
                ConfigManager.ReloadConfig(player);
                ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21] Config reloaded successfully!");
                return HookResult.Handled;
            }

            if (!ConfigManager.IsGameEnabled || !ConfigManager.CanPlayerUse(steamId))
            {
                ChatUtils.SendColoredMessage(player, MessageType.Warning, "[CH21]No permission or game disabled!");
                return HookResult.Handled;
            }

            if (message.Equals("!roll"))
            {
                GameManager.Roll(player);
            }
            else if (message.StartsWith("!card21"))
            {
                GameManager.HandleCard21(player, message);
            }
            else if (message.StartsWith("!holdem"))
            {
                GameManager.HandleHoldem(player, message);
            }
            else if (message.Equals("!info-card"))
            {
                ShowInfoCard(player);
            }
            else if (message.Equals("!ch21"))
            {
                var helpMessages = new List<string>
                {
                    "[CH21]Card21Holdem plugin",
                    "[CH21]!roll to roll a number",
                    "[CH21]!card21 help will list all command about !card21",
                    "[CH21]!holdem help will list all command about !holdem"
                };
                foreach (var msg in helpMessages)
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal, msg);
                }
            }
            else return HookResult.Continue;

            return HookResult.Handled;
        }

        private void ShowInfoCard(CCSPlayerController player)
        {
            ChatUtils.SendColoredMessage(player, MessageType.Important, "[CH21]=== CS2-Card21&Holdem Command List ===");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Roll - Generates a random number between 1 and 100 for fun.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Card21 new - Starts a new Blackjack game. You get 2 cards, dealer gets 2 (1 hidden). Aim for 21 without busting.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Card21 hit - Draws another card in your Blackjack game. Be careful not to bust (go over 21).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Card21 stand - Ends your turn in Blackjack. Dealer reveals their cards and draws until 17 or higher. Highest score wins.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Holdem host <maxPlayers> - Hosts a Texas Hold'em game for up to <maxPlayers> (e.g., !Holdem host 3 for 3 players).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Holdem join - Joins an active Texas Hold'em game if there's space and a host exists.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Holdem start - Starts the Texas Hold'em game (host only). Deals community cards and 1 card per player.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Holdem yes - Continues to the next round in Texas Hold'em, drawing another card for yourself.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Holdem no/fold - Folds your hand and exits the current Texas Hold'em game.");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!CH21 enable - Enables all games in the plugin (admin only).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!CH21 disable - Disables all games in the plugin (admin only).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!CH21 reload - Reloads the plugin configuration file (admin only).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, "[CH21]!Info-Card - Displays this detailed list of commands for the CS2-Card21&Holdem plugin.");
        }
    }
}
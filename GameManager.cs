using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using System.Threading.Tasks;

namespace CS2_Card21Holdem
{
    public class GameManager
    {
        public static string? holdemHost { get; set; } = null;
        public static List<ulong> holdemPlayers { get; } = new();
        public static List<string> communityCards { get; private set; } = new();
        public static int round { get; set; } = 0;

        public static BasePlugin? PluginInstance { get; set; }

        private static readonly Random rand = new Random();
        public static string BotName { get; set; } = "Bot A";
        public static readonly ulong BotSteamIdBase = 76561197999999999UL;

        public static string CurrentGameCode { get; private set; } = "00";
        public static bool IsHoldemActive => holdemPlayers.Count > 0;

        private static ulong? hostingPlayer = null;
        private static int? hostedGameNumber = null;
        private static DateTime? hostStartTime = null;
        private static bool isFirstRoundDisplay = true;
        private static List<ulong> pendingPlayers = new List<ulong>();

        public static ulong? WinnerSteamId { get; set; } = null;

        private static CounterStrikeSharp.API.Modules.Timers.Timer? currentTimer;
        private static bool isGameEnding = false;

        public static void Initialize(BasePlugin plugin)
        {
            PluginInstance = plugin;
        }
        
        public static void HandleHoldem(CCSPlayerController? player, string message)
        {
            try
            {
                if (player == null || !player.IsValid || string.IsNullOrEmpty(message))
                {
                    if (player != null && player.IsValid)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Warning, $"[RC: {CurrentGameCode}] Invalid player or message.");
                    }
                    return;
                }

                var steamId = player.SteamID;
                message = message.Trim().ToLower();
                string[] parts = message.Split(' ');

                if (IsHoldemActive && parts.Length > 1 && parts[1] != "yes" && parts[1] != "fold")
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Warning, $"[RC: {CurrentGameCode}] A game is already in progress. Please wait until it finishes.");
                    return;
                }

                if (parts.Length < 1 || !parts[0].StartsWith(ConfigManager.CommandPrefix.ToLower()))
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Warning, $"[RC: {CurrentGameCode}] Invalid command. Use {ConfigManager.CommandPrefix} [host/start/yes/fold/off/result].");
                    return;
                }

                if (parts.Length == 1 && parts[0] == ConfigManager.CommandPrefix.ToLower() || (parts.Length >= 2 && parts[1] == "help"))
                {
                    var helpMessages = new List<string>
                    {
                        "[CH21]Holdem Commands:",
                        $"[CH21]{ConfigManager.CommandPrefix} host [number] - Host a game with [number] players",
                        $"[CH21]{ConfigManager.CommandPrefix} join - Join a hosted game",
                        $"[CH21]{ConfigManager.CommandPrefix} start - Start the game (host only)",
                        $"[CH21]{ConfigManager.CommandPrefix} yes - Continue to the next round",
                        $"[CH21]{ConfigManager.CommandPrefix} fold - Fold your cards",
                        $"[CH21]{ConfigManager.CommandPrefix} off - Cancel the game (host only)",
                        $"[CH21]{ConfigManager.CommandPrefix} result - Show hand rankings",
                        $"[CH21]{ConfigManager.CommandPrefix} help - Show this help message"
                    };
                    foreach (var msg in helpMessages)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal, msg);
                    }
                    return;
                }

                if (parts.Length >= 2 && parts[1] == "host")
                {
                    int maxPlayers = ConfigManager.MaxPlayers;
                    if (parts.Length < 3 || !int.TryParse(parts[2], out int gameNumber) || gameNumber <= 0 || gameNumber > maxPlayers)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Warning, 
                            $"[RC: {CurrentGameCode}] Invalid game number. Must be 1-{maxPlayers}. Use {ConfigManager.CommandPrefix} host [number]");
                        return;
                    }

                    if (hostingPlayer.HasValue)
                    {
                        if (hostingPlayer == steamId)
                        {
                            ChatUtils.SendColoredMessage(player, MessageType.Normal,
                                $"[RC: {CurrentGameCode}] You are already hosting a game for {hostedGameNumber} players. Use {ConfigManager.CommandPrefix} start to begin or {ConfigManager.CommandPrefix} off to cancel.");
                            return;
                        }
                        else
                        {
                            ChatUtils.SendColoredMessage(player, MessageType.Normal,
                                $"[RC: {CurrentGameCode}] Another player is already hosting, wait for them to start or timeout");
                            return;
                        }
                    }

                    ClearHostingState();
                    hostingPlayer = steamId;
                    hostedGameNumber = gameNumber;
                    hostStartTime = DateTime.Now;
                    pendingPlayers.Clear();
                    pendingPlayers.Add(steamId);

                    IEnumerable<CCSPlayerController> recipients = ConfigManager.IsPublicMessages ? 
                        Utilities.GetPlayers() : new[] { player };
                    ChatUtils.SendColoredMessage(recipients, MessageType.Normal,
                        $"[CH21] A {gameNumber}-player Hold'em game is hosting by {ChatColors.Green}{player.PlayerName}{ChatColors.White}.");
                    ChatUtils.SendColoredMessage(recipients, MessageType.Normal,
                        $"[CH21] Use {ChatColors.Yellow}{ConfigManager.CommandPrefix} join{ChatColors.White} to join, {ChatColors.Yellow}{ConfigManager.CommandPrefix} start{ChatColors.White} to start, or {ChatColors.Yellow}{ConfigManager.CommandPrefix} off{ChatColors.White} to cancel.");

                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(30000);
                            if (hostingPlayer == steamId && !IsHoldemActive)
                            {
                                ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                                    $"[RC: {CurrentGameCode}] Hosting by {player.PlayerName} timed out, game cancelled");
                                ClearHostingState();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[CH21] Error in hosting timeout task: {ex.Message}\n{ex.StackTrace}");
                            ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Warning,
                                $"[RC: {CurrentGameCode}] An error occurred during hosting timeout. Game cancelled.");
                            ClearHostingState();
                        }
                    });
                    return;
                }
                else if (message == $"{ConfigManager.CommandPrefix} join")
                {
                    if (!hostingPlayer.HasValue)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] No game is hosted. Use {ConfigManager.CommandPrefix} host [number] first.");
                        return;
                    }

                    if (pendingPlayers.Contains(steamId))
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] You have already joined the game.");
                        return;
                    }

                    if (pendingPlayers.Count >= hostedGameNumber)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] The game is full ({hostedGameNumber} players).");
                        return;
                    }

                    pendingPlayers.Add(steamId);
                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                        $"[CH21] {ChatColors.Green}{player.PlayerName}{ChatColors.White} has joined the Hold'em game! ({pendingPlayers.Count}/{hostedGameNumber})");
                    return;
                }
                else if (message == $"{ConfigManager.CommandPrefix} start")
                {
                    if (hostingPlayer.HasValue && hostingPlayer != steamId)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] Only the host can start the game");
                        return;
                    }

                    if (!hostingPlayer.HasValue)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] No game is hosted. Use {ConfigManager.CommandPrefix} host [number] first");
                        return;
                    }

                    StartHoldem(steamId);
                    return;
                }
                else if (message == $"{ConfigManager.CommandPrefix} off")
                {
                    if (hostingPlayer.HasValue && hostingPlayer != steamId)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] Only the host can cancel the game");
                        return;
                    }

                    if (!hostingPlayer.HasValue)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[RC: {CurrentGameCode}] No game is hosted to cancel");
                        return;
                    }

                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                        $"[RC: {CurrentGameCode}] Hosting cancelled by {player.PlayerName}");
                    ClearHostingState();
                    return;
                }
                else if (message == $"{ConfigManager.CommandPrefix} result")
                {
                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                        $"[CH21] Win result from big to small: [Royal Flush] > [Straight Flush] > [Four of a Kind] > [Full House] > [Flush] > [Straight] > [Three of a Kind] > [Two Pairs] > [Pair] > [High Card]");
                    return;
                }
                else if (message == $"{ConfigManager.CommandPrefix} yes" || message == $"{ConfigManager.CommandPrefix} fold")
                {
                    if (!PlayerData.IsInHoldem(steamId) || !IsHoldemActive)
                    {
                        ChatUtils.SendColoredMessage(player, MessageType.Normal,
                            $"[CH21] No active game!, {ConfigManager.CommandPrefix} host [num] to host a game");
                        return;
                    }

                    bool choice = message == $"{ConfigManager.CommandPrefix} yes";
                    PlayerData.SetHoldemChoice(steamId, choice);
                    CheckHoldemRound(player, choice);
                }
                else
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal,
                        $"[RC: {CurrentGameCode}] Invalid command. Use {ConfigManager.CommandPrefix} [host/start/yes/fold/off/result]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CH21] Error in HandleHoldem: {ex.Message}\n{ex.StackTrace}");
                ChatUtils.SendColoredMessage(player!, MessageType.Warning,
                    $"[RC: {CurrentGameCode}] An error occurred while processing your command. The game has been reset.");
                EndHoldem();
            }
        }

        private static void StartHoldem(ulong hostSteamId)
        {
            holdemPlayers.Clear();
            holdemPlayers.AddRange(pendingPlayers);
            holdemPlayers.Add(BotSteamIdBase);
            holdemHost = Utilities.GetPlayerFromSteamId(hostSteamId)?.PlayerName ?? "Unknown";
            CurrentGameCode = DateTime.Now.Second.ToString("D2");
            BotName = ConfigManager.UseRandomBotName 
                ? ConfigManager.BotNames[new Random().Next(ConfigManager.BotNames.Count)] 
                : ConfigManager.BotNames[0];

            communityCards = new List<string>();
            CardLogic.InitializeDeck();

            for (int i = 0; i < ConfigManager.CommunityCardNumber; i++)
            {
                var card = CardLogic.DrawCard();
                communityCards.Add(card);
            }

            foreach (var steamId in holdemPlayers)
            {
                var card = CardLogic.DrawCard();
                PlayerData.SetHoldemCards(steamId, new List<string> { card });
                PlayerData.SetInHoldem(steamId, true);
                PlayerData.SetHoldemChoice(steamId, false);
            }

            ChatUtils.ShowHoldemStart(holdemPlayers, communityCards, CurrentGameCode, isFirstRoundDisplay);
            ChatUtils.SendColoredMessage(Utilities.GetPlayers().Where(p => holdemPlayers.Contains(p.SteamID)), MessageType.Normal,
                $"[CH21]Use {ChatColors.Yellow}{ConfigManager.CommandPrefix} [Yes/Fold]{ChatColors.White} ['yes' to continue or 'fold' to give up]");
            isFirstRoundDisplay = false;
            round = 0;
            ClearHostingState();

            StartRoundTimeout();
        }

        private static void StartRoundTimeout()
        {
            if (PluginInstance == null)
            {
                return;
            }
            if (currentTimer != null)
            {
                currentTimer.Kill();
            }

            Server.NextFrame(() =>
            {
                currentTimer = PluginInstance.AddTimer(30.0f, () =>
                {
                    var activePlayers = holdemPlayers.Where(PlayerData.IsInHoldem).ToList();
                    var pendingPlayers = activePlayers.Where(s => !PlayerData.HasMadeChoice(s) && s != BotSteamIdBase).ToList();

                    if (pendingPlayers.Any())
                    {
                        foreach (var steamId in pendingPlayers)
                        {
                            var playerName = Utilities.GetPlayerFromSteamId(steamId)?.PlayerName ?? "Unknown";
                            ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                                $"[CH21]{playerName} didn't respond in 30 seconds and has folded their cards.");
                            PlayerData.RemoveFromHoldem(steamId);
                        }
                        CheckHoldemRound();
                    }
                });
            });
        }

        public static void CheckHoldemRound(CCSPlayerController? player = null, bool? playerChoice = null)
        {
            if (isGameEnding)
            {
                return;
            }

            if (player != null && playerChoice.HasValue)
            {
                var playerName = player.PlayerName;
                if (!playerChoice.Value)
                {
                    PlayerData.RemoveFromHoldem(player.SteamID);
                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                        $"[CH21]{playerName} choose to folded the card");
                }
                else
                {
                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                        $"[CH21]{playerName} choose to go");
                }
                if (currentTimer != null)
                {
                    currentTimer.Kill();
                }
            }

            var activePlayers = holdemPlayers.Where(PlayerData.IsInHoldem).ToList();
            var pendingPlayers = activePlayers.Where(s => !PlayerData.HasMadeChoice(s) && s != BotSteamIdBase).ToList();
            if (pendingPlayers.Any())
            {
                ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal, 
                    $"[CH21]{pendingPlayers.Count} is still pending for action (yes/fold)");
                return;
            }

            var playerList = activePlayers.Select(s => s == BotSteamIdBase ? BotName : Utilities.GetPlayerFromSteamId(s)?.PlayerName ?? "Unknown").ToList();

            if (activePlayers.Count == 1)
            {
                var winnerSteamId = activePlayers.First();
                var winnerName = winnerSteamId == BotSteamIdBase ? BotName : Utilities.GetPlayerFromSteamId(winnerSteamId)?.PlayerName ?? "Unknown";
                WinnerSteamId = winnerSteamId;
                isGameEnding = true;
                EndHoldem();
                return;
            }

            var humanPlayers = activePlayers.Where(s => s != BotSteamIdBase).ToList();
            var bots = activePlayers.Where(s => s == BotSteamIdBase).ToList();

            if (humanPlayers.All(s => PlayerData.HasMadeChoice(s)))
            {
                var messages = new List<string>();

                if (bots.Any())
                {
                    foreach (var botSteamId in bots)
                    {
                        var botCards = PlayerData.GetHoldemCards(botSteamId) ?? new List<string>();
                        var botHand = CardLogic.EvaluateHoldemHand(botCards, communityCards);
                        var humanSteamId = humanPlayers.FirstOrDefault();
                        var humanHand = humanSteamId != 0 ? CardLogic.EvaluateHoldemHand(PlayerData.GetHoldemCards(humanSteamId) ?? new List<string>(), communityCards) : new HandResult("High Card", new List<string>());
                        bool botContinues;

                        int comparison = CardLogic.CompareHands(botHand, humanHand);
                        if (comparison > 0) botContinues = true;
                        else if (comparison == 0) botContinues = round == 0 || (CardLogic.GetHandRank(botHand.Hand) >= CardLogic.GetHandRank("Pair") && rand.Next(0, 10) > 2);
                        else
                        {
                            if (CardLogic.GetHandRank(humanHand.Hand) >= CardLogic.GetHandRank("Flush"))
                                botContinues = CardLogic.GetHandRank(botHand.Hand) >= CardLogic.GetHandRank("Full House") && rand.Next(0, 10) > 4;
                            else
                                botContinues = round == 0 || (CardLogic.GetHandRank(botHand.Hand) >= CardLogic.GetHandRank("Pair") && rand.Next(0, 10) > 2);
                        }

                        PlayerData.SetHoldemChoice(botSteamId, botContinues);

                        if (!botContinues)
                        {
                            messages.Add($"[CH21]{ChatColors.Green}{BotName}{ChatColors.White} has folded!");
                            PlayerData.RemoveFromHoldem(botSteamId);
                            activePlayers = holdemPlayers.Where(PlayerData.IsInHoldem).ToList();

                            if (activePlayers.Count == 1)
                            {
                                var winnerSteamId = activePlayers.First();
                                var winnerName = winnerSteamId == BotSteamIdBase ? BotName : Utilities.GetPlayerFromSteamId(winnerSteamId)?.PlayerName ?? "Unknown";
                                WinnerSteamId = winnerSteamId;
                                isGameEnding = true;
                                EndHoldem();
                                return;
                            }
                        }
                    }
                }

                round++;
                if (round >= ConfigManager.MaxRounds)
                {
                    var playerHands = activePlayers.Select(steamId => new
                    {
                        SteamId = steamId,
                        Name = steamId == BotSteamIdBase ? BotName : Utilities.GetPlayerFromSteamId(steamId)?.PlayerName ?? "Unknown",
                        HandResult = CardLogic.EvaluateHoldemHand(PlayerData.GetHoldemCards(steamId) ?? new List<string>(), communityCards ?? new List<string>())
                    }).OrderByDescending(h => CardLogic.GetHandRank(h.HandResult.Hand))
                            .ThenByDescending(h => h.HandResult.ThreeRank)
                            .ThenByDescending(h => h.HandResult.PairRank)
                            .ThenByDescending(h => h.HandResult.SecondPairRank)
                            .ThenByDescending(h => h.HandResult.HighCardValue)
                            .ToList();

                    var winner = playerHands.First();
                    WinnerSteamId = winner.SteamId;
                    ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                        $"[CH21]{ChatColors.Green}{winner.Name}{ChatColors.White} wins with {ChatColors.Yellow}{winner.HandResult.Hand}{ChatColors.White}!");
                    isGameEnding = true;
                    EndHoldem();
                    return;
                }
                else
                {
                    foreach (var steamId in holdemPlayers.Where(PlayerData.IsInHoldem))
                    {
                        var newCard = CardLogic.DrawCard();
                        PlayerData.AddHoldemCard(steamId, newCard);
                        var currentCards = PlayerData.GetHoldemCards(steamId) ?? new List<string>();
                        var previousCards = currentCards.Take(currentCards.Count - 1).ToList();
                        var playerObj = Utilities.GetPlayerFromSteamId(steamId);
                        if (steamId == BotSteamIdBase)
                        {
                            messages.Add($"[CH21]Player: [{ChatColors.Green}{BotName}{ChatColors.White}] Current: {string.Join(", ", previousCards.OrderBy(c => CardLogic.CardValue(c) == 0 ? int.MaxValue : CardLogic.CardValue(c)).Select(c => $"{ChatUtils.GetCardColor(c)}{c}"))}, New: {ChatUtils.GetCardColor(newCard)}{newCard}");
                        }
                        else if (playerObj != null && playerObj.IsValid)
                        {
                            messages.Add($"[CH21]Player: [{ChatColors.Green}{playerObj.PlayerName}{ChatColors.White}] Current: {string.Join(", ", previousCards.OrderBy(c => CardLogic.CardValue(c) == 0 ? int.MaxValue : CardLogic.CardValue(c)).Select(c => $"{ChatUtils.GetCardColor(c)}{c}"))}, New: {ChatUtils.GetCardColor(newCard)}{newCard}");
                        }
                    }
                    messages.Insert(0, $"[CH21]Active players: {ChatColors.Green}{string.Join(", ", playerList)}{ChatColors.White}");
                    messages.Add($"[CH21][RC{CurrentGameCode}] Holdem Round {round}:");
                    messages.Add($"[CH21]Community: {string.Join(", ", communityCards.OrderBy(c => CardLogic.CardValue(c)).Select(c => $"{ChatUtils.GetCardColor(c)}{c}"))}");
                    messages.Add($"[CH21]Use {ChatColors.Yellow}{ConfigManager.CommandPrefix} [Yes/Fold]{ChatColors.White} ['yes' to continue or 'fold' to give up]");

                    foreach (var msg in messages)
                    {
                        ChatUtils.SendColoredMessage(Utilities.GetPlayers().Where(p => holdemPlayers.Contains(p.SteamID)), MessageType.Normal, msg);
                    }

                    foreach (var steamId in holdemPlayers.Where(PlayerData.IsInHoldem))
                    {
                        PlayerData.SetHoldemChoice(steamId, false);
                    }
                    StartRoundTimeout();
                }
            }
        }

        public static void EndHoldem()
        {
            string winnerMessage = WinnerSteamId.HasValue
                ? $"[CH21]{(WinnerSteamId == BotSteamIdBase ? BotName : Utilities.GetPlayerFromSteamId(WinnerSteamId.Value)?.PlayerName ?? "Unknown")} is the winner! {(holdemPlayers.All(p => p == WinnerSteamId || !PlayerData.IsInHoldem(p)) ? "Everyone folded their cards!" : "Status-Highest score!")}"
                : $"[CH21]No winner!";

            if (ConfigManager.IsPublicResults)
                ChatUtils.SendColoredMessage(Utilities.GetPlayers().Where(p => p != null && p.IsValid), MessageType.Normal, winnerMessage);
            else
            {
                foreach (var steamId in holdemPlayers)
                {
                    var player = Utilities.GetPlayerFromSteamId(steamId);
                    if (player != null && player.IsValid)
                        ChatUtils.SendColoredMessage(player, MessageType.Normal, winnerMessage);
                }
            }

            ChatUtils.ShowHoldemResults(holdemPlayers, communityCards);

            var playersToClear = new List<ulong>(holdemPlayers);
            holdemPlayers.Clear();
            holdemHost = null;
            communityCards.Clear();
            round = 0;
            CurrentGameCode = "00";
            isFirstRoundDisplay = true;
            foreach (var steamId in playersToClear)
            {
                PlayerData.ClearPlayer(steamId);
            }
            WinnerSteamId = null;
            ClearHostingState();
            isGameEnding = false;
        }

        private static void ClearHostingState()
        {
            hostingPlayer = null;
            hostedGameNumber = null;
            hostStartTime = null;
            pendingPlayers.Clear();
        }

        public static void HandlePluginInfo(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid) return;

            var messages = new List<string>
            {
                "[CH21]Card21Holdem plugin",
                "[CH21]!roll to roll a number from 1-100",
                "[CH21]!card21 help will list all command about !card21",
                "[CH21]!holdem help will list all command about !holdem"
            };

            foreach (var msg in messages)
            {
                ChatUtils.SendColoredMessage(player, MessageType.Normal, msg);
            }
        }

        public static void Roll(CCSPlayerController player)
        {
            if (!ConfigManager.EnableRoll)
            {
                ChatUtils.SendColoredMessage(player, MessageType.Warning, $"[Roll] The !roll command is disabled by the server.");
                return;
            }
            if (player == null || !player.IsValid) return;
            int roll = new Random().Next(1, 101);
            ChatUtils.SendColoredMessage(Utilities.GetPlayers(), MessageType.Normal,
                $"[Roll] {ChatColors.Green}{player.PlayerName}{ChatColors.White} rolled {ChatColors.Yellow}{roll}{ChatColors.White}!");
        }

        public static void HandleCard21(CCSPlayerController player, string message)
        {
            if (!ConfigManager.EnableCard21)
            {
                ChatUtils.SendColoredMessage(player, MessageType.Warning, $"[CH21] The !card21 command is disabled by the server.");
                return;
            }
            if (player == null || !player.IsValid || string.IsNullOrEmpty(message)) return;

            var steamId = player.SteamID;
            message = message.Trim().ToLower();
            string[] parts = message.Split(' ');

            if (parts.Length < 2) return;

            if (parts[1] == "help")
            {
                var helpMessages = new List<string>
                {
                    "[CH21]Card21 Commands:",
                    "[CH21]!card21 start - Start a new Card 21 game",
                    "[CH21]!card21 hit - Draw one more card",
                    "[CH21]!card21 stand - End your turn and let the dealer play"
                };
                foreach (var msg in helpMessages)
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal, msg);
                }
                return;
            }
            else if (parts[1] == "start")
            {
                if (PlayerData.IsInCard21(steamId))
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal, $"[CH21] You are already in a game!");
                    return;
                }

                CardLogic.InitializeDeck();
                var game = new Card21Game
                {
                    PlayerCards = new List<string> { CardLogic.DrawCard(), CardLogic.DrawCard() },
                    DealerCards = new List<string> { CardLogic.DrawCard() }
                };
                PlayerData.SetCard21Game(steamId, game);

                ChatUtils.SendColoredMessage(player, MessageType.Normal,
                    $"[CH21]Card 21 Game started! Your cards: {string.Join(", ", game.PlayerCards.Select(c => $"{ChatUtils.GetCardColor(c)}{c}\x01"))} (Score: {CardLogic.CalculateScore(game.PlayerCards)})");
                ChatUtils.SendColoredMessage(player, MessageType.Normal,
                    $"[CH21]Dealer's card: {ChatUtils.GetCardColor(game.DealerCards[0])}{game.DealerCards[0]}\x01");
                ChatUtils.SendColoredMessage(player, MessageType.Normal,
                    $"[CH21]!card21 \"Hit\" draw one more card or \"stand\" end it now");
            }
            else if (parts[1] == "hit")
            {
                var game = PlayerData.GetCard21Game(steamId);
                if (game == null)
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal,
                        $"[CH21] You are not in a game! Use !card21 start to begin.");
                    return;
                }
                CardLogic.Hit(player, game);
            }
            else if (parts[1] == "stand")
            {
                var game = PlayerData.GetCard21Game(steamId);
                if (game == null)
                {
                    ChatUtils.SendColoredMessage(player, MessageType.Normal,
                        $"[CH21] You are not in a game! Use !card21 start to begin.");
                    return;
                }
                CardLogic.Stand(player, game);
            }
        }
    }
}
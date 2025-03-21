using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Card21Holdem
{
    public enum MessageType
    {
        Normal,
        Warning,
        Important
    }

    public static class ChatUtils
    {
        public static void SendColoredMessage(CCSPlayerController? player, MessageType type, string message)
        {
            if (player == null || !player.IsValid) return;

            string prefix = type switch
            {
                MessageType.Warning => $"{ChatColors.Red.ToString()}[Warning] ",
                MessageType.Important => $"{ChatColors.Yellow.ToString()}[Important] ",
                _ => ""
            };

            player.PrintToChat($"{prefix}{message}");
        }

        public static void SendColoredMessage(IEnumerable<CCSPlayerController> players, MessageType type, string message)
        {
            foreach (var player in players.Where(p => p != null && p.IsValid))
            {
                SendColoredMessage(player, type, message);
            }
        }

        public static string GetCardColor(string card)
        {
            if (string.IsNullOrEmpty(card) || card == "N/A") return ChatColors.Default.ToString();

            string suit = card[^1..];
            return suit switch
            {
                "♥" => ChatColors.Red.ToString(),
                "♦" => ChatColors.Red.ToString(),
                "♠" => ChatColors.Green.ToString(),
                "♣" => ChatColors.Green.ToString(),
                _ => ChatColors.Default.ToString()
            };
        }

        public static void ShowHoldemStart(List<ulong> holdemPlayers, List<string> communityCards, string gameCode, bool isFirstRoundDisplay)
        {
            var playerNames = holdemPlayers.Select(s => s == GameManager.BotSteamIdBase ? GameManager.BotName : Utilities.GetPlayerFromSteamId(s)?.PlayerName ?? "Unknown").ToList();
            string startMessage = $"[CH21][RC{gameCode}] Holdem game started! Active players: {string.Join(", ", playerNames)}";

            if (ConfigManager.IsPublicMessages)
            {
                var publicPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid && !holdemPlayers.Contains(p.SteamID)).ToList();
                SendColoredMessage(publicPlayers, MessageType.Normal, startMessage);
            }

            var humanPlayers = holdemPlayers
                .Where(s => s != GameManager.BotSteamIdBase)
                .Select(s => Utilities.GetPlayerFromSteamId(s))
                .Where(p => p != null && p.IsValid)
                .ToList();

            foreach (var player in humanPlayers)
            {
                if (player != null)
                {
                    SendColoredMessage(player, MessageType.Normal, startMessage);
                }
            }

            foreach (var steamId in holdemPlayers)
            {
                var playerName = steamId == GameManager.BotSteamIdBase ? GameManager.BotName : Utilities.GetPlayerFromSteamId(steamId)?.PlayerName ?? "Unknown";
                var cards = PlayerData.GetHoldemCards(steamId) ?? new List<string>();
                var coloredCards = string.Join(", ", cards.Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));
                var cardMessage = $"[CH21]{playerName} - cards: {coloredCards}";

                foreach (var player in humanPlayers)
                {
                    if (player != null)
                    {
                        SendColoredMessage(player, MessageType.Normal, cardMessage);
                    }
                }
            }

            if (isFirstRoundDisplay)
            {
                var coloredCommunityCards = string.Join(", ", communityCards.Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));
                foreach (var player in humanPlayers)
                {
                    if (player != null)
                    {
                        SendColoredMessage(player, MessageType.Normal, $"[CH21]Community: {coloredCommunityCards}");
                    }
                }
            }
        }

        public static void ShowHoldemRound(List<ulong> holdemPlayers, List<string> communityCards, string gameCode, int round)
        {
            var messages = new List<string>();
            var playerList = holdemPlayers.Select(s => s == GameManager.BotSteamIdBase ? GameManager.BotName : Utilities.GetPlayerFromSteamId(s)?.PlayerName ?? "Unknown").ToList();
            messages.Add($"Active players: {ChatColors.Green.ToString()}{string.Join(", ", playerList)}{ChatColors.Default.ToString()}");
            messages.Add($"[RC{gameCode}] Holdem Round {round}:");
            var coloredCommunityCards = string.Join(", ", communityCards.OrderBy(c => CardLogic.CardValue(c)).Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));
            messages.Add($"Community: {coloredCommunityCards}");
            messages.Add($"Use {ChatColors.Yellow.ToString()}{ConfigManager.CommandPrefix} [Yes/Fold]{ChatColors.Default.ToString()} ['yes' to continue or 'fold' to give up]");
            SendColoredMessage(Utilities.GetPlayers().Where(p => p != null && p.IsValid && !holdemPlayers.Contains(p.SteamID)), MessageType.Normal, 
                $"[RC{gameCode}] Holdem Round {round}:\nCommunity: {coloredCommunityCards}\nActive players: {string.Join(", ", playerList)}");
            foreach (var msg in messages)
            {
                SendColoredMessage(Utilities.GetPlayers().Where(p => p != null && p.IsValid && holdemPlayers.Contains(p.SteamID)), MessageType.Normal, msg);
            }

            if (ConfigManager.IsPublicMessages)
            {
                SendColoredMessage(Utilities.GetPlayers().Where(p => p != null && p.IsValid && !holdemPlayers.Contains(p.SteamID)), MessageType.Normal, 
                    $"[RC{gameCode}] Holdem Round {round}:\nCommunity: {coloredCommunityCards}\nActive players: {string.Join(", ", playerList)}");
            }
        }

            public static void ShowHoldemResults(List<ulong> holdemPlayers, List<string> communityCards)
            {
                var communityStr = string.Join(", ", communityCards.OrderBy(c => CardLogic.CardValue(c)).Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));
                var allMessages = new List<string>
                {
                    $"[CH21][RC: {GameManager.CurrentGameCode}] Results of the game:",
                    $"[CH21]Community: {communityStr}"
                };

            var hands = holdemPlayers.Select(steamId => new
            {
                SteamId = steamId,
                HandResult = CardLogic.EvaluateHoldemHand(PlayerData.GetHoldemCards(steamId) ?? new List<string>(), communityCards),
                IsWinner = GameManager.WinnerSteamId.HasValue && steamId == GameManager.WinnerSteamId.Value,
                Folded = !PlayerData.IsInHoldem(steamId)
            }).ToList();
            var winners = hands.Where(h => h.IsWinner && !h.Folded).ToList();
            var needsKicker = winners.Count > 1 || (winners.Count == 1 && hands.Any(h => !h.Folded && !h.IsWinner && h.HandResult.Hand == winners[0].HandResult.Hand && CardLogic.CompareHands(h.HandResult, winners[0].HandResult) == 0));

            // Collect result messages for all players (including the bot)
            foreach (var steamId in holdemPlayers)
            {
                var name = steamId == GameManager.BotSteamIdBase ? GameManager.BotName : Utilities.GetPlayerFromSteamId(steamId)?.PlayerName ?? "Unknown";
                var cards = PlayerData.GetHoldemCards(steamId) ?? new List<string>();
                var hand = CardLogic.EvaluateHoldemHand(cards, communityCards);
                var folded = !PlayerData.IsInHoldem(steamId);
                var isWinner = GameManager.WinnerSteamId.HasValue && steamId == GameManager.WinnerSteamId.Value;
                var status = isWinner ? "Winner" : (folded ? "(Folded)" : "(Loser)");

                var relevantCards = hand.Hand switch
                {
                    "Two Pair" => hand.RelevantCards.Take(4).ToList(),
                    "Pair" => hand.RelevantCards.Take(2).ToList(),
                    "High Card" => hand.RelevantCards.Take(1).ToList(),
                    "Three of a Kind" => hand.RelevantCards.Take(3).ToList(),
                    "Four of a Kind" => hand.RelevantCards.Take(4).ToList(),
                    _ => hand.RelevantCards
                };

                var formattedCards = relevantCards.Select(c => cards.Contains(c) ? $"[p]{c}" : $"[c]{c}").ToList();
                var coloredRelevantCards = string.Join(", ", formattedCards.Select(c =>
                {
                    var card = c.Substring(3);
                    var color = GetCardColor(card);
                    return $"{color}{c}{ChatColors.Default.ToString()}";
                }));

                var handDisplay = (hand.Hand == "Two Pair" && needsKicker && isWinner)
                    ? $"{hand.Hand.ToLower()} (Kicker: {ChatUtils.GetCardColor(hand.RelevantCards.Last())}{hand.RelevantCards.Last()}{ChatColors.Default.ToString()})"
                    : hand.Hand.ToLower();

                var resultMessage = $"[CH21]{status}: {name} - Best Hand: {handDisplay} - {coloredRelevantCards}";
                allMessages.Add(resultMessage);
            }

            // Send all messages to the appropriate recipients
            if (ConfigManager.IsPublicResults)
            {
                // Send to all valid players
                foreach (var msg in allMessages)
                {
                    SendColoredMessage(Utilities.GetPlayers().Where(p => p != null && p.IsValid), MessageType.Normal, msg);
                }
            }
            else
            {
                // Send to each human player (excluding the bot)
                var humanPlayers = holdemPlayers
                    .Where(steamId => steamId != GameManager.BotSteamIdBase)
                    .Select(steamId => Utilities.GetPlayerFromSteamId(steamId))
                    .Where(player => player != null && player.IsValid)
                    .ToList();

                foreach (var player in humanPlayers)
                {
                    foreach (var msg in allMessages)
                    {
                        SendColoredMessage(player, MessageType.Normal, msg);
                    }
                }
            }
        }

        public static void ShowCard21Result(CCSPlayerController player, Card21Game game, int playerScore, int dealerScore, string result)
        {
            if (player == null || !player.IsValid) return;

            var playerCards = game.PlayerCards ?? new List<string>();
            var dealerCards = game.DealerCards ?? new List<string>();
            var coloredPlayerCards = string.Join(", ", playerCards.Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));
            var coloredDealerCards = string.Join(", ", dealerCards.Select(c => $"{GetCardColor(c)}{c}{ChatColors.Default.ToString()}"));

            ChatUtils.SendColoredMessage(player, MessageType.Normal,
                $"[CH21]Result: {result}");
            ChatUtils.SendColoredMessage(player, MessageType.Normal,
                $"[CH21]Your cards: {coloredPlayerCards} (Score: {playerScore}).");
            ChatUtils.SendColoredMessage(player, MessageType.Normal,   
                $"[CH21]Dealer's cards: {coloredDealerCards} (Score: {dealerScore}).");
        }
    }
}
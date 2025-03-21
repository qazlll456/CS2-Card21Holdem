using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Card21Holdem
{
    public static class CardLogic
    {
        private static List<string> deck = new List<string>();
        private static readonly string[] suits = { "♥", "♠", "♣", "♦" };
        private static readonly string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        private static readonly Dictionary<string, int> cardValueCache = new Dictionary<string, int>();

        public static void InitializeDeck()
        {
            deck.Clear();
            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    deck.Add($"{rank}{suit}");
                }
            }
            ShuffleDeck();
        }

        private static void ShuffleDeck()
        {
            var rng = new Random();
            deck = deck.OrderBy(_ => rng.Next()).ToList();
        }

        public static string DrawCard()
        {
            if (deck.Count == 0)
            {
                return "N/A";
            }
            var card = deck[0];
            deck.RemoveAt(0);
            return card;
        }

        public static int CardValue(string card)
        {
            if (string.IsNullOrEmpty(card) || card == "N/A")
            {
                return 0;
            }

            if (cardValueCache.TryGetValue(card, out int cachedValue))
            {
                return cachedValue;
            }

            try
            {
                string rank = card.Length >= 2 ? (card.StartsWith("10") ? "10" : card[..^1]) : card;
                int value = rank switch
                {
                    "A" => 14,
                    "K" => 13,
                    "Q" => 12,
                    "J" => 11,
                    "10" => 10,
                    "9" => 9,
                    "8" => 8,
                    "7" => 7,
                    "6" => 6,
                    "5" => 5,
                    "4" => 4,
                    "3" => 3,
                    "2" => 2,
                    _ => 0
                };
                cardValueCache[card] = value;
                return value;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"[CH21] CardValue Error: Invalid card format '{card}', returning 0. Error: {ex.Message}");
                return 0;
            }
        }

        public static int GetHandRank(string hand)
        {
            return hand switch
            {
                "Royal Flush" => 10,
                "Straight Flush" => 9,
                "Four of a Kind" => 8,
                "Full House" => 7,
                "Flush" => 6,
                "Straight" => 5,
                "Three of a Kind" => 4,
                "Two Pair" => 3,
                "Pair" => 2,
                "High Card" => 1,
                _ => 0
            };
        }

        public static int CalculateScore(List<string> cards)
        {
            if (cards == null || !cards.Any()) return 0;

            int score = 0;
            int aceCount = 0;

            foreach (var card in cards)
            {
                if (string.IsNullOrEmpty(card) || card == "N/A") continue;

                string rank = card.Length >= 2 ? (card.StartsWith("10") ? "10" : card[..^1]) : card;
                if (rank == "A")
                {
                    aceCount++;
                    score += 11;
                }
                else if (rank == "K" || rank == "Q" || rank == "J")
                {
                    score += 10;
                }
                else
                {
                    score += int.TryParse(rank, out int value) ? value : 0;
                }
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        public static void Hit(CCSPlayerController player, Card21Game game)
        {
            if (game.PlayerCards == null) game.PlayerCards = new List<string>();
            var newCard = DrawCard();
            game.PlayerCards.Add(newCard);
            int playerScore = CalculateScore(game.PlayerCards);
            
            ChatUtils.SendColoredMessage(player, MessageType.Normal, $"[CH21]You drawed 1 card!");
            ChatUtils.SendColoredMessage(player, MessageType.Normal, $"[CH21]Hit: {ChatUtils.GetCardColor(newCard)}{newCard}\x01 (Score: {playerScore})");
            
            if (playerScore > 21)
            {
                ChatUtils.ShowCard21Result(player, game, playerScore, game.DealerCards != null ? CalculateScore(game.DealerCards) : 0, "Bust! You lose!");
                PlayerData.ClearCard21Game(player.SteamID);
            }
            else
            {
                ChatUtils.SendColoredMessage(player, MessageType.Normal, $"[CH21]!card21 \"Hit\" draw one more card or \"stand\" end it now");
            }
        }

        public static void Stand(CCSPlayerController player, Card21Game game)
        {
            if (game.DealerCards == null) game.DealerCards = new List<string>();
            int playerScore = game.PlayerCards != null ? CalculateScore(game.PlayerCards) : 0;
            while (CalculateScore(game.DealerCards) < 17)
            {
                game.DealerCards.Add(DrawCard());
            }
            int dealerScore = CalculateScore(game.DealerCards);
            
            ChatUtils.SendColoredMessage(player, MessageType.Normal, $"[CH21]Stand! now will calculate the score...");
            string result = dealerScore > 21 ? "Dealer busts! You win!" :
                            playerScore > dealerScore ? "You win!" :
                            dealerScore > playerScore ? "Dealer wins!" : "Push!";
            ChatUtils.ShowCard21Result(player, game, playerScore, dealerScore, result);
            PlayerData.ClearCard21Game(player.SteamID);
        }

        public static HandResult EvaluateHoldemHand(List<string> playerCards, List<string> communityCards)
        {
            if (playerCards == null || communityCards == null || !playerCards.Any() || !communityCards.Any())
            {
                return new HandResult("High Card", new List<string>());
            }

            var allCards = playerCards.Concat(communityCards).ToList();

            // Precompute card values to avoid multiple CardValue calls
            var allCardsWithValues = allCards.Select(c => new { Card = c, Value = CardValue(c) }).ToList();
            var rankGroups = allCardsWithValues
                .GroupBy(c => c.Card.Length >= 2 ? (c.Card.StartsWith("10") ? "10" : c.Card[..^1]) : c.Card)
                .Select(g => new { Rank = g.Key, Cards = g.Select(x => x.Card).ToList(), Count = g.Count(), MaxValue = g.Max(x => CardValue(x.Card)) })
                .OrderByDescending(g => g.Count)
                .ThenByDescending(g => g.MaxValue)
                .ToList();

            var suitGroups = allCardsWithValues.GroupBy(c => c.Card[^1..])
                                               .Select(g => new { Suit = g.Key, Cards = g.Select(x => x.Card).ToList(), Count = g.Count() })
                                               .OrderByDescending(g => g.Count)
                                               .ToList();

            var sortedCards = allCardsWithValues.OrderByDescending(c => c.Value).Select(c => new CardValuePair(c.Card, c.Value)).ToList();

            // Helper function to check if a hand includes at least one player card
            bool IncludesPlayerCard(List<string> hand, List<string> playerCards)
            {
                return hand.Any(card => playerCards.Contains(card));
            }

            var flushSuit = suitGroups.FirstOrDefault(g => g.Count >= 5);
            List<string>? flushCards = null;
            bool isStraightFlush = false;

            if (flushSuit != null)
            {
                flushCards = flushSuit.Cards.OrderByDescending(c => CardValue(c)).Take(5).ToList();
                var flushSortedValues = flushCards.Select(CardValue).OrderByDescending(v => v).ToList();
                for (int i = 0; i <= flushSortedValues.Count - 5; i++)
                {
                    var sequence = flushSortedValues.Skip(i).Take(5).ToList();
                    if (sequence[0] - sequence[4] == 4 || (sequence[0] == 14 && sequence[1] == 5 && sequence[2] == 4 && sequence[3] == 3 && sequence[4] == 2))
                    {
                        isStraightFlush = true;
                        if (sequence[0] == 14 && sequence[1] == 13)
                        {
                            if (IncludesPlayerCard(flushCards, playerCards))
                            {
                                return new HandResult("Royal Flush", flushCards)
                                {
                                    HighCardValue = CardValue(flushCards.First()),
                                    UsesPlayerCard = true
                                };
                            }
                            else
                            {
                                return CreateHighCardResult(playerCards);
                            }
                        }
                        if (IncludesPlayerCard(flushCards, playerCards))
                        {
                            return new HandResult("Straight Flush", flushCards)
                            {
                                HighCardValue = CardValue(flushCards.First()),
                                UsesPlayerCard = true
                            };
                        }
                    }
                }
                if (!isStraightFlush && flushSuit.Cards.Count >= 5 && IncludesPlayerCard(flushCards, playerCards))
                {
                    return new HandResult("Flush", flushCards)
                    {
                        HighCardValue = CardValue(flushCards.First()),
                        UsesPlayerCard = true
                    };
                }
            }

            var straightCards = FindStraight(sortedCards);
            if (straightCards != null && IncludesPlayerCard(straightCards, playerCards))
            {
                return new HandResult("Straight", straightCards)
                {
                    HighCardValue = CardValue(straightCards.First()),
                    UsesPlayerCard = true
                };
            }

            var fourOfAKind = rankGroups.FirstOrDefault(g => g.Count == 4);
            if (fourOfAKind != null)
            {
                var fourCards = fourOfAKind.Cards;
                var kicker = sortedCards.Except(fourCards.Select(c => new CardValuePair(c, CardValue(c)))).OrderByDescending(c => c.Value).FirstOrDefault();
                var relevantCards = fourCards.Concat(new[] { kicker?.Card ?? "N/A" }).Where(c => c != "N/A").ToList();
                if (IncludesPlayerCard(relevantCards, playerCards))
                {
                    var result = new HandResult("Four of a Kind", relevantCards)
                    {
                        ThreeRank = CardValue(fourOfAKind.Cards.First()),
                        HighCardValue = kicker != null ? CardValue(kicker.Card) : 0,
                        UsesPlayerCard = true
                    };
                    return result;
                }
            }

            var threeOfAKind = rankGroups.FirstOrDefault(g => g.Count == 3);
            if (threeOfAKind != null)
            {
                var threeCards = threeOfAKind.Cards;
                var pair = rankGroups.FirstOrDefault(g => g != threeOfAKind && g.Count >= 2);
                if (pair != null)
                {
                    var relevantCards = threeCards.Concat(pair.Cards.Take(2)).ToList();
                    if (IncludesPlayerCard(relevantCards, playerCards))
                    {
                        var result = new HandResult("Full House", relevantCards)
                        {
                            ThreeRank = CardValue(threeOfAKind.Cards.First()),
                            PairRank = CardValue(pair.Cards.First()),
                            UsesPlayerCard = true
                        };
                        return result;
                    }
                }
                if (IncludesPlayerCard(threeCards, playerCards))
                {
                    var kickers = sortedCards.Where(c => !threeCards.Contains(c.Card)).Take(2).Select(c => c.Card).ToList();
                    var relevantCardsThree = threeCards.Concat(kickers).ToList();
                    var resultThree = new HandResult("Three of a Kind", relevantCardsThree)
                    {
                        ThreeRank = CardValue(threeOfAKind.Cards.First()),
                        HighCardValue = kickers.Any() ? CardValue(kickers.First()) : 0,
                        UsesPlayerCard = true
                    };
                    return resultThree;
                }
            }

            var pairs = rankGroups.Where(g => g.Count == 2).OrderByDescending(g => CardValue(g.Rank + "♠")).ToList();
            if (pairs.Count >= 2)
            {
                var twoPairs = pairs.Take(2).SelectMany(g => g.Cards).ToList();
                var kicker = sortedCards.Where(c => !twoPairs.Contains(c.Card)).OrderByDescending(c => c.Value).FirstOrDefault()?.Card ?? "N/A";
                var relevantCards = twoPairs.Concat(new[] { kicker }).Where(c => c != "N/A").ToList();
                if (IncludesPlayerCard(relevantCards, playerCards))
                {
                    var result = new HandResult("Two Pair", relevantCards)
                    {
                        PairRank = CardValue(pairs[0].Cards.First()),
                        SecondPairRank = CardValue(pairs[1].Cards.First()),
                        HighCardValue = kicker != "N/A" ? CardValue(kicker) : 0,
                        UsesPlayerCard = true
                    };
                    return result;
                }
            }
            if (pairs.Count == 1)
            {
                var pairCards = pairs[0].Cards;
                var kickers = sortedCards.Where(c => !pairCards.Contains(c.Card)).Take(3).Select(c => c.Card).ToList();
                var relevantCards = pairCards.Concat(kickers).ToList();
                if (IncludesPlayerCard(pairCards, playerCards))
                {
                    var result = new HandResult("Pair", relevantCards)
                    {
                        PairRank = CardValue(pairs[0].Cards.First()),
                        HighCardValue = kickers.Any() ? CardValue(kickers.First()) : 0,
                        UsesPlayerCard = true
                    };
                    return result;
                }
            }

            var highCardResult = CreateHighCardResult(playerCards);
            return highCardResult;
        }

        private static HandResult CreateHighCardResult(List<string> playerCards)
        {
            var playerHighCard = playerCards.OrderByDescending(c => CardValue(c)).First();
            var playerHighCardKickers = playerCards.Where(c => c != playerHighCard).OrderByDescending(c => CardValue(c)).Take(4).Select(c => c).ToList();
            var highCardRelevantCards = new List<string> { playerHighCard }.Concat(playerHighCardKickers).ToList();
            return new HandResult("High Card", highCardRelevantCards)
            {
                HighCardValue = CardValue(playerHighCard),
                UsesPlayerCard = true
            };
        }

        public static int CompareHands(HandResult hand1, HandResult hand2)
        {
            var handRankings = new Dictionary<string, int>
            {
                { "High Card", 1 },
                { "Pair", 2 },
                { "Two Pair", 3 },
                { "Three of a Kind", 4 },
                { "Straight", 5 },
                { "Flush", 6 },
                { "Full House", 7 },
                { "Four of a Kind", 8 },
                { "Straight Flush", 9 },
                { "Royal Flush", 10 }
            };

            int rank1 = handRankings[hand1.Hand];
            int rank2 = handRankings[hand2.Hand];
            if (rank1 != rank2)
            {
                return rank1 - rank2;
            }

            switch (hand1.Hand)
            {
                case "Full House":
                    if (hand1.ThreeRank != hand2.ThreeRank)
                    {
                        return hand1.ThreeRank - hand2.ThreeRank;
                    }
                    return hand1.PairRank - hand2.PairRank;

                case "Four of a Kind":
                case "Three of a Kind":
                    if (hand1.ThreeRank != hand2.ThreeRank)
                    {
                        return hand1.ThreeRank - hand2.ThreeRank;
                    }
                    return hand1.HighCardValue - hand2.HighCardValue;

                case "Two Pair":
                    if (hand1.PairRank != hand2.PairRank)
                    {
                        return hand1.PairRank - hand2.PairRank;
                    }
                    if (hand1.SecondPairRank != hand2.SecondPairRank)
                    {
                        return hand1.SecondPairRank - hand2.SecondPairRank;
                    }
                    return hand1.HighCardValue - hand2.HighCardValue;

                case "Pair":
                case "High Card":
                    for (int i = 0; i < Math.Min(hand1.RelevantCards.Count, hand2.RelevantCards.Count); i++)
                    {
                        int value1 = CardValue(hand1.RelevantCards[i]);
                        int value2 = CardValue(hand2.RelevantCards[i]);
                        if (value1 != value2)
                        {
                            return value1 - value2;
                        }
                    }
                    return 0;

                case "Straight":
                case "Flush":
                case "Straight Flush":
                case "Royal Flush":
                    int high1 = CardValue(hand1.RelevantCards.First());
                    int high2 = CardValue(hand2.RelevantCards.First());
                    return high1 - high2;
            }

            return 0;
        }

        private static List<string>? FindStraight(List<CardValuePair> sortedCards)
        {
            var values = sortedCards.Select(c => c.Value).Distinct().OrderByDescending(v => v).ToList();
            if (values.Count < 5) return null;

            for (int i = 0; i <= values.Count - 5; i++)
            {
                var sequence = values.Skip(i).Take(5).ToList();
                if (sequence[0] - sequence[4] == 4)
                {
                    var straightCards = new List<string>();
                    foreach (var val in sequence)
                    {
                        var card = sortedCards.FirstOrDefault(c => c.Value == val && !straightCards.Any(sc => CardValue(sc) == val));
                        if (card != null)
                            straightCards.Add(card.Card);
                        else
                            break;
                    }
                    if (straightCards.Count == 5)
                    {
                        return straightCards;
                    }
                }
            }

            if (values.Contains(14) && values.Contains(5) && values.Contains(4) && values.Contains(3) && values.Contains(2))
            {
                var straightValues = new List<int> { 5, 4, 3, 2, 1 };
                var straightCards = sortedCards.Where(c => straightValues.Contains(c.Value == 14 ? 1 : c.Value))
                                               .OrderByDescending(c => c.Value == 14 ? 1 : c.Value)
                                               .Take(5)
                                               .Select(c => c.Card)
                                               .ToList();
                return straightCards;
            }

            return null;
        }
    }

    public class CardValuePair
    {
        public string Card { get; }
        public int Value { get; }

        public CardValuePair(string card, int value)
        {
            Card = card;
            Value = value;
        }
    }

    public class HandResult
    {
        public string Hand { get; }
        public List<string> RelevantCards { get; }
        public int ThreeRank { get; set; }
        public int PairRank { get; set; }
        public int SecondPairRank { get; set; }
        public int HighCardValue { get; set; }
        public bool UsesPlayerCard { get; set; }

        public HandResult(string hand, List<string> relevantCards)
        {
            Hand = hand;
            RelevantCards = relevantCards;
            ThreeRank = 0;
            PairRank = 0;
            SecondPairRank = 0;
            HighCardValue = 0;
            UsesPlayerCard = false;
        }
    }
}
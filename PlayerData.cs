using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Card21Holdem
{
    public class Card21Game
    {
        public List<string>? PlayerCards { get; set; }
        public List<string>? DealerCards { get; set; }
    }

    public static class PlayerData
    {
        private static readonly Dictionary<ulong, Card21Game> card21Games = new Dictionary<ulong, Card21Game>();
        private static readonly Dictionary<ulong, bool> inHoldem = new Dictionary<ulong, bool>();
        private static readonly Dictionary<ulong, List<string>> holdemCards = new Dictionary<ulong, List<string>>();
        private static readonly Dictionary<ulong, bool> holdemChoices = new Dictionary<ulong, bool>();

        public static void SetCard21Game(ulong steamId, Card21Game game)
        {
            card21Games[steamId] = game;
        }

        public static Card21Game? GetCard21Game(ulong steamId)
        {
            card21Games.TryGetValue(steamId, out var game);
            return game;
        }

        public static void ClearCard21Game(ulong steamId)
        {
            card21Games.Remove(steamId);
        }

        public static bool IsInCard21(ulong steamId)
        {
            return card21Games.ContainsKey(steamId);
        }

        public static bool IsInHoldem(ulong steamId)
        {
            return inHoldem.TryGetValue(steamId, out var inGame) && inGame;
        }

        public static void SetInHoldem(ulong steamId, bool inGame)
        {
            inHoldem[steamId] = inGame;
        }

        public static void RemoveFromHoldem(ulong steamId)
        {
            inHoldem.Remove(steamId);
        }

        public static void SetHoldemCards(ulong steamId, List<string> cards)
        {
            holdemCards[steamId] = cards;
        }

        public static List<string>? GetHoldemCards(ulong steamId)
        {
            holdemCards.TryGetValue(steamId, out var cards);
            return cards;
        }

        public static void AddHoldemCard(ulong steamId, string card)
        {
            if (!holdemCards.ContainsKey(steamId))
            {
                holdemCards[steamId] = new List<string>();
            }
            holdemCards[steamId].Add(card);
        }

        public static void SetHoldemChoice(ulong steamId, bool choice)
        {
            holdemChoices[steamId] = choice;
        }

        public static bool HasMadeChoice(ulong steamId)
        {
            return holdemChoices.TryGetValue(steamId, out var choice) && choice;
        }

        public static void ClearPlayer(ulong steamId)
        {
            inHoldem.Remove(steamId);
            holdemCards.Remove(steamId);
            holdemChoices.Remove(steamId);
        }
    }

}
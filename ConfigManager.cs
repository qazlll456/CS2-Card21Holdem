using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Card21Holdem
{
    public static class ConfigManager
    {
        private static bool isEnabled = true;
        private static readonly HashSet<ulong> whitelist = new();
        private static readonly HashSet<ulong> blacklist = new();
        private static string permissionGroup = "all";
        private static int maxPlayers = 4;
        private static bool isPublicMessages = false;
        private static bool isPublicResults = false;
        private static bool isDebugLogging = true;
        private static int maxRounds = 4;
        private static string commandPrefix = "!holdem";
        private static List<string> botNames = new List<string> { "Bot A", "Bot B", "Bot C" };
        private static bool useRandomBotName = true;
        private static int communityCardNumber = 5;
        private static bool enableHoldem = true;
        private static bool enableCard21 = true;
        private static bool enableRoll = true;

        private static readonly HashSet<ulong> admins = new();
        public static HashSet<ulong> Admins => admins;

        public static bool IsGameEnabled => isEnabled;
        public static int MaxPlayers => maxPlayers;
        public static bool IsPublicMessages => isPublicMessages;
        public static bool IsPublicResults => isPublicResults;
        public static bool IsDebugLogging => isDebugLogging;
        public static int MaxRounds => maxRounds;
        public static string CommandPrefix => commandPrefix;
        public static List<string> BotNames => botNames;
        public static bool UseRandomBotName => useRandomBotName;
        public static int CommunityCardNumber => communityCardNumber;
        public static bool EnableHoldem => enableHoldem;
        public static bool EnableCard21 => enableCard21;
        public static bool EnableRoll => enableRoll;

        static ConfigManager()
        {
            ReloadConfig();
        }

        public static bool CanPlayerUse(ulong steamId)
        {
            return permissionGroup switch
            {
                "all" => true,
                "whitelist" => whitelist.Contains(steamId),
                "blacklist" => !blacklist.Contains(steamId),
                _ => true
            };
        }

        public static void ToggleGame(CCSPlayerController? player, bool enable)
        {
            if (player != null && !player.IsValid)
            {
                Server.PrintToConsole("[CH21] ToggleGame failed: Invalid player");
                return;
            }
            isEnabled = enable;
            IEnumerable<CCSPlayerController> recipients = IsPublicMessages ? 
                Utilities.GetPlayers() : (player != null ? new[] { player } : Utilities.GetPlayers());
            ChatUtils.SendColoredMessage(recipients, MessageType.Important, 
                $"Games {(enable ? "enabled" : "disabled")} by {(player != null ? $"{ChatColors.Yellow}{player.PlayerName}{ChatColors.White}" : "server console")}!");
        }

        public static void ReloadConfig(CCSPlayerController? player = null)
        {
            string pluginDir = Path.Combine(Server.GameDirectory, "csgo/addons/counterstrikesharp/configs/plugins/CS2-Card21Holdem");
            string configPath = Path.Combine(pluginDir, "config.json");

            try
            {
                if (!Directory.Exists(pluginDir))
                {
                    Directory.CreateDirectory(pluginDir);
                    Server.PrintToConsole($"[CH21] Created config directory at: {pluginDir}");
                }

                if (!File.Exists(configPath))
                {
                    var defaultConfig = new
                    {
                        Permissions = new
                        {
                            PermissionGroup = "all",
                            PermissionGroupDescription = "Access mode: 'all' (everyone), 'whitelist' (only listed SteamIDs), 'blacklist' (exclude listed SteamIDs)",
                            Whitelist = new ulong[] { 100000000000UL, 100000000000UL },
                            WhitelistDescription = "SteamIDs allowed to use the plugin when PermissionGroup is 'whitelist'",
                            Blacklist = new ulong[] { 100000000000UL, 100000000000UL },
                            BlacklistDescription = "SteamIDs blocked from using the plugin when PermissionGroup is 'blacklist'",
                            Admins = new ulong[] { 100000000000UL },
                            AdminsDescription = "SteamIDs of players with admin permissions for restricted commands (e.g., !ch21 enable, !ch21 reload)"
                        },
                        Gameplay = new
                        {
                            MaxPlayers = 4,
                            MaxPlayersDescription = "Maximum players per game (minimum 2)",
                            MaxRounds = 4,
                            MaxRoundsDescription = "Number of rounds before game ends (1-10)",
                            CommunityCardNumber = 5,
                            CommunityCardNumberDescription = "Number of community cards dealt (1-10)",
                            CommandPrefix = "!holdem",
                            CommandPrefixDescription = "Prefix for all commands (e.g., '!holdem host 1')"
                        },
                        Bots = new
                        {
                            BotNames = new string[] { "Bot A", "Bot B", "Bot C" },
                            BotNamesDescription = "List of bot names for Hold'em games",
                            UseRandomBotName = true,
                            UseRandomBotNameDescription = "Randomly select bot name from BotNames (true) or use first name (false)"
                        },
                        Messaging = new
                        {
                            PublicMessages = false,
                            PublicMessagesDescription = "Show game messages to all players (true) or only participants (false)",
                            PublicResults = false,
                            PublicResultsDescription = "Show game results to all players (true) or only participants (false)",
                            IsDebugLogging = true,
                            IsDebugLoggingDescription = "Enable detailed console logs for debugging (true/false)"
                        },
                        Features = new
                        {
                            EnableHoldem = true,
                            EnableHoldemDescription = "Enable Hold'em game mode (true/false)",
                            EnableCard21 = true,
                            EnableCard21Description = "Enable Card 21 game mode (true/false)",
                            EnableRoll = true,
                            EnableRollDescription = "Enable Roll game mode (true/false)"
                        }
                    };
                    File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }));
                    Server.PrintToConsole($"[CH21] Created default config file at: {configPath}");
                }

                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Config>(json);

                if (config != null)
                {
                    whitelist.Clear();
                    blacklist.Clear();

                    permissionGroup = config.Permissions?.PermissionGroup?.ToLower() ?? "all";
                    if (permissionGroup != "all" && permissionGroup != "whitelist" && permissionGroup != "blacklist")
                    {
                        Server.PrintToConsole($"[CH21] Invalid PermissionGroup value '{permissionGroup}'. Defaulting to 'all'.");
                        permissionGroup = "all";
                    }

                    admins.Clear();
                    foreach (var steamId in config.Permissions?.Admins ?? Array.Empty<ulong>())
                        admins.Add(steamId);

                    foreach (var steamId in config.Permissions?.Whitelist ?? Array.Empty<ulong>())
                        whitelist.Add(steamId);

                    foreach (var steamId in config.Permissions?.Blacklist ?? Array.Empty<ulong>())
                        blacklist.Add(steamId);

                    maxPlayers = Math.Max(2, config.Gameplay?.MaxPlayers ?? 4);

                    maxRounds = config.Gameplay?.MaxRounds ?? 4;
                    if (maxRounds < 1 || maxRounds > 10) maxRounds = 4;

                    communityCardNumber = config.Gameplay?.CommunityCardNumber ?? 5;
                    if (communityCardNumber < 1 || communityCardNumber > 10) communityCardNumber = 5;

                    commandPrefix = config.Gameplay?.CommandPrefix ?? "!holdem";

                    botNames = config.Bots?.BotNames?.ToList() ?? new List<string> { "Bot A", "Bot B", "Bot C" };

                    useRandomBotName = config.Bots?.UseRandomBotName ?? true;

                    isPublicMessages = config.Messaging?.PublicMessages ?? false;

                    isPublicResults = config.Messaging?.PublicResults ?? false;

                    isDebugLogging = config.Messaging?.IsDebugLogging ?? true;

                    enableHoldem = config.Features?.EnableHoldem ?? true;

                    enableCard21 = config.Features?.EnableCard21 ?? true;

                    enableRoll = config.Features?.EnableRoll ?? true;
                }
                else
                {
                    Server.PrintToConsole("[CH21] Failed to parse config file. Using default settings.");
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"[CH21] Error reloading config: {ex.Message}");
                if (player != null && player.IsValid)
                    ChatUtils.SendColoredMessage(player, MessageType.Warning, $"Error reloading config: {ex.Message}");
            }
        }

        public static bool IsAdmin(ulong steamId)
        {
            return Admins.Contains(steamId);
        }

        private class Config
        {
            public PermissionSettings? Permissions { get; set; }
            public GameplaySettings? Gameplay { get; set; }
            public BotSettings? Bots { get; set; }
            public MessagingSettings? Messaging { get; set; }
            public FeatureSettings? Features { get; set; }
        }

        private class PermissionSettings
        {
            public string PermissionGroup { get; set; } = "all";
            public string? PermissionGroupDescription { get; set; } = "Access mode: 'all' (everyone), 'whitelist' (only listed SteamIDs), 'blacklist' (exclude listed SteamIDs)";
            public ulong[] Whitelist { get; set; } = Array.Empty<ulong>();
            public string? WhitelistDescription { get; set; } = "SteamIDs allowed to use the plugin when PermissionGroup is 'whitelist'";
            public ulong[] Blacklist { get; set; } = Array.Empty<ulong>();
            public string? BlacklistDescription { get; set; } = "SteamIDs blocked from using the plugin when PermissionGroup is 'blacklist'";
            public ulong[] Admins { get; set; } = Array.Empty<ulong>();
            public string? AdminsDescription { get; set; } = "SteamIDs of players with admin permissions for restricted commands (e.g., !game enable, !ch21 reload)";
        }

        private class GameplaySettings
        {
            public int? MaxPlayers { get; set; }
            public string? MaxPlayersDescription { get; set; } = "Maximum players per game (minimum 2)";
            public int? MaxRounds { get; set; }
            public string? MaxRoundsDescription { get; set; } = "Number of rounds before game ends (1-10)";
            public int? CommunityCardNumber { get; set; }
            public string? CommunityCardNumberDescription { get; set; } = "Number of community cards dealt (1-10)";
            public string CommandPrefix { get; set; } = "!holdem";
            public string? CommandPrefixDescription { get; set; } = "Prefix for all commands (e.g., '!holdem host 1')";
        }

        private class BotSettings
        {
            public string[] BotNames { get; set; } = new[] { "Bot A", "Bot B", "Bot C" };
            public string? BotNamesDescription { get; set; } = "List of bot names for Hold'em games";
            public bool? UseRandomBotName { get; set; }
            public string? UseRandomBotNameDescription { get; set; } = "Randomly select bot name from BotNames (true) or use first name (false)";
        }

        private class MessagingSettings
        {
            public bool? PublicMessages { get; set; }
            public string? PublicMessagesDescription { get; set; } = "Show game messages to all players (true) or only participants (false)";
            public bool? PublicResults { get; set; }
            public string? PublicResultsDescription { get; set; } = "Show game results to all players (true) or only participants (false)";
            public bool? IsDebugLogging { get; set; }
            public string? IsDebugLoggingDescription { get; set; } = "Enable detailed console logs for debugging (true/false)";
        }

        private class FeatureSettings
        {
            public bool? EnableHoldem { get; set; }
            public string? EnableHoldemDescription { get; set; } = "Enable Hold'em game mode (true/false)";
            public bool? EnableCard21 { get; set; }
            public string? EnableCard21Description { get; set; } = "Enable Card 21 game mode (true/false)";
            public bool? EnableRoll { get; set; }
            public string? EnableRollDescription { get; set; } = "Enable Roll game mode (true/false)";
        }
    }
}
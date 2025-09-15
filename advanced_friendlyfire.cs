using System;
using System.IO;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using System.Reflection;

namespace AdvancedFriendlyFire
{ 
    public class AdvancedFriendlyFireConfig : BasePluginConfig
    {
        [JsonPropertyName("Enable/Disable Advanced Friendly Fire")]
        public bool IsAdvancedFriendlyFireEnabled { get; set; } = true;

        [JsonPropertyName("Enable/Disable Punishments")]
        public bool ArePunishmentsEnabled { get; set; } = true;

        [JsonPropertyName("Damage Inflictors")]
        public string[] DamageInflictors { get; set; } =
        {
            "inferno",
            "hegrenade_projectile",
            "flashbang_projectile",
            "smokegrenade_projectile",
            "decoy_projectile",
            "planted_c4"
        };

        [JsonPropertyName("Warning #1 Required Team Damage (HP Metrics)")]
        public int Warn1 { get; set; } = 100;

        [JsonPropertyName("Warning #1 Chat message")]
        public string chatWarn1 { get; set; } = "Avoid friendly fire, or you will be punished! Friendly fire warning [1/3]";

        [JsonPropertyName("Warning #1 Punishment")]
        public string punishWarn1 { get; set; } = "css_slay {Player} \"Friendly fire warning [1/3]\"";

        [JsonPropertyName("Warning #2 Required Team Damage (HP Metrics)")]
        public int Warn2 { get; set; } = 200;

        [JsonPropertyName("Warning #2 Chat message")]
        public string chatWarn2 { get; set; } = "You have been kicked for dealing excessive damage to your teammates!";

        [JsonPropertyName("Warning #2 Punishment")]
        public string punishWarn2 { get; set; } = "css_kick {Player} \"Friendly fire warning [2/3]\"";

        [JsonPropertyName("Warning #3 Required Team Damage (HP Metrics)")]
        public int Warn3 { get; set; } = 300;

        [JsonPropertyName("Warning #3 Chat message")]
        public string chatWarn3 { get; set; } = "You have been banned for dealing excessive damage to your teammates!";

        [JsonPropertyName("Warning #3 Punishment")]
        public string punishWarn3 { get; set; } = "css_ban {Player} 30 \"Friendly fire warning [3/3]\"";
    }

    public class AdvancedFriendlyFire : BasePlugin, IPluginConfig<AdvancedFriendlyFireConfig>
    {
        public override string ModuleName => "Advanced Friendly Fire [Extracted from Argentum Suite]";
        public override string ModuleVersion => "1.1.4";
        public override string ModuleAuthor => "phara1";
        public override string ModuleDescription => "https://steamcommunity.com/id/kenoxyd";

        public required AdvancedFriendlyFireConfig Config { get; set; }
        public void OnConfigParsed(AdvancedFriendlyFireConfig config)
        {
            Config = config;
        }

        public override void Load(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnAdvancedFriendlyFireHook, HookMode.Pre);

            RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);

            if (Config.IsAdvancedFriendlyFireEnabled)
            {
                Server.ExecuteCommand("mp_friendlyfire 1");
                Server.ExecuteCommand("ff_damage_reduction_bullets 0.33");
                Server.ExecuteCommand("ff_damage_reduction_grenade 0.85");
                Server.ExecuteCommand("ff_damage_reduction_grenade_self 1");
                Server.ExecuteCommand("ff_damage_reduction_other 0.4");
            }

        }

        public override void Unload(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnAdvancedFriendlyFireHook, HookMode.Pre);
        }

        private Dictionary<ulong, (float damage, string attackerName)> tempDamageTracker = new Dictionary<ulong, (float, string)>();
        private Dictionary<ulong, float> teamDamageTracker = new Dictionary<ulong, float>();
        private Dictionary<ulong, int> punishmentLevelTracker = new Dictionary<ulong, int>();

        private HookResult OnPlayerHurt(EventPlayerHurt eventInfo, GameEventInfo info)
        {
            if (eventInfo == null)
            {
                Console.WriteLine("eventInfo is null, skipping...");
                return HookResult.Continue;
            }

            var attacker = eventInfo?.Attacker;
            var victim = eventInfo?.Userid;

            if (attacker == null && victim == null)
            {
                return HookResult.Continue;
            }

            if (attacker != null)
            {
                ulong attackerSteamId = attacker.SteamID;

                if (attacker != victim)
                {
                    var damageTaken = eventInfo.DmgHealth;

                    string attackerName = attacker.PlayerName;  // Safe to access after null check

                    tempDamageTracker[attackerSteamId] = (damageTaken, attackerName);
                }
            }


            return HookResult.Continue;
        }


        private HookResult OnAdvancedFriendlyFireHook(DynamicHook hook)
        {
            if (!Config.IsAdvancedFriendlyFireEnabled) return HookResult.Continue;

            var victim = hook.GetParam<CEntityInstance>(0);
            var idmg = hook.GetParam<CTakeDamageInfo>(1);

            

            if (victim.DesignerName != "player") return HookResult.Continue;

            var attacker = new CCSPlayerPawn(idmg.Attacker.Value.Handle);
            var victimPlayer = new CCSPlayerController(victim.Handle);
            var attackerController = new CCSPlayerController(idmg.Attacker.Value.Handle);

            if (attacker.TeamNum != victimPlayer.TeamNum || attacker == victimPlayer) return HookResult.Continue;

            string inflictor = idmg.Inflictor.Value?.DesignerName ?? "";

            if (!Config.DamageInflictors.Contains(inflictor))
            {
                return HookResult.Continue;
            }

            attackerController.PrintToCenterAlert("DON'T HURT YOUR TEAMMATES!");

            if (Config.ArePunishmentsEnabled)
            {
                ulong attackerSteamId = attacker.Controller.Value.SteamID;

                if (!tempDamageTracker.TryGetValue(attackerSteamId, out var attackerInfo))
                {
                    attackerInfo = (0, "Unknown");
                }
                float damageAmount = attackerInfo.damage;
                string attackerName = attackerInfo.attackerName;

                if (!teamDamageTracker.TryGetValue(attackerSteamId, out float totalDamage))
                    totalDamage = 0;

                totalDamage += damageAmount;
                teamDamageTracker[attackerSteamId] = totalDamage;

                //Server.PrintToChatAll($"Total damage: {teamDamageTracker[attackerSteamId]}");

                if (!punishmentLevelTracker.TryGetValue(attackerSteamId, out int punishmentLevel))
                    punishmentLevel = 0;

                if (totalDamage >= Config.Warn1 && punishmentLevel < 1)
                {
                    attackerController.PrintToChat($"{Config.chatWarn1}");

                    string commandToExecute = Config.punishWarn1;

                    if (commandToExecute.Contains("{Player}"))
                        Server.ExecuteCommand(commandToExecute.Replace("{Player}", attackerName));

                    punishmentLevelTracker[attackerSteamId] = 1;
                }
                else if (totalDamage >= Config.Warn2 && punishmentLevel < 2)
                {
                    attackerController.PrintToChat($"{Config.chatWarn2}");

                    string commandToExecute = Config.punishWarn2;

                    if (commandToExecute.Contains("{Player}"))
                        Server.ExecuteCommand(commandToExecute.Replace("{Player}", attackerName));

                    punishmentLevelTracker[attackerSteamId] = 2;
                }
                else if (totalDamage >= Config.Warn3 && punishmentLevel < 3)
                {
                    attackerController.PrintToChat($"{Config.chatWarn3}");

                    string commandToExecute = Config.punishWarn3;

                    if (commandToExecute.Contains("{Player}"))
                        Server.ExecuteCommand(commandToExecute.Replace("{Player}", attackerName));

                    teamDamageTracker[attackerSteamId] = 0;
                    punishmentLevelTracker[attackerSteamId] = 0;
                }
            }

            return HookResult.Continue;
        }

    }
}

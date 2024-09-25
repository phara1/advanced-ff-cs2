using System;
using System.IO;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Memory;

namespace AdvancedFriendlyFire
{
    public class AdvancedFriendlyFireConfig : BasePluginConfig
    {
        [JsonPropertyName("Enable/Disable Advanced Friendly Fire")]
        public bool IsAdvancedFriendlyFireEnabled { get; set; } = true;

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
    }

    // Main plugin class
    public class AdvancedFriendlyFire : BasePlugin, IPluginConfig<AdvancedFriendlyFireConfig>
    {
        public override string ModuleName => "Advanced Friendly Fire [Extracted from Argentum Suite]";
        public override string ModuleVersion => "1.0";
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
        }
        private HookResult OnAdvancedFriendlyFireHook(DynamicHook hook)
        {

            if (!Config.IsAdvancedFriendlyFireEnabled)
            {
                return HookResult.Continue;
            }

            var victim = hook.GetParam<CEntityInstance>(0);
            var idmg = hook.GetParam<CTakeDamageInfo>(1);

            var attacker = new CCSPlayerPawn(idmg.Attacker.Value.Handle);
            var victimPlayer = new CCSPlayerController(victim.Handle);
            var attackerController = new CCSPlayerController(idmg.Attacker.Value.Handle);

            string inflictor = idmg.Inflictor.Value.DesignerName ?? "";

            if (attacker.TeamNum == victimPlayer.TeamNum && "player".Equals(victim.DesignerName) && attacker != victimPlayer)
            {
                if (Config.DamageInflictors.Contains(inflictor))
                {
                    attackerController.PrintToCenterAlert("DON'T HURT YOUR TEAMMATES!");
                    return HookResult.Continue;
                }

                return HookResult.Handled; // Prevent damage if the inflictor is not allowed
            }

            return HookResult.Continue; // Allow the damage
        }
    }
}

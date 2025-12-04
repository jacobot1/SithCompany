using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SithCompany.Patches;
using SithCompany.Abilities;
using SithCompany.Util;
using UnityEngine;

namespace SithCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class SithCompanyMod : BaseUnityPlugin
    {
        // Mod metadata
        public const string modGUID = "com.jacobot5.SithCompany";
        public const string modName = "SithCompany";
        public const string modVersion = "1.0.2";

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Configuration
        public static ConfigEntry<float> configLightningVolume;
        public static ConfigEntry<float> configLightningDamageRadius;
        public static ConfigEntry<float> configLightningLength;
        public static ConfigEntry<bool> configKillEnemies;
        public static ConfigEntry<bool> configKillPlayers;
        public static ConfigEntry<float> configForceRadius;

        // Create static instance
        public static SithCompanyMod Instance;

        // Create static instance of input class
        internal static SithInput SithInputInstance;

        // Initialize logging
        public static ManualLogSource mls;

        // Config change handler
        private void OnSettingChanged(object Sender, System.EventArgs e)
        {
            ForceAbility.indicator.transform.localScale = SithCompanyMod.configForceRadius.Value * 2f * Vector3.one;
        }

        private void Awake()
        {
            // Ensure static instance
            if (Instance == null)
            {
                Instance = this;
            }
            
            SithInputInstance = new SithInput();
            SithInputInstance.Enable();

            // Send alive message
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("SithCompany has awoken.");

            // Bind configuration
            configLightningVolume = Config.Bind("Lightning.Thunder",
                                                "Volume",
                                                1f,
                                                "How loud thunder should be. Default is 1; ranges from 0-1");

            configLightningDamageRadius = Config.Bind("Lightning.Damage",
                                                "Radius",
                                                3f,
                                                "How close players/enemies must be to the end of a lighting bolt to be killed by it");

            configLightningLength = Config.Bind("Lightning.Length",
                                                "Length",
                                                10f,
                                                "How long lightning bolt should be");

            configKillPlayers = Config.Bind("Lightning.Damage",
                                                "Kill Players",
                                                true,
                                                "Whether to kill players on lightning strike");

            configKillEnemies = Config.Bind("Lightning.Damage",
                                                "Kill Enemies",
                                                true,
                                                "Whether to kill enemies on lightning strike");

            configForceRadius = Config.Bind("Force",
                                                "Force Radius",
                                                2f,
                                                "How large the influence of the Force should be");

            // Subscribe to config changes
            configForceRadius.SettingChanged += OnSettingChanged;

            // Do the patching
            harmony.PatchAll(typeof(SithCompanyMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
        }
    }
}

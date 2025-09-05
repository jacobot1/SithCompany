using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SithCompany.Patches;
using UnityEngine;

namespace SithCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SithCompanyMod : BaseUnityPlugin
    {
        // Mod metadata
        public const string modGUID = "com.jacobot5.SithCompany";
        public const string modName = "SithCompany";
        public const string modVersion = "1.0.0";

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Configuration
        public static ConfigEntry<float> configLightningVolume;
        public static ConfigEntry<float> configLightningDamageRadius;
        public static ConfigEntry<float> configLightningLength;

        // Create static instance
        public static SithCompanyMod Instance;

        // Initialize logging
        public static ManualLogSource mls;

        private void Awake()
        {
            // Ensure static instance
            if (Instance == null)
            {
                Instance = this;
            }

            // Send alive message
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("SithCompany has awoken.");

            // Bind configuration
            configLightningVolume = Config.Bind("Lightning.Thunder",
                                                "Volume",
                                                1f,
                                                "How loud thunder should be. Default is 1; ranges from 0-1");
            // Bind configuration
            configLightningDamageRadius = Config.Bind("Lightning.Damage",
                                                "Radius",
                                                3f,
                                                "How close players/enemies must be to a lighting bolt to be killed by it");
            // Bind configuration
            configLightningLength = Config.Bind("Lightning.Length",
                                                "Length",
                                                10f,
                                                "How long lightning bolt should be");

            // Do the patching
            harmony.PatchAll(typeof(SithCompanyMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
        }
    }
}

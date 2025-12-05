using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SithCompany.Abilities;
using SithCompany.Patches;
using SithCompany.Util;
using System.IO;
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
        public static ConfigEntry<float> configLightningMinimumStamina;
        public static ConfigEntry<float> configLightningStaminaDock;
        public static ConfigEntry<float> configLightningLength;
        public static ConfigEntry<bool> configKillEnemies;
        public static ConfigEntry<bool> configKillPlayers;
        public static ConfigEntry<float> configForceRadius;
        public static ConfigEntry<float> configEnemyFallGravity;
        public static ConfigEntry<float> configDefaultForceIndicatorDistance;
        public static ConfigEntry<float> configForceRange;
        public static ConfigEntry<float> configForceStaminaMultiplier;
        public static ConfigEntry<float> configForceMinimumStamina;

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

            configLightningMinimumStamina = Config.Bind("Lightning.General",
                                                "Force Lightning Minimum Stamina",
                                                0.6f,
                                                new ConfigDescription(
                                                    "Minimum Stamina required to use Force Lightning",
                                                    new AcceptableValueRange<float>(0f, 1f)
                                                )
                                            );

            configLightningStaminaDock = Config.Bind("Lightning.General",
                                                "Force Lightning Stamina Dock",
                                                0.6f,
                                                new ConfigDescription(
                                                    "Stamina cost of using Force Lightning",
                                                    new AcceptableValueRange<float>(0f, 1f)
                                                )
                                            );

            configLightningLength = Config.Bind("Lightning.General",
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

            configForceRadius = Config.Bind("Force.Influence",
                                                "Force Radius",
                                                2f,
                                                "How large the influence of the Force should be");

            configForceMinimumStamina = Config.Bind("Force.General",
                                                "Force Minimum Stamina",
                                                0f,
                                                new ConfigDescription(
                                                    "Minimum Stamina required to use the Force",
                                                    new AcceptableValueRange<float>(0f, 1f)
                                                )
                                            );

            configForceStaminaMultiplier = Config.Bind("Force.General",
                                                "Force Stamina Use Multiplier",
                                                1f,
                                                "Adjusts speed of stamina use while using the Force");

            configEnemyFallGravity = Config.Bind("Force.Enemies",
                                                "Enemy Fall Gravity",
                                                15f,
                                                "How much gravity falling enemies should experience");

            configDefaultForceIndicatorDistance = Config.Bind("Force.Influence",
                                                "Default Force Indicator Distance",
                                                5f,
                                                "How far the Force Indicator should be from the player upon enabling Force Mode");

            configForceRange = Config.Bind("Force.Influence",
                                                "Maximum Force Influence Range",
                                                50f,
                                                "Maximum distance at which the Force can be used.");

            // Load indicator AssetBundle
            var bundlePath = Path.Combine(Paths.PluginPath, "SeismicMods-SithCompany/bubblebundle");
            var bundle = AssetBundle.LoadFromFile(bundlePath);

            // Find the shader compiled from file above
            ForceAbility.bubbleShader = bundle.LoadAsset<Shader>("ThinBubbleUnlit");

            // Subscribe to config changes
            configForceRadius.SettingChanged += OnSettingChanged;

            // Do the patching
            harmony.PatchAll(typeof(SithCompanyMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
            harmony.PatchAll(typeof(QuickMenuManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
        }
    }
}

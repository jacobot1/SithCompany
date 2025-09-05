using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace SithCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("PerformEmote")]
        [HarmonyPostfix]
        static void LightningStrikePatch(PlayerControllerB __instance, int emoteID)
        {
            if (emoteID != 2)
            {
                return; // Not the lightning emote
            }
            // Set up AudioSource
            AudioSource thunderOrigin;
            GameObject audioObject = new GameObject("SithCompany_ThunderAudioSource");
            audioObject.SetActive(true);
            UnityEngine.Object.DontDestroyOnLoad(audioObject); // persist across scenes
            thunderOrigin = audioObject.AddComponent<AudioSource>();
            thunderOrigin.spatialBlend = 0f; // 2D sound
            thunderOrigin.minDistance = 5f;
            thunderOrigin.maxDistance = 200f;
            thunderOrigin.rolloffMode = AudioRolloffMode.Logarithmic;
            thunderOrigin.playOnAwake = false;
            thunderOrigin.loop = false;
            thunderOrigin.enabled = true;

            // Simplifications
            Vector3 playerPosition = __instance.transform.position;
            Vector3 strikeOrigin = playerPosition + __instance.transform.forward * 1f + Vector3.up * 2f;
            Vector3 strikePosition = __instance.gameplayCamera.transform.position + __instance.gameplayCamera.transform.forward * SithCompanyMod.configLightningLength.Value;

            // Damage Everything
            EZDamage.API.KillEverything(strikePosition, SithCompanyMod.configLightningDamageRadius.Value, CauseOfDeath.Electrocution);

            // Fire lightning bolt
            EZLightning.API.Strike(strikePosition , strikeOrigin, 1f, 0.5f, 0.5f, 0, -1f, minCount: 0, maxCount: 1);
        }
    }
}

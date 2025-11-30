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
            if ((emoteID != 2) || (__instance.sprintMeter < 0.6f) || (__instance.isTypingChat) || (__instance.inTerminalMenu))
            {
                return;
            }

            // Simplifications
            Vector3 playerPosition = __instance.transform.position;
            Vector3 strikeOrigin = playerPosition + __instance.transform.forward * 1f + Vector3.up * 2f;
            Vector3 strikePosition = __instance.gameplayCamera.transform.position + __instance.gameplayCamera.transform.forward * SithCompanyMod.configLightningLength.Value;

            // Damage Selector
            if (SithCompanyMod.configKillEnemies.Value)
            {
                EZDamage.API.KillEnemies(strikePosition, SithCompanyMod.configLightningDamageRadius.Value);
            }
            if (SithCompanyMod.configKillPlayers.Value)
            {
                EZDamage.API.KillPlayers(strikePosition, SithCompanyMod.configLightningDamageRadius.Value, CauseOfDeath.Electrocution);
            }

            // Fire lightning bolt
            EZLightning.API.Strike(strikePosition , strikeOrigin, 1f, 0.5f, 0.5f, 0, -1f, minCount: 0, maxCount: 1);

            // Dock sprint
            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - 0.6f, 0f, 1f);
        }
    }
}

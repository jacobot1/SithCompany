using GameNetcodeStuff;
using HarmonyLib;
using SithCompany.Abilities;
using UnityEngine;
using SithCompany.Util;

namespace SithCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void HookIntoPlayerControllerPatch(PlayerControllerB __instance)
        {
            if ((!__instance.isTypingChat) && (!__instance.inTerminalMenu))
            {
                if ((SithCompanyMod.SithInputInstance.LightningButton.WasPressedThisFrame()) && (__instance.sprintMeter >= 0.6f))
                {
                    LightningAbility.ForceLightning(__instance);
                }
                if (SithCompanyMod.SithInputInstance.ForceModeButton.WasPressedThisFrame())
                {
                    ForceAbility.ToggleForceMode(__instance);
                }
                if (ForceAbility.forceModeEnabled)
                {
                    ForceAbility.indicator.transform.position = __instance.gameplayCamera.transform.position + ForceAbility.indicatorDistance * __instance.gameplayCamera.transform.forward;
                    if (SithCompanyMod.SithInputInstance.UseTheForceButton.IsPressed())
                    {
                        ForceAbility.UseTheForce(__instance);
                    }
                }
            }
        }
    }
}
using GameNetcodeStuff;
using HarmonyLib;
using SithCompany.Abilities;
using SithCompany.Util;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SithCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void HookIntoPlayerControllerPatch(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || !__instance.isPlayerControlled) return;
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
                    else if (SithCompanyMod.SithInputInstance.UseTheForceButton.WasReleasedThisFrame())
                    {
                        ForceAbility.ReleaseTheForce();
                    }
                }
            }
        }
        [HarmonyPatch("ScrollMouse_performed")]
        [HarmonyPrefix]
        static bool ForceIndicatorScrollPatch(PlayerControllerB __instance, ref InputAction.CallbackContext context)
        {
            if (ForceAbility.forceModeEnabled)
            {
                float amount = context.ReadValue<float>();
                if (amount > 0f)
                {
                    ForceAbility.indicatorDistance += 0.2f;
                }
                else
                {
                    ForceAbility.indicatorDistance -= 0.2f;
                }
                return false;
            }
            return true;
        }
    }
}
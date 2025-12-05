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
                        ForceAbility.gotForcablesAlready = false;

                        // Define a guaranteed safe drop location near the player's feet.
                        Vector3 indicatorDropWorldPos = ForceAbility.indicator.transform.position;
                        indicatorDropWorldPos.y += 0.1f;
                        
                        foreach (var grabObject in ForceAbility.grabbableHits)
                        {
                            // 1. Clear internal state flags FIRST
                            grabObject.Key.parentObject = null;
                            grabObject.Key.isHeld = false;

                            // 2. Release network ownership (Crucial for local physics authority)
                            if (grabObject.Key.IsOwner)
                            {
                                grabObject.Key.gameObject.GetComponent<NetworkObject>().RemoveOwnership();
                            }

                            // 3. Clear the Unity Transform parent and maintain the current world position
                            grabObject.Key.transform.SetParent(null, true);

                            // 4. Force teleport the world position to the indicator
                            grabObject.Key.transform.position = indicatorDropWorldPos;

                            // 5. Manually set the field FallToGround relies on, using the new world position as the local position.
                            grabObject.Key.startFallingPosition = grabObject.Key.transform.localPosition;


                            // 6. Temporarily disable colliders for safe raycasting
                            if (grabObject.Key.propBody != null)
                            {
                                grabObject.Key.propBody.isKinematic = false;
                                grabObject.Key.propBody.velocity = Vector3.zero;
                                grabObject.Key.propBody.angularVelocity = Vector3.zero;
                            }
                            grabObject.Key.EnablePhysics(false);

                            // 7. Trigger the FallToGround sequence. This should now succeed.
                            // The function will use startFallingPosition set in step 5.
                            grabObject.Key.FallToGround(randomizePosition: false, justSpawned: false);

                            // 8. Re-enable colliders/physics
                            grabObject.Key.EnablePhysics(true);

                            SithCompanyMod.mls.LogInfo($"Successfully released and dropped object: {grabObject.Key.name}");
                        }
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
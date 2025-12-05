using GameNetcodeStuff;
using HarmonyLib;
using SithCompany.Abilities;
using SithCompany.Util;
using System.Collections.Generic;
using System.Linq;
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
                // --- START OF MANUAL FALLING ENEMY LOGIC ---
                // We use ToList() for safe iteration while modifying the dictionary.
                foreach (var pair in ForceAbility.fallingEnemies.ToList())
                {
                    EnemyAI enemy = pair.Key;
                    Vector3 currentVelocity = pair.Value;
                    float deltaTime = Time.deltaTime;

                    // 1. Apply Gravity (Acceleration)
                    currentVelocity.y += ForceAbility.Gravity * deltaTime;

                    // 2. Calculate new position
                    Vector3 nextPosition = enemy.transform.position + currentVelocity * deltaTime;

                    // 3. Raycast Check for Ground (CRITICAL)
                    // Use a comprehensive layer mask (1<<0: Default/World, 1<<3: Terrain, etc.). 
                    // Adjusting the distance to be proportional to the movement distance prevents tunneling.
                    float checkDistance = Mathf.Abs(currentVelocity.y * deltaTime) + 0.1f;
                    int layerMask = 1 << 0 | 1 << 3 | 1 << 8 | 1 << 19; // Combining several common environmental layers

                    RaycastHit hit;
                    if (Physics.Raycast(enemy.transform.position, Vector3.down, out hit, checkDistance, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        // Ground Hit - End the fall

                        // Snap to the hit position (ensures they are exactly on the floor)
                        enemy.transform.position = hit.point;

                        // CRITICAL: Re-enable NavMeshAgent/AI
                        enemy.SetClientCalculatingAI(true);

                        // Remove from falling list
                        ForceAbility.fallingEnemies.Remove(enemy);
                        SithCompanyMod.mls.LogInfo($"Enemy {enemy.name} hit the ground and AI resumed.");
                    }
                    else
                    {
                        // Still Falling - Update position and velocity
                        enemy.transform.position = nextPosition;
                        ForceAbility.fallingEnemies[enemy] = currentVelocity;
                    }
                }
                // --- END OF MANUAL FALLING ENEMY LOGIC ---
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
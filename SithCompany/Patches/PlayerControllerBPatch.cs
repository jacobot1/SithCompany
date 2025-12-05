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
                if (SithCompanyMod.SithInputInstance.LightningButton.WasPressedThisFrame())
                {
                    LightningAbility.ForceLightning(__instance);
                }
                if (SithCompanyMod.SithInputInstance.ForceModeButton.WasPressedThisFrame())
                {
                    ForceAbility.ToggleForceMode(__instance);
                }
                if (ForceAbility.forceModeEnabled)
                {
                    const float INTERPOLATION_SPEED = 10f; // Adjust this value (e.g., 5f to 20f) to control sliding speed

                    ForceAbility.indicatorDistance = Mathf.Lerp(
                        ForceAbility.indicatorDistance,
                        ForceAbility.targetIndicatorDistance,
                        Time.deltaTime * INTERPOLATION_SPEED
                    );

                    // Use the smoothly updated indicatorDistance for the position
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
                    currentVelocity.y -= SithCompanyMod.configEnemyFallGravity.Value * deltaTime;

                    // 2. Calculate new position
                    Vector3 nextPosition = enemy.transform.position + currentVelocity * deltaTime;

                    // 3. Raycast Check for Ground (CRITICAL)
                    float checkDistance = Mathf.Abs(currentVelocity.y * deltaTime) + 0.1f;
                    int layerMask = 1 << 0 | 1 << 3 | 1 << 8 | 1 << 19; // Combining several common environmental layers

                    RaycastHit hit;
                    if (Physics.Raycast(enemy.transform.position, Vector3.down, out hit, checkDistance, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        // Ground Hit - End the fall

                        // CRITICAL FIX A: Calculate the vertical offset to land on feet, not the pivot.
                        CapsuleCollider capsule = enemy.GetComponent<CapsuleCollider>();
                        float verticalOffset = 0.1f;
                        if (capsule != null)
                        {
                            // Offset is calculated from the pivot (transform.position) to the bottom of the capsule.
                            verticalOffset = capsule.height / 2f + capsule.center.y + 0.1f;
                        }

                        // Snap to the corrected hit position (hit.point is the floor).
                        enemy.transform.position = hit.point + Vector3.up * verticalOffset;

                        // CRITICAL FIX B: Set special animation to false to allow AI Update to resume logic.
                        enemy.inSpecialAnimation = false;

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
                const float TARGET_STEP = 0.2f; // This is the amount the *target* snaps per scroll tick
                const float MIN_DISTANCE = 1f;

                float amount = context.ReadValue<float>();

                if (amount > 0f)
                {
                    ForceAbility.targetIndicatorDistance += TARGET_STEP;
                }
                else if (amount < 0f) // Use else if for explicit check
                {
                    ForceAbility.targetIndicatorDistance -= TARGET_STEP;
                }

                // Clamp the target distance to prevent it from going out of bounds
                ForceAbility.targetIndicatorDistance = Mathf.Clamp(ForceAbility.targetIndicatorDistance, MIN_DISTANCE, SithCompanyMod.configForceRange.Value);
                return false;
            }
            return true;
        }
    }
}
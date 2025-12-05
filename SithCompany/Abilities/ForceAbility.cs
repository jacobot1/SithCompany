using BepInEx;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace SithCompany.Abilities
{
    internal class ForceAbility
    {
        public static bool forceModeEnabled = false;
        public static float indicatorDistance = 5f;
        public static bool gotForcablesAlready = false;
        public static GameObject indicator;
        public static Shader bubbleShader;

        public static float Gravity = -12f; // Increased gravity for a noticeable fall
        public static float InitialFallImpulse = 10f; // Initial downward speed when released
        public static Dictionary<EnemyAI, Vector3> fallingEnemies = new Dictionary<EnemyAI, Vector3>(); // New state tracker

        public static Dictionary<GrabbableObject, Transform> grabbableHits = new Dictionary<GrabbableObject, Transform>();
        public static Dictionary<PlayerControllerB, Vector3> playerHits = new Dictionary<PlayerControllerB, Vector3>();
        public static Dictionary<EnemyAI, Vector3> enemyHits = new Dictionary<EnemyAI, Vector3>();

        public static void CreateIndicator(float radius)
        {
            // Create sphere
            indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "ThinBubbleIndicator";
            UnityEngine.Object.Destroy(indicator.GetComponent<Collider>());
            indicator.transform.localScale = Vector3.one * radius * 2f;

            if (bubbleShader == null)
            {
                Debug.LogError("[BubbleIndicator] Shader 'Custom/ThinBubbleUnlit' not found. Make sure the .shader file is included in the build.");
                return;
            }

            Material mat = new Material(bubbleShader);
            // Set default colors (you can tweak these)
            mat.SetColor("_BaseColor", new Color(1f, 0f, 0f, 0.06f));   // very transparent red
            mat.SetColor("_RimColor", new Color(1f, 0.35f, 0.35f, 1f));  // soft rim
            mat.SetFloat("_RimPower", 3f);
            mat.SetFloat("_RimIntensity", 0.9f);

            // Make sure render queue is transparent
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Apply material to renderer
            var rend = indicator.GetComponent<Renderer>();
            rend.material = mat;

            // Start disabled by default
            indicator.SetActive(false);
        }
        public static void CleanupIndicator()
        {
            if (indicator != null)
            {
                // Explicitly destroy the Unity GameObject
                UnityEngine.Object.Destroy(indicator);
                // Set the static reference to null so it can be re-created later
                indicator = null;
                // Also reset the mode state
                forceModeEnabled = false;

                SithCompanyMod.mls.LogInfo("Force Indicator cleaned up on round end.");
            }
        }
        public static void ToggleForceMode(PlayerControllerB player)
        {
            if (forceModeEnabled)
            {
                // Disable force mode
                forceModeEnabled = false;
                SithCompanyMod.mls.LogInfo("Force Mode Disabled");
                // Hide indicator
                indicator.SetActive(false);
            }
            else
            {
                // Enable force mode
                forceModeEnabled = true;
                SithCompanyMod.mls.LogInfo("Force Mode Enabled");
                // Show indicator
                if (indicator == null)
                {
                    ForceAbility.CreateIndicator(SithCompanyMod.configForceRadius.Value);
                }
                indicator.SetActive(true);
            }
        }
        public static void UseTheForce(PlayerControllerB player)
        {
            if (!gotForcablesAlready)
            {
                playerHits.Clear();
                enemyHits.Clear();
                grabbableHits.Clear();
                FindTargetsInSphere(indicator.transform.position, SithCompanyMod.configForceRadius.Value);
                gotForcablesAlready = true;
            }
            foreach (var grabObject in grabbableHits)
            {
                if (!grabObject.Key.IsOwner)
                {
                    grabObject.Key.gameObject.GetComponent<NetworkObject>().ChangeOwnership(player.OwnerClientId);
                }
                grabObject.Key.transform.SetParent(indicator.transform, true);
                grabObject.Key.parentObject = indicator.transform;
                grabObject.Key.isHeld = true;
                grabObject.Key.EnablePhysics(false);
                // SithCompanyMod.mls.LogInfo("Set GrabbableObject transform.position to " + grabObject.Key.transform.position);
            }
            foreach (var playerObject in playerHits)
            {
                if (playerObject.Key != GameNetworkManager.Instance.localPlayerController)
                {
                    playerObject.Key.transform.position = indicator.transform.position + playerObject.Value;
                    SithCompanyMod.mls.LogInfo("Set PlayerControllerB transform.position to " + playerObject.Key.transform.position);
                }
            }
            foreach (var enemyObject in enemyHits)
            {
                EnemyAI enemy = enemyObject.Key;

                // 1. Force the position
                enemy.transform.position = indicator.transform.position + enemyObject.Value;

                // CRITICAL FIX A: Disable NavMeshAgent and AI calculation entirely
                enemy.SetClientCalculatingAI(false);

                // CRITICAL FIX B: Set internal flag to pause AI logic in the Update loop
                enemy.inSpecialAnimation = true;

                // NEW: If a Rigidbody is present, disable physics while held
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }
        public static void ReleaseTheForce()
        {
            gotForcablesAlready = false;
            ReleaseGrabbables();
            ReleaseEnemies();
        }
        public static void ReleaseGrabbables()
        {
            // Define a guaranteed safe drop location near the indicator
            Vector3 indicatorDropWorldPos = indicator.transform.position;
            indicatorDropWorldPos.y += 0.1f;

            foreach (var grabObject in grabbableHits)
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
        public static void ReleaseEnemies()
        {
            foreach (var enemyObject in ForceAbility.enemyHits)
            {
                EnemyAI enemy = enemyObject.Key;

                // 1. Restore AI Logic flags
                enemy.inSpecialAnimation = false;

                // 2. CRITICAL: Add to falling state with downward velocity
                Vector3 initialVelocity = Vector3.down * InitialFallImpulse;
                if (!fallingEnemies.ContainsKey(enemy))
                {
                    fallingEnemies.Add(enemy, initialVelocity);
                }

                SithCompanyMod.mls.LogInfo($"Released EnemyAI: {enemy.name}");
            }
            enemyHits.Clear();
        }
        // find all grabbable objects, players, and enemies in a sphere
        public static void FindTargetsInSphere(Vector3 center, float radius)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);

            foreach (Collider c in hits)
            {
                // Grabbable
                var grabCol = c.GetComponent<GrabbableObject>();
                if (grabCol != null && !grabbableHits.ContainsKey(grabCol))
                {
                    grabbableHits.Add(grabCol, grabCol.parentObject);
                    SithCompanyMod.mls.LogInfo("Found GrabbableObject at " + grabCol.transform.position.ToString());
                }
                // Player
                var playerCol = c.GetComponent<PlayerControllerB>();
                if (playerCol != null && !playerHits.ContainsKey(playerCol))
                {
                    playerHits.Add(playerCol, playerCol.transform.position - indicator.transform.position);
                    SithCompanyMod.mls.LogInfo("Found PlayerControllerB at " + playerCol.transform.position.ToString());
                }
                // Enemy
                EnemyAICollisionDetect enemyCollision = c.GetComponent<EnemyAICollisionDetect>();
                if ((enemyCollision != null) && (enemyCollision.mainScript != null))
                {
                    // The mainScript field holds the reference to the main EnemyAI component.
                    EnemyAI enemy = enemyCollision.mainScript;
                    if (enemy != null && !enemyHits.ContainsKey(enemy))
                    {
                        // 1. Claim ownership of the enemy if not already owned by the server.
                        // We don't claim ownership here, just track it. Ownership is claimed in UseTheForce.

                        enemyHits.Add(enemy, enemy.transform.position - indicator.transform.position);
                        SithCompanyMod.mls.LogInfo("Found EnemyAI at " + enemy.transform.position.ToString());
                    }
                }
            }
        }
    }
}

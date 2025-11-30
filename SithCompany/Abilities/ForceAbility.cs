using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SithCompany.Abilities
{
    internal class ForceAbility
    {
        public static bool forceModeEnabled = false;
        public static float indicatorDistance = 10f;
        public static bool gotForcablesAlready = false;
        public static Dictionary<UnityEngine.Object, Vector3> savedOffsets = new Dictionary<UnityEngine.Object, Vector3>();
        public static GameObject indicator;

        public static void CreateIndicator(float radius)
        {
            // Create primitive sphere
            indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "SithCompanyForceIndicator";

            // Scale to radius
            indicator.transform.localScale = Vector3.one * radius * 2f;

            // Load HDRP Unlit shader
            Shader shader = Shader.Find("HDRP/Unlit");
            if (shader == null)
            {
                Debug.LogError("HDRP/Unlit shader not found!");
                return;
            }

            Material mat = new Material(shader);

            // Transparent red color
            mat.SetColor("_UnlitColor", new Color(1f, 0f, 0f, 0.3f));

            // Transparent surface
            mat.SetFloat("_SurfaceType", 1); // 0 = Opaque, 1 = Transparent
            mat.SetFloat("_BlendMode", 0);   // 0 = Alpha
            mat.SetFloat("_ZWrite", 0);      // disable depth writing

            // Correct HDRP blending
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Assign material
            indicator.GetComponent<Renderer>().material = mat;

            // Start hidden
            indicator.SetActive(false);
        }

        public static void ToggleForceMode(PlayerControllerB player)
        {
            if (forceModeEnabled)
            {
                // Disable force mode
                forceModeEnabled = false;
                SithCompanyMod.mls.LogInfo("Force Mode Disabled");
                // Hide indicator
                if (ForceAbility.indicator != null)
                {
                    ForceAbility.indicator.SetActive(false);
                }
                else
                {
                    SithCompanyMod.mls.LogWarning("Force Indicator is null when trying to disable Force Mode.");
                }
            }
            else
            {
                // Enable force mode
                forceModeEnabled = true;
                SithCompanyMod.mls.LogInfo("Force Mode Enabled");
                // Show indicator
                if (ForceAbility.indicator != null)
                {
                    ForceAbility.indicator.SetActive(false);
                }
                else
                {
                    SithCompanyMod.mls.LogWarning("Force Indicator is null when trying to enable Force Mode.");
                }
            }
        }
        public static void UseTheForce(PlayerControllerB player)
        {
            if (!gotForcablesAlready)
            {
                LockTargetsRelativeToSphere(ForceAbility.indicator.transform.position, FindTargetsInSphere(ForceAbility.indicator.transform.position, SithCompanyMod.configForceRadius.Value));

            }

        }

        // find all grabbable objects, players, and enemies in a sphere
        public static List<UnityEngine.Object> FindTargetsInSphere(Vector3 center, float radius)
        {
            List<UnityEngine.Object> results = new List<UnityEngine.Object>();

            Collider[] hits = Physics.OverlapSphere(center, radius);

            foreach (Collider c in hits)
            {
                // Grabbable
                var grab = c.GetComponentInParent<GrabbableObject>();
                if (grab != null && !results.Contains(grab))
                    results.Add(grab);

                // Player
                var player = c.GetComponentInParent<PlayerControllerB>();
                if (player != null && !results.Contains(player))
                    results.Add(player);

                // Enemy
                var enemy = c.GetComponentInParent<EnemyAI>();
                if (enemy != null && !results.Contains(enemy))
                    results.Add(enemy);
            }

            return results;
        }
        public static void LockTargetsRelativeToSphere(
        Vector3 sphereCenter,
        List<UnityEngine.Object> targets)
        {
            foreach (var target in targets)
            {
                // Resolve root transform
                Transform t = ResolveTransform(target);
                if (t == null) continue;

                // Save offset only once
                if (!savedOffsets.ContainsKey(target))
                {
                    savedOffsets[target] = t.position - sphereCenter;
                }

                // Maintain displacement
                t.position = sphereCenter + savedOffsets[target];
            }
        }
        private static Transform ResolveTransform(UnityEngine.Object obj)
        {
            if (obj is GrabbableObject g)
                return g.transform;

            if (obj is PlayerControllerB p)
                return p.transform;

            if (obj is EnemyAI e)
                return e.transform;

            return null;
        }
    }
}

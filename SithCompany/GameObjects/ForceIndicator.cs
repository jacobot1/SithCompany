using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SithCompany.GameObjects
{
    internal class ForceIndicator
    {
        private static GameObject sphere;

        static void CreateIndicator()
        {
            // Create primitive sphere
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "SithCompanyForceIndicator";

            // Make it small-ish
            sphere.transform.localScale = new Vector3(2f, 2f, 2f);

            // Create a transparent red material
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0f, 0f, 0.3f);      // RGBA (alpha = 0.3)
            mat.SetFloat("_Mode", 3);                     // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            sphere.GetComponent<Renderer>().material = mat;

            // Start as hidden
            sphere.SetActive(false);
        }

    }
}

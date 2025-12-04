Shader "Custom/ThinBubbleUnlit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,0,0,0.06)
        _RimColor  ("Rim Color", Color)  = (1,0.5,0.5,1)
        _RimPower  ("Rim Power", Range(1,8)) = 3
        _RimIntensity ("Rim Intensity", Range(0,2)) = 0.9
        _MainTex ("MainTex (optional)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Cull Off               // double-sided
        ZWrite Off             // don't write depth (transparency)
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize vectors
                float3 N = normalize(i.normalWS);
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Fresnel-like rim term (higher power -> thinner rim)
                float f = pow(1.0 - saturate(dot(N, V)), _RimPower);
                float rim = saturate(f * _RimIntensity);

                // Base color (use texture if desired)
                fixed4 baseTex = tex2D(_MainTex, i.uv);
                fixed4 baseCol = lerp(_BaseColor, baseTex * _BaseColor, baseTex.a);

                // Combine base + rim (rim adds lightness, not full opacity)
                fixed3 colorOut = baseCol.rgb + _RimColor.rgb * rim;
                float alphaOut = baseCol.a; // keep base alpha for overall transparency

                // optional: slightly brighten specular highlight driven by rim
                // colorOut += pow(rim, 2.0) * 0.15;

                return fixed4(colorOut, alphaOut);
            }
            ENDHLSL
        }
    }
    FallBack "Unlit/Transparent"
}

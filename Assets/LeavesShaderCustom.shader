Shader "Unlit/LeavesShaderCustom"
{
    Properties
    {
        _MainTex ("Leaf Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _EmissiveColor ("Emissive Color", Color) = (0,1,0)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 1
        _TranslucencyColor ("Translucency Color", Color) = (0,1,0)
        _TranslucencyStrength ("Translucency Strength", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        CGPROGRAM
        #pragma surface surf Lambert alpha:fade addshadow

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _EmissiveColor;
        float _EmissionStrength;
        fixed4 _TranslucencyColor;
        float _TranslucencyStrength;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA // Necessary for shadows and lighting
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Sample the texture and apply the color tint
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Set the albedo and alpha for transparency
            o.Albedo = tex.rgb;
            o.Alpha = tex.a;

            // Apply emissive color for glowing effect
            o.Emission = _EmissiveColor.rgb * _EmissionStrength;

            // Calculate translucency based on light direction and normal
            fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
            float translucencyFactor = saturate(dot(IN.worldNormal, -lightDir));

            // Blend the translucency color based on translucency factor and strength
            fixed3 translucency = _TranslucencyColor.rgb * _TranslucencyStrength * translucencyFactor;

            // Add translucency to albedo
            o.Albedo += translucency;
        }
        ENDCG
    }

    FallBack "Transparent/Cutout/Diffuse"
}

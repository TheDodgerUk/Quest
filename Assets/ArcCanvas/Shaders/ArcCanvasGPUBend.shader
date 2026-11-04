// =============================================================================
// GENERATED FILE — do not edit GPUBend.shader directly.
// Source of truth is GPUBend.template.txt (this file) plus GPUBend_URP.subshader.txt
// and GPUBend_HDRP.subshader.txt, spliced together by
// Editor/GPUBendShaderGenerator.cs based on which render pipeline packages
// are actually installed in the project.
// =============================================================================
Shader "ArcCanvas/GPUBend"
{
    // Optional alternate path: bends geometry in the vertex shader instead of
    // via ArcCanvasVertexEffect's CPU mesh rebuild. Use on panels that update
    // every frame; pair with a low-quality ArcCanvasVertexEffect purely for
    // correct raycasting and let this shader carry the visual smoothness.

    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Curve Radius", Float) = 800
        _CurveBlend ("Curve Blend", Range(0,1)) = 1
    }

    // ---------------------------------------------------------------------
    // Built-in Render Pipeline
    // Always present. Has zero SRP package dependencies, so this SubShader
    // alone guarantees the shader compiles cleanly in every project.
    // ---------------------------------------------------------------------
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Radius;
            float _CurveBlend;

            v2f vert(appdata_t v)
            {
                v2f o;
                float safeRadius = max(_Radius, 1.0);
                float theta = v.vertex.x / safeRadius;
                float3 curved = float3(sin(theta) * safeRadius, v.vertex.y, cos(theta) * safeRadius - safeRadius);
                float3 bent = lerp(v.vertex.xyz, curved, _CurveBlend);
                o.vertex = UnityObjectToClipPos(float4(bent, 1.0));
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.texcoord) * i.color;
            }
            ENDCG
        }
    }


    FallBack "Sprites/Default"
}

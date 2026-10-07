// World-space signs, labels and crew numbers (TextMesh). Unlike the font's built-in GUI/Text material,
// this one is depth tested (the sign board and the walls hide the letters) and draws only from the
// reading side: text seen from behind is culled instead of showing mirrored. The facing test uses the
// TextMesh's own frame (glyphs face -Z), so it doesn't depend on the generated triangle winding.
Shader "Abandoned/WorldText"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -1

        Pass
        {
            Name "WorldText"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float front : TEXCOORD1;
                half fog : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _Color;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                // Positive while the camera is on the text's readable (-Z) side of its plane.
                float3 cameraOS = TransformWorldToObject(GetCameraPositionWS());
                output.front = input.positionOS.z - cameraOS.z;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                clip(input.front);
                half4 colour = input.color;
                colour.a *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                colour.rgb = MixFog(colour.rgb, input.fog);
                return colour;
            }
            ENDHLSL
        }
    }
    Fallback Off
}

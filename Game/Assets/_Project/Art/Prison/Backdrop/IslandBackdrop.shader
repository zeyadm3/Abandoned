Shader "Abandoned/IslandBackdrop"
{
    Properties
    {
        _BaseColor ("Colour", Color) = (.32,.39,.33,1)
        _HazeColor ("Haze", Color) = (.66,.71,.76,1)
        _HazeDistance ("Haze distance", Float) = 1400
        _Glow ("Glow (lamps keep their light through the haze)", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "IslandBackdrop"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HazeColor;
                float _HazeDistance;
                half _Glow;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half shade : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                half3 normal = TransformObjectToWorldNormal(input.normalOS);
                // The same high sun as the island: a soft daylight floor keeps unlit distant faces open.
                output.shade = .8h + .2h * saturate(dot(normal, normalize(half3(-.19h, .93h, .33h))));
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float bayDistance = length(_WorldSpaceCameraPos.xz - input.positionWS.xz);
                half distanceMist = 1.0h - exp2(-bayDistance * 1.7 / max(_HazeDistance, 1));
                // Dense air stays close to the water; summits retain a little more shape above the horizon band.
                half seaMist = .16h * exp2(-max(input.positionWS.y + 2.5, 0.0) / 90.0);
                half mist = saturate(distanceMist + seaMist) * (1.0h - _Glow * .85h);
                half3 lit = _BaseColor.rgb * lerp(input.shade, (half)1.0, _Glow);
                return half4(lerp(lit, _HazeColor.rgb, mist), 1);
            }
            ENDHLSL
        }
    }
}
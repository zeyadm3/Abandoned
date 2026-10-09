Shader "Abandoned/MountainBackdrop"
{
    Properties
    {
        _BaseColor ("Rock / foliage colour", Color) = (.08,.09,.085,1)
        _HazeColor ("Night atmospheric haze", Color) = (.092,.117,.139,1)
        _HazeDistance ("Atmospheric perspective distance", Float) = 1100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "MountainBackdrop"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
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
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half shade : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                half3 normal = TransformObjectToWorldNormal(input.normalOS);
                output.shade = .58h + .42h * saturate(dot(normal, normalize(half3(-.35h,.8h,-.45h))));
                output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half mist = saturate(distance(_WorldSpaceCameraPos, input.positionWS) / max(_HazeDistance,1));
                mist = .12h + .56h * mist;
                half3 rock = _BaseColor.rgb * input.shade * input.color.rgb;
                return half4(lerp(rock, _HazeColor.rgb, mist), 1);
            }
            ENDHLSL
        }
    }
}
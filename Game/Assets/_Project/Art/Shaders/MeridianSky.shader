Shader "Abandoned/MeridianSky"
{
    Properties
    {
        _BaseMap ("Original overcast sky", 2D) = "white" {}
        _Exposure ("Exposure", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "RenderPipeline"="UniversalPipeline" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Exposure;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.direction=input.positionOS.xyz;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 direction=normalize(input.direction);
                float2 uv=float2(atan2(direction.z,direction.x)/6.2831853+.5,asin(direction.y)/3.14159265+.5);
                return half4(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb*_Exposure,1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}

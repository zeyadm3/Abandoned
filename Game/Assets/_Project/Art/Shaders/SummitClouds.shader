Shader "Abandoned/SummitClouds"
{
    Properties
    {
        _CloudNoise("Cloud density", 3D) = "white" {}
        _CloudCenter("Cloud centre",Vector)=(0,232,0,0)
        _CloudHalfSize("Cloud extent",Vector)=(600,36,600,0)
        _BuildingCenter("Interior centre",Vector)=(0,125,6,0)
        _CloudTop("Sunlit tops",Color)=(.76,.81,.86,1)
        _CloudBottom("Cloud shadow",Color)=(.22,.28,.36,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-50" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Summit cloud bank"
            Blend One OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Front
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE3D(_CloudNoise); SAMPLER(sampler_CloudNoise);
            CBUFFER_START(UnityPerMaterial)
                float4 _CloudCenter, _CloudHalfSize, _BuildingCenter;
                half4 _CloudTop, _CloudBottom;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                // The volume is a raymarch proxy. Keep its back faces from being clipped at the
                // camera far plane, which otherwise opens hard polygon holes in the cloud bank.
                #if UNITY_REVERSED_Z
                    o.positionCS.z=max(o.positionCS.z,.00001*o.positionCS.w);
                #else
                    o.positionCS.z=min(o.positionCS.z,.99999*o.positionCS.w);
                #endif
                return o;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 eye=GetCameraPositionWS();
                float3 ray=normalize(input.positionWS-eye);
                float3 inverse=rcp(ray+1e-6);
                float3 a=(_CloudCenter.xyz-_CloudHalfSize.xyz-eye)*inverse;
                float3 b=(_CloudCenter.xyz+_CloudHalfSize.xyz-eye)*inverse;
                float3 low=min(a,b),high=max(a,b);
                float entry=max(0,max(low.x,max(low.y,low.z)));
                float exit=min(high.x,min(high.y,high.z));
                float2 uv=GetNormalizedScreenSpaceUV(input.positionCS);
                float depth=SampleSceneDepth(uv);
                #if !UNITY_REVERSED_Z
                    depth=lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
                #endif
                float3 opaque=ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);
                exit=min(exit,distance(eye,opaque));
                if(exit<=entry) return 0;
                float stride=min((exit-entry)/32,18);
                half3 accumulated=0; half transmittance=1;
                float shift=_Time.y*.00035;
                [loop] for(int i=0;i<32;i++)
                {
                    float t=entry+(i+.5)*stride;
                    float3 p=eye+ray*t;
                    float3 local=p-_CloudCenter.xyz;
                    float3 interior=p-_BuildingCenter.xyz;
                    // Keep every enclosed room clear while clouds brush the glazing outside.
                    if(abs(interior.x)<19.5 && abs(interior.z)<17.5) continue;
                    float broad=SAMPLE_TEXTURE3D_LOD(_CloudNoise,sampler_CloudNoise,p*.0045+float3(shift,0,shift*.4),0).r;
                    float detail=SAMPLE_TEXTURE3D_LOD(_CloudNoise,sampler_CloudNoise,p*.018+float3(0,0,shift),0).r;
                    float lower=exp(-pow((local.y+25)/18,2));
                    float wisps=exp(-pow((local.y-40)/10,2))*.16;
                    float density=lower*saturate((broad*.8+detail*.2-.35)*3.5)
                        +wisps*saturate((detail-.58)*5);
                    density*=saturate((min(_CloudHalfSize.x,_CloudHalfSize.z)-max(abs(local.x),abs(local.z)))/180);
                    float alpha=1-exp(-density*stride*.09);
                    half3 colour=lerp(_CloudBottom.rgb,_CloudTop.rgb,saturate((local.y+35)/70));
                    accumulated+=transmittance*alpha*colour;
                    transmittance*=1-alpha;
                    if(transmittance<.015)break;
                }
                return half4(accumulated,1-transmittance);
            }
            ENDHLSL
        }
    }
}

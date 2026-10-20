// Volumetric fog (0.24.12): marched at reduced resolution through the camera's depth, lit by the sun and every
// clustered local light with its shadows and cookie, blurred with depth awareness, then upsampled and blended over
// the opaque scene (colour * transmittance + in-scattered light). Driven by Abandoned.Core.VolumetricFogPass.
Shader "Hidden/Abandoned/VolumetricFog"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        float4 _FogCorner00, _FogCorner10, _FogCorner01; // world-space rays to the view's corners at view depth 1
        float4 _FogCamPos;
        float4 _FogSourceSize;                            // low-res target: w, h, 1/w, 1/h
        TEXTURE2D(_FogInput); SAMPLER(sampler_FogInput);

        struct V2F { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

        V2F Vert(uint id : SV_VertexID)
        {
            V2F o;
            o.pos = GetFullScreenTriangleVertexPosition(id);
            o.uv = GetFullScreenTriangleTexCoord(id);
            return o;
        }

        float3 ViewRay(float2 uv) { return _FogCorner00.xyz + uv.x * (_FogCorner10.xyz - _FogCorner00.xyz) + uv.y * (_FogCorner01.xyz - _FogCorner00.xyz); }
        float EyeDepth(float2 uv) { return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams); }
        ENDHLSL

        // 0: march
        Pass
        {
            Name "VolumetricFogMarch"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment March
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #define _ADDITIONAL_LIGHTS 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _FogParams0;   // density, base height, height falloff, max distance
            float4 _FogParams1;   // anisotropy, main light, local lights, unshadowed scale
            float4 _FogParams2;   // noise, noise scale, steps, shadow distance
            float4 _FogAlbedo;
            float4 _FogAmbient;
            float4 _FogWind;      // xyz offset (m), w unused

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float ValueNoise(float3 x)
            {
                float3 i = floor(x), f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            float Density(float3 p)
            {
                float d = _FogParams0.x * exp(-max(0.0, p.y - _FogParams0.y) * _FogParams0.z);
                float n = ValueNoise((p + _FogWind.xyz) * _FogParams2.y);
                return d * lerp(1.0, saturate(n * 2.0), _FogParams2.x);
            }

            float Phase(float cosTheta, float g)
            {
                float g2 = g * g;
                return (1.0 - g2) / (12.5663706 * pow(max(1e-4, 1.0 + g2 - 2.0 * g * cosTheta), 1.5));
            }

            float3 LocalLights(float3 p, float3 rayDir, float2 screenUV)
            {
                float3 sum = 0;
                InputData inputData = (InputData)0;
                inputData.positionWS = p;
                inputData.normalizedScreenSpaceUV = screenUV;
                half4 shadowMask = half4(1, 1, 1, 1);
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, p, shadowMask);
                    float scale = 1.0;
                    #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    if (GetAdditionalLightShadowParams(lightIndex).x <= 0.0) scale = _FogParams1.w;
                    #else
                    scale = _FogParams1.w;
                    #endif
                    // Scattering near a light grows without bound (1/d^2); a torch in your hand would fill the view
                    // with a white blob. Fade it in over the first metre and a half from the bulb.
                    #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                    float3 lightPos = _AdditionalLightsBuffer[lightIndex].position.xyz;
                    #else
                    float3 lightPos = _AdditionalLightsPosition[lightIndex].xyz;
                    #endif
                    float near = saturate((distance(lightPos, p) - 0.25) / 1.5);
                    float atten = light.distanceAttenuation * light.shadowAttenuation * scale * near * near;
                    sum += light.color * atten * Phase(dot(light.direction, rayDir), _FogParams1.x);
                LIGHT_LOOP_END
                return sum * _FogParams1.z;
            }

            float4 March(V2F i) : SV_Target
            {
                float3 ray = ViewRay(i.uv);
                float rawDepth = SampleSceneDepth(i.uv);
                float eye = LinearEyeDepth(rawDepth, _ZBufferParams);
                float rayLength = length(ray);
                float3 dir = ray / rayLength;
                float maxDist = _FogParams0.w;
                #if UNITY_REVERSED_Z
                bool sky = rawDepth <= 1e-6;
                #else
                bool sky = rawDepth >= 1.0 - 1e-6;
                #endif
                float dist = sky ? maxDist : min(eye * rayLength, maxDist);

                int steps = (int)_FogParams2.z;
                // A fixed (not per-frame) interleaved-gradient offset: the blur turns it into smooth gradients without crawling.
                float jitter = InterleavedGradientNoise(i.pos.xy, 0);
                Light main = GetMainLight();
                float mainPhase = Phase(dot(main.direction, dir), _FogParams1.x);
                float3 ambient = _FogAmbient.rgb;
                float3 scattered = 0;
                float transmittance = 1;

                [loop] for (int s = 0; s < steps; s++)
                {
                    // Samples bunch up near the camera (quadratic spacing), where beams and shadows change fastest.
                    float u0 = s / (float)steps, u1 = (s + 1) / (float)steps, u = (s + jitter) / (float)steps;
                    float stepLen = dist * (u1 * u1 - u0 * u0);
                    float t = dist * u * u;
                    float3 p = _FogCamPos.xyz + dir * t;
                    float density = Density(p);
                    if (density <= 1e-6) continue;

                    // Past the shadow distance the sun's shadow map says "lit" everywhere, which would pour daylight
                    // through roofs: fade the sun out where its shadows end.
                    float sunFade = 1.0 - smoothstep(_FogParams2.w * 0.8, _FogParams2.w, t);
                    float sunShadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(p));
                    float3 light = ambient
                                 + main.color * (sunShadow * sunFade * mainPhase * _FogParams1.y)
                                 + LocalLights(p, dir, i.uv);

                    float3 sigmaS = density * _FogAlbedo.rgb;
                    float sigmaT = max(density, 1e-5);
                    float stepT = exp(-sigmaT * stepLen);
                    scattered += transmittance * (light * sigmaS - light * sigmaS * stepT) / sigmaT;
                    transmittance *= stepT;
                    if (transmittance < 0.01) break;
                }
                return float4(scattered, transmittance);
            }
            ENDHLSL
        }

        // 1: depth-aware blur (direction in _FogBlurDir)
        Pass
        {
            Name "VolumetricFogBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Blur
            float4 _FogBlurDir;

            float4 Blur(V2F i) : SV_Target
            {
                const float weights[4] = { 0.383, 0.242, 0.061, 0.006 };
                float centreDepth = EyeDepth(i.uv);
                float4 sum = SAMPLE_TEXTURE2D(_FogInput, sampler_FogInput, i.uv) * weights[0];
                float total = weights[0];
                [unroll] for (int k = 1; k < 4; k++)
                {
                    [unroll] for (int side = -1; side <= 1; side += 2)
                    {
                        float2 uv = i.uv + _FogBlurDir.xy * _FogSourceSize.zw * (k * side * 1.75);
                        float d = EyeDepth(uv);
                        float w = weights[k] * rcp(1.0 + abs(d - centreDepth) * 4.0 / max(centreDepth, 0.1));
                        sum += SAMPLE_TEXTURE2D(_FogInput, sampler_FogInput, uv) * w;
                        total += w;
                    }
                }
                return sum / total;
            }
            ENDHLSL
        }

        // 2: upsample and blend over the scene (scattered + scene * transmittance)
        Pass
        {
            Name "VolumetricFogComposite"
            Blend One SrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Composite
            float _FogDebug;

            float4 Composite(V2F i) : SV_Target
            {
                float depth = EyeDepth(i.uv);
                float2 texel = i.uv * _FogSourceSize.xy - 0.5;
                float2 base = floor(texel);
                float2 f = texel - base;
                float4 sum = 0;
                float total = 0;
                [unroll] for (int y = 0; y < 2; y++)
                [unroll] for (int x = 0; x < 2; x++)
                {
                    float2 uv = (base + float2(x, y) + 0.5) * _FogSourceSize.zw;
                    float bilinear = (x == 0 ? 1.0 - f.x : f.x) * (y == 0 ? 1.0 - f.y : f.y);
                    float w = bilinear * rcp(1e-3 + abs(EyeDepth(uv) - depth) / max(depth, 0.1)) + 1e-5;
                    sum += SAMPLE_TEXTURE2D_LOD(_FogInput, sampler_FogInput, uv, 0) * w;
                    total += w;
                }
                float4 fog = sum / total;
                // F1 view: the fog alone over black.
                if (_FogDebug > 0.5) return float4(fog.rgb * 4.0, 0.0);
                return fog;
            }
            ENDHLSL
        }
    }
    Fallback Off
}

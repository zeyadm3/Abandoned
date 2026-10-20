// Eye adaptation (0.24.12), driven by Abandoned.Core.AutoExposurePass:
// 0 writes centre-weighted log luminance into a 64 x 64 map, 1 averages it and eases last frame's exposure toward
// the new target, 2 multiplies the scene by it (Blend DstColor Zero) before bloom and tonemapping.
Shader "Hidden/Abandoned/AutoExposure"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        TEXTURE2D(_ExposureSource); SAMPLER(sampler_ExposureSource);
        TEXTURE2D(_ExposurePrevious); SAMPLER(sampler_ExposurePrevious);
        float4 _ExposureParams0; // key, strength, min EV, max EV
        float4 _ExposureParams1; // speed down, speed up, delta time, mip of the 1x1 average
        float _ExposureReset;

        struct V2F { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
        V2F Vert(uint id : SV_VertexID)
        {
            V2F o;
            o.pos = GetFullScreenTriangleVertexPosition(id);
            o.uv = GetFullScreenTriangleTexCoord(id);
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "Luminance"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            float4 Frag(V2F i) : SV_Target
            {
                float3 c = SAMPLE_TEXTURE2D_LOD(_ExposureSource, sampler_ExposureSource, i.uv, 0).rgb;
                float lum = max(Luminance(c), 1e-4);
                float2 d = i.uv - 0.5;
                float weight = exp(-dot(d, d) * 6.0); // the middle of the view counts most
                return float4(log2(lum) * weight, weight, 0, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Adapt"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            float4 Frag(V2F i) : SV_Target
            {
                // Average the 64 x 64 luminance map with 32 x 32 bilinear taps (each covers four texels); no mip chain.
                float2 sum = 0;
                [loop] for (int y = 0; y < 32; y++)
                [loop] for (int x = 0; x < 32; x++)
                    sum += SAMPLE_TEXTURE2D_LOD(_ExposureSource, sampler_ExposureSource, (float2(x, y) + 0.5) / 32.0, 0).rg;
                float avgEv = sum.x / max(sum.y, 1e-4);
                float targetEv = clamp((log2(_ExposureParams0.x) - avgEv) * _ExposureParams0.y, _ExposureParams0.z, _ExposureParams0.w);
                float previous = SAMPLE_TEXTURE2D_LOD(_ExposurePrevious, sampler_ExposurePrevious, float2(0.5, 0.5), 0).r;
                if (_ExposureReset > 0.5) return float4(targetEv, 0, 0, 0);
                float speed = targetEv < previous ? _ExposureParams1.x : _ExposureParams1.y;
                float step = speed * _ExposureParams1.z;
                return float4(previous + clamp(targetEv - previous, -step, step), 0, 0, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Apply"
            Blend DstColor Zero
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            float4 Frag(V2F i) : SV_Target
            {
                float ev = SAMPLE_TEXTURE2D_LOD(_ExposurePrevious, sampler_ExposurePrevious, float2(0.5, 0.5), 0).r;
                return float4(exp2(ev).xxx, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}

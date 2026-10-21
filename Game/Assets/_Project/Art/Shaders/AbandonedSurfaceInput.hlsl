#ifndef ABANDONED_SURFACE_INPUT_INCLUDED
#define ABANDONED_SURFACE_INPUT_INCLUDED

// 0.24.13 environment surfaces: URP 17.3's LitInput.hlsl (same properties, so a material keeps every value when its
// shader is swapped) plus four of our own at the end of the material buffer, and InitializeAbandonedSurfaceData, which
// the forward pass calls with the pixel's world position and normal. What it adds, and why:
//  - _AntiTile: two offset reads blended by a slow noise (Quilez, "texture repetition", technique 3), so a 2 m scan on a
//    40 m concourse floor stops reading as a grid.
//  - _Variation: big world-space blotches of light and dark, so no two bays are the same grey.
//  - _Grime: dirt that gathers in the scan's own cavities, in patches, and runs down walls in streaks.
//  - _Dust: a matt film on everything that faces up. The building has been shut for years.
// Globals from Core.SurfaceWeathering (per map): _AbandonedGrimeColor, _AbandonedDustColor, _AbandonedWeather (0 off).

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

#if defined(_DETAIL_MULX2) || defined(_DETAIL_SCALED)
#define _DETAIL
#endif

// NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
float4 _DetailAlbedoMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _Parallax;
half _OcclusionStrength;
half _ClearCoatMask;
half _ClearCoatSmoothness;
half _DetailAlbedoMapScale;
half _DetailNormalMapScale;
half _Surface;
half _AntiTile;
half _Variation;
half _Grime;
half _Dust;
UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END

// Per map, from Core.SurfaceWeathering. Unset globals read 0, so _AbandonedWeather = 0 means "plain URP Lit".
float4 _AbandonedGrimeColor;
float4 _AbandonedDustColor;
float _AbandonedWeather;

// NOTE: Do not ifdef the properties for dots instancing, but ifdef the actual usage.
// Otherwise you might break CPU-side as property constant-buffer offsets change per variant.
// NOTE: Dots instancing is orthogonal to the constant buffer above.
#ifdef UNITY_DOTS_INSTANCING_ENABLED

UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
    UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
    UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
    UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
    UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
    UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)
    UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
    UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

// Here, we want to avoid overriding a property like e.g. _BaseColor with something like this:
// #define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor0)
//
// It would be simpler, but it can cause the compiler to regenerate the property loading code for each use of _BaseColor.
//
// To avoid this, the property loads are cached in some static values at the beginning of the shader.
// The properties such as _BaseColor are then overridden so that it expand directly to the static value like this:
// #define _BaseColor unity_DOTS_Sampled_BaseColor
//
// This simple fix happened to improve GPU performances by ~10% on Meta Quest 2 with URP on some scenes.
static float4 unity_DOTS_Sampled_BaseColor;
static float4 unity_DOTS_Sampled_SpecColor;
static float4 unity_DOTS_Sampled_EmissionColor;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Smoothness;
static float  unity_DOTS_Sampled_Metallic;
static float  unity_DOTS_Sampled_BumpScale;
static float  unity_DOTS_Sampled_Parallax;
static float  unity_DOTS_Sampled_OcclusionStrength;
static float  unity_DOTS_Sampled_ClearCoatMask;
static float  unity_DOTS_Sampled_ClearCoatSmoothness;
static float  unity_DOTS_Sampled_DetailAlbedoMapScale;
static float  unity_DOTS_Sampled_DetailNormalMapScale;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSLitMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor            = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
    unity_DOTS_Sampled_SpecColor            = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _SpecColor);
    unity_DOTS_Sampled_EmissionColor        = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _EmissionColor);
    unity_DOTS_Sampled_Cutoff               = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Cutoff);
    unity_DOTS_Sampled_Smoothness           = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Smoothness);
    unity_DOTS_Sampled_Metallic             = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Metallic);
    unity_DOTS_Sampled_BumpScale            = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _BumpScale);
    unity_DOTS_Sampled_Parallax             = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Parallax);
    unity_DOTS_Sampled_OcclusionStrength    = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _OcclusionStrength);
    unity_DOTS_Sampled_ClearCoatMask        = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _ClearCoatMask);
    unity_DOTS_Sampled_ClearCoatSmoothness  = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _ClearCoatSmoothness);
    unity_DOTS_Sampled_DetailAlbedoMapScale = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _DetailAlbedoMapScale);
    unity_DOTS_Sampled_DetailNormalMapScale = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _DetailNormalMapScale);
    unity_DOTS_Sampled_Surface              = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSLitMaterialPropertyCaches()

#define _BaseColor              unity_DOTS_Sampled_BaseColor
#define _SpecColor              unity_DOTS_Sampled_SpecColor
#define _EmissionColor          unity_DOTS_Sampled_EmissionColor
#define _Cutoff                 unity_DOTS_Sampled_Cutoff
#define _Smoothness             unity_DOTS_Sampled_Smoothness
#define _Metallic               unity_DOTS_Sampled_Metallic
#define _BumpScale              unity_DOTS_Sampled_BumpScale
#define _Parallax               unity_DOTS_Sampled_Parallax
#define _OcclusionStrength      unity_DOTS_Sampled_OcclusionStrength
#define _ClearCoatMask          unity_DOTS_Sampled_ClearCoatMask
#define _ClearCoatSmoothness    unity_DOTS_Sampled_ClearCoatSmoothness
#define _DetailAlbedoMapScale   unity_DOTS_Sampled_DetailAlbedoMapScale
#define _DetailNormalMapScale   unity_DOTS_Sampled_DetailNormalMapScale
#define _Surface                unity_DOTS_Sampled_Surface

#endif

TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = half4(SAMPLE_METALLICSPECULAR(uv));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
#else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
        specGloss.rgb = _SpecColor.rgb;
    #else
        specGloss.rgb = _Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a = _Smoothness;
    #endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
    #ifdef _OCCLUSIONMAP
        half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
        return LerpWhiteTo(occ, _OcclusionStrength);
    #else
        return half(1.0);
    #endif
}


// Returns clear coat parameters
// .x/.r == mask
// .y/.g == smoothness
half2 SampleClearCoat(float2 uv)
{
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoatMaskSmoothness = half2(_ClearCoatMask, _ClearCoatSmoothness);

#if defined(_CLEARCOATMAP)
    clearCoatMaskSmoothness *= SAMPLE_TEXTURE2D(_ClearCoatMap, sampler_ClearCoatMap, uv).rg;
#endif

    return clearCoatMaskSmoothness;
#else
    return half2(0.0, 1.0);
#endif  // _CLEARCOAT
}

void ApplyPerPixelDisplacement(half3 viewDirTS, inout float2 uv)
{
#if defined(_PARALLAXMAP)
    uv += ParallaxMapping(TEXTURE2D_ARGS(_ParallaxMap, sampler_ParallaxMap), viewDirTS, _Parallax, uv);
#endif
}

// Used for scaling detail albedo. Main features:
// - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
// - No effect is applied if detailAlbedo is 0.5.
half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
{
    // detailAlbedo = detailAlbedo * 2.0h - 1.0h;
    // detailAlbedo *= _DetailAlbedoMapScale;
    // detailAlbedo = detailAlbedo * 0.5h + 0.5h;
    // return detailAlbedo * 2.0f;

    // A bit more optimized
    return half(2.0) * detailAlbedo * scale - scale + half(1.0);
}

half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
{
#if defined(_DETAIL)
    half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;

    // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
#if defined(_DETAIL_SCALED)
    detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
#else
    detailAlbedo = half(2.0) * detailAlbedo;
#endif

    return albedo * LerpWhiteTo(detailAlbedo, detailMask);
#else
    return albedo;
#endif
}

half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
{
#if defined(_DETAIL)
#if BUMP_SCALE_NOT_SUPPORTED
    half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
#else
    half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
#endif

    // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
    // For visual consistancy we going to do in all cases
    detailNormalTS = normalize(detailNormalTS);

    return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
#else
    return normalTS;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

#if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
#endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
#else
    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
#endif

#if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
#endif
}

// ---------------------------------------------------------------------------------------------------------------------
// Abandoned additions

float AbHash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float2 AbHash22(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

float AbHash13(float3 p)
{
    p = frac(p * 0.1031);
    p += dot(p, p.zyx + 31.32);
    return frac((p.x + p.y) * p.z);
}

float AbNoise2(float2 p)
{
    float2 i = floor(p), f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = AbHash12(i), b = AbHash12(i + float2(1, 0)), c = AbHash12(i + float2(0, 1)), d = AbHash12(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float AbNoise3(float3 p)
{
    float3 i = floor(p), f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float n000 = AbHash13(i), n100 = AbHash13(i + float3(1, 0, 0)), n010 = AbHash13(i + float3(0, 1, 0)), n110 = AbHash13(i + float3(1, 1, 0));
    float n001 = AbHash13(i + float3(0, 0, 1)), n101 = AbHash13(i + float3(1, 0, 1)), n011 = AbHash13(i + float3(0, 1, 1)), n111 = AbHash13(i + float3(1, 1, 1));
    return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y), lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
}

float AbFbm3(float3 p)
{
    return AbNoise3(p) * 0.55 + AbNoise3(p * 2.03 + 17.1) * 0.3 + AbNoise3(p * 4.11 + 41.7) * 0.15;
}

// Two reads of one texture, offset per noise cell and cross-faded; gradients from the real UV so mips never jump.
struct AbTile
{
    float2 uvA, uvB, dx, dy;
    half blend;
};

AbTile AbMakeTile(float2 uv)
{
    AbTile t;
    t.dx = ddx(uv);
    t.dy = ddy(uv);
    float k = AbNoise2(uv * 0.29) * 8.0;
    float i = floor(k);
    t.uvA = uv + AbHash22(float2(i, 7.3)) * 7.0;
    t.uvB = uv + AbHash22(float2(i + 1.0, 7.3)) * 7.0;
    t.blend = half(frac(k));
    return t;
}

#define AB_READ(tex, smp, t) lerp(SAMPLE_TEXTURE2D_GRAD(tex, smp, t.uvA, t.dx, t.dy), SAMPLE_TEXTURE2D_GRAD(tex, smp, t.uvB, t.dx, t.dy), t.blend)

#if defined(_TRIPLANAR)
// Three projections in the object's own space, scaled to metres, so the scan sits the same size on every face whatever
// the mesh's UVs say, and nothing slides when the object moves. Blend weights sharpen toward the dominant axis.
struct AbTri
{
    float2 uvX, uvY, uvZ;
    half3 w, nOS, axisSign;
};

AbTri AbMakeTri(float3 positionWS, half3 normalWS)
{
    AbTri t;
    float4x4 m = GetObjectToWorldMatrix();
    float3 scaleOS = float3(length(m._m00_m10_m20), length(m._m01_m11_m21), length(m._m02_m12_m22));
    float3 p = TransformWorldToObject(positionWS) * scaleOS;
    t.nOS = half3(normalize(TransformWorldToObjectNormal(normalWS)));
    half3 w = pow(abs(t.nOS), 4.0h);
    t.w = w / max(w.x + w.y + w.z, 1e-4h);
    t.axisSign = half3(t.nOS.x < 0 ? -1 : 1, t.nOS.y < 0 ? -1 : 1, t.nOS.z < 0 ? -1 : 1);
    float2 st = _BaseMap_ST.xy;
    t.uvX = p.zy * st + _BaseMap_ST.zw;
    t.uvY = p.xz * st + _BaseMap_ST.zw;
    t.uvZ = p.xy * st + _BaseMap_ST.zw;
    return t;
}

#define AB_TRI(tex, smp, t) (SAMPLE_TEXTURE2D(tex, smp, t.uvX) * t.w.x + SAMPLE_TEXTURE2D(tex, smp, t.uvY) * t.w.y + SAMPLE_TEXTURE2D(tex, smp, t.uvZ) * t.w.z)

// Whiteout-blended normal (Golus, "Normal Mapping for a Triplanar Shader"), returned in tangent space so URP's own
// tangent-to-world step puts it back where it belongs.
half3 AbTriNormal(AbTri t, half3 normalWS, half4 tangentWS)
{
    half3 nX = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, t.uvX), _BumpScale);
    half3 nY = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, t.uvY), _BumpScale);
    half3 nZ = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, t.uvZ), _BumpScale);
    nX.x *= t.axisSign.x;
    nY.x *= t.axisSign.y;
    nZ.x *= -t.axisSign.z;
    nX = half3(nX.xy + t.nOS.zy, abs(nX.z) * t.nOS.x);
    nY = half3(nY.xy + t.nOS.xz, abs(nY.z) * t.nOS.y);
    nZ = half3(nZ.xy + t.nOS.xy, abs(nZ.z) * t.nOS.z);
    half3 nOS = normalize(nX.zyx * t.w.x + nY.xzy * t.w.y + nZ.xyz * t.w.z);
    half3 n = half3(normalize(TransformObjectToWorldNormal(nOS)));
    half3 bitangent = half3(tangentWS.w * cross(normalWS, tangentWS.xyz));
    return normalize(half3(dot(n, tangentWS.xyz), dot(n, bitangent), dot(n, normalWS)));
}
#endif

void InitializeAbandonedSurfaceData(float2 uv, float3 positionWS, half3 normalWS, half4 tangentWS, out SurfaceData outSurfaceData)
{
#if defined(_TRIPLANAR)
    AbTri tri = AbMakeTri(positionWS, normalWS);
    half4 albedoAlpha = AB_TRI(_BaseMap, sampler_BaseMap, tri);
#elif defined(_ANTITILE)
    AbTile tile = AbMakeTile(uv);
    half4 albedoA = SAMPLE_TEXTURE2D_GRAD(_BaseMap, sampler_BaseMap, tile.uvA, tile.dx, tile.dy);
    half4 albedoB = SAMPLE_TEXTURE2D_GRAD(_BaseMap, sampler_BaseMap, tile.uvB, tile.dx, tile.dy);
    // Quilez: the darker read wins the middle of the fade, which hides it as a stain instead of a smear.
    half3 diff = albedoA.rgb - albedoB.rgb;
    tile.blend = smoothstep(0.25h, 0.75h, tile.blend - 0.1h * (diff.r + diff.g + diff.b));
    half4 albedoAlpha = lerp(albedoA, albedoB, tile.blend);
#else
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
#endif
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

#if defined(_TRIPLANAR) && defined(_METALLICSPECGLOSSMAP) && !defined(_SPECULAR_SETUP)
    half4 specGloss = half4(AB_TRI(_MetallicGlossMap, sampler_MetallicGlossMap, tri));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha.a * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
#elif defined(_ANTITILE) && !defined(_TRIPLANAR) && defined(_METALLICSPECGLOSSMAP) && !defined(_SPECULAR_SETUP)
    half4 specGloss = half4(AB_READ(_MetallicGlossMap, sampler_MetallicGlossMap, tile));
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha.a * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
#else
    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
#endif
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.albedo = AlphaModulate(outSurfaceData.albedo, outSurfaceData.alpha);

#if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
#endif
    outSurfaceData.smoothness = specGloss.a;

#if defined(_TRIPLANAR) && defined(_NORMALMAP)
    outSurfaceData.normalTS = AbTriNormal(tri, normalWS, tangentWS);
#elif defined(_ANTITILE) && defined(_NORMALMAP)
    half4 nA = SAMPLE_TEXTURE2D_GRAD(_BumpMap, sampler_BumpMap, tile.uvA, tile.dx, tile.dy);
    half4 nB = SAMPLE_TEXTURE2D_GRAD(_BumpMap, sampler_BumpMap, tile.uvB, tile.dx, tile.dy);
    outSurfaceData.normalTS = normalize(lerp(UnpackNormalScale(nA, _BumpScale), UnpackNormalScale(nB, _BumpScale), tile.blend));
#else
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
#endif

#if defined(_TRIPLANAR) && defined(_OCCLUSIONMAP)
    half cavity = AB_TRI(_OcclusionMap, sampler_OcclusionMap, tri).g;
#elif defined(_ANTITILE) && defined(_OCCLUSIONMAP)
    half cavity = AB_READ(_OcclusionMap, sampler_OcclusionMap, tile).g;
#elif defined(_OCCLUSIONMAP)
    half cavity = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half cavity = half(1.0);
#endif
    outSurfaceData.occlusion = LerpWhiteTo(cavity, _OcclusionStrength);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    outSurfaceData.clearCoatMask = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);

#if defined(_WEATHERING)
    half weather = half(saturate(_AbandonedWeather));
    if (weather > 0.001h)
    {
        // Large, slow light/dark: each bay of the same material is a slightly different age.
        float big = AbNoise3(positionWS * 0.21);
        half shade = half(1.0 + (big - 0.5) * 2.0 * _Variation * weather);
        outSurfaceData.albedo *= shade;

        // Grime: blotches, deepened by the scan's own cavities, and streaks running down anything vertical.
        // Two scales, so a counter top gets a few smudges rather than one dark wash; about a quarter of any surface.
        float patch = AbFbm3(positionWS * 0.9) * 0.7 + AbNoise3(positionWS * 3.1 + 11.7) * 0.3;
        half grime = half(saturate((patch - 0.55) * 3.0));
        grime = max(grime, (1.0h - cavity) * 0.7h);
        half vertical = half(saturate(1.0 - abs(normalWS.y) * 3.0));
        float across = dot(positionWS.xz, normalize(float2(-normalWS.z, normalWS.x) + 1e-5));
        float streak = AbNoise2(float2(across * 2.7, positionWS.y * 0.22)) * AbNoise2(float2(across * 0.6 + 9.1, positionWS.y * 0.05));
        grime = max(grime, half(saturate((streak - 0.28) * 3.0)) * vertical);
        grime *= _Grime * weather * 0.75h;
        half3 grimeTint = _AbandonedGrimeColor.a > 0 ? half3(_AbandonedGrimeColor.rgb) : half3(0.42, 0.38, 0.31);
        outSurfaceData.albedo = lerp(outSurfaceData.albedo, outSurfaceData.albedo * grimeTint, grime);
        outSurfaceData.smoothness = lerp(outSurfaceData.smoothness, outSurfaceData.smoothness * 0.35h, grime);

        // Dust settles on what faces up, thinner where boots and drafts have moved it.
        half up = half(saturate((normalWS.y - 0.6) * 2.5));
        float drift = AbFbm3(positionWS * 1.7 + 5.3);
        half dust = up * half(saturate(0.35 + (drift - 0.5) * 1.8)) * _Dust * weather;
        half3 dustTint = _AbandonedDustColor.a > 0 ? half3(_AbandonedDustColor.rgb) : half3(0.36, 0.34, 0.31);
        // Lerp toward a dust colour no brighter than the surface plus a little, so a dark floor greys, never glows.
        half lum = dot(outSurfaceData.albedo, half3(0.2126h, 0.7152h, 0.0722h));
        half3 film = dustTint * saturate(lum * 1.6h + 0.12h);
        outSurfaceData.albedo = lerp(outSurfaceData.albedo, film, dust * 0.55h);
        outSurfaceData.smoothness = lerp(outSurfaceData.smoothness, 0.06h, dust);
        outSurfaceData.metallic *= 1.0h - dust * 0.7h;
    }
#endif
}

#endif // ABANDONED_SURFACE_INPUT_INCLUDED

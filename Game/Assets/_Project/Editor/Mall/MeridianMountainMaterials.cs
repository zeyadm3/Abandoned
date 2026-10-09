using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Authored mountain haze keeps the enclosing silhouette readable behind the mall's dense local fog.</summary>
    public static class MeridianMountainMaterials
    {
        internal const string Folder = "Assets/_Project/Art/Generated/MeridianMountains";

        public static Material[] Rocks() => new[] {
            Material("WoodedSlope", new Color(.035f, .051f, .042f)),
            Material("ExposedRock", new Color(.078f, .086f, .084f)),
            Material("UpperWeatheredRidge", new Color(.118f, .129f, .132f))
        };

        public static Material[] Woodland() => new[] {
            Material("MountainBark", new Color(.034f, .035f, .029f)),
            Material("MountainConifers", new Color(.025f, .042f, .033f)),
            Material("MountainBroadleaf", new Color(.041f, .057f, .041f))
        };

        private static Material Material(string name, Color color)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = BackgroundShader();
            var material = new Material(shader) { name = name, enableInstancing = true };
            material.SetColor("_BaseColor", color);
            material.SetColor("_HazeColor", new Color(.092f, .117f, .139f));
            material.SetFloat("_HazeDistance", 1100f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Shader BackgroundShader()
        {
            const string path = Folder + "/MeridianMountainBackdrop.shader";
            Shader existing = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (existing != null) return existing;
            // These off-map ridges use authored atmospheric perspective: local exponential fog would
            // completely erase mountains hundreds of metres away and recreate the empty horizon.
            File.WriteAllText(path, @"Shader ""Abandoned/MountainBackdrop""
{
    Properties
    {
        _BaseColor (""Rock / foliage colour"", Color) = (.08,.09,.085,1)
        _HazeColor (""Night atmospheric haze"", Color) = (.092,.117,.139,1)
        _HazeDistance (""Atmospheric perspective distance"", Float) = 1100
    }
    SubShader
    {
        Tags { ""RenderType""=""Opaque"" ""Queue""=""Geometry"" ""RenderPipeline""=""UniversalPipeline"" }
        Pass
        {
            Name ""MountainBackdrop""
            Tags { ""LightMode""=""SRPDefaultUnlit"" }
            Cull Back
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""
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
}");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Shader>(path);
        }
    }
}

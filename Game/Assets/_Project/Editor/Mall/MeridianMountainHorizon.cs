using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>A continuous wooded basin surrounds the existing district without changing its streets or buildings.</summary>
    public static class MeridianMountainHorizon
    {
        public const string GroupName = "MeridianMountainHorizon";
        private const int Sectors = 16;
        private static readonly Vector3 Centre = new(28f, -.49f, 14f);

        public static void Build(Transform mall)
        {
            Transform old = mall.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform horizon = Group(GroupName, mall);
            horizon.localPosition = Centre;
            horizon.gameObject.AddComponent<WalkThroughObject>()
                .EditorSetup("The continuous mountain basin is distant scenery beyond the enclosed estate boundary.");
            Material[] mountains = MeridianMountainMaterials.Rocks();
            Material[] woodland = MeridianMountainMaterials.Woodland();

            for (int sector = 0; sector < Sectors; sector++)
            {
                Transform ridge = Group($"ContinuousRidge_{sector:00}", horizon);
                var detailed = MeshObject("WoodedFoothillsAndRockRidge", ridge,
                    MeridianMountainMeshes.Range(sector, false), mountains);
                var simplified = MeshObject("DistantRidgeSilhouette", ridge,
                    MeridianMountainMeshes.Range(sector, true), mountains);
                var forest = MeshObject("MountainForest", ridge,
                    MeridianMountainMeshes.Forest(sector), woodland);
                var lod = ridge.gameObject.AddComponent<LODGroup>();
                // The low-detail silhouette remains visible at every distance, so no horizon gap can open.
                lod.SetLODs(new[] {
                    new LOD(.055f, new Renderer[] { detailed, forest }),
                    new LOD(0f, new Renderer[] { simplified })
                });
                lod.fadeMode = LODFadeMode.None;
                lod.RecalculateBounds();
            }

            foreach (Transform item in horizon.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(item.gameObject, StaticEditorFlags.BatchingStatic);
        }

        private static MeshRenderer MeshObject(string name, Transform parent, Mesh mesh, Material[] materials)
        {
            Transform item = Group(name, parent);
            item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = item.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return renderer;
        }
    }
}

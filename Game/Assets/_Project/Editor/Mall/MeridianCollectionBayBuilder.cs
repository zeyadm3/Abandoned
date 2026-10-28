using System;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Marks the real truck collection point without adding a second cargo or payout system.</summary>
    public static class MeridianCollectionBayBuilder
    {
        public const float ApproachWidth = 4.4f;
        private const string MaterialFolder = "Assets/_Project/Art/Generated/MeridianCollection";

        public static void Build(Transform mall, TruckCargo truck = null)
        {
            truck ??= mall.GetComponentInChildren<TruckCargo>();
            if (truck == null) throw new InvalidOperationException("Build the functional mall truck before its collection presentation.");
            Transform ramp = truck.transform.Find("Ramp"), floor = truck.transform.Find("BayFloor");
            Transform apron = mall.Find("Exterior/GroundSlabs/DockApron");
            if (ramp == null || floor == null || apron == null)
                throw new InvalidOperationException("Meridian collection presentation requires the real truck ramp, cargo floor and dock apron.");

            Transform group = GreyboxFactory.Group("MeridianCollectionBay", mall);
            Material amber = Paint("Amber", new Color(0.94f, 0.67f, 0.13f));
            Material ivory = Paint("Ivory", new Color(0.88f, 0.90f, 0.77f));
            Material ink = Paint("Ink", new Color(0.065f, 0.105f, 0.09f));

            // The approach is wide while the original cargo box remains the sole counting volume.
            Vector3 apronSize = apron.localScale;
            apronSize.x = ApproachWidth + .6f;
            apron.localScale = apronSize;
            Vector3 rampSize = ramp.localScale;
            rampSize.x = TruckBuilder.BayWidth;
            ramp.localScale = rampSize;
            ramp.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("Metal");
            apron.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("Concrete");
            if (ramp.GetComponent<SurfaceTag>() == null) ramp.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            if (apron.TryGetComponent(out SurfaceTag apronSurface)) apronSurface.EditorSet(SurfaceMaterial.Concrete);

            Approach(group, truck.transform, amber, ivory);
            RampPaint(ramp, amber);
            CargoBoundary(floor, amber);
            Signs(group, truck.transform, ink, ivory);
        }

        private static void Approach(Transform group, Transform truck, Material amber, Material ivory)
        {
            Vector3 rear = truck.TransformPoint(new Vector3(0f, 0f, -TruckBuilder.BayLength * .5f));
            rear.y = 0f;
            Vector3 towardMall = -truck.forward;
            Vector3 rampFoot = rear + towardMall * TruckBuilder.RampLength;
            // The short front loading lane meets the main entry plaza; there is no east-side dock route.
            Transform lane = GreyboxFactory.Group("FrontCollectionApproach", group);
            lane.SetPositionAndRotation(rampFoot + towardMall * 2f, Quaternion.LookRotation(towardMall));
            foreach (float side in new[] { -1f, 1f })
            {
                Box("ClearHaulLaneEdge", lane, new Vector3(side * ApproachWidth * .5f, .026f, 0f),
                    new Vector3(.10f, .006f, 4f), Quaternion.identity, amber);
                for (int dash = 0; dash < 4; dash++)
                    Box("CollectionEdgeDash", lane, new Vector3(side * 1.98f, .028f, -.9f + dash * .65f),
                        new Vector3(.065f, .006f, .35f), Quaternion.identity, ivory);
            }
            Transform arrow = GreyboxFactory.Group("CollectionApproachArrow", lane);
            arrow.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Box("CollectionArrowStem", arrow, new Vector3(0f, .028f, -.15f),
                new Vector3(.14f, .006f, 1.2f), Quaternion.identity, amber);
            foreach (float side in new[] { -1f, 1f })
                Box("CollectionArrowHead", arrow, new Vector3(side * .23f, .028f, .36f),
                    new Vector3(.11f, .006f, .72f), Quaternion.Euler(0f, side * -45f, 0f), amber);
        }

        private static void RampPaint(Transform ramp, Material amber)
        {
            // Child dimensions compensate for the existing scaled cube and follow its exact walking slope.
            foreach (float side in new[] { -1f, 1f })
                Box("LoadingRampEdge", ramp, new Vector3(side * 0.482f, 0.56f, 0f),
                    new Vector3(0.08f / ramp.localScale.x, 0.035f, 0.97f), Quaternion.identity, amber);
            for (int stripe = 0; stripe < 5; stripe++)
                Box("LoadingRampGrip", ramp, new Vector3(0f, 0.555f, -0.38f + stripe * 0.19f),
                    new Vector3(0.55f, 0.03f, 0.012f), Quaternion.identity, amber);
        }

        private static void CargoBoundary(Transform floor, Material amber)
        {
            foreach (float side in new[] { -1f, 1f })
                Box("ActualCargoBoundary", floor, new Vector3(side * 0.473f, 0.515f, 0f),
                    new Vector3(0.06f / floor.localScale.x, 0.0125f, 0.96f), Quaternion.identity, amber);
            Box("CargoTailgateLine", floor, new Vector3(0f, 0.515f, -0.485f),
                new Vector3(0.94f, 0.0125f, 0.015f), Quaternion.identity, amber);
        }

        private static void Signs(Transform group, Transform truck, Material ink, Material ivory)
        {
            Vector3 rear = truck.TransformPoint(new Vector3(0f, 0f, -TruckBuilder.BayLength * .5f));
            rear.y = 0f;
            Vector3 front = -truck.forward;
            Sign(group, group.InverseTransformPoint(rear + front * .18f + Vector3.up * 3.28f), front,
                "COLLECTION", 1.2f, .12f, ink, ivory);
        }

        private static void Sign(Transform group, Vector3 position, Vector3 front, string text, float width, float height,
            Material fascia, Material lettering, float spacing = 1f)
        {
            MeridianDressingSigns.Sign(group, position, front, text, width, height, spacing);
            Transform sign = group.GetChild(group.childCount - 1);
            sign.Find("Fascia").GetComponent<Renderer>().sharedMaterial = fascia;
            TextMesh label = sign.GetComponentInChildren<TextMesh>();
            label.color = lettering.GetColor("_BaseColor");
        }

        private static void Box(string name, Transform parent, Vector3 position, Vector3 size, Quaternion rotation, Material material)
        {
            GameObject paint = GreyboxFactory.Box(name, parent, position, size, material, false);
            paint.transform.localRotation = rotation;
            paint.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(paint, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static Material Paint(string name, Color colour)
        {
            PolishAssets.EnsureFolder(MaterialFolder);
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(MeridianArt.Material("FixtureTrim")) { name = "MeridianCollection_" + name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.CopyPropertiesFromMaterial(MeridianArt.Material("FixtureTrim"));
            material.SetColor("_BaseColor", colour);
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

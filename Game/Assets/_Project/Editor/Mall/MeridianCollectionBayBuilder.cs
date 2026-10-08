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
            apronSize.z = ApproachWidth;
            apron.localScale = apronSize;
            Vector3 rampSize = ramp.localScale;
            rampSize.x = ApproachWidth;
            ramp.localScale = rampSize;
            ramp.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("Metal");
            apron.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("Concrete");
            if (ramp.GetComponent<SurfaceTag>() == null) ramp.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            apron.GetComponent<SurfaceTag>()?.EditorSet(SurfaceMaterial.Concrete);

            Approach(group, amber, ivory);
            RampPaint(ramp, amber);
            CargoBoundary(floor, amber);
            Signs(group, truck, ink, ivory);
        }

        private static void Approach(Transform group, Material amber, Material ivory)
        {
            float door = MallLayout.LoadingDoor.x, foot = MallLayout.TruckRear.x - TruckBuilder.RampLength;
            float length = foot - door, centre = (door + foot) * 0.5f, z = MallLayout.LoadingDoor.z;
            foreach (float side in new[] { -1f, 1f })
            {
                Box("ClearHaulLaneEdge", group, new Vector3(centre, 0.012f, z + side * ApproachWidth * 0.5f),
                    new Vector3(length, 0.006f, 0.10f), Quaternion.identity, amber);
                for (int stripe = 0; stripe < 4; stripe++)
                    Box("DockEdgeDash", group, new Vector3(door + stripe + 0.35f, 0.014f, z + side * 1.98f),
                        new Vector3(0.45f, 0.006f, 0.065f), Quaternion.identity, ivory);
            }
            foreach (float x in new[] { door + 1.4f, door + 3.0f })
            foreach (float side in new[] { -1f, 1f })
                Box("ToTruckArrow", group, new Vector3(x - 0.36f, 0.015f, z + side * 0.25f),
                    new Vector3(0.9f, 0.006f, 0.11f), Quaternion.Euler(0f, side * 35f, 0f), amber);
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

        private static void Signs(Transform group, TruckCargo truck, Material ink, Material ivory)
        {
            Vector3 rear = MallLayout.TruckRear;
            Material steel = MeridianArt.Material("EscalatorSteel");
            foreach (float side in new[] { -1f, 1f })
            {
                Box("TailgateSignBracket", group, rear + new Vector3(.12f, 2.94f, side * 1.26f),
                    new Vector3(.06f, .68f, .06f), Quaternion.identity, steel);
                Box("LoadingNoticePost", group, new Vector3(58.45f + side * 1.4f, .77f, rear.z - ApproachWidth * .5f - .26f),
                    new Vector3(.08f, 1.54f, .08f), Quaternion.identity, steel);
            }
            Sign(group, rear + new Vector3(0.08f, 3.05f, 0f), Vector3.left,
                "COLLECTION", TruckBuilder.BayWidth, 0.48f, ink, ivory);
            Sign(group, rear + new Vector3(0.08f, 2.73f, 0f), Vector3.left,
                "LOAD HERE / INSIDE TRUCK", TruckBuilder.BayWidth, 0.22f, ink, ivory);
            Sign(group, new Vector3(58.45f, 1.55f, rear.z - ApproachWidth * 0.5f - 0.26f), Vector3.forward,
                "LOAD FULLY INSIDE THE TRUCK\nCHECK THE HAUL BOARD\nPULL THE LEVER TO LEAVE\nGET ABOARD BEFORE DEPARTURE\nPAID AFTER DEPARTURE",
                3.9f, 1.65f, ink, ivory, 1.3f);
            Sign(group, new Vector3(56.15f, 2.2f, rear.z - 2.65f), Vector3.right,
                "COLLECTION / TRUCK >", 2.5f, 0.36f, ink, ivory);
            Sign(group, truck.Ignition.position + new Vector3(0.10f, 0.38f, 0f), -truck.transform.forward,
                "LEAVE / CASH OUT", 1.1f, 0.18f, ink, ivory);
            Sign(group, truck.transform.TransformPoint(new Vector3(0f, TruckBuilder.FloorHeight + 0.47f, TruckBuilder.BayLength * 0.5f - 0.09f)),
                -truck.transform.forward, "HAUL BOARD ABOVE / PAYMENT ON DEPARTURE", 2.25f, 0.24f, ink, ivory);
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

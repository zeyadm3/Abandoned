using Abandoned.Core;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Replaces the truck's proxy art while retaining the actual haul, ignition and lighting components.</summary>
    public static class CollectionTruck0157
    {
        public const string GroupName = "CollectionTruck_0157";

        public static void Upgrade(Transform truckRoot)
        {
            if (truckRoot == null) return;
            Transform old = truckRoot.Find(GroupName);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            CollectionTruckMaterials.Prepare();

            // Referenced gameplay objects survive; the fitting pass places them on the new deck.
            foreach (Transform child in truckRoot)
                if (child.name is "Cab" or "BayFloor" or "Wall_L" or "Wall_R" or "Wall_Front" or "Wheel")
                {
                    if (child.GetComponent<MeshRenderer>() is MeshRenderer renderer) Object.DestroyImmediate(renderer);
                    if (child.GetComponent<MeshFilter>() is MeshFilter filter) Object.DestroyImmediate(filter);
                }
            CollectionTruckFitRedo.Apply(truckRoot);

            Transform art = GreyboxFactory.Group(GroupName, truckRoot);
            art.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Detailed truck art follows the retained truck body/deck collision proxies; handles, wheel tread and trim do not snag hauled items.");
            var lods = new LOD[3];
            for (int level = 0; level < 3; level++)
            {
                Transform source = GreyboxFactory.Group("Source", art);
                var geometry = new CollectionTruckGeometry(source, level);
                CollectionTruckCabRedo.Build(geometry);
                CollectionTruckChassisRedo.Build(geometry);
                Transform box = GreyboxFactory.Group("CargoSource", source);
                box.localPosition = Vector3.up * CollectionTruckFitRedo.DeckLift;
                CollectionTruckCargoRedo.Build(new CollectionTruckGeometry(box, level));
                Renderer[] combined = geometry.SaveCombined(art, "TruckRedo_LOD" + level);
                Object.DestroyImmediate(source.gameObject);
                lods[level] = new LOD(level == 0 ? .15f : level == 1 ? .055f : .006f, combined);
            }
            var lod = art.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(lods); lod.RecalculateBounds();
            Transform cargoAttachments = GreyboxFactory.Group("CargoBodyAttachments", art);
            cargoAttachments.localPosition = Vector3.up * CollectionTruckFitRedo.DeckLift;
            CollectionTruckCargoRedo.Doors(cargoAttachments, truckRoot.GetComponent<TruckSanctuary>());
            GreyboxFactory.Box("RearViewMonitorFrame", art, new Vector3(.85f, 2.95f, 2.43f),
                new Vector3(.36f, .20f, .06f), CollectionTruckMaterials.Get("Dark"), false);
            Markings(art);
            RoofCollider(art);
            foreach (Transform child in art.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject,
                    IsDoor(child, art) ? 0 : StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
            Debug.Log("[Truck0157b] Replaced truck art with formed cab panels and open windows, profiled tyres, pressed-steel rims, fitted box body and independent PBR finishes. Refit loading deck to 1.05m and retained extraction components.");
        }

        private static bool IsDoor(Transform child, Transform art)
        {
            for (Transform current = child; current != null && current != art; current = current.parent)
                if (current.name.StartsWith("RearDoorHinge_")) return true;
            return false;
        }

        private static void RoofCollider(Transform art)
        {
            Transform proxy = GreyboxFactory.Group("CargoRoofCollision", art);
            var collider = proxy.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, 3.35f, 0); collider.size = new Vector3(2.82f, .10f, 5.22f);
            proxy.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            proxy.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Detailed cargo-body roof uses a simple box for solid overhead clearance.");
        }

        private static void Markings(Transform art)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Text(art, "SOLUTIONS INC.", new Vector3(side * 1.407f, 2.53f, .20f), Quaternion.Euler(0, side * -90f, 0), .105f, new Color(.12f, .20f, .19f));
                Text(art, "CLEARANCE DIVISION", new Vector3(side * 1.407f, 2.26f, .20f), Quaternion.Euler(0, side * -90f, 0), .050f, new Color(.20f, .27f, .25f));
                Text(art, "SC-07", new Vector3(side * 1.257f, 1.36f, 3.39f), Quaternion.Euler(0, side * -90f, 0), .042f, new Color(.14f, .18f, .17f));
            }
            Text(art, "SC 07 INC", new Vector3(0, .70f, 5.274f), Quaternion.Euler(0, 180, 0), .030f, new Color(.12f, .14f, .12f));
            Text(art, "LOAD SECURELY", new Vector3(0, 3.28f, -2.632f), Quaternion.identity, .025f, new Color(.12f, .15f, .13f));
        }

        private static void Text(Transform parent, string label, Vector3 at, Quaternion rotation, float size, Color color)
        {
            Transform holder = GreyboxFactory.Group("Decal_" + label, parent);
            holder.SetLocalPositionAndRotation(at, rotation);
            var text = holder.gameObject.AddComponent<TextMesh>();
            text.text = label; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
            text.characterSize = size; text.fontSize = 96; text.color = color;
            WorldTextMaterial.Apply(text, AssetDatabase.LoadAssetAtPath<Font>(UiContentBuilder.FontPath));
            holder.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}

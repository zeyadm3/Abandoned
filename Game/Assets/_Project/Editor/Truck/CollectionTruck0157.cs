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

    }
}

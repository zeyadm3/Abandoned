using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The company truck (GDD 10), greybox: an open-backed cargo bay with a ramp at the rear, a cab at
    /// the front, the ignition lever on the bay's front wall, a cargo trigger and the ride-along zone.
    /// Local +z runs from the rear (ramp) to the cab.
    /// </summary>
    public static class TruckBuilder
    {
        public const float BayLength = 5f, BayWidth = 2.6f, FloorHeight = 0.4f, WallHeight = 2.2f, RampLength = 2f;

        public static TruckCargo Build(Transform parent, Vector3 rearCenter, Vector3 forward)
        {
            Material body = GetMaterial("Greybox_Truck", new Color(0.7f, 0.22f, 0.2f));
            Material floor = GetMaterial("Greybox_TruckFloor", new Color(0.3f, 0.3f, 0.32f));
            Material lever = GetMaterial("Greybox_Ignition", new Color(1f, 0.85f, 0.1f));

            var root = new GameObject("Truck").transform;
            root.SetParent(parent, false);
            root.position = rearCenter + forward.normalized * (BayLength / 2f);
            root.rotation = Quaternion.LookRotation(forward, Vector3.up);

            float half = BayLength / 2f, w = BayWidth / 2f, top = FloorHeight + WallHeight;
            Box("BayFloor", root, new Vector3(0f, FloorHeight / 2f, 0f), new Vector3(BayWidth, FloorHeight, BayLength), floor);
            Box("Wall_L", root, new Vector3(-w - 0.05f, (FloorHeight + top) / 2f, 0f), new Vector3(0.1f, WallHeight, BayLength), body);
            Box("Wall_R", root, new Vector3(w + 0.05f, (FloorHeight + top) / 2f, 0f), new Vector3(0.1f, WallHeight, BayLength), body);
            Box("Wall_Front", root, new Vector3(0f, (FloorHeight + top) / 2f, half + 0.05f), new Vector3(BayWidth + 0.2f, WallHeight, 0.1f), body);
            Box("Cab", root, new Vector3(0f, 1.6f, half + 1.3f), new Vector3(BayWidth, 2.4f, 2.4f), body);
            foreach (float side in new[] { -1f, 1f })
            foreach (float z in new[] { -half + 0.8f, half + 1.3f })
                Primitive(PrimitiveType.Cylinder, "Wheel", root, new Vector3(side * (w + 0.1f), 0.45f, z), new Vector3(0.9f, 0.15f, 0.9f), floor, withCollider: false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Ramp from the ground up to the bay floor, so players and dragged racks get in.
            float angle = Mathf.Atan2(FloorHeight, RampLength) * Mathf.Rad2Deg;
            var ramp = Box("Ramp", root, new Vector3(0f, FloorHeight / 2f - 0.05f, -half - RampLength / 2f),
                new Vector3(BayWidth, 0.1f, Mathf.Sqrt(RampLength * RampLength + FloorHeight * FloorHeight)), floor);
            ramp.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f); // low end at the rear

            var ignition = Box("Ignition", root, new Vector3(w - 0.35f, FloorHeight + 1.1f, half - 0.2f), new Vector3(0.3f, 0.3f, 0.3f), lever);
            ignition.AddComponent<TruckIgnition>();

            var bay = new GameObject("Bay");
            bay.transform.SetParent(root, false);
            bay.layer = LayerMask.NameToLayer("Ignore Raycast");
            var trigger = bay.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, FloorHeight + WallHeight / 2f, 0f);
            trigger.size = new Vector3(BayWidth, WallHeight, BayLength);

            var cargo = root.gameObject.AddComponent<TruckCargo>();
            cargo.EditorSetup(trigger, new Bounds(new Vector3(0f, 1.5f, 1f), new Vector3(BayWidth + 0.6f, 3.5f, BayLength + 3f)), ignition.transform);
            foreach (Transform piece in root) piece.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return cargo;
        }
    }
}

using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.20.3 truck winch: the upgrade in the truck catalog (append only), RunWinch on the RunState prefab, and
    /// the cable drum at the cargo bay's open end (the end facing the mall), with the ramp's foot 4.6 m out.
    /// </summary>
    public static class Release0203
    {
        [MenuItem("Tools/Abandoned/Mall/Add Truck Winch (0.20.3)")]
        public static void Author()
        {
            CompanyContentBuilder.CreateTruckUpgrades();
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            TruckCargo truck = Object.FindAnyObjectByType<TruckCargo>(FindObjectsInactive.Include);
            var bay = truck != null ? new SerializedObject(truck).FindProperty("bay").objectReferenceValue as BoxCollider : null;
            if (bay == null)
            {
                Debug.LogWarning("[Winch0203] No truck cargo bay found; the winch was not placed.");
                return;
            }
            Transform old = truck.transform.Find("TruckWinch");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            // The bay's long axis; its open end is the one nearer the mall's entrance.
            Transform b = bay.transform;
            Vector3 size = Vector3.Scale(bay.size, b.lossyScale);
            Vector3 axis = size.x >= size.z ? b.right : b.forward;
            float half = Mathf.Max(size.x, size.z) * 0.5f;
            Vector3 centre = b.TransformPoint(bay.center);
            Vector3 endA = centre + axis * half, endB = centre - axis * half;
            Vector3 entrance = new(28f, 0f, 3f);
            Vector3 open = Vector3.Distance(endA, entrance) < Vector3.Distance(endB, entrance) ? endA : endB;
            Vector3 outward = Vector3.ProjectOnPlane(open - centre, Vector3.up).normalized;
            float floor = centre.y - size.y * 0.5f;
            Vector3 foot = open + outward * 4.6f;
            foot.y = Physics.Raycast(foot + Vector3.up * 3f, Vector3.down, out RaycastHit ground, 6f, ~0, QueryTriggerInteraction.Ignore) ? ground.point.y : 0f;

            Transform root = Group("TruckWinch", truck.transform);
            root.position = new Vector3(open.x, floor, open.z) - outward * 0.4f + Vector3.Cross(Vector3.up, outward) * (size.x >= size.z ? size.z : size.x) * 0.35f;
            root.rotation = Quaternion.LookRotation(outward);
            Box("Mount", root, new Vector3(0f, 0.25f, 0f), new Vector3(0.5f, 0.5f, 0.4f), SalvageMaterials.Get("Safety"));
            GameObject drum = Primitive(PrimitiveType.Cylinder, "Drum", root, new Vector3(0f, 0.6f, 0f), new Vector3(0.35f, 0.25f, 0.35f), SalvageMaterials.Get("Metal"), false);
            drum.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Transform footMark = Group("RampFoot", root);
            footMark.position = foot;
            Transform inside = Group("BayCentre", root);
            inside.position = centre;
            var line = root.gameObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.widthMultiplier = 0.025f;
            line.sharedMaterial = SalvageMaterials.Get("Metal");
            line.enabled = false;
            root.gameObject.AddComponent<TruckWinch>().EditorSetup(drum.transform, footMark, inside, line);
            Debug.Log($"[Winch0203] Winch at the bay's open end {open}, ramp foot {foot}.");
            MallSites.SaveMall(scene, "Winch0203");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

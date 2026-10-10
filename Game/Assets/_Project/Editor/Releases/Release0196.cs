using System.Linq;
using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.19.6 HQ break-room toys: a basketball and darts as zero-value utility loot (definitions, prefabs,
    /// catalog), and in the saved HQ's crew wing a wall hoop with its net trigger, a dartboard over a small
    /// crate, and the spawner that keeps the toys there.
    /// </summary>
    public static class Release0196
    {
        public const string GroupName = "_BREAKROOM_TOYS_0196";
        private static readonly Vector3 HoopSize = new(1.2f, 1.0f, 0.6f), BoardSize = new(0.55f, 0.55f, 0.08f);

        [MenuItem("Tools/Abandoned/HQ/Add Break-Room Toys (0.19.6)")]
        public static void Author()
        {
            GameObject ball = Toy("basketball", "Basketball", 0.6f, new Vector3(0.24f, 0.24f, 0.24f), PlaceholderShape.Sphere, new Color(0.9f, 0.45f, 0.15f));
            GameObject dart = Toy("dart", "Dart", 0.05f, new Vector3(0.03f, 0.03f, 0.17f), PlaceholderShape.Cube, new Color(0.85f, 0.15f, 0.15f));
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            SoundBankBuilder.CreateMissing();
            Transform root = HqSites.OpenHq(out var scene, GroupName);
            if (root == null) return;
            var wing = new Rect(HqLayout.HallWidth, 10f, HqLayout.Width - HqLayout.HallWidth, 8f);
            var whole = new Rect(0f, 0f, HqLayout.Width, HqLayout.Depth);
            var hoops = HqSites.WallSites(wing, HoopSize, 2.5f);
            if (hoops.Count == 0) hoops = HqSites.WallSites(whole, HoopSize, 2.5f);
            Transform ballSpot = null;
            if (hoops.Count > 0)
            {
                MallSites.Site h = hoops[0];
                HqSites.Claim(h, HoopSize, 2.5f);
                ballSpot = Hoop(root, h);
            }
            var boards = HqSites.WallSites(wing, new Vector3(0.6f, 1.9f, 0.5f));
            if (boards.Count == 0) boards = HqSites.WallSites(whole, new Vector3(0.6f, 1.9f, 0.5f));
            Transform[] dartSpots = null;
            if (boards.Count > 0)
            {
                MallSites.Site b = hoops.Count > 0 ? boards.OrderBy(s => Vector3.Distance(s.At, hoops[0].At)).First() : boards[0];
                HqSites.Claim(b, new Vector3(0.6f, 1.9f, 0.5f));
                dartSpots = Board(root, b);
            }
            root.gameObject.AddComponent<HqToys>().EditorSetup(ball, dart, ballSpot, dartSpots);
            Debug.Log($"[Toys0196] Hoop {(ballSpot != null ? "placed" : "NOT PLACED")}, dartboard {(dartSpots != null ? "placed" : "NOT PLACED")}.");
            HqSites.SaveHq(scene, "Toys0196");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static GameObject Toy(string id, string name, float weight, Vector3 size, PlaceholderShape shape, Color colour)
        {
            string path = LootCatalogBuilder.PathFor(id);
            var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<LootDefinition>();
                AssetDatabase.CreateAsset(d, path);
            }
            d.EditorSetup(id, name, 0, 0, weight, CarryClass.OneHand, size, Fragility.None, SurfaceMaterial.Plastic, 0.2f, 0f, shape, colour, 1f, 1f);
            d.EditorSetUtility(true);
            EditorUtility.SetDirty(d);
            AssetDatabase.SaveAssets();
            return LootPrefabGenerator.Generate(d);
        }

        private static Transform Hoop(Transform parent, MallSites.Site site)
        {
            Transform root = Group("BasketballHoop", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material white = SalvageMaterials.Get("Wrap"), orange = SalvageMaterials.Get("Safety"), steel = SalvageMaterials.Get("Metal");
            Box("Backboard", root, new Vector3(0f, 3.25f, 0.06f), new Vector3(1.1f, 0.75f, 0.04f), white);
            Box("Bracket", root, new Vector3(0f, 3.0f, 0.2f), new Vector3(0.08f, 0.06f, 0.3f), steel);
            Vector3 rim = new(0f, 3.05f, 0.5f);
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                GameObject seg = Box("Rim", root, rim + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.23f, new Vector3(0.13f, 0.02f, 0.02f), orange);
                seg.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg + 90f, 0f);
            }
            GameObject net = Box("Net", root, rim + Vector3.down * 0.2f, new Vector3(0.36f, 0.12f, 0.36f), white, false);
            net.GetComponent<Renderer>().enabled = false;
            var trigger = net.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            net.AddComponent<HoopGoal>();
            Transform spot = Group("BallSpot", root);
            spot.localPosition = new Vector3(0f, 0.5f, 2.2f);
            return spot;
        }

        private static Transform[] Board(Transform parent, MallSites.Site site)
        {
            Transform root = Group("Darts", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Transform board = Group("Dartboard", root);
            board.localPosition = new Vector3(0f, 1.73f, 0.06f);
            GameObject face = Primitive(PrimitiveType.Cylinder, "Face", board, Vector3.zero, new Vector3(0.46f, 0.02f, 0.46f), SalvageMaterials.Get("Fabric"), false);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Primitive(PrimitiveType.Cylinder, "Bull", board, new Vector3(0f, 0f, 0.025f), new Vector3(0.05f, 0.005f, 0.05f), SalvageMaterials.Get("Safety"), false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var hit = board.gameObject.AddComponent<BoxCollider>();
            hit.size = new Vector3(0.46f, 0.46f, 0.04f);
            board.gameObject.AddComponent<DartBoard>();
            // A crate to keep the darts on, out by the throwing line.
            Box("Crate", root, new Vector3(0.6f, 0.4f, 2.4f), new Vector3(0.5f, 0.8f, 0.4f), SalvageMaterials.Get("Wood"));
            return Enumerable.Range(0, 3).Select(i =>
            {
                Transform t = Group($"DartSpot_{i}", root);
                t.localPosition = new Vector3(0.48f + i * 0.12f, 0.9f, 2.4f);
                t.localRotation = Quaternion.Euler(0f, 90f, 0f);
                return t;
            }).ToArray();
        }
    }
}

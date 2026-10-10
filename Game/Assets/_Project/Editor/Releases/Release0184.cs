using System.Linq;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.18.4 shopping carts: the cart as wheeled utility loot (definition, prefab with a basket of colliders,
    /// catalog entry, rattle quirk), RunCarts on the RunState prefab, and five cart bays on the ground floor's
    /// concourse and food court.
    /// </summary>
    public static class Release0184
    {
        public const string GroupName = "_CART_BAYS_0184";
        private static readonly Vector3 Size = new(0.62f, 1.0f, 0.95f);

        [MenuItem("Tools/Abandoned/Mall/Add Shopping Carts (0.18.4)")]
        public static void Author()
        {
            LootDefinition cart = Definition();
            Prefab(cart);
            LootPrefabGenerator.WriteCatalog();
            NetworkContentBuilder.CreateRunStatePrefab();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Bays(mall);
            MallSites.SaveMall(scene, "Carts0184");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        private static LootDefinition Definition()
        {
            string path = LootCatalogBuilder.PathFor(RunCarts.CartId);
            var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(path);
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<LootDefinition>();
                AssetDatabase.CreateAsset(d, path);
            }
            // Heavy so it is dragged (host-simulated, with whatever rides in it), light so it doesn't slow you.
            d.EditorSetup(RunCarts.CartId, "Shopping Cart", 0, 0, 18f, CarryClass.Heavy, Size, Fragility.None, SurfaceMaterial.Metal,
                0.4f, 0f, PlaceholderShape.Cube, new Color(0.7f, 0.72f, 0.75f), 1f, 1f, 1);
            d.EditorSetUtility(true);
            d.EditorSetWheeled(true);
            d.EditorSetQuirks(LootQuirk.Rattles);
            EditorUtility.SetDirty(d);
            AssetDatabase.SaveAssets();
            return d;
        }

        // The generated loot prefab, with its solid box swapped for a basket (base, four sides) on a chassis.
        private static void Prefab(LootDefinition d)
        {
            LootPrefabGenerator.Generate(d);
            string path = LootPrefabGenerator.PrefabPathFor(d);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            Transform old = root.transform.Find("Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Material wire = SalvageMaterials.Get("Metal"), plastic = SalvageMaterials.Get("Safety"), rubber = SalvageMaterials.Get("Rubber");
            float hx = Size.x * 0.5f, hy = Size.y * 0.5f, hz = Size.z * 0.5f, t = 0.03f;
            Transform art = Group("Cart", root.transform);
            Box("Chassis", art, new Vector3(0f, -hy + 0.2f, 0f), new Vector3(Size.x - 0.08f, 0.06f, Size.z - 0.1f), wire);
            Box("BasketFloor", art, new Vector3(0f, -0.08f, 0f), new Vector3(Size.x, t, Size.z), wire);
            Box("SideL", art, new Vector3(-hx + t * 0.5f, 0.17f, 0f), new Vector3(t, 0.5f, Size.z), wire);
            Box("SideR", art, new Vector3(hx - t * 0.5f, 0.17f, 0f), new Vector3(t, 0.5f, Size.z), wire);
            Box("Front", art, new Vector3(0f, 0.17f, hz - t * 0.5f), new Vector3(Size.x, 0.5f, t), wire);
            Box("Back", art, new Vector3(0f, 0.17f, -hz + t * 0.5f), new Vector3(Size.x, 0.5f, t), wire);
            Box("Handle", art, new Vector3(0f, hy - 0.04f, -hz - 0.08f), new Vector3(Size.x, 0.04f, 0.04f), plastic, false);
            foreach (float x in new[] { -hx + 0.06f, hx - 0.06f })
            foreach (float z in new[] { -hz + 0.08f, hz - 0.08f })
                Primitive(PrimitiveType.Cylinder, "Wheel", art, new Vector3(x, -hy + 0.06f, z), new Vector3(0.1f, 0.02f, 0.1f), rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            // The chassis box reaches the floor through the wheels' height: the cart stands on it.
            Box("Stand", art, new Vector3(0f, -hy + 0.1f, 0f), new Vector3(Size.x - 0.1f, 0.2f, Size.z - 0.12f), wire).GetComponent<Renderer>().enabled = false;
            SerializedWiring.SetLayerRecursively(root, GameLayers.LootLayer);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void Bays(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            var sites = MallSites.FlatSites((t, f) => f == 0 && MallLayout.ZoneAt(t, f)?.Kind != MallLayout.Kind.Store, 0.6f);
            int n = 0;
            foreach (MallSites.Site site in MallSites.Spread(sites, 5, 8f, 184))
            {
                MallSites.Claim(site, new Vector3(1.2f, 0.2f, 1.2f));
                Transform bay = Group($"CartBay_{n++}", root);
                bay.SetPositionAndRotation(site.At, Quaternion.Euler(0f, n * 47f, 0f));
                bay.gameObject.AddComponent<CartBay>();
            }
            Debug.Log($"[Carts0184] {n} cart bays.");
        }
    }
}

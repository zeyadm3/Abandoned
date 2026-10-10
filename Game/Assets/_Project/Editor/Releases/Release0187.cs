using System.Collections.Generic;
using System.Linq;
using Abandoned.Company;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.18.7 loot catalogue and trophy shelf, added to the saved HQ (never regenerated): a steel display shelf
    /// with twelve trophy places and a lectern holding the catalogue ledger, on bare walls of the crew wing
    /// (anywhere in the building if the wing has no room).
    /// </summary>
    public static class Release0187
    {
        public const string GroupName = "_CATALOGUE_0187";
        private static readonly Vector3 ShelfSize = new(2.4f, 2.0f, 0.45f), LecternSize = new(0.7f, 1.15f, 0.6f);

        [MenuItem("Tools/Abandoned/HQ/Add Loot Catalogue and Trophy Shelf (0.18.7)")]
        public static void Author()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LootCatalog>(LootCatalogBuilder.CatalogPath);
            Transform root = HqSites.OpenHq(out var scene, GroupName);
            if (root == null) return;
            var wing = new Rect(HqLayout.HallWidth, 10f, HqLayout.Width - HqLayout.HallWidth, HqLayout.Depth - 10f);
            var whole = new Rect(0f, 0f, HqLayout.Width, HqLayout.Depth);
            MallSites.Site? shelf = Pick(wing, whole, ShelfSize);
            if (shelf != null)
            {
                HqSites.Claim(shelf.Value, ShelfSize);
                Shelf(root, shelf.Value, catalog);
            }
            MallSites.Site? lectern = Pick(wing, whole, LecternSize, shelf?.At);
            if (lectern != null)
            {
                HqSites.Claim(lectern.Value, LecternSize);
                Lectern(root, lectern.Value, catalog);
            }
            Debug.Log($"[Catalogue0187] Trophy shelf {(shelf != null ? $"at {shelf.Value.At}" : "NOT PLACED")}, catalogue {(lectern != null ? $"at {lectern.Value.At}" : "NOT PLACED")}.");
            HqSites.SaveHq(scene, "Catalogue0187");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        // Nearest to the other piece when there is one (they belong together), else the first found.
        private static MallSites.Site? Pick(Rect first, Rect fallback, Vector3 size, Vector3? near = null)
        {
            List<MallSites.Site> sites = HqSites.WallSites(first, size);
            if (sites.Count == 0) sites = HqSites.WallSites(fallback, size);
            if (sites.Count == 0) return null;
            return near.HasValue ? sites.OrderBy(s => Vector3.Distance(s.At, near.Value)).First() : sites.OrderBy(s => s.At.z).ThenBy(s => s.At.x).First();
        }

        private static void Shelf(Transform parent, MallSites.Site site, LootCatalog catalog)
        {
            Transform root = Group("TrophyShelf", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material steel = SalvageMaterials.Get("Metal"), wood = SalvageMaterials.Get("Wood");
            float w = ShelfSize.x, d = ShelfSize.z, z = d * 0.5f + 0.04f;
            foreach (float side in new[] { -1f, 1f })
                Box("Upright", root, new Vector3(side * (w * 0.5f - 0.03f), ShelfSize.y * 0.5f, z), new Vector3(0.05f, ShelfSize.y, d), steel);
            var slots = new List<Transform>();
            float[] heights = { 0.35f, 1.0f, 1.65f };
            foreach (float h in heights)
            {
                Box("Board", root, new Vector3(0f, h, z), new Vector3(w, 0.04f, d), wood);
                for (int i = 0; i < 4; i++)
                {
                    Transform slot = Group($"Slot_{slots.Count}", root);
                    slot.localPosition = new Vector3(-w * 0.375f + i * w * 0.25f, h + 0.02f, z);
                    slots.Add(slot);
                }
            }
            root.gameObject.AddComponent<TrophyShelf>().EditorSetup(catalog, slots.ToArray());
        }

        private static void Lectern(Transform parent, MallSites.Site site, LootCatalog catalog)
        {
            Transform root = Group("LootCatalogue", parent);
            root.SetPositionAndRotation(site.At, site.Facing);
            Material steel = SalvageMaterials.Get("Metal"), paper = SalvageMaterials.Get("Paper"), cover = SalvageMaterials.Get("Fabric");
            float z = LecternSize.z * 0.5f + 0.04f;
            Box("Post", root, new Vector3(0f, 0.5f, z), new Vector3(0.12f, 1f, 0.12f), steel);
            Box("Foot", root, new Vector3(0f, 0.03f, z), new Vector3(0.5f, 0.06f, 0.45f), steel);
            GameObject desk = Box("Desk", root, new Vector3(0f, 1.05f, z), new Vector3(0.6f, 0.04f, 0.45f), steel);
            desk.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            GameObject book = Box("Ledger", root, new Vector3(0f, 1.1f, z), new Vector3(0.48f, 0.05f, 0.34f), cover, false);
            book.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            GameObject pages = Box("Pages", root, new Vector3(0f, 1.13f, z + 0.01f), new Vector3(0.44f, 0.01f, 0.3f), paper, false);
            pages.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            root.gameObject.AddComponent<LootCatalogueBook>().EditorSetup(catalog);
        }
    }
}

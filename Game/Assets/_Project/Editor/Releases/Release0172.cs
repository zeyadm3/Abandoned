using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.17.2 floor types, on the saved mall's collapsible sections: the upper gallery's bridge becomes glass
    /// (and looks it), the dining terrace is tile, the cinema and toy archive upstairs are timber. Everything
    /// else stays concrete. Footsteps follow the surface tag.
    /// </summary>
    public static class Release0172
    {
        [MenuItem("Tools/Abandoned/Mall/Set Floor Types (0.17.2)")]
        public static void Author()
        {
            ReleaseKit.Config<FloorKindConfig>("Assets/_Project/Data/Structure/Resources", FloorKindConfig.ResourcePath);
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Apply(mall);
            MallSites.SaveMall(scene, "Floors0172");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Apply(Transform mall)
        {
            var counts = new Dictionary<FloorKind, int>();
            Material glass = SalvageMaterials.Get("Glass");
            foreach (StructuralSection s in mall.GetComponentsInChildren<StructuralSection>(true))
            {
                if (!s.CanCollapse || s.Type != SectionType.Floor) continue;
                Vector3 at = Centre(s);
                int floor = Mathf.RoundToInt(at.y / MallLayout.StoryHeight);
                var tile = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                FloorKind kind = KindAt(tile, floor);
                s.EditorSetFloorKind(kind);
                EditorUtility.SetDirty(s);
                counts[kind] = counts.TryGetValue(kind, out int n) ? n + 1 : 1;
                if (kind == FloorKind.Concrete) continue;
                SurfaceTag tag = s.GetComponent<SurfaceTag>() ?? s.gameObject.AddComponent<SurfaceTag>();
                tag.EditorSet(kind switch { FloorKind.Glass => SurfaceMaterial.Glass, FloorKind.Timber => SurfaceMaterial.Wood, _ => SurfaceMaterial.Tile });
                if (kind == FloorKind.Glass && s.Visual != null)
                    foreach (Renderer r in s.Visual.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = glass;
            }
            foreach (var pair in counts) Debug.Log($"[Floors0172] {pair.Value} collapsible floor sections are {pair.Key}.");
        }

        private static FloorKind KindAt(Vector2Int tile, int floor)
        {
            if (floor == 1 && MallLayout.IsBridge(tile, floor)) return FloorKind.Glass;
            string zone = MallLayout.ZoneAt(tile, floor)?.Name;
            return zone switch
            {
                "FoodTerrace" or "FoodCourt" => FloorKind.Tile,
                "Cinema" or "ToyArchive" when floor == 1 => FloorKind.Timber,
                _ => FloorKind.Concrete,
            };
        }

        private static Vector3 Centre(StructuralSection s)
        {
            Collider c = s.GetComponentInChildren<Collider>(true);
            return c != null ? c.bounds.center : s.transform.position;
        }
    }
}

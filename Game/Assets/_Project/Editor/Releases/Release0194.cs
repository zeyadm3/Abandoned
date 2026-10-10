using Abandoned.Company;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>0.19.4 difficulty presets: the tunables and a three-way switch on a bare HQ wall near dispatch.</summary>
    public static class Release0194
    {
        public const string GroupName = "_DIFFICULTY_0194";
        private static readonly Vector3 Size = new(0.4f, 0.5f, 0.15f);

        [MenuItem("Tools/Abandoned/HQ/Add Difficulty Switch (0.19.4)")]
        public static void Author()
        {
            ReleaseKit.Config<DifficultyConfig>("Assets/_Project/Data/Core/Resources", DifficultyConfig.ResourcePath);
            Transform root = HqSites.OpenHq(out var scene, GroupName);
            if (root == null) return;
            var near = new Rect(HqLayout.HallWidth - 6f, 0f, HqLayout.Width - HqLayout.HallWidth + 6f, 14f);
            var sites = HqSites.WallSites(near, Size, 1.1f);
            if (sites.Count == 0) sites = HqSites.WallSites(new Rect(0f, 0f, HqLayout.Width, HqLayout.Depth), Size, 1.1f);
            if (sites.Count == 0)
            {
                Debug.LogWarning("[Difficulty0194] No HQ wall for the difficulty switch.");
                return;
            }
            MallSites.Site site = sites[0];
            HqSites.Claim(site, Size, 1.1f);
            Transform box = Group("DifficultySwitch", root);
            box.SetPositionAndRotation(site.At, site.Facing);
            Box("Plate", box, new Vector3(0f, 1.35f, 0.06f), new Vector3(0.4f, 0.5f, 0.08f), SalvageMaterials.Get("Paint"));
            foreach ((float angle, Color c) in new[] { (-60f, new Color(0.3f, 0.8f, 0.4f)), (0f, new Color(0.9f, 0.8f, 0.3f)), (60f, new Color(0.9f, 0.25f, 0.2f)) })
            {
                GameObject dot = Primitive(PrimitiveType.Sphere, "Mark", box, Vector3.zero, Vector3.one * 0.04f, GetMaterial($"Greybox_Difficulty{angle}", c), false);
                dot.transform.localPosition = new Vector3(Mathf.Sin(-angle * Mathf.Deg2Rad) * -0.13f, 1.35f + Mathf.Cos(angle * Mathf.Deg2Rad) * 0.13f, 0.11f);
            }
            Transform knob = Group("Knob", box);
            knob.localPosition = new Vector3(0f, 1.35f, 0.12f);
            Box("Pointer", knob, new Vector3(0f, 0.06f, 0f), new Vector3(0.025f, 0.12f, 0.02f), SalvageMaterials.Get("Metal"), false);
            box.gameObject.AddComponent<DifficultySwitch>().EditorSetup(knob);
            Debug.Log($"[Difficulty0194] Difficulty switch at {site.At}.");
            HqSites.SaveHq(scene, "Difficulty0194");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

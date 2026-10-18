using System.Linq;
using Abandoned.Loot;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The rebuild's check that no loot model stands in the level as scenery (keep it at 0), and the F1
    /// report object that shows the same count in play.
    /// </summary>
    public static class LootLookalikeAudit
    {
        public const string ReportName = "LootLookalikeReport";

        /// <summary>Logs each look-alike still standing and the total; returns the total.</summary>
        public static int Run(Transform level, string levelName)
        {
            int left = 0;
            foreach ((GameObject item, string id) in LootLookalikePass.Instances(level))
            {
                Debug.LogError($"[MallValidation] {levelName}: static loot look-alike {Path(item.transform)} ({id}).");
                left++;
            }
            foreach (string name in new[] { SalvageMallDensity.GroupName, MallExpansion0156.GroupName })
            {
                Transform layer = level.Find(name);
                if (layer == null) continue;
                foreach (var holder in LootLookalikePass.Holders(layer))
                {
                    Debug.LogError($"[MallValidation] {levelName}: static loot look-alike {Path(holder.transform)}.");
                    left++;
                }
            }
            Debug.Log($"[MallValidation] {levelName}: static loot look-alikes: {left}.");
            return left;
        }

        /// <summary>Writes the F1 report (swaps made, look-alikes left) onto the level, replacing an older one.</summary>
        public static void Report(Transform level, Bounds[] swapped, int left)
        {
            Transform old = level.Find(ReportName);
            var report = old != null ? old.GetComponent<LootLookalikeDebug>() : null;
            if (report == null)
            {
                if (old != null) Object.DestroyImmediate(old.gameObject);
                var holder = new GameObject(ReportName);
                holder.transform.SetParent(level, false);
                report = holder.AddComponent<LootLookalikeDebug>();
            }
            report.EditorSetup(report.Spots.Concat(swapped).ToArray(), left);
        }

        private static string Path(Transform node)
        {
            string path = node.name;
            for (Transform parent = node.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;
            return path;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Every machine: the rubble a collapse leaves behind stays for the rest of the job (2026-10-08). Each
    /// fallen section drops one solid <see cref="RubbleMound"/>; when the section under a mound (or under
    /// settled chunks) falls in turn, they drop again. A restore (next run) clears them.
    /// </summary>
    public partial class StructureSimulation
    {
        private readonly Dictionary<StructuralSection, RubbleMound> mounds = new();
        private readonly List<StructuralSection> redrop = new();

        private void PlaceRubble(StructuralSection fallen)
        {
            Bounds surface = fallen.SurfaceBounds;
            // Loose chunks lying on it go down with it.
            DebrisSpawner.Unsettle(new Bounds(surface.center + Vector3.up * 1.5f, surface.size + new Vector3(0.4f, 3f, 0.4f)));

            redrop.Clear();
            foreach (KeyValuePair<StructuralSection, RubbleMound> m in mounds)
                if (m.Value != null && m.Value.Support == fallen) redrop.Add(m.Key);
            foreach (StructuralSection s in redrop) RemoveMound(s);

            Drop(fallen);
            foreach (StructuralSection s in redrop) Drop(s);
        }

        private void Drop(StructuralSection fallen)
        {
            if (fallen == null || !fallen.IsCollapsed || mounds.ContainsKey(fallen)) return;
            RubbleMound mound = RubbleMound.Drop(fallen, config);
            if (mound != null) mounds[fallen] = mound;
        }

        private void OnSectionRestored(StructuralSection section) => RemoveMound(section);

        private void RemoveMound(StructuralSection section)
        {
            if (!mounds.TryGetValue(section, out RubbleMound mound)) return;
            mounds.Remove(section);
            if (mound == null) return;
            // Out of physics now, not at the end of the frame: a re-drop must not land on its own old heap.
            mound.gameObject.SetActive(false);
            Destroy(mound.gameObject);
        }
    }
}

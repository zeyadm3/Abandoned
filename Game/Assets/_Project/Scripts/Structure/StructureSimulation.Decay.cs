using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Host hooks for the building's own decay (Extraction.BuildingDecay): crack a section so it groans and
    /// shows it, then send it into the usual Failing sag. Collapse, cascades, sync and NavMesh carving are the
    /// normal ones; nothing here skips the warning.
    /// </summary>
    public partial class StructureSimulation
    {
        /// <summary>Host: can the building's decay pick this section now?</summary>
        public bool CanDecay(StructuralSection s) =>
            HasAuthority && s != null && s.CanCollapse && !s.IsCollapsed && s.Stage != StructuralStage.Failing;

        /// <summary>Host: crack it down to <paramref name="leaveFraction"/> of its health (it creaks and shows cracks).</summary>
        public bool Weaken(StructuralSection s, float leaveFraction)
        {
            if (!CanDecay(s)) return false;
            float amount = s.Health - s.MaxHealth * Mathf.Clamp01(leaveFraction);
            if (amount > 0f) s.Damage(amount);
            return true;
        }

        /// <summary>Host: take its last health so it starts Failing (and falls after the usual sag).</summary>
        public bool StartFailing(StructuralSection s)
        {
            if (!CanDecay(s)) return false;
            s.Damage(s.Health);
            return s.Stage == StructuralStage.Failing || s.IsCollapsed;
        }
    }
}

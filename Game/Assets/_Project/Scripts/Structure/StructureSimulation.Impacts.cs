using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Impacts on sections: physics hits gathered per step and point impacts (landings), shared out at the next step.</summary>
    public partial class StructureSimulation
    {
        /// <summary>Host: a rigidbody hit a section this step; applied (shared) at the start of the next step.</summary>
        public void ReportImpact(StructuralSection section, Rigidbody body, float momentum)
        {
            if (!pendingImpacts.TryGetValue(body, out var entry))
            {
                entry = (0f, new List<StructuralSection>());
                pendingOrder.Add(body);
            }
            if (!entry.sections.Contains(section)) entry.sections.Add(section);
            pendingImpacts[body] = (Mathf.Max(entry.momentum, momentum), entry.sections);
        }

        private void ApplyPendingImpacts()
        {
            foreach (Rigidbody body in pendingOrder)
            {
                (float momentum, List<StructuralSection> hit) = pendingImpacts[body];
                foreach (StructuralSection s in hit) s.ApplyImpact(momentum / hit.Count);
            }
            pendingImpacts.Clear();
            pendingOrder.Clear();
        }

        /// <summary>Host: a non-physics impact (a player landing) at a point; hits the section underneath.</summary>
        private void OnPointImpact(Vector3 position, float momentum)
        {
            if (!HasAuthority) return;
            StructuralSection below = SectionBelow(position + Vector3.up * RayLift, RayLift + 0.5f);
            if (below != null) below.ApplyImpact(momentum);
        }
    }
}

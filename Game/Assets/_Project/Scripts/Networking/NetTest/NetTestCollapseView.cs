using System;
using System.Collections.Generic;
using Abandoned.Structure;

namespace Abandoned.Networking
{
    /// <summary>What one machine saw in the 'collapse' scenario: its collapsed sections and how far its own player fell.</summary>
    [Serializable]
    public class NetTestCollapseView
    {
        public ulong observer;
        /// <summary>This machine's structure is a mirror of the host's (clients) or runs it (host).</summary>
        public bool mirror;
        public bool stoodOnTarget;
        public float startHeight;
        public float lowestHeight;
        public List<NetTestCollapsedSection> collapsed = new();

        public float Fall => startHeight - lowestHeight;

        public static NetTestCollapseView Take(ulong observer, StructureSimulation simulation)
        {
            var view = new NetTestCollapseView { observer = observer, mirror = simulation != null && simulation.IsMirror };
            if (simulation == null) return view;
            foreach (StructuralSection s in simulation.Sections)
                if (s.IsCollapsed)
                    view.collapsed.Add(new NetTestCollapsedSection { id = s.Id, name = s.name, seed = s.CollapseSeed, collidersOff = !s.CollidersEnabled });
            return view;
        }
    }
}

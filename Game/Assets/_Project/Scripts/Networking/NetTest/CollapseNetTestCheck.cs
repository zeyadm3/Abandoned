using System.Collections.Generic;
using System.Linq;

namespace Abandoned.Networking
{
    /// <summary>
    /// The 'collapse' verdict, free of networking so EditMode tests can feed it good and bad data.
    /// The host runs the structure, so its view is the truth: the target must have collapsed there,
    /// every client must mirror exactly the same collapsed sections with the same seeds and colliders
    /// off, and every client that stood on the target must have fallen.
    /// </summary>
    public static class CollapseNetTestCheck
    {
        public const ulong HostId = 0;

        public static List<string> Verify(IReadOnlyList<NetTestCollapseView> views, int expectedPlayers, int targetId, float minFall)
        {
            var errors = new List<string>();
            if (views.Count != expectedPlayers) errors.Add($"expected a view from each of {expectedPlayers} machines, got {views.Count}");
            NetTestCollapseView host = views.FirstOrDefault(v => v.observer == HostId);
            if (host == null)
            {
                errors.Add("no view from the host");
                return errors;
            }
            if (host.mirror) errors.Add("the host's structure is a mirror; the host must run it");
            NetTestCollapsedSection target = host.collapsed.FirstOrDefault(c => c.id == targetId);
            if (target == null) errors.Add($"section {targetId} never collapsed on the host");
            foreach (NetTestCollapsedSection c in host.collapsed.Where(c => !c.collidersOff))
                errors.Add($"the host still has colliders on collapsed section {c.id} ({c.name})");

            foreach (NetTestCollapseView view in views.Where(v => v.observer != HostId))
            {
                string who = $"client {view.observer}";
                if (!view.mirror) errors.Add($"{who}'s structure isn't mirroring the host (it may be simulating its own)");
                foreach (NetTestCollapsedSection truth in host.collapsed)
                {
                    NetTestCollapsedSection seen = view.collapsed.FirstOrDefault(c => c.id == truth.id);
                    if (seen == null) errors.Add($"{who} never saw section {truth.id} ({truth.name}) collapse");
                    else
                    {
                        if (seen.seed != truth.seed) errors.Add($"{who} broke section {truth.id} with seed {seen.seed}, the host with {truth.seed}");
                        if (seen.name != truth.name) errors.Add($"{who}'s section {truth.id} is '{seen.name}', the host's '{truth.name}' (layouts differ)");
                        if (!seen.collidersOff) errors.Add($"{who} still has colliders on collapsed section {truth.id}");
                    }
                }
                foreach (NetTestCollapsedSection extra in view.collapsed.Where(c => host.collapsed.All(h => h.id != c.id)))
                    errors.Add($"{who} shows section {extra.id} ({extra.name}) collapsed, the host doesn't");
                if (view.stoodOnTarget && view.Fall < minFall)
                    errors.Add($"{who} stood on the collapsing section but fell only {view.Fall:0.00} m (need {minFall:0.00})");
            }
            if (!views.Any(v => v.observer != HostId && v.stoodOnTarget)) errors.Add("no client stood on the target section");
            return errors;
        }
    }
}

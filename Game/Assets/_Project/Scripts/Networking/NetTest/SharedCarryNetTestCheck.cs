using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The 'sharedcarry' verdict, free of networking so EditMode tests can feed it good and bad data.
    /// The host simulates a shared carry, so its view is the truth: both carriers must have got a
    /// handle and seen the item lifted, the host must have had the full crew, lifted it, received their
    /// hold targets and moved it, and at the end every machine must see it free at the host's position.
    /// </summary>
    public static class SharedCarryNetTestCheck
    {
        public const ulong HostId = 0;

        public static List<string> Verify(IReadOnlyList<NetTestLootView> views, IReadOnlyList<NetTestSharedCarryAction> actions,
            NetTestSharedCarryHostView host, int expectedPlayers, int expectedCarriers, float tolerance, float minMove, float minLift)
        {
            var errors = new List<string>();
            if (host == null)
            {
                errors.Add("no shared-carry measurements from the host");
                return errors;
            }
            string rack = $"#{host.item}";
            if (actions.Count != expectedCarriers) errors.Add($"expected {expectedCarriers} carriers to report, got {actions.Count}");
            foreach (NetTestSharedCarryAction a in actions)
            {
                if (!a.grabbed) errors.Add($"client {a.client} never got a handle of {rack}");
                if (!a.sawLifted) errors.Add($"client {a.client} never saw {rack} lifted by the full crew on its own machine");
                if (!a.letGo) errors.Add($"the host never confirmed client {a.client} letting go of {rack}");
            }
            if (host.maxCarriers < expectedCarriers) errors.Add($"the host only ever saw {host.maxCarriers} carriers on {rack} (need {expectedCarriers})");
            if (host.maxLift < minLift) errors.Add($"{rack} rose only {host.maxLift:0.00} m on the host (need {minLift:0.00})");
            if (host.targetsReceived == 0) errors.Add($"the host received no hold targets for {rack} from the carriers");
            if (host.endCarriers != 0) errors.Add($"{rack} still has {host.endCarriers} carrier(s) on the host after everyone let go");
            Vector3 moved = host.end - host.start;
            moved.y = 0f;
            if (moved.magnitude < minMove) errors.Add($"{rack} moved only {moved.magnitude:0.00} m on the host (need {minMove:0.00})");

            if (views.Count != expectedPlayers) errors.Add($"expected a view from each of {expectedPlayers} machines, got {views.Count}");
            NetTestLootSnapshot truth = views.FirstOrDefault(v => v.observer == HostId)?.items.FirstOrDefault(i => i.id == host.item);
            if (truth == null)
            {
                errors.Add($"the host's view doesn't include {rack}");
                return errors;
            }
            if (truth.owner != HostId) errors.Add($"{rack} ended owned by c{truth.owner}; shared carries stay host-simulated");
            foreach (NetTestLootView view in views.Where(v => v.observer != HostId))
            {
                NetTestLootSnapshot seen = view.items.FirstOrDefault(i => i.id == host.item);
                if (seen == null)
                {
                    errors.Add($"machine {view.observer} is missing {rack}");
                    continue;
                }
                float off = Vector3.Distance(seen.position, truth.position);
                if (off > tolerance)
                    errors.Add($"machine {view.observer} sees {rack} at {seen.position}, {off:0.00} m from the host's {truth.position} (tolerance {tolerance:0.00})");
                if (seen.owner != truth.owner) errors.Add($"machine {view.observer} sees {rack} owned by c{seen.owner}, host c{truth.owner}");
            }
            return errors;
        }
    }
}

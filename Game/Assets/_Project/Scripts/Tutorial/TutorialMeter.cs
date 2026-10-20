using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Host: how far along the current lesson the crew is, as a 0..1 bar and the numbers behind it. Lessons with
    /// a place fill as the nearest player closes in (from wherever they were when the target appeared); carrying
    /// lessons fill as the item nears where it goes; the floor lesson fills with the floor's load, the quota with
    /// the haul. Personal lessons are measured on each player's machine (<see cref="TutorialWatch"/>).
    /// </summary>
    public static class TutorialMeter
    {
        /// <summary>Within this many metres the target counts as reached.</summary>
        private const float Arrive = 1.2f;

        public static void Measure(RunTraining t, TutorialStepDefinition s, TutorialGuideSpec spec, TutorialGuideState state, ref TutorialGuide g)
        {
            float f = 0f;
            LootItem item = string.IsNullOrEmpty(s.Key) ? null : t.Item(s.Key);
            bool held = RunTraining.IsHeld(item);
            switch (s.Check)
            {
                case TutorialCheck.Read:
                    f = t.StepElapsed / (s.Amount > 0f ? s.Amount : TutorialConfig.Current.ReadSeconds);
                    break;
                case TutorialCheck.EnterArea:
                    f = Approach(t, ref g, state);
                    break;
                case TutorialCheck.HoldItem:
                case TutorialCheck.PocketItem:
                case TutorialCheck.HasGear:
                case TutorialCheck.DoorShut:
                case TutorialCheck.DoorOpen:
                case TutorialCheck.Breaker:
                case TutorialCheck.Hide:
                    f = 0.9f * Approach(t, ref g, state);
                    break;
                case TutorialCheck.DropItem:
                    f = item == null || !held ? 1f : g.Kind != TutorialGuideKind.None ? 0.9f * Approach(t, ref g, state) : 0.5f;
                    break;
                case TutorialCheck.MoveItem:
                    f = Moved(t, s, item, held, ref g, state);
                    break;
                case TutorialCheck.ItemInArea:
                case TutorialCheck.ItemInTruck:
                    f = Carried(t, item, held, ref g, state);
                    break;
                case TutorialCheck.SectionStage:
                    f = FloorLoad(s, ref g);
                    break;
                case TutorialCheck.OffSection:
                    float need = Mathf.Max(0.5f, s.Amount), off = t.AnyoneOn(TutorialSection.Find(s.Key)) ? 0f : Mathf.Min(need, t.SecondsOffSection);
                    Set(ref g, TutorialReadout.Seconds, off, need);
                    f = off / need;
                    break;
                case TutorialCheck.SectionBraced:
                    f = TutorialSection.Find(s.Key) is StructuralSection braced && braced.Reinforcement > 1f ? 1f
                        : spec.Kind == TutorialGuideKind.Brace ? 0.4f + 0.55f * Approach(t, ref g, state) : 0.4f * Approach(t, ref g, state);
                    break;
                case TutorialCheck.SectionDown:
                    f = TutorialSection.Find(s.Key) is StructuralSection down && !down.IsCollapsed ? 0f : 1f;
                    break;
                case TutorialCheck.ThreatGone:
                    f = t.VisitorLeaving;
                    break;
                case TutorialCheck.Haul:
                    f = Haul(t, s, ref g);
                    break;
                case TutorialCheck.Lever:
                    f = Aboard(t, ref g);
                    break;
            }
            g.Progress = (ushort)Mathf.RoundToInt(Mathf.Clamp01(f) * 1000f);
        }

        // The nearest player closing on the target, from where they stood when it appeared.
        private static float Approach(RunTraining t, ref TutorialGuide g, TutorialGuideState state)
        {
            if (g.Kind == TutorialGuideKind.None) return 0f;
            Vector3 target = Where(t, g);
            float d = TutorialGuides.NearestPlayer(t, target, out NetworkPlayer _);
            if (d == float.MaxValue) return 0f;
            if (state.ApproachFrom < 0f) state.ApproachFrom = Mathf.Max(d, Arrive + 1f);
            state.ApproachFrom = Mathf.Max(state.ApproachFrom, d);
            Set(ref g, TutorialReadout.MetresLeft, Mathf.Max(0f, d), 0f);
            return Mathf.Clamp01(1f - (d - Arrive) / Mathf.Max(0.5f, state.ApproachFrom - Arrive));
        }

        private static float Moved(RunTraining t, TutorialStepDefinition s, LootItem item, bool held, ref TutorialGuide g, TutorialGuideState state)
        {
            if (item == null) return 0f;
            float need = Mathf.Max(0.5f, s.Amount);
            float moved = Vector3.Distance(item.transform.position, t.ItemStart(s.Key));
            if (!held && moved < 0.25f) return 0.3f * Approach(t, ref g, state);
            Set(ref g, TutorialReadout.Metres, Mathf.Min(moved, need), need);
            return 0.3f + 0.7f * Mathf.Clamp01(moved / need);
        }

        // Holding it: the item closing on where it goes. Not yet: the nearest player closing on the item.
        private static float Carried(RunTraining t, LootItem item, bool held, ref TutorialGuide g, TutorialGuideState state)
        {
            if (item == null) return 0f;
            if (!held || g.Kind == TutorialGuideKind.None) return 0.3f * Approach(t, ref g, state);
            float d = Vector3.Distance(item.transform.position, g.Point);
            if (state.CarryFrom < 0f) state.CarryFrom = Mathf.Max(d, Arrive + 1f);
            state.CarryFrom = Mathf.Max(state.CarryFrom, d);
            Set(ref g, TutorialReadout.MetresLeft, d, 0f);
            return 0.3f + 0.7f * Mathf.Clamp01(1f - (d - Arrive) / Mathf.Max(0.5f, state.CarryFrom - Arrive));
        }

        private static float FloorLoad(TutorialStepDefinition s, ref TutorialGuide g)
        {
            if (TutorialSection.Find(s.Key) is not StructuralSection floor) return 0f;
            float stage = (int)floor.Stage / Mathf.Max(1f, s.Amount);
            Set(ref g, TutorialReadout.LoadPercent, Mathf.Round(floor.LoadRatio * 100f), 100f);
            return Mathf.Max(stage, Mathf.Min(1f, floor.LoadRatio) * 0.95f);
        }

        private static float Haul(RunTraining t, TutorialStepDefinition s, ref TutorialGuide g)
        {
            RunState run = t.Run;
            if (run == null || !run.IsSpawned) return 0f;
            float goal = s.Amount > 0f ? s.Amount : run.State.Quota;
            Set(ref g, TutorialReadout.Dollars, run.State.Haul, goal);
            return goal > 0f ? run.State.Haul / goal : 1f;
        }

        // "Everyone into the cargo bay, then the lever": the bar fills as the crew climbs aboard.
        private static float Aboard(RunTraining t, ref TutorialGuide g)
        {
            TruckCargo truck = TruckCargo.Current;
            if (truck == null) return 0f;
            int living = 0, aboard = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != t.NetworkManager || p.IsDead) continue;
                living++;
                if (truck.Carries(p.transform.position)) aboard++;
            }
            Set(ref g, TutorialReadout.Aboard, aboard, living);
            return living > 0 ? 0.9f * aboard / living : 0f;
        }

        private static Vector3 Where(RunTraining t, TutorialGuide g)
        {
            if (g.Follow != 0 && t.NetworkManager != null && t.NetworkManager.SpawnManager != null &&
                t.NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(g.Follow, out Unity.Netcode.NetworkObject o) && o != null)
                return o.transform.position;
            return g.Point;
        }

        private static void Set(ref TutorialGuide g, TutorialReadout readout, float value, float goal)
        {
            g.Readout = readout;
            g.Value = value;
            g.Goal = goal;
        }
    }
}

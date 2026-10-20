using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Host: the guide for the current lesson. Where to go comes from the lesson's Guide line or, left empty, from
    /// its check and how far the crew has got (the vase: first the vase, then the truck once someone holds it).
    /// The target is found in the house (areas, spots, the training salvage, crates, doors, the breaker, closets,
    /// floors, the truck) and <see cref="TutorialMeter"/> fills in the progress.
    /// </summary>
    public static class TutorialGuides
    {
        public const string TruckBaySpot = "truck_bay", TruckRampSpot = "truck_ramp";

        public static TutorialGuide Compute(RunTraining training, TutorialStepDefinition step, TutorialGuideState state)
        {
            var guide = new TutorialGuide();
            if (training.VisitorThreat is Threats.Threat visitor && visitor.IsSpawned) guide.Danger = visitor.NetworkObjectId;
            TutorialGuideSpec spec = SpecFor(training, step);
            state.Aim(spec.Id);
            if (spec.Kind != TutorialGuideKind.None && !Resolve(training, spec, ref guide)) spec = TutorialGuideSpec.Nothing;
            guide.Kind = spec.Kind;
            guide.Target = spec.Target;
            guide.Key = spec.Key.Length <= 29 ? spec.Key : spec.Key.Substring(0, 29);
            TutorialMeter.Measure(training, step, spec, state, ref guide);
            return Quantise(guide);
        }

        /// <summary>The lesson's own Guide line, or the guide worked out from its check; then any phase change.</summary>
        public static TutorialGuideSpec SpecFor(RunTraining training, TutorialStepDefinition s)
        {
            TutorialGuideSpec spec;
            if (!string.IsNullOrWhiteSpace(s.Guide))
            {
                if (!TutorialGuideSpec.TryParse(s.Guide, out spec, out _)) return TutorialGuideSpec.Nothing;
            }
            else spec = Auto(training, s);
            // Something to pick up stops being pointed at once it is in someone's hands.
            if (spec.Kind == TutorialGuideKind.Take && spec.Target == TutorialGuideTarget.Item && RunTraining.IsHeld(training.Item(spec.Key)))
                return TutorialGuideSpec.Nothing;
            return spec;
        }

        private static TutorialGuideSpec Auto(RunTraining training, TutorialStepDefinition s)
        {
            bool held = RunTraining.IsHeld(training.Item(s.Key));
            switch (s.Check)
            {
                case TutorialCheck.EnterArea:
                    return new TutorialGuideSpec(TutorialGuideKind.Go, TutorialGuideTarget.Area, s.Area);
                case TutorialCheck.HoldItem:
                case TutorialCheck.PocketItem:
                case TutorialCheck.MoveItem:
                    return string.IsNullOrEmpty(s.Key) ? TutorialGuideSpec.Nothing : Take(s.Key);
                case TutorialCheck.ItemInArea:
                    return held ? new TutorialGuideSpec(TutorialGuideKind.Go, TutorialGuideTarget.Area, s.Area) : Take(s.Key);
                case TutorialCheck.ItemInTruck:
                    return held ? Load : Take(s.Key);
                case TutorialCheck.HasGear:
                    return new TutorialGuideSpec(TutorialGuideKind.Use, TutorialGuideTarget.Crate, s.Key);
                case TutorialCheck.DoorShut:
                case TutorialCheck.DoorOpen:
                    return new TutorialGuideSpec(TutorialGuideKind.Use, TutorialGuideTarget.Door, Mathf.RoundToInt(s.Amount).ToString());
                case TutorialCheck.SectionStage:
                    return new TutorialGuideSpec(TutorialGuideKind.Go, TutorialGuideTarget.Section, s.Key);
                case TutorialCheck.OffSection:
                case TutorialCheck.SectionDown:
                    return new TutorialGuideSpec(TutorialGuideKind.Away, TutorialGuideTarget.Section, s.Key);
                case TutorialCheck.SectionBraced:
                    return AnyoneCarries(training, "support_jack")
                        ? new TutorialGuideSpec(TutorialGuideKind.Brace, TutorialGuideTarget.Section, s.Key)
                        : new TutorialGuideSpec(TutorialGuideKind.Use, TutorialGuideTarget.Crate, "support_jack");
                case TutorialCheck.Breaker:
                    return new TutorialGuideSpec(TutorialGuideKind.Use, TutorialGuideTarget.Breaker);
                case TutorialCheck.Hide:
                    return new TutorialGuideSpec(TutorialGuideKind.Hide, TutorialGuideTarget.Closet, "0");
                case TutorialCheck.ThreatGone:
                    return new TutorialGuideSpec(TutorialGuideKind.Wait);
                case TutorialCheck.Haul:
                    return training.AnyTrainingLootInHands() ? Load : training.NearestUnloadedKey() is string key ? Take(key) : Load;
                case TutorialCheck.Lever:
                    return new TutorialGuideSpec(TutorialGuideKind.Use, TutorialGuideTarget.Lever);
                default:
                    return TutorialGuideSpec.Nothing;
            }
        }

        private static TutorialGuideSpec Take(string key) => new(TutorialGuideKind.Take, TutorialGuideTarget.Item, key);
        private static readonly TutorialGuideSpec Load = new(TutorialGuideKind.Load, TutorialGuideTarget.Truck);

        // Where the target is now; false when the house doesn't have it (the guide then shows nothing).
        private static bool Resolve(RunTraining training, TutorialGuideSpec spec, ref TutorialGuide guide)
        {
            switch (spec.Target)
            {
                case TutorialGuideTarget.Area:
                    if (TutorialArea.Find(spec.Key) is not TutorialArea area) return false;
                    guide.Point = area.transform.position;
                    return true;
                case TutorialGuideTarget.Spot:
                    if (TutorialSpot.Find(spec.Key) is not TutorialSpot spot) return false;
                    guide.Point = spot.transform.position;
                    return true;
                case TutorialGuideTarget.Item:
                    if (training.Item(spec.Key) is not LootItem item) return false;
                    guide.Follow = item.TryGetComponent(out Unity.Netcode.NetworkObject net) && net.IsSpawned ? net.NetworkObjectId : 0;
                    guide.Point = item.transform.position;
                    return true;
                case TutorialGuideTarget.Truck:
                    return TruckPoint(ref guide, false);
                case TutorialGuideTarget.Lever:
                    return TruckPoint(ref guide, true);
                case TutorialGuideTarget.Crate:
                    foreach (TutorialGearCrate crate in TutorialGearCrate.All)
                        if (crate != null && crate.EquipmentId == spec.Key)
                        {
                            guide.Point = crate.transform.position;
                            return true;
                        }
                    return false;
                case TutorialGuideTarget.Door:
                    if (!int.TryParse(spec.Key, out int door) || SlidingDoor.ByIndex(door) is not SlidingDoor sliding) return false;
                    guide.Point = sliding.Centre;
                    return true;
                case TutorialGuideTarget.Breaker:
                    foreach (BreakerPanel panel in BreakerPanel.All)
                        if (panel != null)
                        {
                            guide.Point = panel.transform.position;
                            return true;
                        }
                    return false;
                case TutorialGuideTarget.Closet:
                    if (!int.TryParse(spec.Key, out int closet) || HidingLocker.ByIndex(closet) is not HidingLocker locker) return false;
                    guide.Point = locker.ExitPoint;
                    return true;
                case TutorialGuideTarget.Section:
                    if (TutorialSection.Find(spec.Key) is not StructuralSection section) return false;
                    Bounds b = section.SurfaceBounds;
                    guide.Point = new Vector3(b.center.x, b.max.y, b.center.z);
                    return true;
                default:
                    return false;
            }
        }

        // The truck's object sits at the middle of its bay at ground level, facing the cab (Editor TruckBuilder:
        // a 5 m bay, its deck 1.05 m up, a 4.4 m ramp off the back). Spots placed by the house builder win.
        private const float TruckDeck = 1.05f, TruckRampFoot = 2.5f + 4.4f + 0.3f;

        private static bool TruckPoint(ref TutorialGuide guide, bool lever)
        {
            TruckCargo truck = TruckCargo.Current;
            if (lever && truck != null && truck.Ignition != null) guide.Point = truck.Ignition.position;
            else if (TutorialSpot.Find(TruckBaySpot) is TutorialSpot bay) guide.Point = bay.transform.position;
            else if (truck != null) guide.Point = truck.transform.position + Vector3.up * TruckDeck;
            else return false;
            if (TutorialSpot.Find(TruckRampSpot) is TutorialSpot ramp) guide.Via = ramp.transform.position;
            else if (truck != null) guide.Via = truck.transform.position - truck.transform.forward * TruckRampFoot;
            else return true;
            guide.HasVia = true;
            return true;
        }

        public static bool AnyoneCarries(RunTraining training, string equipmentId)
        {
            foreach (PlayerEquipment e in PlayerEquipment.All)
            {
                if (e == null || !e.IsSpawned || e.NetworkManager != training.NetworkManager) continue;
                for (int slot = 0; slot < EquipState.SlotCount; slot++)
                    if (e.InSlot(slot) is EquipmentDefinition d && d.Id == equipmentId) return true;
            }
            return false;
        }

        /// <summary>Host: the living player nearest to a point, and how far.</summary>
        public static float NearestPlayer(RunTraining training, Vector3 point, out NetworkPlayer nearest)
        {
            nearest = null;
            float best = float.MaxValue;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.NetworkManager != training.NetworkManager || p.IsDead) continue;
                float d = Vector3.Distance(p.transform.position, point);
                if (d >= best) continue;
                best = d;
                nearest = p;
            }
            return best;
        }

        // Small moves aren't worth sending: the marker follows a moving item on each machine anyway.
        private static TutorialGuide Quantise(TutorialGuide g)
        {
            g.Point = Round(g.Point, g.Follow != 0 ? 0.5f : 0.05f);
            g.Via = Round(g.Via, 0.05f);
            g.Value = Mathf.Round(g.Value * 10f) / 10f;
            g.Goal = Mathf.Round(g.Goal * 10f) / 10f;
            return g;
        }

        private static Vector3 Round(Vector3 v, float step) =>
            new(Mathf.Round(v.x / step) * step, Mathf.Round(v.y / step) * step, Mathf.Round(v.z / step) * step);
    }
}

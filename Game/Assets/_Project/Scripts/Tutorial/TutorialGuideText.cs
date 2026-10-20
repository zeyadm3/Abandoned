using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The guide as this machine words and places it: the marker's words ("PICK UP: VASE", "KITCHEN"), the
    /// progress bar's numbers ("12 m to go", "$4,200 / $6,000") and where the target is right now (a moving item
    /// is followed on this machine, not only where the host last said).
    /// </summary>
    public static class TutorialGuideText
    {
        /// <summary>Where the target is now; false when there is none.</summary>
        public static bool TryTarget(TutorialGuide g, out Vector3 point)
        {
            point = g.Point;
            if (g.Kind is TutorialGuideKind.None or TutorialGuideKind.Wait) return false;
            if (Followed(g) is NetworkObject o) point = o.transform.position;
            return true;
        }

        public static NetworkObject Followed(TutorialGuide g) => Spawned(g.Follow);

        public static NetworkObject Spawned(ulong id)
        {
            NetworkManager net = NetworkManager.Singleton;
            if (id == 0 || net == null || net.SpawnManager == null) return null;
            return net.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject o) && o != null ? o : null;
        }

        /// <summary>The words on the target's marker.</summary>
        public static string Label(TutorialGuide g)
        {
            string key = g.Key.ToString();
            switch (g.Kind)
            {
                case TutorialGuideKind.Take:
                    LootItem item = Followed(g) is NetworkObject o ? o.GetComponent<LootItem>() : null;
                    return item != null && item.Definition != null ? Loc.T("PICK UP: {0}", item.Definition.LocalName.ToUpperInvariant()) : Loc.T("PICK UP");
                case TutorialGuideKind.Use:
                    return g.Target switch
                    {
                        TutorialGuideTarget.Crate => Gear(key) is string gear ? Loc.T("TAKE: {0}", gear.ToUpperInvariant()) : Loc.T("KIT CRATE"),
                        TutorialGuideTarget.Door => Loc.T("DOOR"),
                        TutorialGuideTarget.Breaker => Loc.T("BREAKER"),
                        TutorialGuideTarget.Lever => Loc.T("LEVER"),
                        _ => Loc.T("USE"),
                    };
                case TutorialGuideKind.Load: return Loc.T("TRUCK");
                case TutorialGuideKind.Hide: return Loc.T("HIDE HERE");
                case TutorialGuideKind.Away: return Loc.T("GET OFF");
                case TutorialGuideKind.Brace: return Loc.T("BRACE HERE");
                case TutorialGuideKind.Look: return Place(key) ?? Loc.T("LOOK");
                case TutorialGuideKind.Go: return Place(key) ?? Loc.T("GO HERE");
                default: return "";
            }
        }

        /// <summary>The key the marker shows once the player is close and looking (null = none).</summary>
        public static string ActionAtTarget(TutorialGuide g) => g.Kind switch
        {
            TutorialGuideKind.Take or TutorialGuideKind.Use or TutorialGuideKind.Hide => "Interact",
            TutorialGuideKind.Brace => "Use",
            _ => null,
        };

        // The rooms and floors of the training house by their keys (TutorialHouseGameplay, TutorialHouseStructure).
        private static string Place(string key) => key switch
        {
            "driveway" => Loc.T("DRIVEWAY"),
            "porch" => Loc.T("FRONT DOOR"),
            "living" => Loc.T("LIVING ROOM"),
            "dining" => Loc.T("DINING ROOM"),
            "kitchen" => Loc.T("KITCHEN"),
            "laundry" => Loc.T("LAUNDRY"),
            "hallway" => Loc.T("HALLWAY"),
            "hall_bath" => Loc.T("BATHROOM"),
            "bedroom_a" => Loc.T("NORTH BEDROOM"),
            "bedroom_b" => Loc.T("SOUTH BEDROOM"),
            "master" => Loc.T("MASTER BEDROOM"),
            "ensuite" => Loc.T("ENSUITE"),
            "basement" => Loc.T("BASEMENT"),
            "patio" => Loc.T("PATIO"),
            _ => null,
        };

        private static string Gear(string equipmentId)
        {
            NetworkPlayer me = NetworkPlayer.Local;
            PlayerEquipment gear = me != null ? me.GetComponent<PlayerEquipment>() : null;
            if (gear == null || gear.Catalog == null) return null;
            EquipmentDefinition d = gear.Catalog.At(gear.Catalog.IndexOf(equipmentId));
            return d != null ? d.LocalName : null;
        }

        /// <summary>The progress bar's numbers in the player's language ("" for a bar alone).</summary>
        public static string Readout(TutorialReadout readout, float value, float goal) => readout switch
        {
            TutorialReadout.MetresLeft => value <= 1.2f ? Loc.T("Here") : Loc.T("{0:0} m to go", value),
            TutorialReadout.Metres => Loc.T("{0:0.0} / {1:0.#} m", value, goal),
            TutorialReadout.Seconds => Loc.T("{0:0.0} / {1:0.#} s", value, goal),
            TutorialReadout.Degrees => Loc.T("{0:0}° / {1:0}°", value, goal),
            TutorialReadout.Dollars => Loc.T("${0:N0} / ${1:N0}", value, goal),
            TutorialReadout.LoadPercent => Loc.T("Floor load {0:0}%", value),
            TutorialReadout.Aboard => Loc.T("{0:0} / {1:0} aboard", value, goal),
            _ => "",
        };

        /// <summary>"12 m" from the camera to a point.</summary>
        public static string Distance(float metres) => Loc.T("{0:0} m", metres);
    }
}

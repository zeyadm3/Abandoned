using System;
using Abandoned.Core;

namespace Abandoned.UI
{
    /// <summary>
    /// Which tips this player has seen (Prefs: PlayerPrefs, memory in batch mode) and the "show tips"
    /// switch. Texts use the player's own keys.
    /// </summary>
    public static class Hints
    {
        private const string EnabledKey = "hints.enabled", SeenPrefix = "hints.seen.";

        public static bool Enabled
        {
            get => Prefs.GetInt(EnabledKey, 1) == 1;
            set { Prefs.SetInt(EnabledKey, value ? 1 : 0); Prefs.Save(); }
        }

        public static bool Seen(HintId id) => Prefs.GetInt(SeenPrefix + id, 0) == 1;

        public static void MarkSeen(HintId id) => Prefs.SetInt(SeenPrefix + id, 1);

        /// <summary>Every tip shows again (Settings, "Show tips again").</summary>
        public static void ResetSeen()
        {
            foreach (HintId id in Enum.GetValues(typeof(HintId))) Prefs.Delete(SeenPrefix + id);
            Prefs.Save();
        }

        public static string Text(HintId id)
        {
            string interact = InputBindings.Display("Interact"), use = InputBindings.Display("Use"), drop = InputBindings.Display("Drop"),
                inventory = InputBindings.Display("Inventory"), crouch = InputBindings.Display("Crouch"), light = InputBindings.Display("Flashlight"),
                scan = InputBindings.Display("Scan"), rotate = InputBindings.Display("Rotate");
            return id switch
            {
                HintId.HqBoard => $"This is the company HQ. Read the contract board ({interact}) to pick today's job.",
                HintId.HqVan => $"Job taken. Grab gear from the rack and the shop, then get in the van ({interact}) when the crew is ready.",
                HintId.RunGoal => $"Scan ({scan}) to reveal prices and haul. Carry loot to the loading-bay truck and pull the yellow lever to leave. Its board shows your quota.",
                HintId.PickUp => $"{interact} picks things up. Heavier loot slows you down; fragile loot loses value when it hits something.",
                HintId.Pockets => $"Small things go straight into your pockets. Hold {inventory} to see them ({inventory} + {drop} drops the last).",
                HintId.Heavy => "Too heavy for one person: grab a handle together with your crew to lift it, or drag it alone (slow, and it still weighs on the floor).",
                HintId.Throw => $"Hold {rotate} + mouse to turn it. Tap {drop} to drop, hold {drop} to set it down gently; hold {use} to throw. Careful with glass.",
                HintId.FloorCracking => "The floor is cracking under the weight! Spread out, drop something heavy or get off it before it gives way.",
                HintId.PowerOff => $"The power is out. {light} turns your flashlight on.",
                HintId.BlindOne => $"Something is clicking in the dark. The Blind One hunts by sound: crouch ({crouch}), don't run, don't drop things.",
                HintId.Stalker => "Something is watching you. The Stalker only moves when nobody looks at it: keep an eye on it, and don't wander off alone.",
                HintId.Collector => "Something wants your loot. The Collector steals what's left lying around and runs; chase it and it drops it.",
                HintId.Ghost => "You died. Watch your crew as a ghost; the living can't hear you. A nearby crewmate with a medkit can revive you.",
                HintId.Hunter => $"Something big is patrolling. If it sees you it charges: break line of sight, crouch ({crouch}) to hide, or sprint. It's heavy, and weak floors don't hold it.",
                HintId.TruckLeaving => "The truck is leaving! Get in the bay before the honking stops, or you're left behind.",
                HintId.LockedShutter => "A locked store. What's behind a shutter is worth more. Bolt cutters open it quietly; a crowbar opens it loudly (both from the HQ shop).",
                HintId.Voicemail => "The answering machine is blinking: the boss left you a message.",
                _ => "",
            };
        }
    }
}

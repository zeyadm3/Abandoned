using Abandoned.Core;

namespace Abandoned.Threats
{
    /// <summary>
    /// The company's threat files, read on the HQ desk computer (tester feedback, 2026-10-09: nothing in the
    /// game explained the monsters). A file opens once this player has met its subject; that's local, in Prefs,
    /// like tips. The text describes each monster's actual rule, so keep it in step when a behaviour changes.
    /// </summary>
    public static class ThreatFiles
    {
        public readonly struct Entry
        {
            public readonly string Name, Summary, Movement, Counter;

            public Entry(string name, string summary, string movement, string counter)
            {
                Name = name; Summary = summary; Movement = movement; Counter = counter;
            }
        }

        private const string MetPrefix = "threatfiles.met.";

        /// <summary>The order files are listed in: roughly the order a crew meets them.</summary>
        public static readonly ThreatKind[] Order =
        {
            ThreatKind.BlindOne, ThreatKind.Stalker, ThreatKind.Collector, ThreatKind.Hunter,
            ThreatKind.Crawlers, ThreatKind.Weight, ThreatKind.Thing, ThreatKind.LastHunter,
        };

        public static bool Met(ThreatKind kind) => Prefs.GetInt(MetPrefix + kind, 0) == 1;

        /// <summary>True the first time: the file has just opened.</summary>
        public static bool MarkMet(ThreatKind kind)
        {
            if (Met(kind)) return false;
            Prefs.SetInt(MetPrefix + kind, 1);
            Prefs.Save();
            return true;
        }

        public static int MetCount
        {
            get
            {
                int count = 0;
                foreach (ThreatKind kind in Order) if (Met(kind)) count++;
                return count;
            }
        }

        public static Entry For(ThreatKind kind) => kind switch
        {
            ThreatKind.BlindOne => new Entry(Loc.T("Blind One"),
                Loc.T("Tall and eyeless. It clicks as it walks, and the clicking speeds up when it is hunting."),
                Loc.T("It cannot see. It wanders until it hears something, walks to the sound and stands there listening. " +
                "One loud noise, or several in a row, and it runs at the source. A couple of hits kill."),
                Loc.T("Crouch, don't sprint, and don't drop or throw things near it. If it is close, stop and stay quiet: " +
                "it only strikes what it has just heard. A noise somewhere else gives it somewhere else to go. " +
                "A locker hides nothing from it, but it is safe as long as you make no sound in there; it cannot open a shut door.")),
            ThreatKind.Stalker => new Entry(Loc.T("Stalker"),
                Loc.T("Thin and tall, usually at the edge of your light. You may hear it breathing before you see it."),
                Loc.T("It picks whoever is furthest from the crew and follows about ten metres back. It cannot move while " +
                "that person is looking at it. If they are alone, in its sight and facing away for a few seconds, it rushes."),
                Loc.T("Stay within a few steps of a teammate and it never works up to a rush. Alone, turn and look at it often: " +
                "every look resets it, and even a rush stops dead if you turn round in time. Your heartbeat and its breathing quicken as it works up to it, and the compass marks where it is. " +
                "It cannot open a door, and in a locker it loses you and waits where it stands, unless it was already rushing when you climbed in.")),
            ThreatKind.Collector => new Entry(Loc.T("Collector"),
                Loc.T("Small, quick, and not interested in you."),
                Loc.T("It never attacks. It takes loot nobody is standing near and carries it to one nest, far from the truck, " +
                "for the whole job. The hoard clinks now and then. Someone hiding in a locker doesn't count as standing near."),
                Loc.T("Don't leave piles unguarded; carry things straight to the truck. Get close and it drops what it holds " +
                "and runs. Find the nest and everything it took is still there.")),
            ThreatKind.Hunter => new Entry(Loc.T("Hunter"),
                Loc.T("Big, heavy, and it can see. You hear its footsteps first."),
                Loc.T("It patrols. If it sees you it charges, and one touch kills. It weighs as much as a piano. A shut door " +
                "stops it seeing you, but it breaks down a door it saw you go through: a few blows the whole building hears, " +
                "and that door never shuts again."),
                Loc.T("Break line of sight, crouch behind something, or sprint while your breath lasts. Slide a door shut " +
                "behind you and use the seconds it buys. A locker hides you, unless it watched you climb in: then it " +
                "pulls you out. Weak floors don't hold it: when one gives way under it, it falls and lies stunned.")),
            ThreatKind.Crawlers => new Entry(Loc.T("Crawlers"),
                Loc.T("A carpet of small things that keeps to the dark."),
                Loc.T("They arrive once the building starts to turn. They swarm whoever is carrying, bite lightly and tear " +
                "what is in your hands loose. They are small enough to pour into a locker after you."),
                Loc.T("Light. Put your flashlight beam on them and they scatter for a few seconds. A shut door keeps them out; a locker does not.")),
            ThreatKind.Weight => new Entry(Loc.T("The Weight"),
                Loc.T("Something enormous moving above the halls."),
                Loc.T("Very slow, and it never touches you. It leans on the building: you hear it wind up, then the weak " +
                "sections under it crack."),
                Loc.T("When you hear the wind-up, get off cracked floors and out from under them. Keep heavy loot away " +
                "from where it walks.")),
            ThreatKind.Thing => new Entry(Loc.T("The Thing"),
                Loc.T("It looks like part of the room until it doesn't."),
                Loc.T("Harmless while ignored. Hold a flashlight on it too long and it wakes, and comes for whoever showed " +
                "it where they were."),
                Loc.T("Don't study it. Take the beam off it and keep to the dark; in time it loses interest. " +
                "A shut door stops it, and so does a locker it didn't watch you climb into.")),
            ThreatKind.LastHunter => new Entry(Loc.T("The Last Hunter"),
                Loc.T("What comes when you have stayed too long."),
                Loc.T("It only appears after the window has closed. It knows where every living crew member is, and walls, " +
                "floors and doors don't stop it: it breaks any shut door it comes near. One touch kills."),
                Loc.T("There is no fighting it and no hiding from it. Be in the truck: that is the one place it will not follow.")),
            _ => new Entry(kind.ToString(), "", "", ""),
        };
    }
}

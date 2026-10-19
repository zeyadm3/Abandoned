using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Light on Gallows Rock: mostly dark, pools you move between. Pendants hang on long chains from the cellhouse
    /// trusses over the aisles and Broadway (they swing when something heavy falls or a monster passes on the gallery
    /// beside them), ceiling fittings in the rooms and the basement, bulkhead lamps in the barracks and the warden's
    /// house, and outside a few road lamps, two sodium masts over the dock and a lamp over the cellhouse door. About
    /// a third are dead, a quarter stutter and buzz; all of them follow the job's power and the danger's blackouts.
    /// The lighthouse's own lamp is separate (<see cref="IslandLighthouse"/>).
    /// </summary>
    public static class IslandLighting
    {
        public const string GroupName = "Lights";
        private static int seed = 5200;

        public static int Build(Transform root, IslandPrisonArt art)
        {
            seed = 5200;
            Transform g = Group(GroupName, root);
            int n = 0;
            // Cellhouse pendants: D aisle, Broadway (two rows), A aisle, and the entrance hall.
            foreach (float x in new[] { 36f, 51.5f, 56.5f, 72f })
            for (float z = 56f; z < L.StackNorth + 1f; z += 6f)
                n += Pendant(g, new Vector3(x, L.CellhouseEaves - .9f, z), 3.6f, art) ? 1 : 0;
            foreach (float x in new[] { 44f, 54f, 64f })
                n += Pendant(g, new Vector3(x, L.CellhouseEaves - .9f, 47f), 3.6f, art) ? 1 : 0;
            // Rooms: ceiling fittings.
            for (float x = 36f; x < 66f; x += 9f) n += Ceiling(g, new Vector3(x, L.NorthBandCeiling - .05f, 86f), art, 9f);
            n += Ceiling(g, new Vector3(70f, L.NorthBandCeiling - .05f, 84f), art, 9f);
            n += Ceiling(g, new Vector3(74f, L.NorthBandCeiling - .05f, 89f), art, 9f);
            n += Ceiling(g, new Vector3(73.5f, L.RoomCeiling - .05f, 44f), art, 8f);
            n += Ceiling(g, new Vector3(73.5f, L.RoomCeiling - .05f, 49.5f), art, 8f);
            n += Ceiling(g, new Vector3(34.5f, L.RoomCeiling - .05f, 44f), art, 8f);
            n += Ceiling(g, new Vector3(34.5f, L.RoomCeiling - .05f, 49.5f), art, 8f);
            // Room-specific pools keep the haul corridor readable; the stair and each doorway always have one
            // working lamp. Fixtures sit below the beams, rather than lighting the wrong side of the soffit.
            foreach (float x in new[] { 36f, 43f, 53.5f, 60.5f, 69.5f, 74.8f })
                n += Ceiling(g, new Vector3(x, 10.9f, 85.2f), art, 8f, bulb: true, reliable: true);
            // Barracks: three per floor.
            for (int f = 0; f < L.BarracksStoreys; f++)
            foreach (float x in new[] { 108f, 116f, 126f })
                n += Ceiling(g, new Vector3(x, (f + 1) * L.Storey - L.Slab - .05f, 9f), art, 8f, bulb: true);
            // Warden's house: what the fire left.
            n += Ceiling(g, new Vector3(46f, L.PlateauY + L.Storey - L.Slab - .05f, 105f), art, 7f, bulb: true);
            n += Ceiling(g, new Vector3(46f, L.PlateauY + 2f * L.Storey - .1f, 105f), art, 7f, bulb: true);
            n += Ceiling(g, new Vector3(L.Lighthouse.xMax + 3f, L.PlateauY + 3.3f, L.Lighthouse.yMin + 3f), art, 6f, bulb: true);
            // Outside.
            for (float z = 26f; z < L.RoadA.yMax; z += 14f) n += Post(g, new Vector3(L.RoadA.xMax - .8f, Mathf.Lerp(L.QuayY, L.TurnY, (z - L.RoadA.yMin) / L.RoadA.height), z), art);
            for (float z = 34f; z < L.RoadB.yMax; z += 14f) n += Post(g, new Vector3(L.RoadB.xMin + .6f, Mathf.Lerp(L.PlateauY, L.TurnY, (z - L.RoadB.yMin) / L.RoadB.height), z), art);
            foreach (Vector3 at in new[] { new Vector3(L.MainDoorX + 4f, L.PlateauY, 30f), new Vector3(L.Terrace.center.x, L.PlateauY, L.Terrace.yMax - .5f),
                new Vector3(40f, L.PlateauY, 96f), new Vector3(66f, L.PlateauY, 100f), new Vector3(L.RecYard.center.x, L.PlateauY, L.RecYard.yMin + 1f) })
                n += Post(g, at, art);
            n += Ceiling(g, new Vector3(L.MainDoorX, L.PlateauY + L.MainDoorHeight + .5f, L.Cellhouse.yMin - 1.5f), art, 10f, bulb: true);
            // Bulkheads on the cellhouse's outside walls (corners and side doors), the powerhouse, the gatehouse, the jetty.
            Rect c = L.Cellhouse;
            foreach (Vector3 at in new[]
            {
                new Vector3(c.xMin - .3f, L.PlateauY + 4f, c.yMin - .3f), new Vector3(c.xMax + .3f, L.PlateauY + 4f, c.yMin - .3f),
                new Vector3(c.xMin - .3f, L.PlateauY + 4f, c.yMax + .3f), new Vector3(c.xMax + .3f, L.PlateauY + 4f, c.yMax + .3f),
                new Vector3(c.xMin - .4f, L.PlateauY + 3.4f, L.YardDoorZ), new Vector3(c.xMax + .4f, L.PlateauY + 3.4f, L.EastDoorZ),
                new Vector3(L.ServiceDoorX, L.PlateauY + 3.4f, c.yMax + .4f),
                new Vector3(IslandExterior.Powerhouse.xMax + .4f, L.PlateauY + 3f, IslandExterior.Powerhouse.center.y),
                new Vector3(L.Terrace.xMin - 2.4f, L.PlateauY + 2.7f, L.Plateau.yMin + 9.8f),
            })
                n += Ceiling(g, at, art, 9f, bulb: true);
            Masts(g, art);
            return n;
        }

        private static int Roll() => Mathf.Abs((seed * 7919 + 2203) % 100);

        // Dead, stuttering, sputtering or steady, in the island's proportions.
        private static void Fit(LightFixture fixture, Light lamp, Renderer glow)
        {
            int roll = Roll();
            int s = seed++;
            if (roll < 12) fixture.EditorSetup(lamp, glow, false, true, s);
            else if (roll < 24) fixture.EditorSetupStutter(lamp, glow, s);
            else fixture.EditorSetup(lamp, glow, roll < 34, false, s);
        }

        private static Light Spot(Transform holder, float range, float angle, float intensity, Color colour)
        {
            Transform lampRoot = Group("Light", holder);
            lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light lamp = lampRoot.gameObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = range;
            lamp.spotAngle = angle;
            lamp.intensity = intensity;
            lamp.color = colour;
            lamp.shadows = LightShadows.None;
            lamp.innerSpotAngle = angle * .68f;
            lamp.bounceIntensity = 1f;
            return lamp;
        }

        // A green enamel shade on a chain from the truss, swinging from a pivot just under it.
        private static bool Pendant(Transform g, Vector3 pivotAt, float chain, IslandPrisonArt art)
        {
            Transform pivot = Group("Pendant", g);
            pivot.position = pivotAt;
            Box("Chain", pivot, Vector3.down * chain * .5f, new Vector3(.025f, chain, .025f), art.Steel, false);
            Transform holder = Group("Lamp", pivot);
            holder.localPosition = Vector3.down * chain;
            Box("Shade", holder, Vector3.up * .08f, new Vector3(.7f, .16f, .7f), art.Dado, false);
            GameObject glow = Box("Bulb", holder, Vector3.zero, new Vector3(.22f, .12f, .22f), art.Lamp, false);
            Light lamp = Spot(holder, 18f, 104f, 24f, new Color(1f, .86f, .65f));
            Fit(holder.gameObject.AddComponent<LightFixture>(), lamp, glow.GetComponent<Renderer>());
            pivot.gameObject.AddComponent<SwingingLamp>().EditorSetup(chain);
            return true;
        }

        private static int Ceiling(Transform g, Vector3 at, IslandPrisonArt art, float range, bool bulb = false, bool reliable = false)
        {
            Transform holder = Group(bulb ? "Bulkhead" : "Fitting", g);
            holder.position = at;
            GameObject glow;
            if (bulb)
            {
                Box("Mount", holder, Vector3.up * .035f, new Vector3(.25f, .07f, .25f), art.Steel, false);
                glow = Box("Bulb", holder, Vector3.down * .11f, new Vector3(.11f, .18f, .11f), art.Lamp, false);
                Box("GuardRing", holder, Vector3.down * .23f, new Vector3(.23f, .025f, .23f), art.Bars, false);
                foreach (Vector3 side in new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back })
                    Box("Guard", holder, side * .105f + Vector3.down * .12f, new Vector3(.018f, .23f, .018f), art.Bars, false);
            }
            else
            {
                Box("Housing", holder, Vector3.zero, new Vector3(1.3f, .08f, .3f), art.Steel, false);
                glow = Box("Diffuser", holder, Vector3.down * .05f, new Vector3(1.2f, .04f, .24f), art.Lamp, false);
            }
            Light lamp = Spot(holder, range, 112f, bulb ? 9f : 16f, bulb ? new Color(1f, .8f, .53f) : new Color(.78f, .9f, 1f));
            LightFixture fixture = holder.gameObject.AddComponent<LightFixture>();
            if (reliable) fixture.EditorSetup(lamp, glow.GetComponent<Renderer>(), false, false, seed++);
            else Fit(fixture, lamp, glow.GetComponent<Renderer>());
            return 1;
        }

        // A road lamp: a concrete post, an arm, a sodium head. Half of them are dead.
        private static int Post(Transform g, Vector3 foot, IslandPrisonArt art)
        {
            Transform post = Group("LampPost", g);
            post.position = foot;
            Box("Post", post, Vector3.up * 3f, new Vector3(.18f, 6f, .18f), art.FacadeDark);
            Box("Arm", post, new Vector3(0f, 5.9f, .6f), new Vector3(.08f, .08f, 1.2f), art.Steel, false);
            Transform head = Group("Head", post);
            head.localPosition = new Vector3(0f, 5.8f, 1.15f);
            GameObject glow = Box("Lens", head, Vector3.zero, new Vector3(.35f, .08f, .5f), art.Lamp, false);
            Light lamp = Spot(head, 18f, 100f, 8f, new Color(1f, .66f, .32f));
            var fixture = head.gameObject.AddComponent<LightFixture>();
            int roll = Roll();
            if (roll < 50) fixture.EditorSetup(lamp, glow.GetComponent<Renderer>(), false, true, seed++);
            else fixture.EditorSetup(lamp, glow.GetComponent<Renderer>(), roll < 70, false, seed++);
            return 1;
        }

        // Two tall sodium masts either side of the dock, aimed at the boat and the foot of the cliff stair.
        private static void Masts(Transform g, IslandPrisonArt art)
        {
            foreach (float x in new[] { L.Slip.xMin - 6f, L.Slip.xMax + 10f })
            {
                Transform mast = Group("DockMast", g);
                mast.position = new Vector3(x, L.QuayY, 3f);
                Box("Pole", mast, new Vector3(0f, 5.5f, 0f), new Vector3(.3f, 11f, .3f), art.Steel);
                Box("Head", mast, new Vector3(0f, 11.1f, 0f), new Vector3(1.4f, .35f, .5f), art.Lamp, false);
                Transform aim = Group("Light", mast);
                aim.localPosition = new Vector3(0f, 10.8f, 0f);
                aim.rotation = Quaternion.LookRotation(new Vector3(L.BoatStern.x, 0f, 2f) - (mast.position + Vector3.up * 10.8f));
                Light flood = aim.gameObject.AddComponent<Light>();
                flood.type = LightType.Spot;
                flood.range = 40f;
                flood.spotAngle = 75f;
                flood.intensity = 12f;
                flood.color = new Color(1f, .62f, .28f);
                flood.shadows = LightShadows.None;
            }
        }
    }
}

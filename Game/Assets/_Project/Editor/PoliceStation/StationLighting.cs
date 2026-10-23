using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A station's light. Fluorescent troffers in every room and along the halls on a checkerboard (one per two cells),
    /// high-bay lamps under the roof of a tall room, all ordinary <see cref="LightFixture"/>s: the power, the danger's
    /// blackouts and the breakers treat them like every other ceiling light (sectors come from where they hang). About
    /// one in five is dead and one in six flickers, fixed by position so every run looks the same. The breaker bank
    /// gives one <see cref="BreakerPanel"/> per sector. Street lamps outside are not on the building's power.
    /// </summary>
    public static class StationLighting
    {
        public const string GroupName = "Lights";

        public static int Build(Transform root, StationPlan plan, StationArt art, Color colour, int seed)
        {
            Transform lights = Group(GroupName, root);
            int made = 0;
            foreach (KeyValuePair<int, HashSet<Vector2Int>> floor in plan.Floors)
            {
                int f = floor.Key;
                var lit = new HashSet<P.Room>();
                foreach (Vector2Int c in floor.Value)
                {
                    P.Room room = plan.RoomAt(f, c);
                    bool pick = (c.x + c.y) % 2 == 0;
                    // Small rooms always get one, in their first cell.
                    if (!pick && room != null && !lit.Contains(room) && room.Cells.width * room.Cells.height <= 2 && c == room.Cells.min) pick = true;
                    if (!pick || plan.Pierced.ContainsKey(P.Key(f, c)) || plan.Pierced.ContainsKey(P.Key(f + 1, c)) || plan.Custom.Contains(c)) continue;
                    if (room != null) lit.Add(room);
                    float ceiling = Ceiling(plan, f, c, out bool tall);
                    if (float.IsNaN(ceiling)) continue;
                    Vector3 at = P.CellCentre(c, f);
                    at.y = ceiling;
                    int roll = Mathf.Abs((c.x * 7919 + c.y * 104729 + f * 31 + seed) % 100);
                    if (tall) HighBay(lights, $"HighBay_{f}_{c.x}_{c.y}", at, art, colour, roll, seed + made);
                    else Troffer(lights, $"Troffer_{f}_{c.x}_{c.y}", at, art, colour, roll, seed + made, (c.x + c.y) % 4 == 0);
                    made++;
                }
            }
            return made;
        }

        // Underside of what is above this cell: the next slab, the roof, or NaN (a stairwell above).
        private static float Ceiling(StationPlan plan, int f, Vector2Int c, out bool tall)
        {
            tall = false;
            if (plan.IsHole(f + 1, c)) return float.NaN;
            if (plan.HasFloor(f + 1, c)) return P.FloorY(f + 1) - P.Slab;
            if (f < 0) return float.NaN;
            tall = P.FloorY(f) + P.Story < plan.RoofY - .5f;
            return plan.RoofY;
        }

        public static void Troffer(Transform parent, string name, Vector3 ceiling, StationArt art, Color colour, int roll, int seed, bool turned)
        {
            Transform holder = Group(name, parent);
            holder.position = ceiling + Vector3.down * .05f;
            holder.rotation = Quaternion.Euler(0f, turned ? 90f : 0f, 0f);
            Box("Housing", holder, new Vector3(0f, .025f, 0f), new Vector3(1.24f, .05f, .64f), art.Steel, false);
            GameObject lens = Box("Lens", holder, new Vector3(0f, -.012f, 0f), new Vector3(1.18f, .025f, .58f), art.Lamp, false);
            Transform aim = Group("Light", holder);
            aim.localPosition = Vector3.down * .04f;
            aim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light lamp = aim.gameObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = 7.5f;
            lamp.spotAngle = 125f;
            lamp.innerSpotAngle = 70f;
            lamp.intensity = 3.4f;
            lamp.color = colour;
            lamp.shadows = LightShadows.None;
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, lens.GetComponent<Renderer>(), roll >= 20 && roll < 36, roll < 20, seed);
        }

        private static void HighBay(Transform parent, string name, Vector3 ceiling, StationArt art, Color colour, int roll, int seed)
        {
            Transform holder = Group(name, parent);
            holder.position = ceiling + Vector3.down * .9f;
            Box("Rod", holder, new Vector3(0f, .45f, 0f), new Vector3(.03f, .9f, .03f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Shade", holder, new Vector3(0f, -.05f, 0f), new Vector3(.62f, .12f, .62f), art.DarkSteel, false);
            GameObject lens = Primitive(PrimitiveType.Cylinder, "Lens", holder, new Vector3(0f, -.18f, 0f), new Vector3(.48f, .01f, .48f), art.Lamp, false);
            Transform aim = Group("Light", holder);
            aim.localPosition = Vector3.down * .2f;
            aim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light lamp = aim.gameObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = 13f;
            lamp.spotAngle = 105f;
            lamp.intensity = 7f;
            lamp.color = colour;
            lamp.shadows = LightShadows.None;
            holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, lens.GetComponent<Renderer>(), roll >= 20 && roll < 36, roll < 20, seed);
        }

        /// <summary>One breaker per power sector in a row from <paramref name="first"/>, each facing out along the row's +z.</summary>
        public static int Breakers(Transform root, StationArt art, Vector3 first, float yaw, float spacing = 1f)
        {
            HorrorConfig horror = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            int sectors = Mathf.Clamp(horror != null ? horror.Sectors : 6, 1, RunBreakers.MaxSectors);
            Transform bank = Group("BreakerBank", root);
            Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
            for (int s = 0; s < sectors; s++) Panel(bank, s, first + turn * Vector3.right * (s * spacing), turn, art);
            return sectors;
        }

        private static void Panel(Transform parent, int sector, Vector3 at, Quaternion turn, StationArt art)
        {
            var size = new Vector3(.7f, .9f, .28f);
            const float lift = 1f;
            Transform root = Group($"Breaker_Sector{sector}", parent);
            root.SetPositionAndRotation(at, turn);
            float z = size.z * .5f + .02f, y = lift + size.y * .5f;
            Box("Body", root, new Vector3(0f, y, z), size, art.DarkSteel, false);
            Box("Door", root, new Vector3(0f, y, size.z + .03f), new Vector3(size.x - .06f, size.y - .06f, .02f), art.Steel, false);
            Box("Stripe", root, new Vector3(0f, lift + size.y - .06f, size.z + .045f), new Vector3(size.x - .1f, .06f, .01f), art.Safety, false);
            Box("Conduit", root, new Vector3(.2f, lift + size.y + .9f, .06f), new Vector3(.06f, 1.8f, .06f), art.Steel, false);
            Transform pivot = Group("Handle", root);
            pivot.localPosition = new Vector3(size.x * .5f + .06f, y, size.z * .5f);
            Box("Arm", pivot, new Vector3(0f, .16f, 0f), new Vector3(.05f, .32f, .05f), art.Steel, false);
            Box("Grip", pivot, new Vector3(0f, .34f, .04f), new Vector3(.12f, .06f, .1f), art.Safety, false);
            pivot.localRotation = Quaternion.Euler(40f, 0f, 0f);
            GameObject lamp = Primitive(PrimitiveType.Sphere, "Lamp", root, new Vector3(-.22f, lift + size.y - .15f, size.z + .06f), Vector3.one * .07f, art.WarningRed, false);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, y, z);
            solid.size = size;
            root.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, pivot, lamp.GetComponent<Renderer>());
        }

        public const string EmergencyGroup = "EmergencyLights";

        /// <summary>
        /// Battery emergency lights: a twin-head unit over every doorway that opens onto a hall, on the hall side, and
        /// over each outside door. Dim and warm, never on the building's power (no <see cref="LightFixture"/>), so a
        /// blackout still leaves the way out faintly marked.
        /// </summary>
        public static int Emergency(Transform root, StationPlan plan, StationArt art)
        {
            Transform group = Group(EmergencyGroup, root);
            int made = 0;
            foreach (P.Doorway d in plan.Doors)
            {
                bool aHall = plan.RoomAt(d.Floor, d.A) == null, bHall = plan.RoomAt(d.Floor, d.B) == null;
                if (!aHall && !bHall) continue;
                Vector2Int hallCell = aHall ? d.A : d.B, other = aHall ? d.B : d.A;
                Vector3 hall = P.CellCentre(hallCell, d.Floor), room = P.CellCentre(other, d.Floor);
                Vector3 mid = (hall + room) * .5f, toHall = (hall - room).normalized;
                Unit(group, $"Emergency_{made}", mid + toHall * .15f + Vector3.up * (d.Height + .32f), toHall, art);
                made++;
            }
            foreach (P.Opening o in plan.Facade)
            {
                if (o.Glazed) continue;
                Vector3 inward = o.Side switch { P.Side.South => Vector3.forward, P.Side.North => Vector3.back, P.Side.West => Vector3.right, _ => Vector3.left };
                Vector3 at = o.Side switch
                {
                    P.Side.South => new Vector3(o.Centre, 0f, 0f),
                    P.Side.North => new Vector3(o.Centre, 0f, plan.Depth * P.Tile),
                    P.Side.West => new Vector3(0f, 0f, o.Centre),
                    _ => new Vector3(plan.Width * P.Tile, 0f, o.Centre),
                };
                // Over the opening; under a storey's ceiling it tucks in just below it.
                float y = o.Top + .32f < 3.6f || o.Top > 4f ? o.Top + .32f : 3.6f;
                Unit(group, $"Emergency_{made}", at + inward * (P.ShellThickness * .5f + .12f) + Vector3.up * y, inward, art);
                made++;
            }
            return made;
        }

        private static void Unit(Transform parent, string name, Vector3 at, Vector3 facing, StationArt art)
        {
            Transform unit = Group(name, parent);
            unit.SetPositionAndRotation(at, Quaternion.LookRotation(facing, Vector3.up));
            Box("Case", unit, Vector3.zero, new Vector3(.36f, .16f, .1f), art.White, false);
            foreach (float x in new[] { -.13f, .13f })
            {
                GameObject head = Box("Head", unit, new Vector3(x, -.02f, .1f), new Vector3(.09f, .09f, .1f), art.Lamp, false);
                head.transform.localRotation = Quaternion.Euler(25f, x < 0f ? -20f : 20f, 0f);
            }
            Transform aim = Group("Light", unit);
            aim.localPosition = new Vector3(0f, -.05f, .1f);
            aim.localRotation = Quaternion.Euler(55f, 0f, 0f);
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 6f;
            l.spotAngle = 110f;
            l.intensity = .7f;
            l.color = new Color(1f, .82f, .62f);
            l.shadows = LightShadows.None;
        }

        /// <summary>A sodium street lamp on a pole, arm toward <paramref name="toward"/>; not on the building's power.</summary>
        public static void StreetLamp(Transform parent, Vector3 foot, Vector3 toward, StationArt art, float intensity = 9f)
        {
            Transform pole = Group("StreetLamp", parent);
            pole.position = foot;
            Vector3 dir = toward - foot;
            dir.y = 0f;
            pole.rotation = Quaternion.LookRotation(dir.sqrMagnitude > .01f ? dir.normalized : Vector3.forward);
            Primitive(PrimitiveType.Cylinder, "Pole", pole, new Vector3(0f, 4f, 0f), new Vector3(.18f, 4f, .18f), art.DarkSteel);
            Box("Arm", pole, new Vector3(0f, 7.9f, .8f), new Vector3(.08f, .08f, 1.7f), art.DarkSteel, false);
            Box("Head", pole, new Vector3(0f, 7.85f, 1.65f), new Vector3(.32f, .12f, .6f), art.DarkSteel, false);
            Box("Glow", pole, new Vector3(0f, 7.78f, 1.65f), new Vector3(.26f, .02f, .5f), art.Lamp, false);
            Transform aim = Group("Light", pole);
            aim.localPosition = new Vector3(0f, 7.7f, 1.65f);
            aim.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 18f;
            l.spotAngle = 120f;
            l.intensity = intensity;
            l.color = new Color(1f, .7f, .38f);
            l.shadows = LightShadows.None;
        }
    }
}

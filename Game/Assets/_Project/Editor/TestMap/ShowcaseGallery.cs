using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A small gallery north of the TestMap warehouse where the crew and every monster stand still on plinths, each
    /// under its own spot, for looking at models and materials up close. Brick outside, dark plaster inside, terrazzo
    /// floor; one open entrance on the south side, reached by the path round the warehouse's west side.
    /// </summary>
    public static class ShowcaseGallery
    {
        public static readonly Vector3 Min = new(-6f, 0f, 20f), Max = new(26f, 0f, 34f);
        public const float Floor = .02f, Height = 5f, Wall = .25f;
        public const string CeilingLights = "Ceiling_", StatueLights = "Spot_";
        private static readonly string[] Coveralls = { "orange", "navy", "olive", "rust" };
        private const float Gap = 1.3f, Margin = .35f;

        public static void Build(Transform root)
        {
            Transform hall = Group("Gallery", root);
            Shell(hall);
            Transform statues = Group("Statues", hall), lights = Group("Lights", hall);
            int seed = 900;

            // The crew: one long plinth facing the entrance.
            float crewZ = Min.z + 3.6f;
            Box("CrewPlinth", statues, new Vector3(10f, Floor + .1f, crewZ), new Vector3(7.2f, .2f, 1.3f), ShowcaseMaterials.Plinth);
            for (int i = 0; i < Coveralls.Length; i++)
            {
                var spot = new Vector3(10f + (i - 1.5f) * 1.7f, Floor + .2f, crewZ);
                GameObject crew = ShowcaseStatues.Crew(statues, Coveralls[i]);
                if (crew == null) continue;
                // The crew model faces -z, so unturned they face the entrance (at 180 they showed it their backpacks).
                crew.transform.rotation = Quaternion.identity;
                Bounds b = Seat(crew, spot);
                Plaque(statues, $"Crew · {char.ToUpper(Coveralls[i][0])}{Coveralls[i].Substring(1)}", new Vector3(spot.x, Floor + .1f, crewZ - .65f));
                Light(lights, b, seed++);
            }

            // The monsters along the back, as many rows as they need, each on its own plinth.
            var made = new List<(GameObject statue, Bounds bounds)>();
            foreach (string name in ShowcaseStatues.Threats)
            {
                GameObject statue = ShowcaseStatues.Threat(statues, name);
                if (statue == null) continue;
                statue.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // facing whoever walks in
                made.Add((statue, ShowcaseStatues.WorldBounds(statue)));
            }
            float x = Min.x + 1f, back = Max.z - .8f, rowDepth = 0f;
            foreach (var (statue, bounds) in made)
            {
                Vector2 plinth = new(Mathf.Max(1.2f, bounds.size.x + Margin * 2f), Mathf.Max(1.2f, bounds.size.z + Margin * 2f));
                if (x + plinth.x > Max.x - 1f)
                {
                    x = Min.x + 1f;
                    back -= rowDepth + 2.4f;
                    rowDepth = 0f;
                }
                var centre = new Vector3(x + plinth.x / 2f, Floor, back - plinth.y / 2f);
                Box($"Plinth_{statue.name}", statues, centre + Vector3.up * .15f, new Vector3(plinth.x, .3f, plinth.y), ShowcaseMaterials.Plinth);
                Bounds b = Seat(statue, centre + Vector3.up * .3f);
                Plaque(statues, ShowcaseStatues.Pretty(statue.name), new Vector3(centre.x, Floor + .15f, centre.z - plinth.y / 2f));
                Light(lights, b, seed++);
                x += plinth.x + Gap;
                rowDepth = Mathf.Max(rowDepth, plinth.y);
            }

            // A low general light so the room reads between the spots (LightingRebuild calibrates, TestMapShowcase dims).
            for (int i = 0; i < 4; i++)
                ShowcaseLighting.Ceiling(lights, "mounted_fluorescent_lights", new Vector3(Min.x + 4f + i * 8f, Floor + Height - .04f, (Min.z + Max.z) / 2f),
                    0f, new Vector3(1.6f, .4f, .5f), ShowcaseLighting.Fluorescent, seed++).name = CeilingLights + i;
        }

        // Puts the statue's drawn bounds centred on the top of a plinth; returns the bounds where it ends up.
        private static Bounds Seat(GameObject statue, Vector3 top)
        {
            Bounds b = ShowcaseStatues.WorldBounds(statue);
            statue.transform.position += new Vector3(top.x - b.center.x, top.y - b.min.y, top.z - b.center.z);
            b.center += new Vector3(top.x - b.center.x, top.y - b.min.y, top.z - b.center.z);
            return b;
        }

        // A key light from the ceiling, in front of and above the statue, sized to it.
        private static void Light(Transform lights, Bounds statue, int seed)
        {
            var at = new Vector3(statue.center.x, Floor + Height - .06f, statue.min.z - 2.2f);
            var target = new Vector3(statue.center.x, statue.min.y + statue.size.y * .55f, statue.center.z);
            float reach = Mathf.Max(statue.size.x, statue.size.y) / 2f + .5f;
            float angle = Mathf.Clamp(2f * Mathf.Atan2(reach, Vector3.Distance(at, target)) * Mathf.Rad2Deg, 24f, 75f);
            ShowcaseLighting.Spot(lights, StatueLights + seed, at, target, angle, ShowcaseLighting.Gallery, seed);
        }

        // A brass plate on the plinth's front face, the name on it (front face at z = front).
        private static void Plaque(Transform parent, string text, Vector3 front)
        {
            Box("Plaque", parent, new Vector3(front.x, front.y, front.z - .006f), new Vector3(.62f, .11f, .01f), ShowcaseMaterials.Plaque, false);
            Label(parent, "Name", text, new Vector3(front.x, front.y, front.z - .0125f), Quaternion.identity, .012f, new Color(.12f, .1f, .07f));
        }

        private static void Shell(Transform hall)
        {
            float w = Max.x - Min.x, d = Max.z - Min.z, cx = (Min.x + Max.x) / 2f, cz = (Min.z + Max.z) / 2f;
            Box("Floor", hall, new Vector3(cx, Floor - .15f, cz), new Vector3(w + Wall * 2f + .1f, .3f, d + Wall * 2f + .1f), ShowcaseMaterials.GalleryFloor);
            var entrance = new WallOpening(4f, 0f, 3.2f);
            GreyboxWall.Panel(hall, "Wall_S", new Vector3(Min.x - Wall, Floor, Min.z - Wall / 2f), Vector3.right, w + Wall * 2f, Height, Wall,
                entrance, ShowcaseMaterials.Brick, ShowcaseMaterials.Brick);
            ShowcaseWalls.StripFrames(hall.GetChild(hall.childCount - 1));
            Box("Wall_N", hall, new Vector3(cx, Floor + Height / 2f, Max.z + Wall / 2f), new Vector3(w + Wall * 2f, Height, Wall), ShowcaseMaterials.Brick);
            Box("Wall_W", hall, new Vector3(Min.x - Wall / 2f, Floor + Height / 2f, cz), new Vector3(Wall, Height, d), ShowcaseMaterials.Brick);
            Box("Wall_E", hall, new Vector3(Max.x + Wall / 2f, Floor + Height / 2f, cz), new Vector3(Wall, Height, d), ShowcaseMaterials.Brick);

            // Dark plaster lining 5 mm inside the brick, 2 cm thick.
            const float off = .005f, lining = .02f;
            Material plaster = ShowcaseMaterials.GalleryWall;
            GreyboxWall.Panel(hall, "Lining_S", new Vector3(Min.x + off, Floor, Min.z + off + lining / 2f), Vector3.right, w - off * 2f, Height, lining,
                entrance, plaster, plaster);
            ShowcaseWalls.StripFrames(hall.GetChild(hall.childCount - 1));
            Box("Lining_N", hall, new Vector3(cx, Floor + Height / 2f, Max.z - off - lining / 2f), new Vector3(w - off * 2f, Height, lining), plaster, false);
            Box("Lining_W", hall, new Vector3(Min.x + off + lining / 2f, Floor + Height / 2f, cz), new Vector3(lining, Height, d - off * 2f - lining * 2f), plaster, false);
            Box("Lining_E", hall, new Vector3(Max.x - off - lining / 2f, Floor + Height / 2f, cz), new Vector3(lining, Height, d - off * 2f - lining * 2f), plaster, false);
            Box("Ceiling", hall, new Vector3(cx, Floor + Height - .03f, cz), new Vector3(w - .002f, .02f, d - .002f), ShowcaseMaterials.GalleryCeiling, false);
            Box("Roof", hall, new Vector3(cx, Floor + Height + .175f, cz), new Vector3(w + Wall * 2f + .1f, .35f, d + Wall * 2f + .1f), ShowcaseMaterials.Roof);

            // Steel frame round the entrance, proud of the brick outside and the plaster inside.
            Material steel = ShowcaseMaterials.DoorFrame;
            float z = Min.z - Wall / 2f + (off + lining) / 2f, depth = Wall + off + lining + .04f;
            Box("Entrance_L", hall, new Vector3(10f - 2f + .03f, Floor + 1.6f, z), new Vector3(.06f, 3.2f, depth), steel, false);
            Box("Entrance_R", hall, new Vector3(10f + 2f - .03f, Floor + 1.6f, z), new Vector3(.06f, 3.2f, depth), steel, false);
            Box("Entrance_Head", hall, new Vector3(10f, Floor + 3.2f - .03f, z), new Vector3(3.88f, .06f, depth), steel, false);
            MarkStatic(hall);
        }
    }
}

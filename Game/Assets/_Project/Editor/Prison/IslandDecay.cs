using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Decay that follows a cause, like the mall's. The sea air: rust streaks under every window and bar, salt bloom
    /// low on the outside walls, ivy up the lee faces. The leaks: damp and moss under the lantern's empty bays and
    /// along the north wall where the gutters failed. Time: peeling paint on the cell blocks' ends, cracked floors,
    /// fallen plaster in the corners. People: mattresses dragged into the aisles, paper everywhere, a tipped food
    /// cart. All of it flat or walk-over (no colliders), and none of it on the haul lanes or in a doorway.
    /// The bay gets its buoys and a wreck's mast to give the sea a near distance in the fog.
    /// </summary>
    public static class IslandDecay
    {
        private const float Y = L.PlateauY;

        public static int Build(Transform root, IslandPrisonArt art, List<Rect> keepClear)
        {
            Transform g = Group("Decay", root);
            int n = 0;
            n += Floors(Group("Floors", g), art, keepClear);
            n += Walls(Group("Walls", g), art);
            n += Exterior(Group("Exterior", g), art);
            n += People(Group("Leftovers", g), art, keepClear);
            Bay(Group("Bay", root), art);
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            MarkStatic(g);
            return n;
        }

        private static bool Clear(Vector2 p, List<Rect> keepClear)
        {
            foreach (Rect r in keepClear) if (r.Contains(p)) return false;
            return true;
        }

        private static GameObject Flat(Transform g, string name, Vector3 at, Vector2 size, float yaw, Material m) =>
            K.Patch(name, g, at, size, yaw, m);

        private static int Floors(Transform g, IslandPrisonArt art, List<Rect> keepClear)
        {
            int n = 0;
            // Damp along the north wall where the gutters failed, and moss in the dining hall's corners.
            for (float x = L.Cellhouse.xMin + 2f; x < L.Cellhouse.xMax - 2f; x += 5f)
            {
                float h = K.Hash((int)x, 211);
                if (h < .3f) continue;
                Flat(g, "Damp", new Vector3(x, Y + .007f, L.Cellhouse.yMax - 1.2f), new Vector2(2f + h * 2f, 1.4f), h * 30f - 15f, art.Moss);
                n++;
            }
            // Basement: standing water is in IslandCellhouse; here a tide line of grime round the walls.
            for (float x = L.Cellhouse.xMin + 1f; x < L.Cellhouse.xMax - 1f; x += 3f)
            {
                Flat(g, "Grime", new Vector3(x, L.BasementY + .009f, L.Cellhouse.yMax - .9f), new Vector2(3f, .7f), 0f, art.Stain);
                n++;
            }
            // Outside: wet leaves blown into the lee of every wall, and puddles on the paths.
            for (int i = 0; i < 30; i++)
            {
                float hx = K.Hash(i, 221), hz = K.Hash(221, i);
                var p = new Vector2(Mathf.Lerp(L.Plateau.xMin + 3f, L.Plateau.xMax - 3f, hx), Mathf.Lerp(L.Plateau.yMin + 3f, L.Plateau.yMax - 3f, hz));
                if (L.Cellhouse.Contains(p) || L.WardenHouse.Contains(p)) continue;
                Flat(g, i % 3 == 0 ? "Puddle" : "Leaves", new Vector3(p.x, Y + .015f, p.y), new Vector2(1.5f + hx * 2.5f, 1f + hz * 2f), hx * 360f, i % 3 == 0 ? art.Water : art.Leaves);
                n++;
            }
            _ = keepClear;
            return n;
        }

        private static bool InStack(Vector2 p)
        {
            foreach (L.Stack s in L.Stacks)
                if (p.x > s.West && p.x < s.East && p.y > L.StackSouth - 2f && p.y < L.StackNorth + 2f) return true;
            return false;
        }

        // Stains on the inside walls were mall decals (hard rectangles on a box); the walls' own textures carry the age.
        private static int Walls(Transform g, IslandPrisonArt art) => 0;

        // Ivy up the cellhouse's sheltered north and west faces: clumps of leaves standing off the wall, not a flat panel.
        private static int Exterior(Transform g, IslandPrisonArt art)
        {
            int n = 0;
            Rect c = L.Cellhouse;
            void Clump(Vector3 foot, Vector3 outward, float h)
            {
                for (int k = 0; k < 7; k++)
                {
                    float y = .3f + k * (h / 7f), w = 1.6f - k * .14f + K.Hash(k, (int)foot.x) * .6f;
                    Vector3 side = Vector3.Cross(Vector3.up, outward) * ((K.Hash(k, (int)foot.z) - .5f) * 1.2f);
                    PolishAssets.Shape("Ivy", g, foot + Vector3.up * y + outward * .18f + side, new Vector3(Mathf.Abs(outward.x) > .5f ? .4f : w, .9f, Mathf.Abs(outward.x) > .5f ? w : .4f), art.Ivy, true);
                    n++;
                }
            }
            for (float x = c.xMin + 3f; x < c.xMax - 3f; x += 5f)
                if (K.Hash((int)x, 231) > .55f) Clump(new Vector3(x, Y, c.yMax + .3f), Vector3.forward, 4f + K.Hash((int)x, 232) * 5f);
            for (float z = c.yMin + 3f; z < c.yMax - 3f; z += 5f)
                if (K.Hash((int)z, 241) > .6f && Mathf.Abs(z - L.YardDoorZ) > 3f) Clump(new Vector3(c.xMin - .3f, Y, z), Vector3.left, 3f + K.Hash((int)z, 242) * 4f);
            return n;
        }

        // What the last people here left: mattresses in the aisles, a food cart on its side, paper, a bucket.
        private static int People(Transform g, IslandPrisonArt art, List<Rect> keepClear)
        {
            int n = 0;
            foreach ((float x, float z, float yaw) in new[] { (34.6f, 63f, 10f), (37.6f, 71f, -6f), (70.6f, 66f, 80f), (73.5f, 74f, 20f) })
            {
                if (!Clear(new Vector2(x, z), keepClear)) continue;
                K.RotBox("Mattress", g, new Vector3(x, Y + .06f, z), new Vector3(.75f, .1f, 1.9f), Quaternion.Euler(0f, yaw, 4f), art.Mattress, false);
                n++;
            }
            Transform cart = Group("FoodCart", g);
            cart.SetPositionAndRotation(new Vector3(71.5f, Y + .45f, 77f), Quaternion.Euler(0f, 30f, 90f));
            Box("Body", cart, Vector3.zero, new Vector3(.9f, .7f, 1.5f), art.Steel, false);
            Box("Lid", cart, new Vector3(0f, .4f, .9f), new Vector3(.9f, .05f, .7f), art.Steel, false);
            for (int i = 0; i < 60; i++)
            {
                float hx = K.Hash(i, 251), hz = K.Hash(251, i);
                var p = new Vector2(Mathf.Lerp(L.Cellhouse.xMin + 1f, L.Cellhouse.xMax - 1f, hx), Mathf.Lerp(L.Cellhouse.yMin + 1f, L.NorthBand.yMin - .5f, hz));
                if (InStack(p) || L.Admin.Contains(p) || L.Library.Contains(p)) continue;
                Flat(g, "Paper", new Vector3(p.x, Y + .01f, p.y), new Vector2(.21f, .3f), hx * 360f, art.Paper);
                n++;
            }
            // Fallen plaster heaps in the corners of the hall (walk-over).
            foreach ((float x, float z) in new[] { (31.3f, 41.3f), (76.6f, 41.3f), (31.3f, 79f), (76.6f, 79f) })
            for (int i = 0; i < 5; i++)
            {
                float h = K.Hash((int)x + i, (int)z);
                GameObject chunk = PolishAssets.Shape("Plaster", g, new Vector3(x + (h - .5f) * 1.2f, Y + .05f, z + (K.Hash(i, (int)x) - .5f) * 1.2f),
                    new Vector3(.3f + h * .3f, .1f + h * .08f, .25f + h * .25f), art.Plaster, true);
                chunk.transform.rotation = Quaternion.Euler(0f, h * 360f, 0f);
                n++;
            }
            return n;
        }

        // Out in the bay: a red and a green channel buoy either side of the approach, each with a blinking lamp, and the
        // mast of something that sank off the north point.
        private static void Bay(Transform g, IslandPrisonArt art)
        {
            foreach ((Vector3 at, Material m) in new[] { (new Vector3(70f, L.SeaY, -45f), art.HullTrim), (new Vector3(96f, L.SeaY, -48f), art.Grass) })
            {
                Transform buoy = Group("Buoy", g);
                buoy.position = at;
                Primitive(PrimitiveType.Cylinder, "Float", buoy, Vector3.up * .4f, new Vector3(1.6f, .6f, 1.6f), m, false);
                Box("Cage", buoy, Vector3.up * 1.8f, new Vector3(.6f, 1.8f, .6f), m, false);
                Transform lampAt = Group("Lamp", buoy);
                lampAt.localPosition = Vector3.up * 2.9f;
                Box("Glass", lampAt, Vector3.zero, new Vector3(.25f, .3f, .25f), art.Ember, false);
                Light light = lampAt.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 8f;
                light.intensity = 3f;
                light.color = m == art.HullTrim ? new Color(1f, .3f, .2f) : new Color(.4f, 1f, .5f);
                light.shadows = LightShadows.None;
                buoy.gameObject.AddComponent<Core.PowerIndependentLight>();
                buoy.gameObject.AddComponent<Structure.IdleSway>().EditorSetup(4f, 4.5f);
            }
            Transform wreck = Group("Wreck", g);
            wreck.SetPositionAndRotation(new Vector3(40f, L.SeaY, 140f), Quaternion.Euler(0f, 0f, 14f));
            Box("Mast", wreck, Vector3.up * 5f, new Vector3(.3f, 10f, .3f), art.DarkWood, false);
            Box("Yard", wreck, new Vector3(0f, 8f, 0f), new Vector3(4f, .2f, .2f), art.DarkWood, false);
            K.RotBox("Rail", wreck, new Vector3(0f, .4f, -4f), new Vector3(6f, .3f, .2f), Quaternion.Euler(0f, 30f, -6f), art.DarkWood, false);
        }
    }
}

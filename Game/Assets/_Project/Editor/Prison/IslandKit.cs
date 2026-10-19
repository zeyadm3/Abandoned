using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Shared construction helpers for the island's builders: walls with any number of door and window openings
    /// (lintels and sills filled in), rotated boxes, railings with posts, and sloped ramps. Everything is a box, so
    /// colliders are exact and the world-UV pass gives each surface true-size textures.
    /// </summary>
    public static class IslandKit
    {
        /// <summary>An opening in a wall: centre along the wall, width, height of its top, and sill height.</summary>
        public readonly struct Opening
        {
            public readonly float At, Width, Top, Sill;
            public Opening(float at, float width, float top, float sill = 0f) { At = at; Width = width; Top = top; Sill = sill; }
            public static Opening Door(float at, float width, float height) => new(at, width, height, 0f);
            public static Opening Window(float at, float width, float sill, float top) => new(at, width, top, sill);
        }

        /// <summary>A wall along x from <paramref name="x0"/> to <paramref name="x1"/> at z, base y, with openings
        /// (their <see cref="Opening.At"/> is a world x). Returns the holder.</summary>
        public static Transform WallX(Transform parent, string name, float x0, float x1, float z, float y, float height,
            float thick, Material material, params Opening[] openings) =>
            Wall(parent, name, new Vector3(x0, y, z), Vector3.right, x1 - x0, height, thick, material, openings, x0);

        /// <summary>A wall along z from <paramref name="z0"/> to <paramref name="z1"/> at x (openings at world z).</summary>
        public static Transform WallZ(Transform parent, string name, float z0, float z1, float x, float y, float height,
            float thick, Material material, params Opening[] openings) =>
            Wall(parent, name, new Vector3(x, y, z0), Vector3.forward, z1 - z0, height, thick, material, openings, z0);

        private static Transform Wall(Transform parent, string name, Vector3 start, Vector3 axis, float length, float height,
            float thick, Material material, Opening[] openings, float origin)
        {
            Transform holder = Group(name, parent);
            var sorted = new List<Opening>(openings);
            sorted.Sort((a, b) => a.At.CompareTo(b.At));
            Vector3 across = axis == Vector3.right ? Vector3.forward : Vector3.right;
            Vector3 Size(float run, float tall) => axis == Vector3.right ? new Vector3(run, tall, thick) : new Vector3(thick, tall, run);
            Vector3 At(float along, float y) => start + axis * along + Vector3.up * y;

            float cursor = 0f;
            int n = 0;
            foreach (Opening o in sorted)
            {
                float a = Mathf.Clamp(o.At - origin - o.Width * .5f, 0f, length);
                float b = Mathf.Clamp(o.At - origin + o.Width * .5f, 0f, length);
                if (a - cursor > .01f) Box($"{name}_{n++}", holder, At((cursor + a) * .5f, height * .5f), Size(a - cursor, height), material);
                float top = Mathf.Min(o.Top, height);
                if (height - top > .01f) Box($"{name}_Lintel{n++}", holder, At((a + b) * .5f, (top + height) * .5f), Size(b - a, height - top), material);
                if (o.Sill > .01f) Box($"{name}_Sill{n++}", holder, At((a + b) * .5f, o.Sill * .5f), Size(b - a, o.Sill), material);
                cursor = Mathf.Max(cursor, b);
            }
            if (length - cursor > .01f) Box($"{name}_{n}", holder, At((cursor + length) * .5f, height * .5f), Size(length - cursor, height), material);
            _ = across;
            return holder;
        }

        public static GameObject RotBox(string name, Transform parent, Vector3 centre, Vector3 size, Quaternion rotation,
            Material material, bool collider = true)
        {
            GameObject go = Box(name, parent, centre, size, material, collider);
            go.transform.localRotation = rotation;
            return go;
        }

        /// <summary>A rail between two points: posts every ~1.6 m, a top rail and a mid rail. Gaps are left to the caller.</summary>
        public static Transform Railing(Transform parent, string name, Vector3 from, Vector3 to, float height, Material material,
            bool collider = true, float bar = .06f)
        {
            Transform run = Group(name, parent);
            Vector3 along = to - from;
            float length = along.magnitude;
            if (length < .05f) return run;
            Quaternion facing = Quaternion.LookRotation(along / length, Vector3.up);
            Vector3 mid = (from + to) * .5f;
            RotBox("Top", run, mid + Vector3.up * height, new Vector3(bar, bar, length), facing, material, collider);
            RotBox("Mid", run, mid + Vector3.up * height * .5f, new Vector3(bar * .7f, bar * .7f, length), facing, material, false);
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 1.6f) + 1);
            for (int i = 0; i < posts; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, i / (float)(posts - 1));
                Box($"Post{i}", run, at + Vector3.up * height * .5f, new Vector3(bar, height, bar), material, false);
            }
            return run;
        }

        /// <summary>A solid sloped slab from <paramref name="foot"/> (top surface, low end) rising <paramref name="rise"/>
        /// over <paramref name="run"/> along <paramref name="dir"/>. Its sides stay vertical; <paramref name="thick"/>
        /// hangs below the walking surface (deep for road embankments, thin for stairs).</summary>
        public static GameObject Ramp(string name, Transform parent, Vector3 foot, Vector3 dir, float run, float rise, float width,
            float thick, Material material, bool collider = true)
        {
            dir.y = 0f;
            dir.Normalize();
            float slope = Mathf.Sqrt(run * run + rise * rise);
            float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(-angle, 0f, 0f);
            Vector3 surfaceMid = foot + dir * (run * .5f) + Vector3.up * (rise * .5f);
            Vector3 centre = surfaceMid - rot * Vector3.up * (thick * .5f);
            GameObject go = Box(name, parent, centre, new Vector3(width, thick, slope), material, collider);
            go.transform.rotation = rot;
            go.transform.position = centre;
            return go;
        }

        /// <summary>A world-space box (position and size in world units) under any parent.</summary>
        public static GameObject WorldBox(string name, Transform parent, Vector3 centre, Vector3 size, Material material, bool collider = true)
        {
            GameObject go = Box(name, parent, Vector3.zero, size, material, collider);
            go.transform.position = centre;
            return go;
        }

        /// <summary>A soft-edged flat patch on the floor (a puddle, soot, moss, leaf litter): a flattened faceted disc,
        /// never a box (a box decal shows hard rectangle edges). No collider; the finishing pass lifts it clear.</summary>
        public static GameObject Patch(string name, Transform parent, Vector3 centre, Vector2 size, float yaw, Material material)
        {
            GameObject go = PolishAssets.Shape(name, parent, centre, new Vector3(size.x, .03f, size.y), material, true);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.position = centre;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Broken rock: a few tilted, shrinking blocks stacked on each other (sea stacks, islets, outcrops), angular
        /// like split stone. <paramref name="size"/> is the whole rock; one box collider fits its core when asked.</summary>
        public static Transform Rock(string name, Transform parent, Vector3 baseCentre, Vector3 size, Material material, int seed, bool collider)
        {
            Transform root = GreyboxFactory.Group(name, parent);
            root.position = baseCentre;
            int blocks = 3 + (int)(Hash(seed, 1) * 3f);
            float y = 0f;
            for (int i = 0; i < blocks; i++)
            {
                float t = i / (float)blocks, h = size.y / blocks * (1.15f - Hash(seed, i + 7) * .3f);
                float w = size.x * (1f - t * .45f) * (.8f + Hash(seed, i + 13) * .35f), d = size.z * (1f - t * .45f) * (.8f + Hash(i + 13, seed) * .35f);
                var off = new Vector3((Hash(seed, i + 19) - .5f) * size.x * .18f, y + h * .5f, (Hash(i + 19, seed) - .5f) * size.z * .18f);
                RotBox("Block", root, off, new Vector3(w, h, d),
                    Quaternion.Euler((Hash(seed, i + 23) - .5f) * 14f, Hash(seed, i + 29) * 90f, (Hash(i + 29, seed) - .5f) * 14f), material, false);
                y += h * .85f;
            }
            if (collider)
            {
                var c = root.gameObject.AddComponent<BoxCollider>();
                c.center = new Vector3(0f, y * .5f, 0f);
                c.size = new Vector3(size.x * .7f, y, size.z * .7f);
            }
            return root;
        }

        /// <summary>A box from its min and max corners, in world space.</summary>
        public static GameObject Span(string name, Transform parent, Vector3 min, Vector3 max, Material material, bool collider = true) =>
            WorldBox(name, parent, (min + max) * .5f, max - min, material, collider);

        /// <summary>Deterministic 0-1 hash of an integer pair: the same dressing every rebuild.</summary>
        public static float Hash(int a, int b)
        {
            unchecked
            {
                uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ 0x9E3779B9u;
                h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
                return (h & 0xFFFFFF) / 16777215f;
            }
        }
    }
}

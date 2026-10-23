using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    // All decks share the same core cutouts. No full slab may cross a vertical circulation volume.
    public static class SkyscraperArchitecture
    {
        public static readonly Rect Shaft = Rect.MinMaxRect(-2.4f, -2.4f, 2.4f, 2.4f);
        public static readonly Rect Stairs = Rect.MinMaxRect(-15.6f, -5f, -8.4f, 7f);
        public static readonly Vector2[] DiningTables = {
            new(-4.5f, 8f), new(4.5f, 8f), new(-4.5f, 13f), new(4.5f, 13f),
            new(-4.5f, 18f), new(4.5f, 18f)
        };

        public static List<Transform> Deck(Transform parent, float y, Material material, bool roof = false)
        {
            var result = new List<Transform>();
            for (float x = -18f; x < 18f; x += 4f)
            for (float z = -10f; z < 22f; z += 4f)
            {
                var pieces = new List<Rect> { new Rect(x, z, 4f, 4f) };
                if (!roof) pieces = Subtract(pieces, Shaft);
                pieces = Subtract(pieces, Stairs);
                foreach (Rect r in pieces)
                    if (r.width > .01f && r.height > .01f)
                        result.Add(Slab(parent, new Vector3(r.center.x, y, r.center.y), r.size, material));
            }
            return result;
        }

        private static List<Rect> Subtract(List<Rect> input, Rect hole)
        {
            var output = new List<Rect>();
            foreach (Rect r in input)
            {
                if (!r.Overlaps(hole)) { output.Add(r); continue; }
                float l = Mathf.Max(r.xMin, hole.xMin), b = Mathf.Max(r.yMin, hole.yMin);
                float h = Mathf.Min(r.yMax, hole.yMax), right = Mathf.Min(r.xMax, hole.xMax);
                if (l > r.xMin) output.Add(Rect.MinMaxRect(r.xMin, r.yMin, l, r.yMax));
                if (right < r.xMax) output.Add(Rect.MinMaxRect(right, r.yMin, r.xMax, r.yMax));
                if (b > r.yMin) output.Add(Rect.MinMaxRect(l, r.yMin, right, b));
                if (h < r.yMax) output.Add(Rect.MinMaxRect(l, h, right, r.yMax));
            }
            return output;
        }

        public static Transform Slab(Transform parent, Vector3 at, Vector2 size, Material material)
        {
            Transform slab = Group($"Deck_{at.x:0.0}_{at.z:0.0}", parent);
            slab.position = at;
            var collider = slab.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.down * .2f;
            collider.size = new Vector3(size.x, .4f, size.y);
            Box("Surface", slab, collider.center, collider.size, material, false);
            slab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            return slab;
        }

        public static void Wall(Transform p, string name, Vector3 center, Vector3 size, Material mat) => Box(name, p, center, size, mat);

        // Portal pieces never include a collider across the opening.
        public static void Portal(Transform p, Vector3 at, float width, float height, Material mat, bool alongZ = false)
        {
            Transform g = Group("OpenPortal", p); g.position = at;
            if (alongZ) g.localRotation = Quaternion.Euler(0f, 90f, 0f);
            Box("JambL", g, new Vector3(-width / 2f - .08f, height / 2f, 0), new Vector3(.16f, height, .28f), mat);
            Box("JambR", g, new Vector3(width / 2f + .08f, height / 2f, 0), new Vector3(.16f, height, .28f), mat);
            Box("Lintel", g, new Vector3(0, height + .08f, 0), new Vector3(width + .32f, .16f, .28f), mat);
        }

        public static void Rail(Transform p, Vector3 a, Vector3 b, Material mat)
        {
            Vector3 middle = (a + b) / 2f, delta = b - a;
            Transform rail = Group("SafetyRail", p);
            rail.position = middle + Vector3.up * 1.1f;
            Box("Handrail", rail, Vector3.zero, new Vector3(.07f, .07f, delta.magnitude), mat, false)
                .transform.rotation = Quaternion.LookRotation(delta);
            int count = Mathf.CeilToInt(delta.magnitude / 1.5f);
            for (int i = 0; i <= count; i++)
                Box("Post", p, Vector3.Lerp(a, b, (float)i / count) + Vector3.up * .55f, new Vector3(.07f, 1.1f, .07f), mat, false);
            // Simple continuous collider keeps loose salvage and players behind the detailed rails.
            Transform guard = Group("GuardCollider", p); guard.position = middle + Vector3.up * .55f;
            guard.rotation = Quaternion.LookRotation(delta);
            var c = guard.gameObject.AddComponent<BoxCollider>(); c.size = new Vector3(.08f, 1.1f, delta.magnitude);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using Kind = Abandoned.EditorTools.IronFactoryCatwalkPlan.Kind;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L1: the floor and walls tell where the heat was. Scorch and cooled spatter crowd the lava's banks; oil and rust
    /// drips pool under the catwalks; cracks run across the machinery floor; damp and moss creep in from the cold
    /// corners far from the channel; and soot climbs the walls, nearly black under the roof. All flat quads just
    /// off the surface: no colliders, no shadows, seeded so every rebuild is the same.
    /// </summary>
    public static class IronFactoryDecals
    {
        private const float Lift = .012f;

        public static int Build(Transform root, IronFactoryArt art, Dictionary<Vector2Int, Kind>[] cells)
        {
            Transform group = Group("Decals", root);
            var rng = new System.Random(4409);
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            int count = 0;
            Rect c = L.ChannelMeters;

            // Scorch along both banks, thicker near the edge; spatter (small scorch) further out.
            for (float z = c.yMin - 2f; z <= c.yMax + 2f; z += 2.6f)
            foreach (float side in new[] { -1f, 1f })
            {
                float edge = side < 0 ? c.xMin : c.xMax;
                Floor(group, art.Scorch, new Vector3(edge + side * R(.6f, 2.4f), 0f, z + R(-1f, 1f)), R(1.6f, 3.2f), R(0f, 360f));
                if (rng.NextDouble() < .6) Floor(group, art.Scorch, new Vector3(edge + side * R(3f, 7f), 0f, z + R(-1f, 1f)), R(.4f, .9f), R(0f, 360f));
                count += 2;
            }

            // Oil and drips under the tier-1 catwalks, wherever there is floor beneath.
            foreach (Vector2Int cell in cells[1].Keys)
            {
                if (L.InChannel(cell) || IronFactoryCatwalkPlan.IsRoom(cell) || rng.NextDouble() > .55) continue;
                Floor(group, art.Oil, L.CellCentre(cell) + new Vector3(R(-1.2f, 1.2f), 0f, R(-1.2f, 1.2f)), R(1f, 2.4f), R(0f, 360f));
                count++;
            }

            // Cracks anywhere on the open floor; damp and moss in the cold corners (far from the lava, near the walls).
            foreach (Vector2Int cell in L.GroundCells())
            {
                if (IronFactoryCatwalkPlan.IsRoom(cell)) continue;
                Vector3 at = L.CellCentre(cell) + new Vector3(R(-1.5f, 1.5f), 0f, R(-1.5f, 1.5f));
                if (rng.NextDouble() < .12) { Floor(group, art.Crack, at, R(1.2f, 2.8f), R(0f, 360f)); count++; }
                float fromLava = Mathf.Min(Mathf.Abs(at.x - c.xMin), Mathf.Abs(at.x - c.xMax));
                bool cold = fromLava > 30f && (cell.x <= 1 || cell.x >= L.Width - 2 || cell.y >= L.Depth - 2);
                if (cold && rng.NextDouble() < .5) { Floor(group, art.Moss, at, R(1.4f, 3f), R(0f, 360f)); count++; }
            }

            // Soot on the inside of the outer walls: from 5 m up to the roof, darkest at the top. A quad shows its
            // front toward -Z, so each is turned to face into the hall.
            float w = L.Width * L.Tile, d = L.Depth * L.Tile, inset = L.WallThickness * .5f + .02f, from = 5f;
            float h = L.WallHeight - from, mid = from + h * .5f;
            Wall(group, art.SootVeil, new Vector3(w * .5f, mid, inset), new Vector3(w, h, 1f), 180f);
            Wall(group, art.SootVeil, new Vector3(w * .5f, mid, d - inset), new Vector3(w, h, 1f), 0f);
            Wall(group, art.SootVeil, new Vector3(inset, mid, d * .5f), new Vector3(d, h, 1f), -90f);
            Wall(group, art.SootVeil, new Vector3(w - inset, mid, d * .5f), new Vector3(d, h, 1f), 90f);
            return count + 4;
        }

        private static void Floor(Transform group, Material m, Vector3 at, float size, float yaw)
        {
            GameObject quad = Primitive(PrimitiveType.Quad, m.name, group, new Vector3(at.x, Lift, at.z), new Vector3(size, size, 1f), m, false);
            quad.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
            Quiet(quad);
        }

        private static void Wall(Transform group, Material m, Vector3 at, Vector3 size, float yaw)
        {
            GameObject quad = Primitive(PrimitiveType.Quad, "SootVeil", group, at, size, m, false);
            quad.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Quiet(quad);
        }

        private static void Quiet(GameObject go)
        {
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = true;
        }
    }
}

using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// TestMap showcase walls: the same colliding wall panel as before (GreyboxWall), finished inside in paint, with a
    /// brick skin a hand's width outside it, steel door and window frames sitting in the openings, glazing, precast
    /// sills and lintels. Faces never share a plane with a neighbour (the skin stands 5 mm off the wall, frames stand
    /// proud of both faces), so nothing flickers.
    /// </summary>
    public static class ShowcaseWalls
    {
        public const float SkinGap = .005f, SkinThickness = .1f;
        private const float Member = .06f;   // frame section
        private const float Proud = .02f;    // frames stand this far off each face

        public enum Kind { Door, LoadingDoor, Window, OpenWindow }

        /// <param name="start">Panel start on the wall centreline, at the panel's base height (as GreyboxWall).</param>
        /// <param name="outward">Unit vector out of the building.</param>
        public static void Panel(Transform parent, string name, Vector3 start, Vector3 direction, Vector3 outward, float length,
            float height, float thickness, WallOpening? hole, Kind kind, Material inside)
        {
            GreyboxWall.Panel(parent, name, start, direction, length, height, thickness, hole, inside, inside);
            StripFrames(parent.GetChild(parent.childCount - 1));
            float skin = thickness / 2f + SkinGap + SkinThickness / 2f;
            GreyboxWall.Panel(parent, name + "_Brick", start + outward * skin, direction, length, height, SkinThickness, hole,
                ShowcaseMaterials.Brick, ShowcaseMaterials.Brick);
            StripFrames(parent.GetChild(parent.childCount - 1));
            if (hole == null) return;

            WallOpening o = hole.Value;
            var frame = Group(name + "_Opening", parent);
            frame.SetPositionAndRotation(start + direction * (length / 2f) + Vector3.up * o.Bottom, Quaternion.LookRotation(outward));
            float inner = -thickness / 2f, outer = thickness / 2f + SkinGap + SkinThickness;
            Opening(frame, o, kind, inner, outer);
        }

        /// <summary>Brick corner blocks closing the skin at the four corners, for one storey.</summary>
        public static void Corners(Transform parent, Vector3 min, Vector3 max, float y0, float height, float thickness)
        {
            // Distances from a wall's centreline: its inner face, and the skin's outer face.
            float inset = .0025f, half = thickness / 2f, reach = half + SkinGap + SkinThickness;
            float[] xs = { min.x - half, max.x + half }, zs = { min.z - half, max.z + half }; // corner centrelines
            for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                float sx = i == 0 ? -1f : 1f, sz = j == 0 ? -1f : 1f;
                // From just short of the walls' inner faces (inside the corner post, never on a visible plane) to the skin.
                float x0 = xs[i] - sx * (half - inset), x1 = xs[i] + sx * reach;
                float z0 = zs[j] - sz * (half - inset), z1 = zs[j] + sz * reach;
                var c = new Vector3((x0 + x1) / 2f, y0 + height / 2f, (z0 + z1) / 2f);
                Box($"BrickCorner_{i}{j}", parent, c, new Vector3(Mathf.Abs(x1 - x0), height, Mathf.Abs(z1 - z0)), ShowcaseMaterials.Brick);
            }
        }

        /// <summary>A precast band around the building at <paramref name="y"/>: the floor line seen from outside.</summary>
        public static void StringCourse(Transform parent, Vector3 min, Vector3 max, float y, float thickness)
        {
            const float depth = .03f, tall = .22f;
            float face = thickness / 2f + SkinGap + SkinThickness; // skin outer face, from the centreline
            float x0 = min.x - thickness / 2f - face, x1 = max.x + thickness / 2f + face;
            float z0 = min.z - thickness / 2f - face, z1 = max.z + thickness / 2f + face;
            Material m = ShowcaseMaterials.Sill;
            // South and north run past the corners; west and east stop at them, so no two bands overlap.
            Box("Band_S", parent, new Vector3((x0 + x1) / 2f, y, z0 - depth / 2f), new Vector3(x1 - x0 + depth * 2f, tall, depth), m, false);
            Box("Band_N", parent, new Vector3((x0 + x1) / 2f, y, z1 + depth / 2f), new Vector3(x1 - x0 + depth * 2f, tall, depth), m, false);
            Box("Band_W", parent, new Vector3(x0 - depth / 2f, y, (z0 + z1) / 2f), new Vector3(depth, tall, z1 - z0), m, false);
            Box("Band_E", parent, new Vector3(x1 + depth / 2f, y, (z0 + z1) / 2f), new Vector3(depth, tall, z1 - z0), m, false);
        }

        // Local frame: x along the wall, y up from the opening's sill, z out of the building.
        private static void Opening(Transform frame, WallOpening o, Kind kind, float inner, float outer)
        {
            float w = o.Width, h = o.Height;
            float z0 = inner - Proud, z1 = outer + Proud, depth = z1 - z0, zc = (z0 + z1) / 2f;
            Material steel = ShowcaseMaterials.DoorFrame;
            if (kind == Kind.LoadingDoor)
            {
                // Roller-shutter guide channels, and the coil box over the opening on the inside.
                Box("Guide_L", frame, new Vector3(-w / 2f + .05f, h / 2f, inner - .05f), new Vector3(.1f, h, .1f), steel, false);
                Box("Guide_R", frame, new Vector3(w / 2f - .05f, h / 2f, inner - .05f), new Vector3(.1f, h, .1f), steel, false);
                Box("CoilBox", frame, new Vector3(0f, h + .1f, inner - .2f), new Vector3(w + .3f, .19f, .38f), steel, false);
                Box("Jamb_L", frame, new Vector3(-w / 2f + Member / 2f, h / 2f, outer - .04f), new Vector3(Member, h, .1f), steel, false);
                Box("Jamb_R", frame, new Vector3(w / 2f - Member / 2f, h / 2f, outer - .04f), new Vector3(Member, h, .1f), steel, false);
                Lintel(frame, w, h, outer);
                return;
            }

            Box("Jamb_L", frame, new Vector3(-w / 2f + Member / 2f, h / 2f, zc), new Vector3(Member, h, depth), steel, false);
            Box("Jamb_R", frame, new Vector3(w / 2f - Member / 2f, h / 2f, zc), new Vector3(Member, h, depth), steel, false);
            Box("Head", frame, new Vector3(0f, h - Member / 2f, zc), new Vector3(w - Member * 2f, Member, depth), steel, false);
            Lintel(frame, w, h, outer);
            if (kind == Kind.Door)
            {
                // The leaf stands open, flat against the inside wall beside the hinge jamb.
                GameObject leaf = Box("Leaf", frame, new Vector3(w / 2f + .6f, (h - .02f) / 2f + .01f, inner - .035f),
                    new Vector3(1.16f, h - .02f, .045f), steel);
                Box("Handle", leaf.transform.parent, new Vector3(w / 2f + 1.1f, 1.02f, inner - .075f), new Vector3(.12f, .025f, .035f),
                    ShowcaseMaterials.Steel, false);
                return;
            }

            // Windows: the bottom rail, a precast sill outside and (unless it is the broken rope-route window) glass.
            Box("Rail", frame, new Vector3(0f, .01f + Member / 2f, zc), new Vector3(w - Member * 2f, Member, depth), steel, false);
            Box("Sill", frame, new Vector3(0f, -.025f, outer + .05f), new Vector3(w + .24f, .07f, .14f), ShowcaseMaterials.Sill, false);
            if (kind == Kind.OpenWindow) return;
            Box("Mullion", frame, new Vector3(0f, h / 2f, 0f), new Vector3(.05f, h - Member * 2f, .08f), steel, false);
            GameObject glass = Box("Glass", frame, new Vector3(0f, h / 2f + .005f, 0f), new Vector3(w - Member * 2f, h - Member * 2f - .01f, .012f),
                ShowcaseMaterials.Glass);
            glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // A precast lintel plate over the opening, a few millimetres proud of the brick.
        private static void Lintel(Transform frame, float w, float h, float outer) =>
            Box("Lintel", frame, new Vector3(0f, h + .1f, outer + .015f), new Vector3(w + .32f, .2f, .02f), ShowcaseMaterials.Sill, false);

        /// <summary>Drops GreyboxWall's bright greybox frames from a panel it just built (ours replace them).</summary>
        public static void StripFrames(Transform panel)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
                if (panel.GetChild(i).name.StartsWith("Frame_")) Object.DestroyImmediate(panel.GetChild(i).gameObject);
        }
    }
}

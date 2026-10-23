using UnityEngine;
using B = Abandoned.EditorTools.StationProps;
using M = Abandoned.EditorTools.StationMeshes;
using R = Abandoned.EditorTools.StationMeshes.Ring;

namespace Abandoned.EditorTools
{
    /// <summary>The small parts the station furniture is built from: padded cushions, drawer fronts with pulls and
    /// locks, five-star chair bases on casters, and the scans that stand on desks and shelves.</summary>
    public static partial class StationFurniture
    {
        /// <summary>A padded cushion (a soft loft), <paramref name="size"/> wide, high and deep, centred at <paramref name="centre"/>.</summary>
        private static GameObject Cushion(Transform t, StationArt a, string name, Vector3 centre, Vector3 size, Material m, float tilt = 0f)
        {
            float w = Snap(size.x), h = Snap(size.y), d = Snap(size.z);
            Mesh mesh = M.Loft(a, $"Cushion_{Mathf.RoundToInt(w * 100)}_{Mathf.RoundToInt(h * 100)}_{Mathf.RoundToInt(d * 100)}", new[]
            {
                new R(-d * .5f, -h * .32f, h * .32f, w * .44f, w * .44f, 3f), new R(-d * .5f + .03f, -h * .5f, h * .5f, w * .5f, w * .49f, 4f),
                new R(d * .5f - .03f, -h * .5f, h * .5f, w * .5f, w * .49f, 4f), new R(d * .5f, -h * .32f, h * .32f, w * .44f, w * .44f, 3f),
            }, 20);
            return M.Draw(t, name, mesh, centre, new Vector3(tilt, 0f, 0f), Vector3.one, m);
        }

        // Cushion sizes snap to 2 cm so pieces share their meshes.
        private static float Snap(float v) => Mathf.Max(.04f, Mathf.Round(v * 50f) / 50f);

        /// <summary>A drawer front at <paramref name="centre"/> (its face toward +z) with a recessed pull and, if asked, a lock.</summary>
        private static void DrawerFront(Transform t, StationArt a, Vector3 centre, float w, float h, bool locked = false, Material face = null)
        {
            B.B(t, "DrawerFront", centre, new Vector3(w, h, .015f), face != null ? face : a.Steel);
            B.B(t, "PullRecess", centre + new Vector3(0f, h * .25f, .009f), new Vector3(Mathf.Min(.16f, w * .5f), .035f, .005f), a.Rubber);
            B.B(t, "Pull", centre + new Vector3(0f, h * .25f - .01f, .014f), new Vector3(Mathf.Min(.15f, w * .45f), .012f, .012f), a.Chrome);
            B.B(t, "Label", centre + new Vector3(0f, h * .05f, .009f), new Vector3(.08f, .04f, .003f), a.Paper);
            if (locked) B.C(t, "Lock", centre + new Vector3(w * .38f, h * .32f, .01f), new Vector3(.025f, .008f, .025f), a.Chrome).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        /// <summary>A swivel chair's five-star base on casters with its gas column, to seat height <paramref name="seat"/>.</summary>
        private static void StarBase(Transform t, StationArt a, float seat, float spin)
        {
            B.C(t, "Column", new Vector3(0f, (seat - .1f) * .5f + .08f, 0f), new Vector3(.055f, (seat - .1f) * .5f, .055f), a.Chrome);
            B.C(t, "Shroud", new Vector3(0f, .2f, 0f), new Vector3(.075f, .09f, .075f), a.Rubber);
            B.C(t, "Hub", new Vector3(0f, .09f, 0f), new Vector3(.12f, .03f, .12f), a.Rubber);
            for (int i = 0; i < 5; i++)
            {
                float ang = (spin + i * 72f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(ang), 0f, Mathf.Cos(ang));
                M.Strut(t, "Spoke", new Vector3(0f, .09f, 0f), dir * .3f + Vector3.up * .065f, .045f, a.Rubber, .035f);
                GameObject caster = B.C(t, "Caster", dir * .31f + Vector3.up * .03f, new Vector3(.05f, .02f, .05f), a.Rubber);
                caster.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        /// <summary>A scan standing on a piece (a desk lamp, binders, a box of stationery): skipped if missing.</summary>
        private static void Prop(Transform t, string id, Vector3 local, float yaw, Vector3 fit) => StationScans.In(t, id, local, yaw, fit);

        private static int Seed(Vector3 at) => Mathf.Abs(Mathf.RoundToInt(at.x * 13f + at.z * 7f + at.y * 3f));
    }
}

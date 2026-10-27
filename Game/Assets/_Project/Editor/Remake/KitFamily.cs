using System.Text.RegularExpressions;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// One kind of map prop the remake models once and stands in place of every code-built copy (KitSwap): what its
    /// objects are called, how big a real one is, and how a model is fitted to the copy it replaces.
    /// Models: Art/Remake/Maps/Kit/Kit_&lt;Id&gt;_&lt;variant&gt;.fbx (Tools/Blender/remake/maps/kit.py), front +Z, standing on y 0.
    /// </summary>
    public sealed class KitFamily
    {
        public enum Fit
        {
            /// <summary>Stands on the floor; the footprint may stretch a little against the height (furniture).</summary>
            Stand,
            /// <summary>Fills the box on every axis (windows, frames, radiators: shapes that come in any size).</summary>
            Stretch,
            /// <summary>Hangs where the old one hung, fitted on every axis within limits (wall and ceiling fittings).</summary>
            Hang,
        }

        public readonly string Id;
        public readonly Regex Name;
        public readonly Regex Not;
        /// <summary>A real one's size: length (the longer side on the floor), height, depth, in metres.</summary>
        public readonly Vector3 Typical;
        /// <summary>How far a copy may be from <see cref="Typical"/> on each side and still be this (not a part of one).</summary>
        public readonly float Tolerance;
        public readonly Fit Fitting;
        /// <summary>Turn its back to the nearest wall (lockers, shelving); otherwise it keeps the copy's long axis.</summary>
        public readonly bool AgainstWall;
        /// <summary>The model's length runs along X (a bench, a desk); the copy's longer side decides its turn.</summary>
        public readonly bool Long;
        /// <summary>One variant for the whole map (windows: a facade in one style), not one per copy.</summary>
        public readonly bool PerMap;

        public KitFamily(string id, string name, Vector3 typical, Fit fit = Fit.Stand, bool againstWall = false,
                         bool along = true, string not = null, float tolerance = 2f, bool perMap = false)
        {
            Id = id;
            Name = new Regex("(?i)" + name);
            Not = not != null ? new Regex("(?i)" + not) : null;
            Typical = typical;
            Tolerance = tolerance;
            Fitting = fit;
            AgainstWall = againstWall;
            Long = along;
            PerMap = perMap;
        }

        public bool Matches(string objectName) => Name.IsMatch(objectName) && (Not == null || !Not.IsMatch(objectName));

        /// <summary>Null when a copy of this real size can be this; otherwise which side is off.</summary>
        public string Sized(Vector3 real)
        {
            float a = Mathf.Max(real.x, real.z), b = Mathf.Min(real.x, real.z);
            float w = Mathf.Max(Typical.x, Typical.z), d = Mathf.Min(Typical.x, Typical.z);
            if (Off(a, w)) return a < w ? "short" : "long";
            if (Off(real.y, Typical.y)) return real.y < Typical.y ? "low" : "tall";
            // Depth only counts once it is solid: thin things (a sign, a window) vary most there.
            if (d >= .1f && Off(Mathf.Max(b, .02f), d)) return b < d ? "thin" : "deep";
            return null;
        }

        private bool Off(float have, float want) => have < want / Tolerance || have > want * Tolerance;
    }
}

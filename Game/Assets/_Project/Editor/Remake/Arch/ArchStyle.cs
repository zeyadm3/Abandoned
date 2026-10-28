using System.Text.RegularExpressions;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// How one map's architecture is built (remodel v2): which construction each of its surfaces gets, the trims along
    /// its walls (profiles in metres) and what they are made of, its ceilings and its columns. Every map has one
    /// (<see cref="ArchStyles"/>); the palettes follow the art bible in Docs/REMAKE_2026.md.
    /// </summary>
    public sealed class ArchStyle
    {
        /// <summary>What a box is made of, from its material's name. Skip = leave the box exactly as it is.</summary>
        public enum Surface { Skip, Flat, Ribbed, Masonry, Concrete, Stone, Tile, Boards, Steel, Glass, Floor, Ceiling }

        public enum CeilingKind { Plain, Grid, Formwork }
        public enum ColumnKind { Chamfer, SteelH, Clad }

        /// <summary>A trim: a profile swept along a run, made of a material the style names.</summary>
        public sealed class Trim
        {
            public Vector2[] Profile;
            public string Material;
            public Trim(string material, params Vector2[] profile) { Material = material; Profile = profile; }
            public float Height => Profile.Length == 0 ? 0f : Mathf.Max(System.Array.ConvertAll(Profile, p => p.y));
            public float Depth => Profile.Length == 0 ? 0f : Mathf.Max(System.Array.ConvertAll(Profile, p => p.x));
        }

        /// <summary>A material the trims use: a copy of one of the map's own surfaces (so it carries the map's scans and
        /// weathering), tinted; or a plain colour when the source isn't found.</summary>
        public sealed class Finish
        {
            public string Key, Source;
            public Color Tint;
            public float Smoothness, Metallic;
            /// <summary>The tint is the colour itself, not a multiplier of the source's (a dark source painted light).</summary>
            public bool Paint;
            public Finish(string key, string source, Color tint, float smoothness = .3f, float metallic = 0f, bool paint = false)
            {
                Key = key; Source = source; Tint = tint; Smoothness = smoothness; Metallic = metallic; Paint = paint;
            }
        }

        /// <summary>Surfaces whose body takes a finish instead of the box's own material, because the geometry now carries
        /// what the old scan only pictured (profiled sheet over real ribs, say).</summary>
        public System.Collections.Generic.Dictionary<Surface, string> Bodies = new();

        public string Map;
        public (Regex Pattern, Surface Surface)[] Rules = System.Array.Empty<(Regex, Surface)>();
        public Finish[] Finishes = System.Array.Empty<Finish>();

        public Trim Skirting, Cornice, Casing, Coping, Nosing, Plinth, ChairRail, WainscotCap, StringCourse;
        /// <summary>Tiled lower wall on interior faces of tiled or flat walls (0 = none) and its material.</summary>
        public float Wainscot;
        public string WainscotMaterial;
        public float ChairRailHeight;

        public CeilingKind Ceiling = CeilingKind.Plain;
        public string GridMaterial = "Metal";
        public string SoffitMaterial = "Soffit";
        public ColumnKind Columns = ColumnKind.Chamfer;
        public string ColumnTrim = "Metal";
        public string FrameMaterial = "Metal";

        public float RibPitch = .25f, RibDepth = .035f;
        public Vector2 ConcretePanel = new(2.4f, 1.2f);
        public Vector2 StonePanel = new(1.2f, .6f);
        public float BoardPitch = .2f;
        /// <summary>Exterior faces: a projecting band every this many metres up (storey lines), 0 = none.</summary>
        public float StoreyBands;

        public static Regex R(string pattern) => new(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

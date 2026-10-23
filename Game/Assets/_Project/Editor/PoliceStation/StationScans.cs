using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The photo-scanned Poly Haven models already in the project (CC0, Art/ThirdParty/PolyHaven, credited in
    /// Docs/ASSET_CREDITS.md) placed in a station: chairs, the sofa, desks, shelving, crates, barrels, street lamps,
    /// hydrants, power poles, air-conditioning units. Single-material scans go through <see cref="ShowcaseModels"/>,
    /// multi-material ones through <see cref="ShowcaseNature"/>; both measure the model, so its pivot and export scale
    /// never matter. A scan that is missing returns null and the caller keeps its code-built piece.
    /// </summary>
    public static class StationScans
    {
        // The kits fetched by Tools/Art/fetch_nature.py: one material per part, placed by part.
        private static readonly HashSet<string> Multi = new()
        {
            "modular_chainlink_fence", "modular_electricity_poles", "modular_metal_gutter", "fire_hydrant", "security_light",
            "utility_box_02", "portable_searchlight", "barrel_stove", "street_lamp_02", "Barrel_02", "plastic_crate_03",
            "old_military_crate", "small_lpg_tank", "portable_welding_cart", "security_camera_01", "ladder_sectioned_01",
            "rusted_wheel_rim_01", "concrete_road_barrier", "rock_moss_set_01", "boulder_01", "tree_stump_01", "dead_tree_trunk",
        };

        public static bool Has(string id) => ShowcaseModels.Has(id);

        /// <summary>
        /// Model <paramref name="id"/> standing on <paramref name="floor"/> (world), turned <paramref name="yaw"/> (world
        /// degrees), scaled down to fit <paramref name="fit"/> (width, height, depth) if it is bigger.
        /// </summary>
        /// <param name="solid">One box collider fitted to it.</param>
        /// <param name="still">Static for batching and occlusion: true outside, false for anything that may fall with a slab.</param>
        public static GameObject Put(Transform parent, string id, Vector3 floor, float yaw, Vector3 fit, bool solid = false, bool still = true)
        {
            if (!Has(id)) return null;
            GameObject g;
            if (Multi.Contains(id))
            {
                g = ShowcaseNature.Part(id, null, parent, floor, yaw, 1f, solid);
                if (g != null) Shrink(g.transform, fit);
            }
            else g = ShowcaseModels.Place(id, parent, floor, yaw, fit, solid);
            if (g == null) return null;
            if (still) GreyboxFactory.MarkStatic(g.transform);
            else Loose(g.transform);
            return g;
        }

        /// <summary>A scan inside a fitting <paramref name="piece"/>, at <paramref name="local"/> in the piece's frame.</summary>
        public static GameObject In(Transform piece, string id, Vector3 local, float localYaw, Vector3 fit, bool still = false) =>
            Put(piece, id, piece.TransformPoint(local), piece.eulerAngles.y + localYaw, fit, false, still);

        /// <summary>One named part of a multi-material kit (a fence post, a gate leaf, a gutter outlet).</summary>
        public static GameObject Part(Transform parent, string id, string part, Vector3 at, float yaw, float scale = 1f, bool solid = false)
        {
            if (!Has(id)) return null;
            GameObject g = ShowcaseNature.Part(id, part, parent, at, yaw, scale, solid);
            if (g != null) GreyboxFactory.MarkStatic(g.transform);
            return g;
        }

        /// <summary>A dressed power pole from the electricity-pole kit (preset_01 or preset_02), standing on <paramref name="at"/>.</summary>
        public static Transform Pole(Transform parent, Vector3 at, float yaw, bool second = false, float scale = 1.45f)
        {
            if (!Has("modular_electricity_poles")) return null;
            Transform t = ShowcaseNature.Group("modular_electricity_poles", second ? "preset_02" : "preset_01", parent, at, yaw, scale);
            GreyboxFactory.MarkStatic(t);
            return t;
        }

        private static void Shrink(Transform holder, Vector3 fit)
        {
            Bounds b = ShowcaseModels.LocalBounds(holder);
            if (b.size.sqrMagnitude < 1e-6f) return;
            float s = Mathf.Min(1f, fit.x / Mathf.Max(b.size.x, .001f), fit.y / Mathf.Max(b.size.y, .001f), fit.z / Mathf.Max(b.size.z, .001f));
            if (s < .999f) holder.localScale *= s;
        }

        private static void Loose(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)0);
        }
    }
}

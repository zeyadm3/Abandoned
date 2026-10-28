using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Role = Abandoned.EditorTools.Remake.ArchBox.Role;
using Surface = Abandoned.EditorTools.Remake.ArchStyle.Surface;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Finds a map's architecture: every active box renderer, sorted into what it is (wall, slab, ceiling, column, beam,
    /// step, pane, roof) by its shape and into what it is made of by its material. Left alone: anything that moves or does
    /// something (doors, shutters, lockers, loot, vehicles, lights, machines), anything inside a prop or a LOD group, the
    /// remake's own models, and boxes too small or thin to be building fabric.
    /// </summary>
    public static class ArchClassify
    {
        private static readonly Regex Active = ArchStyle.R(
            @"door|locker|shutter|hiding|grab|loot|vehicle|truck|boat|elevator|lift|ladder|interact|network|rigidbody|animator|" +
            @"animation|lava|spawn|swing|sway|fixture|light|machine|hazard|breaker|conveyor|terminal|station|screen|kitswap|" +
            @"archrecord|mapsite|cargo|ignition|display|drone|camera|audio|particle");

        private static readonly Regex PropName = ArchStyle.R(
            @"chair|table|bench|desk|cabinet|locker|crate|pallet|barrel|bin\b|trash|computer|monitor|server|radio|phone|printer|" +
            @"fridge|cooker|kitchen(?!wall)|toilet|sink|bath(?!room)|sofa|couch|bed\b|mattress|cushion|furniture|loot|tool|machine|" +
            @"conveyor|boiler|furnace|pump|battery|breaker|lamp|(?<!roof|sky|fan|side|f)light|drawer|shelf|shelv|counter|appliance|" +
            @"stove|oven|washer|generator|spool|suitcase|storage|container|bookcase|wardrobe|pedestal|seat|filing|truck|van\b|" +
            @"car\b|bus\b|boxcar|forklift|skip\b|dumpster|mold|ingot|ladle|crane|hvac|duct|pipe|cable|sign\b|signboard|" +
            @"noticeboard|whiteboard|chalkboard|keyboard|cardboard|poster|picture|rug|curtain(?!glaz|wall|_?glass)|blind|mirror|vanity|cooler|planter|" +
            @"bollard|fence|gate|barrier|railing|handrail|baluster|rail_|_rail|guard|trophy|clipboard|pegboard|scale|readout|" +
            @"chute|\btub|kiosk|awning|canopy|flag|mast|antenna|lattice|picket|lod\d|_far|fractur|debris|rubble|remnant|kit_|rm_");

        private static readonly Regex StepName = ArchStyle.R(@"step|tread|riser|stair");

        private static readonly HashSet<string> Passive = new()
        {
            "Transform", "MeshFilter", "MeshRenderer", "BoxCollider", "MeshCollider", "SurfaceTag", "SectionProp",
            "WalkThroughObject", "NavMeshModifier", "NavMeshModifierVolume", "NavMeshObstacle", "IntentionalInvisibleCollider",
        };

        public static List<ArchBox> Scan(Scene scene, ArchStyle style, out List<ArchBox> solids, Dictionary<string, int> skipped)
        {
            var rebuild = new List<ArchBox>();
            solids = new List<ArchBox>();
            var verdicts = new Dictionary<Transform, bool>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name is "_KitSwap" or "_MapSite" or "_Arch" || !root.activeInHierarchy) continue;
                foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(false))
                {
                    if (!r.enabled) continue;
                    ArchBox box = ArchBox.From(r);
                    if (box == null) continue;
                    if (Mathf.Max(box.Size.x, box.Size.y, box.Size.z) >= .3f) solids.Add(box);
                    string why = Why(box, style, verdicts);
                    if (why == null) rebuild.Add(box);
                    else
                    {
                        skipped.TryGetValue(why, out int n);
                        skipped[why] = n + 1;
                    }
                }
            }
            return rebuild;
        }

        /// <summary>Null when the box is rebuilt (its role and surface are then set); otherwise why not.</summary>
        private static string Why(ArchBox box, ArchStyle style, Dictionary<Transform, bool> verdicts)
        {
            box.Surface = ArchProfiles.Classify(style, box.Material);
            if (box.Surface == Surface.Skip) return "material";
            box.Kind = RoleOf(box);
            if (box.Kind == Role.None) return "shape";
            foreach (Component comp in box.Transform.GetComponents<Component>())
                if (comp == null || !Passive.Contains(comp.GetType().Name)) return "component";
            if (PropName.IsMatch(box.Transform.name)) return "prop";
            int depth = 0;
            for (Transform t = box.Transform.parent; t != null; t = t.parent, depth++)
            {
                if (!verdicts.TryGetValue(t, out bool ok))
                {
                    ok = true;
                    // A scene root carries level-wide settings (camera range, atmosphere), not something that moves.
                    if (t.parent != null)
                        foreach (Component comp in t.GetComponents<Component>())
                        if (comp == null || comp is LODGroup || Active.IsMatch(comp.GetType().Name)) ok = false;
                    verdicts[t] = ok;
                }
                if (!ok) return "inside something active";
                if (depth < 3 && PropName.IsMatch(t.name)) return "inside a prop";
            }
            return null;
        }

        private static Role RoleOf(ArchBox b)
        {
            Vector3 s = b.Size;
            if (!b.Upright)
            {
                float thin = Mathf.Min(s.x, s.y, s.z), other = s.x + s.y + s.z - thin - Mathf.Max(s.x, s.y, s.z);
                bool sheet = s.y <= thin + 1e-4f && thin <= .4f && other >= 1.5f;
                return sheet && b.Surface is Surface.Ribbed or Surface.Steel or Surface.Floor or Surface.Boards ? Role.Roof : Role.None;
            }
            float h = s.y, a = Mathf.Max(s.x, s.z), t = Mathf.Min(s.x, s.z);
            if (b.Surface == Surface.Glass)
                return t <= .15f && a >= .4f && h >= .3f ? Role.Pane : Role.None;
            if (b.Surface == Surface.Ceiling)
                return h <= .25f && t >= .8f ? Role.Slab : Role.None;
            bool treadLike = t >= .2f && t <= .6f && a >= .6f && a <= 6f;
            if (treadLike && h >= .1f && (h <= .3f || h <= .6f && StepName.IsMatch(b.Transform.name)) &&
                b.Surface is Surface.Concrete or Surface.Stone or Surface.Floor or Surface.Boards or Surface.Steel)
                return Role.Step;
            if (h >= .06f && h <= 1.2f && t >= .8f && h <= t * .4f) return Role.Slab;
            if (h >= 2f && t >= .15f && a <= 1.5f && a <= t * 2.5f) return Role.Column;
            if (t >= .04f && t <= 1f && a >= .5f && a >= t * 1.5f && h >= .4f) return Role.Wall;
            if (h >= .15f && h <= 1.2f && t >= .1f && t <= 1f && a >= 1.5f && b.Surface is Surface.Steel or Surface.Concrete or Surface.Stone or Surface.Flat)
                return Role.Beam;
            return Role.None;
        }
    }
}

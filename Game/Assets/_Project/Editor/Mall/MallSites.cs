using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Shared by the 0.16.1+ additions to the saved mall: opens and saves it, and finds free spots against
    /// walls or on open floor, checked the way the 0.16.0 lockers are (nothing solid, no dressing, no carry
    /// route, doorway, flight or spawn point, a floor under it). Sites are found by casting from every tile's
    /// middle, so they need no hand-picked coordinates and come out the same each time.
    /// </summary>
    public static class MallSites
    {
        public readonly struct Site
        {
            public readonly Vector3 At, Outward;
            public readonly int Floor;
            public readonly Vector2Int Tile;
            public readonly StructuralSection Section;
            public Site(Vector3 at, Vector3 outward, int floor, Vector2Int tile, StructuralSection section)
            { At = at; Outward = outward; Floor = floor; Tile = tile; Section = section; }
            public Quaternion Facing => Quaternion.LookRotation(Outward);
            public MallLayout.Zone? Zone => MallLayout.ZoneAt(Tile, Floor);
        }

        private static readonly Vector3[] Directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        // Groups under the mall that are the building itself (or its air): never in a site's way.
        private static readonly HashSet<string> Building = new()
        {
            "Tiles", "MallCeilings", "Roof", "Walls", "Basement", "Exterior", "MallFacade", "_LIGHTING_VOLUMETRICS",
            "MeridianAtmosphere", "MeridianFixtures", MeridianWorldSurroundings.GroupName, MeridianMountainHorizon.GroupName,
        };

        private static List<Bounds> taken;
        private static List<Bounds> dressing;
        private static MallRouteClearance routes;

        /// <summary>Opens the saved mall (batch mode never asks) and returns its root.</summary>
        public static Transform OpenMall(out Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Author the mall in Edit mode.");
            scene = SceneManager.GetSceneByPath(MallBuilder.ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;
                scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            }
            SceneManager.SetActiveScene(scene);
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Mall") return root.transform;
            throw new InvalidOperationException("The saved Mall root is missing.");
        }

        public static void SaveMall(Scene scene, string tag)
        {
            BuildScript.ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, MallBuilder.ScenePath))
                throw new InvalidOperationException($"Unity could not save the {tag} mall.");
            Debug.Log($"[{tag}] Saved the mall as {BuildScript.Version}.");
        }

        /// <summary>Replaces a named group under the mall (so an addition can be run again).</summary>
        public static Transform FreshGroup(Transform mall, string name)
        {
            Transform previous = mall.Find(name);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Begin(mall);
            return GreyboxFactory.Group(name, mall);
        }

        /// <summary>Takes a fresh look at the mall; sites claimed since the last call stay claimed until the next.</summary>
        public static void Begin(Transform mall)
        {
            Physics.SyncTransforms();
            routes = new MallRouteClearance(mall);
            dressing = Dressing(mall);
            taken = new List<Bounds>();
        }

        /// <summary>
        /// Free wall spots: a box <paramref name="size"/> (x along the wall, y up, z out of it) standing
        /// <paramref name="lift"/> above the floor against the wall, with standing room in front.
        /// </summary>
        public static List<Site> WallSites(Func<Vector2Int, int, bool> where, Vector3 size, float lift = 0f, bool needsFront = true)
        {
            var found = new List<Site>();
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            {
                var tile = new Vector2Int(x, z);
                if (MallLayout.IsVoid(tile, floor) || !where(tile, floor)) continue;
                Vector3 centre = MallLayout.TileTopCenter(tile, floor) + Vector3.up * 1.3f;
                foreach (Vector3 dir in Directions)
                {
                    if (!Physics.Raycast(centre, dir, out RaycastHit hit, MallLayout.Tile * 0.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (Vector3.Dot(hit.normal, -dir) < 0.95f) continue; // not a wall square to the building
                    if (hit.collider.GetComponentInParent<StructuralSection>() is StructuralSection s && s.CanCollapse) continue;
                    Vector3 at = new(hit.point.x, MallLayout.FloorY(floor), hit.point.z);
                    if (Check(at, -dir, size, lift, needsFront, out StructuralSection under) is null)
                        found.Add(new Site(at, -dir, floor, tile, under));
                }
            }
            return found;
        }

        /// <summary>Free floor spots at the middle of tiles (or nudged a metre off it), facing +Z.</summary>
        public static List<Site> FloorSites(Func<Vector2Int, int, bool> where, Vector3 size)
        {
            var found = new List<Site>();
            Vector3[] nudges = { Vector3.zero, new(1f, 0f, 1f), new(-1f, 0f, -1f), new(1f, 0f, -1f), new(-1f, 0f, 1f) };
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            {
                var tile = new Vector2Int(x, z);
                if (MallLayout.IsVoid(tile, floor) || !where(tile, floor)) continue;
                foreach (Vector3 nudge in nudges)
                {
                    Vector3 at = MallLayout.TileTopCenter(tile, floor) + nudge;
                    if (Check(at - Vector3.forward * (size.z * 0.5f + 0.04f), Vector3.forward, size, 0f, false, out StructuralSection under) is null)
                    {
                        found.Add(new Site(at, Vector3.forward, floor, tile, under));
                        break;
                    }
                }
            }
            return found;
        }

        /// <summary>Marks a site's box as used, so the next search does not hand it out again.</summary>
        public static void Claim(Site site, Vector3 size, float lift = 0f) =>
            taken.Add(Box(site.At, site.Outward, size, lift));

        // Why a box can't stand here, or null when it can.
        public static string Check(Vector3 at, Vector3 outward, Vector3 size, float lift, bool needsFront, out StructuralSection floor)
        {
            floor = null;
            Quaternion facing = Quaternion.LookRotation(outward);
            Bounds body = Box(at, outward, size, lift);
            Vector3 half = size * 0.5f - new Vector3(0.03f, 0.03f, 0.03f);
            Vector3 bodyCentre = at + outward * (size.z * 0.5f + 0.04f) + Vector3.up * (lift + size.y * 0.5f);
            if (Physics.CheckBox(bodyCentre, half, facing, ~0, QueryTriggerInteraction.Ignore)) return "something solid stands there";
            Bounds front = default;
            if (needsFront)
            {
                Vector3 frontCentre = at + outward * (size.z + 0.6f) + Vector3.up * 1f;
                var frontHalf = new Vector3(Mathf.Max(0.35f, size.x * 0.5f - 0.03f), 0.85f, 0.5f);
                if (Physics.CheckBox(frontCentre, frontHalf, facing, ~0, QueryTriggerInteraction.Ignore)) return "something solid stands in front of it";
                front = new Bounds(frontCentre, Abs(facing * (frontHalf * 2f)));
            }
            foreach (Bounds piece in dressing)
                if (piece.Intersects(body) || (needsFront && piece.Intersects(front))) return "dressing stands there";
            foreach (Bounds used in taken)
                if (used.Intersects(body)) return "another addition stands there";
            string conflict = routes.Conflict(body);
            if (conflict != null) return conflict;
            Vector3 under = at + outward * (size.z * 0.5f + 0.04f) + Vector3.up * 0.5f;
            if (!Physics.Raycast(under, Vector3.down, out RaycastHit ground, 0.8f, ~0, QueryTriggerInteraction.Ignore)) return "there is no floor under it";
            floor = ground.collider.GetComponentInParent<StructuralSection>();
            return null;
        }

        private static Bounds Box(Vector3 at, Vector3 outward, Vector3 size, float lift)
        {
            Quaternion facing = Quaternion.LookRotation(outward);
            Vector3 centre = at + outward * (size.z * 0.5f + 0.04f) + Vector3.up * (lift + size.y * 0.5f);
            return new Bounds(centre, Abs(facing * size));
        }

        private static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        // Everything standing in the mall that isn't the building: fittings, stock, posters, earlier additions.
        private static List<Bounds> Dressing(Transform mall)
        {
            var found = new List<Bounds>();
            foreach (Transform group in mall)
            {
                if (Building.Contains(group.name)) continue;
                foreach (Renderer renderer in group.GetComponentsInChildren<Renderer>(false))
                {
                    if (!renderer.enabled || renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer) continue;
                    Material material = renderer.sharedMaterial;
                    if (material == null || material.renderQueue >= 3000 || renderer.name.Contains("Beam")) continue;
                    found.Add(renderer.bounds);
                }
            }
            return found;
        }

        /// <summary>Picks up to <paramref name="count"/> sites spread out: each at least <paramref name="apart"/> m from the others.</summary>
        public static List<Site> Spread(List<Site> sites, int count, float apart, int seed)
        {
            var picked = new List<Site>();
            var order = new List<Site>(sites);
            var random = new System.Random(seed);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            foreach (Site s in order)
            {
                if (picked.Count >= count) break;
                bool near = false;
                foreach (Site p in picked) if (Vector3.Distance(p.At, s.At) < apart) { near = true; break; }
                if (!near) picked.Add(s);
            }
            return picked;
        }

        /// <summary>Binds an addition to the floor it stands on: gone with it, back next run.</summary>
        public static void Bind(GameObject added, StructuralSection floor)
        {
            if (floor != null) added.AddComponent<SectionProp>().EditorSetup(floor);
        }

        /// <summary>
        /// Free floor for something flat and walk-through (litter, puddles, marks): only solid things at shin
        /// height and the flights' approaches rule a spot out, not carry routes or doorways.
        /// </summary>
        public static List<Site> FlatSites(Func<Vector2Int, int, bool> where, float half)
        {
            var found = new List<Site>();
            Vector3[] nudges = { Vector3.zero, new(1.2f, 0f, 0.6f), new(-1.1f, 0f, -0.8f), new(0.7f, 0f, -1.2f), new(-0.8f, 0f, 1.1f) };
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            for (int z = 0; z < MallLayout.TilesZ; z++)
            for (int x = 0; x < MallLayout.TilesX; x++)
            {
                var tile = new Vector2Int(x, z);
                if (MallLayout.IsVoid(tile, floor) || !where(tile, floor)) continue;
                if (MallLayout.AllFlights.Any(f => f.Tiles.Contains(tile) && (f.Floor == floor || f.Floor + 1 == floor))) continue;
                foreach (Vector3 nudge in nudges)
                {
                    Vector3 top = MallLayout.TileTopCenter(tile, floor) + nudge + Vector3.up * 1f;
                    if (!Physics.Raycast(top, Vector3.down, out RaycastHit ground, 1.4f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (Mathf.Abs(ground.point.y - MallLayout.FloorY(floor)) > 0.2f) continue; // landed on furniture
                    if (Physics.CheckBox(ground.point + Vector3.up * 0.3f, new Vector3(half, 0.22f, half), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore)) continue;
                    var box = new Bounds(ground.point + Vector3.up * 0.1f, new Vector3(half * 2f, 0.2f, half * 2f));
                    if (taken.Any(t => t.Intersects(box))) continue;
                    found.Add(new Site(ground.point, Vector3.forward, floor, tile, ground.collider.GetComponentInParent<StructuralSection>()));
                    break;
                }
            }
            return found;
        }
    }
}

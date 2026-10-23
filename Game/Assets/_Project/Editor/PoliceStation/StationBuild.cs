using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds a station's scene from scratch, the way the Iron Factory and the island are built: the slabs and flights,
    /// the walls and the shell, the station's own fittings, the light, the lot and the truck; then the structure, the
    /// loot and monster points (each checked against what was built), the shared run plumbing, the NavMesh, the save and
    /// the occlusion bake. Re-running it rebuilds the scene.
    /// </summary>
    public static class StationBuild
    {
        public static void Build(StationRecipe recipe)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild a station in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            StationPlan plan = recipe.Plan;
            // First, while no scene of ours is open: the icon pass renders in a scene of its own.
            recipe.EnsureLoot();
            recipe.Location();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(plan.SceneName).transform;
            Vector3 size = plan.SizeMeters;
            var building = new Bounds(new Vector3(size.x * .5f, plan.ParapetTop * .5f - 2.5f, size.z * .5f), new Vector3(size.x, plan.ParapetTop + 5f, size.z));
            (Color haze, float density) = recipe.InteriorHaze;
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(building, haze, density);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(building);

            StationArt art = recipe.CreateArt();
            StationFloors.Result floors = StationFloors.Build(root, plan, art);
            var setup = new StructureSceneSetup();
            if (!setup.IsValid) throw new InvalidOperationException($"Missing structural content for {plan.DisplayName}.");
            Dictionary<Vector3Int, StructuralSection> sections = StationFloors.AddSections(setup, plan, floors);
            int walls = StationWalls.BuildInterior(root, plan, art);
            int shell = StationFacade.Build(root, plan, art);
            shell += StationFacadeDetail.Apply(root, plan, art);

            var ctx = new StationContext(root, plan, art, setup, sections, plan.SceneName.ToLowerInvariant() + "_loot_");
            recipe.Dress(ctx);
            int lights = StationLighting.Build(root, plan, art, recipe.LampColour, plan.Seed);
            int emergency = StationLighting.Emergency(root, plan, art);
            recipe.Site(ctx);
            int bound = BindToSlabs(ctx);
            // Open-framed furniture and hiding shells are solid on purpose: mark them for the invisible-collider audit.
            StationCoverMarks.Apply(ctx.Props.Root);
            StationCoverMarks.Apply(StationSite.Exterior(root));
            int uvs = StationSurfaces.Apply(root, art);
            StructureSimulation simulation = setup.AddSimulation(plan.Stability, plan.Seed);

            List<LootPointSpec> loot = CheckLoot(ctx);
            List<Vector3> threats = CheckThreats(ctx, recipe);
            LocationPlumbing.Populate(root, recipe.CrewSpawns, loot, threats, simulation);
            string[] ignored = new[] { "Exterior", "Truck", "Shell/Roof", "MeridianCollectionBay", StationLighting.GroupName, "UnderStairs" }
                .Concat(recipe.NavMeshIgnored).ToArray();
            LocationPlumbing.BakeNavMesh(root, plan.SceneName, ignored);
            // Solid props (0.24.11), guarded by the first bake so no box cuts a route; then bake round them.
            int solid = LevelSolidity.Apply(root, plan.SceneName);
            // The second bake adds its own surface; drop the first so the level carries one.
            foreach (NavMeshSurface first in root.GetComponents<NavMeshSurface>()) Object.DestroyImmediate(first);
            LocationPlumbing.BakeNavMesh(root, plan.SceneName, ignored);
            LocationPlumbing.ConfigureHorrorScenes();
            AddHorrorScene(plan.SceneName);

            LevelAtmosphere.Apply(plan.SceneName, recipe.Mood, root);
            recipe.Sound(root);
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.transform.position = recipe.Camera.position;
                camera.transform.LookAt(recipe.Camera.lookAt);
            }
            BuildScenes.ApplyToEditorSettings();
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, plan.ScenePath))
                throw new InvalidOperationException($"Unity could not save {plan.DisplayName}.");
            // Only what never breaks occludes: walls and the shell, never a slab (they carry no static flags).
            if (!OcclusionBake.Bake()) throw new InvalidOperationException($"Unity could not bake {plan.DisplayName}'s occlusion.");
            if (!EditorSceneManager.SaveScene(scene, plan.ScenePath))
                throw new InvalidOperationException($"Unity could not save {plan.DisplayName} after the occlusion bake.");

            foreach (string problem in ctx.Props.Problems.Concat(ctx.Problems)) Debug.LogWarning($"[{plan.LogTag}] {problem}");
            int collapsible = plan.Collapsible.Keys.Count(k => sections.ContainsKey(k) && !plan.Pierced.ContainsKey(k));
            Debug.Log($"[{plan.LogTag}] Built {plan.DisplayName}: {sections.Count} slabs ({collapsible} can fall), {floors.Stairs.Count} flights, " +
                      $"{walls} wall pieces, {shell} shell pieces, {ctx.Props.Placed} fittings, {ctx.Hiding.Count} hiding places " +
                      $"({ctx.Hiding.Cells} cells, {ctx.Hiding.Lockers} lockers, {ctx.Hiding.Cabinets} cabinets, {ctx.Hiding.Cages} gear cages, " +
                      $"{ctx.Hiding.Closets} closets, {ctx.Hiding.Carts} carts), {emergency} emergency lights, " +
                      $"{lights} ceiling lights, {ctx.Breakers} breakers, {loot.Count}/{ctx.Loot.Count} loot points ({loot.Count(l => l.IsJackpot)} jackpot), " +
                      $"{threats.Count}/{ctx.Threats.Count} monster spawns, {bound} fittings on slabs that can fall, {solid} made solid, " +
                      $"{uvs} boxes with world-scale UVs, {ctx.Props.Problems.Count + ctx.Problems.Count} problems (warnings above).");
        }

        // Fittings standing on or hanging from a slab that can fall go with it (SectionProp); hiding places already do.
        private static int BindToSlabs(StationContext ctx)
        {
            int bound = 0;
            var pieces = new List<Transform>();
            foreach (Transform room in ctx.Props.Root) foreach (Transform piece in room) pieces.Add(piece);
            Transform lights = ctx.Root.Find(StationLighting.GroupName);
            if (lights != null) foreach (Transform fixture in lights) pieces.Add(fixture);
            foreach (Transform piece in pieces)
            {
                if (piece.GetComponentInChildren<HidingLocker>(true) != null) continue;
                StructuralSection section = ctx.SectionAt(piece.position);
                if (section == null || !section.CanCollapse) continue;
                SectionProp prop = piece.GetComponent<SectionProp>();
                if (prop == null) prop = piece.gameObject.AddComponent<SectionProp>();
                prop.EditorSetup(section);
                bound++;
            }
            return bound;
        }

        // A loot point must stand on a slab with nothing solid where the item will appear.
        private static List<LootPointSpec> CheckLoot(StationContext ctx)
        {
            Physics.SyncTransforms();
            var kept = new List<LootPointSpec>();
            foreach (LootPointSpec p in ctx.Loot)
            {
                string why = Blocked(p.At, p.IsJackpot ? .7f : .3f);
                if (why == null) kept.Add(p);
                else ctx.Problems.Add($"Loot point {p.Name} ({p.Tag}) at {p.At} left out: {why}.");
            }
            return kept;
        }

        private static List<Vector3> CheckThreats(StationContext ctx, StationRecipe recipe)
        {
            var kept = new List<Vector3>();
            foreach (Vector3 at in ctx.Threats)
            {
                string why = Blocked(at, .45f);
                if (why == null && Vector3.Distance(Flat(at), Flat(recipe.Entrance)) < recipe.MinThreatDistance) why = "it is too near the way in";
                if (why == null) kept.Add(at);
                else ctx.Problems.Add($"Monster spawn at {at} left out: {why}.");
            }
            return kept;
        }

        private static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        private static string Blocked(Vector3 at, float radius)
        {
            if (!Physics.Raycast(at + Vector3.up * .4f, Vector3.down, out RaycastHit floor, .9f, ~0, QueryTriggerInteraction.Ignore))
                return "no floor under it";
            if (floor.collider.GetComponentInParent<StructuralSection>() == null && !OnRoof(floor.collider.transform))
                return $"it stands on {floor.collider.name}, not a floor";
            foreach (Collider c in Physics.OverlapBox(at + Vector3.up * .95f, new Vector3(radius, .75f, radius), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            {
                StructuralSection s = c.GetComponentInParent<StructuralSection>();
                if (s != null && s.transform.position.y <= at.y + .05f && s.GetComponent<Collider>() == c) continue;
                return $"it is inside {c.transform.parent?.name}/{c.name}";
            }
            return null;
        }

        // The flat roof (Shell/Roof, reached by a hatch) is a floor too, though not a structural one.
        private static bool OnRoof(Transform t)
        {
            for (; t != null; t = t.parent)
                if (t.name == "Roof" && t.parent != null && t.parent.name == "Shell") return true;
            return false;
        }

        private static void AddHorrorScene(string sceneName)
        {
            HorrorConfig config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            if (config == null) return;
            var serialized = new SerializedObject(config);
            SerializedProperty levels = serialized.FindProperty("levels");
            if (levels == null) return;
            for (int i = 0; i < levels.arraySize; i++)
                if (levels.GetArrayElementAtIndex(i).stringValue == sceneName) return;
            levels.arraySize++;
            levels.GetArrayElementAtIndex(levels.arraySize - 1).stringValue = sceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }
    }
}

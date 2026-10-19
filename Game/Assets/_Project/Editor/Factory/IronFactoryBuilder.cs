using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the Iron Factory (Docs/IRON_FACTORY_PLAN.md) into Scenes/Factory.unity from scratch, the way the
    /// mall is built: layout data, then geometry, then the shared run plumbing, then the NavMesh. It replaces
    /// the small 0.24 factory; contracts that roll "Factory" get this map. Re-running it rebuilds the scene.
    /// </summary>
    public static class IronFactoryBuilder
    {
        /// <summary>Warm sodium-and-ember night: heavier bloom for the lava, haze that eats the far wall.</summary>
        private static readonly LevelAtmosphere.Mood Mood = new(
            // L2: darker ambient so the dead lamps leave real darkness and the working ones make pools of light.
            sky: new Color(.05f, .047f, .042f), equator: new Color(.032f, .028f, .024f), ground: new Color(.014f, .011f, .009f),
            fog: new Color(.17f, .13f, .10f), fogDensity: .024f, bloom: .3f, vignette: .27f, grain: .2f,
            saturation: -36f, contrast: 15f, sunColor: new Color(1f, .68f, .42f), sunIntensity: .10f, sunEuler: new Vector3(62f, -35f, 0f));

        [MenuItem("Tools/Abandoned/Factory/Rebuild Iron Factory")]
        public static void Rebuild() => Build();

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild the Iron Factory in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // First, while no scene of ours is open: its icon pass renders in a scene of its own.
            IronFactoryLoot.EnsureContent();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;
            // Inside: warm, smoky haze from the hot channel, thicker than the night air outside.
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(L.Building, new Color(.09f, .068f, .052f), .03f);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.Building);

            var art = new IronFactoryArt();
            List<Transform> tiles = IronFactoryFloor.Build(root, art);
            IronFactoryShell.Build(root, art);
            IronFactoryChannel.Build(root, art);
            IronFactoryExterior.Build(root, art);
            IronFactoryDock.Build(root, art);
            IronFactorySurroundings.Build(root, art);
            int highBays = IronFactoryLighting.Build(root, art);
            // L3 set pieces: moonlight through the roof holes, the crane over the lava, the cold blast furnace.
            IronFactoryRoof.BuildMoonlight(root, art);
            IronFactoryCrane.Build(root, art);
            IronFactoryBlastFurnace.Build(root, art);
            IronFactoryCowperStoves.Build(root, art);
            IronFactoryPiping.Build(root, art);
            // The mall's mountain basin, centred on the plant (its ridges stand 330 m+ out, clear of the yard).
            MeridianMountainHorizon.Build(root);
            root.Find(MeridianMountainHorizon.GroupName).localPosition = new Vector3(L.Width * L.Tile * .5f, -.49f, L.Depth * L.Tile * .5f);
            AmbienceBuilder.Factory(root);
            IronFactorySoundscape.Build(root);

            Dictionary<Vector2Int, IronFactoryCatwalkPlan.Kind>[] cells = IronFactoryCatwalkPlan.Cells();
            foreach (string problem in IronFactoryCatwalkPlan.Problems(cells)) Debug.LogWarning("[IronFactory] Catwalk plan: " + problem);
            List<IronFactoryCatwalks.Piece> walks = IronFactoryCatwalks.Build(root, art, cells);
            List<Transform> stairs = IronFactoryAccess.BuildStairs(root, art);
            IronFactoryAccess.BuildLadders(root, art);
            var clearance = new IronFactoryClearance();
            IronFactoryPulpit.Build(root, art);
            IronFactoryMoldBay.Build(root, art);
            var builtMachines = IronFactoryMachines.Build(root, art, clearance);
            int machines = builtMachines.Count;
            int hiding = IronFactoryHiding.Build(root, art, clearance);
            int breakers = IronFactoryPower.BuildBreakers(root, art);
            int hazards = IronFactoryPower.AddHazards(builtMachines, art);
            List<LootPointSpec> loot = IronFactoryLootPlan.Points(builtMachines, cells, out int lootRejected);
            List<Vector3> threats = IronFactoryThreatPlan.Points(cells, out List<string> threatProblems);
            foreach (string problem in threatProblems) Debug.LogWarning("[IronFactory] Left out threat spawn: " + problem);
            // L4: clutter, kept off every route, door, danger zone, hiding place and loot or threat point.
            IronFactoryClutter.Counts clutter = IronFactoryClutter.Build(root, art, clearance, builtMachines, cells, loot, threats);
            IronFactoryLore.Build(root);

            // L1: real surfaces. Decals first, then every tiled box gets world-scale UVs.
            int decals = IronFactoryDecals.Build(root, art, cells);
            int uvBoxes = IronFactorySurfaces.Apply(root, art.Tiling);

            var setup = new StructureSceneSetup();
            if (!setup.IsValid) throw new InvalidOperationException("Missing structural content for the Iron Factory build.");
            IronFactoryFloor.AddSections(setup, tiles);
            IronFactoryCatwalks.AddSections(setup, walks);
            IronFactoryAccess.AddSections(setup, stairs);
            StructureSimulation simulation = setup.AddSimulation(stability: .66f, seed: 4409);

            LocationPlumbing.Populate(root, L.CrewSpawns, loot, threats, simulation);
            // Crews must come in through the dock before a monster can reach them; the roof, truck and the dock's
            // canopy and boxcars are not floors, and nothing walks on the light rigs.
            LocationPlumbing.BakeNavMesh(root, L.SceneName,
                new[] { "Exterior", "Truck", "Shell/Roof", "MeridianCollectionBay", "RailDock", "Lights", "Ladders",
                    IronFactorySurroundings.GroupName, MeridianMountainHorizon.GroupName, "Trusses", "YardLights", "CowperStoves",
                    "ElectricalSubstation", "SecurityGatehouse", "YardClutter" });
            LocationPlumbing.ConfigureHorrorScenes();

            LevelAtmosphere.Apply(L.SceneName, Mood, root);
            PlaceCamera();
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, FactoryBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the Iron Factory.");
            // Occlusion is baked on the saved scene (its data lands beside it). Only what never breaks occludes:
            // the shell, rooms and machines; floors and catwalks carry no static flags (OcclusionBake).
            if (!OcclusionBake.Bake()) throw new InvalidOperationException("Unity could not bake the Iron Factory's occlusion.");
            if (!EditorSceneManager.SaveScene(scene, FactoryBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the Iron Factory after the occlusion bake.");
            Debug.Log($"[IronFactory] Built {L.Width * L.Tile:0} x {L.Depth * L.Tile:0} m: {tiles.Count} floor sections, " +
                      $"{walks.Count} catwalk sections ({walks.Count(w => w.Kind == IronFactoryCatwalkPlan.Kind.Spine)} spine), " +
                      $"{stairs.Count} stairs, {IronFactoryCatwalkPlan.Ladders.Length} ladders, {L.Rooms.Length} rooms, " +
                      $"{machines}/{IronFactoryMachinePlan.Machines.Length} machines ({hazards} come alive on a breaker), " +
                      $"{breakers} breakers, {hiding}/{IronFactoryMachinePlan.Spots.Length} hiding places, " +
                      $"{loot.Count} loot points ({loot.Count(p => p.IsJackpot)} jackpot, {lootRejected} left out), " +
                      $"{threats.Count} threat spawns ({threats.Count(t => t.y > 1f)} on catwalks), {highBays} high-bay lamps, " +
                      $"{decals} decals, {uvBoxes} boxes with world-scale UVs, clutter: {clutter.Solid} solid props, " +
                      $"{clutter.Litter} litter, {clutter.Chains} chains, {clutter.Cables} cables.");
            for (int level = 1; level < cells.Length; level++)
                Debug.Log($"[IronFactory] Tier {level} ({IronFactoryCatwalkPlan.Height(level):0} m), north up:\n{IronFactoryCatwalkPlan.Map(cells, level)}");
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(L.DockX, 28f, -30f);
            camera.transform.LookAt(new Vector3(L.DockX, 0f, 50f));
        }
    }
}

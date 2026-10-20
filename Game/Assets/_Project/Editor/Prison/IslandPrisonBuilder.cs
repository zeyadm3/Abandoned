using System;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Gallows Rock Penitentiary (Docs/ISLAND_PRISON_PLAN.md) into Scenes/Prison.unity from scratch, the way the
    /// mall and the Iron Factory are built: layout data, then the rock, the buildings and their fittings, then the
    /// shared run plumbing, then the NavMesh and the occlusion bake. It replaces the small 0.24 prison; contracts that
    /// roll "Prison" get the island. Re-running it rebuilds the scene.
    /// </summary>
    public static class IslandPrisonBuilder
    {
        /// <summary>Noon on the bay (the user's choice): a high sun, a pale sky, a light sea haze you see the far shore
        /// through. The cellhouse keeps its own dim interior haze, so inside is still the derelict dark.</summary>
        public static readonly Color NoonHaze = new(.66f, .71f, .76f);
        private static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(.5f, .57f, .66f), equator: new Color(.42f, .44f, .45f), ground: new Color(.22f, .21f, .19f),
            fog: NoonHaze, fogDensity: .0018f, bloom: .16f, vignette: .12f, grain: .035f,
            saturation: -9f, contrast: 10f, sunColor: new Color(1f, .96f, .88f), sunIntensity: 1.7f, sunEuler: new Vector3(68f, 150f, 0f));

        [MenuItem(ToolMenu.Prison + "Rebuild Island Prison", priority = ToolMenu.PrisonP)]
        public static void Rebuild() => Build();

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Rebuild the island prison in Edit mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // First, while no scene of ours is open: its icon pass renders in a scene of its own.
            IslandLoot.EnsureContent();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Transform root = new GameObject(L.SceneName).transform;
            // Inside the cellhouse: a still, damp haze; outside is the bay's fog (the Mood).
            root.gameObject.AddComponent<BuildingInteriorAtmosphere>().EditorSetup(L.CellhouseBounds, new Color(.15f, .18f, .19f), .009f);
            root.gameObject.AddComponent<LevelBuildingBounds>().EditorSetup(L.Building);

            IslandMeshKit.Reset();
            IslandHideSpots.Reset();
            var art = new IslandPrisonArt();
            var kits = new IslandCellKits(art);
            var loot = new List<LootPointSpec>();
            var keepClear = new List<Rect>();
            var otherFlights = new List<IslandFlights.Built>();

            IslandTerrain.Build(root, art);
            IslandLandform.Build(root, art);
            List<IslandFlights.Built> cliffStair = IslandRoad.Build(root, art);
            List<IslandCellhouse.Tile> tiles = IslandCellhouse.Build(root, art);
            IslandCellStacks.Result stacks = IslandCellStacks.Build(root, art, kits, loot);
            (int solitary, int sideOpen) = IslandSideBlocks.Build(root, art, kits, loot);
            IslandInteriors.Build(root, art, loot, keepClear);
            otherFlights.Add(IslandBasement.Build(root, art, loot));
            List<IslandCellhouse.Tile> barracksFloors = IslandBarracks.Build(root, art, loot, otherFlights);
            List<IslandCellhouse.Tile> wardenFloors = IslandWardenHouse.Build(root, art, loot, otherFlights);
            IslandLighthouse.Build(root, art, loot);
            IslandGrounds.Build(root, art, loot);
            IslandExterior.Build(root, art, loot);
            IslandBoat.Build(root, art);
            (int trees, int shrubs, int rocks) = IslandVegetation.Build(root, art);
            IslandBackdrop.Build(root);
            IslandArchitecturePolish.Build(root, art);
            IslandCorridorDetails.Build(root, art);
            int lights = IslandLighting.Build(root, art);
            int decay = IslandDecay.Build(root, art, keepClear);
            AmbienceBuilder.Island(root);
            int uvBoxes = IslandPrisonSurfaces.Apply(root, art.Tiling);

            var setup = new StructureSceneSetup();
            if (!setup.IsValid) throw new InvalidOperationException("Missing structural content for the island prison build.");
            IslandCellhouse.AddSections(setup, tiles);
            foreach (IslandCellStacks.Gallery gallery in stacks.Galleries)
            {
                setup.AddSection(gallery.Transform, SectionType.Walkway, gallery.Collapsible, gallery.Health, gallery.Capacity, fractured: false);
                if (gallery.Collapsible) SectionNavCarver.EditorAdd(gallery.Transform.gameObject, new Vector3(gallery.Size.x, L.Slab, gallery.Size.y));
                else gallery.Transform.gameObject.AddComponent<SectionKeepClear>();
            }
            // The cliff stair is old, rusted steel: weaker than anything else on the island.
            IslandFlights.AddSections(setup, cliffStair, health: .6f, capacity: .6f);
            IslandFlights.AddSections(setup, stacks.Flights);
            IslandFlights.AddSections(setup, otherFlights);
            Floors(setup, barracksFloors, .85f, .8f);
            // Burned timber: the warden's upper floor gives way first.
            Floors(setup, wardenFloors, .5f, .55f);
            StructureSimulation simulation = setup.AddSimulation(stability: .68f, seed: 5209);
            IslandInteriorSupport.Bind(root);

            // Finishing: decals off every surface, intentional walk-throughs marked, then everything else with volume
            // made solid (sections already own their collision, so the pass leaves them alone).
            int decals = IslandSurfaceFixes.LiftDecals(root);
            IslandSurfaceFixes.MarkIntentional(root);
            int madeSolid = IslandSurfaceFixes.MakeSolid(root);
            IslandPrisonRendering.Apply(root, art);

            List<Vector3> threats = IslandThreatPlan.Points(out List<string> threatProblems);
            foreach (string problem in threatProblems) Debug.LogWarning("[IslandPrison] Left out threat spawn: " + problem);
            IslandSurfaceFixes.SnapToGround(loot, threats);
            LocationPlumbing.Populate(root, L.CrewSpawns, loot, threats, simulation);
            // Monsters walk the island, not the sea, the boat, the roofs or the lamp room's glass.
            LocationPlumbing.BakeNavMesh(root, L.SceneName, new[]
            {
                IslandBoat.GroupName, "Sea", "Bay", "CellhouseRoof", IslandLighting.GroupName, "Decay", "Lighthouse/LampRoom/Lantern", IslandBackdrop.GroupName,
                IslandArchitecturePolish.GroupName,
                IslandCorridorDetails.GroupName,
                "GuardTower/TowerLadder", "Grounds/WaterTower",
                "Cellhouse/Inside/DiningCeiling", "Cellhouse/Inside/Admin/Admin_Ceiling", "Cellhouse/Inside/Library/Library_Ceiling",
                "SideBlocks/D_Block/Roof", "SideBlocks/A_Block/Roof", "SideBlocks/D_Block/Slab_T1", "SideBlocks/A_Block/Slab_T1",
                "CellBlocks/B_Block/Roof", "CellBlocks/C_Block/Roof",
                "Barracks/Shell/Roof", "WardenHouse/Shell/Roof_E", "Exterior/Gatehouse/Booth_Roof", "Lighthouse/Store/Roof",
            });
            LocationPlumbing.ConfigureHorrorScenes();

            LevelAtmosphere.Apply(L.SceneName, Mood, root);
            // A clear noon sky with the sun in it (Unity's built-in procedural sky; no package).
            RenderSettings.skybox = NoonSky();
            IslandPresentation.Apply(root);
            PlaceCamera();
            NetworkObjectIds.StampScene(scene);
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, PrisonBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the island prison.");
            // Occlusion is baked on the saved scene. Only what never breaks occludes: walls, stacks, rock; never a floor.
            if (!OcclusionBake.Bake()) throw new InvalidOperationException("Unity could not bake the island prison's occlusion.");
            if (!EditorSceneManager.SaveScene(scene, PrisonBuilder.ScenePath))
                throw new InvalidOperationException("Unity could not save the island prison after the occlusion bake.");

            int sections = tiles.Count + stacks.Galleries.Count + cliffStair.Count + stacks.Flights.Count + otherFlights.Count + barracksFloors.Count + wardenFloors.Count;
            Debug.Log($"[IslandPrison] Built {L.DisplayName}: {sections} structural sections ({tiles.Count} cellhouse tiles, " +
                      $"{stacks.Galleries.Count} galleries of which {stacks.Galleries.Count(g => g.Collapsible)} can fall, {cliffStair.Count} cliff-stair pieces), " +
                      $"{stacks.Cells} stack cells ({stacks.OpenCells} open), {L.SideBlockCells * 4} side-block cells ({sideOpen} open), " +
                      $"{IslandHideSpots.Count} hiding places ({IslandHideSpots.Cells} cells, {IslandHideSpots.Solitary} solitary, {IslandHideSpots.Carts} carts, " +
                      $"{IslandHideSpots.Lockers} lockers), {loot.Count} loot points ({loot.Count(p => p.IsJackpot)} jackpot), {threats.Count} threat spawns, " +
                      $"{lights} lamps, {decay} decay pieces ({decals} decals lifted), {madeSolid} objects made solid, {trees} trees, {shrubs} plants, {rocks} boulders, {uvBoxes} boxes with world-scale UVs.");
            _ = solitary;
        }

        private static Material NoonSky()
        {
            const string path = IslandPrisonArt.Folder + "/Sky/IslandNoonSky.mat";
            PolishAssets.EnsureFolder(IslandPrisonArt.Folder + "/Sky");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Skybox/Procedural")) { name = "IslandNoonSky" };
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = Shader.Find("Skybox/Procedural");
            m.SetFloat("_SunDisk", 2f);
            m.SetFloat("_SunSize", .035f);
            m.SetFloat("_SunSizeConvergence", 6f);
            m.SetFloat("_AtmosphereThickness", .85f);
            m.SetColor("_SkyTint", new Color(.48f, .55f, .62f));
            m.SetColor("_GroundColor", new Color(.36f, .4f, .43f));
            m.SetFloat("_Exposure", 1.15f);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void Floors(StructureSceneSetup setup, IEnumerable<IslandCellhouse.Tile> floors, float health, float capacity)
        {
            foreach (IslandCellhouse.Tile t in floors)
            {
                setup.AddSection(t.Transform, SectionType.Floor, t.Collapsible, health, capacity, fractured: false);
                if (t.Collapsible) SectionNavCarver.EditorAdd(t.Transform.gameObject, new Vector3(L.Tile, L.Slab, L.Tile));
            }
        }

        private static void PlaceCamera()
        {
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.transform.position = new Vector3(L.BoatStern.x - 20f, 26f, -40f);
            camera.transform.LookAt(new Vector3(L.Cellhouse.center.x, L.PlateauY, L.Cellhouse.center.y));
        }
    }
}

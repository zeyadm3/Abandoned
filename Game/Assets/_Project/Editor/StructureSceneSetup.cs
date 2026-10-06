using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Turns built pieces into StructuralSections and adds the level's StructureSimulation (+ its network
    /// sync spawner). Shared by every level builder so sections are set up the same way everywhere.
    /// </summary>
    public sealed class StructureSceneSetup
    {
        private readonly StructureConfig config;
        private readonly StructureVisualConfig visuals;
        private readonly GameObject fracturedTile;

        public bool IsValid => config != null && visuals != null && fracturedTile != null;

        public StructureSceneSetup()
        {
            config = AssetDatabase.LoadAssetAtPath<StructureConfig>(StructureContentBuilder.ConfigPath);
            visuals = AssetDatabase.LoadAssetAtPath<StructureVisualConfig>(StructureContentBuilder.VisualConfigPath);
            fracturedTile = AssetDatabase.LoadAssetAtPath<GameObject>(FracturedTileGenerator.PrefabPath);
            if (!IsValid) Debug.LogError("Structure content missing; run Tools/Abandoned/Create Structure Content.");
        }

        /// <param name="fractured">Tiles break into the pre-fractured tile; stairs pass false (no fracture prefab yet).</param>
        public StructuralSection AddSection(Transform piece, SectionType type, bool collapsible, float health = 1f,
            float capacity = 1f, bool fractured = true)
        {
            Transform visual = piece.Find("Visual");
            if (visual == null) Debug.LogError($"Section '{piece.name}' has no Visual child.");
            var section = piece.gameObject.AddComponent<StructuralSection>();
            section.EditorSetup(config, type, collapsible, health, capacity, visual);
            // Collisions are only reported to the collider's own object; relay child colliders to the section.
            foreach (Collider c in piece.GetComponentsInChildren<Collider>(true))
                if (c.gameObject != piece.gameObject)
                    c.gameObject.AddComponent<SectionColliderRelay>().EditorSetup(section);
            piece.gameObject.AddComponent<SectionPresentation>().EditorSetup(visuals, fractured ? fracturedTile : null);
            SerializedWiring.SetLayerRecursively(piece.gameObject, GameLayers.StructureLayer);
            return section;
        }

        public StructureSimulation AddSimulation(float stability, int seed)
        {
            var simulationObject = new GameObject("Structure");
            var simulation = simulationObject.AddComponent<StructureSimulation>();
            SerializedWiring.Set(simulation, "config", config);
            SerializedWiring.SetFloat(simulation, "stability", stability);
            SerializedWiring.SetInt(simulation, "seed", seed);
            SerializedWiring.Set(simulationObject.AddComponent<StructureDebugView>(), "simulation", simulation);
            SerializedWiring.Set(simulationObject.AddComponent<StructureDebugControls>(), "simulation", simulation);
            var syncPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkContentBuilder.StructureNetPrefabPath);
            if (syncPrefab == null) Debug.LogError("StructureNet prefab missing; run Tools/Abandoned/Create Structure Net Prefab.");
            else simulationObject.AddComponent<StructureNetSpawner>().EditorSetup(syncPrefab.GetComponent<Unity.Netcode.NetworkObject>());
            return simulation;
        }
    }
}

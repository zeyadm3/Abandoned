using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Abandoned.Tests
{
    /// <summary>
    /// Builds sections and a StructureSimulation in code. The simulation's FixedUpdate is switched
    /// off so tests step it by hand with exact time steps.
    /// </summary>
    public class StructureTestRig
    {
        public const float Dt = 0.02f;

        private readonly List<GameObject> spawned = new();

        public StructureConfig Config { get; private set; }
        public StructureVisualConfig Visuals { get; private set; }
        public GameObject FracturedTile { get; private set; }
        public StructureSimulation Simulation { get; private set; }

        public static StructureTestRig Create(bool ground = true)
        {
            var rig = new StructureTestRig();
#if UNITY_EDITOR
            rig.Config = AssetDatabase.LoadAssetAtPath<StructureConfig>("Assets/_Project/Data/Structure/StructureConfig.asset");
            rig.Visuals = AssetDatabase.LoadAssetAtPath<StructureVisualConfig>("Assets/_Project/Data/Structure/StructureVisualConfig.asset");
            rig.FracturedTile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Structure/Fractured_Tile_4x4.prefab");
#endif
            if (ground)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Ground";
                floor.transform.position = new Vector3(0f, -0.5f, 0f);
                floor.transform.localScale = new Vector3(100f, 1f, 100f);
                rig.spawned.Add(floor);
            }
            var sun = new GameObject("TestSun");
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.AddComponent<Light>().type = LightType.Directional;
            rig.spawned.Add(sun);
            return rig;
        }

        /// <summary>A 4 x 0.3 x 4 m section with its walking surface at topCentre, built like TestMap's tiles.</summary>
        public StructuralSection AddTile(string name, Vector3 topCentre, SectionType type = SectionType.Floor,
            bool collapsible = true, float health = 1f, float capacity = 1f, bool presentation = true,
            bool colliderOnChild = false, float tiltDegrees = 0f)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetPositionAndRotation(topCentre, Quaternion.Euler(tiltDegrees, 0f, 0f));
            GameObject colliderHost = go;
            if (colliderOnChild)
            {
                colliderHost = new GameObject("Ramp");
                colliderHost.transform.SetParent(go.transform, false);
                colliderHost.layer = GameLayers.StructureLayer;
            }
            var box = colliderHost.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, -0.15f, 0f);
            box.size = new Vector3(4f, 0.3f, 4f);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.name = "Visual";
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            visual.transform.localScale = new Vector3(4f, 0.3f, 4f);

            int layer = GameLayers.StructureLayer;
            go.layer = layer;
            visual.layer = layer;

            var section = go.AddComponent<StructuralSection>();
#if UNITY_EDITOR
            section.EditorSetup(Config, type, collapsible, health, capacity, visual.transform);
            if (presentation) go.AddComponent<SectionPresentation>().EditorSetup(Visuals, FracturedTile);
            if (colliderOnChild) colliderHost.AddComponent<SectionColliderRelay>().EditorSetup(section);
#endif
            go.SetActive(true);
            spawned.Add(go);
            Physics.SyncTransforms();
            return section;
        }

        /// <summary>Creates the simulation over every section added so far, applied and ready to step.</summary>
        public StructureSimulation StartSimulation(float stability = 1f, int seed = 1)
        {
            var go = new GameObject("Structure");
            go.SetActive(false);
            Simulation = go.AddComponent<StructureSimulation>();
#if UNITY_EDITOR
            var so = new SerializedObject(Simulation);
            so.FindProperty("config").objectReferenceValue = Config;
            so.FindProperty("stability").floatValue = stability;
            so.FindProperty("seed").intValue = seed;
            so.ApplyModifiedPropertiesWithoutUndo();
#endif
            go.SetActive(true);       // Awake collects and numbers the sections
            Simulation.enabled = false; // manual stepping only (Start won't run while disabled)
            Simulation.ApplyStability(stability, seed);
            spawned.Add(go);
            return Simulation;
        }

        public void Step(int steps = 1)
        {
            for (int i = 0; i < steps; i++) Simulation.Step(Dt);
        }

        /// <summary>Steps until the condition holds; returns the number of steps taken, or -1.</summary>
        public int StepUntil(System.Func<bool> condition, float maxSeconds)
        {
            int max = Mathf.CeilToInt(maxSeconds / Dt);
            for (int i = 0; i <= max; i++)
            {
                if (condition()) return i;
                Simulation.Step(Dt);
            }
            return -1;
        }

        public T Track<T>(T component) where T : Component
        {
            spawned.Add(component.gameObject);
            return component;
        }

        public void Destroy()
        {
            foreach (GameObject go in spawned) if (go != null) Object.DestroyImmediate(go);
            spawned.Clear();
            foreach (DebrisLifetime debris in Object.FindObjectsByType<DebrisLifetime>(FindObjectsSortMode.None))
                Object.DestroyImmediate(debris.gameObject);
        }
    }
}

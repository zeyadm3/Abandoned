using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Abandoned.Core;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Checks content the compiler can't: prefabs and scenes have no missing scripts and no unwired
    /// references on our components, scenes contain what they need, and every config/definition
    /// asset passes its own <see cref="IValidatable"/> rules.
    /// </summary>
    public static class ContentValidator
    {
        private const string PrefabRoot = "Assets/_Project/Prefabs";
        private const string SceneRoot = "Assets/_Project/Scenes";
        private const string DataRoot = "Assets/_Project/Data";

        /// <summary>Components every Player prefab must carry (missing ones silently break other systems).</summary>
        private static readonly Type[] PlayerRequirements =
        {
            typeof(PlayerMotor), typeof(PlayerLook), typeof(PlayerInputReader), typeof(PlayerStamina), typeof(PlayerRagdoll),
            typeof(PlayerCameraFeel), typeof(PlayerFootsteps), typeof(Abandoned.Interaction.PlayerCarrier),
            typeof(Abandoned.Interaction.PlayerInteractor), typeof(Abandoned.Interaction.CarrierLoad),
        };

        /// <summary>Component types each scene must contain at least once.</summary>
        private static readonly Dictionary<string, Type[]> SceneRequirements = new()
        {
            [TestBuildingBuilder.ScenePath] = new[]
            {
                typeof(PlayerSpawnPoint), typeof(Abandoned.Networking.NetworkBootstrap), typeof(DebugViewToggle), typeof(CinemachineBrain),
                typeof(Abandoned.Loot.LootItem), typeof(Abandoned.Structure.StructureSimulation),
                typeof(Abandoned.Structure.StructuralSection), typeof(NoiseDebugView)
            }
        };

        [MenuItem("Tools/Abandoned/Validate Content")]
        public static void RunFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Report(Run());
        }

        public static List<string> Run()
        {
            var errors = new List<string>();
            ValidatePrefabs(errors);
            ValidateScenes(errors);
            ValidateAssets(errors);
            return errors;
        }

        public static void Report(List<string> errors)
        {
            if (errors.Count == 0) Debug.Log("[ALL PASS] Content validation");
            else Debug.LogError($"[{errors.Count} FAILED] Content validation\n" + string.Join("\n", errors));
        }

        private static void ValidatePrefabs(List<string> errors)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ValidateHierarchy(root, path, errors);
                    if (path == PlayerPrefabBuilder.PrefabPath)
                    {
                        foreach (Type type in PlayerRequirements)
                            if (root.GetComponent(type) == null) errors.Add($"{path}: missing {type.Name}.");
                        NetworkValidation.ValidatePlayerPrefab(root, path, errors);
                    }
                    if (path.StartsWith(LootPrefabGenerator.Folder + "/") && root.GetComponent<Abandoned.Loot.LootItem>() != null)
                        NetworkValidation.ValidateLootPrefab(root, path, errors);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        private static void ValidateScenes(List<string> errors)
        {
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { SceneRoot }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects())
                        ValidateHierarchy(root, path, errors);
                    if (Object.FindFirstObjectByType<Abandoned.Networking.NetworkBootstrap>(FindObjectsInactive.Include) != null)
                        NetworkValidation.ValidateSessionScene(path, errors);

                    if (!SceneRequirements.TryGetValue(path, out Type[] required)) continue;
                    foreach (Type type in required)
                        if (Object.FindFirstObjectByType(type, FindObjectsInactive.Include) == null)
                            errors.Add($"{path}: needs a {type.Name}.");
                }
            }
            finally
            {
                if (previous.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        private static void ValidateAssets(List<string> errors)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition", new[] { LootCatalogBuilder.Folder }))
            {
                var definition = AssetDatabase.LoadAssetAtPath<Abandoned.Loot.LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LootPrefabGenerator.PrefabPathFor(definition));
                if (prefab != null) LootPrefabGenerator.ValidatePrefab(definition, prefab, errors);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { DataRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null) errors.Add($"{path}: asset failed to load (missing script?).");
                else if (asset is IValidatable validatable) validatable.Validate(errors);
            }
        }

        private static void ValidateHierarchy(GameObject root, string context, List<string> errors)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject go = t.gameObject;
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                    errors.Add($"{context}: '{PathOf(t)}' has a missing script.");

                foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
                    if (behaviour != null && IsOurs(behaviour.GetType()))
                        ValidateReferences(behaviour, $"{context}: '{PathOf(t)}' {behaviour.GetType().Name}", errors);
            }
        }

        private static void ValidateReferences(Object component, string context, List<string> errors)
        {
            SerializedProperty property = new SerializedObject(component).GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = property.propertyType == SerializedPropertyType.Generic;
                if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (property.name == "m_Script" || property.objectReferenceValue != null) continue;
                if (IsOptional(component.GetType(), property.name)) continue;

                bool dangling = property.objectReferenceInstanceIDValue != 0;
                errors.Add($"{context}.{property.propertyPath} is {(dangling ? "a missing reference" : "not assigned")}.");
            }
        }

        private static bool IsOptional(Type type, string fieldName)
        {
            for (Type t = type; t != null && t != typeof(MonoBehaviour); t = t.BaseType)
            {
                FieldInfo field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) return field.IsDefined(typeof(OptionalReferenceAttribute), true);
            }
            return false;
        }

        private static bool IsOurs(Type type) => type.Namespace != null && type.Namespace.StartsWith("Abandoned");

        private static string PathOf(Transform t) =>
            string.Join("/", t.GetComponentsInParent<Transform>(true).Reverse().Select(x => x.name));
    }
}

using System;
using System.Linq;
using Abandoned.Threats;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Installs original skinned FBX models and five authored animation takes without changing threat AI tuning.</summary>
    public static class ThreatRigBuilder
    {
        public const string ArtFolder = "Assets/_Project/Art/Custom/Threats";
        private static readonly string[] Names = { "Ash", "Charcoal", "Canvas", "Bone", "Iron", "Eyes", "Rust" };
        public static void Install(GameObject root, string modelName)
        {
            CharacterMaterialBuilder.Prepare();
            Transform old = root.transform.Find("Visual");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach (ThreatPresentation presentation in root.GetComponents<ThreatPresentation>()) UnityEngine.Object.DestroyImmediate(presentation);
            foreach (ThreatAnimationSync animation in root.GetComponents<ThreatAnimationSync>()) UnityEngine.Object.DestroyImmediate(animation);
            foreach (ThreatHorrorAudio audio in root.GetComponents<ThreatHorrorAudio>()) UnityEngine.Object.DestroyImmediate(audio);
            string modelPath = $"{ArtFolder}/{modelName}.fbx";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (source == null) throw new InvalidOperationException($"Missing custom threat model: {modelPath}. Run Tools/Blender/threats.py first.");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            GameObject imported = (GameObject)PrefabUtility.InstantiatePrefab(source);
            imported.transform.SetParent(visual.transform, false);
            Animator animator = imported.GetComponent<Animator>() ?? imported.AddComponent<Animator>();
            animator.runtimeAnimatorController = Controller(modelName, modelPath);
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
            animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            root.AddComponent<ThreatAnimationSync>().EditorSetup(animator);
            root.AddComponent<ThreatHorrorAudio>();
            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = Material(mats[i] != null ? mats[i].name : "THREAT_Ash");
                renderer.sharedMaterials = mats;
            }
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = root.layer;
        }
        public static GameObject Upgrade(string path, string modelName, ThreatDefinition definition)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Install(root, modelName);
                root.GetComponent<Threat>().EditorSetupDefinition(definition);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static AnimatorController Controller(string modelName, string modelPath)
        {
            PolishAssets.EnsureFolder(ArtFolder + "/Animations");
            string path = $"{ArtFolder}/Animations/{modelName}.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path) ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in machine.states) machine.RemoveState(child.state);
            AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
            foreach (string name in Enum.GetNames(typeof(ThreatMotion)))
            {
                AnimationClip clip = clips.FirstOrDefault(c => c.name == name) ?? clips.FirstOrDefault(c => c.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
                if (clip == null) throw new InvalidOperationException($"{modelName} is missing its {name} animation take.");
                AnimatorState state = machine.AddState(name); state.motion = clip;
                if (name == "Idle") machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }
        private static int MaterialIndex(string importedName)
        {
            int index = Array.FindIndex(Names, name => importedName.Contains("THREAT_" + name) || importedName.Contains("CustomThreat_" + name));
            return index >= 0 ? index : 0;
        }
        public static string MaterialPath(string importedName) => $"{PolishAssets.Folder}/CustomThreat_{Names[MaterialIndex(importedName)]}.mat";
        public static void ConfigureMaterial(Material material, string importedName)
        {
            int index = MaterialIndex(importedName);
            CharacterMaterialBuilder.ConfigureThreat(material, Names[index]);
        }
        private static Material Material(string importedName)
        {
            CharacterMaterialBuilder.Prepare();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(importedName));
            ConfigureMaterial(material, importedName);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

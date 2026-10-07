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
        private static readonly Color[] Colors = {
            new(0.43f,0.47f,0.43f),new(0.105f,0.135f,0.14f),new(0.31f,0.29f,0.23f),new(0.56f,0.58f,0.49f),
            new(0.21f,0.24f,0.25f),new(0.39f,0.50f,0.41f),new(0.30f,0.18f,0.13f)
        };
        public static void Install(GameObject root, string modelName)
        {
            Transform old = root.transform.Find("Visual");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach (ThreatPresentation presentation in root.GetComponents<ThreatPresentation>()) UnityEngine.Object.DestroyImmediate(presentation);
            foreach (ThreatAnimationSync animation in root.GetComponents<ThreatAnimationSync>()) UnityEngine.Object.DestroyImmediate(animation);
            foreach (ThreatHorrorAudio audio in root.GetComponents<ThreatHorrorAudio>()) UnityEngine.Object.DestroyImmediate(audio);
            string modelPath = $"{ArtFolder}/{modelName}.fbx";
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (source == null) throw new InvalidOperationException($"Missing custom threat model: {modelPath}. Run Tools/Blender/threats.py first.");
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
            visual.name = "Visual"; visual.transform.SetParent(root.transform, false);
            Animator animator = visual.GetComponent<Animator>() ?? visual.AddComponent<Animator>();
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
        private static Material Material(string importedName)
        {
            int index = Array.FindIndex(Names, n => importedName.Contains("THREAT_" + n));
            if (index < 0) index = 0;
            Material material = PolishAssets.Material("CustomThreat_" + Names[index], Colors[index], emission: index == 5 ? 0.18f : 0f);
            Texture2D grime = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtFolder + "/ThreatGrime.png");
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", grime);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", index == 4 ? 0.45f : 0f);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

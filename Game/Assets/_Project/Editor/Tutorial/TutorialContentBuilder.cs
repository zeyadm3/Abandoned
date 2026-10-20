using System.Collections.Generic;
using Abandoned.Tutorial;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Writes the training job's data: the config (Data/Tutorial/Resources/TutorialConfig), one step asset per lesson
    /// (Data/Tutorial/Steps) and the ordered catalog (Data/Tutorial/Resources/TutorialCatalog). A lesson that already
    /// has an asset is left as it is (it may have been edited); a missing one is created and put in its place in the
    /// catalog. "Reset Tutorial Lessons" writes every default again.
    /// </summary>
    public static class TutorialContentBuilder
    {
        public const string Folder = "Assets/_Project/Data/Tutorial";
        public const string StepsFolder = Folder + "/Steps";
        public const string ConfigPath = Folder + "/Resources/TutorialConfig.asset";
        public const string CatalogPath = Folder + "/Resources/TutorialCatalog.asset";

        [MenuItem(ToolMenu.Tutorial + "Build Tutorial Lessons", priority = ToolMenu.TutorialP + 20)]
        public static void Build() => Write(overwrite: false);

        [MenuItem(ToolMenu.Tutorial + "Reset Tutorial Lessons to Defaults", priority = ToolMenu.TutorialP + 40)]
        public static void Reset()
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Reset tutorial lessons",
                    "Write every default lesson again, replacing any edits to their assets and the catalog's order?", "Reset", "Cancel")) return;
            Write(overwrite: true);
        }

        private static void Write(bool overwrite)
        {
            PolishAssets.EnsureFolder(StepsFolder);
            PolishAssets.EnsureFolder(Folder + "/Resources");
            SerializedWiring.LoadOrCreateAsset<TutorialConfig>(ConfigPath);
            TutorialCatalog catalog = SerializedWiring.LoadOrCreateAsset<TutorialCatalog>(CatalogPath);

            var ordered = new List<TutorialStepDefinition>();
            int created = 0, filled = 0;
            foreach (TutorialLessons.Lesson lesson in TutorialLessons.All)
            {
                string path = $"{StepsFolder}/Step_{lesson.Id.Replace('.', '_')}.asset";
                var step = AssetDatabase.LoadAssetAtPath<TutorialStepDefinition>(path);
                bool fresh = step == null;
                if (fresh)
                {
                    step = ScriptableObject.CreateInstance<TutorialStepDefinition>();
                    AssetDatabase.CreateAsset(step, path);
                    created++;
                }
                if (fresh || overwrite)
                {
                    string chapter = lesson.Chapter < TutorialLessons.Chapters.Length ? TutorialLessons.Chapters[lesson.Chapter] : "TRAINING";
                    step.EditorSetup(lesson.Id, lesson.Chapter, chapter, lesson.Title, lesson.Text, lesson.Crew, lesson.Radio,
                        lesson.Check, lesson.Key, lesson.Area, lesson.Amount, lesson.Beat, lesson.BeatKey, lesson.BeatAmount, skippable: true);
                    step.EditorSetGuidance(lesson.Guide, lesson.Hud, lesson.Voice);
                    EditorUtility.SetDirty(step);
                }
                else if (string.IsNullOrEmpty(step.Guide) && string.IsNullOrEmpty(step.Highlight) && string.IsNullOrEmpty(step.VoiceLine))
                {
                    // Written before lessons had guidance: give it the defaults' arrows, frames and voice.
                    step.EditorSetGuidance(lesson.Guide, lesson.Hud, lesson.Voice);
                    EditorUtility.SetDirty(step);
                    filled++;
                }
                ordered.Add(step);
            }
            GuidePathMaterial();

            // The catalog keeps its own order and its own extra lessons; defaults it lacks go in after their predecessor.
            var list = new List<TutorialStepDefinition>();
            if (!overwrite) foreach (TutorialStepDefinition s in catalog.Steps) if (s != null) list.Add(s);
            for (int i = 0; i < ordered.Count; i++)
            {
                if (list.Contains(ordered[i])) continue;
                int after = i == 0 ? -1 : list.IndexOf(ordered[i - 1]);
                list.Insert(after + 1, ordered[i]);
            }
            catalog.EditorSet(list);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            var errors = new List<string>();
            catalog.Validate(errors);
            foreach (string e in errors) Debug.LogError("[Tutorial] " + e);
            Debug.Log($"[Tutorial] Content: {list.Count} lessons in the catalog ({created} new, {filled} given guidance{(overwrite ? ", every default rewritten" : "")}).");
        }

        public const string GuidePathMaterialPath = "Assets/_Project/Art/Tutorial/Resources/" + TutorialFloorPath.MaterialResource + ".mat";

        // The floor arrows' material: unlit, see-through, tinted per vertex (URP's particle shader multiplies
        // vertex colour). In Resources so the shader and its transparent variant go into every build.
        private static void GuidePathMaterial()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Art/Tutorial/Resources");
            var material = AssetDatabase.LoadAssetAtPath<Material>(GuidePathMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (material == null)
            {
                material = new Material(shader) { name = TutorialFloorPath.MaterialResource };
                AssetDatabase.CreateAsset(material, GuidePathMaterialPath);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
        }
    }
}

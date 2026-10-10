using System;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Abandoned.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>Retains only small mounted wayfinding text; printed retail artwork carries the mall's identity.</summary>
    public static class MallTextCleanup0158
    {
        public static void Build(Transform mall)
        {
            if (mall == null) throw new ArgumentNullException(nameof(mall));
            int libraryLabels = CleanSavedSalvageModels();
            int removed = 0, fitted = 0;
            var routes = new List<TextMesh>();
            foreach (TextMesh label in mall.GetComponentsInChildren<TextMesh>(true))
            {
                if (label == null) continue;
                string words = Normalise(label.text);
                if (!IsEssential(words) || Duplicate(label, words, routes))
                {
                    Remove(label);
                    removed++;
                    continue;
                }
                if (FitMountedRoute(label, words)) fitted++;
                routes.Add(label);
            }
            RemoveBlankSignArt(mall);
            Debug.Log($"[Mall0158b] Removed {removed} decorative world-text captions and {libraryLabels} library labels; fitted {fitted} small mounted exit/service/collection signs. Retail image artwork and functional UI remain.");
        }

        private static bool Duplicate(TextMesh label, string words, List<TextMesh> routes)
        {
            string kind = words.StartsWith("COLLECTION", StringComparison.Ordinal) ? "COLLECTION" : words;
            float distance = kind == "COLLECTION" ? 3f : .65f;
            foreach (TextMesh route in routes)
            {
                if (route == null) continue;
                string existing = Normalise(route.text);
                if (existing.StartsWith("COLLECTION", StringComparison.Ordinal)) existing = "COLLECTION";
                if (existing == kind && Vector3.Distance(route.transform.position, label.transform.position) < distance &&
                    Vector3.Dot(route.transform.forward, label.transform.forward) > .90f) return true;
            }
            return false;
        }

        private static int CleanSavedSalvageModels()
        {
            int removed = 0;
            // Models are first so their nested dressing and network-loot instances inherit the clean source assets.
            string[] folders = { SalvageAssetBuilder.ModelFolder, SalvageExpansionModels0156.Folder + "/Prefabs",
                SalvageAssetBuilder.DressingFolder, LootPrefabGenerator.Folder };
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (folder == LootPrefabGenerator.Folder &&
                        !System.IO.Path.GetFileName(path).StartsWith("Loot_salvage_", StringComparison.Ordinal)) continue;
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset == null || asset.GetComponentsInChildren<TextMesh>(true).Length == 0) continue;
                    GameObject contents = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        foreach (TextMesh label in contents.GetComponentsInChildren<TextMesh>(true))
                        {
                            if (label == null) continue;
                            Remove(label);
                            removed++;
                        }
                        RemoveBlankSignArt(contents.transform);
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                }
            }
            return removed;
        }

        private static bool FitMountedRoute(TextMesh label, string words)
        {
            Transform parent = label.transform.parent;
            if (parent == null || label.font == null) return false;
            Transform face = parent.Find("Fascia") ?? parent.Find("FadedFascia") ?? parent.Find("Paper");
            if (face == null || face.GetComponent<Renderer>() == null) return false;
            float maxWidth = words == "SERVICE STAIRS" ? 1.4f : words == "SERVICE" ? 1.2f : .9f;
            float maxHeight = words.StartsWith("COLLECTION", StringComparison.Ordinal) ? .18f : .20f;
            float width = Mathf.Min(Mathf.Abs(face.localScale.x), maxWidth);
            float height = Mathf.Min(Mathf.Abs(face.localScale.y), maxHeight);
            if (width <= .01f || height <= .01f) return false;
            if (face.GetComponent<Collider>() == null)
            {
                face.localScale = new Vector3(width, height, Mathf.Min(Mathf.Abs(face.localScale.z), .045f));
                Record(face);
            }

            // Font size controls glyph rasterisation, not a desired sign's world dimensions. Fit the actual glyph mesh.
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = 1f;
            label.transform.localScale = Vector3.one;
            label.font.RequestCharactersInTexture(words, label.fontSize, label.fontStyle);
            WorldTextMaterial.Apply(label, label.font);
            label.text = string.Empty;
            label.text = words;
            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer == null) return false;
            Bounds glyphs = renderer.localBounds;
            if (glyphs.size.x <= .0001f || glyphs.size.y <= .0001f) return false;
            float scale = Mathf.Min(width * .88f / glyphs.size.x, height * .72f / glyphs.size.y);
            label.transform.localScale = Vector3.one * scale;
            Vector3 center = face.localPosition + Vector3.forward * (Mathf.Abs(face.localScale.z) * .5f + .006f);
            label.transform.localPosition = center - label.transform.localRotation * (glyphs.center * scale);
            Record(label);
            Record(label.transform);
            return true;
        }

        private static void Remove(TextMesh label)
        {
            Transform parent = label.transform.parent;
            if (label.name == "StatusText" && parent != null && parent.name == "LabelAnchor")
            {
                Transform plate = parent.Find("StatusPlate");
                if (plate != null) RemoveArt(plate);
            }
            if (parent != null && IsSignHolder(parent.name))
            {
                if (OnlyArt(parent)) RemoveArt(parent);
                else
                {
                    // A legacy sign can own a physical collider: keep it and hide only its decorative face and letters.
                    foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                        if (child != parent && (child.name == "Fascia" || child.name == "FadedFascia" ||
                            child.name == "Paper" || child.name == "status backing")) HideFace(child);
                    RemoveTextView(label);
                }
                return;
            }
            if (OnlyArt(label.transform)) RemoveArt(label.transform);
            else RemoveTextView(label);
        }

        private static void RemoveBlankSignArt(Transform root)
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                if (item == null || item == root) continue;
                bool emptySign = IsSignHolder(item.name) && item.GetComponentsInChildren<TextMesh>(true).Length == 0;
                if ((emptySign || item.name == "StatusTag" || item.name == "ClearanceCheckInFascia" ||
                    item.name == "StatusPlate") && OnlyArt(item)) RemoveArt(item);
            }
        }

        private static void RemoveTextView(TextMesh label)
        {
            label.text = string.Empty;
            Renderer renderer = label.GetComponent<Renderer>();
            if (renderer != null) { renderer.enabled = false; Record(renderer); }
            Record(label);
        }

        private static void HideFace(Transform face)
        {
            Renderer renderer = face.GetComponent<Renderer>();
            if (renderer != null) { renderer.enabled = false; Record(renderer); }
        }

        private static void RemoveArt(Transform art)
        {
            if (!OnlyArt(art)) return;
            if (!PrefabUtility.IsPartOfPrefabInstance(art.gameObject))
            {
                Object.DestroyImmediate(art.gameObject);
                return;
            }
            // Source models are cleaned above; nested instances retain their gameplay references and removal overrides.
            foreach (TextMesh label in art.GetComponentsInChildren<TextMesh>(true)) RemoveTextView(label);
            foreach (Renderer renderer in art.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
                Record(renderer);
            }
        }

        private static bool OnlyArt(Transform art)
        {
            foreach (Component component in art.GetComponentsInChildren<Component>(true))
                if (component == null || !(component is Transform || component is MeshFilter || component is Renderer ||
                    component is TextMesh || component is WorldTextBinding || component is LODGroup ||
                    component is SectionProp || component is WalkThroughObject)) return false;
            return true;
        }

        private static bool IsSignHolder(string name)
            => name.StartsWith("Sign_", StringComparison.Ordinal) || name.StartsWith("Notice_", StringComparison.Ordinal) ||
               name.StartsWith("Mounted_", StringComparison.Ordinal);

        private static bool IsEssential(string words)
            => words == "EXIT" || words == "SERVICE" || words == "SERVICE STAIRS" ||
               words == "COLLECTION" || words == "COLLECTION >";

        private static string Normalise(string words)
            => string.Join(" ", (words ?? string.Empty).ToUpperInvariant()
                .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));

        private static void Record(Component component)
        {
            EditorUtility.SetDirty(component);
            if (PrefabUtility.IsPartOfPrefabInstance(component))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }
    }
}

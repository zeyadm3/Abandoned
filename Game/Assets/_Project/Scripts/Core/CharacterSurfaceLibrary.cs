using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Persistent character PBR surfaces, shared by worker bodies, first-person hands and carried tools.</summary>
    [CreateAssetMenu(menuName = "Abandoned/Art/Character Surfaces", fileName = "CharacterSurfaceLibrary")]
    public sealed class CharacterSurfaceLibrary : ScriptableObject
    {
        [SerializeField] private string[] keys = Array.Empty<string>();
        [SerializeField] private Material[] materials = Array.Empty<Material>();
        private static CharacterSurfaceLibrary current;
        private static readonly Dictionary<string, Material> cache = new();

        public static Material Get(string key, Color? tint = null)
        {
            string id = tint.HasValue ? key + "/" + ColorUtility.ToHtmlStringRGBA(tint.Value) : key;
            if (cache.TryGetValue(id, out Material cached) && cached != null) return cached;
            if (current == null) current = Resources.Load<CharacterSurfaceLibrary>("CharacterSurfaceLibrary");
            Material source = null;
            if (current != null)
            {
                int index = Array.IndexOf(current.keys, key);
                if (index >= 0 && index < current.materials.Length) source = current.materials[index];
            }
            if (source != null && !tint.HasValue) return cache[id] = source;
            Shader shader = source != null ? source.shader : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return null;
            var material = source != null ? new Material(source) : new Material(shader);
            material.name = "CharacterSurface_" + id;
            material.enableInstancing = true;
            Color colour = tint ?? DefaultColour(key);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Color")) material.SetColor("_Color", colour);
            if (source == null)
            {
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", key == "Steel" ? .45f : .18f);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", key == "Steel" ? .8f : 0f);
            }
            return cache[id] = material;
        }

        private static Color DefaultColour(string key) => key switch
        {
            "CrewFabric" => new Color(.88f, .39f, .085f),
            "CrewTrim" or "Rubber" => new Color(.075f, .09f, .1f),
            "Steel" => new Color(.42f, .46f, .48f),
            "Tape" => new Color(.72f, .75f, .67f),
            "Skin" => new Color(.63f, .46f, .34f),
            "Canvas" => new Color(.36f, .34f, .27f),
            "Leather" => new Color(.12f, .1f, .08f),
            "Wood" => new Color(.45f, .3f, .16f),
            _ => new Color(.2f, .23f, .24f),
        };

#if UNITY_EDITOR
        public void EditorSetup(string[] names, Material[] surfaces)
        {
            keys = names;
            materials = surfaces;
            current = this;
            cache.Clear();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { current = null; cache.Clear(); }
    }
}

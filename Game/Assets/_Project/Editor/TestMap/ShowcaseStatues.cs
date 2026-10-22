using System;
using System.Linq;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Still copies of the crew and the monsters for the TestMap gallery: the same models and materials the game uses,
    /// frozen in a pose and stripped of every behaviour (no AI, no network, no animator, no colliders), so they only
    /// stand there to be looked at. A monster is posed from a frame of its own Idle take.
    /// </summary>
    public static class ShowcaseStatues
    {
        /// <summary>The roster, as the prefabs in Prefabs/Threats are named.</summary>
        public static readonly string[] Threats = { "Hunter", "LastHunter", "Stalker", "BlindOne", "Collector", "Weight", "Thing", "Crawlers" };
        private const string ThreatFolder = "Assets/_Project/Prefabs/Threats";
        private const string CoverallFolder = "Assets/_Project/Data/Player/Cosmetics";

        /// <summary>The crew worker at rest (arms down, elbows soft) in the coverall colour <paramref name="coverall"/>.</summary>
        public static GameObject Crew(Transform parent, string coverall)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorkerVisualBuilder.PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Showcase] {WorkerVisualBuilder.PrefabPath} is missing; no crew statue.");
                return null;
            }
            var statue = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            PrefabUtility.UnpackPrefabInstance(statue, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            statue.name = "Crew_" + coverall;
            WorkerRig rig = statue.GetComponentInChildren<WorkerRig>(true);
            if (rig != null)
            {
                // WorkerPresentation's standing pose.
                rig.LeftArm.localRotation = Quaternion.Euler(0f, 0f, -9f);
                rig.RightArm.localRotation = Quaternion.Euler(0f, 0f, 9f);
                if (rig.LeftElbow != null) rig.LeftElbow.localRotation = Quaternion.Euler(-10f, 0f, 0f);
                if (rig.RightElbow != null) rig.RightElbow.localRotation = Quaternion.Euler(-10f, 0f, 0f);
                // The jacket is tinted at run time by a property block, which a scene doesn't keep: give it its own material.
                Material suit = rig.Coverall.FirstOrDefault(r => r != null)?.sharedMaterial;
                Material jacket = Jacket(suit, coverall);
                foreach (Renderer renderer in rig.Coverall)
                {
                    if (renderer == null || jacket == null) continue;
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++) if (materials[i] == suit) materials[i] = jacket;
                    renderer.sharedMaterials = materials;
                }
            }
            Strip(statue);
            return statue;
        }

        /// <summary>Monster <paramref name="name"/> frozen at <paramref name="at"/> (0-1) through its <paramref name="motion"/> take.</summary>
        public static GameObject Threat(Transform parent, string name, string motion = "Idle", float at = .35f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ThreatFolder}/{name}.prefab");
            Transform visual = prefab != null ? prefab.transform.Find("Visual") : null;
            if (visual == null)
            {
                Debug.LogWarning($"[Showcase] {ThreatFolder}/{name}.prefab has no Visual; no statue.");
                return null;
            }
            GameObject statue = Object.Instantiate(visual.gameObject, parent);
            statue.name = name;
            Animator animator = statue.GetComponentInChildren<Animator>(true);
            AnimationClip[] clips = animator != null && animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.animationClips : Array.Empty<AnimationClip>();
            AnimationClip clip = clips.FirstOrDefault(c => c.name == motion) ?? clips.FirstOrDefault();
            // The takes are generic (not humanoid), so a clip can write its bones straight into the copy.
            if (clip != null) clip.SampleAnimation(animator.gameObject, clip.length * at);
            Strip(statue);
            return statue;
        }

        /// <summary>World bounds of what is drawn, measured from the meshes (skinned ones in their posed shape).</summary>
        public static Bounds WorldBounds(GameObject root)
        {
            bool any = false;
            var bounds = new Bounds();
            var baked = new Mesh();
            void Add(Vector3 p)
            {
                if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; }
                else bounds.Encapsulate(p);
            }
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer || !renderer.enabled) continue;
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    if (skinned.sharedMesh == null) continue;
                    skinned.BakeMesh(baked, true);
                    Matrix4x4 m = Matrix4x4.TRS(skinned.transform.position, skinned.transform.rotation, Vector3.one);
                    foreach (Vector3 v in baked.vertices) Add(m.MultiplyPoint3x4(v));
                }
                else if (renderer.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
                {
                    Bounds local = filter.sharedMesh.bounds;
                    for (int corner = 0; corner < 8; corner++)
                    {
                        var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                        Add(renderer.transform.TransformPoint(local.center + Vector3.Scale(local.extents, sign)));
                    }
                }
            }
            Object.DestroyImmediate(baked);
            return bounds;
        }

        /// <summary>Spaced names for labels: "LastHunter" -> "Last Hunter".</summary>
        public static string Pretty(string name) =>
            string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()));

        private static Material Jacket(Material suit, string coverall)
        {
            if (suit == null) return null;
            var definition = AssetDatabase.LoadAssetAtPath<CosmeticDefinition>($"{CoverallFolder}/Coverall_{coverall}.asset");
            Color colour = definition != null ? definition.Color : Color.white;
            PolishAssets.EnsureFolder(ShowcaseMaterials.Folder);
            string path = $"{ShowcaseMaterials.Folder}/TestMap_Crew_Jacket_{coverall}.mat";
            var jacket = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (jacket == null)
            {
                jacket = new Material(suit);
                AssetDatabase.CreateAsset(jacket, path);
            }
            else jacket.CopyPropertiesFromMaterial(suit);
            jacket.SetColor("_BaseColor", colour);
            EditorUtility.SetDirty(jacket);
            return jacket;
        }

        // Keep what is drawn; drop everything that acts. Dependents go before what they depend on.
        private static void Strip(GameObject root)
        {
            Remove<MonoBehaviour>(root);
            Remove<Joint>(root);
            Remove<Animator>(root);
            Remove<Collider>(root);
            Remove<Rigidbody>(root);
            Remove<AudioSource>(root);
            Remove<UnityEngine.AI.NavMeshAgent>(root);
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = 0;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            }
        }

        private static void Remove<T>(GameObject root) where T : Component
        {
            foreach (T component in root.GetComponentsInChildren<T>(true)) Object.DestroyImmediate(component);
        }
    }
}

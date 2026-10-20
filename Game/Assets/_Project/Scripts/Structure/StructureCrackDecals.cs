using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Abandoned.Structure
{
    /// <summary>
    /// Cracks you can see (0.24.12): a floor that is stressed shows hairlines, one that is cracking or failing shows
    /// the full fracture, on its walking face and on the ceiling of the room beneath it. Driven by the replicated
    /// stage (StageChanged), so every machine draws the same cracks and nothing new is sent. It only shows what the
    /// creak already says: a creak still only ever means the floor. Which crack a slab gets comes from its id.
    /// </summary>
    public class StructureCrackDecals : MonoBehaviour
    {
        public const string MaterialResource = "AV_Cracks";
        private const float Depth = 0.6f;
        private static Material material;

        private readonly Dictionary<StructuralSection, (DecalProjector top, DecalProjector under)> projectors = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            OnLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Application.isBatchMode) return;
            material ??= Resources.Load<Material>(MaterialResource);
            if (material == null) return;
            var sections = FindObjectsByType<StructuralSection>(FindObjectsSortMode.None);
            if (sections.Length == 0) return;
            var go = new GameObject("StructureCrackDecals");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<StructureCrackDecals>().Watch(sections);
        }

        private void Watch(StructuralSection[] sections)
        {
            foreach (StructuralSection s in sections)
            {
                if (s == null || s.Type != SectionType.Floor) continue;
                s.StageChanged += OnStage;
                s.Restored += OnRestored;
                Show(s, s.Stage);
            }
        }

        private void OnDestroy()
        {
            foreach (StructuralSection s in projectors.Keys)
            {
                if (s == null) continue;
                s.StageChanged -= OnStage;
                s.Restored -= OnRestored;
            }
        }

        private void OnStage(StructuralSection s, StructuralStage previous) => Show(s, s.Stage);
        private void OnRestored(StructuralSection s) => Show(s, StructuralStage.Stable);

        private void Show(StructuralSection s, StructuralStage stage)
        {
            float opacity = stage switch
            {
                StructuralStage.Stressed => 0.35f,
                StructuralStage.Cracking => 0.8f,
                StructuralStage.Failing => 1f,
                _ => 0f,
            };
            if (opacity <= 0f && !projectors.ContainsKey(s)) return;
            if (!projectors.TryGetValue(s, out var pair)) projectors[s] = pair = Make(s);
            Set(pair.top, opacity);
            Set(pair.under, opacity * 0.85f);
        }

        private static void Set(DecalProjector p, float opacity)
        {
            if (p == null) return;
            p.fadeFactor = opacity;
            p.enabled = opacity > 0f;
        }

        private (DecalProjector, DecalProjector) Make(StructuralSection s)
        {
            Bounds b = s.SurfaceBounds;
            if (b.size.x < 0.5f || b.size.z < 0.5f) return (null, null);
            // One of four cracks in the 2 x 2 atlas, from where the slab is, so every machine picks the same one.
            int variant = Mathf.Abs(Mathf.RoundToInt(b.center.x * 7f) * 31 + Mathf.RoundToInt(b.center.z * 13f)) % 4;
            var bias = new Vector2(variant % 2 * 0.5f, variant / 2 * 0.5f);
            // The boxes sit inside the slab with only 5 cm above (below) its face, so boots and loot on it stay clean.
            DecalProjector top = Projector(s, "CracksTop", new Vector3(b.center.x, b.max.y - Depth * 0.5f + 0.05f, b.center.z), Quaternion.Euler(90f, 0f, 0f), b, bias);
            DecalProjector under = Projector(s, "CracksUnder", new Vector3(b.center.x, b.min.y + Depth * 0.5f - 0.05f, b.center.z), Quaternion.Euler(-90f, 0f, 0f), b, bias);
            return (top, under);
        }

        private DecalProjector Projector(StructuralSection s, string name, Vector3 at, Quaternion facing, Bounds b, Vector2 bias)
        {
            var go = new GameObject(name);
            go.transform.SetParent(s.transform, true);
            go.transform.SetPositionAndRotation(at, facing);
            var p = go.AddComponent<DecalProjector>();
            p.material = material;
            p.size = new Vector3(b.size.x * 0.96f, b.size.z * 0.96f, Depth);
            p.uvScale = new Vector2(0.5f, 0.5f);
            p.uvBias = bias;
            p.drawDistance = 45f;
            p.enabled = false;
            return p;
        }
    }
}

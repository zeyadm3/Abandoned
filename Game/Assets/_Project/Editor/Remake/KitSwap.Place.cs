using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools.Remake
{
    public static partial class KitSwap
    {
        /// <summary>How far a stood model's footprint may stretch against its height (more looks wrong).</summary>
        private const float Squash = 1.35f;

        private static readonly Dictionary<string, List<GameObject>> models = new();
        private static readonly Dictionary<GameObject, Vector3> natural = new();

        /// <summary>Puts back anything a previous run took off, then swaps every prop a model exists for.</summary>
        public static string Apply(Scene scene)
        {
            int restored = Undo(scene);
            models.Clear();
            natural.Clear();
            Physics.SyncTransforms();
            List<Candidate> found = Find(scene, new Dictionary<string, int>(), null);
            KitSwapRecord record = RecordFor(scene, true);
            int swapped = 0;
            var missing = new HashSet<string>();
            foreach (Candidate c in found)
            {
                List<GameObject> kit = ModelsFor(c.Family.Id);
                if (kit.Count == 0)
                {
                    missing.Add(c.Family.Id);
                    continue;
                }
                try
                {
                    Place(c, kit, record);
                    swapped++;
                }
                catch (System.Exception e)
                {
                    Transform left = c.Group.Find("RM_Kit_" + c.Family.Id);
                    if (left != null && !record.Added.Contains(left.gameObject)) Destroy(left.gameObject);
                    Debug.LogWarning($"[KitSwap] {scene.name}: {c.Group.name} not swapped: {e.Message}");
                }
            }
            // The HQ's working stations (gear rack, shop terminal) are dressed here too, so an undo and redo keeps them.
            swapped += HqStationModels.Dress(scene, record);
            EditorUtility.SetDirty(record);
            return $"[KitSwap] {scene.name}: {swapped} props swapped ({restored} renderers restored first); no models yet for: " +
                   (missing.Count > 0 ? string.Join(", ", missing.OrderBy(m => m)) : "none");
        }

        /// <summary>Puts every recorded renderer back and removes the models. Returns how many renderers came back.</summary>
        public static int Undo(Scene scene)
        {
            KitSwapRecord record = RecordFor(scene, false);
            if (record == null) return 0;
            foreach (GameObject kit in record.Added) Destroy(kit);
            int restored = 0;
            foreach (KitSwapRecord.Removed r in record.RemovedRenderers)
            {
                if (r.owner == null) continue;
                // Not ??: a missing component is Unity's fake null, not a C# null.
                MeshFilter filter = r.owner.GetComponent<MeshFilter>();
                if (filter == null) filter = r.owner.AddComponent<MeshFilter>();
                filter.sharedMesh = r.mesh;
                MeshRenderer renderer = r.owner.GetComponent<MeshRenderer>();
                if (renderer == null) renderer = r.owner.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = r.materials;
                renderer.renderingLayerMask = r.renderingLayers;
                renderer.shadowCastingMode = r.shadows;
                restored++;
            }
            foreach (KitSwapRecord.Levels l in record.Silenced)
            {
                if (l.group == null) continue;
                var lods = new LOD[l.heights.Length];
                for (int i = 0; i < lods.Length; i++)
                    lods[i] = new LOD(l.heights[i], l.owners.Where((o, k) => l.level[k] == i && o != null)
                        .Select(o => (Renderer)o.GetComponent<MeshRenderer>()).Where(x => x != null).ToArray());
                l.group.SetLODs(lods);
                l.group.enabled = true;
            }
            record.EditorClear();
            EditorUtility.SetDirty(record);
            return restored;
        }

        private static List<GameObject> ModelsFor(string id)
        {
            if (models.TryGetValue(id, out List<GameObject> list)) return list;
            list = AssetDatabase.FindAssets($"Kit_{id}_ t:GameObject", new[] { KitFolder })
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => System.IO.Path.GetFileName(p).StartsWith($"Kit_{id}_") && p.EndsWith(".fbx"))
                .OrderBy(p => p).Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(g => g != null).ToList();
            return models[id] = list;
        }

        private static void Place(Candidate c, List<GameObject> kit, KitSwapRecord record)
        {
            Transform t = c.Group;
            Vector3 box = c.Box.size;
            Vector3 parentScale = Abs(t.lossyScale);
            Vector3 real = Vector3.Scale(box, parentScale);
            int seed = c.Family.PerMap ? Stable(t.gameObject.scene.name) : Seed(t);

            // The model and its quarter turn whose proportions match the copy best; a wall piece then turns its back to
            // the wall, anything else either way round.
            (GameObject model, int quarter) = Pick(kit, real, c.Family.Long, seed);
            if (c.Family.AgainstWall) quarter = FaceAway(t, c.Box, quarter);
            else if ((seed & 2) != 0) quarter += 2;
            bool sideways = (quarter & 1) == 1;

            var holder = new GameObject("RM_Kit_" + c.Family.Id);
            holder.transform.SetParent(t, false);
            GameObject imported = (GameObject)PrefabUtility.InstantiatePrefab(model);
            imported.transform.SetParent(holder.transform, false);
            foreach (Collider collider in imported.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            Vector3 nat = Natural(model);

            // Sizes in the holder's own axes (before its turn), in metres, then into the parent's (possibly stretched) units.
            Vector3 target = sideways ? new Vector3(real.z, real.y, real.x) : real;
            Vector3 s = new(target.x / nat.x, target.y / nat.y, target.z / nat.z);
            if (c.Family.Fitting != KitFamily.Fit.Stretch)
            {
                float limit = c.Family.Fitting == KitFamily.Fit.Hang ? 1.5f : Squash;
                s.x = Mathf.Clamp(s.x, s.y / limit, s.y * limit);
                s.z = Mathf.Clamp(s.z, s.y / limit, s.y * limit);
            }
            Vector3 along = sideways ? new Vector3(parentScale.z, parentScale.y, parentScale.x) : parentScale;
            holder.transform.localRotation = Quaternion.Euler(0f, 90f * quarter, 0f);
            holder.transform.localScale = new Vector3(s.x / Mathf.Max(along.x, 1e-5f), s.y / Mathf.Max(along.y, 1e-5f), s.z / Mathf.Max(along.z, 1e-5f));
            Bounds placed = ModelFit.LocalBounds(holder, t);
            Vector3 goal = c.Family.Fitting == KitFamily.Fit.Stand
                ? new Vector3(c.Box.center.x, c.Box.min.y, c.Box.center.z)
                : c.Box.center;
            Vector3 have = c.Family.Fitting == KitFamily.Fit.Stand
                ? new Vector3(placed.center.x, placed.min.y, placed.center.z)
                : placed.center;
            holder.transform.localPosition += goal - have;

            // Draws like the copy did: same layer, storey light layers, shadows and static flags.
            MeshRenderer first = c.Renderers[0];
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(first.gameObject);
            foreach (Transform part in holder.GetComponentsInChildren<Transform>(true))
            {
                part.gameObject.layer = first.gameObject.layer;
                GameObjectUtility.SetStaticEditorFlags(part.gameObject, flags);
            }
            foreach (Renderer r in holder.GetComponentsInChildren<Renderer>(true))
            {
                r.renderingLayerMask = first.renderingLayerMask;
                if (first.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off) r.shadowCastingMode = first.shadowCastingMode;
            }

            foreach (LODGroup group in t.GetComponentsInChildren<LODGroup>(true))
            {
                if (group.transform.IsChildOf(holder.transform)) continue;
                LOD[] lods = group.GetLODs();
                var owners = new List<GameObject>();
                var level = new List<int>();
                for (int i = 0; i < lods.Length; i++)
                    foreach (Renderer r in lods[i].renderers)
                        if (r != null) { owners.Add(r.gameObject); level.Add(i); }
                record.EditorSilenced(new KitSwapRecord.Levels
                {
                    group = group, heights = lods.Select(l => l.screenRelativeTransitionHeight).ToArray(),
                    owners = owners.ToArray(), level = level.ToArray()
                });
                group.SetLODs(new LOD[0]);
                group.enabled = false;
            }
            foreach (MeshRenderer r in c.Renderers)
            {
                var filter = r.GetComponent<MeshFilter>();
                record.EditorRemoved(new KitSwapRecord.Removed
                {
                    owner = r.gameObject, mesh = filter.sharedMesh, materials = r.sharedMaterials,
                    renderingLayers = r.renderingLayerMask, shadows = r.shadowCastingMode
                });
                Object.DestroyImmediate(r);
                Object.DestroyImmediate(filter);
            }
            record.EditorAdded(holder);
        }

        /// <summary>Takes these renderers off (recorded, so <see cref="Undo"/> brings them back) and stands `model` at the
        /// origin of `parent`, drawing like the first of them did.</summary>
        public static GameObject Replace(KitSwapRecord record, List<MeshRenderer> renderers, GameObject model, Transform parent, string name)
        {
            if (renderers.Count == 0) return null;
            var holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            go.transform.SetParent(holder.transform, false);
            foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            MeshRenderer first = renderers[0];
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(first.gameObject);
            foreach (Transform part in holder.GetComponentsInChildren<Transform>(true))
            {
                part.gameObject.layer = first.gameObject.layer;
                GameObjectUtility.SetStaticEditorFlags(part.gameObject, flags);
            }
            foreach (Renderer r in holder.GetComponentsInChildren<Renderer>(true)) r.renderingLayerMask = first.renderingLayerMask;
            foreach (MeshRenderer r in renderers)
            {
                var filter = r.GetComponent<MeshFilter>();
                record.EditorRemoved(new KitSwapRecord.Removed
                {
                    owner = r.gameObject, mesh = filter.sharedMesh, materials = r.sharedMaterials,
                    renderingLayers = r.renderingLayerMask, shadows = r.shadowCastingMode
                });
                Object.DestroyImmediate(r);
                Object.DestroyImmediate(filter);
            }
            record.EditorAdded(holder);
            return holder;
        }

        /// <summary>The variant and quarter turn (0 or 1) whose proportions are closest to the copy's (ties broken by
        /// the copy's place). A model that isn't long (a chair, a crate) may stand either way, so only its variant counts.</summary>
        private static (GameObject, int) Pick(List<GameObject> kit, Vector3 real, bool turnToFit, int seed)
        {
            float Score(Vector3 n, int q)
            {
                Vector3 want = q == 1 ? new Vector3(real.z, real.y, real.x) : real;
                return Mathf.Abs(Mathf.Log(want.x / want.y * n.y / n.x)) + Mathf.Abs(Mathf.Log(want.z / want.y * n.y / n.z));
            }
            var ranked = kit.SelectMany(m => new[] { 0, 1 }.Select(q => (m, q, score: Score(Natural(m), q))))
                .Where(x => turnToFit || x.q == ((seed >> 3) & 1)).OrderBy(x => x.score).ToList();
            var close = ranked.Where(x => x.score <= ranked[0].score + .2f).ToList();
            var pick = close[(seed & 0x7fffffff) % close.Count];
            return (pick.m, pick.q);
        }

        /// <summary>A model's own size, in metres, as it stands unturned.</summary>
        private static Vector3 Natural(GameObject model)
        {
            if (natural.TryGetValue(model, out Vector3 size)) return size;
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(model);
            try { size = ModelFit.LocalBounds(probe, probe.transform).size; }
            finally { Object.DestroyImmediate(probe); }
            size = Vector3.Max(size, Vector3.one * .01f);
            return natural[model] = size;
        }

        /// <summary>Of the two turns along the chosen axis, the one whose back (model -Z) is nearer a wall.</summary>
        private static int FaceAway(Transform t, Bounds box, int quarter)
        {
            Vector3 centre = t.TransformPoint(box.center);
            float Hit(int q)
            {
                Vector3 back = t.TransformDirection(Quaternion.Euler(0f, 90f * q, 0f) * Vector3.back);
                back.y = 0f;
                if (back.sqrMagnitude < 1e-6f) return float.MaxValue;
                float best = float.MaxValue;
                foreach (RaycastHit hit in Physics.RaycastAll(centre, back.normalized, 3f, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.collider.transform.IsChildOf(t) && hit.distance < best) best = hit.distance;
                return best;
            }
            return Hit(quarter) <= Hit(quarter + 2) ? quarter : quarter + 2;
        }

        private static int Stable(string text)
        {
            unchecked
            {
                int h = 23;
                foreach (char ch in text) h = h * 31 + ch;
                return h;
            }
        }

        private static int Seed(Transform t)
        {
            Vector3 p = t.position;
            return Mathf.RoundToInt(p.x * 7.31f) * 73856093 ^ Mathf.RoundToInt(p.y * 5.17f) * 19349663 ^ Mathf.RoundToInt(p.z * 3.97f) * 83492791;
        }
    }
}

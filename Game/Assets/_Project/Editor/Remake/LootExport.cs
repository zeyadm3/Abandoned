using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Hands every loot item's current look to Blender for the remaster (Tools/Blender/remake/loot/remaster.py): the
    /// visible renderers of its generated prefab, in the prefab's own space (so the remastered model drops back in the
    /// same place, at the same size), as OBJ with one group per material, plus a manifest with each material's colour,
    /// texture and the item's id, Size and carry class. Writes Game/Logs/remake/loot/ (not an asset folder).
    /// </summary>
    public static class LootExport
    {
        public const string OutFolder = "Logs/remake/loot";

        [System.Serializable]
        private class Entry { public string id, name, carry, tags; public Vector3 size; public string obj; public List<Mat> mats = new(); }

        [System.Serializable]
        private class Mat { public string name, texture; public Color color; public float metallic, smoothness; }

        [System.Serializable]
        private class Manifest { public List<Entry> items = new(); }

        [MenuItem(ToolMenu.Loot + "Export Loot for Remaster (remake)", priority = ToolMenu.LootP + 40)]
        public static void Export()
        {
            Directory.CreateDirectory(OutFolder);
            var manifest = new Manifest();
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition"))
            {
                var d = AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (d == null) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LootPrefabGenerator.PrefabPathFor(d));
                if (prefab == null) continue;
                var entry = new Entry { id = d.Id, name = d.DisplayName, carry = d.CarryClass.ToString(), size = d.Size, obj = d.Id + ".obj" };
                if (Write(prefab, Path.Combine(OutFolder, entry.obj), entry)) manifest.items.Add(entry);
            }
            File.WriteAllText(Path.Combine(OutFolder, "manifest.json"), JsonUtility.ToJson(manifest, true));
            Debug.Log($"[Remake] exported {manifest.items.Count} loot items to {OutFolder}.");
        }

        private static bool Write(GameObject prefab, string path, Entry entry)
        {
            var sb = new StringBuilder();
            var inv = CultureInfo.InvariantCulture;
            Matrix4x4 root = prefab.transform.worldToLocalMatrix;
            int vBase = 1, written = 0;
            foreach (MeshRenderer r in prefab.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.enabled || !ActiveChain(r.transform)) continue;
                if (Under(r.transform, "OutlineVisual")) continue;
                var mf = r.GetComponent<MeshFilter>();
                Mesh mesh = mf != null ? mf.sharedMesh : null;
                if (mesh == null || !mesh.isReadable && !CanRead(mesh)) continue;
                Matrix4x4 m = root * r.transform.localToWorldMatrix;
                Vector3[] v = mesh.vertices;
                Vector3[] n = mesh.normals;
                Vector2[] uv = mesh.uv;
                foreach (Vector3 p in v)
                {
                    Vector3 w = m.MultiplyPoint3x4(p);
                    // OBJ is right-handed: mirror X (Blender's importer undoes it with its default axes).
                    sb.Append("v ").Append((-w.x).ToString(inv)).Append(' ').Append(w.y.ToString(inv)).Append(' ').Append(w.z.ToString(inv)).Append('\n');
                }
                for (int i = 0; i < v.Length; i++)
                {
                    Vector3 nn = i < n.Length ? m.MultiplyVector(n[i]).normalized : Vector3.up;
                    sb.Append("vn ").Append((-nn.x).ToString(inv)).Append(' ').Append(nn.y.ToString(inv)).Append(' ').Append(nn.z.ToString(inv)).Append('\n');
                    Vector2 t = i < uv.Length ? uv[i] : Vector2.zero;
                    sb.Append("vt ").Append(t.x.ToString(inv)).Append(' ').Append(t.y.ToString(inv)).Append('\n');
                }
                Material[] mats = r.sharedMaterials;
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    Material mat = s < mats.Length ? mats[s] : null;
                    string key = Key(mat, entry);
                    sb.Append("usemtl ").Append(key).Append('\n');
                    int[] tris = mesh.GetTriangles(s);
                    for (int k = 0; k < tris.Length; k += 3)
                    {
                        // Mirroring X flips the winding: write the triangle reversed.
                        int a = tris[k] + vBase, b = tris[k + 2] + vBase, c = tris[k + 1] + vBase;
                        sb.Append($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}\n");
                        written++;
                    }
                }
                vBase += v.Length;
            }
            if (written == 0) return false;
            File.WriteAllText(path, sb.ToString());
            return true;
        }

        private static bool CanRead(Mesh mesh)
        {
            // In the editor, mesh data of imported models is readable through the asset even when isReadable is off.
            try { return mesh.vertexCount > 0 && mesh.vertices.Length > 0; }
            catch { return false; }
        }

        /// <summary>Prefab assets are never "in the hierarchy": walk activeSelf up instead.</summary>
        private static bool ActiveChain(Transform t)
        {
            for (; t != null; t = t.parent)
                if (!t.gameObject.activeSelf) return false;
            return true;
        }

        private static bool Under(Transform t, string name)
        {
            for (; t != null; t = t.parent)
                if (t.name == name) return true;
            return false;
        }

        private static string Key(Material mat, Entry entry)
        {
            string name = mat != null ? mat.name.Replace(' ', '_') : "Default";
            foreach (Mat m in entry.mats)
                if (m.name == name) return name;
            var info = new Mat { name = name, color = Color.grey };
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor")) info.color = mat.GetColor("_BaseColor");
                else if (mat.HasProperty("_Color")) info.color = mat.GetColor("_Color");
                Texture tex = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                if (tex != null) info.texture = Path.GetFullPath(AssetDatabase.GetAssetPath(tex));
                if (mat.HasProperty("_Metallic")) info.metallic = mat.GetFloat("_Metallic");
                if (mat.HasProperty("_Smoothness")) info.smoothness = mat.GetFloat("_Smoothness");
            }
            entry.mats.Add(info);
            return name;
        }
    }
}

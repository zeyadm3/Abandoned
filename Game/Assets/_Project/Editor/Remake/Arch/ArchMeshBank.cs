using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// All of one map's architecture meshes in a single asset (Art/Remake/Maps/Arch/&lt;Map&gt;/ArchMeshes.asset), identical
    /// ones shared: a 4 m floor tile with the same neighbours is drawn by one mesh however often it repeats. The bank is
    /// started afresh on every run (the previous drawings are gone by then).
    /// </summary>
    public sealed class ArchMeshBank
    {
        private readonly string path;
        private readonly Dictionary<ulong, Mesh> meshes = new();
        private readonly List<Mesh> order = new();

        public int Count => order.Count;

        public ArchMeshBank(string map)
        {
            string folder = $"{RemakePaths.Maps}/Arch/{map}";
            PolishAssets.EnsureFolder(folder);
            path = $"{folder}/ArchMeshes.asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
        }

        /// <summary>The bank's copy of a mesh with this content (the given one, the first time).</summary>
        public Mesh Add(Mesh mesh)
        {
            ulong key = Hash(mesh);
            if (meshes.TryGetValue(key, out Mesh existing))
            {
                Object.DestroyImmediate(mesh);
                return existing;
            }
            mesh.name = $"Arch_{order.Count:00000}_{mesh.name}";
            meshes[key] = mesh;
            order.Add(mesh);
            return mesh;
        }

        public void Save()
        {
            if (order.Count == 0) return;
            AssetDatabase.CreateAsset(order[0], path);
            for (int i = 1; i < order.Count; i++) AssetDatabase.AddObjectToAsset(order[i], order[0]);
            AssetDatabase.SaveAssets();
        }

        private static ulong Hash(Mesh mesh)
        {
            ulong h = 1469598103934665603UL;
            void Mix(int v)
            {
                h ^= (uint)v;
                h *= 1099511628211UL;
            }
            Mix(mesh.vertexCount);
            Mix(mesh.subMeshCount);
            foreach (Vector3 v in mesh.vertices)
            {
                Mix(Mathf.RoundToInt(v.x * 2000f));
                Mix(Mathf.RoundToInt(v.y * 2000f));
                Mix(Mathf.RoundToInt(v.z * 2000f));
            }
            for (int s = 0; s < mesh.subMeshCount; s++) Mix((int)mesh.GetIndexCount(s));
            return h;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Doors and lockers are modelled from boxes, then each part is merged into one saved mesh per material:
    /// every locker and every leaf of a kind shares the same few meshes, instead of a dozen cubes each.
    /// </summary>
    public static class MallRoomsMeshes
    {
        public const string Folder = "Assets/_Project/Art/Generated/Rooms0160";

        /// <summary>Replaces the boxes under <paramref name="part"/> with one renderer per material. The same key always yields the same meshes.</summary>
        public static void Merge(Transform part, string key)
        {
            PolishAssets.EnsureFolder(Folder);
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] sources = part.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter source in sources)
            {
                Material material = source.GetComponent<MeshRenderer>().sharedMaterial;
                if (material == null) continue;
                if (!byMaterial.TryGetValue(material, out List<CombineInstance> pieces)) byMaterial[material] = pieces = new List<CombineInstance>();
                pieces.Add(new CombineInstance { mesh = source.sharedMesh, transform = part.worldToLocalMatrix * source.transform.localToWorldMatrix });
            }
            foreach (MeshFilter source in sources) Object.DestroyImmediate(source.gameObject);
            foreach (KeyValuePair<Material, List<CombineInstance>> pair in byMaterial.OrderBy(p => p.Key.name, StringComparer.Ordinal))
            {
                string name = $"{key}_{Clean(pair.Key.name)}";
                var mesh = new Mesh { name = name };
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                mesh = SalvageAssetBuilder.SaveMesh(mesh, $"{Folder}/{name}.asset");
                Transform drawn = GreyboxFactory.Group(pair.Key.name, part);
                drawn.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                drawn.gameObject.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
        }

        private static string Clean(string value) =>
            new string(value.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
    }
}

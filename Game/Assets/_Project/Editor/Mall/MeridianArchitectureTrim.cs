using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianArchitectureTrim
    {
        private static Material brass;
        private const string Folder = "Assets/_Project/Art/Generated/MeridianDetails";

        public static void WallBands(Transform group, Vector3 at, Vector3 front, bool plinth)
        {
            Quaternion rotation = Quaternion.LookRotation(front);
            Box("StoryCornice", group, at + Vector3.up * 3.85f + front * 0.13f, rotation,
                new Vector3(4f, 0.19f, 0.13f), MeridianArt.Material("FacadeStone"));
            if (plinth)
            {
                Box("StonePlinth", group, at + Vector3.up * 0.13f + front * 0.135f, rotation,
                    new Vector3(4f, 0.26f, 0.065f), MeridianArt.Material("FacadeStone"));
                Box("BrassDado", group, at + Vector3.up * 1.015f + front * 0.17f, rotation,
                    new Vector3(4f, 0.035f, 0.035f), Brass());
            }
        }

        public static GameObject Box(string name, Transform group, Vector3 at, Quaternion rotation, Vector3 size, Material material)
        {
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = name;
            model.transform.SetParent(group, false);
            model.transform.SetLocalPositionAndRotation(at, rotation);
            model.transform.localScale = size;
            Object.DestroyImmediate(model.GetComponent<Collider>());
            model.GetComponent<Renderer>().sharedMaterial = material;
            MeridianArchitectureDetails.Permanent(model);
            return model;
        }

        public static Material Brass()
        {
            if(brass!=null)return brass;
            PolishAssets.EnsureFolder(Folder);
            string path = Folder + "/BrassAccent.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(MeridianArt.Material("EscalatorSteel")) { name = "Meridian_BrassAccent" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.CopyPropertiesFromMaterial(MeridianArt.Material("EscalatorSteel"));
            material.SetColor("_BaseColor", new Color(0.61f, 0.43f, 0.19f));
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            brass=material;return material;
        }
    }
}

using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalLargeLootModels
    {
        public static GameObject Build(HospitalKit kit, LootDefinition definition)
        {
            string id = definition.Id;
            if (id != "hospital_ward_frame" && id != "hospital_stretcher" && id != "hospital_chapel_cross" && id != "hospital_mixer" && id != "hospital_copper_coil" && id != "hospital_tea_urn")
            {
                return null;
            }
            string path = HospitalKit.Art + "/LootModels/" + id + ".prefab";
            HospitalKit.Ensure(HospitalKit.Art + "/LootModels");
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }
            var root = new GameObject(id);
            void Part(string name, Vector3 at, Vector3 size, Material material)
            {
                kit.Box(name, root.transform, Vector3.Scale(at, definition.Size), Vector3.Scale(size, definition.Size), material, false);
            }
            if (id == "hospital_ward_frame" || id == "hospital_stretcher")
            {
                foreach (float x in new []
                {
                    - .45f, .45f
                })
                {
                    Part("CarryRail", new Vector3(x, 0f, 0f), new Vector3(.06f, .08f, 1f), kit.Metal);
                    foreach (float z in new []
                    {
                        - .4f, .4f
                    })
                    {
                        Part("Leg", new Vector3(x, - .25f, z), new Vector3(.06f, .5f, .06f), kit.Metal);
                    }
                }
                for (int i = 0; i <7; i++)
                {
                    Part("Slat", new Vector3(0f, 0f, - .4f + i * .13f), new Vector3(.9f, .04f, .06f), kit.Metal);
                }
                if (id == "hospital_stretcher")
                {
                    Part("Canvas", new Vector3(0f, .06f, 0f), new Vector3(.8f, .04f, .8f), kit.Cloth);
                }
                else
                {
                    foreach (float z in new []
                    {
                        - .45f, .45f
                    })
                    {
                        Part("EndRail", new Vector3(0f, .25f, z), new Vector3(.95f, .45f, .06f), kit.Metal);
                    }
                }
            }
            else if (id == "hospital_chapel_cross")
            {
                Part("Base", new Vector3(0f, - .45f, 0f), new Vector3(1f, .1f, 1f), kit.Wood);
                Part("Stem", new Vector3(0f, 0f, 0f), new Vector3(.18f, 1f, .25f), kit.Metal);
                Part("Arms", new Vector3(0f, .2f, 0f), new Vector3(1f, .1f, .25f), kit.Metal);
            }
            else if (id == "hospital_mixer")
            {
                Part("Foot", new Vector3(0f, - .43f, 0f), new Vector3(1f, .14f, 1f), kit.Metal);
                Part("Column", new Vector3(0f, 0f, .3f), new Vector3(.3f, .8f, .3f), kit.Accent);
                Part("Head", new Vector3(0f, .35f, 0f), new Vector3(.6f, .25f, .85f), kit.Accent);
                Part("Bowl", new Vector3(0f, - .15f, - .15f), new Vector3(.75f, .4f, .65f), kit.Metal);
            }
            else
            {
                for (int i = 0; i <7; i++)
                {
                    Part("Rib", new Vector3(0f, - .4f + i * .13f, 0f), new Vector3(1f, .07f, .85f), kit.Metal);
                }
                Part("Riser", new Vector3(.35f, 0f, .3f), new Vector3(.08f, 1f, .08f), kit.Metal);
                if (id == "hospital_tea_urn")
                {
                    Part("Tap", new Vector3(0f, - .25f, - .48f), new Vector3(.2f, .08f, .15f), kit.Dark);
                }
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}

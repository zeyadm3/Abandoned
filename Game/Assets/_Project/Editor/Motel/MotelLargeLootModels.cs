using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelLargeLootModels
    {
        public static GameObject Build(MotelKit k, LootDefinition d)
        {
            string id = d.Id;
            if (id != "motel_percolator" && id != "motel_griddle" && id != "motel_freezer" && id != "motel_milkshake" && id != "motel_compressor" && id != "motel_pool_pump" && id != "motel_transformer")
            {
                return null;
            }
            string folder = MotelKit.Art + "/LootModels";
            MotelKit.Ensure(folder);
            string path = folder + "/" + id + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }
            var root = new GameObject(id);
            void Part(string name, Vector3 at, Vector3 size, Material material)
            {
                k.Box(name, root.transform, Vector3.Scale(at, d.Size), Vector3.Scale(size, d.Size), material, false);
            }
            if (id == "motel_freezer")
            {
                Part("InsulatedChest", Vector3.zero, new Vector3(1f, .9f, 1f), k.Wall);
                Part("LidGasket", new Vector3(0f, .43f, 0f), new Vector3(1.02f, .04f, 1.02f), k.Dark);
                Part("Lid", new Vector3(0f, .48f, 0f), new Vector3(1f, .04f, 1f), k.Metal);
                Part("Handle", new Vector3(0f, .4f, - .52f), new Vector3(.35f, .06f, .06f), k.Accent);
                for (int i = 0; i <7; i++)
                {
                    Part("CompressorVent", new Vector3(- .35f + i * .045f, - .25f, - .51f), new Vector3(.02f, .25f, .02f), k.Dark);
                }
            }
            else if (id == "motel_griddle")
            {
                Part("HeavyBase", new Vector3(0f, - .1f, 0f), new Vector3(1f, .7f, 1f), k.Metal);
                Part("CookingPlate", new Vector3(0f, .3f, 0f), new Vector3(.98f, .12f, .95f), k.Dark);
                Part("SplashBack", new Vector3(0f, .42f, .45f), new Vector3(1f, .16f, .07f), k.Metal);
                foreach (float x in new []
                {
                    - .3f, 0f, .3f
                })
                {
                    Part("HeatDial", new Vector3(x, .05f, - .52f), new Vector3(.12f, .15f, .04f), k.Accent);
                }
            }
            else if (id == "motel_transformer")
            {
                Part("Core", Vector3.zero, new Vector3(.7f, .85f, .65f), k.Metal);
                for (int i = 0; i <9; i++)
                {
                    Part("CoolingFin", new Vector3(- .45f + i * .11f, 0f, 0f), new Vector3(.035f, .85f, .95f), k.Accent);
                }
                foreach (float x in new []
                {
                    - .25f, .25f
                })
                {
                    Part("CeramicTerminal", new Vector3(x, .42f, 0f), new Vector3(.1f, .15f, .15f), k.Wall);
                    Part("Foot", new Vector3(x, - .46f, 0f), new Vector3(.15f, .08f, 1f), k.Dark);
                }
            }
            else if (id == "motel_compressor" || id == "motel_pool_pump")
            {
                Part("PressureBody", new Vector3(0f, - .1f, 0f), new Vector3(.9f, .5f, .85f), k.Accent);
                Part("Motor", new Vector3(.15f, .27f, 0f), new Vector3(.55f, .35f, .6f), k.Metal);
                Part("Outlet", new Vector3(- .4f, .15f, - .25f), new Vector3(.15f, .2f, .2f), k.Metal);
                Part("Gauge", new Vector3(- .25f, .4f, - .1f), new Vector3(.15f, .15f, .1f), k.Wall);
                foreach (float x in new []
                {
                    - .35f, .35f
                })
                {
                    Part("Skid", new Vector3(x, - .43f, 0f), new Vector3(.15f, .12f, 1f), k.Dark);
                }
            }
            else if (id == "motel_milkshake")
            {
                Part("Base", new Vector3(0f, - .45f, 0f), new Vector3(1f, .1f, 1f), k.Accent);
                Part("Column", new Vector3(0f, 0f, .3f), new Vector3(.25f, .9f, .3f), k.Metal);
                Part("Head", new Vector3(0f, .4f, 0f), new Vector3(.65f, .16f, .8f), k.Accent);
                Part("Cup", new Vector3(0f, - .15f, - .15f), new Vector3(.55f, .45f, .55f), k.Metal);
            }
            else
            {
                Part("Urn", Vector3.zero, new Vector3(.8f, .85f, .8f), k.Metal);
                Part("Lid", new Vector3(0f, .45f, 0f), new Vector3(.9f, .08f, .9f), k.Dark);
                Part("Tap", new Vector3(0f, - .3f, - .47f), new Vector3(.15f, .1f, .15f), k.Accent);
                foreach (float x in new []
                {
                    - .45f, .45f
                })
                {
                    Part("Handle", new Vector3(x, .2f, 0f), new Vector3(.1f, .16f, .35f), k.Dark);
                }
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}

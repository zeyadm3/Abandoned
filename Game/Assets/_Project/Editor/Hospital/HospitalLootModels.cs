using Abandoned.Loot;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalLootModels
    {
        public static GameObject Build(HospitalKit k, LootDefinition d, int kind)
        {
            GameObject specialised = HospitalLargeLootModels.Build(k, d);
            if (specialised != null)
            {
                return specialised;
            }
            string folder = HospitalKit.Art + "/LootModels";
            HospitalKit.Ensure(folder);
            string path = folder + "/" + d.Id + ".prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }
            var go = new GameObject(d.Id);
            Transform p = go.transform;
            void Part(string n, Vector3 at, Vector3 size, Material mat) => k.Box(n, p, Vector3.Scale(at, d.Size), Vector3.Scale(size, d.Size), mat, false);
            switch (kind)
            {
                case 0 : Part("InstrumentTray", new(0, - .3f, 0), new(1, .4f, 1), k.Metal);
                for (int i = 0; i <4; i++)
                {
                    Part("Instrument", new(- .3f + i * .2f, 0, 0), new(.06f, .2f, .8f), k.Metal);
                }
                Part("Lid", new(0, .35f, 0), new(1, .1f, 1), k.Accent);
                break;
                case 1 : Part("Case", new(0, - .3f, 0), new(1, .4f, 1), k.Wood);
                for (int i = 0; i <3; i++)
                {
                    for (int j = 0; j <2; j++)
                    {
                        Part("Bottle", new(- .3f + i * .3f, .1f, - .25f + j * .5f), new(.18f, .65f, .3f), k.Glass);
                        Part("Stopper", new(- .3f + i * .3f, .46f, - .25f + j * .5f), new(.16f, .08f, .28f), k.Dark);
                    }
                }
                break;
                case 3 : Part("ArchiveBody", Vector3.zero, new(1, .95f, 1), k.Wood);
                for (int i = 0; i <4; i++)
                {
                    Part("Drawer", new(0, - .35f + i * .23f, - .5f), new(.9f, .18f, .02f), k.Accent);
                    Part("Pull", new(0, - .35f + i * .23f, - .53f), new(.25f, .04f, .05f), k.Metal);
                }
                break;
                case 5 : Part("Base", new(0, - .44f, 0), new(1, .12f, 1), k.Dark);
                Part("Arm", new(0, 0, .25f), new(.22f, .8f, .2f), k.Metal);
                Part("Stage", new(0, - .05f, 0), new(.8f, .08f, .7f), k.Dark);
                Part("Optics", new(0, .32f, - .05f), new(.25f, .3f, .5f), k.Metal);
                Part("Eyepiece", new(0, .46f, - .25f), new(.15f, .08f, .2f), k.Dark);
                break;
                case 6 : Part("Plinth", new(0, - .4f, 0), new(1, .2f, 1), k.Wood);
                Part("Column", new(0, 0, 0), new(.15f, .7f, .15f), k.Metal);
                Part("Dial", new(0, .2f, - .15f), new(.8f, .55f, .16f), k.Metal);
                Part("Face", new(0, .2f, - .25f), new(.65f, .4f, .03f), k.Cloth);
                Part("Needle", new(.1f, .2f, - .28f), new(.25f, .025f, .02f), k.Dark);
                break;
                default : Part("Cabinet", new(0, 0, 0), new(.9f, .75f, .85f), k.Accent);
                Part("Display", new(- .12f, .15f, - .44f), new(.5f, .3f, .03f), k.Glass);
                for (int i = 0; i <3; i++)
                {
                    Part("Knob", new(.28f, - .2f + i * .15f, - .46f), new(.07f, .07f, .06f), k.Dark);
                }
                Part("Handle", new(0, .44f, 0), new(.6f, .07f, .12f), k.Metal);
                if (kind == 4)
                {
                    Part("Mast", new(.3f, .2f, .2f), new(.12f, .6f, .12f), k.Metal);
                    foreach (float x in new []
                    {
                        - .35f, .35f
                    })
                    {
                        foreach (float z in new []
                        {
                            - .3f, .3f
                        })
                        {
                            Part("Wheel", new(x, - .44f, z), new(.15f, .12f, .15f), k.Dark);
                        }
                    }
                }
                break;
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}

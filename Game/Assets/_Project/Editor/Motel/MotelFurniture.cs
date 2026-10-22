using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Motel
{
    /// <summary>Fitted remnants have one solid footprint and section-bound visual details.</summary>
    public static class MotelFurniture
    {
        public static Transform Prop(Transform parent, MotelKit kit, MotelStructure floors, string name, Vector3 at, Vector3 size, float yaw = 0f)
        {
            Transform root = MotelKit.Group(name, parent);
            root.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.up *(size.y * .5f);
            solid.size = size;
            StructuralSection support = floors.At(at);
            if (support != null)
            {
                kit.Attach(root, support);
            }
            return root;
        }
        public static void Chair(Transform parent, MotelKit k, MotelStructure floors, Vector3 at, float yaw)
        {
            Transform t = Prop(parent, k, floors, "Chair", at, new Vector3(.55f, .9f, .55f), yaw);
            k.Box("Seat", t, new Vector3(0f, .45f, 0f), new Vector3(.5f, .08f, .5f), k.Cloth, false);
            k.Box("Back", t, new Vector3(0f, .7f, .23f), new Vector3(.5f, .45f, .05f), k.Wood, false);
            foreach (float x in new []
            {
                - .2f, .2f
            })
            {
                foreach (float z in new []
                {
                    - .2f, .2f
                })
                {
                    k.Box("Leg", t, new Vector3(x, .2f, z), new Vector3(.04f, .4f, .04f), k.Metal, false);
                }
            }
        }
        public static void Table(Transform parent, MotelKit k, MotelStructure floors, Vector3 at, float width, float depth)
        {
            Transform t = Prop(parent, k, floors, "Table", at, new Vector3(width, .8f, depth));
            k.Box("Top", t, new Vector3(0f, .77f, 0f), new Vector3(width, .06f, depth), k.Wood, false);
            foreach (float x in new []
            {
                - .4f, .4f
            })
            {
                k.Box("Leg", t, new Vector3(x * width, .38f, 0f), new Vector3(.06f, .76f, depth * .8f), k.Metal, false);
            }
        }
        public static void Shelf(Transform parent, MotelKit k, MotelStructure floors, Vector3 at, float width)
        {
            Transform t = Prop(parent, k, floors, "FixedShelving", at, new Vector3(width, 2f, .5f));
            foreach (float x in new []
            {
                - .48f, .48f
            })
            {
                k.Box("Post", t, new Vector3(x * width, 1f, 0f), new Vector3(.04f, 2f, .5f), k.Metal, false);
            }
            for (int row = 0; row <4; row++)
            {
                k.Box("Shelf", t, new Vector3(0f, .2f + row * .5f, 0f), new Vector3(width, .04f, .5f), k.Wood, false);
                for (int i = 0; i <3; i++)
                {
                    k.Box("FixedStock", t, new Vector3(- width * .3f + i * width * .3f, .36f + row * .5f, 0f), new Vector3(width * .2f, .28f, .4f),(i + row) % 2 == 0 ? k.Cloth : k.Accent, false);
                }
            }
        }
        public static void Machine(Transform parent, MotelKit k, MotelStructure floors, string name, Vector3 at, bool vending)
        {
            Transform t = Prop(parent, k, floors, name, at, new Vector3(1.3f, 1.9f, .9f));
            k.Box("Body", t, new Vector3(0f, .95f, 0f), new Vector3(1.3f, 1.9f, .9f), k.Accent, false);
            k.Box("Window", t, new Vector3(- .15f, 1.15f, - .46f), new Vector3(.8f, 1.1f, .03f), vending ? k.Glass : k.Metal, false);
            k.Box("DispenseHatch", t, new Vector3(0f, .4f, - .47f), new Vector3(.75f, .25f, .04f), k.Dark, false);
            for (int i = 0; i <5; i++)
            {
                k.Box("Button", t, new Vector3(.5f, .8f + i * .15f, - .48f), new Vector3(.1f, .06f, .03f), k.Metal, false);
            }
        }
    }
}

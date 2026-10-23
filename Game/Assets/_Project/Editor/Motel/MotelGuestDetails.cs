using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelGuestDetails
    {
        public static void Dress(Transform parent, MotelKit k, MotelStructure floors, int room, int level)
        {
            float x = MotelLayout.RoomX(room),
            y = level * 4f;
            int variant =(room + level) % 4;
            Transform g = MotelKit.Group("FittedRoomDetails", parent);
            // A fixed bedside fitting reads differently from the loose clocks and radios on loot tags.
            Transform stand = MotelFurniture.Prop(g, k, floors, "BoltedBedsideCabinet", new Vector3(x + .35f, y, 18.6f), new Vector3(.5f, .65f, .5f));
            k.Box("Cabinet", stand, new Vector3(0, .3f, 0), new Vector3(.5f, .6f, .5f), k.Wood, false);
            k.Box("Top", stand, new Vector3(0, .63f, 0), new Vector3(.54f, .06f, .54f), k.Metal, false);
            k.Box("DrawerSeam", stand, new Vector3(.26f, .38f, 0), new Vector3(.015f, .015f, .42f), k.Dark, false);
            Transform lamp = MotelKit.Group("WallReadingLight", g);
            lamp.position = new Vector3(x + .2f, y + 1.5f, 18.7f);
            k.Box("Mount", lamp, Vector3.zero, new Vector3(.08f, .32f, .28f), k.Metal, false);
            Renderer shade = k.Box("Shade", lamp, new Vector3(.12f, 0, 0), new Vector3(.24f, .25f, .24f), k.Glow, false).GetComponent<Renderer>();
            lamp.gameObject.AddComponent<LightFixture>().EditorSetup(null, shade, variant == 2, false, 17800 + level * 8 + room);
            k.Attach(lamp, floors.At(new Vector3(x + .2f, y, 18.7f)));
            if (variant == 1)
            {
                Workspace(g, k, floors, new Vector3(x + .55f, y, 22.6f));
            }
            Bathroom(g, k, floors, new Vector3(x, y, 24));
            foreach (float z in new []
            {
                25f, 27f
            })
            {
                k.Box("BathTileBand", g, new Vector3(x + .12f, y + 1.2f, z), new Vector3(.03f, .16f, 1.6f), k.Accent, false);
            }
            k.Box("BathSkirting", g, new Vector3(x + 3.5f, y + .08f, 27.86f), new Vector3(6.7f, .16f, .06f), k.Wood, false);
            // The original room silhouette stays intact; trim, fabrics and fitted hardware do the differentiation.
            foreach (float dx in new []
            {
                - .55f, .55f
            })
            {
                k.Box("CurtainStack", g, new Vector3(x + 1.2f + dx, y + 1.95f, 16.17f), new Vector3(.14f, 1.65f, .08f), variant == 0 ? k.Cloth : k.Accent, false);
            }
        }
        private static void Workspace(Transform parent, MotelKit k, MotelStructure f, Vector3 at)
        {
            Transform t = MotelFurniture.Prop(parent, k, f, "BuiltInWritingDesk", at, new Vector3(.65f, .8f, 1.6f));
            k.Box("Worktop", t, new Vector3(0, .77f, 0), new Vector3(.65f, .06f, 1.6f), k.Wood, false);
            k.Box("DrawerPedestal", t, new Vector3(0, .36f, .5f), new Vector3(.6f, .72f, .5f), k.Wood, false);
            foreach (float z in new []
            {
                - .65f, .65f
            })
            {
                k.Box("Bracket", t, new Vector3(0, .4f, z), new Vector3(.08f, .7f, .08f), k.Metal, false);
            }
            k.Box("CableTray", t, new Vector3(- .25f, .65f, 0), new Vector3(.06f, .1f, 1.4f), k.Dark, false);
        }
        private static void Bathroom(Transform parent, MotelKit k, MotelStructure f, Vector3 at)
        {
            Transform t = MotelKit.Group("BathHardware", parent);
            t.position = at;
            foreach (float z in new []
            {
                1.85f, 2.15f
            })
            {
                k.Box("SinkTap", t, new Vector3(5.9f, 1.16f, z), new Vector3(.06f, .12f, .06f), k.Metal, false);
            }
            k.Box("Spout", t, new Vector3(5.9f, 1.23f, 2f), new Vector3(.08f, .05f, .2f), k.Metal, false);
            k.Box("ShowerHead", t, new Vector3(.5f, 2.55f, 2.6f), new Vector3(.35f, .07f, .2f), k.Metal, false);
            k.Box("CurtainRod", t, new Vector3(2.25f, 2.6f, 2f), new Vector3(.04f, .04f, 2.3f), k.Metal, false);
            for (int i = 0; i <6; i++)
            {
                k.Box("GatheredShowerCurtain", t, new Vector3(2.2f, 1.6f, 2.6f + i * .07f), new Vector3(.04f, 1.85f, .06f), k.Cloth, false);
            }
            k.Attach(t, f.At(at));
        }
    }
}

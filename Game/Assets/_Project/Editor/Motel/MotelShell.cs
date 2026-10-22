using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelShell
    {
        public static void Build(Transform root, MotelKit k, MotelStructure floors)
        {
            Transform shell = MotelKit.Group("Shell", root);
            for (int level = 0; level <2; level++)
            {
                float y = level * 4;
                for (int room = 0; room <8; room++)
                {
                    float x = MotelLayout.RoomX(room);
                    Transform front = MotelKit.Group("RoomFront", shell);
                    front.localPosition = new Vector3(x + 3.5f, y, 16);
                    Front(front, k);
                    k.Label(front, MotelLayout.RoomNumber(level, room).ToString(), new Vector3(2.1f, 2.6f, - .16f), size : .045f);
                    Transform bath = MotelKit.Group("BathroomPartition", shell);
                    bath.localPosition = new Vector3(x + 3.5f, y, 24);
                    DoorWall(bath, k, 7, 2.2f);
                    k.Box("RearLower", shell, new Vector3(x + 3.5f, y + .75f, 28), new Vector3(7, 1.5f, .2f), k.Wall);
                    k.Box("RearUpper", shell, new Vector3(x + 3.5f, y + 3.12f, 28), new Vector3(7, 1.04f, .2f), k.Wall);
                    k.Box("FrostedWindow", shell, new Vector3(x + 3.5f, y + 2.05f, 28), new Vector3(7, 1.1f, .08f), k.Glass);
                    foreach (float edge in new []
                    {
                        - .1f, 3.5f, 7.1f
                    })
                    {
                        k.Box("WindowMullion", shell, new Vector3(x + edge, y + 2.05f, 28), new Vector3(.08f, 1.1f, .24f), k.Metal, false);
                    }
                    k.Box("FrontSill", front, new Vector3(- 2.3f, 1.15f, - .18f), new Vector3(2.2f, .08f, .2f), k.Wood, false);
                }
                foreach (float x in new []
                {
                    0f, 7f, 14f, 21f, 28f, 36f, 43f, 50f, 57f, 64f
                })
                {
                    k.Box("PartyWall", shell, new Vector3(x, y + 1.82f, 22), new Vector3(.18f, 3.64f, 12), k.Wall);
                }
                for (int x = 0; x <16; x++)
                {
                    for (int z = 4; z <7; z++)
                    {
                        Transform panel = MotelKit.Group("CeilingPanel", shell);
                        panel.localPosition = new Vector3(x * 4 + 2, y + 3.62f, z * 4 + 2);
                        k.Box("Panel", panel, Vector3.zero, new Vector3(3.96f, .08f, 3.96f), k.Wall, false);
                        if (level == 0)
                        {
                            k.Attach(panel, floors.At(new Vector3(x * 4 + 2, 4, z * 4 + 2)));
                        }
                    }
                }
            }
            Transform roof = MotelKit.Group("Roof", root);
            foreach (float x in new []
            {
                14f, 50f
            })
            {
                k.Box("WingRoof", roof, new Vector3(x, 7.92f, 22), new Vector3(28, .2f, 12), k.Dark);
                k.Box("Parapet", roof, new Vector3(x, 8.2f, 28), new Vector3(28, .5f, .2f), k.Accent);
                for (int i = 0; i <4; i++)
                {
                    k.Box("Chimney", roof, new Vector3(x - 10 + i * 7, 8.35f, 25), new Vector3(.5f, .7f, .5f), k.Metal);
                }
            }
            k.Box("GalleryCanopy", roof, new Vector3(32, 7.5f, 14), new Vector3(64, .16f, 4), k.Accent);
            k.Box("ManagerRoof", roof, new Vector3(32, 7.92f, 22), new Vector3(8, .2f, 12), k.Dark);
            Transform services = MotelKit.Group("ServiceShell", shell);
            Transform lobby = MotelKit.Group("ReceptionFront", services);
            lobby.localPosition = new Vector3(32, 0, 16);
            DoorWall(lobby, k, 8, 3.2f);
            Transform partition = MotelKit.Group("LaundryDoor", services);
            partition.localPosition = new Vector3(32, 0, 22);
            DoorWall(partition, k, 8, 2.4f);
            Transform rear = MotelKit.Group("LaundryExit", services);
            rear.localPosition = new Vector3(32, 0, 28);
            DoorWall(rear, k, 8, 2.4f);
            Transform flatFront = MotelKit.Group("ManagerEntrance", shell);
            flatFront.localPosition = new Vector3(32, 4, 16);
            DoorWall(flatFront, k, 8, 2.4f);
            k.Box("ManagerRearWall", shell, new Vector3(32, 5.82f, 28), new Vector3(8, 3.64f, .2f), k.Wall);
            Abandoned.EditorTools.GreyboxFactory.MarkStatic(shell);
            Abandoned.EditorTools.GreyboxFactory.MarkStatic(roof);
            k.Label(lobby, "OFFICE", new Vector3(0, 3.1f, - .14f), size : .045f);
            k.Label(partition, "LAUNDRY", new Vector3(2.6f, 2.6f, - .14f), size : .03f);
        }
        public static void DoorWall(Transform p, MotelKit k, float width, float gap)
        {
            float side =(width - gap) * .5f;
            foreach (float s in new []
            {
                - 1f, 1f
            })
            {
                k.Box("Jamb", p, new Vector3(s *(gap + side) * .5f, 1.82f, 0), new Vector3(side, 3.64f, .18f), k.Wall);
            }
            k.Box("Header", p, new Vector3(0, 3.22f, 0), new Vector3(gap, .84f, .18f), k.Wall);
        }
        private static void Front(Transform p, MotelKit k)
        {
            foreach (float s in new []
            {
                - 1f, 1f
            })
            {
                Vector3 at = new(s * 2.3f, 0, 0);
                k.Box("SillWall", p, at + new Vector3(0, .55f, 0), new Vector3(2.4f, 1.1f, .2f), k.Wall);
                k.Box("Window", p, at + new Vector3(0, 1.9f, 0), new Vector3(2.4f, 1.6f, .08f), k.Glass);
                k.Box("HeaderWall", p, at + new Vector3(0, 3.17f, 0), new Vector3(2.4f, .94f, .2f), k.Wall);
                k.Box("DoorJamb", p, new Vector3(s * 1.12f, 1.35f, - .02f), new Vector3(.06f, 2.7f, .26f), k.Metal, false);
            }
            k.Box("DoorHeader", p, new Vector3(0, 3.17f, 0), new Vector3(2.2f, .94f, .2f), k.Wall);
        }
    }
}

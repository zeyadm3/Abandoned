using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelGuestProps
    {
        public static void Dress(Transform parent, MotelKit k, MotelStructure floors, int room, int level)
        {
            float x = MotelLayout.RoomX(room);
            float y = level * 4f;
            int variant =(room + level) % 4;
            if (variant == 0)
            {
                Bed(parent, k, floors, new Vector3(x + 1.2f, y, 20f), .9f, false);
                Bed(parent, k, floors, new Vector3(x + 3f, y, 20f), .9f, true);
            }
            else
            {
                Bed(parent, k, floors, new Vector3(x + 1.5f, y, 20f), 1.7f, variant == 2);
            }
            Vanity(parent, k, floors, new Vector3(x + 5f, y, 23f));
            Bathroom(parent, k, floors, new Vector3(x, y, 24f));
            if (variant == 1)
            {
                MotelFurniture.Chair(parent, k, floors, new Vector3(x + 1.6f, y, 22.7f), 90f);
                WallArt(parent, k, new Vector3(x + .2f, y + 1.7f, 22.6f), true);
            }
            if (variant == 2)
            {
                Transform cot = MotelFurniture.Prop(parent, k, floors, "ChildCot", new Vector3(x + 3.3f, y, 22.5f), new Vector3(.7f, .9f, 1.1f));
                k.Box("Frame", cot, new Vector3(0f, .3f, 0f), new Vector3(.7f, .1f, 1.1f), k.Wood, false);
                k.Box("Pad", cot, new Vector3(0f, .42f, 0f), new Vector3(.65f, .12f, 1f), k.Cloth, false);
                foreach (float side in new []
                {
                    - .35f, .35f
                })
                {
                    k.Box("Side", cot, new Vector3(side, .65f, 0f), new Vector3(.03f, .45f, 1.1f), k.Metal, false);
                }
            }
            if (variant == 3)
            {
                Transform couch = MotelFurniture.Prop(parent, k, floors, "GuestSofa", new Vector3(x + 1.5f, y, 23f), new Vector3(1.8f, .8f, .8f));
                k.Box("Seat", couch, new Vector3(0f, .35f, 0f), new Vector3(1.8f, .5f, .8f), k.Cloth, false);
                k.Box("Back", couch, new Vector3(0f, .65f, .32f), new Vector3(1.8f, .4f, .15f), k.Accent, false);
            }
            WallArt(parent, k, new Vector3(x + 2.1f, y + 2f, 16.15f), false);
            MotelGuestDetails.Dress(parent, k, floors, room, level);
        }
        private static void Bed(Transform p, MotelKit k, MotelStructure f, Vector3 at, float width, bool bare)
        {
            Transform t = MotelFurniture.Prop(p, k, f, bare ? "UnmadeBed" : "GuestBed", at, new Vector3(width, .8f, 2.2f));
            k.Box("Base", t, new Vector3(0f, .3f, 0f), new Vector3(width, .6f, 2.2f), k.Wood, false);
            k.Box("Mattress", t, new Vector3(0f, .7f, 0f), new Vector3(width - .05f, .2f, 2.1f), k.Cloth, false);
            k.Box("Headboard", t, new Vector3(0f, 1.05f, 1.05f), new Vector3(width + .1f, 1.2f, .12f), k.Wood, false);
            k.Box("Pillow", t, new Vector3(0f, .86f, .7f), new Vector3(width * .65f, .12f, .4f), k.Cloth, false);
            if (! bare)
            {
                k.Box("Coverlet", t, new Vector3(0f, .82f, - .4f), new Vector3(width, .04f, 1.1f), k.Accent, false);
            }
        }
        private static void Vanity(Transform p, MotelKit k, MotelStructure f, Vector3 at)
        {
            Transform t = MotelFurniture.Prop(p, k, f, "FittedVanity", at, new Vector3(1.8f, 1.3f, .7f));
            k.Box("Cabinet", t, new Vector3(0f, .65f, 0f), new Vector3(1.8f, 1.3f, .7f), k.Wood, false);
            k.Box("MirrorFrame", t, new Vector3(0f, 1.9f, .37f), new Vector3(1.5f, 1f, .08f), k.Metal, false);
            k.Box("Mirror", t, new Vector3(0f, 1.9f, .315f), new Vector3(1.35f, .86f, .025f), k.Glass, false);
            k.Box("AirConditioner", t, new Vector3(0f, 2.95f, .35f), new Vector3(1.4f, .35f, .35f), k.Accent, false);
            for (int i = 0; i <7; i++)
            {
                k.Box("VentSlat", t, new Vector3(- .55f + i * .18f, 2.94f, .16f), new Vector3(.06f, .2f, .04f), k.Metal, false);
            }
        }
        private static void Bathroom(Transform p, MotelKit k, MotelStructure f, Vector3 at)
        {
            Transform t = MotelKit.Group("BathroomFittings", p);
            t.position = at;
            k.Box("Tub", t, new Vector3(1.3f, .25f, 2f), new Vector3(1.8f, .5f, 2.2f), k.Wall);
            k.Box("TubInset", t, new Vector3(1.3f, .51f, 2f), new Vector3(1.5f, .03f, 1.85f), k.Glass, false);
            k.Box("Sink", t, new Vector3(5.9f, .5f, 2f), new Vector3(.65f, 1f, .6f), k.Wall);
            k.Box("Basin", t, new Vector3(5.9f, 1.05f, 2f), new Vector3(.85f, .12f, .7f), k.Metal, false);
            k.Box("ToiletBowl", t, new Vector3(5.8f, .25f, 3.3f), new Vector3(.45f, .5f, .65f), k.Wall);
            k.Box("Cistern", t, new Vector3(5.8f, .65f, 3.6f), new Vector3(.45f, .4f, .2f), k.Wall);
            k.Box("ShowerPipe", t, new Vector3(.35f, 1.6f, 2.6f), new Vector3(.05f, 1.9f, .05f), k.Metal, false);
            k.Box("TowelRail", t, new Vector3(3.5f, 1.4f, 3.8f), new Vector3(1.2f, .04f, .06f), k.Metal, false);
            k.Attach(t, f.At(at));
        }
        private static void WallArt(Transform p, MotelKit k, Vector3 at, bool side)
        {
            Transform t = MotelKit.Group("BoltedWallPicture", p);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, side ? 90f : 0f, 0f));
            k.Box("Frame", t, Vector3.zero, new Vector3(1.1f, .7f, .04f), k.Wood, false);
            k.Box("Canvas", t, new Vector3(0f, 0f, .025f), new Vector3(1f, .6f, .01f), k.Cloth, false);
            for (int i = 0; i <5; i++)
            {
                k.Box("Landscape", t, new Vector3(- .4f + i * .2f, - .12f + i % 2 * .1f, .035f), new Vector3(.2f, .2f, .01f), k.Accent, false);
            }
        }
    }
}

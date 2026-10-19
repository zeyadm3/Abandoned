using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Mechanical detail is merged into its existing fixed frame or moving leaf, so doors still move as one.</summary>
    public static class IslandCellHardware
    {
        public static void Front(Transform r, IslandPrisonArt art)
        {
            const float h = IslandCellKits.FrontHeight;
            Box("TrackUpperLip", r, new Vector3(0f, h + .267f, .1f), new Vector3(1.98f, .035f, .13f), art.Rust, false);
            Box("TrackLowerLip", r, new Vector3(0f, h + .025f, .14f), new Vector3(1.98f, .026f, .08f), art.Bars, false);
            Box("LockingRod", r, new Vector3(0f, h + .145f, .152f), new Vector3(1.94f, .035f, .025f), art.Brass, false);
            Box("DoorRunTrack", r, new Vector3(0f, 2.435f, .105f), new Vector3(1.98f, .08f, .08f), art.Steel, false);
            foreach (float x in new[] { -.86f, -.32f, .28f, .88f })
            {
                Box("TrackSaddle", r, new Vector3(x, h + .14f, .169f), new Vector3(.075f, .12f, .035f), art.Steel, false);
                foreach (float y in new[] { h + .1f, h + .18f })
                    Rivet(r, new Vector3(x, y, .194f), .021f, art.Rust);
            }
            foreach (float x in new[] { -.97f, .97f })
            foreach (float y in new[] { .26f, 1.02f, 2.28f, 3.15f })
                Rivet(r, new Vector3(x, y, .062f), .028f, art.Steel);
            foreach (float x in new[] { -.74f, -.45f, -.16f })
            foreach (float y in new[] { 1.05f, IslandCellKits.DoorHeight })
                Box("BarCollar", r, new Vector3(x, y, .025f), new Vector3(.055f, .07f, .034f), art.Rust, false);
            Box("NumberFrame", r, new Vector3(.55f, h + .14f, .146f), new Vector3(.27f, .16f, .018f), art.Bars, false);
            foreach (float x in new[] { .44f, .66f })
                Rivet(r, new Vector3(x, h + .14f, .168f), .014f, art.Steel);
        }

        public static void Door(Transform r, IslandPrisonArt art, bool solid)
        {
            float w = IslandCellKits.DoorWidth;
            Box("LockKeeper", r, new Vector3(w - .095f, 1.045f, .088f), new Vector3(.16f, .22f, .027f), art.Rust, false);
            Rivet(r, new Vector3(w - .095f, .97f, .107f), .022f, art.Brass);
            Rivet(r, new Vector3(w - .095f, 1.12f, .107f), .022f, art.Brass);
            Box("Keyway", r, new Vector3(w - .095f, 1.045f, .109f), new Vector3(.018f, .035f, .007f), art.Bars, false);
            foreach (float y in new[] { 1.27f, 1.43f })
                Box("PullMount", r, new Vector3(w - .15f, y, .074f), new Vector3(.035f, .035f, .09f), art.Steel, false);
            Box("DoorPull", r, new Vector3(w - .15f, 1.35f, .117f), new Vector3(.028f, .18f, .028f), art.Steel, false);
            if (solid)
            {
                Box("KickPlate", r, new Vector3(w * .5f, .3f, .041f), new Vector3(w - .15f, .36f, .012f), art.Rust, false);
                foreach (float x in new[] { .12f, w - .12f })
                foreach (float y in new[] { .15f, .45f })
                    Rivet(r, new Vector3(x, y, .053f), .024f, art.Steel);
                foreach (float y in new[] { .79f, 1.01f })
                    Box("FoodHatchRim", r, new Vector3(w * .5f, y, .06f), new Vector3(.42f, .025f, .034f), art.Rust, false);
                foreach (float x in new[] { w * .5f - .2f, w * .5f + .2f })
                    Box("FoodHatchRim", r, new Vector3(x, .9f, .06f), new Vector3(.025f, .2f, .034f), art.Rust, false);
                Box("HatchHinge", r, new Vector3(w * .5f, .79f, .088f), new Vector3(.3f, .04f, .04f), art.Steel, false);
                Box("ViewingSlotShutter", r, new Vector3(w * .5f, 1.675f, .052f), new Vector3(.35f, .027f, .04f), art.Steel, false);
                foreach (float x in new[] { .28f, .62f })
                    Rivet(r, new Vector3(x, 1.61f, .058f), .017f, art.Brass);
                return;
            }
            foreach (float x in new[] { .16f, .67f })
            {
                Box("RollerHanger", r, new Vector3(x, 2.345f, .015f), new Vector3(.06f, .18f, .05f), art.Steel, false);
                Primitive(PrimitiveType.Cylinder, "DoorRoller", r, new Vector3(x, 2.425f, .035f), new Vector3(.095f, .024f, .095f), art.Rust, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Rivet(r, new Vector3(x, 2.425f, .065f), .028f, art.Steel);
            }
            foreach (float x in new[] { .035f, w - .035f })
            foreach (float y in new[] { .15f, 1.05f, 2.2f })
                Rivet(r, new Vector3(x, y, .042f), .025f, art.Steel);
        }

        public static void SolitaryRoom(Transform r, IslandPrisonArt art, float depth)
        {
            foreach (float x in new[] { -.922f, .922f })
            {
                Box("SolitarySideDado", r, new Vector3(x, .61f, depth * .5f), new Vector3(.009f, 1.18f, depth - .18f), art.Dado, false);
                Box("SolitaryPaintLine", r, new Vector3(x + (x < 0f ? .008f : -.008f), 1.217f, depth * .5f),
                    new Vector3(.005f, .025f, depth - .18f), art.Facade, false);
            }
            foreach (float x in new[] { -.91f, -.2f })
                Box("PlinthEdgeWear", r, new Vector3(x, .407f, depth * .5f), new Vector3(.035f, .014f, depth - .38f), art.Facade, false);
            for (int i = 0; i < 6; i++)
                Box("DrainGrate", r, new Vector3(.55f, .013f, .297f + i * .041f), new Vector3(.23f, .006f, .015f), art.Steel, false);
            Box("VentRecess", r, new Vector3(.34f, 2.79f, .04f), new Vector3(.54f, .22f, .035f), art.Bars, false);
            for (int i = 0; i < 5; i++)
                Box("VentSlot", r, new Vector3(.34f, 2.7f + i * .045f, .065f), new Vector3(.49f, .017f, .025f), art.Steel, false);
            Box("WallPlate", r, new Vector3(-.56f, 1.89f, .025f), new Vector3(.23f, .3f, .02f), art.FacadeDark, false);
            foreach (float y in new[] { 1.78f, 2f })
                Rivet(r, new Vector3(-.56f, y, .044f), .023f, art.Rust);
            for (int group = 0; group < 3; group++)
            for (int i = 0; i < 4; i++)
                Box("SolitaryTally", r, new Vector3(-.7f + group * .16f + i * .025f, 1.38f, .021f), new Vector3(.005f, .095f, .004f), art.Stain, false);
            IslandCellFurnishings.Pipe("SolitaryConduit", r, new Vector3(.8f, 2.2f, .045f), new Vector3(.8f, 3.44f, .045f), .022f, art.Steel);
            Box("ConduitJunction", r, new Vector3(.8f, 2.26f, .05f), new Vector3(.09f, .12f, .065f), art.Steel, false);
            foreach (float y in new[] { 2.58f, 3.2f })
                Box("ConduitClip", r, new Vector3(.8f, y, .064f), new Vector3(.05f, .025f, .025f), art.Rust, false);
        }

        private static void Rivet(Transform r, Vector3 at, float size, Material material) =>
            Box("Rivet", r, at, new Vector3(size, size, size * .42f), material, false);
    }
}

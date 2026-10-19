using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.WorkerVisualMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted workwear construction; all geometry is cosmetic and follows the existing worker joints.</summary>
    public static class WorkerClothingDetails
    {
        private static Material M(string key) => CharacterSurfaceLibrary.Get(key);

        public static void Hips(Transform root)
        {
            F.Form("CoverallSeat", root, Vector3.zero, new Vector3(.37f, .25f, .27f), M("CrewFabric"));
            Box("WovenBelt", root, new Vector3(0f, .075f, .006f), new Vector3(.382f, .048f, .282f), M("Leather"), false);
            Box("BuckleFrame", root, new Vector3(0f, .075f, .155f), new Vector3(.073f, .05f, .018f), M("Steel"), false);
            Box("BuckleInset", root, new Vector3(0f, .075f, .166f), new Vector3(.044f, .026f, .009f), M("Rubber"), false);
            for (int i = -2; i <= 2; i++)
                Box("BeltLoop", root, new Vector3(i * .064f, .074f, .153f), new Vector3(.012f, .067f, .012f), M("CrewFabric"), false);
            for (int side = -1; side <= 1; side += 2)
            {
                F.Form("FittedUtilityPocket", root, new Vector3(side * .194f, -.015f, -.014f), new Vector3(.07f, .18f, .15f), M("Canvas"), "Pad");
                Box("PocketFlap", root, new Vector3(side * .22f, .038f, -.014f), new Vector3(.017f, .047f, .153f), M("Leather"), false);
                F.Form("PocketSnap", root, new Vector3(side * .232f, .035f, -.014f), new Vector3(.009f, .015f, .015f), M("Steel"), "Tube");
                for (int i = 0; i < 3; i++)
                    Box("WebbingLoop", root, new Vector3(side * .226f, -.052f, -.052f + i * .035f), new Vector3(.012f, .045f, .012f), M("Rubber"), false);
            }
            Box("RearReinforcement", root, new Vector3(0f, -.018f, -.137f), new Vector3(.29f, .12f, .012f), M("CrewTrim"), false);
            F.Merge(root, "HipsV3");
        }

        public static void Torso(Transform root)
        {
            F.Form("TailoredCoverall", root, new Vector3(0f, .17f, 0f), new Vector3(.48f, .49f, .32f), M("CrewFabric"), "Jacket");
            Box("ShoulderYoke", root, new Vector3(0f, .32f, -.145f), new Vector3(.39f, .095f, .018f), M("CrewTrim"), false);
            Box("BackActionPleat", root, new Vector3(0f, .17f, -.16f), new Vector3(.026f, .32f, .018f), M("CrewFabric"), false);
            Box("ZipPlacket", root, new Vector3(0f, .14f, .158f), new Vector3(.039f, .34f, .02f), M("CrewTrim"), false);
            Box("ZipTeeth", root, new Vector3(0f, .14f, .172f), new Vector3(.009f, .315f, .009f), M("Steel"), false);
            for (int i = 0; i < 18; i++)
                Box("ZipStitch", root, new Vector3(.013f, -.01f + i * .017f, .173f), new Vector3(.003f, .008f, .003f), M("Canvas"), false);
            F.Form("ZipPull", root, new Vector3(0f, .28f, .183f), new Vector3(.023f, .041f, .008f), M("Steel"), "Pad");
            for (int side = -1; side <= 1; side += 2)
            {
                F.Form("CollarFold", root, new Vector3(side * .075f, .38f, .055f), new Vector3(.12f, .12f, .135f), M("CrewTrim"), "Pad")
                    .transform.localRotation = Quaternion.Euler(8f, 0f, side * -23f);
                F.Form("ChestPocket", root, new Vector3(side * .13f, .225f, .158f), new Vector3(.12f, .13f, .028f), M("CrewFabric"), "Pad");
                Box("PocketFlap", root, new Vector3(side * .13f, .27f, .179f), new Vector3(.12f, .035f, .012f), M("CrewTrim"), false);
                Box("PocketStitch", root, new Vector3(side * .13f, .166f, .178f), new Vector3(.102f, .004f, .004f), M("Canvas"), false);
                F.Form("PocketSnap", root, new Vector3(side * .13f, .265f, .189f), new Vector3(.013f, .013f, .006f), M("Steel"), "Tube");
                Strap(root, new Vector3(side * .18f, .355f, .144f), new Vector3(side * .13f, -.055f, .165f), .037f);
                Strap(root, new Vector3(side * .18f, .355f, -.14f), new Vector3(side * .11f, -.045f, -.147f), .037f);
                Box("HarnessAdjuster", root, new Vector3(side * .16f, .15f, .188f), new Vector3(.052f, .048f, .012f), M("Steel"), false);
                Box("AdjusterWebbing", root, new Vector3(side * .16f, .15f, .198f), new Vector3(.028f, .032f, .008f), M("Rubber"), false);
                Box("HighVisibilityTape", root, new Vector3(side * .205f, .30f, .136f), new Vector3(.037f, .15f, .015f), M("Tape"), false);
                for (int i = 0; i < 4; i++)
                    Box("WaistClothFold", root, new Vector3(side * (.095f + i * .027f), -.025f + i % 2 * .014f, .14f), new Vector3(.035f, .008f, .012f), M("CrewFabric"), false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, side * (8f + i * 6f));
            }
            Box("SternumStrap", root, new Vector3(0f, .11f, .189f), new Vector3(.28f, .025f, .012f), M("Leather"), false);
            Box("SternumClip", root, new Vector3(.015f, .11f, .201f), new Vector3(.048f, .033f, .018f), M("Rubber"), false);
            Box("UpperBackTape", root, new Vector3(0f, .29f, -.164f), new Vector3(.28f, .032f, .013f), M("Tape"), false);
            Box("CrewPatchBacking", root, new Vector3(.085f, .22f, .204f), new Vector3(.08f, .062f, .012f), M("Rubber"), false);
            Box("PatchBorderTop", root, new Vector3(.085f, .25f, .213f), new Vector3(.075f, .003f, .003f), M("Canvas"), false);
            Box("PatchBorderBottom", root, new Vector3(.085f, .19f, .213f), new Vector3(.075f, .003f, .003f), M("Canvas"), false);
            F.Merge(root, "TorsoV3");
            WorkerVisualBuilder.NumberBadge(root, new Vector3(.085f, .22f, .216f));
        }

        private static void Strap(Transform root, Vector3 a, Vector3 b, float width)
        {
            Vector3 d = b - a;
            GameObject strap = Box("HarnessWebbing", root, (a + b) * .5f, new Vector3(width, d.magnitude, .012f), M("Leather"), false);
            strap.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        public static void Arm(Transform root, float side)
        {
            F.Form("Sleeve", root, new Vector3(0f, -.235f, 0f), new Vector3(.16f, .49f, .18f), M("CrewFabric"));
            F.Form("ReinforcedElbow", root, new Vector3(0f, -.255f, -.087f), new Vector3(.12f, .15f, .026f), M("CrewTrim"), "Pad");
            Box("OuterSleeveSeam", root, new Vector3(side * .073f, -.23f, .005f), new Vector3(.007f, .39f, .008f), M("CrewTrim"), false);
            for (int i = 0; i < 5; i++)
                F.Form("SleeveFold", root, new Vector3(0f, -.315f - i * .022f, .005f), new Vector3(.15f - i * .004f, .012f, .16f - i * .005f), M("CrewFabric"), "Tube")
                    .transform.localRotation = Quaternion.Euler(0f, 0f, side * (i % 2 == 0 ? 5f : -5f));
            F.Form("ReflectiveCuff", root, new Vector3(0f, -.415f, 0f), new Vector3(.147f, .029f, .158f), M("Tape"), "Tube");
            F.Form("KnittedCuff", root, new Vector3(0f, -.459f, 0f), new Vector3(.125f, .065f, .13f), M("Rubber"), "Tube");
            Glove(root, new Vector3(0f, -.475f, .012f), Quaternion.identity, 1f, side);
            F.Merge(root, side < 0f ? "ArmLV3" : "ArmRV3");
        }

        public static void Glove(Transform root, Vector3 at, Quaternion rotation, float scale, float side)
        {
            Transform glove = Group("WorkGlove", root);
            glove.localPosition = at; glove.localRotation = rotation; glove.localScale = Vector3.one * scale;
            F.Form("LeatherPalm", glove, new Vector3(0f, -.055f, .009f), new Vector3(.115f, .115f, .092f), M("Leather"), "Pad");
            F.Form("RubberBack", glove, new Vector3(0f, -.05f, -.036f), new Vector3(.112f, .09f, .027f), M("Rubber"), "Pad");
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * .025f, length = i is 0 or 3 ? .065f : .078f;
                F.Form("Finger", glove, new Vector3(x, -.112f - length * .2f, .023f), new Vector3(.027f, length, .042f), M("Leather"), "Tube");
                F.Form("FingerGuard", glove, new Vector3(x, -.105f, -.004f), new Vector3(.026f, .027f, .025f), M("Rubber"), "Pad");
                Box("FingerStitch", glove, new Vector3(x, -.132f, .046f), new Vector3(.003f, .032f, .003f), M("Canvas"), false);
            }
            F.Form("Thumb", glove, new Vector3(side * .064f, -.065f, .041f), new Vector3(.041f, .075f, .045f), M("Leather"), "Tube")
                .transform.localRotation = Quaternion.Euler(-25f, 0f, side * -28f);
            Box("WristStrap", glove, new Vector3(0f, -.005f, -.052f), new Vector3(.115f, .025f, .012f), M("CrewTrim"), false);
            Box("WristFastener", glove, new Vector3(side * .035f, -.005f, -.061f), new Vector3(.032f, .023f, .011f), M("Rubber"), false);
            for (int i = 0; i < 3; i++)
                F.Form("KnuckleGuard", glove, new Vector3((i - 1) * .03f, -.068f, -.053f), new Vector3(.027f, .029f, .021f), M("Rubber"), "Pad");
        }

        public static void Thigh(Transform root, float side)
        {
            F.Form("TailoredTrouser", root, new Vector3(0f, -.18f, 0f), new Vector3(.195f, .37f, .22f), M("CrewFabric"));
            Box("TrouserSeam", root, new Vector3(side * .094f, -.18f, .004f), new Vector3(.007f, .29f, .009f), M("CrewTrim"), false);
            F.Form("CargoPocket", root, new Vector3(side * .099f, -.17f, .016f), new Vector3(.04f, .16f, .15f), M("CrewFabric"), "Pad");
            Box("CargoFlap", root, new Vector3(side * .12f, -.10f, .016f), new Vector3(.015f, .039f, .155f), M("CrewTrim"), false);
            F.Form("CargoSnap", root, new Vector3(side * .131f, -.101f, .016f), new Vector3(.009f, .012f, .012f), M("Steel"), "Tube");
            F.Form("KneeGuard", root, new Vector3(0f, -.315f, .10f), new Vector3(.14f, .15f, .038f), M("Rubber"), "Pad");
            for (int i = 0; i < 3; i++)
                Box("KneeFlexRib", root, new Vector3(0f, -.27f - i * .03f, .123f), new Vector3(.105f, .008f, .014f), M("CrewTrim"), false);
            F.Merge(root, side < 0f ? "ThighLV3" : "ThighRV3");
        }

        public static void Shin(Transform root, float side)
        {
            F.Form("TrouserShin", root, new Vector3(0f, -.19f, 0f), new Vector3(.18f, .39f, .20f), M("CrewFabric"));
            Box("ShinSeam", root, new Vector3(side * .082f, -.17f, .005f), new Vector3(.006f, .28f, .007f), M("CrewTrim"), false);
            F.Form("AnkleTape", root, new Vector3(0f, -.255f, .001f), new Vector3(.183f, .035f, .202f), M("Tape"), "Tube");
            F.Form("BootShaft", root, new Vector3(0f, -.355f, 0f), new Vector3(.173f, .22f, .19f), M("Leather"), "Cloth");
            F.Form("FormedBoot", root, new Vector3(0f, -.42f, .055f), new Vector3(.205f, .17f, .32f), M("Leather"), "Boot");
            F.Form("ProtectiveToe", root, new Vector3(0f, -.429f, .15f), new Vector3(.209f, .105f, .145f), M("Rubber"), "Pad");
            F.Form("BootWelt", root, new Vector3(0f, -.477f, .055f), new Vector3(.214f, .025f, .325f), M("CrewTrim"), "Pad");
            F.Form("LayeredOutsole", root, new Vector3(0f, -.5f, .055f), new Vector3(.22f, .035f, .335f), M("Rubber"), "Pad");
            for (int i = 0; i < 6; i++)
            {
                Box("SoleLug", root, new Vector3(0f, -.517f, -.074f + i * .05f), new Vector3(.19f, .018f, .023f), M("Rubber"), false);
                for (int s = -1; s <= 1; s += 2)
                    F.Form("LaceEyelet", root, new Vector3(s * .045f, -.329f - i * .018f, .09f + i * .006f), new Vector3(.012f, .012f, .008f), M("Steel"), "Tube");
                F.Beam("CrossLace", root, new Vector3(-.044f, -.326f - i * .018f, .098f + i * .006f),
                    new Vector3(.044f, -.343f - i * .018f, .098f + i * .006f), .006f, M("Canvas"));
            }
            Box("BootPullTab", root, new Vector3(0f, -.285f, -.096f), new Vector3(.036f, .07f, .015f), M("Canvas"), false);
            F.Merge(root, side < 0f ? "ShinLV3" : "ShinRV3");
        }
    }
}

using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.WorkerVisualMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>Human proportions and subdued facial detail, leaving the crown and face available for cosmetics.</summary>
    public static class WorkerHeadDetails
    {
        public static void Build(Transform root)
        {
            Material skin = CharacterSurfaceLibrary.Get("Skin"), dark = CharacterSurfaceLibrary.Get("Rubber");
            Material fabric = CharacterSurfaceLibrary.Get("CrewTrim"), trim = CharacterSurfaceLibrary.Get("Canvas");
            F.Form("Neck", root, new Vector3(0f, -.17f, -.015f), new Vector3(.14f, .18f, .14f), skin, "Tube");
            F.Form("FoldedNeckGaiter", root, new Vector3(0f, -.18f, -.015f), new Vector3(.17f, .085f, .17f), fabric, "Tube");
            for (int i = 0; i < 3; i++)
                F.Form("GaiterFold", root, new Vector3(0f, -.155f - i * .019f, -.015f), new Vector3(.173f, .008f, .172f), trim, "Tube");
            F.Form("SculptedHead", root, Vector3.zero, new Vector3(.265f, .32f, .255f), skin, "Head");
            F.Form("Jaw", root, new Vector3(0f, -.09f, .03f), new Vector3(.20f, .105f, .18f), skin, "Pad");
            F.Form("NoseBridge", root, new Vector3(0f, -.004f, .123f), new Vector3(.041f, .084f, .051f), skin, "Pad");
            F.Form("NoseTip", root, new Vector3(0f, -.033f, .148f), new Vector3(.052f, .031f, .032f), skin, "Pad");
            Box("MouthShadow", root, new Vector3(0f, -.084f, .124f), new Vector3(.07f, .005f, .008f), fabric, false);
            F.Form("LowerLip", root, new Vector3(0f, -.092f, .123f), new Vector3(.064f, .012f, .012f), skin, "Pad");
            for (int side = -1; side <= 1; side += 2)
            {
                F.Form("Ear", root, new Vector3(side * .128f, -.012f, -.008f), new Vector3(.033f, .083f, .043f), skin, "Pad");
                F.Form("EarFold", root, new Vector3(side * .142f, -.012f, .001f), new Vector3(.008f, .045f, .022f), fabric, "Pad");
                F.Form("Cheek", root, new Vector3(side * .073f, -.035f, .087f), new Vector3(.075f, .07f, .055f), skin, "Pad");
                F.Form("EyeSocket", root, new Vector3(side * .054f, .019f, .111f), new Vector3(.067f, .028f, .018f), fabric, "Pad");
                F.Form("EyeWhite", root, new Vector3(side * .054f, .018f, .122f), new Vector3(.044f, .012f, .009f), CharacterSurfaceLibrary.Get("Tape"), "Pad");
                F.Form("Iris", root, new Vector3(side * .052f, .018f, .129f), new Vector3(.012f, .013f, .006f), dark, "Pad");
                Box("UpperLid", root, new Vector3(side * .054f, .025f, .128f), new Vector3(.048f, .005f, .007f), skin, false);
                GameObject brow = F.Form("Brow", root, new Vector3(side * .055f, .045f, .113f), new Vector3(.075f, .012f, .014f), dark, "Pad");
                brow.transform.localRotation = Quaternion.Euler(0f, 0f, side * -7f);
                Box("Sideburn", root, new Vector3(side * .119f, .04f, -.002f), new Vector3(.012f, .059f, .06f), dark, false);
            }
            F.Form("CroppedHair", root, new Vector3(0f, .103f, -.025f), new Vector3(.256f, .12f, .237f), dark, "Head");
            for (int i = 0; i < 9; i++)
                F.Beam("HairGrain", root, new Vector3((i - 4) * .022f, .14f - Mathf.Abs(i - 4) * .006f, -.055f),
                    new Vector3((i - 4) * .02f, .128f - Mathf.Abs(i - 4) * .006f, .035f), .006f, fabric);
            F.Merge(root, "HeadV3");
        }
    }
}

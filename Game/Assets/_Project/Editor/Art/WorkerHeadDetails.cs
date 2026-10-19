using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.WorkerVisualMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Completely redesigned player character head: anatomical facial contours, expressive eyes with depth,
    /// volumetric layered hair with natural flow, salvage communication headset, and fitted tactical neck gaiter.
    /// Leaves the crown anchor perfectly aligned for helmets and cosmetics.
    /// </summary>
    public static class WorkerHeadDetails
    {
        public static void Build(Transform root)
        {
            Material skin = CharacterSurfaceLibrary.Get("Skin");
            Material dark = CharacterSurfaceLibrary.Get("Rubber");
            Material fabric = CharacterSurfaceLibrary.Get("CrewTrim");
            Material trim = CharacterSurfaceLibrary.Get("Canvas");
            Material steel = CharacterSurfaceLibrary.Get("Steel");
            Material tape = CharacterSurfaceLibrary.Get("Tape");
            Material leather = CharacterSurfaceLibrary.Get("Leather");

            // --- NECK & TACTICAL GAITER ---
            F.Form("MuscularNeck", root, new Vector3(0f, -.165f, -.015f), new Vector3(.145f, .185f, .145f), skin, "Tube");
            // Heavy ballistic neck seal
            F.Form("BallisticNeckGaiter", root, new Vector3(0f, -.175f, -.015f), new Vector3(.175f, .10f, .175f), fabric, "Tube");
            for (int i = 0; i < 4; i++)
            {
                F.Form($"GaiterRib_{i}", root, new Vector3(0f, -.145f - i * .018f, -.015f), new Vector3(.178f, .009f, .176f), trim, "Tube");
            }
            // Throat mic / environmental seal clasp at base of throat
            Box("ThroatMicClasp", root, new Vector3(0f, -.15f, .072f), new Vector3(.032f, .022f, .012f), steel, false);

            // --- CRANIUM & FACIAL FOUNDATION ---
            // Anatomically contoured head and jawline
            F.Form("CranialBase", root, Vector3.zero, new Vector3(.265f, .315f, .26f), skin, "Head");
            F.Form("SculptedJawline", root, new Vector3(0f, -.092f, .035f), new Vector3(.195f, .11f, .185f), skin, "Pad");
            F.Form("ChinCleft", root, new Vector3(0f, -.132f, .095f), new Vector3(.058f, .038f, .042f), skin, "Pad");

            // --- NOSE & MOUTH ---
            F.Form("NoseBridge", root, new Vector3(0f, -.002f, .124f), new Vector3(.038f, .085f, .052f), skin, "Pad");
            F.Form("NoseTip", root, new Vector3(0f, -.034f, .152f), new Vector3(.046f, .032f, .036f), skin, "Pad");
            for (int s = -1; s <= 1; s += 2)
            {
                F.Form("NostrilWing", root, new Vector3(s * .024f, -.038f, .138f), new Vector3(.018f, .022f, .025f), skin, "Pad");
            }
            F.Form("Philtrum", root, new Vector3(0f, -.062f, .132f), new Vector3(.022f, .024f, .015f), skin, "Pad");
            F.Form("UpperLip", root, new Vector3(0f, -.074f, .130f), new Vector3(.068f, .014f, .016f), skin, "Pad");
            Box("MouthLine", root, new Vector3(0f, -.082f, .128f), new Vector3(.066f, .004f, .008f), fabric, false);
            F.Form("LowerLip", root, new Vector3(0f, -.092f, .126f), new Vector3(.064f, .014f, .015f), skin, "Pad");

            // --- EYES, CHEEKS & EARS ---
            for (int side = -1; side <= 1; side += 2)
            {
                // Defined zygomatic cheekbone
                F.Form("Cheekbone", root, new Vector3(side * .075f, -.032f, .092f), new Vector3(.072f, .065f, .058f), skin, "Pad");

                // Deep orbital eye socket
                F.Form("DeepSocket", root, new Vector3(side * .055f, .021f, .112f), new Vector3(.065f, .029f, .022f), fabric, "Pad");
                // Realistic sclera, iris, and pupil
                F.Form("Sclera", root, new Vector3(side * .055f, .020f, .123f), new Vector3(.042f, .014f, .011f), tape, "Pad");
                F.Form("Iris", root, new Vector3(side * .053f, .020f, .130f), new Vector3(.016f, .016f, .006f), leather, "Pad");
                F.Form("Pupil", root, new Vector3(side * .053f, .020f, .133f), new Vector3(.008f, .008f, .004f), dark, "Pad");

                // Eyelids
                Box("UpperEyelid", root, new Vector3(side * .055f, .029f, .129f), new Vector3(.048f, .006f, .010f), skin, false);
                Box("LowerEyelid", root, new Vector3(side * .055f, .011f, .128f), new Vector3(.046f, .005f, .009f), skin, false);

                // Expressive, sculpted brow ridge
                GameObject brow = F.Form("SculptedBrow", root, new Vector3(side * .058f, .048f, .116f), new Vector3(.078f, .015f, .018f), dark, "Pad");
                brow.transform.localRotation = Quaternion.Euler(0f, 0f, side * -9f);

                // Ears with anatomical folds
                F.Form("AuricleEar", root, new Vector3(side * .130f, -.010f, -.008f), new Vector3(.034f, .086f, .045f), skin, "Pad");
                F.Form("ConchaFold", root, new Vector3(side * .143f, -.010f, .001f), new Vector3(.010f, .048f, .024f), fabric, "Pad");
            }

            // --- LAYERED VOLUMETRIC HAIRCUT ---
            // Volumetric crown foundation
            F.Form("HairVolumeBase", root, new Vector3(0f, .108f, -.022f), new Vector3(.264f, .125f, .245f), dark, "Head");
            // Styled parting and sweeping locks across the top
            for (int i = 0; i < 11; i++)
            {
                float x = (i - 5) * .021f;
                float arch = Mathf.Cos(x * 12f) * .014f;
                F.Beam($"CrownLock_{i}", root,
                    new Vector3(x, .145f + arch, -.065f),
                    new Vector3(x * .92f, .132f + arch, .048f), .0085f, dark);
            }
            // Front fringe / bangs
            for (int f = 0; f < 6; f++)
            {
                float fx = -.06f + f * .024f;
                F.Beam($"FringeLock_{f}", root,
                    new Vector3(fx, .132f, .045f),
                    new Vector3(fx + .006f, .082f, .105f), .0075f, dark);
            }
            // Sideburns and temple hair tapering down
            for (int s = -1; s <= 1; s += 2)
            {
                Box("TempleSideburn", root, new Vector3(s * .122f, .035f, .012f), new Vector3(.014f, .068f, .055f), dark, false);
                Box("NapeTaper", root, new Vector3(s * .065f, -.025f, -.112f), new Vector3(.048f, .085f, .022f), dark, false);
            }

            // --- SALVAGE CREW COMMS HEADSET ---
            // Single-ear tactical headset on the right ear with boom mic
            Transform headset = Group("SalvageCommsHeadset", root);
            F.Form("HeadsetEarCup", headset, new Vector3(.148f, -.008f, -.008f), new Vector3(.028f, .072f, .062f), dark, "Pad");
            Box("EarCupRing", headset, new Vector3(.158f, -.008f, -.008f), new Vector3(.008f, .055f, .048f), steel, false);
            // Behind-head tension strap
            for (int i = 0; i < 7; i++)
            {
                float a = (i / 6f) * Mathf.PI;
                Box($"HeadsetStrap_{i}", headset,
                    new Vector3(Mathf.Cos(a) * .138f, .06f, -Mathf.Sin(a) * .125f),
                    new Vector3(.012f, .014f, .028f), steel, false);
            }
            // Boom microphone extending from the ear cup toward the mouth
            F.Beam("BoomArm", headset, new Vector3(.152f, -.012f, -.005f), new Vector3(.095f, -.068f, .105f), .006f, steel);
            Box("MicrophoneCapsule", headset, new Vector3(.092f, -.070f, .112f), new Vector3(.016f, .016f, .026f), dark, false);
            Box("MicGrill", headset, new Vector3(.084f, -.070f, .118f), new Vector3(.005f, .012f, .018f), steel, false);
            // Green status indicator LED on the mic
            Box("CommsLed", headset, new Vector3(.096f, -.062f, .108f), new Vector3(.004f, .004f, .004f), tape, false);

            F.Merge(root, "HeadV4");
        }
    }
}

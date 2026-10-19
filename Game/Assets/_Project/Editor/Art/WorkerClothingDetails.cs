using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.WorkerVisualMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Completely redesigned tactical salvage workwear and protective hazard suit.
    /// Rich depth, articulated armor plates, functional harness webbing, tactical cargo pouches,
    /// reinforced composite knee/elbow guards, and rugged steel-toe salvage boots.
    /// </summary>
    public static class WorkerClothingDetails
    {
        private static Material M(string key) => CharacterSurfaceLibrary.Get(key);

        public static void Hips(Transform root)
        {
            // Reinforced hazard suit seat with tailored ergonomic seams
            F.Form("CoverallSeat", root, Vector3.zero, new Vector3(.38f, .26f, .28f), M("CrewFabric"));
            // Heavy ballistic seat reinforcement panel
            Box("RearReinforcement", root, new Vector3(0f, -.022f, -.142f), new Vector3(.30f, .14f, .016f), M("CrewTrim"), false);

            // Heavy duty modular tactical rigger's belt
            Box("TacticalBelt", root, new Vector3(0f, .078f, .006f), new Vector3(.392f, .052f, .292f), M("Leather"), false);
            // Quick-release Cobra-style steel buckle
            Box("CobraBuckleBase", root, new Vector3(0f, .078f, .158f), new Vector3(.076f, .054f, .020f), M("Steel"), false);
            Box("CobraBuckleReleaseL", root, new Vector3(-.042f, .078f, .162f), new Vector3(.012f, .024f, .014f), M("Rubber"), false);
            Box("CobraBuckleReleaseR", root, new Vector3(.042f, .078f, .162f), new Vector3(.012f, .024f, .014f), M("Rubber"), false);
            Box("BeltKeeper", root, new Vector3(.058f, .078f, .155f), new Vector3(.016f, .058f, .014f), M("Rubber"), false);

            // Reinforced belt loops around the waistband
            for (int i = -3; i <= 3; i++)
            {
                float a = (i / 3f) * 1.1f;
                Box($"BeltLoop_{i}", root, new Vector3(Mathf.Sin(a) * .175f, .076f, Mathf.Cos(a) * .145f), new Vector3(.014f, .072f, .014f), M("CrewFabric"), false);
            }

            // Modular utility pouches and climbing carabiners on both hips
            for (int side = -1; side <= 1; side += 2)
            {
                F.Form("SideUtilityPouch", root, new Vector3(side * .202f, -.018f, -.012f), new Vector3(.075f, .185f, .155f), M("Canvas"), "Pad");
                Box("PouchStormFlap", root, new Vector3(side * .228f, .042f, -.012f), new Vector3(.020f, .052f, .158f), M("Leather"), false);
                F.Form("PouchSnapL", root, new Vector3(side * .240f, .038f, -.045f), new Vector3(.010f, .016f, .016f), M("Steel"), "Tube");
                F.Form("PouchSnapR", root, new Vector3(side * .240f, .038f, .025f), new Vector3(.010f, .016f, .016f), M("Steel"), "Tube");

                // MOLLE webbing loops on pouch side
                for (int w = 0; w < 3; w++)
                    Box($"MolleWebbing_{side}_{w}", root, new Vector3(side * .236f, -.055f, -.055f + w * .038f), new Vector3(.014f, .048f, .014f), M("Rubber"), false);

                // Steel gear carabiner hanging on hip
                Box("GearCarabiner", root, new Vector3(side * .185f, .045f, .135f), new Vector3(.015f, .042f, .028f), M("Steel"), false);
            }
            F.Merge(root, "HipsV4");
        }

        public static void Torso(Transform root)
        {
            // Tailored ballistic protective jacket foundation
            F.Form("BallisticJacket", root, new Vector3(0f, .17f, 0f), new Vector3(.49f, .50f, .33f), M("CrewFabric"), "Jacket");
            // Heavy ballistic shoulder yokes with abrasion plating
            Box("ShoulderYoke", root, new Vector3(0f, .33f, -.148f), new Vector3(.40f, .10f, .022f), M("CrewTrim"), false);
            // Accordion-stretch action pleat down the spine for athletic range of motion
            Box("SpineActionPleat", root, new Vector3(0f, .17f, -.165f), new Vector3(.032f, .34f, .020f), M("CrewFabric"), false);

            // Heavy duty industrial zipper with protective storm placket
            Box("StormPlacket", root, new Vector3(0f, .14f, .162f), new Vector3(.042f, .35f, .022f), M("CrewTrim"), false);
            Box("HeavyZipTeeth", root, new Vector3(0f, .14f, .176f), new Vector3(.012f, .32f, .012f), M("Steel"), false);
            F.Form("ZipSliderPull", root, new Vector3(0f, .29f, .188f), new Vector3(.026f, .044f, .010f), M("Steel"), "Pad");

            // Tactical chest pockets, harness, and high-visibility striping
            for (int side = -1; side <= 1; side += 2)
            {
                // Ergonomic collar lapel fold
                F.Form("CollarLapel", root, new Vector3(side * .078f, .385f, .058f), new Vector3(.125f, .125f, .14f), M("CrewTrim"), "Pad")
                    .transform.localRotation = Quaternion.Euler(8f, 0f, side * -24f);

                // Deep angled chest bellows pocket
                F.Form("AngledChestPocket", root, new Vector3(side * .135f, .23f, .162f), new Vector3(.125f, .135f, .032f), M("CrewFabric"), "Pad");
                Box("ChestPocketFlap", root, new Vector3(side * .135f, .28f, .184f), new Vector3(.125f, .038f, .014f), M("CrewTrim"), false);
                F.Form("PocketMetalSnap", root, new Vector3(side * .135f, .275f, .195f), new Vector3(.014f, .014f, .008f), M("Steel"), "Tube");

                // Heavy load-bearing nylon harness straps
                Strap(root, new Vector3(side * .185f, .365f, .148f), new Vector3(side * .135f, -.055f, .170f), .042f);
                Strap(root, new Vector3(side * .185f, .365f, -.145f), new Vector3(side * .115f, -.045f, -.152f), .042f);

                // Steel harness quick-adjusters and friction buckles
                Box("HarnessFrictionBuckle", root, new Vector3(side * .165f, .155f, .194f), new Vector3(.056f, .052f, .014f), M("Steel"), false);
                Box("AdjusterTab", root, new Vector3(side * .165f, .155f, .204f), new Vector3(.032f, .036f, .010f), M("Rubber"), false);

                // Prismatic high-visibility safety stripes
                Box("ShoulderHiVisTape", root, new Vector3(side * .215f, .31f, .140f), new Vector3(.040f, .16f, .018f), M("Tape"), false);
                Box("WaistHiVisTape", root, new Vector3(side * .145f, .03f, .172f), new Vector3(.095f, .028f, .014f), M("Tape"), false);
            }

            // Sternum cross-strap with quick-release tactical buckle
            Box("SternumCrossStrap", root, new Vector3(0f, .115f, .195f), new Vector3(.29f, .028f, .014f), M("Leather"), false);
            Box("SternumQuickRelease", root, new Vector3(.016f, .115f, .208f), new Vector3(.052f, .036f, .020f), M("Rubber"), false);

            // Broad reflective safety band across upper back
            Box("BackReflectiveBand", root, new Vector3(0f, .30f, -.168f), new Vector3(.34f, .038f, .015f), M("Tape"), false);
            // Radio shoulder holster on left shoulder harness
            Transform radioHolster = Group("RadioHolster", root);
            radioHolster.localPosition = new Vector3(-.185f, .27f, .162f);
            Box("RadioCase", radioHolster, Vector3.zero, new Vector3(.045f, .078f, .032f), M("Rubber"), false);
            Box("RadioAntenna", radioHolster, new Vector3(0f, .068f, 0f), new Vector3(.008f, .075f, .008f), M("Rubber"), false);
            Box("RadioPTT", radioHolster, new Vector3(-.024f, .015f, 0f), new Vector3(.006f, .022f, .016f), M("Steel"), false);

            // Embossed crew identification patch with steel border frame
            Box("CrewPatchHolder", root, new Vector3(.088f, .225f, .208f), new Vector3(.084f, .066f, .014f), M("Rubber"), false);
            Box("PatchFrameTop", root, new Vector3(.088f, .256f, .218f), new Vector3(.080f, .005f, .005f), M("Steel"), false);
            Box("PatchFrameBottom", root, new Vector3(.088f, .194f, .218f), new Vector3(.080f, .005f, .005f), M("Steel"), false);

            F.Merge(root, "TorsoV4");
            WorkerVisualBuilder.NumberBadge(root, new Vector3(.088f, .225f, .222f));
        }

        private static void Strap(Transform root, Vector3 a, Vector3 b, float width)
        {
            Vector3 d = b - a;
            GameObject strap = Box("HarnessWebbing", root, (a + b) * .5f, new Vector3(width, d.magnitude, .014f), M("Leather"), false);
            strap.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        }

        public static void Arm(Transform root, float side)
        {
            // Ergonomic articulated sleeve
            F.Form("ArticulatedSleeve", root, new Vector3(0f, -.235f, 0f), new Vector3(.165f, .50f, .185f), M("CrewFabric"));
            // Molded composite elbow armor cap with steel attachment rivets
            F.Form("CompositeElbowShell", root, new Vector3(0f, -.255f, -.092f), new Vector3(.128f, .158f, .032f), M("CrewTrim"), "Pad");
            Box("ElbowCapCenter", root, new Vector3(0f, -.255f, -.105f), new Vector3(.082f, .11f, .016f), M("Rubber"), false);
            for (int r = -1; r <= 1; r += 2)
            {
                Box($"ElbowRivetTop_{r}", root, new Vector3(r * .042f, -.215f, -.098f), new Vector3(.012f, .012f, .008f), M("Steel"), false);
                Box($"ElbowRivetBottom_{r}", root, new Vector3(r * .042f, -.295f, -.098f), new Vector3(.012f, .012f, .008f), M("Steel"), false);
            }

            // Arm high-vis band and reinforced cuff
            F.Form("SleeveReflectiveBand", root, new Vector3(0f, -.412f, 0f), new Vector3(.152f, .034f, .162f), M("Tape"), "Tube");
            // Heavy duty hook-and-loop cuff adjustment strap
            Box("CuffCinchStrap", root, new Vector3(0f, -.438f, -.052f), new Vector3(.138f, .026f, .016f), M("CrewTrim"), false);
            Box("CuffCinchBuckle", root, new Vector3(side * .055f, -.438f, -.058f), new Vector3(.028f, .032f, .012f), M("Steel"), false);
            // Sealed ribbed inner knit cuff
            F.Form("SealedKnitCuff", root, new Vector3(0f, -.462f, 0f), new Vector3(.128f, .068f, .135f), M("Rubber"), "Tube");

            // Tactical heavy work glove
            Glove(root, new Vector3(0f, -.478f, .012f), Quaternion.identity, 1f, side);
            F.Merge(root, side < 0f ? "ArmLV4" : "ArmRV4");
        }

        public static void Glove(Transform root, Vector3 at, Quaternion rotation, float scale, float side)
        {
            Transform glove = Group("WorkGlove", root);
            glove.localPosition = at; glove.localRotation = rotation; glove.localScale = Vector3.one * scale;

            // Heavy duty split-grain leather palm
            F.Form("SplitLeatherPalm", glove, new Vector3(0f, -.055f, .009f), new Vector3(.118f, .118f, .095f), M("Leather"), "Pad");
            // Molded rubber backhand protection
            F.Form("RubberBackhandPlate", glove, new Vector3(0f, -.052f, -.038f), new Vector3(.115f, .094f, .030f), M("Rubber"), "Pad");

            // Articulated armored knuckles
            for (int k = 0; k < 4; k++)
            {
                float kx = (k - 1.5f) * .027f;
                F.Form($"ArmoredKnuckle_{k}", glove, new Vector3(kx, -.070f, -.056f), new Vector3(.026f, .032f, .024f), M("Steel"), "Pad");
            }

            // Fingers with dual-density protection and leather grip pads
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * .026f, length = i is 0 or 3 ? .068f : .082f;
                F.Form($"Finger_{i}", glove, new Vector3(x, -.115f - length * .2f, .024f), new Vector3(.028f, length, .044f), M("Leather"), "Tube");
                // Armored rubber finger guard pads
                F.Form($"FingerGuard_{i}", glove, new Vector3(x, -.108f, -.005f), new Vector3(.026f, .030f, .026f), M("Rubber"), "Pad");
                Box($"FingerTipGrip_{i}", glove, new Vector3(x, -.152f, .025f), new Vector3(.024f, .020f, .022f), M("Rubber"), false);
            }

            // Ergonomic opposed thumb with reinforced thumb crotch
            F.Form("ArmoredThumb", glove, new Vector3(side * .066f, -.068f, .042f), new Vector3(.042f, .078f, .046f), M("Leather"), "Tube")
                .transform.localRotation = Quaternion.Euler(-25f, 0f, side * -28f);

            // Wrist retention strap with Velcro pull tab
            Box("WristRetentionStrap", glove, new Vector3(0f, -.006f, -.054f), new Vector3(.118f, .026f, .014f), M("CrewTrim"), false);
            Box("WristPullTab", glove, new Vector3(side * .042f, -.006f, -.064f), new Vector3(.034f, .024f, .012f), M("Rubber"), false);
        }

        public static void Thigh(Transform root, float side)
        {
            // Ergonomic articulated hazard trousers
            F.Form("TacticalTrouser", root, new Vector3(0f, -.18f, 0f), new Vector3(.20f, .38f, .225f), M("CrewFabric"));
            Box("DoubleNeedleSeam", root, new Vector3(side * .096f, -.18f, .004f), new Vector3(.008f, .30f, .010f), M("CrewTrim"), false);

            // Expandable bellows cargo pocket with compression strap
            F.Form("BellowsCargoPocket", root, new Vector3(side * .105f, -.17f, .016f), new Vector3(.046f, .17f, .16f), M("CrewFabric"), "Pad");
            Box("CargoPocketFlap", root, new Vector3(side * .128f, -.095f, .016f), new Vector3(.018f, .042f, .164f), M("CrewTrim"), false);
            F.Form("CargoSnapA", root, new Vector3(side * .138f, -.096f, -.035f), new Vector3(.010f, .014f, .014f), M("Steel"), "Tube");
            F.Form("CargoSnapB", root, new Vector3(side * .138f, -.096f, .065f), new Vector3(.010f, .014f, .014f), M("Steel"), "Tube");
            // Cargo pocket compression strap
            Box("CargoCompressionStrap", root, new Vector3(side * .128f, -.18f, .016f), new Vector3(.014f, .022f, .162f), M("Leather"), false);

            // Molded heavy duty knee pad with articulated flex ribs
            F.Form("HeavyKneePad", root, new Vector3(0f, -.318f, .105f), new Vector3(.148f, .158f, .042f), M("Rubber"), "Pad");
            for (int i = 0; i < 4; i++)
            {
                Box($"KneeFlexRib_{i}", root, new Vector3(0f, -.265f - i * .028f, .130f), new Vector3(.115f, .009f, .016f), M("CrewTrim"), false);
            }
            F.Merge(root, side < 0f ? "ThighLV4" : "ThighRV4");
        }

        public static void Shin(Transform root, float side)
        {
            // Lower trouser leg with ankle gaiter seal
            F.Form("TrouserShin", root, new Vector3(0f, -.19f, 0f), new Vector3(.185f, .39f, .205f), M("CrewFabric"));
            Box("ShinSideSeam", root, new Vector3(side * .085f, -.17f, .005f), new Vector3(.007f, .28f, .008f), M("CrewTrim"), false);
            // High-visibility reflective ankle band
            F.Form("AnkleReflectiveTape", root, new Vector3(0f, -.252f, .001f), new Vector3(.188f, .038f, .208f), M("Tape"), "Tube");

            // RUGGED STEEL-TOE COMBAT SALVAGE BOOTS
            // Reinforced full-grain leather boot shaft
            F.Form("LeatherBootShaft", root, new Vector3(0f, -.355f, 0f), new Vector3(.178f, .23f, .195f), M("Leather"), "Cloth");
            // Molded boot foot foundation
            F.Form("MoldedBootFoot", root, new Vector3(0f, -.42f, .058f), new Vector3(.21f, .175f, .33f), M("Leather"), "Boot");

            // Armored steel toe cap shell with impact rubber coating
            F.Form("SteelToeCap", root, new Vector3(0f, -.432f, .156f), new Vector3(.214f, .11f, .15f), M("Rubber"), "Pad");
            Box("SteelToeAccent", root, new Vector3(0f, -.438f, .215f), new Vector3(.11f, .035f, .015f), M("Steel"), false);

            // Reinforced heel counter
            F.Form("HeelCounterShell", root, new Vector3(0f, -.435f, -.065f), new Vector3(.195f, .12f, .11f), M("Rubber"), "Pad");

            // Goodyear welt stitched midsole
            F.Form("StitchedWelt", root, new Vector3(0f, -.478f, .058f), new Vector3(.218f, .026f, .335f), M("CrewTrim"), "Pad");
            // Aggressive Vibram-style deep lugged rubber outsole
            F.Form("LuggedOutsole", root, new Vector3(0f, -.502f, .058f), new Vector3(.225f, .038f, .345f), M("Rubber"), "Pad");

            // Aggressive directional traction cleats and lugs
            for (int i = 0; i < 7; i++)
            {
                float z = -.085f + i * .052f;
                Box($"TractionLug_{i}", root, new Vector3(0f, -.520f, z), new Vector3(.195f, .020f, .026f), M("Rubber"), false);
                Box($"EdgeCleatL_{i}", root, new Vector3(-.102f, -.518f, z), new Vector3(.022f, .020f, .022f), M("Rubber"), false);
                Box($"EdgeCleatR_{i}", root, new Vector3(.102f, -.518f, z), new Vector3(.022f, .020f, .022f), M("Rubber"), false);
            }

            // Speed-lace eyelets and tactical cross-lacing
            for (int i = 0; i < 6; i++)
            {
                float ly = -.325f - i * .019f;
                float lz = .092f + i * .007f;
                for (int s = -1; s <= 1; s += 2)
                {
                    F.Form($"SpeedEyelet_{s}_{i}", root, new Vector3(s * .048f, ly, lz), new Vector3(.013f, .013f, .009f), M("Steel"), "Tube");
                }
                F.Beam($"BootLaceCross1_{i}", root,
                    new Vector3(-.046f, ly + .004f, lz + .006f),
                    new Vector3(.046f, ly - .014f, lz + .006f), .007f, M("Canvas"));
                F.Beam($"BootLaceCross2_{i}", root,
                    new Vector3(.046f, ly + .004f, lz + .006f),
                    new Vector3(-.046f, ly - .014f, lz + .006f), .007f, M("Canvas"));
            }

            // Rear boot pull loop
            Box("BootPullLoop", root, new Vector3(0f, -.288f, -.098f), new Vector3(.038f, .075f, .016f), M("Canvas"), false);
            F.Merge(root, side < 0f ? "ShinLV4" : "ShinRV4");
        }
    }
}

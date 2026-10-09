using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Company plans, schematics and pinned papers retain their nontext visual detail.</summary>
    public static class HqLogisticsBoard
    {
        public static void Build(Transform parent)
        {
            Transform board = At("LogisticsCorkboard", parent, HqLayout.Logistics, 90f);
            Material cork = PolishAssets.Material("HQ_Corkboard", new Color(.40f, .29f, .18f), .96f,
                PolishAssets.Texture("HQ_CorkGrain"));
            Material wood = CustomMallArt.Material("Wood");
            Box("CorkPanel", board, Vector3.zero, new Vector3(3.18f, 2.14f, .06f), cork, false);
            foreach (float x in new[] { -1.63f, 1.63f })
                Box("BoardFrame", board, new Vector3(x, 0f, 0f), new Vector3(.075f, 2.23f, .11f), wood, false);
            foreach (float y in new[] { -1.105f, 1.105f })
                Box("BoardFrame", board, new Vector3(0f, y, 0f), new Vector3(3.32f, .075f, .11f), wood, false);
            Label(board, "LOGISTICS / CREW NOTES", new Vector3(0f, 1.31f, -.034f), .087f, new Color(.69f, .64f, .49f));

            MallBlueprint(board);
            AnnotatedNote(board, new Vector3(.8f, .53f, -.059f), "BLIND ONE / LESS CLATTER", -4f);
            AnnotatedNote(board, new Vector3(.8f, -.09f, -.065f), "STALKER / WORK IN PAIRS", 3f);
            Transform warning = Paper(board, new Vector3(.82f, -.76f, -.067f), new Vector2(1.18f, .49f), -3f);
            Box("HazardHeader", warning, new Vector3(0f, .16f, -.011f), new Vector3(1.15f, .12f, .008f), CustomMallArt.Material("Hazard"), false);
            Label(warning, "LIGHT / USE WITH CARE", new Vector3(0f, .16f, -.017f), .039f);
            Label(warning, "BEAM ON CRAWLERS\nBEAM OFF THE THING", new Vector3(0f, -.053f, -.021f), .044f);
            Transform returnNote = Paper(board, new Vector3(-.75f, -.77f, -.066f), new Vector2(1.24f, .35f), 3f);
            Label(returnNote, "TAG EVERY LOAD\nKEEP AN EYE ON STOCK", new Vector3(0f, 0f, -.02f), .039f);
            Label(board, "LOAD LIMITS ARE NOT A SUGGESTION", new Vector3(0f, -1.28f, -.022f), .057f, new Color(.68f, .6f, .38f));
            MarkStatic(board);
        }

        private static void MallBlueprint(Transform board)
        {
            Transform plan = Paper(board, new Vector3(-.72f, .22f, -.052f), new Vector2(1.46f, 1.39f), 2f,
                PolishAssets.Material("HQ_BlueprintPaper", new Color(.23f, .32f, .36f), .94f));
            Material ink = PolishAssets.Material("HQ_BlueprintInk", new Color(.64f, .73f, .69f), .95f);
            Label(plan, "MERIDIAN MALL\nSERVICE / HAUL ROUTES", new Vector3(0f, .53f, -.02f), .051f, new Color(.77f, .80f, .73f));
            Outline(plan, new Vector2(-.59f, -.34f), new Vector2(.59f, .34f), ink);
            Outline(plan, new Vector2(-.2f, -.14f), new Vector2(.24f, .18f), ink);
            MapLine(plan, new Vector2(-.36f, -.34f), new Vector2(-.36f, .34f), ink);
            MapLine(plan, new Vector2(.39f, -.34f), new Vector2(.39f, .34f), ink);
            MapLine(plan, new Vector2(-.59f, .21f), new Vector2(.59f, .21f), ink);
            MapLine(plan, new Vector2(-.59f, -.22f), new Vector2(.59f, -.22f), ink);
            for (int i = 0; i < 4; i++)
                MapLine(plan, new Vector2(-.56f + i * .041f, -.13f), new Vector2(-.56f + i * .041f, .07f), ink, .009f);
            Label(plan, "ATRIUM\nVOID", new Vector3(.02f, .015f, -.023f), .035f, new Color(.77f, .80f, .73f));
            Label(plan, "RAMP", new Vector3(-.49f, -.07f, -.024f), .021f, new Color(.77f, .80f, .73f));
            Label(plan, "LOADING", new Vector3(-.32f, -.45f, -.026f), .038f, new Color(.77f, .80f, .73f));

            Material marker = PolishAssets.Material("HQ_CrewRedMarker", new Color(.61f, .16f, .09f), .94f);
            for (int i = 0; i < 7; i++)
                MapLine(plan, new Vector2(-.3f, -.3f + i * .087f), new Vector2(-.294f, -.26f + i * .087f), marker, .016f);
            for (int i = 0; i < 5; i++)
                MapLine(plan, new Vector2(-.23f + i * .13f, .28f), new Vector2(-.17f + i * .13f, .276f), marker, .016f);
            MapLine(plan, new Vector2(.41f, .27f), new Vector2(.31f, .35f), marker, .016f);
            MapLine(plan, new Vector2(.41f, .27f), new Vector2(.32f, .19f), marker, .016f);
            MapLine(plan, new Vector2(.43f, -.16f), new Vector2(.55f, -.29f), marker, .018f);
            MapLine(plan, new Vector2(.55f, -.16f), new Vector2(.43f, -.29f), marker, .018f);
            Label(plan, "CRACKED / GO AROUND", new Vector3(.12f, -.55f, -.025f), .035f, new Color(.78f, .43f, .27f));

            Transform circuit = Paper(board, new Vector3(-1.01f, .78f, -.087f), new Vector2(.74f, .32f), -9f);
            Material black = CustomMallArt.Material("Black");
            MapLine(circuit, new Vector2(-.29f, -.025f), new Vector2(.29f, -.025f), black, .009f);
            for (int i = 0; i < 3; i++)
            {
                Box("FuseDiagram", circuit, new Vector3(-.16f + i * .17f, -.024f, -.018f), new Vector3(.055f, .073f, .004f), black, false);
                Box("FuseCentre", circuit, new Vector3(-.16f + i * .17f, -.024f, -.022f), new Vector3(.04f, .049f, .003f), CustomMallArt.Material("Paper"), false);
            }
            Label(circuit, "CHARGER / 12V ONLY", new Vector3(0f, .095f, -.028f), .033f);
        }

        private static void AnnotatedNote(Transform board, Vector3 at, string subject, float angle)
        {
            Transform note = Paper(board, at, new Vector2(1.14f, .53f), angle);
            Label(note, subject, new Vector3(0f, -.14f, -.021f), .037f);
            Material marker = CustomMallArt.Material("Black");
            MapLine(note, new Vector2(-.38f, -.018f), new Vector2(.34f, -.008f), marker, .007f);
            MapLine(note, new Vector2(-.32f, -.036f), new Vector2(.37f, -.023f), marker, .005f);
        }

        private static Transform Paper(Transform board, Vector3 at, Vector2 size, float angle, Material paper = null)
        {
            Transform note = At("PinnedDocument", board, at);
            note.localRotation = Quaternion.Euler(0f, 0f, angle);
            Box("Paper", note, Vector3.zero, new Vector3(size.x, size.y, .013f), paper ?? CustomMallArt.Material("Paper"), false);
            Material tape = PolishAssets.Material("HQ_AgedTape", new Color(.63f, .55f, .37f), .96f);
            foreach (float x in new[] { -size.x * .35f, size.x * .35f })
            {
                GameObject strip = Box("MaskingTape", note, new Vector3(x, size.y * .48f, -.01f), new Vector3(.17f, .071f, .006f), tape, false);
                strip.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0f ? -13f : 8f);
            }
            Primitive(PrimitiveType.Sphere, "DrawingPin", note, new Vector3(.014f, size.y * .38f, -.025f),
                new Vector3(.025f, .025f, .018f), CustomMallArt.Material("Rust"), false);
            return note;
        }

        private static void Outline(Transform paper, Vector2 min, Vector2 max, Material ink)
        {
            MapLine(paper, min, new Vector2(max.x, min.y), ink);
            MapLine(paper, new Vector2(max.x, min.y), max, ink);
            MapLine(paper, max, new Vector2(min.x, max.y), ink);
            MapLine(paper, new Vector2(min.x, max.y), min, ink);
        }

        private static void MapLine(Transform paper, Vector2 from, Vector2 to, Material ink, float width = .013f)
            => Bar("PenLine", paper, new Vector3(from.x, from.y, -.018f), new Vector3(to.x, to.y, -.018f), width, .005f, ink);

    }
}

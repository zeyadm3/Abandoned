using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Nordic Habitat stock for both floors, with lightweight office silhouettes upstairs.</summary>
    internal static class SalvageFurnitureModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "steel_filing_cabinet": Cabinet(root, lod); return true;
                case "ergonomic_chair": Chair(root, lod, false); return true;
                case "executive_chair": Chair(root, lod, true); return true;
                case "oak_conference_table": Table(root, lod); return true;
                case "modular_shelving": Shelving(root, lod); return true;
                case "fabric_roll": FabricRoll(root, lod); return true;
                case "flatpack_desk": Desk(root, lod); return true;
                case "shrinkwrap_roll": Shrinkwrap(root, lod); return true;
                case "heavy_dolly": Dolly(root, lod); return true;
                default: return false;
            }
        }

        private static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        private static void Cabinet(Transform p, int lod)
        {
            Box(p, "steel cabinet shell", new Vector3(0, .66f, 0), new Vector3(.48f, 1.32f, .58f), "Metal");
            Box(p, "plinth toe recess", new Vector3(0, .04f, -.291f), new Vector3(.42f, .07f, .01f), "Rubber");
            for (int i = 0; i < 4; i++)
            {
                float y = .213f + i * .303f;
                Box(p, "drawer front", new Vector3(0, y, -.296f), new Vector3(.433f, .286f, .019f), "Paint");
                Box(p, "drawer seam", new Vector3(0, y - .147f, -.307f), new Vector3(.438f, .003f, .003f), "Rubber");
                if (lod == 2) continue;
                Bar(p, "recessed drawer pull", new Vector3(-.058f, y + .058f, -.317f), new Vector3(.058f, y + .058f, -.317f), .012f, "Metal", Sides(lod));
                Box(p, "index card frame", new Vector3(0, y - .03f, -.309f), new Vector3(.092f, .03f, .005f), "Metal");
                if (lod == 0)
                    Box(p, "paper index card", new Vector3(0, y - .03f, -.313f), new Vector3(.076f, .021f, .002f), "Paper");
            }
            if (lod == 2) return;
            Cylinder(p, "cabinet lock", new Vector3(.182f, 1.279f, -.302f), new Vector3(.017f, .012f, .017f), "Metal", new Vector3(90, 0, 0), Sides(lod));
            Box(p, "salvage adhesive strip", new Vector3(.10f, .78f, -.318f), new Vector3(.085f, .022f, .002f), "Safety", new Vector3(0, 0, 16));
            if (lod != 0) return;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "scuffed lower corner", new Vector3(x * .227f, .085f, z * .28f), new Vector3(.016f, .13f, .015f), "Rust");
        }

        private static void Chair(Transform p, int lod, bool executive)
        {
            float seatY = .48f;
            Cylinder(p, "gas lift", new Vector3(0, .27f, 0), new Vector3(.065f, .39f, .065f), "Metal", null, Sides(lod));
            Cylinder(p, "gas lift shroud", new Vector3(0, .16f, 0), new Vector3(.093f, .16f, .093f), "Plastic", null, Sides(lod));
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2 / 5;
                Vector3 end = new Vector3(Mathf.Sin(angle) * .295f, .08f, Mathf.Cos(angle) * .295f);
                Bar(p, "five star base spoke", new Vector3(0, .14f, 0), end, .04f, "Metal", Sides(lod));
                Cylinder(p, "castor wheel", end + Vector3.down * .04f, new Vector3(.08f, .048f, .08f), "Rubber", new Vector3(0, angle * Mathf.Rad2Deg, 90), Sides(lod));
                if (lod == 0)
                    Cylinder(p, "castor fork", end, new Vector3(.04f, .035f, .04f), "Plastic", null, 8);
            }
            Box(p, "upholstered seat", new Vector3(0, seatY, -.018f), new Vector3(.515f, executive ? .11f : .082f, .49f), "Fabric");
            Box(p, "seat pan", new Vector3(0, .424f, -.018f), new Vector3(.46f, .029f, .445f), "Plastic");
            float backY = executive ? .849f : .816f;
            float backHeight = executive ? .51f : .43f;
            Box(p, "backrest frame", new Vector3(0, backY, .22f), new Vector3(.47f, backHeight, .055f), "Plastic", new Vector3(8, 0, 0));
            Box(p, executive ? "padded backrest" : "mesh backrest", new Vector3(0, backY, .178f), new Vector3(.411f, backHeight - .065f, executive ? .071f : .012f), executive ? "Fabric" : "Net", new Vector3(8, 0, 0));
            if (executive)
                Box(p, "head support", new Vector3(0, 1.105f, .192f), new Vector3(.35f, .11f, .083f), "Fabric", new Vector3(8, 0, 0));
            for (int x = -1; x <= 1; x += 2)
            {
                Bar(p, "arm support", new Vector3(x * .27f, .455f, .12f), new Vector3(x * .293f, .66f, .08f), .028f, "Metal", Sides(lod));
                Box(p, "arm pad", new Vector3(x * .293f, .674f, -.015f), new Vector3(.07f, .033f, .255f), executive ? "Fabric" : "Rubber");
            }
            if (lod == 2) return;
            Bar(p, "rear spine", new Vector3(0, .40f, .17f), new Vector3(0, .83f, .257f), .055f, "Metal", Sides(lod));
            Box(p, "lumbar support", new Vector3(0, .691f, .149f), new Vector3(.32f, .074f, .031f), "Fabric");
            if (lod != 0) return;
            Bar(p, "adjustment lever", new Vector3(.13f, .40f, 0), new Vector3(.269f, .40f, -.1f), .009f, "Metal");
            Box(p, "adjustment paddle", new Vector3(.27f, .40f, -.10f), new Vector3(.047f, .013f, .044f), "Plastic");
            if (executive)
                for (int i = 0; i < 3; i++)
                    Box(p, "stitched upholstery seam", new Vector3(0, .72f + i * .1f, .131f), new Vector3(.36f, .003f, .003f), "Rubber");
            else
                for (int i = 0; i < 5; i++)
                    Box(p, "mesh support stripe", new Vector3(-.16f + i * .08f, .82f, .169f), new Vector3(.006f, .33f, .009f), "Fabric", new Vector3(8, 0, 0));
        }

        private static void Table(Transform p, int lod)
        {
            Box(p, "heavy oak table slab", new Vector3(0, .735f, 0), new Vector3(3, .09f, 1.2f), "Wood");
            for (int x = -1; x <= 1; x += 2)
            {
                Box(p, "oak pedestal", new Vector3(x * .91f, .34f, 0), new Vector3(.15f, .68f, .64f), "Wood");
                Box(p, "pedestal foot", new Vector3(x * .91f, .045f, 0), new Vector3(.24f, .09f, .91f), "Wood");
            }
            Box(p, "under-table stretcher", new Vector3(0, .22f, 0), new Vector3(1.84f, .11f, .09f), "Wood");
            if (lod == 2) return;
            Box(p, "conference cable hatch", new Vector3(0, .783f, 0), new Vector3(.34f, .008f, .15f), "Metal");
            for (int z = -1; z <= 1; z += 2)
                Box(p, "edge apron", new Vector3(0, .665f, z * .555f), new Vector3(2.81f, .1f, .055f), "Wood");
            Box(p, "high value paper tag", new Vector3(.78f, .782f, -.30f), new Vector3(.21f, .003f, .13f), "Paper", new Vector3(0, -12, 0));
            if (lod != 0) return;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Cylinder(p, "pedestal bolt", new Vector3(x * .993f, .35f, z * .20f), new Vector3(.022f, .012f, .022f), "Metal", new Vector3(0, 0, 90), 6);
            Box(p, "lift strap", new Vector3(-.74f, .785f, 0), new Vector3(.054f, .012f, 1.2f), "Fabric");
        }

        private static void Shelving(Transform p, int lod)
        {
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "slotted steel upright", new Vector3(x * .529f, .9f, z * .204f), new Vector3(.042f, 1.8f, .042f), "Metal");
            for (int i = 0; i < 4; i++)
            {
                float y = .125f + i * .54f;
                Box(p, "modular timber shelf", new Vector3(0, y, 0), new Vector3(1.07f, .032f, .45f), "Wood");
                if (lod < 2)
                    Box(p, "shelf front folded edge", new Vector3(0, y - .025f, -.213f), new Vector3(1.045f, .04f, .023f), "Metal");
            }
            Bar(p, "rear diagonal brace", new Vector3(-.52f, .14f, .207f), new Vector3(.52f, 1.72f, .207f), .016f, "Metal", Sides(lod));
            if (lod == 2) return;
            Bar(p, "rear second brace", new Vector3(.52f, .14f, .207f), new Vector3(-.52f, 1.72f, .207f), .016f, "Metal", Sides(lod));
            if (lod != 0) return;
            for (int x = -1; x <= 1; x += 2)
                for (int i = 0; i < 12; i++)
                    Box(p, "upright keyhole", new Vector3(x * .529f, .19f + i * .129f, -.227f), new Vector3(.015f, .018f, .004f), "Rubber");
            Box(p, "stock classification sticker", new Vector3(.34f, .631f, -.228f), new Vector3(.11f, .025f, .002f), "Paper");
        }

        private static void FabricRoll(Transform p, int lod)
        {
            Cylinder(p, "rolled upholstery textile", new Vector3(0, .125f, 0), new Vector3(.25f, 1.15f, .25f), "Fabric", new Vector3(90, 0, 0), Sides(lod));
            Cylinder(p, "cardboard core left", new Vector3(0, .125f, -.578f), new Vector3(.045f, .008f, .045f), "Paper", new Vector3(90, 0, 0), Sides(lod));
            Cylinder(p, "cardboard core right", new Vector3(0, .125f, .578f), new Vector3(.045f, .008f, .045f), "Paper", new Vector3(90, 0, 0), Sides(lod));
            if (lod == 2) return;
            for (int z = -1; z <= 1; z += 2)
                Cylinder(p, "packing band", new Vector3(0, .125f, z * .40f), new Vector3(.258f, .044f, .258f), "Wrap", new Vector3(90, 0, 0), Sides(lod));
            if (lod != 0) return;
            Box(p, "loose textile fold", new Vector3(.10f, .023f, .03f), new Vector3(.08f, .014f, .89f), "Fabric", new Vector3(0, 0, 12));
            Box(p, "sample identification tag", new Vector3(0, .253f, .2f), new Vector3(.12f, .003f, .09f), "Paper", new Vector3(0, 14, 0));
        }

        private static void Desk(Transform p, int lod)
        {
            Box(p, "flat pack desktop", new Vector3(0, .72f, 0), new Vector3(1.4f, .06f, .80f), "Wood");
            Box(p, "left assembled side panel", new Vector3(-.565f, .345f, 0), new Vector3(.042f, .69f, .66f), "Paint");
            Box(p, "rear modesty board", new Vector3(0, .50f, .26f), new Vector3(1.12f, .30f, .035f), "Wood");
            Box(p, "temporary steel support", new Vector3(.51f, .342f, .27f), new Vector3(.06f, .684f, .06f), "Metal");
            Box(p, "detached right side panel", new Vector3(.14f, .025f, -.06f), new Vector3(.69f, .038f, .62f), "Paint", new Vector3(0, -12, 0));
            if (lod == 2) return;
            Box(p, "half installed drawer", new Vector3(-.32f, .59f, -.165f), new Vector3(.40f, .17f, .43f), "Wood");
            Box(p, "drawer face", new Vector3(-.32f, .59f, -.388f), new Vector3(.42f, .18f, .024f), "Paint");
            Bar(p, "drawer pull", new Vector3(-.38f, .61f, -.412f), new Vector3(-.26f, .61f, -.412f), .012f, "Metal", Sides(lod));
            Box(p, "loose assembly guide", new Vector3(.38f, .752f, -.19f), new Vector3(.24f, .003f, .18f), "Paper", new Vector3(0, -18, 0));
            if (lod != 0) return;
            for (int i = 0; i < 4; i++)
                Cylinder(p, "cam lock", new Vector3(-.589f, .2f + i * .12f, -.22f), new Vector3(.016f, .007f, .016f), "Metal", new Vector3(0, 0, 90), 8);
            Box(p, "hardware packet", new Vector3(-.02f, .755f, .07f), new Vector3(.10f, .012f, .065f), "Wrap");
            Bar(p, "loose rail", new Vector3(.08f, .056f, -.29f), new Vector3(.53f, .056f, -.29f), .013f, "Metal");
        }

        private static void Shrinkwrap(Transform p, int lod)
        {
            Cylinder(p, "clear stretch film roll", new Vector3(0, .255f, 0), new Vector3(.17f, .51f, .17f), "Wrap", null, Sides(lod));
            Cylinder(p, "cardboard roll core", new Vector3(0, .275f, 0), new Vector3(.052f, .55f, .052f), "Paper", null, Sides(lod));
            Cylinder(p, "hollow core top", new Vector3(0, .553f, 0), new Vector3(.035f, .003f, .035f), "Rubber", null, Sides(lod));
            if (lod == 2) return;
            Box(p, "film leading edge", new Vector3(.072f, .25f, -.03f), new Vector3(.017f, .48f, .10f), "Wrap", new Vector3(0, 24, 0));
            Box(p, "shipping sticker", new Vector3(0, .21f, -.087f), new Vector3(.075f, .105f, .002f), "Paper");
            if (lod == 0)
                for (int i = 0; i < 4; i++)
                    Cylinder(p, "film winding seam", new Vector3(0, .046f + i * .145f, 0), new Vector3(.172f, .003f, .172f), "Wrap", null, 12);
        }

        private static void Dolly(Transform p, int lod)
        {
            Box(p, "steel loading toe", new Vector3(0, .068f, -.125f), new Vector3(.60f, .042f, .29f), "Metal");
            for (int x = -1; x <= 1; x += 2)
            {
                Bar(p, "trolley frame upright", new Vector3(x * .223f, .1f, .075f), new Vector3(x * .223f, 1.09f, .14f), .031f, "Safety", Sides(lod));
                Cylinder(p, "solid dolly wheel", new Vector3(x * .25f, .125f, .13f), new Vector3(.25f, .10f, .25f), "Rubber", new Vector3(0, 0, 90), Sides(lod));
                if (lod < 2)
                    Cylinder(p, "wheel hub", new Vector3(x * .305f, .125f, .13f), new Vector3(.075f, .012f, .075f), "Metal", new Vector3(0, 0, 90), Sides(lod));
            }
            Bar(p, "top steering bar", new Vector3(-.223f, 1.09f, .14f), new Vector3(.223f, 1.09f, .14f), .037f, "Safety", Sides(lod));
            for (int i = 0; i < (lod == 2 ? 2 : 4); i++)
                Bar(p, "load support crossbar", new Vector3(-.223f, .27f + i * .21f, .09f), new Vector3(.223f, .27f + i * .21f, .09f), .025f, "Metal", Sides(lod));
            if (lod == 2) return;
            Bar(p, "axle", new Vector3(-.29f, .125f, .13f), new Vector3(.29f, .125f, .13f), .025f, "Metal", Sides(lod));
            Bar(p, "rubber hand grip", new Vector3(-.15f, 1.09f, .14f), new Vector3(.15f, 1.09f, .14f), .043f, "Rubber", Sides(lod));
            Box(p, "cargo retaining strap", new Vector3(.025f, .41f, .058f), new Vector3(.042f, .48f, .011f), "Fabric", new Vector3(0, 0, -9));
            if (lod != 0) return;
            Box(p, "strap buckle", new Vector3(.063f, .37f, .05f), new Vector3(.053f, .065f, .023f), "Metal");
            Box(p, "crew inventory label", new Vector3(.15f, .48f, .067f), new Vector3(.09f, .054f, .003f), "Paper");
        }
    }
}

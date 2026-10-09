using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Small, metre-space salvage props shared by the depot's authored stations.</summary>
    public static class HqSalvageProps
    {
        public static Transform At(string name, Transform parent, Vector3 position, float yaw = 0f)
        {
            Transform prop = Group(name, parent);
            prop.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            return prop;
        }

        public static void Label(Transform parent, string text, Vector3 position, float size = .045f,
            Color? color = null, float yaw = 0f)
        {
            TextMesh label = new GameObject("Label_" + text.Split('\n')[0]).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = size;
            label.color = color ?? new Color(.17f, .16f, .12f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf");
            WorldTextMaterial.Apply(label, font);
        }

        public static GameObject Rod(string name, Transform parent, Vector3 from, Vector3 to,
            float diameter, Material material, bool solid = false)
        {
            GameObject rod = Primitive(PrimitiveType.Cylinder, name, parent, (from + to) * .5f,
                new Vector3(diameter, Vector3.Distance(from, to) * .5f, diameter), material, solid);
            rod.transform.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
            return rod;
        }

        public static GameObject Bar(string name, Transform parent, Vector3 from, Vector3 to,
            float width, float depth, Material material)
        {
            GameObject bar = Box(name, parent, (from + to) * .5f,
                new Vector3(width, Vector3.Distance(from, to), depth), material, false);
            bar.transform.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
            return bar;
        }

        public static Transform Crate(Transform parent, Vector3 floor, Vector3 size, string contents, float yaw = 0f)
        {
            Transform crate = At("WoodenCrate_" + contents.Replace('\n', '_'), parent, floor, yaw);
            Material wood = CustomMallArt.Material("Wood"), dark = CustomMallArt.Material("Stain");
            Box("PackedCrate", crate, Vector3.up * size.y * .5f, size, wood);
            for (int i = 1; i < 5; i++)
            {
                float y = size.y * i / 5f;
                foreach (float side in new[] { -1f, 1f })
                {
                    Box("PlankJoint", crate, new Vector3(0f, y, side * (size.z * .5f + .002f)),
                        new Vector3(size.x, .009f, .004f), dark, false);
                    Box("EndJoint", crate, new Vector3(side * (size.x * .5f + .002f), y, 0f),
                        new Vector3(.004f, .009f, size.z), dark, false);
                }
            }
            foreach (float side in new[] { -1f, 1f })
            {
                Box("CornerBatten", crate, new Vector3(side * size.x * .41f, size.y * .5f, -size.z * .5f - .024f),
                    new Vector3(.095f, size.y, .05f), wood, false);
                Box("LidBatten", crate, new Vector3(side * size.x * .32f, size.y + .023f, 0f),
                    new Vector3(.09f, .045f, size.z), wood, false);
                for (int nail = 0; nail < 3; nail++)
                    Primitive(PrimitiveType.Sphere, "Nail", crate,
                        new Vector3(side * size.x * .41f, size.y * (.13f + nail * .35f), -size.z * .5f - .052f),
                        Vector3.one * .017f, CustomMallArt.Material("Metal"), false);
            }
            Box("CargoStencil", crate, new Vector3(0f, size.y * .57f, -size.z * .5f - .009f),
                new Vector3(size.x * .62f, size.y * .4f, .012f), CustomMallArt.Material("Paper"), false);
            Label(crate, contents, new Vector3(0f, size.y * .57f, -size.z * .5f - .018f), .035f);
            return crate;
        }

        public static Transform Bin(Transform parent, Vector3 floor, string contents, int color = 0, float yaw = 0f)
        {
            Transform bin = At("StorageBin_" + contents.Replace('\n', '_'), parent, floor, yaw);
            Color tint = color == 1 ? new Color(.31f, .36f, .24f) : color == 2 ? new Color(.43f, .28f, .15f) : new Color(.19f, .29f, .31f);
            Material plastic = PolishAssets.Material("HQ_Bin_" + color, tint, .94f);
            Box("PlasticTub", bin, new Vector3(0f, .21f, 0f), new Vector3(.62f, .42f, .46f), plastic);
            Box("Lid", bin, new Vector3(0f, .44f, 0f), new Vector3(.66f, .055f, .5f), plastic, false);
            Box("HandleRecess", bin, new Vector3(0f, .33f, -.236f), new Vector3(.18f, .045f, .01f), CustomMallArt.Material("Rubber"), false);
            Box("InventoryLabel", bin, new Vector3(0f, .19f, -.24f), new Vector3(.37f, .13f, .015f), CustomMallArt.Material("Paper"), false);
            Label(bin, contents, new Vector3(0f, .19f, -.251f), .027f);
            return bin;
        }

        public static Transform Shelf(Transform parent, Vector3 floor, float width, float height, float depth, float yaw = 0f)
        {
            Transform shelf = At("RustedIndustrialShelving", parent, floor, yaw);
            Material rust = CustomMallArt.Material("Rust"), metal = CustomMallArt.Material("Metal");
            foreach (float x in new[] { -width * .5f, width * .5f })
            foreach (float z in new[] { -depth * .5f, depth * .5f })
            {
                Box("AngleUpright", shelf, new Vector3(x, height * .5f, z), new Vector3(.09f, height, .09f), rust);
                for (int i = 0; i < 5; i++)
                    Box("BoltHole", shelf, new Vector3(x, .34f + i * .32f, z - .046f), new Vector3(.026f, .03f, .004f), CustomMallArt.Material("Black"), false);
            }
            for (int level = 0; level < 3; level++)
            {
                float y = .13f + level * (height - .27f) * .5f;
                Box("ShelfDeck", shelf, new Vector3(0f, y, 0f), new Vector3(width, .065f, depth), metal);
                Box("RustLip", shelf, new Vector3(0f, y - .018f, -depth * .5f), new Vector3(width, .1f, .045f), rust, false);
            }
            Bar("RearCrossBrace", shelf, new Vector3(-width * .48f, .18f, depth * .5f),
                new Vector3(width * .48f, height - .1f, depth * .5f), .055f, .035f, rust);
            Bar("RearCrossBrace", shelf, new Vector3(width * .48f, .18f, depth * .5f),
                new Vector3(-width * .48f, height - .1f, depth * .5f), .055f, .035f, rust);
            return shelf;
        }

        public static Transform Drum(Transform parent, Vector3 floor, string contents, float yaw = 0f)
        {
            Transform drum = At("OilDrum_" + contents.Replace('\n', '_'), parent, floor, yaw);
            Material paint = PolishAssets.Material("HQ_DrumPaint", new Color(.29f, .32f, .23f), .87f);
            Material rust = CustomMallArt.Material("Rust");
            Primitive(PrimitiveType.Cylinder, "Drum", drum, new Vector3(0f, .48f, 0f), new Vector3(.63f, .48f, .63f), paint);
            foreach (float y in new[] { .03f, .26f, .68f, .94f })
                Primitive(PrimitiveType.Cylinder, "CorrodedBand", drum, Vector3.up * y, new Vector3(.65f, .018f, .65f), rust, false);
            Primitive(PrimitiveType.Cylinder, "Bung", drum, new Vector3(.15f, .979f, .1f), new Vector3(.085f, .015f, .085f), CustomMallArt.Material("Metal"), false);
            Box("DrumLabel", drum, new Vector3(0f, .49f, -.316f), new Vector3(.34f, .23f, .014f), CustomMallArt.Material("Paper"), false);
            Label(drum, contents, new Vector3(0f, .49f, -.327f), .034f);
            for (int i = 0; i < 4; i++)
                Box("PaintWear", drum, new Vector3(-.17f + i * .1f, .79f - i * .045f, -.305f),
                    new Vector3(.037f, .15f + i * .035f, .009f), rust, false);
            return drum;
        }

        public static Transform Flashlight(Transform parent, Vector3 position, Quaternion rotation)
        {
            Transform light = At("SpareFlashlight", parent, position);
            light.localRotation = rotation;
            Material rubber = CustomMallArt.Material("Rubber"), metal = CustomMallArt.Material("Metal");
            Rod("Grip", light, Vector3.zero, Vector3.up * .24f, .055f, rubber);
            Rod("LampHead", light, Vector3.up * .235f, Vector3.up * .30f, .09f, metal);
            Rod("Lens", light, Vector3.up * .30f, Vector3.up * .305f, .074f, CustomMallArt.Material("Lens"));
            Box("Switch", light, new Vector3(0f, .19f, -.028f), new Vector3(.021f, .035f, .016f), metal, false);
            return light;
        }
    }
}

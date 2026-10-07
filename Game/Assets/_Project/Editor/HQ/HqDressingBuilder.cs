using Abandoned.Audio;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Quiet depot detail: original kit silhouettes, previous crews' belongings and failing tubes.</summary>
    public static class HqDressingBuilder
    {
        public static void Build(Transform root)
        {
            Transform decor = Group("DepotDressing", root);
            for (int x = 0; x < 6; x++)
            for (int z = 0; z < 4; z++)
            {
                Vector3 centre = new(x * 4f + 2f, 0f, z * 4f + 2f);
                CustomMallArt.Place((x + z * 3) % 7 == 0 ? "FloorDamaged" : "FloorConcrete", decor, centre, Quaternion.identity, new Vector3(1f, .3f / .18f, 1f));
                if (x >= 3)
                    CustomMallArt.Place(x == 5 && z == 3 ? "CeilingSagging" : "CeilingGrid", decor, centre + Vector3.up * 3.94f);
                if ((x * 11 + z * 7) % 9 == 0)
                    CustomMallArt.Place("FloorCrack", decor, centre + new Vector3(.5f, .007f, -.6f), 33f * z, new Vector3(.55f, 1f, .55f));
            }
            ExposedServices(decor);
            for (int z = 0; z < 4; z++)
                CustomMallArt.Place("Pillar", decor, new Vector3(11.4f, 0f, z * 4f + 1f), Quaternion.identity, new Vector3(.62f, 1.046f, .62f));

            // Heavy furniture stays along walls, leaving the crew's spawn, departure and gear lanes open.
            Prop("Bench", new Vector3(8.8f, 0f, 15.2f), 180f, new Vector3(1.2f, 1f, 1f), new Vector3(2.4f, .95f, .55f));
            Prop("Counter", new Vector3(21.7f, 0f, 15.2f), 180f, new Vector3(1.35f, 1f, 1f), new Vector3(2.86f, 1.1f, .82f));
            Prop("RetailShelf", new Vector3(23.15f, 0f, 11.8f), 270f, Vector3.one, new Vector3(1.72f, 2.12f, .52f));
            Prop("SecurityDesk", new Vector3(22.7f, 0f, 7.8f), 270f, Vector3.one, new Vector3(2f, 1.25f, 1f));
            Prop("Bench", new Vector3(13.3f, 0f, 7.8f), 90f, Vector3.one, new Vector3(2f, .95f, .55f));
            Prop("RetailShelf", new Vector3(10.3f, 0f, 15.6f), 180f, Vector3.one, new Vector3(1.72f, 2.12f, .52f));
            for (int i = 0; i < 5; i++)
            {
                CustomMallArt.Place("Suitcase", decor, new Vector3(8.3f + (i % 3) * .71f, (i / 3) * .48f, 14.2f), 17f + i * 18f);
                CustomMallArt.Place("AbandonedBackpack", decor, new Vector3(21f + (i % 3) * .55f, (i / 3) * .52f + 1.1f, 15.2f), i * 41f);
            }
            CustomMallArt.Place("Barricade", decor, new Vector3(2.5f, 0f, 15.1f), 14f);
            CustomMallArt.Place("MannequinWrong", decor, new Vector3(23.2f, 0f, 14.7f), 225f);
            CustomMallArt.Place("DebrisPile", decor, new Vector3(10.9f, .01f, 1.25f), 32f, new Vector3(.7f, .7f, .7f));
            CustomMallArt.Place("Overgrowth", decor, new Vector3(.4f, .01f, .7f), 11f, new Vector3(.5f, .6f, .5f));
            foreach (Vector3 position in new[] { new Vector3(2.3f, .008f, 1.7f), new Vector3(9.4f, .008f, 12.8f), new Vector3(22f, .008f, 11f) })
                CustomMallArt.Place("Puddle", decor, position, position.x * 13f, new Vector3(.68f, 1f, .68f));

            WallGrime(decor);
            CustomMallArt.Place("SignDepot", decor, new Vector3(6f, 3.46f, -.26f), 180f, new Vector3(2.15f, .72f, 1f));
            CustomMallArt.Place("SignExit", decor, new Vector3(6f, 3.05f, .13f), 0f, new Vector3(.65f, .7f, 1f));
            CustomMallArt.Place("SignDontGoUp", decor, new Vector3(10.1f, 2.15f, 15.86f), 180f, new Vector3(.8f, .8f, 1f));
            CustomMallArt.Place("SignQuarantine", decor, new Vector3(23.87f, 1.5f, 13.8f), 270f, new Vector3(.6f, .8f, 1f));
            CustomMallArt.Place("MissingPoster", decor, new Vector3(.14f, 1.4f, 2.25f), 90f);
            CustomMallArt.Place("MissingPoster", decor, new Vector3(14.6f, 1.35f, .15f), 0f, new Vector3(.8f, .8f, 1f));
            Caption(decor, "DEPARTURE BAY", new Vector3(8.9f, 2.8f, 15.85f), 180f, .14f);
            Caption(decor, "CONTRACTS / CREW OFFICE", new Vector3(12.18f, 2.64f, 5f), 90f, .095f);
            Caption(decor, "EQUIPMENT / REPAIRS", new Vector3(18f, 2.74f, 10.16f), 180f, .11f);
            Caption(decor, "NO CREW LEFT BEHIND", new Vector3(3.2f, 2.12f, .16f), 180f, .10f);

            Lighting(root);
            Area(root, "DepotVentilation", new Vector3(6f, 2f, 8f), new Vector3(12f, 4f, 16f), 0);
            Area(root, "OfficeElectricalHum", new Vector3(18f, 2f, 5f), new Vector3(12f, 4f, 10f), 1);
            Area(root, "WorkshopLeak", new Vector3(18f, 2f, 13f), new Vector3(12f, 4f, 6f), 2);

            void Prop(string model, Vector3 floor, float yaw, Vector3 scale, Vector3 collisionSize)
            {
                GameObject instance = CustomMallArt.Place(model, decor, floor, yaw, scale);
                Bounds bounds = ModelFit.LocalBounds(instance, instance.transform);
                var collider = instance.AddComponent<BoxCollider>();
                collider.center = bounds.center;
                collider.size = bounds.size;
            }
        }

        public static void ContractFace(Transform board)
        {
            Transform face = Group("ContractFace", board.parent);
            face.localPosition = board.localPosition + Vector3.forward * .086f;
            Caption(face, "ASHLINE / DISPATCH", new Vector3(0f, .7f, .01f), 180f, .14f);
            for (int i = 0; i < 4; i++)
            {
                float x = -.94f + i * .63f;
                GameObject paper = Box("JobSlip", face, new Vector3(x, -.10f + (i % 2) * .07f, .008f), new Vector3(.53f, .67f, .012f), CustomMallArt.Material("Paper"), false);
                paper.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? -4f : 3f);
                for (int line = 0; line < 4; line++)
                    Box("TypeLine", face, new Vector3(x, .10f - line * .08f, .017f), new Vector3(line == 0 ? .30f : .38f, .008f, .002f), CustomMallArt.Material("Stain"), false);
                Box("Pin", face, new Vector3(x, .22f, .019f), new Vector3(.03f, .03f, .013f), CustomMallArt.Material("Rust"), false);
            }
            Caption(face, "TAKE ONLY WHAT YOU CAN CARRY", new Vector3(0f, -.70f, .02f), 180f, .065f);
        }

        public static void TerminalFace(Transform terminal)
        {
            Transform face = Group("TerminalFace", terminal.parent);
            face.localPosition = terminal.localPosition;
            Box("CRTBezel", face, new Vector3(-.04f, .025f, -.16f), new Vector3(.66f, .37f, .035f), CustomMallArt.Material("Rubber"), false);
            Box("AmberCRT", face, new Vector3(-.04f, .025f, -.18f), new Vector3(.57f, .28f, .014f), PolishAssets.Material("HQ_CRTGlow", new Color(.19f, .12f, .045f), emission: .9f), false);
            Caption(face, "ASHLINE SUPPLY\nGEAR / BATTERIES\nPRESS E", new Vector3(-.04f, .022f, -.19f), 0f, .035f);
            for (int i = 0; i < 3; i++)
                Box("Switch", face, new Vector3(.33f, .075f - i * .07f, -.17f), new Vector3(.032f, .025f, .023f), CustomMallArt.Material("Metal"), false);
        }

        private static void ExposedServices(Transform decor)
        {
            Material metal = CustomMallArt.Material("Metal"), rust = CustomMallArt.Material("Rust");
            for (int z = 0; z < 4; z++)
            {
                Box("RoofTruss", decor, new Vector3(6f, 3.86f, z * 4f + 2f), new Vector3(11.8f, .22f, .13f), metal, false);
                for (int x = 0; x < 4; x++)
                {
                    GameObject brace = Box("TrussBrace", decor, new Vector3(x * 3f + 1.5f, 3.55f, z * 4f + 2f), new Vector3(3.1f, .08f, .08f), rust, false);
                    brace.transform.localRotation = Quaternion.Euler(0f, 0f, x % 2 == 0 ? 11f : -11f);
                }
            }
            Box("VentDuct", decor, new Vector3(10.1f, 3.63f, 8f), new Vector3(.48f, .36f, 15.6f), metal, false);
            for (int z = 0; z < 8; z++)
                Box("DuctBand", decor, new Vector3(10.1f, 3.63f, z * 2f + .8f), new Vector3(.51f, .39f, .04f), rust, false);
            Box("WaterMain", decor, new Vector3(23.6f, 3.60f, 8f), new Vector3(.10f, .10f, 15.6f), rust, false);
        }

        private static void WallGrime(Transform decor)
        {
            for (int i = 0; i < 6; i++)
            {
                CustomMallArt.Place("WaterStain", decor, new Vector3(i * 4f + 2f, 2.1f, 15.875f), Quaternion.Euler(-90f, 0f, 0f), new Vector3(.74f, 1f, 1.45f));
                if (i % 2 == 0)
                    CustomMallArt.Place("MouldPatch", decor, new Vector3(i * 4f + 2.7f, .4f, 15.875f), Quaternion.Euler(-90f, 0f, 0f));
            }
            for (int z = 0; z < 4; z++)
                CustomMallArt.Place("WaterStain", decor, new Vector3(.125f, 2.35f, z * 4f + 1.2f), Quaternion.Euler(0f, 0f, -90f), new Vector3(1f, 1f, 1.5f));
        }

        private static void Lighting(Transform root)
        {
            Transform lights = Group("DepotLights", root);
            Fixture(new Vector3(6f, 3.15f, 9f), new Color(1f, .71f, .42f), 6.5f, 9f, false, false, 1);
            Fixture(new Vector3(5.2f, 3.30f, 3.4f), new Color(1f, .72f, .46f), 3.6f, 8f, false, false, 2);
            Fixture(new Vector3(18f, 3.25f, 13.1f), new Color(1f, .68f, .40f), 5f, 7.5f, false, false, 3);
            Fixture(new Vector3(18.1f, 3.20f, 2.1f), new Color(.65f, .72f, .74f), 2.8f, 8f, false, false, 4);
            Fixture(new Vector3(20.8f, 3.65f, 7.4f), new Color(.61f, .70f, .74f), 1.7f, 7f, true, false, 5);
            Fixture(new Vector3(8.8f, 3.5f, 14.5f), new Color(.63f, .70f, .72f), 1.4f, 6.5f, true, false, 6);
            Fixture(new Vector3(15f, 3.72f, 7.6f), new Color(.62f, .7f, .74f), 1.5f, 7f, false, true, 7);
            Point("TerminalWarmth", new Vector3(18f, 1.7f, 13.15f), new Color(1f, .57f, .28f), .65f, 2.2f);
            Point("DoorYardLamp", new Vector3(6f, 3.15f, -1.4f), new Color(.78f, .65f, .46f), 1.3f, 5f);

            void Fixture(Vector3 at, Color color, float intensity, float range, bool faulty, bool dead, int seed)
            {
                Transform fixture = Group("Tube_" + seed, lights);
                fixture.localPosition = at;
                CustomMallArt.Place("FluorescentFixture", fixture, Vector3.zero);
                var lamp = new GameObject("RealtimeLight").AddComponent<Light>();
                lamp.transform.SetParent(fixture, false);
                lamp.transform.localPosition = Vector3.down * .13f;
                lamp.type = LightType.Point;
                lamp.color = color;
                lamp.intensity = intensity;
                lamp.range = range;
                lamp.shadows = seed is 1 or 3 ? LightShadows.Soft : LightShadows.None;
                GameObject diffuser = Box("TubeGlow", fixture, new Vector3(0f, -.091f, 0f), new Vector3(1.03f, .012f, .18f), PolishAssets.Material("HQ_TubeGlow", new Color(.45f, .37f, .24f), emission: 1f), false);
                fixture.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, diffuser.GetComponent<Renderer>(), faulty, dead, seed + 820);
            }
            void Point(string name, Vector3 at, Color color, float intensity, float range)
            {
                var light = new GameObject(name).AddComponent<Light>();
                light.transform.SetParent(lights, false);
                light.transform.localPosition = at;
                light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range;
                light.shadows = LightShadows.None;
            }
        }

        private static void Area(Transform root, string name, Vector3 centre, Vector3 size, int character)
        {
            Transform area = Group(name, root);
            area.localPosition = centre;
            area.gameObject.AddComponent<HorrorAreaAmbience>().EditorSetup(size, character);
        }

        private static void Caption(Transform parent, string text, Vector3 position, float yaw, float size)
        {
            TextMesh caption = new GameObject("Label_" + text.Split('\n')[0]).AddComponent<TextMesh>();
            caption.transform.SetParent(parent, false);
            caption.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            caption.text = text; caption.anchor = TextAnchor.MiddleCenter; caption.alignment = TextAlignment.Center;
            caption.fontSize = 64; caption.characterSize = size; caption.color = new Color(.65f, .61f, .46f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf");
            if (font == null) return;
            caption.font = font;
            caption.GetComponent<Renderer>().sharedMaterial = font.material;
        }
    }
}

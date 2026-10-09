using Abandoned.Audio;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>The depot's worn shell and services, around an active salvage operation.</summary>
    public static class HqDressingBuilder
    {
        public static void Build(Transform root)
        {
            Transform decor = Group("CompanyOperationsDressing", root);
            ExposedServices(decor);
            HqSalvageDressing.Build(decor);
            HqCompanyRooms.Build(decor);
            Area(root, "WarehouseVentilation", new Vector3(12f, 3.2f, 14f), new Vector3(24f, 6.4f, 28f), 0);
            Area(root, "DispatchElectricalHum", new Vector3(30f, 1.9f, 5f), new Vector3(12f, 3.8f, 10f), 1);
            Area(root, "WorkshopVentilation", new Vector3(31.5f, 1.9f, 23f), new Vector3(9f, 3.8f, 10f), 2);
        }

        public static void ContractFace(Transform board)
        {
            Transform face = Group("ContractFace", board.parent);
            face.localPosition = board.localPosition + Vector3.forward * .086f;
            Caption(face, "ABANDONED SALVAGING / DISPATCH", new Vector3(0f, 1.364f, .01f), 180f, .11f);
            for (int i = 0; i < 4; i++)
            {
                float x = -.94f + i * .63f;
                GameObject paper = Box("JobSlip", face, new Vector3(x, -.10f + (i % 2) * .07f, .008f), new Vector3(.53f, .67f, .012f), CustomMallArt.Material("Paper"), false);
                paper.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? -4f : 3f);
                for (int line = 0; line < 4; line++)
                    Box("TypeLine", face, new Vector3(x, .10f - line * .08f, .017f), new Vector3(line == 0 ? .30f : .38f, .008f, .002f), CustomMallArt.Material("Stain"), false);
                Box("Pin", face, new Vector3(x, .22f, .019f), new Vector3(.03f, .03f, .013f), CustomMallArt.Material("Rust"), false);
            }
            Caption(face, "TAKE ONLY WHAT YOU CAN CARRY", new Vector3(0f, -1.091f, .02f), 180f, .065f);
        }

        public static void TerminalFace(Transform terminal)
        {
            Transform face = Group("TerminalFace", terminal.parent);
            face.localPosition = terminal.localPosition;
            Box("CRTBezel", face, new Vector3(-.04f, .025f, -.16f), new Vector3(.66f, .37f, .035f), CustomMallArt.Material("Rubber"), false);
            Box("AmberCRT", face, new Vector3(-.04f, .025f, -.18f), new Vector3(.57f, .28f, .014f), PolishAssets.Material("HQ_CRTGlow", new Color(.19f, .12f, .045f), emission: .9f), false);
            Caption(face, "ABANDONED SUPPLY\nGEAR / BATTERIES\nPRESS E", new Vector3(-.04f, -.677f, -.464f), 0f, .035f);
            for (int i = 0; i < 3; i++)
                Box("Switch", face, new Vector3(.33f, .075f - i * .07f, -.17f), new Vector3(.032f, .025f, .023f), CustomMallArt.Material("Metal"), false);
        }

        private static void ExposedServices(Transform decor)
        {
            Material metal = CustomMallArt.Material("Metal"), rust = CustomMallArt.Material("Rust");
            Box("WarehouseVentDuct", decor, new Vector3(22.95f, 5.72f, 14f), new Vector3(.48f, .42f, 27.5f), metal, false);
            for (int z = 0; z < 14; z++)
                Box("DuctBand", decor, new Vector3(22.95f, 5.72f, z * 2f + .5f), new Vector3(.53f, .47f, .045f), rust, false);
            Box("ServiceWingCableTray", decor, new Vector3(25.5f, 3.4f, 18.5f), new Vector3(.35f, .12f, 18.7f), metal, false);
            Box("WorkshopWaterMain", decor, new Vector3(35.7f, 3.35f, 23f), new Vector3(.09f, .09f, 9.8f), rust, false);
            Box("ExtractorBranch", decor, new Vector3(31.5f, 3.42f, 25.9f), new Vector3(8.1f, .28f, .32f), metal, false);
            Box("ExtractorHood", decor, new Vector3(31f, 3.14f, 26.7f), new Vector3(2.2f, .25f, .75f), metal, false);
            MarkStatic(decor);
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
            WorldTextMaterial.Apply(caption, font);
        }
    }
}

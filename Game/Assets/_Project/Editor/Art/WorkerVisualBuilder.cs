using System.Collections.Generic;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>An original faceted salvage worker with boots, gloves, coveralls, reflective bands and a readable face.</summary>
    public static class WorkerVisualBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Resources/WorkerVisual.prefab";
        public static Material Suit => PolishAssets.Material("Crew_Coverall", new Color(0.88f, 0.39f, 0.085f));
        public static Material Dark => PolishAssets.Material("Crew_BootsGloves", new Color(0.105f, 0.12f, 0.13f));
        public static Material Skin => PolishAssets.Material("Crew_Face", new Color(0.66f, 0.49f, 0.34f));
        public static Material Tape => PolishAssets.Material("Crew_ReflectiveTape", new Color(0.78f, 0.79f, 0.69f), emission: 0.12f);

        public static GameObject Build(Transform parent)
        {
            var root = new GameObject("Body");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * 0.95f; // Pelvis pivot also works for the remote laid-down pose.
            Material suit = Suit, dark = Dark, skin = Skin, tape = Tape;
            var coverall = new List<Renderer>();
            GameObject Cloth(string name, Transform owner, Vector3 pos, Vector3 size)
            {
                GameObject shape = PolishAssets.Shape(name, owner, pos, size, suit);
                coverall.Add(shape.GetComponent<Renderer>());
                return shape;
            }
            Cloth("Hips", root.transform, Vector3.zero, new Vector3(0.36f, 0.24f, 0.25f));
            Transform torso = GreyboxFactory.Group("Torso", root.transform);
            torso.localPosition = Vector3.up * 0.15f;
            Cloth("Jacket", torso, new Vector3(0f, 0.15f, 0f), new Vector3(0.46f, 0.44f, 0.29f));
            GreyboxFactory.Box("Zip", torso, new Vector3(0f, 0.16f, 0.149f), new Vector3(0.015f, 0.32f, 0.012f), dark, false);
            foreach (float side in new[] { -1f, 1f })
            {
                GreyboxFactory.Box("ChestPocket", torso, new Vector3(side * 0.13f, 0.22f, 0.15f), new Vector3(0.095f, 0.09f, 0.018f), suit, false);
                GreyboxFactory.Box("ShoulderTape", torso, new Vector3(side * 0.16f, 0.3f, 0.15f), new Vector3(0.055f, 0.2f, 0.018f), tape, false);
            }
            GreyboxFactory.Box("Belt", root.transform, new Vector3(0f, 0.07f, 0f), new Vector3(0.37f, 0.055f, 0.27f), dark, false);
            GreyboxFactory.Box("Buckle", root.transform, new Vector3(0f, 0.07f, 0.14f), new Vector3(0.06f, 0.045f, 0.018f), tape, false);
            NumberBadge(torso, new Vector3(0.12f, 0.22f, 0.171f));
            Transform head = GreyboxFactory.Group("Head", torso);
            head.localPosition = new Vector3(0f, 0.47f, 0f);
            PolishAssets.Shape("Face", head, Vector3.zero, new Vector3(0.27f, 0.29f, 0.25f), skin, true);
            PolishAssets.Shape("Hair", head, new Vector3(0f, 0.1f, -0.025f), new Vector3(0.29f, 0.1f, 0.25f), dark, true);
            GreyboxFactory.Box("Brow", head, new Vector3(0f, 0.035f, 0.12f), new Vector3(0.19f, 0.023f, 0.02f), dark, false);
            foreach (float side in new[] { -1f, 1f })
                GreyboxFactory.Box("Eye", head, new Vector3(side * 0.06f, 0.005f, 0.125f), new Vector3(0.024f, 0.024f, 0.013f), dark, false);
            Transform crown = GreyboxFactory.Group("HeadAnchor", head);
            crown.localPosition = new Vector3(0f, 0.12f, 0f);

            var arms = new Transform[2]; var legs = new Transform[2]; var knees = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Transform arm = arms[i] = GreyboxFactory.Group(i == 0 ? "ArmL" : "ArmR", torso);
                arm.localPosition = new Vector3(side * 0.28f, 0.29f, 0f);
                Cloth("Sleeve", arm, new Vector3(0f, -0.24f, 0f), new Vector3(0.15f, 0.49f, 0.16f));
                PolishAssets.Shape("CuffTape", arm, new Vector3(0f, -0.41f, 0f), new Vector3(0.156f, 0.04f, 0.166f), tape);
                PolishAssets.Shape("Glove", arm, new Vector3(0f, -0.52f, 0.015f), new Vector3(0.14f, 0.14f, 0.17f), dark);
                Transform leg = legs[i] = GreyboxFactory.Group(i == 0 ? "LegL" : "LegR", root.transform);
                leg.localPosition = new Vector3(side * 0.11f, -0.07f, 0f);
                Cloth("Thigh", leg, new Vector3(0f, -0.18f, 0f), new Vector3(0.19f, 0.36f, 0.21f));
                Transform knee = knees[i] = GreyboxFactory.Group("Knee", leg);
                knee.localPosition = Vector3.down * 0.36f;
                Cloth("Shin", knee, new Vector3(0f, -0.2f, 0f), new Vector3(0.18f, 0.4f, 0.2f));
                PolishAssets.Shape("AnkleTape", knee, new Vector3(0f, -0.26f, 0f), new Vector3(0.186f, 0.045f, 0.206f), tape);
                GreyboxFactory.Box("Boot", knee, new Vector3(0f, -0.42f, 0.045f), new Vector3(0.2f, 0.18f, 0.32f), dark, false);
            }
            coverall.Clear();
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
                if (renderer.sharedMaterial == suit) coverall.Add(renderer);
            root.AddComponent<WorkerRig>().EditorSetup(arms[0], arms[1], legs[0], legs[1], knees[0], knees[1], torso, crown, coverall.ToArray());
            return root;
        }

        public static void SavePreviewPrefab()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Prefabs/Resources");
            GameObject worker = Build(null);
            worker.transform.localPosition = Vector3.zero;
            PrefabUtility.SaveAsPrefabAsset(worker, PrefabPath);
            Object.DestroyImmediate(worker);
        }

        public static void NumberBadge(Transform parent, Vector3 position)
        {
            var label = new GameObject("CrewNumber").AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.065f;
            label.fontSize = 48;
            label.text = "1";
            label.color = new Color(0.95f, 0.94f, 0.85f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-ExtraBold.ttf");
            WorldTextMaterial.Apply(label, font);
        }
    }
}

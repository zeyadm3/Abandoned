using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Tailored salvage workwear and fitted protection on the existing procedural worker rig.</summary>
    public static class WorkerVisualBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Resources/WorkerVisual.prefab";
        public static Material Suit => CharacterSurfaceLibrary.Get("CrewFabric");
        public static Material Dark => CharacterSurfaceLibrary.Get("Rubber");
        public static Material Skin => CharacterSurfaceLibrary.Get("Skin");
        public static Material Tape => CharacterSurfaceLibrary.Get("Tape");

        public static GameObject Build(Transform parent)
        {
            CharacterMaterialBuilder.Prepare();
            var root = new GameObject("Body");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * .95f;
            WorkerClothingDetails.Hips(GreyboxFactory.Group("HipsVisual", root.transform));
            Transform torso = GreyboxFactory.Group("Torso", root.transform);
            torso.localPosition = Vector3.up * .15f;
            WorkerClothingDetails.Torso(GreyboxFactory.Group("TorsoVisual", torso));
            Transform head = GreyboxFactory.Group("Head", torso);
            head.localPosition = new Vector3(0f, .47f, 0f);
            WorkerHeadDetails.Build(GreyboxFactory.Group("HeadVisual", head));
            Transform crown = GreyboxFactory.Group("HeadAnchor", head);
            crown.localPosition = new Vector3(0f, .12f, 0f);

            var arms = new Transform[2]; var legs = new Transform[2]; var knees = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                arms[i] = GreyboxFactory.Group(i == 0 ? "ArmL" : "ArmR", torso);
                arms[i].localPosition = new Vector3(side * .28f, .29f, 0f);
                WorkerClothingDetails.Arm(GreyboxFactory.Group("ArmVisual", arms[i]), side);
                legs[i] = GreyboxFactory.Group(i == 0 ? "LegL" : "LegR", root.transform);
                legs[i].localPosition = new Vector3(side * .11f, -.07f, 0f);
                WorkerClothingDetails.Thigh(GreyboxFactory.Group("ThighVisual", legs[i]), side);
                knees[i] = GreyboxFactory.Group("Knee", legs[i]);
                knees[i].localPosition = Vector3.down * .36f;
                WorkerClothingDetails.Shin(GreyboxFactory.Group("ShinVisual", knees[i]), side);
            }
            var coverall = new List<Renderer>();
            Material suit = Suit;
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
            WorkerViewModelBuilder.Save();
        }

        public static void NumberBadge(Transform parent, Vector3 position)
        {
            var label = new GameObject("CrewNumber").AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = .047f;
            label.fontSize = 48;
            label.text = "1";
            label.color = new Color(.95f, .94f, .85f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-ExtraBold.ttf");
            WorldTextMaterial.Apply(label, font);
        }
    }
}

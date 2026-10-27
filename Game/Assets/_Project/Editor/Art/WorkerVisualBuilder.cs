using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The crew worker (2026-10-22 redesign): a stocky salvage labourer in a team-coloured ripstop jacket,
    /// plate-carrier vest, cargo trousers and work boots. Source: Tools/Blender/crew (crew_export.py writes one
    /// FBX per joint into Art/Custom/Characters/Crew). Parts hang from WorkerRig's joints so WorkerPresentation
    /// poses them; the jacket uses the Suit material, so the coverall colour tints it.
    /// </summary>
    public static class WorkerVisualBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Resources/WorkerVisual.prefab";
        public const string ArtFolder = "Assets/_Project/Art/Custom/Characters/Crew";
        public static Material Suit => CharacterSurfaceLibrary.Get("CrewFabric");
        public static Material Dark => CharacterSurfaceLibrary.Get("Rubber");
        public static Material Skin => CharacterSurfaceLibrary.Get("Skin");
        public static Material Tape => CharacterSurfaceLibrary.Get("Tape");

        // Joints, local to their parent (Unity: +Z forward, the worker's left is -X). Eyes at 1.65 m.
        private const float BodyY = .95f, TorsoY = .15f, HeadY = .42f, CrownY = .25f;
        private static readonly Vector3 Shoulder = new(.19f, .315f, 0f), Hip = new(.102f, -.05f, 0f);
        private const float UpperArm = .305f, Thigh = .40f;

        public static GameObject Build(Transform parent)
        {
            CharacterMaterialBuilder.Prepare();
            var root = new GameObject("Body");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.up * BodyY;
            // 2026 remake: one skinned head and outfit on the joints; the rigid per-joint parts stay as the fallback
            // until the remake looks exist (Tools/Blender/remake/crew, then Build Crew Looks).
            bool skinned = Remake.CrewLookBuilder.Available && HasLooks();
            if (!skinned) Part("HipsVisual", root.transform, "Hips");
            Transform torso = GreyboxFactory.Group("Torso", root.transform);
            torso.localPosition = Vector3.up * TorsoY;
            if (!skinned)
            {
                Transform torsoVisual = Part("TorsoVisual", torso, "Torso").transform;
                // Crew numbers: the back panel, the vest's ID patch, each shoulder pocket (WorkerPresentation fills them in).
                // They sit in the visual groups so a ragdoll's copy keeps them.
                NumberBadge(torsoVisual, new Vector3(0f, .133f, -.239f), 0f, .07f);
                NumberBadge(torsoVisual, new Vector3(-.045f, .253f, .2135f), 180f, .035f);
            }
            Transform head = GreyboxFactory.Group("Head", torso);
            head.localPosition = Vector3.up * HeadY;
            if (!skinned) Part("HeadVisual", head, "Head");
            Transform crown = GreyboxFactory.Group("HeadAnchor", head);
            crown.localPosition = Vector3.up * CrownY;

            var arms = new Transform[2]; var elbows = new Transform[2]; var legs = new Transform[2]; var knees = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                string n = i == 0 ? "L" : "R";
                arms[i] = GreyboxFactory.Group("Arm" + n, torso);
                arms[i].localPosition = new Vector3(side * Shoulder.x, Shoulder.y, 0f);
                if (!skinned)
                {
                    Transform armVisual = Part("ArmVisual", arms[i], "UpperArm" + n).transform;
                    NumberBadge(armVisual, new Vector3(side * .0732f, -.10f, .004f), side < 0f ? 90f : -90f, .03f);
                }
                elbows[i] = GreyboxFactory.Group("Elbow", arms[i]);
                elbows[i].localPosition = Vector3.down * UpperArm;
                if (!skinned) Part("ForearmVisual", elbows[i], "Forearm" + n);
                legs[i] = GreyboxFactory.Group("Leg" + n, root.transform);
                legs[i].localPosition = new Vector3(side * Hip.x, Hip.y, 0f);
                if (!skinned) Part("ThighVisual", legs[i], "Thigh" + n);
                knees[i] = GreyboxFactory.Group("Knee", legs[i]);
                knees[i].localPosition = Vector3.down * Thigh;
                if (!skinned) Part("ShinVisual", knees[i], "Shin" + n);
            }
            if (skinned)
                Remake.CrewLookBuilder.AddLook(root, root.transform, Vector3.down * BodyY,
                    new[] { root.transform, torso, head, arms[0], elbows[0], arms[1], elbows[1], legs[0], knees[0], legs[1], knees[1] });
            var coverall = new List<Renderer>();
            Material suit = Suit;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
                if (renderer.sharedMaterial == suit) coverall.Add(renderer);
            root.AddComponent<WorkerRig>().EditorSetup(arms[0], arms[1], legs[0], legs[1], knees[0], knees[1], torso, crown,
                coverall.ToArray(), elbows[0], elbows[1]);
            return root;
        }

        /// <summary>The catalog has the remake's characters and outfits (Build Crew Looks has run).</summary>
        public static bool HasLooks()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(CosmeticsBuilder.CatalogPath);
            return catalog != null && catalog.Characters.Count > 0 && catalog.Outfits.Count > 0;
        }

        /// <summary>One exported joint model under a named group; each mesh gets the project material for its key.</summary>
        public static GameObject Part(string name, Transform parent, string model)
        {
            Transform group = GreyboxFactory.Group(name, parent);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{ArtFolder}/Crew_{model}.fbx");
            if (source == null)
            {
                Debug.LogError($"Crew model missing: {ArtFolder}/Crew_{model}.fbx (run Tools/Blender/crew/crew_export.py).");
                return group.gameObject;
            }
            GameObject copy = Object.Instantiate(source, group, false);
            copy.name = model;
            Dress(copy);
            return group.gameObject;
        }

        /// <summary>Assigns materials by the mesh name's key ("Torso_Vest" -> Vest).</summary>
        public static void Dress(GameObject model)
        {
            foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>(true))
            {
                string key = renderer.name.Contains("_") ? renderer.name.Substring(renderer.name.LastIndexOf('_') + 1) : renderer.name;
                int dot = key.IndexOf('.');
                if (dot >= 0) key = key.Substring(0, dot);
                renderer.sharedMaterials = new[] { Material(key) };
            }
        }

        public static Material Material(string key)
        {
            CharacterMaterialBuilder.Prepare();
            return key switch
            {
                "Suit" => Suit,
                "Trim" => CharacterSurfaceLibrary.Get("CrewTrim"),
                "Rubber" => Dark,
                "Tape" => Tape,
                "Steel" => CharacterSurfaceLibrary.Get("Steel"),
                "Skin" => Skin,
                "Vest" => CharacterMaterialBuilder.Tinted("Canvas", new Color(.2f, .205f, .19f), "CrewVest"),
                "Webbing" => CharacterMaterialBuilder.Tinted("CrewTrim", new Color(.075f, .075f, .072f), "CrewWebbing"),
                "Trousers" => CharacterMaterialBuilder.Tinted("Canvas", new Color(.34f, .31f, .25f), "CrewTrousers"),
                "Glove" => CharacterMaterialBuilder.Tinted("Leather", new Color(.06f, .06f, .06f), "CrewGlove"),
                "GlovePalm" => CharacterMaterialBuilder.Tinted("Leather", new Color(.32f, .3f, .27f), "CrewGlovePalm"),
                "Boot" => CharacterMaterialBuilder.Tinted("Leather", new Color(.2f, .12f, .07f), "CrewBoot"),
                "Lace" => CharacterMaterialBuilder.Tinted("Canvas", new Color(.16f, .12f, .08f), "CrewLace"),
                "ChemLight" => CharacterMaterialBuilder.Tinted("Screen", new Color(.35f, .95f, .3f), "CrewChemLight"),
                "Head" => Baked("CrewHead", "CrewHead_BaseColor"),
                "Eye" => Baked("CrewEye", "CrewEye_BaseColor"),
                _ => Dark,
            };
        }

        // The face (crew cut, beard and brows painted on) and the eyes are baked colour maps; no tiling skin normal.
        private static Material Baked(string name, string texture)
        {
            Material m = CharacterMaterialBuilder.Tinted("Skin", Color.white, name);
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtFolder}/{texture}.png"));
            m.SetTexture("_BumpMap", null);
            m.DisableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(m);
            return m;
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

        /// <summary>A crew-seat number in stencil lettering; yaw turns it to face its reader (0 = seen from behind).</summary>
        public static void NumberBadge(Transform parent, Vector3 position, float yaw = 180f, float size = .047f)
        {
            var label = new GameObject("CrewNumber").AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;
            label.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = size;
            label.fontSize = 48;
            label.text = "1";
            label.color = new Color(.84f, .8f, .7f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/SairaStencilOne/SairaStencilOne-Regular.ttf");
            WorldTextMaterial.Apply(label, font);
        }
    }
}

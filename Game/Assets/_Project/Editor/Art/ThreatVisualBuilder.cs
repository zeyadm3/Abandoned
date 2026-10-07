using Abandoned.Threats;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Four original silhouettes: eyeless listener, narrow suspended coat, hunched collector, broad stone hunter.</summary>
    public static class ThreatVisualBuilder
    {
        public static GameObject Upgrade(string path, ThreatPresentation.Silhouette kind)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform old = root.transform.Find("Visual");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                foreach (ThreatPresentation component in root.GetComponents<ThreatPresentation>()) Object.DestroyImmediate(component);
                Build(root, kind);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static Transform Joint(string name, Transform parent, Vector3 at)
        {
            Transform t = Group(name, parent);
            t.localPosition = at;
            return t;
        }

        public static void Build(GameObject root, ThreatPresentation.Silhouette kind)
        {
            Transform visual = Group("Visual", root.transform);
            Transform armL, armR, legL, legR, head;
            if (kind == ThreatPresentation.Silhouette.BlindOne)
            {
                Material pale = PolishAssets.Material("Threat_Listener", new Color(0.66f, 0.67f, 0.6f));
                Material seam = PolishAssets.Material("Threat_ListenerSeam", new Color(0.19f, 0.23f, 0.22f));
                PolishAssets.Shape("RibbedChest", visual, new Vector3(0f, 1.36f, 0f), new Vector3(0.6f, 0.72f, 0.38f), pale);
                PolishAssets.Shape("Pelvis", visual, new Vector3(0f, 0.94f, 0f), new Vector3(0.35f, 0.27f, 0.28f), seam);
                head = Joint("Head", visual, new Vector3(0f, 2.04f, 0.025f));
                PolishAssets.Shape("FeaturelessSkull", head, Vector3.zero, new Vector3(0.44f, 0.61f, 0.39f), pale, true);
                // Broad ear-like listening fins remain recognizable even when the face is lost to fog.
                PolishAssets.Shape("ListeningFinL", head, new Vector3(-0.27f, 0.035f, 0f), new Vector3(0.15f, 0.36f, 0.14f), pale);
                PolishAssets.Shape("ListeningFinR", head, new Vector3(0.27f, 0.035f, 0f), new Vector3(0.15f, 0.36f, 0.14f), pale);
                Box("MouthSeam", head, new Vector3(0f, -0.16f, 0.186f), new Vector3(0.22f, 0.028f, 0.024f), seam, false);
                for (int i = 0; i < 3; i++) Box("ChestRidge", visual, new Vector3(0f, 1.26f + i * 0.14f, 0.191f), new Vector3(0.34f, 0.035f, 0.024f), seam, false);
                armL = Limb("ArmL", visual, new Vector3(-0.37f, 1.67f, 0f), new Vector3(0.14f, 1.13f, 0.15f), pale);
                armR = Limb("ArmR", visual, new Vector3(0.37f, 1.67f, 0f), new Vector3(0.14f, 1.13f, 0.15f), pale);
                legL = Limb("LegL", visual, new Vector3(-0.11f, 0.95f, 0f), new Vector3(0.15f, 0.92f, 0.17f), pale);
                legR = Limb("LegR", visual, new Vector3(0.11f, 0.95f, 0f), new Vector3(0.15f, 0.92f, 0.17f), pale);
            }
            else if (kind == ThreatPresentation.Silhouette.Stalker)
            {
                Material coat = PolishAssets.Material("Threat_StalkerCoat", new Color(0.15f, 0.2f, 0.22f));
                Material hood = PolishAssets.Material("Threat_StalkerHood", new Color(0.07f, 0.095f, 0.1f));
                Material eyes = PolishAssets.Material("Threat_StalkerEyes", new Color(0.71f, 0.83f, 0.73f), emission: 0.55f);
                PolishAssets.Shape("LongCoat", visual, new Vector3(0f, 1.23f, 0f), new Vector3(0.44f, 1.55f, 0.28f), coat);
                head = Joint("Head", visual, new Vector3(0f, 2.28f, 0f));
                PolishAssets.Shape("PointedHood", head, new Vector3(0f, 0.03f, 0f), new Vector3(0.35f, 0.63f, 0.31f), hood);
                Box("NarrowEyes", head, new Vector3(0f, -0.02f, 0.165f), new Vector3(0.17f, 0.022f, 0.022f), eyes, false);
                // A long neck, squared collar and hanging arms distinguish it from the listener.
                Box("RaisedCollar", visual, new Vector3(0f, 1.92f, 0f), new Vector3(0.52f, 0.1f, 0.25f), coat, false);
                armL = Limb("ArmL", visual, new Vector3(-0.28f, 1.86f, 0f), new Vector3(0.1f, 1.43f, 0.12f), coat);
                armR = Limb("ArmR", visual, new Vector3(0.28f, 1.86f, 0f), new Vector3(0.1f, 1.43f, 0.12f), coat);
                legL = Limb("LegL", visual, new Vector3(-0.1f, 0.51f, 0f), new Vector3(0.1f, 0.51f, 0.14f), hood);
                legR = Limb("LegR", visual, new Vector3(0.1f, 0.51f, 0f), new Vector3(0.1f, 0.51f, 0.14f), hood);
            }
            else if (kind == ThreatPresentation.Silhouette.Collector)
            {
                Material cloth = PolishAssets.Material("Threat_CollectorCloth", new Color(0.43f, 0.38f, 0.23f));
                Material bag = PolishAssets.Material("Threat_CollectorBag", new Color(0.61f, 0.51f, 0.31f));
                Material mask = PolishAssets.Material("Threat_CollectorMask", new Color(0.21f, 0.26f, 0.23f));
                PolishAssets.Shape("HunchedBody", visual, new Vector3(0f, 0.66f, -0.08f), new Vector3(0.63f, 0.75f, 0.55f), cloth, true);
                PolishAssets.Shape("OversizedSack", visual, new Vector3(0f, 0.9f, -0.37f), new Vector3(0.73f, 0.69f, 0.6f), bag, true);
                head = Joint("Head", visual, new Vector3(0f, 1.08f, 0.25f));
                PolishAssets.Shape("RoundMask", head, Vector3.zero, new Vector3(0.37f, 0.35f, 0.29f), mask, true);
                Box("MaskBand", head, new Vector3(0f, 0f, 0.145f), new Vector3(0.28f, 0.035f, 0.03f), bag, false);
                armL = Limb("ArmL", visual, new Vector3(-0.33f, 0.92f, 0.1f), new Vector3(0.13f, 0.6f, 0.17f), cloth);
                armR = Limb("ArmR", visual, new Vector3(0.33f, 0.92f, 0.1f), new Vector3(0.13f, 0.6f, 0.17f), cloth);
                legL = Limb("LegL", visual, new Vector3(-0.16f, 0.37f, 0f), new Vector3(0.16f, 0.37f, 0.21f), mask);
                legR = Limb("LegR", visual, new Vector3(0.16f, 0.37f, 0f), new Vector3(0.16f, 0.37f, 0.21f), mask);
                for (int i = 0; i < 3; i++) PolishAssets.Shape("HangingToken", visual, new Vector3(-0.15f + i * 0.15f, 0.6f, 0.28f), new Vector3(0.06f, 0.08f, 0.025f), bag);
            }
            else
            {
                Material stone = PolishAssets.Material("Threat_HunterStone", new Color(0.39f, 0.37f, 0.3f));
                Material plate = PolishAssets.Material("Threat_HunterPlate", new Color(0.2f, 0.25f, 0.26f));
                Material eyes = PolishAssets.Material("Threat_HunterEyes", new Color(0.82f, 0.45f, 0.14f), emission: 0.4f);
                PolishAssets.Shape("MassiveTorso", visual, new Vector3(0f, 1.36f, 0f), new Vector3(1.13f, 1.3f, 0.79f), stone);
                Box("ShoulderPlate", visual, new Vector3(0f, 1.94f, 0f), new Vector3(1.32f, 0.25f, 0.69f), plate, false);
                head = Joint("Head", visual, new Vector3(0f, 2.18f, 0.17f));
                PolishAssets.Shape("SmallBuriedHead", head, Vector3.zero, new Vector3(0.4f, 0.39f, 0.36f), plate);
                Box("Eyes", head, new Vector3(0f, 0.015f, 0.19f), new Vector3(0.24f, 0.035f, 0.025f), eyes, false);
                armL = Limb("ArmL", visual, new Vector3(-0.72f, 1.85f, 0f), new Vector3(0.34f, 1.1f, 0.36f), stone);
                armR = Limb("ArmR", visual, new Vector3(0.72f, 1.85f, 0f), new Vector3(0.34f, 1.1f, 0.36f), stone);
                legL = Limb("LegL", visual, new Vector3(-0.27f, 0.81f, 0f), new Vector3(0.34f, 0.81f, 0.4f), plate);
                legR = Limb("LegR", visual, new Vector3(0.27f, 0.81f, 0f), new Vector3(0.34f, 0.81f, 0.4f), plate);
                for (int i = 0; i < 3; i++) Box("ChestPlate", visual, new Vector3(0f, 1.08f + i * 0.2f, 0.4f), new Vector3(0.78f, 0.09f, 0.05f), plate, false);
            }
            root.AddComponent<ThreatPresentation>().EditorSetup(kind, visual, armL, armR, legL, legR, head);
        }

        private static Transform Limb(string name, Transform parent, Vector3 at, Vector3 size, Material material)
        {
            Transform pivot = Joint(name, parent, at);
            PolishAssets.Shape("Limb", pivot, Vector3.down * size.y * 0.5f, size, material);
            return pivot;
        }
    }
}

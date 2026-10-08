using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The company truck (GDD 10), greybox: an open-backed cargo bay with a ramp at the rear, a cab at
    /// the front, the ignition lever on the bay's front wall, a cargo trigger and the ride-along zone.
    /// Local +z runs from the rear (ramp) to the cab.
    /// </summary>
    public static class TruckBuilder
    {
        public const float BayLength = 5f, BayWidth = 2.6f, FloorHeight = 0.4f, WallHeight = 2.2f, RampLength = 2f;

        public static TruckCargo Build(Transform parent, Vector3 rearCenter, Vector3 forward)
        {
            Material body = GetMaterial("Greybox_Truck", new Color(0.7f, 0.22f, 0.2f));
            Material floor = GetMaterial("Greybox_TruckFloor", new Color(0.3f, 0.3f, 0.32f));
            Material lever = GetMaterial("Greybox_Ignition", new Color(1f, 0.85f, 0.1f));

            var root = new GameObject("Truck").transform;
            root.SetParent(parent, false);
            root.position = rearCenter + forward.normalized * (BayLength / 2f);
            root.rotation = Quaternion.LookRotation(forward, Vector3.up);

            float half = BayLength / 2f, w = BayWidth / 2f, top = FloorHeight + WallHeight;
            Box("BayFloor", root, new Vector3(0f, FloorHeight / 2f, 0f), new Vector3(BayWidth, FloorHeight, BayLength), floor);
            Box("Wall_L", root, new Vector3(-w - 0.05f, (FloorHeight + top) / 2f, 0f), new Vector3(0.1f, WallHeight, BayLength), body);
            Box("Wall_R", root, new Vector3(w + 0.05f, (FloorHeight + top) / 2f, 0f), new Vector3(0.1f, WallHeight, BayLength), body);
            Box("Wall_Front", root, new Vector3(0f, (FloorHeight + top) / 2f, half + 0.05f), new Vector3(BayWidth + 0.2f, WallHeight, 0.1f), body);
            Box("Cab", root, new Vector3(0f, 1.6f, half + 1.3f), new Vector3(BayWidth, 2.4f, 2.4f), body);
            foreach (float side in new[] { -1f, 1f })
            foreach (float z in new[] { -half + 0.8f, half + 1.3f })
                Primitive(PrimitiveType.Cylinder, "Wheel", root, new Vector3(side * (w + 0.1f), 0.45f, z), new Vector3(0.9f, 0.15f, 0.9f), floor, withCollider: false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Ramp from the ground up to the bay floor, so players and dragged racks get in.
            float angle = Mathf.Atan2(FloorHeight, RampLength) * Mathf.Rad2Deg;
            var ramp = Box("Ramp", root, new Vector3(0f, FloorHeight / 2f - 0.05f, -half - RampLength / 2f),
                new Vector3(BayWidth, 0.1f, Mathf.Sqrt(RampLength * RampLength + FloorHeight * FloorHeight)), floor);
            ramp.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f); // low end at the rear

            var ignition = Box("Ignition", root, new Vector3(w - 0.35f, FloorHeight + 1.1f, half - 0.2f), new Vector3(0.3f, 0.3f, 0.3f), lever);
            ignition.AddComponent<TruckIgnition>();

            var bay = new GameObject("Bay");
            bay.transform.SetParent(root, false);
            bay.layer = LayerMask.NameToLayer("Ignore Raycast");
            var trigger = bay.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, FloorHeight + WallHeight / 2f, 0f);
            trigger.size = new Vector3(BayWidth, WallHeight, BayLength);

            AddFloodlights(root, half);
            AddDisplay(root, half);

            var cargo = root.gameObject.AddComponent<TruckCargo>();
            root.gameObject.AddComponent<TruckSanctuary>().EditorSetup(GetMaterial("Greybox_TruckDoors", new Color(0.18f, 0.2f, 0.18f)), TruckMirrorMaterial());
            cargo.EditorSetup(trigger, new Bounds(new Vector3(0f, 1.5f, 1f), new Vector3(BayWidth + 0.6f, 3.5f, BayLength + 3f)), ignition.transform);
            foreach (Transform piece in root) piece.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return cargo;
        }

        // UI step 3: the haul board on the bay's front wall, facing the ramp (a quad shows its front to -z viewers).
        private static void AddDisplay(Transform root, float half)
        {
            Material housing = GetMaterial("Greybox_TruckLightBar", new Color(0.15f, 0.15f, 0.16f));
            Box("DisplayFrame", root, new Vector3(0f, FloorHeight + 1.55f, half - 0.03f), new Vector3(2.45f, 1.27f, 0.04f), housing, withCollider: false);
            GameObject quad = Primitive(PrimitiveType.Quad, "Display", root, new Vector3(0f, FloorHeight + 1.55f, half - 0.06f), new Vector3(2.3f, 1.15f, 1f),
                TruckDisplayMaterial(), withCollider: false);
            quad.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            root.gameObject.AddComponent<TruckDisplay>().EditorSetup(quad.GetComponent<MeshRenderer>(),
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.ThemeStyleSheet>(UiContentBuilder.ThemePath),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(UiContentBuilder.FontPath));
        }

        // QA B-21: an asset, so URP Unlit ships with the build (the mirror's render texture is set at runtime).
        private static Material TruckMirrorMaterial()
        {
            const string path = "Assets/_Project/Art/Greybox/TruckMirror.mat";
            var m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", Color.white);
            UnityEditor.AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // Unlit, so the board reads in a dark lot; the texture is set at runtime.
        private static Material TruckDisplayMaterial()
        {
            const string path = "Assets/_Project/Art/Greybox/TruckDisplay.mat";
            var m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.SetColor("_BaseColor", new Color(0.05f, 0.05f, 0.06f));
            UnityEditor.AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // M10.1 truck upgrade: a light bar on the cab roof aimed back over the bay and the lot; off until bought.
        private static void AddFloodlights(Transform root, float half)
        {
            Material glow = GetMaterial("Greybox_TruckFloodlight", new Color(1f, 0.95f, 0.8f));
            glow.SetColor("_EmissionColor", new Color(3f, 2.8f, 2.3f));
            glow.EnableKeyword("_EMISSION");
            glow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            Material housing = GetMaterial("Greybox_TruckLightBar", new Color(0.15f, 0.15f, 0.16f));
            float roof = 1.6f + 1.2f;
            Box("LightBar", root, new Vector3(0f, roof + 0.08f, half + 0.6f), new Vector3(BayWidth * 0.8f, 0.16f, 0.25f), housing, withCollider: false);
            var lights = new System.Collections.Generic.List<Light>();
            var lamps = new System.Collections.Generic.List<Renderer>();
            foreach (float side in new[] { -0.7f, 0.7f })
            {
                GameObject lamp = Box("Floodlamp", root, new Vector3(side, roof + 0.25f, half + 0.5f), new Vector3(0.45f, 0.3f, 0.12f), glow, withCollider: false);
                lamps.Add(lamp.GetComponent<Renderer>());
                var go = new GameObject("Floodlight");
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(side, roof + 0.3f, half + 0.4f);
                go.transform.localRotation = Quaternion.Euler(22f, 180f + side * 18f, 0f); // back over the bay, slightly outward
                var light = go.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 32f;
                light.spotAngle = 75f;
                light.intensity = 14f;
                light.color = new Color(1f, 0.95f, 0.85f);
                light.shadows = LightShadows.None; // the shadow budget belongs to flashlights (M7.6)
                light.enabled = false;
                lights.Add(light);
            }
            foreach (Renderer r in lamps) r.enabled = false;
            root.gameObject.AddComponent<TruckFloodlights>().EditorSetup(lights.ToArray(), lamps.ToArray());
        }
    }
}

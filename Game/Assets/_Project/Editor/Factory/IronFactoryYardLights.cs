using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// High-mast sodium floodlight towers placed around the factory yard and dock lot.
    /// In heavy industrial plants and rail yards, these 18m lattice steel towers provide broad sodium
    /// illumination across the tracks, scrap heaps, and truck aprons. Features concrete footings, tapered
    /// lattice masts with ladder cages, service platforms with railings, and 4-lamp sodium fixture banks.
    /// </summary>
    public static class IronFactoryYardLights
    {
        private const float TowerHeight = 18f;
        private const float BaseSpread = 1.4f;
        private const float TopSpread = .7f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform group = Group("YardLights", root);

            // 4 strategic floodlight positions around the exterior yard
            Vector3[] positions =
            {
                new(32f, 0f, -15f),   // South-West lot (lights truck approach and substation)
                new(88f, 0f, -15f),   // South-East lot (lights dock apron and rail siding)
                new(115f, 0f, -38f),  // Rail yard central tower
                new(60f, 0f, 115f),   // North perimeter wall & smokestacks
            };

            Material sodiumGlass = GetMaterial("IronFactory_SodiumGlass", new Color(1f, .75f, .3f));
            sodiumGlass.SetColor("_EmissionColor", new Color(2f, 1.3f, .35f));
            sodiumGlass.EnableKeyword("_EMISSION");

            for (int i = 0; i < positions.Length; i++)
            {
                BuildTower(group, $"YardLight_{i}", positions[i], art, sodiumGlass, i == 1);
            }

            MarkStatic(group);
        }

        private static void BuildTower(Transform parent, string name, Vector3 pos, IronFactoryArt art, Material lampMat, bool stutter)
        {
            Transform tower = Group(name, parent);
            tower.position = pos;

            // Reinforced concrete pedestal footing
            Box("Footing", tower, Vector3.up * .3f, new Vector3(2.2f, .6f, 2.2f), art.Concrete, true);

            // 4 tapered corner lattice mast legs
            foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
            {
                Vector3 basePt = new(sx * BaseSpread * .5f, .6f, sz * BaseSpread * .5f);
                Vector3 topPt = new(sx * TopSpread * .5f, TowerHeight, sz * TopSpread * .5f);
                Vector3 delta = topPt - basePt;
                Transform leg = Box("Leg", tower, (basePt + topPt) * .5f, new Vector3(.12f, delta.magnitude, .12f), art.Steel, false).transform;
                leg.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            }

            // Horizontal cross-struts and X-braces up the mast at 2.5m intervals
            for (float y = 2.5f; y < TowerHeight; y += 2.5f)
            {
                float t = y / TowerHeight;
                float span = Mathf.Lerp(BaseSpread, TopSpread, t);
                Box("Strut_X1", tower, new Vector3(0f, y, span * .5f), new Vector3(span, .08f, .08f), art.Steel, false);
                Box("Strut_X2", tower, new Vector3(0f, y, -span * .5f), new Vector3(span, .08f, .08f), art.Steel, false);
                Box("Strut_Z1", tower, new Vector3(span * .5f, y, 0f), new Vector3(.08f, .08f, span), art.Steel, false);
                Box("Strut_Z2", tower, new Vector3(-span * .5f, y, 0f), new Vector3(.08f, .08f, span), art.Steel, false);
            }

            // Maintenance climbing ladder on the south face of the mast
            for (float y = .8f; y < TowerHeight; y += .45f)
            {
                float t = y / TowerHeight;
                float z = Mathf.Lerp(BaseSpread, TopSpread, t) * .5f + .05f;
                Box("Rung", tower, new Vector3(0f, y, z), new Vector3(.4f, .03f, .03f), art.Steel, false);
            }

            // Top service platform
            Transform platform = Group("ServicePlatform", tower);
            platform.localPosition = Vector3.up * TowerHeight;
            Box("Deck", platform, Vector3.zero, new Vector3(2f, .1f, 2f), art.Grating, false);

            // Platform perimeter safety handrails
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Rail_X", platform, new Vector3(0f, .55f, side * .95f), new Vector3(2f, .06f, .06f), art.Safety, false);
                Box("Rail_Z", platform, new Vector3(side * .95f, .55f, 0f), new Vector3(.06f, .06f, 2f), art.Safety, false);
                foreach (float post in new[] { -.95f, .95f })
                    Box("Post", platform, new Vector3(post, .27f, side * .95f), new Vector3(.06f, .55f, .06f), art.Steel, false);
            }

            // Light cross-arm bar and 4 angled sodium floodlight housings
            Transform lightBar = Group("LightBar", platform);
            lightBar.localPosition = new Vector3(0f, 1.4f, 0f);
            Box("CrossArm", lightBar, Vector3.zero, new Vector3(2.6f, .12f, .12f), art.Steel, false);

            float[] fixtureX = { -1.1f, -.4f, .4f, 1.1f };
            for (int f = 0; f < fixtureX.Length; f++)
            {
                Transform fixture = Group($"Fixture_{f}", lightBar);
                fixture.localPosition = new Vector3(fixtureX[f], -.15f, 0f);
                // Aim downward and outward across the yard
                fixture.localRotation = Quaternion.Euler(35f, (fixtureX[f] < 0 ? -15f : 15f), 0f);

                Box("Housing", fixture, Vector3.zero, new Vector3(.45f, .3f, .35f), art.Steel, false);
                GameObject glass = Box("Glass", fixture, Vector3.forward * .18f, new Vector3(.4f, .25f, .02f), lampMat, false);

                // Add point/spot light on the central fixtures
                if (f == 1 || f == 2)
                {
                    Transform emitter = Group("Beam", fixture);
                    emitter.localPosition = Vector3.forward * .25f;
                    Light lamp = emitter.gameObject.AddComponent<Light>();
                    lamp.type = LightType.Spot;
                    lamp.range = 35f;
                    lamp.spotAngle = 75f;
                    lamp.intensity = 12f;
                    lamp.color = new Color(1f, .78f, .45f);
                    lamp.shadows = LightShadows.None;

                    var fixComp = fixture.gameObject.AddComponent<LightFixture>();
                    if (stutter && f == 1) fixComp.EditorSetupStutter(lamp, glass.GetComponent<Renderer>(), 8800 + f);
                    else fixComp.EditorSetup(lamp, glass.GetComponent<Renderer>(), false, false, 8800 + f);
                }
            }

            // Power breaker / electrical disconnect box on mast base
            Transform discon = Group("DisconnectBox", tower);
            discon.localPosition = new Vector3(BaseSpread * .5f + .15f, 1.5f, 0f);
            Box("Box", discon, Vector3.zero, new Vector3(.25f, .65f, .45f), art.Safety, false);
            Box("Conduit", discon, new Vector3(0f, -.55f, 0f), new Vector3(.05f, .5f, .05f), art.Steel, false);

            tower.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }
    }
}

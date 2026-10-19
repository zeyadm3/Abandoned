using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What grows on a rock in a cold bay: dark cypresses and wind-bent pines in the lee of the buildings and down the
    /// long north slope, scrub everywhere the wind can't scour it, mats of ice plant spilling over the slopes, rocks
    /// whitened by the gulls along the shore, and boulders on the slopes (solid: you can climb round them). Trees and
    /// shrubs are mesh kits placed many times. Trunks and boulders are solid; foliage is soft and walk-through. Nothing
    /// grows in a building, on a road or path, on the quay or in a doorway's approach.
    /// </summary>
    public static class IslandVegetation
    {
        public const string GroupName = "Vegetation";

        public static (int trees, int shrubs, int rocks) Build(Transform root, IslandPrisonArt art)
        {
            Transform g = Group(GroupName, root);
            Material needles = PolishAssets.Material("Island_Needles", new Color(.07f, .11f, .08f), .9f);
            Material pine = PolishAssets.Material("Island_PineCrown", new Color(.09f, .13f, .09f), .9f);
            Material scrub = PolishAssets.Material("Island_Scrub", new Color(.17f, .19f, .12f), .95f);
            Material dry = PolishAssets.Material("Island_DryScrub", new Color(.26f, .23f, .15f), .95f);
            Material ice = PolishAssets.Material("Island_IcePlant", new Color(.2f, .26f, .13f), .8f);
            Material bloom = PolishAssets.Material("Island_IcePlantBloom", new Color(.42f, .2f, .22f), .8f);
            Material guano = PolishAssets.Material("Island_Guano", new Color(.8f, .8f, .74f), .9f);
            Material bark = art.DarkWood;

            var cypress = IslandMeshKit.Get("Tree_Cypress", r =>
            {
                Box("Trunk", r, new Vector3(0f, 1.5f, 0f), new Vector3(.35f, 3f, .35f), bark, false);
                for (int i = 0; i < 5; i++)
                    PolishAssets.Shape("Crown", r, new Vector3(0f, 3f + i * 1.9f, 0f), new Vector3(2.6f - i * .4f, 3.4f, 2.6f - i * .4f), needles, true);
            });
            // A Monterey pine bent by the wind: a leaning trunk, a couple of limbs, and tiers of dense crown along them.
            var bent = IslandMeshKit.Get("Tree_WindPine", r =>
            {
                GameObject trunk = Box("Trunk", r, new Vector3(.45f, 2.8f, 0f), new Vector3(.42f, 5.8f, .42f), bark, false);
                trunk.transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
                Box("Limb", r, new Vector3(1.6f, 4.4f, .3f), new Vector3(2.4f, .2f, .2f), bark, false).transform.localRotation = Quaternion.Euler(0f, 20f, 18f);
                Box("Limb", r, new Vector3(-.2f, 4.9f, -.6f), new Vector3(1.8f, .18f, .18f), bark, false).transform.localRotation = Quaternion.Euler(0f, -40f, -15f);
                foreach ((Vector3 at, Vector3 s) in new[]
                {
                    (new Vector3(.9f, 6.1f, 0f), new Vector3(3.4f, 2.4f, 3.2f)), (new Vector3(2.4f, 5.1f, .5f), new Vector3(2.8f, 2f, 2.4f)),
                    (new Vector3(-.6f, 5.4f, -.8f), new Vector3(2.4f, 1.9f, 2.2f)), (new Vector3(1.2f, 7.2f, .2f), new Vector3(2.4f, 1.8f, 2.2f)),
                    (new Vector3(.3f, 4.6f, .9f), new Vector3(2f, 1.6f, 1.8f)),
                })
                    PolishAssets.Shape("Crown", r, at, s, pine, true);
            });
            var bush = IslandMeshKit.Get("Shrub", r =>
            {
                PolishAssets.Shape("Bush", r, new Vector3(0f, .45f, 0f), new Vector3(1.8f, 1f, 1.5f), scrub, true);
                PolishAssets.Shape("Bush", r, new Vector3(.6f, .35f, .4f), new Vector3(1.1f, .8f, 1f), scrub, true);
            });
            var dead = IslandMeshKit.Get("Shrub_Dry", r =>
            {
                PolishAssets.Shape("Bush", r, new Vector3(0f, .35f, 0f), new Vector3(1.4f, .8f, 1.2f), dry, true);
                for (int i = 0; i < 4; i++)
                    Box("Twig", r, new Vector3(Mathf.Cos(i * 1.6f) * .4f, .7f, Mathf.Sin(i * 1.6f) * .4f), new Vector3(.04f, .8f, .04f), bark, false)
                        .transform.localRotation = Quaternion.Euler(i * 20f - 30f, 0f, i * 15f - 20f);
            });
            var mat = IslandMeshKit.Get("IcePlant", r =>
            {
                PolishAssets.Shape("Mat", r, new Vector3(0f, .05f, 0f), new Vector3(3.2f, .25f, 2.4f), ice, true);
                PolishAssets.Shape("Mat", r, new Vector3(1.2f, .04f, .9f), new Vector3(2f, .2f, 1.6f), ice, true);
            });

            Transform trees = Group("Trees", g), shrubs = Group("Shrubs", g), rocks = Group("Rocks", g);
            int nTrees = 0, nShrubs = 0, nRocks = 0;
            for (int i = 0; i < 9000; i++)
            {
                float x = Mathf.Lerp(-60f, 210f, K.Hash(i, 401)), z = Mathf.Lerp(-30f, 200f, K.Hash(401, i));
                float h = IslandLandform.Height(x, z);
                if (h < L.SeaY + .4f || IslandLandform.Open(x, z) || Built(x, z)) continue;
                float slope = Slope(x, z);
                float roll = K.Hash(i, 402);
                Vector3 at = new(x, h - .05f, z);
                Quaternion yaw = Quaternion.Euler(0f, K.Hash(i, 403) * 360f, 0f);
                bool plateau = h > L.PlateauY - .5f;
                // Trees: on gentle ground, sparse on the windy plateau, thicker on the north slope and in the lee.
                float treeChance = plateau ? .012f : z > L.Plateau.yMax ? .06f : .03f;
                if (slope < .55f && roll < treeChance && nTrees < 170)
                {
                    bool windward = x < L.Plateau.xMin + 10f || z < L.Plateau.yMin + 6f;
                    bool isPine = windward || K.Hash(i, 404) < .4f;
                    Transform tree = IslandMeshKit.Place(isPine ? bent : cypress, "Tree", trees, at,
                        windward ? Quaternion.Euler(0f, 60f + K.Hash(i, 405) * 40f, 0f) : yaw);
                    float s = .8f + K.Hash(i, 406) * .6f;
                    tree.localScale = new Vector3(s, s, s);
                    // The trunk's collider sits on the trunk (the pine's leans out from its base).
                    IslandMeshKit.Solid(tree, isPine ? new Vector3(.45f, 1.5f, 0f) : new Vector3(0f, 1.5f, 0f), new Vector3(.5f, 3f, .5f));
                    nTrees++;
                    continue;
                }
                if (roll < .2f && nShrubs < 900)
                {
                    var kit = slope > .9f ? mat : roll < .07f ? dead : roll < .14f ? bush : mat;
                    Transform s = IslandMeshKit.Place(kit, "Plant", shrubs, at, yaw, castShadows: false);
                    float scale = .7f + K.Hash(i, 407) * .8f;
                    s.localScale = new Vector3(scale, scale, scale);
                    nShrubs++;
                    continue;
                }
                // Boulders on the slopes, gull-white near the water.
                // Boulders on the slopes and on the plateau's knolls (never on its flat pads).
                if ((!plateau || h > L.PlateauY + .8f) && roll < .235f && nRocks < 260)
                {
                    float r = .8f + K.Hash(i, 408) * 2.6f;
                    Transform rock = K.Rock("Boulder", rocks, at + Vector3.down * r * .2f, new Vector3(r * 1.3f, r * .8f, r), K.Hash(i, 409) > .5f ? art.Rock : art.RockDark, i, r > 1.2f);
                    rock.rotation = Quaternion.Euler(0f, K.Hash(i, 411) * 360f, 0f);
                    if (h < 2f) K.Patch("Guano", rocks, at + Vector3.up * (r * .55f), new Vector2(r * .8f, r * .6f), K.Hash(i, 412) * 180f, guano);
                    nRocks++;
                }
            }
            // Sea stacks off the shore: tall broken pillars of rock, solid.
            foreach ((Vector3 at, Vector3 size) in IslandLandform.Stacks())
            {
                K.Rock("SeaStack", rocks, at, size, art.Rock, (int)(at.x * 13f + at.z), true);
                nRocks++;
            }
            foreach (Renderer r in shrubs.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            MarkStatic(g);
            return (nTrees, nShrubs, nRocks);
        }

        private static float Slope(float x, float z)
        {
            float dx = IslandLandform.Height(x + 1f, z) - IslandLandform.Height(x - 1f, z);
            float dz = IslandLandform.Height(x, z + 1f) - IslandLandform.Height(x, z - 1f);
            return new Vector2(dx, dz).magnitude * .5f;
        }

        // Buildings, their approaches, paths, the quay, the stair: nothing grows there.
        private static readonly Rect[] Keep =
        {
            Grow(L.Cellhouse, 3f), Grow(L.RecYard, 1f), Grow(L.WardenHouse, 3f), Grow(new Rect(L.Lighthouse.xMin, L.Lighthouse.yMin, 14f, 8f), 3f),
            Grow(L.WaterTower, 3f), Grow(L.OfficersClub, 2f), Grow(L.Barracks, 2f), Grow(L.Quay, 1f), Grow(L.GuardTower, 2f),
            new(86f, 12f, 20f, 12f),                                  // the cliff stair, top and bottom
            new(L.MainDoorX - 4f, L.Plateau.yMin, 8f, L.Cellhouse.yMin - L.Plateau.yMin),   // the main path
            new(L.MainDoorX - 4f, L.Plateau.yMin, L.Terrace.xMin - L.MainDoorX + 4f, 9f),   // path to the terrace
            new(L.Terrace.xMin - 2f, L.Plateau.yMin, 8f, 30f), new(L.Cellhouse.xMax, L.EastDoorZ - 3f, 44f, 6f),
            new(36f, L.Cellhouse.yMax, 8f, L.WardenHouse.yMin - L.Cellhouse.yMax),          // to the warden's house
            new(8f, 20f, 18f, 16f),                                   // the powerhouse ruin
            Grow(L.Slip, 2f), Grow(L.PierWest, 1f), Grow(L.PierEast, 1f),
        };

        private static Rect Grow(Rect r, float m) => new(r.xMin - m, r.yMin - m, r.width + 2f * m, r.height + 2f * m);

        private static bool Built(float x, float z)
        {
            var p = new Vector2(x, z);
            foreach (Rect r in Keep) if (r.Contains(p)) return true;
            return false;
        }
    }
}

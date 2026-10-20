using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Larch Court in December, around the house: the driveway shovelled once and drifted since, the back yard's board
    /// fence, bare maples and oaks, spruce along the back, a snowman nobody finished, the mailbox and bins, the crew's
    /// van at the kerb. These stand in the "Yard" group, inside the NavMesh, so the guide's arrows go round them. The
    /// streets and houses beyond, and the edge of the walkable world, are <see cref="TutorialTown"/>'s ("Neighbourhood").
    /// </summary>
    public static class TutorialNeighbourhood
    {
        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform yard = Group("Yard", root);
            Driveway(Group("Driveway", yard), art);
            Yard(yard, art);
            Trees(Group("Trees", yard), art);
            Van(yard, art);
            TutorialTown.Build(Group("Neighbourhood", root), art);
        }

        private static void Driveway(Transform s, TutorialHouseArt art)
        {
            float y = L.Grade + .006f;
            Rect d = L.Driveway;
            Box("Driveway", s, new Vector3(d.center.x, y, d.center.y), new Vector3(d.width, .012f, d.height), art.Asphalt, false);
            // Shovelled once: banks either side, clear of the truck and the kit crates by the ramp.
            foreach (float z in new[] { d.yMin - .35f, d.yMax + .35f })
                Box("DriveBank", s, new Vector3(d.center.x + 6f, L.Grade + .15f, z), new Vector3(d.width - 14f, .3f, .55f), art.SnowDrift, false);
            // A walk from the drive to the porch steps' side.
            Box("FrontWalk", s, new Vector3(L.Porch.xMax + .5f, y + .001f, L.Porch.yMax + 1f), new Vector3(1f, .012f, 2f), art.Sidewalk, false);
        }

        private static void Yard(Transform y, TutorialHouseArt art)
        {
            // The back yard's board fence: west, and along both sides as far as the front lawn.
            Fence(y, "Fence_West", new Vector3(-12f, 0f, -12f), new Vector3(-12f, 0f, 30f), art);
            Fence(y, "Fence_North", new Vector3(-12f, 0f, 30f), new Vector3(42f, 0f, 30f), art);
            Fence(y, "Fence_South", new Vector3(-12f, 0f, -12f), new Vector3(42f, 0f, -12f), art);
            Mailbox(y, new Vector3(L.StreetX - 1.9f, L.Grade, L.Driveway.yMax + 1.2f), art);
            Snowman(y, new Vector3(40f, L.Grade, 19f), art);
            foreach (float z in new[] { L.Driveway.yMin - 1.4f, L.Driveway.yMin - 2.2f })
            {
                Transform bin = Group("TrashCan", y);
                bin.position = new Vector3(L.StreetX - 2.6f, L.Grade, z);
                Box("Can", bin, new Vector3(0f, .55f, 0f), new Vector3(.6f, 1.1f, .7f), art.VanTrim);
                Box("Lid", bin, new Vector3(0f, 1.13f, 0f), new Vector3(.64f, .06f, .74f), art.VanTrim, false);
                Box("LidSnow", bin, new Vector3(0f, 1.19f, 0f), new Vector3(.6f, .06f, .7f), art.Snow, false);
            }
            foreach (float z in new[] { -10f, 30f })
            {
                Transform pole = Group("StreetLight", y);
                pole.position = new Vector3(L.StreetX - .4f, L.Grade, z);
                Box("Pole", pole, new Vector3(0f, 3.4f, 0f), new Vector3(.14f, 6.8f, .14f), art.DarkMetal);
                Box("Arm", pole, new Vector3(.9f, 6.7f, 0f), new Vector3(1.8f, .08f, .08f), art.DarkMetal, false);
                Box("Head", pole, new Vector3(1.75f, 6.62f, 0f), new Vector3(.6f, .16f, .3f), art.DarkMetal, false);
            }
            // A sled by the porch, a drift along the band's north wall, a few crusted bushes against the house.
            Box("Sled", y, new Vector3(L.Porch.xMin + .6f, L.Grade + .45f, L.Porch.yMax + 1.4f), new Vector3(.45f, .9f, .08f), art.OrnamentRed, false).transform.localRotation = Quaternion.Euler(0f, 0f, 14f);
            Box("WallDrift", y, new Vector3((L.WingEast + L.HouseEast) * .5f, L.Grade + .25f, L.BandNorth + .5f), new Vector3(16f, .5f, .7f), art.SnowDrift, false);
            Vector3[] shrubs =
            {
                new(L.HouseEast + .7f, L.Grade, L.Porch.yMax + 1.6f), new(L.HouseEast + .7f, L.Grade, L.Porch.yMin - 1.6f),
                new(-.9f, L.Grade, 5f), new(-.9f, L.Grade, 12f),
            };
            foreach (Vector3 at in shrubs)
            {
                Transform bush = Group("Shrub", y);
                bush.position = at;
                Primitive(PrimitiveType.Sphere, "Crown", bush, new Vector3(0f, .45f, 0f), new Vector3(1.1f, .8f, 1.1f), art.Pine);
                Primitive(PrimitiveType.Sphere, "SnowCap", bush, new Vector3(0f, .72f, 0f), new Vector3(1f, .4f, 1f), art.Snow, false);
            }
        }

        private static void Fence(Transform parent, string name, Vector3 a, Vector3 b, TutorialHouseArt art)
        {
            Transform f = Group(name, parent);
            Vector3 dir = (b - a).normalized;
            float length = Vector3.Distance(a, b);
            bool alongX = Mathf.Abs(dir.x) > .5f;
            Vector3 mid = (a + b) * .5f + Vector3.up * L.Grade;
            Box("Boards", f, mid + Vector3.up * .9f, alongX ? new Vector3(length, 1.8f, .04f) : new Vector3(.04f, 1.8f, length), art.FenceWood);
            Box("RailSnow", f, mid + Vector3.up * 1.83f, alongX ? new Vector3(length, .06f, .1f) : new Vector3(.1f, .06f, length), art.Snow, false);
            for (float t = 0f; t <= length; t += 2.4f)
                Box("Post", f, a + dir * t + Vector3.up * (L.Grade + .95f), new Vector3(.1f, 1.9f, .1f), art.FenceWood, false);
        }

        private static void Mailbox(Transform parent, Vector3 at, TutorialHouseArt art)
        {
            Transform m = Group("Mailbox", parent);
            m.position = at;
            Box("Post", m, new Vector3(0f, .55f, 0f), new Vector3(.1f, 1.1f, .1f), art.FenceWood);
            Box("Box", m, new Vector3(0f, 1.15f, 0f), new Vector3(.5f, .22f, .24f), art.DarkMetal, false);
            Box("Flag", m, new Vector3(.12f, 1.28f, .14f), new Vector3(.04f, .14f, .02f), art.OrnamentRed, false);
            Box("Snow", m, new Vector3(0f, 1.28f, 0f), new Vector3(.5f, .05f, .24f), art.Snow, false);
        }

        private static void Snowman(Transform parent, Vector3 at, TutorialHouseArt art)
        {
            Transform s = Group("Snowman", parent);
            s.position = at;
            Primitive(PrimitiveType.Sphere, "Base", s, new Vector3(0f, .38f, 0f), Vector3.one * .8f, art.Snow);
            Primitive(PrimitiveType.Sphere, "Middle", s, new Vector3(0f, .95f, 0f), Vector3.one * .56f, art.Snow, false);
            // Never finished: no head, a scarf on the snow beside it.
            Box("Scarf", s, new Vector3(.45f, .03f, .2f), new Vector3(.7f, .03f, .14f), art.OrnamentRed, false).transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            Box("StickArm", s, new Vector3(.36f, 1f, 0f), new Vector3(.5f, .03f, .03f), art.Branch, false).transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        }

        private static void Trees(Transform t, TutorialHouseArt art)
        {
            Vector3[] bare =
            {
                new(36f, 0f, 19f), new(45f, 0f, 2f), new(33f, 0f, -4f), new(-6f, 0f, 21f), new(12f, 0f, 22f),
                new(49f, 0f, 24f), new(22f, 0f, -7f), new(-7f, 0f, -6f), new(4f, 0f, 25f), new(54f, 0f, -6f),
            };
            for (int i = 0; i < bare.Length; i++) BareTree(t, bare[i] + Vector3.up * L.Grade, i, art);
            Vector3[] spruce =
            {
                new(-9f, 0f, 1f), new(-9.5f, 0f, 8.5f), new(-9f, 0f, 16f), new(0f, 0f, 27f), new(8f, 0f, 27f), new(18f, 0f, 27f),
                new(-8f, 0f, -9f), new(6f, 0f, -9.5f), new(18f, 0f, -9.5f),
            };
            for (int i = 0; i < spruce.Length; i++) Spruce(t, "Spruce", spruce[i] + Vector3.up * L.Grade, 5f + i % 3, art);
        }

        public static void BareTree(Transform parent, Vector3 at, int seed, TutorialHouseArt art, float scale = 1f, bool collider = true)
        {
            Transform tree = Group("BareTree", parent);
            tree.position = at;
            var random = new System.Random(seed * 31 + 7);
            float h = (5f + (float)random.NextDouble() * 3f) * scale;
            Primitive(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, h * .3f, 0f), new Vector3(.32f * scale, h * .3f, .32f * scale), art.Bark, collider);
            for (int b = 0; b < 7; b++)
            {
                float a = b * 51f + (float)random.NextDouble() * 20f, y = h * (.45f + b * .07f), len = h * (.35f - b * .025f);
                GameObject limb = Box("Limb", tree, new Vector3(0f, y, 0f), new Vector3(.12f * scale, len, .12f * scale), art.Branch, false);
                limb.transform.localRotation = Quaternion.Euler(35f + b * 4f, a, 0f);
                limb.transform.localPosition += limb.transform.localRotation * Vector3.up * (len * .5f);
                Box("SnowOnLimb", limb.transform, new Vector3(0f, 0f, .7f), new Vector3(1.1f, .5f, .5f), art.Snow, false);
            }
        }

        public static void Spruce(Transform parent, string name, Vector3 at, float height, TutorialHouseArt art, bool collider = true)
        {
            Transform tree = Group(name, parent);
            tree.position = at;
            Mesh cone = TutorialHouseMeshes.Cone();
            Primitive(PrimitiveType.Cylinder, "Trunk", tree, new Vector3(0f, height * .12f, 0f), new Vector3(.25f, height * .12f, .25f), art.Bark, collider);
            for (int i = 0; i < 3; i++)
            {
                float y = height * (.15f + i * .25f), w = height * (.5f - i * .12f);
                TutorialHouseMeshes.Shape("Tier", tree, cone, new Vector3(0f, y, 0f), new Vector3(w, height * .4f, w), art.Pine, false);
                TutorialHouseMeshes.Shape("TierSnow", tree, cone, new Vector3(0f, y + height * .06f, 0f), new Vector3(w * .85f, height * .36f, w * .85f), art.Snow, false);
            }
        }

        // The crew's van at the kerb: a plain white box van, engine off, doors shut.
        private static void Van(Transform parent, TutorialHouseArt art)
        {
            Transform van = Group("CrewVan", parent);
            van.SetPositionAndRotation(L.VanPosition, Quaternion.identity);
            Box("Body", van, new Vector3(0f, 1.35f, -.4f), new Vector3(2f, 1.9f, 4f), art.VanBody);
            Box("Cab", van, new Vector3(0f, 1.05f, 2.15f), new Vector3(2f, 1.3f, 1.1f), art.VanBody);
            Box("Windscreen", van, new Vector3(0f, 1.45f, 2.71f), new Vector3(1.8f, .55f, .04f), art.Window, false).transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            Box("Bumper", van, new Vector3(0f, .4f, 2.75f), new Vector3(2.04f, .2f, .12f), art.VanTrim, false);
            Box("RearBumper", van, new Vector3(0f, .4f, -2.45f), new Vector3(2.04f, .2f, .12f), art.VanTrim, false);
            Box("RoofSnow", van, new Vector3(0f, 2.33f, -.4f), new Vector3(1.9f, .06f, 3.9f), art.Snow, false);
            foreach (float x in new[] { -1f, 1f })
            foreach (float z in new[] { -1.6f, 1.95f })
                Primitive(PrimitiveType.Cylinder, "Wheel", van, new Vector3(x * .95f, .38f, z), new Vector3(.76f, .14f, .76f), art.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            van.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }
    }
}

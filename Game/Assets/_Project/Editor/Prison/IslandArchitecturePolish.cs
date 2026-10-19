using System;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;
using MeshKit = System.Collections.Generic.List<(UnityEngine.Mesh mesh, UnityEngine.Material material)>;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The island's final architectural layer: an imposing civic entrance, deeply framed windows, institutional
    /// cell interiors and fitted metalwork. Repeated details are saved mesh kits rather than hundreds of tiny
    /// renderers. These skins sit on existing solids; none becomes a new obstruction on a haul or collapse route.
    /// </summary>
    public static class IslandArchitecturePolish
    {
        public const string GroupName = "ArchitecturePolish";

        public static void Build(Transform root, IslandPrisonArt art)
        {
            Transform g = Group(GroupName, root);
            g.gameObject.AddComponent<WalkThroughObject>().EditorSetup(
                "Thin architectural skins are backed by the existing walls; ornamental steelwork and lanterns are overhead.");
            Entrance(Group("CivicEntrance", g), art);
            CellhouseWindows(Group("WindowStonework", g), art);
            CellRows(Group("CellDado", g), art);
            Outbuildings(Group("OutbuildingStonework", g), art);
            WaterTower(Group("TowerMetalwork", g), art);
            Lanterns(Group("FittedLanterns", g), art);
            MarkStatic(g);
        }

        private static GameObject Detail(string name, Transform g, Vector3 at, Vector3 size, Material m) =>
            K.WorldBox(name, g, at, size, m, false);

        private static void Entrance(Transform g, IslandPrisonArt art)
        {
            float x = L.MainDoorX, z = L.Cellhouse.yMin;
            // The original opening and its canopy stay clear: this vertical bay begins above their lintel.
            Detail("CentralBay", g, new Vector3(x, 21.5f, z - .16f), new Vector3(6.2f, 9.7f, .32f), art.Facade);
            foreach (float side in new[] { -1f, 1f })
            {
                float px = x + side * 3.55f;
                Detail("Pier", g, new Vector3(px, 19f, z - .48f), new Vector3(.72f, 13.8f, .55f), art.FacadeDark);
                Detail("PierRecess", g, new Vector3(px, 20.5f, z - .78f), new Vector3(.26f, 9.8f, .08f), art.Facade);
                Detail("PierFoot", g, new Vector3(px, 12.45f, z - .52f), new Vector3(1.05f, .9f, .7f), art.Facade);
                Detail("PierCapital", g, new Vector3(px, 25.7f, z - .51f), new Vector3(1.12f, .5f, .8f), art.Facade);
            }
            Detail("Entablature", g, new Vector3(x, 26.25f, z - .31f), new Vector3(8.4f, .5f, 1.05f), art.FacadeDark);
            Detail("CrownLower", g, new Vector3(x, 26.62f, z - .33f), new Vector3(7.5f, .24f, 1.2f), art.Facade);
            Detail("CrownMiddle", g, new Vector3(x, 27.05f, z - .18f), new Vector3(5.7f, .65f, .78f), art.FacadeDark);
            Detail("CrownUpper", g, new Vector3(x, 27.55f, z - .18f), new Vector3(3.5f, .4f, .86f), art.Facade);
            Detail("CrownKeystone", g, new Vector3(x, 27.97f, z - .2f), new Vector3(1.3f, .44f, .9f), art.FacadeDark);
            // A weathered architectural seal, deliberately original to Gallows Rock rather than a real insignia.
            Transform seal = Group("StoneSeal", g);
            seal.SetPositionAndRotation(new Vector3(x, 22.8f, z - .38f), Quaternion.Euler(-90f, 0f, 0f));
            Primitive(PrimitiveType.Cylinder, "OuterDisc", seal, Vector3.zero, new Vector3(2.7f, .085f, 2.7f), art.FacadeDark, false);
            Primitive(PrimitiveType.Cylinder, "InnerDisc", seal, new Vector3(0f, .105f, 0f), new Vector3(2.3f, .04f, 2.3f), art.Facade, false);
            for (int i = -1; i <= 1; i++)
                Box("SealBars", seal, new Vector3(i * .36f, .165f, 0f), new Vector3(.15f, .045f, i == 0 ? 1.65f : 1.2f), art.Bars, false);
            Box("SealCrossbar", seal, new Vector3(0f, .17f, .23f), new Vector3(1.2f, .05f, .15f), art.Bars, false);
            Detail("SealSill", g, new Vector3(x, 20.9f, z - .46f), new Vector3(4.1f, .2f, .5f), art.FacadeDark);
            Detail("FriezeRecess", g, new Vector3(x, 18.75f, z - .345f), new Vector3(5.1f, .85f, .06f), art.Dado);
            foreach (float dx in new[] { -2f, -1f, 0f, 1f, 2f })
                Detail("FriezeRib", g, new Vector3(x + dx, 18.75f, z - .41f), new Vector3(.12f, .6f, .08f), art.Facade);
            // Coffered canopy underside: visible from first-person height, with no additional light components.
            for (int i = 0; i < 5; i++)
                Detail("CanopyCoffer", g, new Vector3(x - 2.4f + i * 1.2f, 16.16f, z - 1.25f), new Vector3(.045f, .07f, 2.4f), art.Steel);
        }

        private static void CellhouseWindows(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            void Facade(float from, float to, float wall, bool alongX, Vector3 outward, float door, float clearance, bool highOnly)
            {
                for (float at = from + 2f; at < to - 1f; at += 4f)
                {
                    if (Mathf.Abs(at - door) < clearance) continue;
                    Vector3 p = alongX ? new Vector3(at, L.PlateauY, wall) : new Vector3(wall, L.PlateauY, at);
                    if (highOnly && at >= L.StackSouth) Window(g, p + Vector3.up * 8.5f, outward, 1.6f, 2.8f, art);
                    else
                    {
                        Window(g, p + Vector3.up * 1.2f, outward, 1.6f, 2.2f, art);
                        Window(g, p + Vector3.up * 5f, outward, 1.6f, 5.6f, art);
                    }
                }
            }
            Facade(c.xMin, c.xMax, c.yMin, true, Vector3.back, L.MainDoorX, 3.2f, false);
            Facade(c.xMin, c.xMax, c.yMax, true, Vector3.forward, L.ServiceDoorX, 2.6f, false);
            Facade(c.yMin, c.yMax, c.xMin, false, Vector3.left, L.YardDoorZ, 2.6f, true);
            Facade(c.yMin, c.yMax, c.xMax, false, Vector3.right, L.EastDoorZ, 2.6f, true);
        }

        private static void Window(Transform g, Vector3 sill, Vector3 outward, float width, float height, IslandPrisonArt art)
        {
            string key = FormattableString.Invariant($"Polish_Window_{width:0.00}_{height:0.00}");
            MeshKit kit = IslandMeshKit.Get(key, r =>
            {
                foreach (float side in new[] { -1f, 1f })
                    Box("Reveal", r, new Vector3(side * (width * .5f + .085f), height * .5f, .075f),
                        new Vector3(.17f, height + .15f, .18f), art.FacadeDark, false);
                Box("Head", r, new Vector3(0f, height + .105f, .09f), new Vector3(width + .5f, .21f, .28f), art.Facade, false);
                Box("Drip", r, new Vector3(0f, height + .245f, .13f), new Vector3(width + .64f, .07f, .36f), art.FacadeDark, false);
                Box("Sill", r, new Vector3(0f, -.075f, .15f), new Vector3(width + .5f, .15f, .44f), art.Facade, false);
                Box("Apron", r, new Vector3(0f, -.23f, .05f), new Vector3(width + .18f, .15f, .18f), art.FacadeDark, false);
                IronFactorySurfaces.Apply(r, art.Tiling);
            });
            IslandMeshKit.Place(kit, "StoneWindow", g, sill, Quaternion.LookRotation(outward), true);
        }

        private static void CellRows(Transform g, IslandPrisonArt art)
        {
            MeshKit skin = IslandMeshKit.Get("Polish_CellDadoRow", r =>
            {
                for (int i = 0; i < L.CellsPerRow; i++)
                {
                    float x = i * L.CellWidth;
                    Box("BackDado", r, new Vector3(x, .61f, .023f), new Vector3(1.83f, 1.2f, .035f), art.Dado, false);
                    foreach (float side in new[] { -1f, 1f })
                        Box("SideDado", r, new Vector3(x + side * .916f, .61f, 1.29f), new Vector3(.025f, 1.2f, 2.5f), art.Dado, false);
                    Box("BackChairRail", r, new Vector3(x, 1.235f, .048f), new Vector3(1.84f, .05f, .045f), art.Plaster, false);
                }
                IronFactorySurfaces.Apply(r, art.Tiling);
            });
            foreach (L.Stack s in L.Stacks)
            for (int tier = 0; tier < L.Tiers.Length; tier++)
            foreach (bool west in new[] { true, false })
            {
                Vector3 origin = new(west ? s.ChaseWest : s.ChaseEast, L.TierY(tier), west ? L.StackSouth + 1f : L.StackNorth - 1f);
                IslandMeshKit.Place(skin, s.Name + "_Dado_" + tier, g, origin, Quaternion.LookRotation(west ? Vector3.left : Vector3.right), false);
            }
            // Longitudinal steel conduits are fixed to the cells, so a falling gallery leaves an honest void.
            foreach (L.Stack s in L.Stacks)
            foreach (float front in new[] { s.CellsWest, s.CellsEast })
            for (int tier = 0; tier < L.Tiers.Length; tier++)
            {
                float sign = front == s.CellsWest ? -1f : 1f;
                Detail("HeadConduit", g, new Vector3(front + sign * .12f, L.TierY(tier) + 3.67f, 66f), new Vector3(.07f, .07f, 24f), art.Railing);
                for (float z = L.StackSouth + 1f; z < L.StackNorth; z += 4f)
                    Detail("ConduitJunction", g, new Vector3(front + sign * .15f, L.TierY(tier) + 3.67f, z), new Vector3(.1f, .18f, .18f), art.Steel);
            }
        }

        private static void Outbuildings(Transform g, IslandPrisonArt art)
        {
            Rect b = L.Barracks;
            foreach (float y in new[] { 3.85f, 7.85f, 11.85f }) PerimeterBand(g, b, y, .18f, .12f, art.FacadeDark);
            for (float x = b.xMin + 2f; x < b.xMax - 1f; x += 4f)
            for (int tier = 0; tier < L.BarracksStoreys; tier++)
            {
                if (Mathf.Abs(x - 116f) < 2.5f || Mathf.Abs(x - 126f) < 2.5f) continue;
                Window(g, new Vector3(x, tier * L.Storey + 1.1f, b.yMin), Vector3.back, 1.4f, 1.5f, art);
                if (tier > 0) Window(g, new Vector3(x, tier * L.Storey + 1.4f, b.yMax), Vector3.forward, 1.4f, 1.2f, art);
            }
            Rect h = L.WardenHouse;
            PerimeterBand(g, h, L.PlateauY + 3.82f, .21f, .13f, art.Facade);
            foreach (float x in new[] { 32.5f, 36f, 44f, 47.5f })
            foreach (float y in new[] { L.PlateauY + 1f, L.PlateauY + L.Storey + 1f })
                Window(g, new Vector3(x, y, h.yMin), Vector3.back, 1.3f, 1.7f, art);
            foreach (float x in new[] { 33f, 46f })
            foreach (float y in new[] { L.PlateauY + 1f, L.PlateauY + L.Storey + 1f })
                Window(g, new Vector3(x, y, h.yMax), Vector3.forward, 1.3f, 1.7f, art);
            // The villa's burned roof remains broken; the surviving east eave gets its own moulding.
            Detail("VillaEastEave", g, new Vector3(h.xMax + .12f, 20.55f, h.center.y), new Vector3(.38f, .26f, h.height + .65f), art.Facade);
            foreach (float x in new[] { 37.3f, 42.7f })
            {
                Detail("PorchCapital", g, new Vector3(x, 15.05f, h.yMin - 1.5f), new Vector3(.48f, .18f, .48f), art.FacadeDark);
                Detail("PorchFoot", g, new Vector3(x, 12.27f, h.yMin - 1.5f), new Vector3(.42f, .22f, .42f), art.FacadeDark);
            }
        }

        private static void PerimeterBand(Transform g, Rect r, float y, float height, float projection, Material m)
        {
            Detail("CourseSouth", g, new Vector3(r.center.x, y, r.yMin - projection * .5f), new Vector3(r.width + .3f, height, projection), m);
            Detail("CourseNorth", g, new Vector3(r.center.x, y, r.yMax + projection * .5f), new Vector3(r.width + .3f, height, projection), m);
            Detail("CourseWest", g, new Vector3(r.xMin - projection * .5f, y, r.center.y), new Vector3(projection, height, r.height), m);
            Detail("CourseEast", g, new Vector3(r.xMax + projection * .5f, y, r.center.y), new Vector3(projection, height, r.height), m);
        }

        private static void WaterTower(Transform g, IslandPrisonArt art)
        {
            Rect r = L.WaterTower;
            for (int storey = 0; storey < 4; storey++)
            {
                float y0 = L.PlateauY + 1f + storey * 4.3f, y1 = y0 + 3.3f;
                foreach (float z in new[] { r.yMin, r.yMax })
                {
                    Beam(g, new Vector3(r.xMin, y0, z), new Vector3(r.xMax, y1, z), .075f, art.Railing);
                    Beam(g, new Vector3(r.xMax, y0, z), new Vector3(r.xMin, y1, z), .075f, art.Railing);
                }
                foreach (float x in new[] { r.xMin, r.xMax })
                {
                    Beam(g, new Vector3(x, y0, r.yMin), new Vector3(x, y1, r.yMax), .075f, art.Railing);
                    Beam(g, new Vector3(x, y0, r.yMax), new Vector3(x, y1, r.yMin), .075f, art.Railing);
                }
            }
            // Polygonal hoops follow the existing tank instead of adding a second opaque cylinder over it.
            foreach (float y in new[] { 30.2f, 33.5f, 36.8f })
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI * 2f / 32f, b = (i + 1) * Mathf.PI * 2f / 32f;
                Vector3 p = new(r.center.x + Mathf.Cos(a) * 5.53f, y, r.center.y + Mathf.Sin(a) * 5.53f);
                Vector3 q = new(r.center.x + Mathf.Cos(b) * 5.53f, y, r.center.y + Mathf.Sin(b) * 5.53f);
                Beam(g, p, q, .09f, art.Steel);
            }
        }

        private static void Beam(Transform g, Vector3 a, Vector3 b, float width, Material material)
        {
            Vector3 d = b - a;
            GameObject go = Detail("Brace", g, (a + b) * .5f, new Vector3(width, width, d.magnitude), material);
            go.transform.rotation = Quaternion.LookRotation(d.normalized);
        }

        private static void Lanterns(Transform g, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Polish_CagedWallLantern", r =>
            {
                Box("Backplate", r, Vector3.zero, new Vector3(.21f, .52f, .06f), art.Steel, false);
                Box("Bracket", r, new Vector3(0f, .18f, .22f), new Vector3(.075f, .075f, .44f), art.Bars, false);
                Box("Hood", r, new Vector3(0f, .22f, .43f), new Vector3(.42f, .12f, .38f), art.Railing, false);
                Box("Lens", r, new Vector3(0f, -.03f, .43f), new Vector3(.22f, .36f, .21f), art.Lamp, false);
                Box("Base", r, new Vector3(0f, -.26f, .43f), new Vector3(.34f, .08f, .32f), art.Steel, false);
                foreach (float x in new[] { -.15f, .15f })
                foreach (float z in new[] { .3f, .56f })
                    Box("Cage", r, new Vector3(x, -.02f, z), new Vector3(.025f, .46f, .025f), art.Bars, false);
            });
            void Place(Vector3 at, Vector3 front) => IslandMeshKit.Place(kit, "CagedLantern", g, at, Quaternion.LookRotation(front), false);
            foreach (float x in new[] { 50.45f, 57.55f }) Place(new Vector3(x, 15.05f, L.Cellhouse.yMin - .8f), Vector3.back);
            foreach (float z in new[] { 44f, 52f, 88f }) Place(new Vector3(L.Cellhouse.xMax + .03f, 15.1f, z), Vector3.right);
            Place(new Vector3(L.Cellhouse.xMin - .03f, 15.1f, L.YardDoorZ - 2.2f), Vector3.left);
            Place(new Vector3(L.ServiceDoorX - 2.2f, 15.1f, L.Cellhouse.yMax + .03f), Vector3.forward);
            foreach (float x in new[] { 114.2f, 117.8f, 124.2f, 127.8f }) Place(new Vector3(x, 2.7f, L.Barracks.yMin - .03f), Vector3.back);
            foreach (float x in new[] { 38.1f, 41.9f }) Place(new Vector3(x, 14.55f, L.WardenHouse.yMin - .03f), Vector3.back);
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}

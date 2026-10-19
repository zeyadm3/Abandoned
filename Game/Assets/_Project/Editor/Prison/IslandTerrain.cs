using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The rock itself: the plateau (12 m up, sheer cliffs on every side), the quay at sea level under its south face,
    /// the two timber piers either side of the boat's slip, the bay, and the host's sea volume (anything that drops
    /// into the water is gone, like the factory's lava). The cliffs carry strata and boulders at the waterline for
    /// shape, never a ledge a falling player could land on and be stranded. Surf and the foghorn are local sound.
    /// </summary>
    public static class IslandTerrain
    {
        public const string GroupName = "Terrain";
        private const float MassBottom = -16f;

        public static void Build(Transform root, IslandPrisonArt art)
        {
            Transform g = Group(GroupName, root);
            Plateau(Group("Plateau", g), art);
            QuayAndPiers(Group("Quay", g), art);
            Sea(Group("Sea", root), art);
            MarkStatic(g);
        }

        // Under the cellhouse: just below its floor tiles, and the basement floor under the north band. Everything else
        // on the plateau is the landform's terrain (IslandLandform), which leaves the cellhouse's footprint open.
        private static void Plateau(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            Mass(g, "Mass_UnderCells", c.xMin, c.xMax, c.yMin, L.NorthBand.yMin, L.PlateauY - L.Slab, art.Rock);
            GameObject basement = Mass(g, "BasementFloor", c.xMin, c.xMax, L.NorthBand.yMin, c.yMax, L.BasementY, art.Floor);
            basement.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
        }

        private static GameObject Mass(Transform g, string name, float x0, float x1, float z0, float z1, float top, Material m) =>
            K.Span(name, g, new Vector3(x0, MassBottom, z0), new Vector3(x1, top, z1), m);

        private static void QuayAndPiers(Transform g, IslandPrisonArt art)
        {
            Rect q = L.Quay;
            GameObject quay = K.Span("QuayMass", g, new Vector3(q.xMin, MassBottom, q.yMin), new Vector3(q.xMax, L.QuayY, q.yMax), art.FacadeDark);
            quay.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            // Coping along the water's edge: a granite kerb, too low to trip on, that reads as an edge in the dark.
            K.Span("Coping_S_W", g, new Vector3(q.xMin, 0f, -.3f), new Vector3(L.Slip.xMin, .08f, .3f), art.Facade, false);
            K.Span("Coping_S_E", g, new Vector3(L.Slip.xMax, 0f, -.3f), new Vector3(q.xMax, .08f, .3f), art.Facade, false);
            K.Span("Coping_W", g, new Vector3(q.xMin - .3f, 0f, q.yMin), new Vector3(q.xMin + .3f, .08f, q.yMax), art.Facade, false);
            K.Span("Coping_E", g, new Vector3(q.xMax - .3f, 0f, q.yMin), new Vector3(q.xMax + .3f, .08f, L.RoadA.yMin), art.Facade, false);
            // Bollards and mooring rings along the edge.
            for (float x = q.xMin + 3f; x < q.xMax - 2f; x += 6f)
            {
                if (x > L.Slip.xMin - 1f && x < L.Slip.xMax + 1f) continue;
                Primitive(PrimitiveType.Cylinder, "Bollard", g, new Vector3(x, .35f, .8f), new Vector3(.4f, .35f, .4f), art.Rust, false);
            }
            // Painted haul lane from the boat to the cliff stair and the road.
            Transform lane = Group("HaulLane", g);
            K.Span("Lane_Boat", lane, new Vector3(L.Slip.xMin + 2f, .005f, .4f), new Vector3(L.Slip.xMax - 2f, .012f, 10f), art.Asphalt, false);
            // Painted only where nothing stands: it stops short of the barracks.
            K.Span("Lane_East", lane, new Vector3(L.Slip.xMax - 2f, .005f, 6f), new Vector3(L.Barracks.xMin - 1f, .012f, 10f), art.Asphalt, false);
            foreach (float z in new[] { 6f, 10f })
                K.Span("LaneEdge", lane, new Vector3(L.Slip.xMax, .012f, z - .06f), new Vector3(L.Barracks.xMin - 1f, .016f, z + .06f), art.Warning, false);

            foreach ((string name, Rect r) in new[] { ("PierWest", L.PierWest), ("PierEast", L.PierEast) })
            {
                Transform pier = Group(name, g);
                GameObject deck = K.Span("Deck", pier, new Vector3(r.xMin, -.45f, r.yMin), new Vector3(r.xMax, L.QuayY, r.yMax), art.Pier);
                deck.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                for (float z = r.yMin + 1f; z < r.yMax; z += 3f)
                foreach (float x in new[] { r.xMin + .4f, r.xMax - .4f })
                    Primitive(PrimitiveType.Cylinder, "Pile", pier, new Vector3(x, -6f, z), new Vector3(.45f, 6f, .45f), art.DarkWood, false);
                // A broken rail on the outer side only: the slip side stays open for loading.
                float outer = name == "PierWest" ? r.xMin + .1f : r.xMax - .1f;
                K.Railing(pier, "Rail_A", new Vector3(outer, 0f, r.yMin + 1f), new Vector3(outer, 0f, r.yMin + 9f), 1f, art.DarkWood);
                K.Railing(pier, "Rail_B", new Vector3(outer, 0f, r.yMin + 13f), new Vector3(outer, 0f, r.yMax - 1f), 1f, art.DarkWood);
            }
            // The slip's quay wall faces, so the hold doesn't look into the void.
            K.Span("SlipWall_W", g, new Vector3(L.Slip.xMin - .4f, MassBottom, L.Slip.yMin), new Vector3(L.Slip.xMin, -.45f, 0f), art.FacadeDark, false);
            K.Span("SlipWall_E", g, new Vector3(L.Slip.xMax, MassBottom, L.Slip.yMin), new Vector3(L.Slip.xMax + .4f, -.45f, 0f), art.FacadeDark, false);
        }

        private static void Sea(Transform g, IslandPrisonArt art)
        {
            GameObject water = Box("Water", g, new Vector3(70f, L.SeaY, 60f), new Vector3(6000f, .1f, 6000f), art.Sea, false);
            water.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // Rock knuckles standing out of the bay round the island: landmarks from the cliff top.
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f, h = K.Hash(i, 7);
                Vector3 at = new(70f + Mathf.Cos(a) * (165f + h * 50f), L.SeaY - 1f, 70f + Mathf.Sin(a) * (160f + h * 50f));
                if (at.z < 0f && at.x > 20f && at.x < 160f) continue;   // keep the approach to the dock open
                if (IslandLandform.Height(at.x, at.z) > L.SeaY - 2f) continue;
                K.Rock("Islet", g, at + Vector3.down * 2f, new Vector3(6f + h * 10f, 5f + h * 6f, 5f + h * 8f), art.RockDark, i * 31, false);
            }

            Transform kill = Group("SeaVolume", g);
            kill.gameObject.AddComponent<LavaVolume>().EditorSetup(L.SeaKill, LavaVolume.SeaDeathLine);

            Transform navExclude = Group("NavExclude", g);
            void Exclude(string name, Vector3 center, Vector3 size)
            {
                var v = Group(name, navExclude).gameObject.AddComponent<NavMeshModifierVolume>();
                v.center = center;
                v.size = size;
                v.area = 1; // Not Walkable
            }
            Exclude("SeaNav", new Vector3(70f, -40f, 60f), new Vector3(1200f, 75.6f, 1200f));
            Exclude("NorthCliffWestNav", new Vector3(-27.5f, 6f, 209.5f), new Vector3(145f, 30f, 182f));
            Exclude("NorthCliffEastNav", new Vector3(177.5f, 6f, 209.5f), new Vector3(245f, 30f, 182f));
            Exclude("WestCliffNav", new Vector3(-95f, 6f, 80f), new Vector3(205f, 30f, 140f));
            Exclude("SouthWestCliffNav", new Vector3(-21f, 6f, -40f), new Vector3(158f, 30f, 120f));
            Exclude("EastCliffNav", new Vector3(180f, 6f, 100f), new Vector3(110f, 30f, 60f));
            Exclude("SouthEastCliffNav", new Vector3(190f, 6f, -40f), new Vector3(100f, 30f, 120f));

            // Sound: surf where the shore actually meets the water (walked out from the middle on twelve bearings),
            // the foghorn from three bearings out in the bay.
            var surf = new List<Transform>();
            var centre = new Vector2(L.Plateau.center.x, L.Plateau.center.y);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                for (float r = 30f; r < 220f; r += 2f)
                {
                    Vector2 p = centre + dir * r;
                    if (IslandLandform.Open(p.x, p.y) || IslandLandform.Height(p.x, p.y) > L.SeaY) continue;
                    Transform s = Group("Surf", g);
                    s.position = new Vector3(p.x, L.SeaY, p.y);
                    surf.Add(s);
                    break;
                }
            }
            var horns = new List<Transform>();
            foreach (Vector3 at in new[] { new Vector3(-180f, 0f, 220f), new Vector3(320f, 0f, 260f), new Vector3(60f, 0f, -260f) })
            {
                Transform h = Group("Foghorn", g);
                h.position = at;
                horns.Add(h);
            }
            g.gameObject.AddComponent<IslandSoundscape>().EditorSetup(surf.ToArray(), horns.ToArray());
        }
    }
}

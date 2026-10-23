using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
namespace Abandoned.Motel
{
    public sealed class MotelStructure
    {
        private readonly Dictionary<Vector3Int,
        StructuralSection> tiles = new();
        private readonly Dictionary<int,
        StructuralSection> galleryEdges = new();
        private readonly List<(Bounds area, StructuralSection section)> special = new();
        private readonly MotelKit k;
        private readonly StructureSceneSetup setup;
        private Transform slabs;
        public MotelStructure(MotelKit kit, StructureSceneSetup sections)
        {
            k = kit;
            setup = sections;
        }
        public StructuralSection At(Vector3 point)
        {
            foreach (var piece in special)
            {
                if (piece.area.Contains(point))
                {
                    return piece.section;
                }
            }
            int level = Mathf.RoundToInt(point.y / 4f);
            int x = Mathf.FloorToInt(point.x / 4f);
            int z = Mathf.FloorToInt(point.z / 4f);
            if (level == 1 && z == 3 && point.z <14f)
            {
                return galleryEdges.TryGetValue(x, out StructuralSection edge) ? edge : null;
            }
            return tiles.TryGetValue(new Vector3Int(x, level, z), out StructuralSection tile) ? tile : null;
        }
        public void Build(Transform root)
        {
            slabs = MotelKit.Group("Structure", root);
            for (int level = 0; level <2; level++)
            {
                for (int z = 3; z <7; z++)
                {
                    for (int x = 0; x <16; x++)
                    {
                        if (level == 1 && z == 3)
                        {
                            // The outer half can peel away without cutting the inner route between both stairs.
                            StructuralSection edge = Slab(new Vector3(x * 4f + 2f, 4f, 13f), new Vector3(4f, .32f, 2f), true, false, k.Metal);
                            galleryEdges [x] = edge;
                            StructuralSection lane = Slab(new Vector3(x * 4f + 2f, 4f, 15f), new Vector3(4f, .32f, 2f), false, true, k.Metal);
                            tiles [new Vector3Int(x, level, z)] = lane;
                            Transform rail = MotelKit.Group("OuterGalleryGuard", root);
                            k.Rail(rail, new Vector3(x * 4f, 4f, 12f), new Vector3(x * 4f + 4f, 4f, 12f));
                            k.Attach(rail, edge);
                            continue;
                        }
                        bool protectedLane = z <= 4;
                        bool falls = level> 0 && ! protectedLane;
                        StructuralSection section = Slab(new Vector3(x * 4f + 2f, level * 4f, z * 4f + 2f), new Vector3(4f, .32f, 4f), falls, protectedLane, z == 3 ? k.Metal : k.Floor);
                        tiles [new Vector3Int(x, level, z)] = section;
                    }
                }
            }
            Stair(root, - 2f);
            Stair(root, 66f);
            for (int i = 0; i <3; i++)
            {
                Vector3 at = new(12f, 0f, 3f + i * 2f);
                StructuralSection deck = Slab(at, new Vector3(4f, .3f, 2f), true, false, k.Wood);
                special.Add((new Bounds(at, new Vector3(4f, .8f, 2f)), deck));
                // A skirt closes the low space beneath the deck until that section actually falls.
                var skirt = deck.gameObject.AddComponent<BoxCollider>();
                skirt.center = new Vector3(0f, - .55f, 0f);
                skirt.size = new Vector3(3.95f, .5f, 1.95f);
                Transform apron = k.Box("ClosedDeckApron", deck.transform, skirt.center, skirt.size, k.Wood, false).transform;
                k.Attach(apron, deck);
                Transform guard = MotelKit.Group("PoolDeckGuard", root);
                k.Rail(guard, new Vector3(14f, 0f, at.z - 1f), new Vector3(14f, 0f, at.z + 1f));
                k.Attach(guard, deck);
            }
        }
        public void AddArea(Rect area)
        {
            for (float x = area.xMin; x <area.xMax; x += 4f)
            {
                for (float z = area.yMin; z <area.yMax; z += 4f)
                {
                    Vector3 at = new(x + 2f, 0f, z + 2f);
                    StructuralSection section = Slab(at, new Vector3(4f, .32f, 4f), false, true, k.Floor);
                    tiles [new Vector3Int(Mathf.FloorToInt(at.x / 4f), 0, Mathf.FloorToInt(at.z / 4f))] = section;
                }
            }
        }
        private StructuralSection Slab(Vector3 at, Vector3 size, bool falls, bool clear, Material material)
        {
            Transform tile = MotelKit.Group("Slab_" + at, slabs);
            tile.position = at;
            var solid = tile.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.down *(size.y * .5f);
            solid.size = size;
            k.Box("Visual", tile, solid.center, size, material, false);
            StructuralSection section = setup.AddSection(tile, SectionType.Floor, falls, falls ? .7f : 1f, falls ? .78f : 1f);
            tile.gameObject.AddComponent<SurfaceTag>().EditorSet(material == k.Metal ? SurfaceMaterial.Metal : material == k.Wood ? SurfaceMaterial.Wood : SurfaceMaterial.Carpet);
            if (clear)
            {
                tile.gameObject.AddComponent<SectionKeepClear>();
            }
            if (falls)
            {
                SectionNavCarver.EditorAdd(tile.gameObject, size);
            }
            return section;
        }
        private void Stair(Transform parent, float x)
        {
            Transform t = MotelKit.Group(x <0 ? "WestStair" : "EastStair", parent);
            t.localPosition = new Vector3(x, 0, 4);
            Transform visual = MotelKit.Group("Visual", t),
            ramp = MotelKit.Group("Ramp", t);
            ramp.localPosition = new Vector3(0, 1.9f, 4);
            ramp.localRotation = Quaternion.Euler(- 26.565f, 0, 0);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(3.2f, .2f, Mathf.Sqrt(80));
            for (int i = 0; i <22; i++)
            {
                k.Box("Tread", visual, new Vector3(0,(i + 1) * 4f / 22 - .04f,(i + .5f) * 8f / 22), new Vector3(3.2f, .08f, 8f / 22), k.Metal, false);
            }
            foreach (float side in new []
            {
                - 1.65f, 1.65f
            })
            {
                Transform g = MotelKit.Group("StairGuard", visual);
                g.localPosition = new Vector3(side, 2.55f, 4);
                g.localRotation = Quaternion.Euler(- 26.565f, 0, 0);
                k.Box("Guard", g, Vector3.zero, new Vector3(.06f, 1.1f, Mathf.Sqrt(80)), k.Metal);
            }
            // Close the low underside: a player or a dropped case must not enter a headroom trap.
            for (int block = 1; block <4; block++)
            {
                float height = block - .45f;
                k.Box("UnderStairFill", visual, new Vector3(0f, height * .5f, block * 2f + 1f), new Vector3(3.18f, height, 1.99f), k.Wall);
            }
            setup.AddSection(t, SectionType.Stair, false, fractured : false);
            t.gameObject.AddComponent<SectionKeepClear>();
            var link = t.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = new Vector3(0, 0, - .7f);
            link.endPoint = new Vector3(0, 4, 8.7f);
            link.width = 2.7f;
            link.bidirectional = true;
            foreach (float y in new []
            {
                0f, 4f
            })
            {
                Transform land = MotelKit.Group("StairLanding", parent);
                land.localPosition = new Vector3(x, y, y == 0 ? 2 : 14);
                var c = land.gameObject.AddComponent<BoxCollider>();
                c.center = Vector3.down * .15f;
                c.size = new Vector3(4, .3f, 4);
                k.Box("Visual", land, c.center, c.size, k.Metal, false);
                setup.AddSection(land, SectionType.Walkway, false, fractured : false);
                land.gameObject.AddComponent<SectionKeepClear>();
                k.Rail(land, new Vector3(x <0 ? - 2 : 2, 0, - 2), new Vector3(x <0 ? - 2 : 2, 0, 2));
            }
        }
    }
}

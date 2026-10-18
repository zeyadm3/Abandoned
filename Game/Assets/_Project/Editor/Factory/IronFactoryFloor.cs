using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The machinery floor: one 4 m structural section per cell, the channel's cells left open. The ground
    /// never collapses (nothing under it but earth), but it is a real section so loads, impacts and rubble
    /// from the catwalks above land on it the way they do in the mall.
    /// </summary>
    public static class IronFactoryFloor
    {
        public static List<Transform> Build(Transform root, IronFactoryArt art)
        {
            Transform group = Group("GroundFloor", Group("Levels", root));
            var tiles = new List<Transform>();
            foreach (Vector2Int cell in L.GroundCells())
            {
                Transform tile = Group($"Tile_0_{cell.x}_{cell.y}", group);
                tile.localPosition = L.CellCentre(cell);
                BoxCollider collider = tile.gameObject.AddComponent<BoxCollider>();
                collider.center = Vector3.down * (L.Slab * .5f);
                collider.size = new Vector3(L.Tile, L.Slab, L.Tile);
                Box("Visual", tile, collider.center, collider.size, art.Floor, false);
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
                tiles.Add(tile);
            }
            return tiles;
        }

        /// <summary>Registers the tiles with the structure simulation. Only the dock lane is kept clear of
        /// rubble: everywhere else a fallen catwalk piles up on the machinery floor.</summary>
        public static void AddSections(StructureSceneSetup setup, IEnumerable<Transform> tiles)
        {
            foreach (Transform tile in tiles)
            {
                setup.AddSection(tile, SectionType.Floor, collapsible: false);
                Vector3 p = tile.position;
                var cell = new Vector2Int(Mathf.FloorToInt(p.x / L.Tile), Mathf.FloorToInt(p.z / L.Tile));
                if (L.DockLane.Contains(cell)) tile.gameObject.AddComponent<SectionKeepClear>();
            }
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Kind = Abandoned.EditorTools.IronFactoryCatwalkPlan.Kind;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Plan = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the catwalk tiers from <see cref="IronFactoryCatwalkPlan"/>: one structural walkway section per
    /// cell, a 2 m steel deck (1.2 m on a narrow bridge) with arms out to every open side and mesh guard panels
    /// on every closed one, hung from the roof. Colliders sit beside the Visual, so on a collapse the deck,
    /// guards and hangers break apart as debris while the colliders switch off.
    /// </summary>
    public static class IronFactoryCatwalks
    {
        private const float Deck = .16f;
        private const float GuardHeight = 1.05f;
        private const float GuardThickness = .05f;

        // Capacity and starting health by tier (higher = lighter steel) and by kind.
        private static readonly float[] TierCapacity = { 1f, .8f, .65f, .5f };
        private const float WeakCapacity = .55f, WeakHealth = .55f;

        public sealed class Piece
        {
            public Transform Transform;
            public int Level;
            public Kind Kind;
            public Vector2Int Cell;
        }

        public static List<Piece> Build(Transform root, IronFactoryArt art, Dictionary<Vector2Int, Kind>[] cells)
        {
            var pieces = new List<Piece>();
            Transform levels = root.Find("Levels") ?? Group("Levels", root);
            for (int level = 1; level < cells.Length; level++)
            {
                Transform tier = Group($"Tier{level}", levels);
                foreach (KeyValuePair<Vector2Int, Kind> entry in cells[level])
                    pieces.Add(Cell(tier, art, cells, level, entry.Key, entry.Value));
            }
            return pieces;
        }

        private static Piece Cell(Transform tier, IronFactoryArt art, Dictionary<Vector2Int, Kind>[] cells, int level,
            Vector2Int cell, Kind kind)
        {
            float height = Plan.Height(level);
            Transform root = Group($"Walk_{level}_{cell.x}_{cell.y}", tier);
            root.position = L.CellCentre(cell, height);
            Transform visual = Group("Visual", root);
            Material deck = kind == Kind.Weak ? art.Rust : art.Steel;
            Material guard = kind == Kind.Weak ? art.Rust : art.Steel;

            float half = (kind == Kind.Narrow ? 1.2f : 2f) * .5f;
            float arm = L.Tile * .5f - half;
            Solid(root, visual, "Deck", new Vector3(0f, -Deck * .5f, 0f), new Vector3(half * 2f, Deck, half * 2f), deck);
            List<Vector2Int> open = Plan.OpenSides(cells, level, cell);
            foreach (Vector2Int side in Plan.Sides)
            {
                Vector3 d = new(side.x, 0f, side.y);
                Vector3 across = new(Mathf.Abs(side.y), 0f, Mathf.Abs(side.x));
                string tag = SideName(side);
                if (open.Contains(side))
                {
                    if (arm > .01f)
                    {
                        Vector3 mid = d * (half + arm * .5f);
                        Solid(root, visual, "Arm_" + tag, mid + Vector3.down * Deck * .5f, Flat(across, half * 2f, arm, Deck), deck);
                        Solid(root, visual, "Guard_" + tag + "_A", mid + across * half + Vector3.up * GuardHeight * .5f,
                            Flat(across, GuardThickness, arm, GuardHeight), guard);
                        Solid(root, visual, "Guard_" + tag + "_B", mid - across * half + Vector3.up * GuardHeight * .5f,
                            Flat(across, GuardThickness, arm, GuardHeight), guard);
                    }
                }
                else
                {
                    Solid(root, visual, "Guard_" + tag, d * half + Vector3.up * GuardHeight * .5f,
                        Flat(across, half * 2f, GuardThickness, GuardHeight), guard);
                }
            }
            // A hanger from the roof truss: only on every other cell, so long runs read as hung, not stilted.
            if ((cell.x + cell.y) % 2 == 0)
            {
                float rise = L.WallHeight - height;
                Box("Hanger", visual, new Vector3(0f, rise * .5f, 0f), new Vector3(.06f, rise, .06f), art.Steel, false);
            }
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return new Piece { Transform = root, Level = level, Kind = kind, Cell = cell };
        }

        /// <summary>Registers the walkways: spines never fall and keep rubble off; the rest can collapse and
        /// carve their cell out of the NavMesh when they do.</summary>
        public static void AddSections(StructureSceneSetup setup, IEnumerable<Piece> pieces)
        {
            foreach (Piece p in pieces)
            {
                bool collapsible = p.Kind != Kind.Spine;
                float capacity = TierCapacity[p.Level] * (p.Kind == Kind.Weak ? WeakCapacity : 1f);
                float health = p.Kind == Kind.Weak ? WeakHealth : 1f;
                setup.AddSection(p.Transform, SectionType.Walkway, collapsible, health, capacity, fractured: false);
                if (collapsible) SectionNavCarver.EditorAdd(p.Transform.gameObject, new Vector3(L.Tile, L.Slab, L.Tile));
                else p.Transform.gameObject.AddComponent<SectionKeepClear>();
            }
        }

        // A collider on the section root's side and its look in the Visual, so a collapse can break the look
        // apart (StructureSceneSetup relays child colliders to the section).
        private static void Solid(Transform root, Transform visual, string name, Vector3 centre, Vector3 size, Material material)
        {
            Transform solid = Group(name, root);
            solid.localPosition = centre;
            solid.gameObject.AddComponent<BoxCollider>().size = size;
            Box(name, visual, centre, size, material, false);
        }

        // A box whose "run" lies along the side's length axis and whose "depth" points across it.
        private static Vector3 Flat(Vector3 across, float width, float run, float tall) =>
            across.x > .5f ? new Vector3(width, tall, run) : new Vector3(run, tall, width);

        private static string SideName(Vector2Int side) =>
            side == Vector2Int.up ? "N" : side == Vector2Int.down ? "S" : side == Vector2Int.right ? "E" : "W";
    }
}

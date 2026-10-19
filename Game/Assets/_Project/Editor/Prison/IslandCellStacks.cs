using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// B and C blocks: free-standing concrete stacks of cells, two rows back to back around a pipe chase, three tiers.
    /// The cells are scenery (some open to walk into, most shut, a few to hide in); the galleries along both faces
    /// are the structure: one walkway section per 4 m, rusted steel plate on knee-brackets, so the signature choice
    /// is carrying a haul along a tier-2 gallery to the stair. Each stack has a switchback stair tower at its south
    /// end that never falls, end wraps that never fall, and one protected tier-1 spine. B-block's tier-2 west gallery
    /// has already lost a section: the wound shows what the floors will do.
    /// </summary>
    public static class IslandCellStacks
    {
        private const float Deck = .12f;
        private const float StackTop = L.PlateauY + 12f;

        public sealed class Gallery
        {
            public Transform Transform;
            public bool Collapsible;
            public float Capacity, Health;
            public Vector2 Size;
        }

        public sealed class Result
        {
            public readonly List<IslandFlights.Built> Flights = new();
            public readonly List<Gallery> Galleries = new();
            public int Cells, OpenCells, HideCells;
        }

        public static Result Build(Transform root, IslandPrisonArt art, IslandCellKits kits, List<LootPointSpec> loot)
        {
            var result = new Result();
            Transform blocks = Group("CellBlocks", root);
            for (int s = 0; s < L.Stacks.Length; s++)
            {
                L.Stack stack = L.Stacks[s];
                Transform g = Group(stack.Name, blocks);
                Shell(g, stack, art, kits);
                Transform cells = Group("Cells", g);
                for (int tier = 0; tier < L.Tiers.Length; tier++)
                for (int k = 0; k < L.CellsPerRow; k++)
                {
                    Cell(cells, s, stack, tier, k, true, art, kits, loot, result);
                    Cell(cells, s, stack, tier, k, false, art, kits, loot, result);
                }
                Galleries(Group("Galleries", root.Find("Levels") ?? Group("Levels", root)), s, stack, art, kits, result);
                StairTower(Group(stack.Name + "_Stair", root), stack, art, result);
            }
            Wound(Group("Wound", blocks), L.Stacks[1], art);
            return result;
        }

        // The concrete body: floor slabs for tiers 1-2, the roof, the chase walls and pipes, the cell dividers, end walls.
        private static void Shell(Transform g, L.Stack s, IslandPrisonArt art, IslandCellKits kits)
        {
            float x0 = s.CellsWest, x1 = s.CellsEast, z0 = L.StackSouth, z1 = L.StackNorth;
            for (int tier = 1; tier < L.Tiers.Length; tier++)
            {
                GameObject slab = K.Span($"Slab_T{tier}", g, new Vector3(x0 - .03f, L.TierY(tier) - .3f, z0 - .03f), new Vector3(x1 + .03f, L.TierY(tier), z1 + .03f), art.Floor);
                slab.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            }
            K.Span("Roof", g, new Vector3(x0 - .1f, StackTop - .3f, z0 - .1f), new Vector3(x1 + .1f, StackTop, z1 + .1f), art.Ceiling);
            // The chase: two walls the cells back onto, a pipe run per tier inside, steel doors at both ends.
            K.Span("ChaseWall_W", g, new Vector3(s.ChaseWest, L.PlateauY, z0), new Vector3(s.ChaseWest + .15f, StackTop - .3f, z1), art.Plaster);
            K.Span("ChaseWall_E", g, new Vector3(s.ChaseEast - .15f, L.PlateauY, z0), new Vector3(s.ChaseEast, StackTop - .3f, z1), art.Plaster);
            for (int tier = 0; tier < L.Tiers.Length; tier++)
            for (float z = z0; z < z1 - .1f; z += 4f)
                IslandMeshKit.Place(kits.Pipes, "Pipes", g, new Vector3((s.ChaseWest + s.ChaseEast) * .5f, L.TierY(tier), z), Quaternion.identity, false);
            foreach (float z in new[] { z0, z1 })
            {
                float face = z == z0 ? z0 - .02f : z1 + .02f;
                K.Span("ChaseDoor", g, new Vector3(s.ChaseWest + .15f, L.PlateauY, face - .03f), new Vector3(s.ChaseEast - .15f, L.PlateauY + 2.1f, face + .03f), art.Steel);
            }
            // Dividers between cells, full height (one box per line per row); thick end walls.
            for (int k = 0; k <= L.CellsPerRow; k++)
            {
                float z = z0 + k * L.CellWidth;
                bool end = k == 0 || k == L.CellsPerRow;
                float t = end ? .3f : .14f;
                float za = end && k == 0 ? z - t : z - t * .5f, zb = end && k == 0 ? z : z + t * .5f;
                if (k == L.CellsPerRow) { za = z; zb = z + t; }
                K.Span($"Div_W_{k}", g, new Vector3(x0, L.PlateauY, za), new Vector3(s.ChaseWest, StackTop - .3f, zb), art.Plaster);
                K.Span($"Div_E_{k}", g, new Vector3(s.ChaseEast, L.PlateauY, za), new Vector3(x1, StackTop - .3f, zb), art.Plaster);
            }
            // End walls cover the chase too, with the stack's painted letter panel.
            K.Span("End_S", g, new Vector3(x0, L.PlateauY + 2.1f, z0 - .3f), new Vector3(x1, StackTop - .3f, z0), art.Plaster);
            K.Span("End_N", g, new Vector3(x0, L.PlateauY + 2.1f, z1), new Vector3(x1, StackTop - .3f, z1 + .3f), art.Plaster);
            MarkStatic(g);
        }

        private static void Cell(Transform parent, int stackIndex, L.Stack s, int tier, int k, bool west, IslandPrisonArt art,
            IslandCellKits kits, List<LootPointSpec> loot, Result result)
        {
            float y = L.TierY(tier);
            float zc = L.StackSouth + (k + .5f) * L.CellWidth;
            // Back wall to front: west row faces west (out to its gallery), east row faces east.
            Vector3 back = new(west ? s.ChaseWest : s.ChaseEast, y, zc);
            Quaternion facing = Quaternion.LookRotation(west ? Vector3.left : Vector3.right, Vector3.up);
            Transform cell = Group($"Cell_{(west ? "W" : "E")}{tier}_{k:00}", parent);
            cell.SetPositionAndRotation(back, facing);
            result.Cells++;

            float h = K.Hash(stackIndex * 1000 + tier * 100 + (west ? 0 : 50) + k, 17);
            bool spineSide = s.SpineWest == west;
            bool canHide = tier == 0 || tier == 1 && spineSide;
            bool fallenFront = stackIndex == 1 && tier == 2 && west && k / 2 == L.FallenGallerySection;
            CellState state = fallenFront ? CellState.Shut
                : canHide && h < .13f && !IslandHideSpots.Full ? CellState.Hide
                : h < .55f ? CellState.Open : CellState.Shut;

            kits.PlaceFurnishing(cell, K.Hash(k + (west ? 0 : 71), tier * 7 + stackIndex));
            Transform front = IslandMeshKit.Place(kits.Fixed, "Front", cell, cell.TransformPoint(new Vector3(0f, 0f, L.CellDepth)), cell.rotation);
            float doorX = state == CellState.Open ? IslandCellKits.FixedTo - .8f : IslandCellKits.FixedTo;
            Transform door = IslandMeshKit.Place(kits.Door, "Door", cell, cell.TransformPoint(new Vector3(doorX, 0f, L.CellDepth + .06f)), cell.rotation);
            _ = front;

            if (state == CellState.Hide)
            {
                IslandHideSpots.Wire(cell, door, new Vector3(1.9f, IslandCellKits.FrontHeight, L.CellDepth), new Vector3(.05f, .02f, 1.05f), HideSpotKind.Cell);
                result.HideCells++;
                return;
            }
            IslandCellKits.CellSolids(cell);
            if (state == CellState.Shut)
            {
                IslandMeshKit.Solid(cell, new Vector3(0f, IslandCellKits.FrontHeight * .5f, L.CellDepth), new Vector3(2f, IslandCellKits.FrontHeight, .12f));
                return;
            }
            result.OpenCells++;
            IslandMeshKit.Solid(cell, new Vector3(-.45f, IslandCellKits.FrontHeight * .5f, L.CellDepth), new Vector3(1.1f, IslandCellKits.FrontHeight, .12f));
            IslandMeshKit.Solid(cell, new Vector3(.55f, (IslandCellKits.DoorHeight + IslandCellKits.FrontHeight) * .5f, L.CellDepth),
                new Vector3(.9f, IslandCellKits.FrontHeight - IslandCellKits.DoorHeight, .12f));
            // A cell left open is where a stash might still be: small things only (contraband).
            if (K.Hash(k + 31, tier + stackIndex * 3) < .45f)
                loot.Add(new LootPointSpec($"Cell_{s.Name}_{(west ? "W" : "E")}{tier}_{k}", IslandLootItems.Contraband,
                    // Just inside the open doorway (the door slides left, the opening is x .2-1): in the cell, by the gallery.
                    cell.TransformPoint(new Vector3(.6f, .12f, L.CellDepth - .15f)), CarryClass.Pocket, CarryClass.OneHand, false));
        }

        private enum CellState { Open, Shut, Hide }

        private static void Galleries(Transform levels, int stackIndex, L.Stack s, IslandPrisonArt art, IslandCellKits kits, Result result)
        {
            Transform g = Group(s.Name + "_Galleries", levels);
            for (int tier = 1; tier < L.Tiers.Length; tier++)
            {
                float y = L.TierY(tier);
                foreach (bool west in new[] { true, false })
                for (int i = 0; i < L.GallerySections; i++)
                {
                    if (stackIndex == 1 && tier == 2 && west && i == L.FallenGallerySection) continue;
                    bool spine = tier == 1 && s.SpineWest == west;
                    bool weak = IsWeak(stackIndex, tier, west, i) || stackIndex == 1 && tier == 2 && west && Mathf.Abs(i - L.FallenGallerySection) == 1;
                    float z0 = L.StackSouth + i * 4f;
                    float xa = west ? s.West : s.CellsEast, xb = west ? s.CellsWest : s.East;
                    Gallery piece = Deck_(g, $"Gallery_{(west ? "W" : "E")}{tier}_{i}", new Rect(xa, z0, xb - xa, 4f), y,
                        weak ? art.Rust : art.Plate, art);
                    // The rail on the hall side, and the brackets under the deck from the cell face.
                    float rail = west ? s.West + .05f : s.East - .05f;
                    IslandMeshKit.Place(kits.GalleryRail, "Rail", piece.Transform.Find("Visual"), new Vector3(rail, y, z0), Quaternion.identity);
                    RailSolid(piece.Transform, new Vector3(rail, y, z0 + 2f), 4f, true);
                    Vector3 bracketAt = west ? new Vector3(s.CellsWest, y, z0 + 4f) : new Vector3(s.CellsEast, y, z0);
                    IslandMeshKit.Place(kits.GalleryBrackets, "Brackets", piece.Transform.Find("Visual"), bracketAt,
                        west ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity, false);
                    piece.Collapsible = !spine;
                    piece.Capacity = (tier == 1 ? .75f : .6f) * (weak ? .55f : 1f);
                    piece.Health = weak ? .5f : 1f;
                    result.Galleries.Add(piece);
                }
                // End wraps across the stack's ends: never fall (they tie the galleries to the stair tower).
                Rect south = new(s.West, L.StackSouth - 2f, s.East - s.West, 2f);
                Rect north = new(s.West, L.StackNorth, s.East - s.West, 2f);
                Gallery ws = Deck_(g, $"Wrap_S{tier}", south, y, art.Plate, art);
                // South edge rail between the flights' ends; side rails on both ends.
                float southFrom = s.CellsWest, southTo = tier == 1 ? s.CellsEast : s.East;
                RailRun(ws.Transform, kits, new Vector3(southFrom, y, south.yMin + .05f), new Vector3(southTo, y, south.yMin + .05f), art);
                RailRun(ws.Transform, kits, new Vector3(s.West + .05f, y, south.yMin), new Vector3(s.West + .05f, y, south.yMax), art);
                RailRun(ws.Transform, kits, new Vector3(s.East - .05f, y, south.yMin), new Vector3(s.East - .05f, y, south.yMax), art);
                result.Galleries.Add(ws);
                Gallery wn = Deck_(g, $"Wrap_N{tier}", north, y, art.Plate, art);
                RailRun(wn.Transform, kits, new Vector3(s.West + .05f, y, north.yMin), new Vector3(s.West + .05f, y, north.yMax), art);
                RailRun(wn.Transform, kits, new Vector3(s.East - .05f, y, north.yMin), new Vector3(s.East - .05f, y, north.yMax), art);
                result.Galleries.Add(wn);
            }
        }

        private static bool IsWeak(int stack, int tier, bool west, int index)
        {
            foreach ((int st, int t, bool w, int i) in L.WeakGalleries)
                if (st == stack && t == tier && w == west && i == index) return true;
            return false;
        }

        // A walkway section: root at the deck's top centre, collider on the root, the plate in the Visual.
        private static Gallery Deck_(Transform parent, string name, Rect area, float y, Material plate, IslandPrisonArt art)
        {
            Transform root = Group(name, parent);
            root.position = new Vector3(area.center.x, y, area.center.y);
            var c = root.gameObject.AddComponent<BoxCollider>();
            c.center = Vector3.down * (Deck * .5f);
            c.size = new Vector3(area.width, Deck, area.height);
            Transform visual = Group("Visual", root);
            K.WorldBox("Plate", visual, root.position + Vector3.down * (Deck * .5f), new Vector3(area.width, Deck, area.height), plate, false);
            K.WorldBox("Edge", visual, root.position + Vector3.down * .175f, new Vector3(area.width + .04f, .1f, area.height + .04f), art.Steel, false);
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return new Gallery { Transform = root, Collapsible = false, Capacity = .8f, Health = 1f, Size = area.size };
        }

        private static void RailRun(Transform piece, IslandCellKits kits, Vector3 from, Vector3 to, IslandPrisonArt art)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            if (len < .2f) return;
            Transform visual = piece.Find("Visual");
            // Railing works in its parent's space; the piece sits at the deck's centre, unrotated.
            K.Railing(visual, "Rail", visual.InverseTransformPoint(from), visual.InverseTransformPoint(to), 1.05f, art.Railing, false);
            RailSolid(piece, (from + to) * .5f, len, Mathf.Abs(d.z) > Mathf.Abs(d.x));
        }

        private static void RailSolid(Transform piece, Vector3 centre, float length, bool alongZ)
        {
            Transform solid = Group("RailSolid", piece);
            solid.position = centre + Vector3.up * .55f;
            solid.gameObject.AddComponent<BoxCollider>().size = alongZ ? new Vector3(.06f, 1.1f, length) : new Vector3(length, 1.1f, .06f);
        }

        // South of each stack: ground -> tier 1 up the west line, tier 1 -> tier 2 back down the east line to a landing,
        // then a short walkway north to the tier-2 wrap. Monsters use it too (links on each flight).
        private static void StairTower(Transform g, L.Stack s, IslandPrisonArt art, Result result)
        {
            float wx = s.West + L.GalleryWidth * .5f, ex = s.East - L.GalleryWidth * .5f;
            float t1 = L.TierY(1), t2 = L.TierY(2);
            float south = L.StackSouth - 2f;
            result.Flights.Add(IslandFlights.Flight(g, new L.Flight(s.Name + "_Flight1", new Vector3(wx, L.PlateauY, south - 8f), Vector3.forward, 8f, 4f, L.GalleryWidth, false), art.Plate, art.Railing));
            result.Flights.Add(IslandFlights.Flight(g, new L.Flight(s.Name + "_Flight2", new Vector3(ex, t1, south), Vector3.back, 8f, 4f, L.GalleryWidth, false), art.Plate, art.Railing));
            Rect landing = new(s.West, south - 10f, s.East - s.West, 2f);
            Vector3 lsw = new(landing.xMin, t2, landing.yMin), lse = new(landing.xMax, t2, landing.yMin),
                lnw = new(landing.xMin, t2, landing.yMax), lne = new(landing.xMax, t2, landing.yMax);
            result.Flights.Add(IslandFlights.Landing(g, new L.Landing(s.Name + "_Landing2", landing, t2, false), art.Plate, art.Railing,
                lsw, lse, lsw, lnw, lse, lne, lnw + Vector3.right * L.GalleryWidth, lne - Vector3.right * L.GalleryWidth));
            Rect link = new(s.West, south - 8f, L.GalleryWidth, 8f);
            result.Flights.Add(IslandFlights.Landing(g, new L.Landing(s.Name + "_Link2", link, t2, false), art.Plate, art.Railing,
                new Vector3(link.xMin, t2, link.yMin), new Vector3(link.xMin, t2, link.yMax),
                new Vector3(link.xMax, t2, link.yMin), new Vector3(link.xMax, t2, link.yMax)));
            // A steel column under the landing's far corners: it's hung from the roof trusses as well, but it reads.
            foreach (float x in new[] { landing.xMin + .1f, landing.xMax - .1f })
                Box("Column", g, new Vector3(x, (L.PlateauY + t2 - .3f) * .5f, landing.yMin + .1f), new Vector3(.18f, t2 - .3f - L.PlateauY, .18f), art.Steel);
        }

        // The section of B-block's tier-2 west gallery that already came down: torn stubs, a rail hanging by one post,
        // and the deck folded onto the hall floor in a rubble heap (solid: it's in the way, but Broadway stays wide).
        private static void Wound(Transform g, L.Stack s, IslandPrisonArt art)
        {
            float y = L.TierY(2), z0 = L.StackSouth + L.FallenGallerySection * 4f;
            K.RotBox("TornStub_S", g, new Vector3(s.West + .8f, y - .4f, z0 + .3f), new Vector3(1.5f, .1f, .8f), Quaternion.Euler(28f, 0f, 6f), art.Rust, false);
            K.RotBox("TornStub_N", g, new Vector3(s.West + .8f, y - .3f, z0 + 3.7f), new Vector3(1.5f, .1f, .7f), Quaternion.Euler(-24f, 0f, -4f), art.Rust, false);
            K.RotBox("HangingRail", g, new Vector3(s.West + .1f, y - 1.6f, z0 + 2.6f), new Vector3(.06f, .06f, 3.8f), Quaternion.Euler(52f, 0f, 0f), art.Railing, false);
            K.RotBox("HangingBracket", g, new Vector3(s.CellsWest - .3f, y - .9f, z0 + 2f), new Vector3(.08f, 1.4f, .08f), Quaternion.Euler(0f, 0f, 30f), art.Steel, false);
            // The heap on the ground below, against the stack's tier-0 gallery line.
            Transform heap = Group("FallenDeck", g);
            K.RotBox("Deck", heap, new Vector3(s.West - .6f, L.PlateauY + .6f, z0 + 2f), new Vector3(1.6f, .12f, 4.2f), Quaternion.Euler(8f, 4f, 38f), art.Rust);
            K.RotBox("Rail", heap, new Vector3(s.West - 1.6f, L.PlateauY + .25f, z0 + 1.5f), new Vector3(.08f, .08f, 4f), Quaternion.Euler(4f, 12f, 0f), art.Railing, false);
            for (int i = 0; i < 9; i++)
            {
                float h = K.Hash(i, 911);
                GameObject chunk = PolishAssets.Shape("Chunk", heap, new Vector3(s.West - .4f - h * 2f, L.PlateauY + .15f, z0 + .3f + i * .45f),
                    new Vector3(.4f + h * .5f, .25f + h * .3f, .4f + h * .4f), h > .5f ? art.FacadeDark : art.Rust, true);
                chunk.transform.rotation = Quaternion.Euler(h * 40f, h * 300f, h * 20f);
            }
            var mound = heap.gameObject.AddComponent<BoxCollider>();
            mound.center = new Vector3(s.West - 1f, L.PlateauY + .35f, z0 + 2f);
            mound.size = new Vector3(2.2f, .7f, 4.2f);
            K.Span("Dust", heap, new Vector3(s.West - 3f, L.PlateauY + .005f, z0 - .5f), new Vector3(s.West + .2f, L.PlateauY + .012f, z0 + 4.5f), art.Gravel, false);
            MarkStatic(g);
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Kit = System.Collections.Generic.List<(UnityEngine.Mesh mesh, UnityEngine.Material material)>;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The cell fittings as mesh kits (<see cref="IslandMeshKit"/>). Fronts are in the front's own space: x across the
    /// 2 m front (-1 .. 1), y up from the floor, z out of the cell (the front plane at 0). A door leaf is in its own
    /// space from its left edge. Furnishings are in the cell's space: x across, z from the back wall (0) to the front
    /// (2.6). Eight furnishing states share the same fixed footprints but keep the rows from feeling cloned.
    /// </summary>
    public sealed class IslandCellKits
    {
        public const float FrontHeight = 3.4f, DoorWidth = .9f, DoorHeight = 2.3f, FixedFrom = -1f, FixedTo = .1f;
        private const float Bar = .028f, BarStep = .11f;

        public readonly Kit Fixed, Door, SolitaryDoor, SolitaryFront, Made, Stripped, Ransacked, GalleryRail, GalleryBrackets, Pipes;
        private readonly Kit[] furnishings;

        public IslandCellKits(IslandPrisonArt art)
        {
            Fixed = IslandMeshKit.Get("CellFront_Fixed", r => FixedFront(r, art));
            Door = IslandMeshKit.Get("CellFront_Door", r => DoorLeaf(r, art));
            SolitaryDoor = IslandMeshKit.Get("Solitary_Door", r => Solitary(r, art));
            SolitaryFront = IslandMeshKit.Get("Solitary_Front", r => SolitaryWall(r, art));
            Made = IslandMeshKit.Get("Cell_Made", r => Furnish(r, art, 0));
            Stripped = IslandMeshKit.Get("Cell_Stripped", r => Furnish(r, art, 1));
            Ransacked = IslandMeshKit.Get("Cell_Ransacked", r => Furnish(r, art, 2));
            furnishings = new[]
            {
                Made, Stripped, Ransacked,
                IslandMeshKit.Get("Cell_Folded", r => Furnish(r, art, 3)),
                IslandMeshKit.Get("Cell_Mended", r => Furnish(r, art, 4)),
                IslandMeshKit.Get("Cell_Damp", r => Furnish(r, art, 5)),
                IslandMeshKit.Get("Cell_Memories", r => Furnish(r, art, 6)),
                IslandMeshKit.Get("Cell_Evacuated", r => Furnish(r, art, 7)),
            };
            GalleryRail = IslandMeshKit.Get("Gallery_Rail4", r => Rail(r, art));
            GalleryBrackets = IslandMeshKit.Get("Gallery_Brackets4", r => Brackets(r, art));
            Pipes = IslandMeshKit.Get("Chase_Pipes4", r => ChasePipes(r, art));
        }

        public Kit Furnishing(float hash) => furnishings[Mathf.Clamp(Mathf.FloorToInt(hash * furnishings.Length), 0, furnishings.Length - 1)];

        public Transform PlaceFurnishing(Transform cell, float hash)
        {
            Transform visual = IslandMeshKit.Place(Furnishing(hash), "Furnishing", cell, cell.position, cell.rotation);
            visual.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Detailed cell fittings use the bunk, sanitary and desk colliders on their cell; wall wear is only a surface.");
            return visual;
        }

        // Frame, the fixed bars left of the door, the transom over it and the locking bar box along the top.
        private static void FixedFront(Transform r, IslandPrisonArt art)
        {
            Box("Head", r, new Vector3(0f, FrontHeight - .06f, 0f), new Vector3(2f, .12f, .1f), art.Bars, false);
            Box("Sill", r, new Vector3((FixedFrom + FixedTo) * .5f, .03f, 0f), new Vector3(FixedTo - FixedFrom, .06f, .08f), art.Bars, false);
            Box("Jamb_L", r, new Vector3(-.97f, FrontHeight * .5f, 0f), new Vector3(.06f, FrontHeight, .1f), art.Bars, false);
            Box("Jamb_R", r, new Vector3(.97f, FrontHeight * .5f, 0f), new Vector3(.06f, FrontHeight, .1f), art.Bars, false);
            Box("Mullion", r, new Vector3(FixedTo, FrontHeight * .5f, 0f), new Vector3(.07f, FrontHeight, .1f), art.Bars, false);
            for (float x = FixedFrom + .1f; x < FixedTo - .05f; x += BarStep)
                Box("Bar", r, new Vector3(x, FrontHeight * .5f, 0f), new Vector3(Bar, FrontHeight, Bar), art.Bars, false);
            foreach (float y in new[] { 1.05f, DoorHeight })
                Box("Flat", r, new Vector3((FixedFrom + FixedTo) * .5f, y, 0f), new Vector3(FixedTo - FixedFrom, .05f, .04f), art.Bars, false);
            // Transom bars above the door opening.
            Box("Transom", r, new Vector3((FixedTo + 1f) * .5f, DoorHeight + .03f, 0f), new Vector3(1f - FixedTo, .06f, .08f), art.Bars, false);
            for (float x = FixedTo + .1f; x < .95f; x += BarStep)
                Box("TBar", r, new Vector3(x, (DoorHeight + FrontHeight) * .5f, 0f), new Vector3(Bar, FrontHeight - DoorHeight, Bar), art.Bars, false);
            // The locking box all doors on a tier share runs along the top: a steel channel.
            Box("LockBox", r, new Vector3(0f, FrontHeight + .14f, .06f), new Vector3(2f, .2f, .16f), art.Steel, false);
            Box("Number", r, new Vector3(.55f, FrontHeight + .14f, .15f), new Vector3(.22f, .12f, .01f), art.Paper, false);
            IslandCellHardware.Front(r, art);
        }

        private static void DoorLeaf(Transform r, IslandPrisonArt art)
        {
            float w = DoorWidth, h = DoorHeight;
            Box("Top", r, new Vector3(w * .5f, h - .04f, 0f), new Vector3(w, .07f, .06f), art.Bars, false);
            Box("Bottom", r, new Vector3(w * .5f, .06f, 0f), new Vector3(w, .07f, .06f), art.Bars, false);
            Box("Stile_L", r, new Vector3(.03f, h * .5f, 0f), new Vector3(.06f, h, .06f), art.Bars, false);
            Box("Stile_R", r, new Vector3(w - .03f, h * .5f, 0f), new Vector3(.06f, h, .06f), art.Bars, false);
            Box("Cross", r, new Vector3(w * .5f, 1.05f, 0f), new Vector3(w, .05f, .05f), art.Bars, false);
            for (float x = .12f; x < w - .08f; x += BarStep)
                Box("Bar", r, new Vector3(x, h * .5f, 0f), new Vector3(Bar, h - .1f, Bar), art.Bars, false);
            Box("Lock", r, new Vector3(w - .1f, 1.05f, .05f), new Vector3(.12f, .18f, .06f), art.Steel, false);
            IslandCellHardware.Door(r, art, false);
        }

        // D-block: a solid steel door with a viewing slot and a food hatch, in the leaf's own space.
        private static void Solitary(Transform r, IslandPrisonArt art)
        {
            float w = DoorWidth, h = DoorHeight;
            Box("Leaf", r, new Vector3(w * .5f, h * .5f, 0f), new Vector3(w, h, .06f), art.Steel, false);
            Box("Slot", r, new Vector3(w * .5f, 1.6f, .035f), new Vector3(.3f, .08f, .02f), art.Bars, false);
            Box("Hatch", r, new Vector3(w * .5f, .9f, .035f), new Vector3(.36f, .16f, .02f), art.Bars, false);
            Box("Hinge_T", r, new Vector3(.02f, h - .3f, .04f), new Vector3(.05f, .14f, .05f), art.Rust, false);
            Box("Hinge_B", r, new Vector3(.02f, .3f, .04f), new Vector3(.05f, .14f, .05f), art.Rust, false);
            Box("Bolt", r, new Vector3(w - .08f, 1.15f, .05f), new Vector3(.1f, .22f, .06f), art.Rust, false);
            IslandCellHardware.Door(r, art, true);
        }

        // D-block's front: solid wall left of the door with a small barred window, steel frame round the door.
        private static void SolitaryWall(Transform r, IslandPrisonArt art)
        {
            Box("Wall", r, new Vector3((FixedFrom + FixedTo) * .5f, FrontHeight * .5f, 0f), new Vector3(FixedTo - FixedFrom, FrontHeight, .2f), art.Plaster, false);
            Box("Over", r, new Vector3((FixedTo + 1f) * .5f, (DoorHeight + FrontHeight) * .5f, 0f), new Vector3(1f - FixedTo, FrontHeight - DoorHeight, .2f), art.Plaster, false);
            Box("Frame_L", r, new Vector3(FixedTo + .02f, DoorHeight * .5f, .08f), new Vector3(.06f, DoorHeight, .08f), art.Steel, false);
            Box("Frame_R", r, new Vector3(.97f, DoorHeight * .5f, .08f), new Vector3(.06f, DoorHeight, .08f), art.Steel, false);
            Box("Frame_T", r, new Vector3((FixedTo + 1f) * .5f, DoorHeight + .03f, .08f), new Vector3(1f - FixedTo, .06f, .08f), art.Steel, false);
            Box("Dado", r, new Vector3((FixedFrom + FixedTo) * .5f, .6f, .11f), new Vector3(FixedTo - FixedFrom, 1.2f, .01f), art.Dado, false);
        }

        // Bunk, mattress, toilet, sink, shelf, fold-down table and stool; the variant decides what's left of them.
        private static void Furnish(Transform r, IslandPrisonArt art, int variant) => IslandCellFurnishings.Build(r, art, variant);

        /// <summary>Colliders worth having in a cell (in the cell's space): bunk, toilet and sink, table.</summary>
        public static void CellSolids(Transform cell)
        {
            IslandMeshKit.Solid(cell, new Vector3(-.58f, .26f, 1.15f), new Vector3(.72f, .5f, 1.95f));
            IslandMeshKit.Solid(cell, new Vector3(.4f, .45f, .3f), new Vector3(.9f, .9f, .55f));
            IslandMeshKit.Solid(cell, new Vector3(.74f, .4f, 1.55f), new Vector3(.5f, .8f, .7f));
        }

        // A 4 m gallery rail in its own space: z along the run (0 .. 4), x = 0 on the rail line, y up from the deck.
        private static void Rail(Transform r, IslandPrisonArt art)
        {
            Box("TopRail", r, new Vector3(0f, 1.05f, 2f), new Vector3(.06f, .06f, 4f), art.Railing, false);
            Box("MidRail", r, new Vector3(0f, .55f, 2f), new Vector3(.04f, .04f, 4f), art.Railing, false);
            Box("ToePlate", r, new Vector3(0f, .07f, 2f), new Vector3(.02f, .14f, 4f), art.Railing, false);
            for (float z = .05f; z <= 4f; z += 1f)
                Box("Post", r, new Vector3(0f, .52f, z), new Vector3(.05f, 1.05f, .05f), art.Railing, false);
            for (float z = .25f; z < 4f; z += .25f)
                Box("Baluster", r, new Vector3(0f, .55f, z), new Vector3(.018f, .95f, .018f), art.Railing, false);
        }

        // Steel knee-brackets under a 4 m gallery deck, from the cell face (x 0) out to the rail line (x = gallery width).
        private static void Brackets(Transform r, IslandPrisonArt art)
        {
            const float w = IslandPrisonLayout.GalleryWidth;
            Box("Stringer", r, new Vector3(w - .05f, -.28f, 2f), new Vector3(.08f, .28f, 4f), art.Steel, false);
            foreach (float z in new[] { .1f, 2f, 3.9f })
            {
                Box("Arm", r, new Vector3(w * .5f, -.22f, z), new Vector3(w, .12f, .08f), art.Steel, false);
                GameObject knee = Box("Knee", r, new Vector3(w * .3f, -.6f, z), new Vector3(.07f, .8f, .07f), art.Steel, false);
                knee.transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
            }
        }

        // Pipes along the chase between the cell rows: soil stack, water, conduit, one 4 m length.
        private static void ChasePipes(Transform r, IslandPrisonArt art)
        {
            foreach ((float x, float y, float d, Material m) in new[] { (-.35f, .4f, .16f, art.Rust), (.3f, .6f, .08f, art.Steel), (.3f, 2.8f, .05f, art.Steel), (-.3f, 3.1f, .06f, art.Rust) })
            {
                GameObject p = Primitive(PrimitiveType.Cylinder, "Pipe", r, new Vector3(x, y, 2f), new Vector3(d, 2f, d), m, false);
                p.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            for (float z = .5f; z < 4f; z += 1f)
            {
                GameObject riser = Primitive(PrimitiveType.Cylinder, "Riser", r, new Vector3(-.35f, 1.8f, z), new Vector3(.07f, 1.8f, .07f), art.Rust, false);
                riser.name = "Riser";
            }
        }
    }
}

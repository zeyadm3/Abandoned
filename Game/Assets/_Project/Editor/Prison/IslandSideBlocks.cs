using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The two blocks against the outer walls. D-block (west) is solitary: solid steel doors with a slot and a hatch,
    /// the safest hiding places on the island and the fewest. A-block (east) is the old block nobody finished
    /// refitting: barred cells on the ground floor, the tier above sealed behind its bars (no gallery, never reached).
    /// Both are two tiers tall; their aisles are the cellhouse's quiet flanks.
    /// </summary>
    public static class IslandSideBlocks
    {
        private const float Top = L.PlateauY + 8f;

        public static (int solitary, int open) Build(Transform root, IslandPrisonArt art, IslandCellKits kits, List<LootPointSpec> loot)
        {
            Transform g = Group("SideBlocks", root);
            int solitary = 0, open = 0;
            Block(Group("D_Block", g), true, art, kits, loot, ref solitary, ref open);
            Block(Group("A_Block", g), false, art, kits, loot, ref solitary, ref open);
            return (solitary, open);
        }

        private static void Block(Transform g, bool d, IslandPrisonArt art, IslandCellKits kits, List<LootPointSpec> loot, ref int solitary, ref int open)
        {
            float back = d ? L.Cellhouse.xMin + L.CellhouseWall : L.Cellhouse.xMax - L.CellhouseWall;
            float front = d ? L.DBlockFront : L.ABlockFront;
            float depth = Mathf.Abs(front - back);
            Vector3 outward = d ? Vector3.right : Vector3.left;
            float z0 = L.StackSouth, z1 = L.StackNorth;
            float xa = Mathf.Min(back, front), xb = Mathf.Max(back, front);
            // Slab between the tiers, the roof over tier 1, dividers and end walls.
            // The slab stands 3 cm proud of the cell fronts, so it never shares a plane with the dividers.
            K.Span("Slab_T1", g, new Vector3(d ? xa : xa - .03f, L.TierY(1) - .3f, z0 - .03f), new Vector3(d ? xb + .03f : xb, L.TierY(1), z1 + .03f), art.Floor);
            K.Span("Roof", g, new Vector3(xa, Top - .3f, z0 - .3f), new Vector3(xb + (d ? .3f : 0f) - (d ? 0f : .3f), Top, z1 + .3f), art.Ceiling);
            for (int k = 0; k <= L.SideBlockCells; k++)
            {
                float z = z0 + k * L.CellWidth;
                float t = k == 0 || k == L.SideBlockCells ? .3f : .14f;
                float za = k == 0 ? z - t : k == L.SideBlockCells ? z : z - t * .5f;
                K.Span($"Div_{k}", g, new Vector3(xa, L.PlateauY, za), new Vector3(xb, Top - .3f, za + t), art.Plaster);
            }
            MarkStatic(g);

            Transform cells = Group("Cells", g);
            for (int tier = 0; tier < 2; tier++)
            for (int k = 0; k < L.SideBlockCells; k++)
            {
                float zc = z0 + (k + .5f) * L.CellWidth;
                Transform cell = Group($"Cell_{tier}_{k:00}", cells);
                cell.SetPositionAndRotation(new Vector3(back, L.TierY(tier), zc), Quaternion.LookRotation(outward));
                float h = K.Hash(k + (d ? 0 : 40), tier * 13 + 5);
                // Solitary was bare: a concrete plinth and a hole in the floor. A-block kept its bunks.
                if (d) Plinth(cell, art, depth);
                else kits.PlaceFurnishing(cell, h);
                Vector3 frontAt = cell.TransformPoint(new Vector3(0f, 0f, depth));
                if (d) IslandMeshKit.Place(kits.SolitaryFront, "Front", cell, frontAt, cell.rotation);
                else IslandMeshKit.Place(kits.Fixed, "Front", cell, frontAt, cell.rotation);

                bool upper = tier == 1;
                bool hide = !upper && (d ? h < .5f : h < .12f) && !IslandHideSpots.Full;
                bool isOpen = !upper && !hide && h < (d ? .8f : .6f);
                Transform door;
                if (d)
                {
                    // The steel door swings out into the aisle (hinged on its left edge); open ones stand ajar.
                    door = Group("Door", cell);
                    door.localPosition = new Vector3(IslandCellKits.FixedTo, 0f, depth + .12f);
                    IslandMeshKit.Place(kits.SolitaryDoor, "Leaf", door, door.position, door.rotation);
                    if (isOpen)
                    {
                        door.localRotation = Quaternion.Euler(0f, -100f, 0f);
                        // A door standing open is still a steel slab in the aisle.
                        IslandMeshKit.Solid(door, new Vector3(IslandCellKits.DoorWidth * .5f, IslandCellKits.DoorHeight * .5f, 0f),
                            new Vector3(IslandCellKits.DoorWidth, IslandCellKits.DoorHeight, .08f));
                    }
                }
                else
                {
                    float doorX = isOpen ? IslandCellKits.FixedTo - .8f : IslandCellKits.FixedTo;
                    door = IslandMeshKit.Place(kits.Door, "Door", cell, cell.TransformPoint(new Vector3(doorX, 0f, depth + .06f)), cell.rotation);
                }
                if (hide)
                {
                    IslandHideSpots.Wire(cell, door, new Vector3(1.9f, IslandCellKits.FrontHeight, depth), new Vector3(0f, .02f, depth * .45f),
                        d ? HideSpotKind.Solitary : HideSpotKind.Cell);
                    if (d) solitary++;
                    continue;
                }
                if (!d) IslandCellKits.CellSolids(cell);
                if (!isOpen)
                {
                    IslandMeshKit.Solid(cell, new Vector3(0f, IslandCellKits.FrontHeight * .5f, depth), new Vector3(2f, IslandCellKits.FrontHeight, .2f));
                    continue;
                }
                open++;
                IslandMeshKit.Solid(cell, new Vector3(-.45f, IslandCellKits.FrontHeight * .5f, depth), new Vector3(1.1f, IslandCellKits.FrontHeight, .2f));
                IslandMeshKit.Solid(cell, new Vector3(.55f, (IslandCellKits.DoorHeight + IslandCellKits.FrontHeight) * .5f, depth),
                    new Vector3(.9f, IslandCellKits.FrontHeight - IslandCellKits.DoorHeight, .2f));
                if (h < .3f)
                    loot.Add(new LootPointSpec($"{(d ? "D" : "A")}Cell_{k}", IslandLootItems.Contraband, cell.TransformPoint(new Vector3(.6f, .12f, depth - .15f)),
                        CarryClass.Pocket, CarryClass.OneHand, false));
            }
        }

        private static void Plinth(Transform cell, IslandPrisonArt art, float depth)
        {
            Box("Plinth", cell, new Vector3(-.55f, .2f, depth * .5f), new Vector3(.8f, .4f, depth - .3f), art.FacadeDark, true);
            Box("Drain", cell, new Vector3(.55f, .005f, .4f), new Vector3(.25f, .01f, .25f), art.Rust, false);
            Box("Dado", cell, new Vector3(0f, .6f, .01f), new Vector3(1.86f, 1.2f, .01f), art.Dado, false);
            Box("Lamp", cell, new Vector3(0f, 3.3f, depth * .5f), new Vector3(.2f, .06f, .2f), art.LampOff, false);
            var details = IslandMeshKit.Get("Solitary_RoomDetails", r => IslandCellHardware.SolitaryRoom(r, art, depth));
            Transform visual = IslandMeshKit.Place(details, "FixedRoomFittings", cell, cell.position, cell.rotation);
            visual.gameObject.AddComponent<Abandoned.Core.WalkThroughObject>().EditorSetup("Thin wall-backed solitary fittings are supported by the cell's existing walls and plinth.");
        }
    }
}

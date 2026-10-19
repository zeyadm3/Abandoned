using System;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;
using MeshKit = System.Collections.Generic.List<(UnityEngine.Mesh mesh, UnityEngine.Material material)>;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Fixed institutional finishes and exposed services give the corridors a lived-in scale. Gallery fittings
    /// belong to each deck's Visual, so its seams, rivets and worn nosing leave with it when the deck collapses.
    /// </summary>
    public static class IslandCorridorDetails
    {
        public const string GroupName = "CorridorDetails";

        public static void Build(Transform root, IslandPrisonArt art)
        {
            Transform g = Group(GroupName, root);
            g.gameObject.AddComponent<WalkThroughObject>().EditorSetup(
                "Thin corridor finishes are backed by masonry; services are suspended overhead and have no gameplay collision.");
            Services(Group("ExposedServices", g), art);
            Entrance(Group("EntranceFinishes", g), art);
            NorthPassage(Group("DiningApproach", g), art);
            BlockEnds(Group("CellblockEndWalls", g), art);
            Galleries(root, art);
        }

        private static void Services(Transform parent, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Corridor_OverheadServices", r =>
            {
                // The open monitor stays open: narrow services hug Broadway's sides, above every gallery head.
                foreach (float x in new[] { 50.2f, 57.8f })
                {
                    Pipe(r, new Vector3(x, 23.45f, 53.4f), new Vector3(x, 23.45f, 79.4f), .11f, art.Steel);
                    Pipe(r, new Vector3(x + (x < 54f ? -.22f : .22f), 23.65f, 53.4f),
                        new Vector3(x + (x < 54f ? -.22f : .22f), 23.65f, 79.4f), .055f, art.Rust);
                    for (float z = 55f; z < 80f; z += 6f)
                    {
                        Pipe(r, new Vector3(x, 23.45f, z - .055f), new Vector3(x, 23.45f, z + .055f), .16f, art.Rust);
                        Box("PipeSaddle", r, new Vector3(x, 23.35f, z), new Vector3(.2f, .055f, .2f), art.Steel, false);
                        Box("RoofDropRod", r, new Vector3(x, 23.83f, z), new Vector3(.024f, .8f, .024f), art.Bars, false);
                    }
                }
                // Slotted trays expose bundles instead of reading as a solid second ceiling.
                foreach (float x in new[] { 35.6f, 72.4f })
                {
                    foreach (float side in new[] { -.19f, .19f })
                        Box("CableTrayEdge", r, new Vector3(x + side, 22.8f, 66f), new Vector3(.035f, .13f, 24f), art.Steel, false);
                    for (float z = 54.3f; z < 78f; z += .65f)
                        Box("TrayRung", r, new Vector3(x, 22.75f, z), new Vector3(.38f, .025f, .055f), art.Steel, false);
                    foreach (float side in new[] { -.1f, .04f, .13f })
                        Pipe(r, new Vector3(x + side, 22.81f, 54f), new Vector3(x + side, 22.81f, 78f), .035f, art.Bars);
                    for (float z = 55f; z < 80f; z += 6f)
                    {
                        Box("TrayHanger", r, new Vector3(x, 22.65f, z), new Vector3(.57f, .05f, .06f), art.Railing, false);
                        foreach (float side in new[] { -.25f, .25f })
                            Box("HangerRod", r, new Vector3(x + side, 23.47f, z), new Vector3(.02f, 1.6f, .02f), art.Railing, false);
                    }
                }
                // Only shallow branches cross the aisles, well above the upper-tier circulation.
                foreach (float z in new[] { 55f, 67f, 77f })
                {
                    Pipe(r, new Vector3(34.4f, 23.15f, z), new Vector3(38.5f, 23.15f, z), .07f, art.Rust);
                    Pipe(r, new Vector3(70.1f, 23.15f, z), new Vector3(73.7f, 23.15f, z), .07f, art.Rust);
                }
            });
            IslandMeshKit.Place(kit, "SuspendedPipeAndTrayRuns", parent, Vector3.zero, Quaternion.identity);
        }

        private static void Entrance(Transform parent, IslandPrisonArt art)
        {
            MeshKit bay = IslandMeshKit.Get("Corridor_EntranceWallBay", r =>
            {
                Box("Skirting", r, new Vector3(0f, .105f, .025f), new Vector3(2.16f, .21f, .05f), art.FacadeDark, false);
                Box("PaintedDado", r, new Vector3(0f, .73f, .023f), new Vector3(2.12f, 1.04f, .045f), art.Dado, false);
                Box("ChairRail", r, new Vector3(0f, 1.29f, .045f), new Vector3(2.16f, .07f, .07f), art.Plaster, false);
                foreach (float x in new[] { -.98f, .98f })
                    Box("ShallowWallRib", r, new Vector3(x, 2.2f, .026f), new Vector3(.09f, 1.68f, .05f), art.Plaster, false);
                Box("HighServiceTrunk", r, new Vector3(0f, 3.38f, .065f), new Vector3(2.15f, .1f, .1f), art.Steel, false);
                Box("TrunkCoverJoint", r, new Vector3(.58f, 3.38f, .12f), new Vector3(.055f, .13f, .026f), art.Rust, false);
                Vent(r, new Vector3(-.1f, 2.3f, .06f), new Vector2(.74f, .4f), art);
                Wear(r, new Vector3(.5f, .9f, .05f), 13, art);
            });
            foreach (float z in new[] { 46.3f, 49.2f })
            {
                IslandMeshKit.Place(bay, "AdminHallWallBay", parent, new Vector3(L.Admin.xMax + .12f, L.PlateauY, z), Quaternion.LookRotation(Vector3.right));
                IslandMeshKit.Place(bay, "LibraryHallWallBay", parent, new Vector3(L.Library.xMin - .12f, L.PlateauY, z), Quaternion.LookRotation(Vector3.left));
            }
            DoorTrim(parent, new Vector3(L.MainDoorX, L.PlateauY, L.Cellhouse.yMin + .53f),
                Vector3.forward, L.MainDoorWidth, L.MainDoorHeight, art);
        }

        private static void NorthPassage(Transform parent, IslandPrisonArt art)
        {
            MeshKit bay = IslandMeshKit.Get("Corridor_DiningApproachBay", r =>
            {
                Box("Skirting", r, new Vector3(0f, .09f, .025f), new Vector3(7.1f, .18f, .05f), art.FacadeDark, false);
                Box("Dado", r, new Vector3(0f, .7f, .025f), new Vector3(7.08f, 1.06f, .035f), art.Dado, false);
                Box("ChairRail", r, new Vector3(0f, 1.26f, .04f), new Vector3(7.12f, .065f, .065f), art.Plaster, false);
                foreach (float x in new[] { -3.35f, 3.35f })
                {
                    Box("PilasterFoot", r, new Vector3(x, .2f, .04f), new Vector3(.3f, .4f, .075f), art.FacadeDark, false);
                    Box("Pilaster", r, new Vector3(x, 2.02f, .03f), new Vector3(.21f, 3.22f, .055f), art.Plaster, false);
                    Box("Capital", r, new Vector3(x, 3.7f, .045f), new Vector3(.38f, .16f, .085f), art.FacadeDark, false);
                }
                Box("Trunking", r, new Vector3(0f, 3.57f, .07f), new Vector3(6.7f, .1f, .11f), art.Steel, false);
                Vent(r, new Vector3(-.35f, 2.25f, .06f), new Vector2(1.35f, .52f), art);
                Wear(r, new Vector3(-2.4f, .85f, .052f), 31, art);
                Wear(r, new Vector3(2.5f, .36f, .052f), 47, art);
            });
            foreach (float x in new[] { 43.5f, 64.5f })
                IslandMeshKit.Place(bay, "DiningWallFinish", parent, new Vector3(x, L.PlateauY, L.NorthBand.yMin - .19f), Quaternion.LookRotation(Vector3.back));
            DoorTrim(parent, new Vector3(36f, L.PlateauY, 79.81f), Vector3.back, 2.6f, 2.9f, art);
            DoorTrim(parent, new Vector3(54f, L.PlateauY, 79.81f), Vector3.back, 10.8f, 4f, art);
            DoorTrim(parent, new Vector3(72f, L.PlateauY, 79.81f), Vector3.back, 2.6f, 2.9f, art);
        }

        private static void BlockEnds(Transform parent, IslandPrisonArt art)
        {
            MeshKit kit = IslandMeshKit.Get("Corridor_CellblockEndFinish", r =>
            {
                // All faces begin above the chase doors. A return grille is fixed architecture, not a control.
                Vent(r, new Vector3(0f, 2.78f, .055f), new Vector2(1.18f, .45f), art);
                foreach (float x in new[] { -2.55f, 2.55f })
                {
                    Box("ConstructionJoint", r, new Vector3(x, 2.82f, .016f), new Vector3(.035f, 1.53f, .02f), art.FacadeDark, false);
                    Box("JointCap", r, new Vector3(x, 3.6f, .027f), new Vector3(.23f, .065f, .05f), art.Plaster, false);
                }
                Box("ServiceCrossbar", r, new Vector3(0f, 3.55f, .08f), new Vector3(5.7f, .07f, .08f), art.Railing, false);
                foreach (float x in new[] { -1.7f, 1.7f })
                {
                    Box("ConduitClip", r, new Vector3(x, 3.55f, .13f), new Vector3(.07f, .13f, .02f), art.Rust, false);
                    Wear(r, new Vector3(x, 2.8f, .021f), (int)(x * 100f), art);
                }
            });
            foreach (L.Stack stack in L.Stacks)
            for (int tier = 0; tier < L.Tiers.Length; tier++)
            {
                IslandMeshKit.Place(kit, stack.Name + "_SouthEnd", parent,
                    new Vector3(stack.Centre, L.TierY(tier), L.StackSouth - .315f), Quaternion.LookRotation(Vector3.back));
                IslandMeshKit.Place(kit, stack.Name + "_NorthEnd", parent,
                    new Vector3(stack.Centre, L.TierY(tier), L.StackNorth + .315f), Quaternion.LookRotation(Vector3.forward));
            }
        }

        private static void Galleries(Transform root, IslandPrisonArt art)
        {
            foreach (bool west in new[] { true, false })
            {
                float side = west ? -1f : 1f;
                MeshKit kit = IslandMeshKit.Get("Corridor_GalleryHardware_" + (west ? "W" : "E"), r =>
                {
                    // These are flush fasteners and construction seams, not new obstacles or extra rails.
                    foreach (float z in new[] { -1.75f, -.85f, .15f, 1.15f, 1.75f })
                    {
                        Box("DeckLapSeam", r, new Vector3(0f, .006f, z), new Vector3(1.35f, .009f, .018f), art.Steel, false);
                        foreach (float x in new[] { -.57f, .57f })
                        {
                            Box("RivetPad", r, new Vector3(x, .012f, z), new Vector3(.09f, .012f, .09f), art.Rust, false);
                            Box("RivetHead", r, new Vector3(x, .022f, z), new Vector3(.028f, .012f, .028f), art.Steel, false);
                        }
                    }
                    foreach (float z in new[] { -.95f, 1.05f })
                    {
                        Box("HandrailSplice", r, new Vector3(side * .75f, 1.05f, z), new Vector3(.09f, .095f, .21f), art.Steel, false);
                        foreach (float dz in new[] { -.065f, .065f })
                            Box("SpliceBolt", r, new Vector3(side * .805f, 1.05f, z + dz), new Vector3(.022f, .028f, .028f), art.Rust, false);
                    }
                    // Broken lengths expose the older plate beneath; a continuous bright stripe felt newly painted.
                    foreach (float z in new[] { -1.28f, .06f, 1.35f })
                        Box("WornNosingPaint", r, new Vector3(side * .655f, .012f, z), new Vector3(.065f, .008f, .64f), art.Warning, false);
                });
                foreach (L.Stack stack in L.Stacks)
                {
                    Transform run = root.Find("Levels/" + stack.Name + "_Galleries");
                    if (run == null) continue;
                    for (int tier = 1; tier < L.Tiers.Length; tier++)
                    for (int i = 0; i < L.GallerySections; i++)
                    {
                        Transform piece = run.Find($"Gallery_{(west ? "W" : "E")}{tier}_{i}");
                        Transform visual = piece != null ? piece.Find("Visual") : null;
                        if (visual == null) continue; // The intentionally fallen gallery never gets cosmetic decking.
                        IslandMeshKit.Place(kit, "AttachedDeckHardware", visual, piece.position, piece.rotation);
                    }
                }
            }
        }

        private static void DoorTrim(Transform parent, Vector3 at, Vector3 front, float width, float height, IslandPrisonArt art)
        {
            string key = FormattableString.Invariant($"Corridor_DoorEdge_{width:0.00}_{height:0.00}");
            MeshKit kit = IslandMeshKit.Get(key, r =>
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    Box("DoorEdge", r, new Vector3(side * (width * .5f + .055f), height * .5f, .028f),
                        new Vector3(.095f, height, .055f), art.Steel, false);
                    for (float y = .3f; y < height; y += .65f)
                        Box("JambFixing", r, new Vector3(side * (width * .5f + .055f), y, .063f),
                            new Vector3(.03f, .035f, .018f), art.Rust, false);
                }
                Box("HeadTrim", r, new Vector3(0f, height + .055f, .03f), new Vector3(width + .22f, .1f, .06f), art.Steel, false);
            });
            IslandMeshKit.Place(kit, "FixedDoorSurround", parent, at, Quaternion.LookRotation(front));
        }

        private static void Vent(Transform r, Vector3 centre, Vector2 size, IslandPrisonArt art)
        {
            Box("RecessShadow", r, centre, new Vector3(size.x, size.y, .025f), art.Bars, false);
            foreach (float side in new[] { -1f, 1f })
                Box("GrilleUpright", r, centre + new Vector3(side * size.x * .5f, 0f, .023f), new Vector3(.035f, size.y + .04f, .04f), art.Steel, false);
            for (float y = -size.y * .5f; y <= size.y * .5f; y += .06f)
                K.RotBox("ReturnLouvre", r, centre + new Vector3(0f, y, .03f), new Vector3(size.x, .025f, .045f), Quaternion.Euler(-18f, 0f, 0f), art.Steel, false);
            foreach (float side in new[] { -1f, 1f })
                Box("GrilleFlange", r, centre + new Vector3(0f, side * (size.y * .5f + .025f), .018f), new Vector3(size.x + .08f, .04f, .045f), art.Railing, false);
        }

        private static void Wear(Transform r, Vector3 centre, int seed, IslandPrisonArt art)
        {
            for (int i = 0; i < 5; i++)
            {
                float h = K.Hash(seed, i + 53);
                Box("PaintChip", r, centre + new Vector3((h - .5f) * .46f, (K.Hash(i, seed) - .5f) * .32f, i * .0007f),
                    new Vector3(.035f + h * .1f, .018f + K.Hash(seed + 5, i) * .06f, .008f), art.FacadeDark, false);
            }
        }

        private static void Pipe(Transform parent, Vector3 a, Vector3 b, float diameter, Material material)
        {
            Vector3 delta = b - a;
            GameObject pipe = Primitive(PrimitiveType.Cylinder, "ServicePipe", parent, (a + b) * .5f,
                new Vector3(diameter, delta.magnitude * .5f, diameter), material, false);
            pipe.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }
    }
}

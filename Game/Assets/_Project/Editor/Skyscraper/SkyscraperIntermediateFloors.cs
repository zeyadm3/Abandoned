using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds intermediate stops and the rooftop crown for Cloudline Tower:
    /// 1. Flooded Floor (Story 3, y = 12m): burst pipes, standing water, and waterlogged office salvage.
    /// 2. Looted Office (Story 8, y = 32m): ransacked corporate cubicles and howling broken windows.
    /// 3. Roof Terrace & Window Gantry (y = 50m): helipad deck, window-cleaning crane rig, and telescope.
    /// </summary>
    public static class SkyscraperIntermediateFloors
    {
        public static List<Transform> Build(Transform root, SkyscraperArt art,
            out List<Transform> collapsibleRoofTiles,
            out List<Transform> soundPoints)
        {
            Transform floorsGroup = Group("IntermediateFloors", root);
            collapsibleRoofTiles = new List<Transform>();
            soundPoints = new List<Transform>();
            var allFloorSlabs = new List<Transform>();

            // ======================================================================================
            // 1. Flooded Floor (Story 3, y = 12.0m)
            // ======================================================================================
            Transform floodedGroup = Group("FloodedFloor_12m", floorsGroup);
            float yFlood = 12.0f;

            // Structural concrete slab (x: -8.8 to 2.0, z: -4.0 to 10.0)
            Transform floodSlab = Group("FloodedFloor_Slab", floodedGroup);
            floodSlab.position = new Vector3(-3.4f, yFlood, 3.0f);
            BoxCollider colFlood = floodSlab.gameObject.AddComponent<BoxCollider>();
            colFlood.center = new Vector3(0f, -0.15f, 0f);
            colFlood.size = new Vector3(10.8f, 0.3f, 14.0f);
            Box("Visual", floodSlab, colFlood.center, colFlood.size, art.Concrete, false);
            floodSlab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            allFloorSlabs.Add(floodSlab);

            // Ceiling overhead at y = 15.5m
            Box("FloodedCeiling", floodedGroup, new Vector3(-3.4f, yFlood + 3.5f, 3.0f),
                new Vector3(10.8f, 0.3f, 14.0f), art.CeilingTile);

            // Standing water sheet / puddle over the slab
            GameObject puddle = Box("StandingWaterSheet", floodedGroup, new Vector3(-3.4f, yFlood + 0.02f, 3.0f),
                new Vector3(10.4f, 0.02f, 13.6f), art.Glass, false);
            puddle.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);

            // Burst overhead fire-sprinkler pipes with drip emitter point
            Transform burstPipe = Group("BurstSprinklerPipe", floodedGroup);
            burstPipe.position = new Vector3(-4.0f, yFlood + 3.2f, 4.0f);
            Box("Pipe", burstPipe, Vector3.zero, new Vector3(0.12f, 0.12f, 8.0f), art.Steel, false);
            Transform dripNode = Group("DripSoundPoint", burstPipe);
            dripNode.position = new Vector3(-4.0f, yFlood + 1.5f, 4.0f);
            soundPoints.Add(dripNode);

            // Waterlogged office desks & overturned partitions
            Transform wetDesk = Group("WetDesk", floodedGroup);
            wetDesk.position = new Vector3(-4.0f, yFlood, 5.0f);
            Box("DeskTop", wetDesk, new Vector3(0f, 0.75f, 0f), new Vector3(1.8f, 0.08f, 0.9f), art.Steel);
            Box("DeskLegs", wetDesk, new Vector3(0f, 0.35f, 0f), new Vector3(1.7f, 0.7f, 0.8f), art.Steel, false);

            Transform cabinet = Group("WetFileCabinet", floodedGroup);
            cabinet.position = new Vector3(-6.0f, yFlood, 2.0f);
            Box("CabinetBody", cabinet, new Vector3(0f, 0.6f, 0f), new Vector3(0.9f, 1.2f, 0.6f), art.Steel);

            // ======================================================================================
            // 2. Looted Corporate Office Floor (Story 8, y = 32.0m)
            // ======================================================================================
            Transform officeGroup = Group("LootedOffice_32m", floorsGroup);
            float yOffice = 32.0f;

            // Structural concrete slab
            Transform officeSlab = Group("Office_Slab", officeGroup);
            officeSlab.position = new Vector3(-3.4f, yOffice, 3.0f);
            BoxCollider colOffice = officeSlab.gameObject.AddComponent<BoxCollider>();
            colOffice.center = new Vector3(0f, -0.15f, 0f);
            colOffice.size = new Vector3(10.8f, 0.3f, 14.0f);
            Box("Visual", officeSlab, colOffice.center, colOffice.size, art.Parquet, false);
            officeSlab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            allFloorSlabs.Add(officeSlab);

            // Ceiling overhead at y = 35.5m
            Box("OfficeCeiling", officeGroup, new Vector3(-3.4f, yOffice + 3.5f, 3.0f),
                new Vector3(10.8f, 0.3f, 14.0f), art.CeilingTile);

            // Ransacked cubicles and executive desks
            Transform desk1 = Group("ExecDesk", officeGroup);
            desk1.position = new Vector3(-6.0f, yOffice, 4.0f);
            Box("Top", desk1, new Vector3(0f, 0.75f, 0f), new Vector3(2.2f, 0.08f, 1.1f), art.Mahogany);
            Box("Drawers", desk1, new Vector3(-0.8f, 0.36f, 0f), new Vector3(0.5f, 0.7f, 0.9f), art.Mahogany);

            Transform serverRack = Group("ServerRack", officeGroup);
            serverRack.position = new Vector3(-3.0f, yOffice, 6.0f);
            Box("RackFrame", serverRack, new Vector3(0f, 1.0f, 0f), new Vector3(0.9f, 2.0f, 0.8f), art.Steel);
            Box("Blades", serverRack, new Vector3(0f, 1.0f, 0.05f), new Vector3(0.8f, 1.8f, 0.6f), art.Steel, false);

            // Broken exterior window letting wind howl into the office
            Transform brokenWindow = Group("BrokenWindowNode", officeGroup);
            brokenWindow.position = new Vector3(-3.4f, yOffice + 1.8f, 10.0f);
            soundPoints.Add(brokenWindow);

            // ======================================================================================
            // 3. Roof Terrace, Helipad & Window Gantry (Summit, y = 50.0m)
            // ======================================================================================
            Transform roofGroup = Group("RoofTerrace_50m", floorsGroup);
            float yRoof = L.RoofY;

            // Stair access from Restaurant Terrace (44m) up to Roof Terrace (50m)
            Transform roofStair = Group("RoofAccessStairs", roofGroup);
            roofStair.position = new Vector3(-16.0f, yRoof - 3.0f, 6.0f);
            Box("RampSlab", roofStair, Vector3.zero, new Vector3(1.6f, 0.25f, 7.5f), art.Steel)
                .transform.localRotation = Quaternion.Euler(-50f, 0f, 0f);
            Box("Handrail", roofStair, new Vector3(0.85f, 0.9f, 0f), new Vector3(0.06f, 0.06f, 7.5f), art.Steel, false)
                .transform.localRotation = Quaternion.Euler(-50f, 0f, 0f);

            // Exterior Helipad marking on the main roof slab
            Transform helipad = Group("HelipadMarking", roofGroup);
            helipad.position = new Vector3(0f, yRoof + 0.26f, 6.0f);
            Box("CircleRim", helipad, Vector3.zero, new Vector3(14.0f, 0.02f, 14.0f), art.Concrete, false);
            Box("H_Left", helipad, new Vector3(-2.2f, 0.04f, 0f), new Vector3(0.6f, 0.02f, 6.0f), art.LampEmissive, false);
            Box("H_Right", helipad, new Vector3( 2.2f, 0.04f, 0f), new Vector3(0.6f, 0.02f, 6.0f), art.LampEmissive, false);
            Box("H_Cross", helipad, new Vector3( 0.0f, 0.04f, 0f), new Vector3(3.8f, 0.02f, 0.6f), art.LampEmissive, false);

            // Window-Cleaning Gantry Crane Rig along the north parapet (z = 20m)
            Transform gantry = Group("WindowCleaningGantry", roofGroup);
            gantry.position = new Vector3(0f, yRoof + 0.5f, 19.0f);

            // Steel crane rails
            Box("RailTrack", gantry, Vector3.zero, new Vector3(20.0f, 0.15f, 0.4f), art.Steel, false);
            Box("CraneCart", gantry, new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 1.0f, 1.6f), art.Steel);
            Box("BoomArm", gantry, new Vector3(0f, 1.8f, 1.2f), new Vector3(0.4f, 0.4f, 3.2f), art.Steel, false);

            // Suspended Window Washer Cradle over the edge
            Transform cradle = Group("SuspendedCradle", gantry);
            cradle.position = new Vector3(0f, yRoof - 2.5f, 22.8f);
            Box("CradleFloor", cradle, Vector3.zero, new Vector3(3.6f, 0.12f, 1.2f), art.DiamondPlate);
            Box("CradleRailN", cradle, new Vector3(0f, 0.5f,  0.55f), new Vector3(3.6f, 1.0f, 0.05f), art.Steel, false);
            Box("CradleRailS", cradle, new Vector3(0f, 0.5f, -0.55f), new Vector3(3.6f, 1.0f, 0.05f), art.Steel, false);
            Box("CradleWireL", cradle, new Vector3(-1.6f, 2.5f, 0f), new Vector3(0.04f, 5.0f, 0.04f), art.Steel, false);
            Box("CradleWireR", cradle, new Vector3( 1.6f, 2.5f, 0f), new Vector3(0.04f, 5.0f, 0.04f), art.Steel, false);
            allFloorSlabs.Add(cradle);
            collapsibleRoofTiles.Add(cradle); // Overloaded cradle can snap and plummet!

            // Cantilevered Observation Viewing Balcony on Roof (collapsible edge section)
            Transform viewDeck = Group("ObservationOverlook_West", roofGroup);
            viewDeck.position = new Vector3(-8.0f, yRoof, 16.0f);
            BoxCollider colView = viewDeck.gameObject.AddComponent<BoxCollider>();
            colView.center = new Vector3(0f, -0.1f, 0f);
            colView.size = new Vector3(4.0f, 0.25f, 4.0f);
            Box("Visual", viewDeck, colView.center, colView.size, art.DiamondPlate, false);
            viewDeck.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            allFloorSlabs.Add(viewDeck);
            collapsibleRoofTiles.Add(viewDeck);

            // Roof Parapet Telescope
            Transform telescope = Group("RoofTelescope", roofGroup);
            telescope.position = new Vector3(-8.0f, yRoof + 0.15f, 16.0f);
            Primitive(PrimitiveType.Cylinder, "Mount", telescope, new Vector3(0f, 0.6f, 0f), new Vector3(0.25f, 1.2f, 0.25f), art.Steel);
            Primitive(PrimitiveType.Cylinder, "Tube", telescope, new Vector3(0f, 1.25f, 0f), new Vector3(0.3f, 1.1f, 0.3f), art.Brass)
                .transform.localRotation = Quaternion.Euler(30f, 0f, 0f);

            return allFloorSlabs;
        }
    }
}

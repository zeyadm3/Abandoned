using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Floor 1 (y = 20m): intermediate technical deck housing the elevator hoist machinery,
    /// high-voltage transformer cages, HVAC chillers, Sector 1 breaker bank, and catwalks over the shaft.
    /// </summary>
    public static class SkyscraperMechanicalFloor
    {
        public static List<Transform> Build(Transform root, SkyscraperArt art, out List<Transform> collapsibleCatwalks)
        {
            Transform mech = Group("MechanicalFloor_20m", root);
            collapsibleCatwalks = new List<Transform>();
            var allFloorPieces = new List<Transform>();

            float y = L.MechanicalY;
            float midZ = (L.TowerBackZ + L.TowerFrontZ) * 0.5f;

            // 1. Concrete Structural Slab (excluding the open elevator shaft at center)
            // West Wing (x: -18 to -3)
            Transform westSlab = Group("MechSlab_West", mech);
            westSlab.position = new Vector3(-10.5f, y, midZ);
            BoxCollider colW = westSlab.gameObject.AddComponent<BoxCollider>();
            colW.center = new Vector3(0f, -0.2f, 0f);
            colW.size = new Vector3(15f, 0.4f, 32f);
            Box("Visual", westSlab, colW.center, colW.size, art.Concrete, false);
            westSlab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            allFloorPieces.Add(westSlab);

            // East Wing (x: +3 to +18)
            Transform eastSlab = Group("MechSlab_East", mech);
            eastSlab.position = new Vector3(10.5f, y, midZ);
            BoxCollider colE = eastSlab.gameObject.AddComponent<BoxCollider>();
            colE.center = new Vector3(0f, -0.2f, 0f);
            colE.size = new Vector3(15f, 0.4f, 32f);
            Box("Visual", eastSlab, colE.center, colE.size, art.Concrete, false);
            eastSlab.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            allFloorPieces.Add(eastSlab);

            // Ceiling Slab overhead at y = 25m
            Box("MechCeiling", mech, new Vector3(0f, y + L.MechanicalHeight, midZ), new Vector3(36f, 0.4f, 32f), art.Concrete);

            // 2. Steel Catwalks Spanning the Elevator Void (Collapsible sections)
            for (int i = 0; i < 3; i++)
            {
                float zOffset = -2.0f + i * 2.0f;
                Transform bridge = Group($"Catwalk_ShaftBridge_{i}", mech);
                bridge.position = new Vector3(0f, y, zOffset);
                BoxCollider colBridge = bridge.gameObject.AddComponent<BoxCollider>();
                colBridge.center = new Vector3(0f, -0.08f, 0f);
                colBridge.size = new Vector3(6.0f, 0.16f, 1.4f);
                Box("Visual", bridge, colBridge.center, colBridge.size, art.DiamondPlate, false);
                bridge.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

                // Railings
                Box("RailL", bridge, new Vector3(0f, 0.5f, -0.65f), new Vector3(6.0f, 1.0f, 0.06f), art.Steel, false);
                Box("RailR", bridge, new Vector3(0f, 0.5f,  0.65f), new Vector3(6.0f, 1.0f, 0.06f), art.Steel, false);

                allFloorPieces.Add(bridge);
                if (i == 1) collapsibleCatwalks.Add(bridge); // Center bridge over the void can buckle under weight!
            }

            // 3. Sector 1 Breaker Panel (Elevator Machinery & Shaft Power)
            Transform breakerGroup = Group("Sector1_ElevatorBreaker", mech);
            breakerGroup.position = new Vector3(3.2f, y, -2.0f);
            breakerGroup.rotation = Quaternion.LookRotation(Vector3.left);
            Box("Cabinet", breakerGroup, new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 1.2f, 0.35f), art.Steel);
            Transform handle = Group("LeverHandle", breakerGroup);
            handle.localPosition = new Vector3(0.45f, 1.2f, 0f);
            Box("LeverArm", handle, new Vector3(0f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), art.Brass, false);
            handle.localRotation = Quaternion.Euler(35f, 0f, 0f);
            GameObject lamp = Box("BreakerLamp", breakerGroup, new Vector3(-0.25f, 1.65f, 0.18f),
                new Vector3(0.1f, 0.1f, 0.08f), art.LampEmissive, false);
            var breaker = breakerGroup.gameObject.AddComponent<BreakerPanel>();
            breaker.EditorSetup(1, handle, lamp.GetComponent<Renderer>(), true);

            // 4. Heavy Plant Equipment & Transformers
            // Chiller Units
            for (float x = 6f; x <= 14f; x += 4f)
            {
                Transform ahu = Group($"ChillerUnit_{x}", mech);
                ahu.position = new Vector3(x, y, 10f);
                Box("Body", ahu, new Vector3(0f, 1.4f, 0f), new Vector3(3.2f, 2.8f, 2.4f), art.Steel);
                Primitive(PrimitiveType.Cylinder, "FanShroud", ahu, new Vector3(0f, 2.85f, 0f), new Vector3(2.0f, 0.2f, 2.0f), art.Steel);
            }

            // High Voltage Electrical Transformer Cages
            Transform transformer = Group("TransformerCage", mech);
            transformer.position = new Vector3(-10f, y, 10f);
            Box("EnclosureMesh", transformer, new Vector3(0f, 1.6f, 0f), new Vector3(4.5f, 3.2f, 4.0f), art.Steel, false);
            Box("TransformerCore1", transformer, new Vector3(-1.2f, 1.2f, 0f), new Vector3(1.4f, 2.4f, 1.6f), art.Steel);
            Box("TransformerCore2", transformer, new Vector3( 1.2f, 1.2f, 0f), new Vector3(1.4f, 2.4f, 1.6f), art.Steel);

            // Overhead Cable Trays & Ventilation Ducting
            Box("DuctMain", mech, new Vector3(0f, y + 4.2f, 4f), new Vector3(34f, 0.8f, 1.2f), art.Steel, false);
            Box("CableTrayN", mech, new Vector3(0f, y + 4.6f, 12f), new Vector3(32f, 0.2f, 0.6f), art.Steel, false);

            return allFloorPieces;
        }
    }
}

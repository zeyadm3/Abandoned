using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the commercial kitchen, stainless steel cooking line, ventilation hoods, dishwashing station,
    /// staff locker bay, and the insulated walk-in cold store / freezer on Floor 2 (y = 44m).
    /// </summary>
    public static class SkyscraperKitchen
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform kitchenGroup = Group("CommercialKitchen", root);
            float y = L.RestaurantY;

            // Kitchen Enclosure Walls (x: -16 to -2, z: -8 to 2)
            Box("KitchenWall_North", kitchenGroup, new Vector3(-9f, y + 2.5f, 2f), new Vector3(14f, 5f, 0.25f), art.KitchenTile);
            Box("KitchenWall_East", kitchenGroup, new Vector3(-2f, y + 2.5f, -3f), new Vector3(0.25f, 5f, 10f), art.KitchenTile);

            // Double Swing Doors into Grand Dining Room (with circular vision panels)
            Transform doors = Group("KitchenSwingDoors", kitchenGroup);
            doors.position = new Vector3(-8f, y, 2f);
            Box("DoorL", doors, new Vector3(-0.65f, 1.25f, 0f), new Vector3(1.2f, 2.5f, 0.08f), art.Steel);
            Box("DoorR", doors, new Vector3( 0.65f, 1.25f, 0f), new Vector3(1.2f, 2.5f, 0.08f), art.Steel);
            Primitive(PrimitiveType.Cylinder, "PortholeL", doors, new Vector3(-0.65f, 1.6f, 0f),
                new Vector3(0.35f, 0.09f, 0.35f), art.Glass, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Primitive(PrimitiveType.Cylinder, "PortholeR", doors, new Vector3( 0.65f, 1.6f, 0f),
                new Vector3(0.35f, 0.09f, 0.35f), art.Glass, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // 1. Central Cooking Island (Heavy ranges, convection ovens, fryers)
            Transform island = Group("CookingIsland", kitchenGroup);
            island.position = new Vector3(-9f, y, -2f);
            Box("RangeBlock", island, new Vector3(0f, 0.5f, 0f), new Vector3(6.5f, 1.0f, 1.8f), art.Steel);
            for (float rx = -2.4f; rx <= 2.4f; rx += 1.2f)
            {
                // Cast iron burners
                Primitive(PrimitiveType.Cylinder, "BurnerF", island, new Vector3(rx, 1.02f, -0.45f), new Vector3(0.35f, 0.04f, 0.35f), art.Steel, false);
                Primitive(PrimitiveType.Cylinder, "BurnerB", island, new Vector3(rx, 1.02f,  0.45f), new Vector3(0.35f, 0.04f, 0.35f), art.Steel, false);
            }

            // Overhead Stainless Steel Extraction Hood
            Transform hood = Group("ExhaustHood", kitchenGroup);
            hood.position = new Vector3(-9f, y + 3.2f, -2f);
            Box("HoodCanopy", hood, Vector3.zero, new Vector3(7.2f, 0.8f, 2.4f), art.Steel);
            Box("Flue1", hood, new Vector3(-1.8f, 1.0f, 0f), new Vector3(0.8f, 1.4f, 0.8f), art.Steel, false);
            Box("Flue2", hood, new Vector3( 1.8f, 1.0f, 0f), new Vector3(0.8f, 1.4f, 0.8f), art.Steel, false);

            // 2. Prep Stations (Stainless steel counters & cutting boards)
            Transform prep = Group("PrepCounters", kitchenGroup);
            prep.position = new Vector3(-14.5f, y, -2f);
            Box("PrepTable", prep, new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 5.5f), art.Steel);
            Box("CuttingBoard1", prep, new Vector3(0f, 0.92f, -1.2f), new Vector3(0.7f, 0.04f, 1.0f), art.Parquet, false);
            Box("CuttingBoard2", prep, new Vector3(0f, 0.92f,  1.2f), new Vector3(0.7f, 0.04f, 1.0f), art.Parquet, false);

            // 3. Dishwashing Line & Scrub Sinks
            Transform dishwashing = Group("DishwashingStation", kitchenGroup);
            dishwashing.position = new Vector3(-9f, y, 0.8f);
            Box("SinkUnit", dishwashing, new Vector3(0f, 0.45f, 0f), new Vector3(4.8f, 0.9f, 1.0f), art.Steel);
            Box("SprayFaucet", dishwashing, new Vector3(-1.2f, 1.4f, 0f), new Vector3(0.08f, 0.9f, 0.3f), art.Brass, false);
            Box("RackDishwasher", dishwashing, new Vector3(1.6f, 0.9f, 0f), new Vector3(1.4f, 1.8f, 1.1f), art.Steel);

            // 4. Walk-in Cold Store / Freezer (Southwest corner, x: -16 to -10, z: -8 to -4)
            Transform coldStore = Group("WalkInColdStore", kitchenGroup);
            coldStore.position = new Vector3(-12.5f, y, -6f);

            // Insulated panel walls
            Box("FreezerWallN", coldStore, new Vector3(0f, 1.75f, 2.0f), new Vector3(5.5f, 3.5f, 0.2f), art.ColdStoreWall);
            Box("FreezerWallE", coldStore, new Vector3(2.75f, 1.75f, 0f), new Vector3(0.2f, 3.5f, 4.2f), art.ColdStoreWall);
            Box("FreezerCeiling", coldStore, new Vector3(0f, 3.5f, 0f), new Vector3(5.5f, 0.2f, 4.2f), art.ColdStoreWall);

            // Heavy Cold Store Latch Door
            Transform freezerDoor = Group("FreezerDoor", coldStore);
            freezerDoor.position = new Vector3(1.2f, 0f, 2.0f);
            Box("DoorJamb", freezerDoor, new Vector3(0f, 1.25f, 0f), new Vector3(1.4f, 2.5f, 0.25f), art.Steel);
            Box("DoorSlab", freezerDoor, new Vector3(0f, 1.25f, 0.05f), new Vector3(1.2f, 2.4f, 0.15f), art.ColdStoreWall);
            Box("LatchHandle", freezerDoor, new Vector3(0.45f, 1.1f, 0.18f), new Vector3(0.12f, 0.35f, 0.14f), art.Brass, false);

            // Cold Store Interior Shelves & Overhead Meat Rails
            Transform interior = Group("ColdStoreInterior", coldStore);
            Box("WireShelfL", interior, new Vector3(-2.2f, 1.2f, 0f), new Vector3(0.6f, 2.4f, 3.5f), art.Steel);
            Box("MeatRailOverhead", interior, new Vector3(0f, 3.0f, 0f), new Vector3(0.1f, 0.1f, 3.5f), art.Steel, false);

            // 5. Kitchen Sector 3 Breaker Panel
            Transform breakerGroup = Group("Sector3_KitchenBreaker", kitchenGroup);
            breakerGroup.position = new Vector3(-2.1f, y, -6.5f);
            breakerGroup.rotation = Quaternion.LookRotation(Vector3.left);
            Box("Cabinet", breakerGroup, new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 1.2f, 0.35f), art.Steel);
            Transform handle = Group("LeverHandle", breakerGroup);
            handle.localPosition = new Vector3(0.45f, 1.2f, 0f);
            Box("LeverArm", handle, new Vector3(0f, 0.15f, 0f), new Vector3(0.06f, 0.3f, 0.06f), art.Brass, false);
            handle.localRotation = Quaternion.Euler(35f, 0f, 0f);
            GameObject lamp = Box("BreakerLamp", breakerGroup, new Vector3(-0.25f, 1.65f, 0.18f),
                new Vector3(0.1f, 0.1f, 0.08f), art.LampEmissive, false);
            var breaker = breakerGroup.gameObject.AddComponent<BreakerPanel>();
            breaker.EditorSetup(3, handle, lamp.GetComponent<Renderer>(), true);
        }
    }
}

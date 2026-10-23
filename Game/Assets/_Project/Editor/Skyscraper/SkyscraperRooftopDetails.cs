using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds high-altitude industrial and luxury rooftop infrastructure for Cloudline Tower:
    /// - Massive rooftop HVAC chillers with top cooling fan cowlings, spring mounts, and ductwork trunks.
    /// - 10-metre communications tower mast with microwave dish, transceiver panels, and red hazard beacon.
    /// - Window-cleaning maintenance gear shack with cable winches, rigging levers, and grease drums.
    /// - Observation terrace mushroom radiant patio heaters and teak skyline cocktail tables.
    /// </summary>
    public static class SkyscraperRooftopDetails
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform roofGroup = Group("RooftopInfrastructure", root);
            float yRoof = L.RoofY;

            // 1. Massive Industrial Rooftop HVAC Chillers (South Roof Deck: y = 50m, z = -4m to -8m)
            Transform chillers = Group("HvacChillerCompound", roofGroup);
            chillers.position = new Vector3(0f, yRoof, -6f);

            foreach (float cx in new[] { -6f, 6f })
            {
                Transform unit = Group($"ChillerUnit_{cx:0}", chillers);
                unit.localPosition = new Vector3(cx, 0f, 0f);

                // Vibration-isolation spring footing pads
                Box("SpringPad1", unit, new Vector3(-1.8f, 0.15f, -1.2f), new Vector3(0.5f, 0.3f, 0.5f), art.Steel, false);
                Box("SpringPad2", unit, new Vector3( 1.8f, 0.15f, -1.2f), new Vector3(0.5f, 0.3f, 0.5f), art.Steel, false);
                Box("SpringPad3", unit, new Vector3(-1.8f, 0.15f,  1.2f), new Vector3(0.5f, 0.3f, 0.5f), art.Steel, false);
                Box("SpringPad4", unit, new Vector3( 1.8f, 0.15f,  1.2f), new Vector3(0.5f, 0.3f, 0.5f), art.Steel, false);

                // Heavy Compressor & Condenser Housing
                Box("ChillerChassis", unit, new Vector3(0f, 1.4f, 0f), new Vector3(4.2f, 2.2f, 2.8f), art.Steel);
                Box("CondenserCoils", unit, new Vector3(0f, 1.4f, 1.42f), new Vector3(3.8f, 1.8f, 0.05f), art.DiamondPlate, false);

                // Top Cylindrical Cooling Fan Cowlings & Impellers
                for (float fx = -1.2f; fx <= 1.2f; fx += 2.4f)
                {
                    Transform fan = Group($"Fan_{fx:0.0}", unit);
                    fan.localPosition = new Vector3(fx, 2.6f, 0f);
                    Primitive(PrimitiveType.Cylinder, "Cowl", fan, Vector3.zero, new Vector3(1.6f, 0.35f, 1.6f), art.Steel);
                    Box("Blade1", fan, new Vector3(0f, 0.15f, 0f), new Vector3(1.4f, 0.02f, 0.2f), art.Rust, false);
                    Box("Blade2", fan, new Vector3(0f, 0.15f, 0f), new Vector3(0.2f, 0.02f, 1.4f), art.Rust, false);
                }

                // Insulated Ductwork Trunk leading into roof penetration curb
                Box("DuctTrunk", unit, new Vector3(0f, 1.2f, -1.8f), new Vector3(1.8f, 1.4f, 1.2f), art.Steel);
                Box("RoofCurb", unit, new Vector3(0f, 0.25f, -2.4f), new Vector3(2.2f, 0.5f, 0.8f), art.Concrete);
            }

            // 2. Telecommunications Antenna Mast (North-East Roof: x = 12m, z = 18m, y = 50m)
            Transform antenna = Group("TelecommunicationsMast", roofGroup);
            antenna.position = new Vector3(12f, yRoof, 18f);

            // Concrete Mounting Plinth
            Box("MountingBase", antenna, new Vector3(0f, 0.35f, 0f), new Vector3(2.4f, 0.7f, 2.4f), art.Concrete);

            // 10-Metre Steel Lattice Tower Structure
            Primitive(PrimitiveType.Cylinder, "TowerLeg1", antenna, new Vector3(-0.6f, 5.0f, -0.6f), new Vector3(0.12f, 5.0f, 0.12f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "TowerLeg2", antenna, new Vector3( 0.6f, 5.0f, -0.6f), new Vector3(0.12f, 5.0f, 0.12f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "TowerLeg3", antenna, new Vector3(-0.6f, 5.0f,  0.6f), new Vector3(0.12f, 5.0f, 0.12f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "TowerLeg4", antenna, new Vector3( 0.6f, 5.0f,  0.6f), new Vector3(0.12f, 5.0f, 0.12f), art.Steel, false);
            for (float ly = 2f; ly <= 8f; ly += 2f)
                Box($"CrossBrace_{ly:0}", antenna, new Vector3(0f, ly, 0f), new Vector3(1.4f, 0.08f, 1.4f), art.Steel, false);

            // Parabolic Microwave Dish Antenna
            Transform dish = Group("MicrowaveDish", antenna);
            dish.localPosition = new Vector3(0f, 6.5f, -0.7f);
            Primitive(PrimitiveType.Sphere, "DishBowl", dish, Vector3.zero, new Vector3(1.6f, 1.6f, 0.4f), art.WallPlaster, false);
            Primitive(PrimitiveType.Cylinder, "DishFeed", dish, new Vector3(0f, 0f, -0.4f), new Vector3(0.06f, 0.4f, 0.06f), art.Steel, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Summit Aviation Warning Hazard Beacon
            Primitive(PrimitiveType.Sphere, "AviationBeacon", antenna, new Vector3(0f, 10.3f, 0f), Vector3.one * 0.45f, art.RedEmergencyEmissive, false);

            // 3. Window-Cleaning Rigging & Gear Shack (North-West Roof: x = -12m, z = 18m, y = 50m)
            Transform shack = Group("GantryMaintenanceShack", roofGroup);
            shack.position = new Vector3(-12f, yRoof, 18f);
            Box("ShackBody", shack, new Vector3(0f, 1.35f, 0f), new Vector3(3.2f, 2.7f, 2.6f), art.Steel);
            Box("CorrugatedRoof", shack, new Vector3(0f, 2.75f, 0f), new Vector3(3.5f, 0.1f, 2.9f), art.DiamondPlate, false);

            // Maintenance Gear Clutter beside Shack
            Transform winch = Group("SpareCableWinch", shack);
            winch.localPosition = new Vector3(2.2f, 0f, -0.4f);
            Box("WinchFrame", winch, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.7f), art.Steel);
            Primitive(PrimitiveType.Cylinder, "CableSpool", winch, new Vector3(0f, 0.45f, 0f), new Vector3(0.65f, 0.5f, 0.65f), art.Steel)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Hydraulic oil drums
            Primitive(PrimitiveType.Cylinder, "OilDrum1", shack, new Vector3(2.2f, 0.45f, 0.8f), new Vector3(0.6f, 0.45f, 0.6f), art.Rust);

            // 4. Observation Terrace Amenities (West Deck: y = 44m, x = -16m, z: 10m to 18m)
            Transform terraceAmenities = Group("TerraceAmenities", root);
            float yTerrace = L.RestaurantY;
            terraceAmenities.position = new Vector3(-16f, yTerrace, 14f);

            // Stainless Steel Radiant Patio Heaters ("Mushroom" Heaters)
            foreach (float hz in new[] { -3.2f, 3.2f })
            {
                Transform heater = Group($"PatioHeater_{hz:0.0}", terraceAmenities);
                heater.localPosition = new Vector3(0.8f, 0f, hz);
                Box("BaseTank", heater, new Vector3(0f, 0.4f, 0f), new Vector3(0.45f, 0.8f, 0.45f), art.Steel);
                Primitive(PrimitiveType.Cylinder, "Pole", heater, new Vector3(0f, 1.4f, 0f), new Vector3(0.08f, 1.0f, 0.08f), art.Steel);
                Primitive(PrimitiveType.Sphere, "Emitter", heater, new Vector3(0f, 2.2f, 0f), Vector3.one * 0.35f, art.LampEmissive, false);
                Primitive(PrimitiveType.Cylinder, "MushroomHood", heater, new Vector3(0f, 2.4f, 0f), new Vector3(0.95f, 0.06f, 0.95f), art.Steel, false);
            }

            // Teak Skyline Cocktail Tables
            foreach (float tz in new[] { -1.0f, 1.0f })
            {
                Transform table = Group($"SkylineCocktailTable_{tz:0.0}", terraceAmenities);
                table.localPosition = new Vector3(-0.6f, 0f, tz);
                Primitive(PrimitiveType.Cylinder, "Tabletop", table, new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 0.06f, 0.9f), art.Parquet);
                Primitive(PrimitiveType.Cylinder, "Stem", table, new Vector3(0f, 0.52f, 0f), new Vector3(0.08f, 1.0f, 0.08f), art.Steel);
                Box("BaseCross", table, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.06f, 0.6f), art.Steel, false);
            }
        }
    }
}

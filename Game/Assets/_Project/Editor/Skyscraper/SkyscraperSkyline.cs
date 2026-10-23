using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the rich, atmospheric metropolitan skyline surrounding Cloudline Tower.
    /// Provides convincing urban depth when looking down from street level or out from the 44m/50m summit:
    /// stepped art-deco skyscrapers, twin towers with skybridge, elevated freeway viaducts, rooftop cooling
    /// towers, radio masts, and red aviation hazard beacons.
    /// Authored under Exterior/Skyline for the open world to hide during streaming.
    /// </summary>
    public static class SkyscraperSkyline
    {
        public static void Build(Transform exterior, SkyscraperArt art)
        {
            Transform skyline = Group("Skyline", exterior);

            // 1. Trans-Continental Tower (North-West: -75m, 110m) — 85m Stepped Art-Deco Spire
            Transform tower1 = Group("TransContinentalTower", skyline);
            tower1.position = new Vector3(-75f, 0f, 110f);
            Box("BaseMass", tower1, new Vector3(0f, 25f, 0f), new Vector3(32f, 50f, 32f), art.Concrete, false);
            Box("Setback1", tower1, new Vector3(0f, 58f, 0f), new Vector3(24f, 16f, 24f), art.Concrete, false);
            Box("Setback2", tower1, new Vector3(0f, 72f, 0f), new Vector3(16f, 12f, 16f), art.Glass, false);
            Box("SpireRoof", tower1, new Vector3(0f, 81f, 0f), new Vector3(8f, 6f, 8f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "SpireMast", tower1, new Vector3(0f, 90f, 0f), new Vector3(0.4f, 12f, 0.4f), art.Steel, false);
            Primitive(PrimitiveType.Sphere, "AviationBeacon", tower1, new Vector3(0f, 96.2f, 0f), Vector3.one * 0.8f, art.RedEmergencyEmissive, false);

            // Vertical window ribbing & mechanical louvers
            for (float ox = -12f; ox <= 12f; ox += 6f)
            {
                Box($"FinN_{ox}", tower1, new Vector3(ox, 25f, 16.2f), new Vector3(0.5f, 48f, 0.4f), art.GraniteDark, false);
                Box($"FinS_{ox}", tower1, new Vector3(ox, 25f, -16.2f), new Vector3(0.5f, 48f, 0.4f), art.GraniteDark, false);
            }

            // 2. Metropolitan Financial Plaza (North: 0m, 130m) — 72m Twin Towers with Mid-Level Skybridge
            Transform twinTowers = Group("MetropolitanFinancialPlaza", skyline);
            twinTowers.position = new Vector3(0f, 0f, 130f);

            // West Tower
            Box("TowerWest_Body", twinTowers, new Vector3(-18f, 36f, 0f), new Vector3(22f, 72f, 24f), art.Glass, false);
            Box("TowerWest_RoofPlant", twinTowers, new Vector3(-18f, 74f, 0f), new Vector3(16f, 4f, 18f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "CoolingTowerW", twinTowers, new Vector3(-18f, 77f, 0f), new Vector3(6f, 3f, 6f), art.Rust, false);

            // East Tower
            Box("TowerEast_Body", twinTowers, new Vector3( 18f, 36f, 0f), new Vector3(22f, 72f, 24f), art.Glass, false);
            Box("TowerEast_RoofPlant", twinTowers, new Vector3( 18f, 74f, 0f), new Vector3(16f, 4f, 18f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "CoolingTowerE", twinTowers, new Vector3( 18f, 77f, 0f), new Vector3(6f, 3f, 6f), art.Rust, false);

            // Connecting Skybridge at y = 42m (dramatic overlook parallel to restaurant elevation!)
            Box("SkybridgeBody", twinTowers, new Vector3(0f, 42f, 0f), new Vector3(14f, 4.5f, 6f), art.Steel, false);
            Box("SkybridgeGlass", twinTowers, new Vector3(0f, 42f, -3.1f), new Vector3(13.6f, 3.2f, 0.2f), art.Glass, false);

            // 3. Vanguard Corporate Tower (North-East: 80m, 105m) — 92m Dark Modern Monolith
            Transform tower3 = Group("VanguardTower", skyline);
            tower3.position = new Vector3(80f, 0f, 105f);
            Box("MonolithBody", tower3, new Vector3(0f, 44f, 0f), new Vector3(30f, 88f, 28f), art.GraniteDark, false);
            Box("CrownAngle", tower3, new Vector3(0f, 90f, 0f), new Vector3(26f, 6f, 24f), art.Glass, false);
            Primitive(PrimitiveType.Sphere, "AviationBeacon3", tower3, new Vector3(0f, 94f, 0f), Vector3.one * 0.8f, art.RedEmergencyEmissive, false);

            // 4. Federal Reserve Building (West: -105m, 10m) — 38m Brutalist Colonnade
            Transform fedBldg = Group("FederalReserveBuilding", skyline);
            fedBldg.position = new Vector3(-105f, 0f, 10f);
            Box("Mass", fedBldg, new Vector3(0f, 19f, 0f), new Vector3(36f, 38f, 50f), art.Concrete, false);
            Box("RoofParapet", fedBldg, new Vector3(0f, 38.5f, 0f), new Vector3(36f, 1.5f, 50f), art.Steel, false);

            // 5. Grand Central Highway Viaduct & Overpass (South-West: -60m to -120m, z: -35m to -55m)
            Transform viaduct = Group("ElevatedHighwayViaduct", skyline);
            viaduct.position = new Vector3(-70f, 0f, -45f);
            // Elevated roadway slab at y = 8m
            Box("HighwaySlab", viaduct, new Vector3(0f, 8f, 0f), new Vector3(70f, 1.2f, 14f), art.Asphalt, false);
            Box("BarrierNorth", viaduct, new Vector3(0f, 9.2f,  6.8f), new Vector3(70f, 1.2f, 0.4f), art.Concrete, false);
            Box("BarrierSouth", viaduct, new Vector3(0f, 9.2f, -6.8f), new Vector3(70f, 1.2f, 0.4f), art.Concrete, false);
            // Heavy concrete supporting piers
            for (float px = -28f; px <= 28f; px += 28f)
                Box($"ViaductPier_{px}", viaduct, new Vector3(px, 4f, 0f), new Vector3(3f, 8f, 8f), art.Concrete, false);

            // Abandoned stalled cars on the highway deck
            Box("WreckSedan1", viaduct, new Vector3(-15f, 9.2f,  2f), new Vector3(1.8f, 1.2f, 4.4f), art.CarPaintBurgundy, false);
            Box("WreckVan",    viaduct, new Vector3(  8f, 9.4f, -2f), new Vector3(2.1f, 1.8f, 5.2f), art.CarPaintBlack, false);

            // 6. Suspension Bridge Anchor Tower (East: 125m, 15m) — 110m Cable Tower
            Transform bridgeTower = Group("SuspensionBridgeTower", skyline);
            bridgeTower.position = new Vector3(125f, 0f, 15f);
            Box("BridgeLeg1", bridgeTower, new Vector3(0f, 55f, -8f), new Vector3(4f, 110f, 4f), art.Steel, false);
            Box("BridgeLeg2", bridgeTower, new Vector3(0f, 55f,  8f), new Vector3(4f, 110f, 4f), art.Steel, false);
            Box("CrossPortal1", bridgeTower, new Vector3(0f, 45f, 0f), new Vector3(3.8f, 4f, 16f), art.Steel, false);
            Box("CrossPortal2", bridgeTower, new Vector3(0f, 85f, 0f), new Vector3(3.8f, 4f, 16f), art.Steel, false);
            Primitive(PrimitiveType.Sphere, "TowerBeacon", bridgeTower, new Vector3(0f, 111f, 0f), Vector3.one * 1.0f, art.RedEmergencyEmissive, false);
        }
    }
}

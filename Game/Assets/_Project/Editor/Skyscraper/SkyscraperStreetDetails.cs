using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds dense municipal and street-level environmental storytelling around Cloudline Tower:
    /// - Metro subway station entrance with cast-iron railings, steps, globe lamps, and signage.
    /// - Sidewalk newsstand kiosk with shuttered counter, magazine racks, bundled newspapers, and soda machine.
    /// - Cantilevered transit bus passenger shelter with illuminated route advertisement billboard.
    /// - Utility excavation roadwork trench with steel trench plates, hazard drums, and flashing sawhorses.
    /// - Municipal street fixtures: fire hydrants, USPS postal mailbox, parking meters, electrical cabinet.
    /// - West delivery service alleyway with wooden pallet stacks, grease drums, and fire escape ladder.
    /// </summary>
    public static class SkyscraperStreetDetails
    {
        public static void Build(Transform exterior, SkyscraperArt art)
        {
            Transform detailsGroup = Group("StreetDetails", exterior);

            // 1. Metro Subway Entrance (South-West Sidewalk: x = -17m, z = -30m)
            Transform metro = Group("SubwayEntrance", detailsGroup);
            metro.position = new Vector3(-17f, 0f, -30f);

            // Perimeter retaining wall & concrete parapet
            Box("CurbWallW", metro, new Vector3(-2.1f, 0.45f, 0f), new Vector3(0.3f, 0.9f, 6.2f), art.Concrete);
            Box("CurbWallE", metro, new Vector3( 2.1f, 0.45f, 0f), new Vector3(0.3f, 0.9f, 6.2f), art.Concrete);
            Box("CurbWallN", metro, new Vector3(0f, 0.45f, 3.1f), new Vector3(4.5f, 0.9f, 0.3f), art.Concrete);

            // Cast-iron perimeter railing
            Box("RailingW", metro, new Vector3(-2.1f, 1.15f, 0f), new Vector3(0.08f, 0.5f, 6.0f), art.Steel, false);
            Box("RailingE", metro, new Vector3( 2.1f, 1.15f, 0f), new Vector3(0.08f, 0.5f, 6.0f), art.Steel, false);
            Box("RailingN", metro, new Vector3(0f, 1.15f, 3.1f), new Vector3(4.3f, 0.5f, 0.08f), art.Steel, false);

            // Entrance Portal Archway (South Side: z = -3.0m)
            Transform arch = Group("PortalArch", metro);
            arch.position = new Vector3(0f, 0f, -3.0f);
            Primitive(PrimitiveType.Cylinder, "PillarL", arch, new Vector3(-1.9f, 1.4f, 0f), new Vector3(0.2f, 1.4f, 0.2f), art.GraniteDark);
            Primitive(PrimitiveType.Cylinder, "PillarR", arch, new Vector3( 1.9f, 1.4f, 0f), new Vector3(0.2f, 1.4f, 0.2f), art.GraniteDark);
            Box("HeaderArch", arch, new Vector3(0f, 2.85f, 0f), new Vector3(4.0f, 0.3f, 0.25f), art.Steel);
            Box("MetroSign", arch, new Vector3(0f, 3.15f, 0f), new Vector3(3.6f, 0.4f, 0.08f), art.GraniteDark, false);

            // Twin Illuminated Green/Bronze Globe Lamps
            foreach (float side in new[] { -1.9f, 1.9f })
            {
                Primitive(PrimitiveType.Sphere, "GlobeLamp", arch, new Vector3(side, 3.2f, 0f), Vector3.one * 0.45f, art.LampEmissive, false);
            }

            // Descending Stairwell Steps (Tiled steps going down from z = -3 to +3, y = 0 to -2.4m)
            for (int s = 0; s < 10; s++)
            {
                float sy = -s * 0.24f;
                float sz = -2.6f + s * 0.55f;
                Box($"Step_{s}", metro, new Vector3(0f, sy - 0.12f, sz), new Vector3(3.8f, 0.24f, 0.6f), art.Concrete);
            }
            // Chained iron security gate at the bottom
            Box("SecurityGate", metro, new Vector3(0f, -1.2f, 2.8f), new Vector3(3.8f, 2.4f, 0.1f), art.Steel);

            // 2. Sidewalk Newsstand Kiosk (South-East Sidewalk: x = 16m, z = -22.5m)
            Transform newsstand = Group("NewsstandKiosk", detailsGroup);
            newsstand.position = new Vector3(16f, 0f, -22.5f);
            Box("KioskBody", newsstand, new Vector3(0f, 1.15f, 0f), new Vector3(2.6f, 2.3f, 1.8f), art.Steel);
            Box("KioskRoof", newsstand, new Vector3(0f, 2.35f, 0f), new Vector3(3.0f, 0.12f, 2.2f), art.Steel);
            Box("AwningOverhang", newsstand, new Vector3(0f, 2.25f, -1.2f), new Vector3(2.8f, 0.08f, 0.8f), art.CarPaintBurgundy, false);

            // Front Counter & Shuttered Grill
            Box("CounterShelf", newsstand, new Vector3(0f, 0.9f, -0.95f), new Vector3(2.4f, 0.08f, 0.4f), art.Parquet);
            Box("RollUpGrill", newsstand, new Vector3(0f, 1.6f, -0.91f), new Vector3(2.4f, 1.2f, 0.04f), art.DiamondPlate, false);

            // Magazine Display Rack
            Box("MagazineCard1", newsstand, new Vector3(-0.6f, 1.1f, -0.95f), new Vector3(0.5f, 0.35f, 0.02f), art.RoadYellow, false);
            Box("MagazineCard2", newsstand, new Vector3( 0.6f, 1.1f, -0.95f), new Vector3(0.5f, 0.35f, 0.02f), art.CarPaintBurgundy, false);

            // Bundled newspaper stacks on the pavement beside kiosk
            Box("NewspaperBundle1", newsstand, new Vector3(-1.8f, 0.25f, 0.2f), new Vector3(0.7f, 0.5f, 0.6f), art.WallPlaster, false);
            Box("NewspaperBundle2", newsstand, new Vector3(-1.8f, 0.65f, 0.2f), new Vector3(0.65f, 0.3f, 0.55f), art.WallPlaster, false);

            // Vintage Beverage Cooler / Vending Machine
            Transform soda = Group("SodaVendingMachine", newsstand);
            soda.position = new Vector3(1.9f, 0f, 0f);
            Box("VenderBody", soda, new Vector3(0f, 1.0f, 0f), new Vector3(1.0f, 2.0f, 0.9f), art.CarPaintBurgundy);
            Box("DisplayLight", soda, new Vector3(0f, 1.1f, -0.46f), new Vector3(0.8f, 1.4f, 0.04f), art.LampEmissive, false);

            // 3. Transit Bus Passenger Shelter (South Sidewalk: x = -9m, z = -22.2m)
            Transform busShelter = Group("BusShelter", detailsGroup);
            busShelter.position = new Vector3(-9f, 0f, -22.2f);
            Box("BackGlass", busShelter, new Vector3(0f, 1.35f, 0.75f), new Vector3(3.8f, 2.4f, 0.06f), art.Glass);
            Box("GlassRoof", busShelter, new Vector3(0f, 2.55f, 0f), new Vector3(4.0f, 0.08f, 1.8f), art.Glass, false);
            Box("SupportLegL", busShelter, new Vector3(-1.9f, 1.25f, 0.75f), new Vector3(0.1f, 2.5f, 0.1f), art.Steel);
            Box("SupportLegR", busShelter, new Vector3( 1.9f, 1.25f, 0.75f), new Vector3(0.1f, 2.5f, 0.1f), art.Steel);
            Box("ShelterBench", busShelter, new Vector3(0f, 0.45f, 0.4f), new Vector3(2.6f, 0.06f, 0.4f), art.Steel);

            // Double-Sided Illuminated Ad Billboard
            Transform adBillboard = Group("RouteAdBillboard", busShelter);
            adBillboard.position = new Vector3(2.1f, 0f, 0f);
            Box("AdPlinth", adBillboard, new Vector3(0f, 1.25f, 0f), new Vector3(0.2f, 2.5f, 1.4f), art.Steel);
            Box("AdPosterW", adBillboard, new Vector3(-0.11f, 1.35f, 0f), new Vector3(0.02f, 1.8f, 1.1f), art.LampEmissive, false);
            Box("AdPosterE", adBillboard, new Vector3( 0.11f, 1.35f, 0f), new Vector3(0.02f, 1.8f, 1.1f), art.LampEmissive, false);

            // 4. Utility Roadwork Excavation Trench (Street Lane: x = 2m, z = -30m)
            Transform roadwork = Group("StreetRoadwork", detailsGroup);
            roadwork.position = new Vector3(2f, 0f, -30f);
            // Excavation hole / depression
            Box("TrenchPit", roadwork, new Vector3(0f, -0.3f, 0f), new Vector3(3.4f, 0.5f, 2.2f), art.Asphalt);
            // Heavy steel road plates bridging across
            Box("TrenchPlate1", roadwork, new Vector3(-0.8f, 0.03f, 0f), new Vector3(1.6f, 0.04f, 2.4f), art.DiamondPlate);
            Box("TrenchPlate2", roadwork, new Vector3( 0.8f, 0.03f, 0f), new Vector3(1.6f, 0.04f, 2.4f), art.DiamondPlate);

            // Construction Hazard Barrels
            Vector3[] barrelPos = { new(-2.0f, 0.45f, -1.3f), new(2.0f, 0.45f, -1.3f), new(-2.0f, 0.45f, 1.3f), new(2.0f, 0.45f, 1.3f) };
            for (int b = 0; b < barrelPos.Length; b++)
            {
                Transform barrel = Group($"HazardBarrel_{b}", roadwork);
                barrel.position = roadwork.position + barrelPos[b];
                Primitive(PrimitiveType.Cylinder, "BarrelBody", barrel, Vector3.zero, new Vector3(0.6f, 0.45f, 0.6f), art.RoadYellow);
                Box("ReflectiveStripe", barrel, new Vector3(0f, 0.1f, 0f), new Vector3(0.62f, 0.15f, 0.62f), art.RoadWhite, false);
            }

            // Wooden Sawhorse Barricades with Flashing Hazard Beacon
            Transform sawhorse = Group("RoadworkSawhorse", roadwork);
            sawhorse.position = new Vector3(0f, 0f, -1.6f);
            Box("SawhorseCrossbar", sawhorse, new Vector3(0f, 0.75f, 0f), new Vector3(2.4f, 0.14f, 0.08f), art.RoadYellow);
            Box("SawhorseStripe", sawhorse, new Vector3(0f, 0.75f, -0.045f), new Vector3(2.4f, 0.08f, 0.01f), art.RoadWhite, false);
            Box("SawhorseLegL", sawhorse, new Vector3(-1.0f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.4f), art.Parquet, false);
            Box("SawhorseLegR", sawhorse, new Vector3( 1.0f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.4f), art.Parquet, false);
            Primitive(PrimitiveType.Cylinder, "AmberBeacon", sawhorse, new Vector3(0.7f, 0.9f, 0f), new Vector3(0.14f, 0.12f, 0.14f), art.LampEmissive, false);

            // 5. Municipal Curbside Infrastructure Props
            Transform props = Group("MunicipalProps", detailsGroup);
            // Cast-Iron Fire Hydrants
            BuildHydrant(props, art, new Vector3(-14f, 0f, -21.4f));
            BuildHydrant(props, art, new Vector3( 14f, 0f, -21.4f));

            // USPS Blue Postal Mailbox
            Transform mailbox = Group("PostalMailbox", props);
            mailbox.position = new Vector3(4.8f, 0f, -21.4f);
            Box("MailboxBody", mailbox, new Vector3(0f, 0.65f, 0f), new Vector3(0.65f, 1.1f, 0.55f), art.Steel);
            Box("MailboxLegs", mailbox, new Vector3(0f, 0.1f, 0f), new Vector3(0.6f, 0.2f, 0.5f), art.Steel, false);
            Box("DropFlap", mailbox, new Vector3(0f, 1.15f, -0.2f), new Vector3(0.5f, 0.12f, 0.15f), art.Brass, false);

            // Curbside Parking Meters
            BuildParkingMeter(props, art, new Vector3(-3.5f, 0f, -21.4f));
            BuildParkingMeter(props, art, new Vector3( 11.5f, 0f, -21.4f));

            // Green Electrical Transformer Cabinet on Concrete Pad
            Transform transformer = Group("SidewalkTransformerCabinet", props);
            transformer.position = new Vector3(-18.5f, 0f, -17.5f);
            Box("ConcretePad", transformer, new Vector3(0f, 0.1f, 0f), new Vector3(1.6f, 0.2f, 1.4f), art.Concrete);
            Box("CabinetBody", transformer, new Vector3(0f, 0.85f, 0f), new Vector3(1.3f, 1.3f, 1.1f), art.Steel);
            Box("WarningSign", transformer, new Vector3(0f, 0.95f, -0.56f), new Vector3(0.35f, 0.25f, 0.02f), art.RoadYellow, false);

            // 6. West Delivery Service Alleyway (x = -20m to -24m, z = -14m to 0m)
            Transform alley = Group("ServiceAlleyway", detailsGroup);
            alley.position = new Vector3(-18.2f, 0f, -12f);
            // Wooden shipping pallet stacks
            for (int p = 0; p < 4; p++)
                Box($"Pallet_{p}", alley, new Vector3(0f, 0.08f + p * 0.15f, 0f), new Vector3(1.2f, 0.14f, 1.2f), art.Parquet);

            // Steel Grease / Oil Drums
            Primitive(PrimitiveType.Cylinder, "GreaseDrum1", alley, new Vector3(-0.8f, 0.45f, 1.6f), new Vector3(0.6f, 0.45f, 0.6f), art.Steel);
            Primitive(PrimitiveType.Cylinder, "GreaseDrum2", alley, new Vector3( 0.2f, 0.45f, 1.8f), new Vector3(0.6f, 0.45f, 0.6f), art.Rust);

            // Suspended Metal Fire Escape Landing (y = 3.8m on western facade)
            Transform fireEscape = Group("AlleyFireEscape", alley);
            fireEscape.position = new Vector3(-1.2f, 3.8f, 0f);
            Box("LandingGrate", fireEscape, Vector3.zero, new Vector3(1.4f, 0.08f, 3.0f), art.DiamondPlate);
            Box("RailingAlley", fireEscape, new Vector3(-0.65f, 0.5f, 0f), new Vector3(0.06f, 1.0f, 3.0f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Counterweight", fireEscape, new Vector3(0f, -1.2f, -1.2f), new Vector3(0.2f, 0.3f, 0.2f), art.Rust, false);
        }

        private static void BuildHydrant(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform hyd = Group("FireHydrant", parent);
            hyd.position = at;
            Primitive(PrimitiveType.Cylinder, "Base", hyd, new Vector3(0f, 0.35f, 0f), new Vector3(0.3f, 0.35f, 0.3f), art.CarPaintBurgundy);
            Primitive(PrimitiveType.Sphere, "Bonnet", hyd, new Vector3(0f, 0.72f, 0f), Vector3.one * 0.32f, art.CarPaintBurgundy);
            Box("NozzleCapL", hyd, new Vector3(-0.2f, 0.45f, 0f), new Vector3(0.15f, 0.12f, 0.12f), art.Brass, false);
            Box("NozzleCapR", hyd, new Vector3( 0.2f, 0.45f, 0f), new Vector3(0.15f, 0.12f, 0.12f), art.Brass, false);
        }

        private static void BuildParkingMeter(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform meter = Group("ParkingMeter", parent);
            meter.position = at;
            Primitive(PrimitiveType.Cylinder, "Post", meter, new Vector3(0f, 0.55f, 0f), new Vector3(0.08f, 0.55f, 0.08f), art.Steel);
            Box("Head", meter, new Vector3(0f, 1.25f, 0f), new Vector3(0.2f, 0.35f, 0.16f), art.Steel);
            Box("CoinWindow", meter, new Vector3(0f, 1.32f, -0.09f), new Vector3(0.14f, 0.12f, 0.02f), art.Glass, false);
        }
    }
}

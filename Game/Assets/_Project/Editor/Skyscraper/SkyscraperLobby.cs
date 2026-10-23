using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the grand, double-height Carrara marble lobby of Cloudline Tower:
    /// - Paved marble floor with dark granite border inlays.
    /// - Fluted double-height structural columns supporting coffered ceiling at 6m.
    /// - Double-height glazed entrance facade with revolving glass cylinder doors and side egress doors.
    /// - Curved concierge reception desk with brass fixtures, reservations register, and key cubbies.
    /// - Optical security turnstiles and walk-through metal detector screening checkpoint.
    /// - Executive waiting lounge with leather sofas, smoked-glass coffee table, and architectural planter.
    /// - Monument tenant directory plaque and mail chute bank.
    /// </summary>
    public static class SkyscraperLobby
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform lobby = Group("Lobby", root);
            float halfW = L.TowerHalfWidth;

            // 1. Flooring: Carrara Marble Main Field with Dark Granite Perimeter
            Transform floorGroup = Group("LobbyFlooring", lobby);
            GameObject floor = Box("FloorField_Marble", floorGroup, new Vector3(0f, -0.1f, -6f),
                new Vector3(halfW * 2f - 1.2f, 0.2f, 7.8f), art.Marble);
            floor.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Tile);

            // Dark Granite Border Strips
            Box("BorderFront", floorGroup, new Vector3(0f, -0.09f, L.TowerFrontZ + 0.5f), new Vector3(halfW * 2f, 0.22f, 1.0f), art.GraniteDark);
            Box("BorderWest", floorGroup, new Vector3(-halfW + 0.5f, -0.09f, -6f), new Vector3(1.0f, 0.22f, 8.0f), art.GraniteDark);
            Box("BorderEast", floorGroup, new Vector3( halfW - 0.5f, -0.09f, -6f), new Vector3(1.0f, 0.22f, 8.0f), art.GraniteDark);
            Box("BorderCore", floorGroup, new Vector3(0f, -0.09f, -2.1f), new Vector3(halfW * 2f, 0.22f, 0.6f), art.GraniteDark);

            // 2. Coffered Ceiling (y = 6m)
            Box("LobbyCeiling", lobby, new Vector3(0f, L.LobbyHeight, -6f), new Vector3(halfW * 2f - 0.4f, 0.3f, 8.2f), art.CeilingTile);

            // 3. Grand Double-Height Colonnade (4 Fluted Structural Pillars at y = 0 to 6m)
            Transform colonnade = Group("InteriorColonnade", lobby);
            Vector3[] pillarPos = {
                new(-8f, 3f, -7.5f), new(-8f, 3f, -3.5f),
                new( 8f, 3f, -7.5f), new( 8f, 3f, -3.5f)
            };
            foreach (var pos in pillarPos)
            {
                Transform col = Group("Pillar", colonnade);
                col.position = pos;
                Primitive(PrimitiveType.Cylinder, "Shaft", col, Vector3.zero, new Vector3(1.1f, 3.0f, 1.1f), art.GraniteDark);
                Box("Base", col, new Vector3(0f, -2.85f, 0f), new Vector3(1.4f, 0.3f, 1.4f), art.Brass);
                Box("Capital", col, new Vector3(0f, 2.85f, 0f), new Vector3(1.4f, 0.3f, 1.4f), art.Brass);
            }

            // 4. Glazed Front Entrance Facade & Revolving Doors (South Wall: z = -10m)
            Transform entrance = Group("EntranceVestibule", lobby);
            Box("FrontGlazingL", entrance, new Vector3(-10f, 3f, L.TowerFrontZ), new Vector3(14.8f, 5.8f, 0.15f), art.Glass);
            Box("FrontGlazingR", entrance, new Vector3( 10f, 3f, L.TowerFrontZ), new Vector3(14.8f, 5.8f, 0.15f), art.Glass);
            Box("PortalFrame", entrance, new Vector3(0f, 3f, L.TowerFrontZ), new Vector3(5.2f, 6.0f, 0.3f), art.Steel);

            // Revolving Door Drum & Glass Wings
            Transform rev = Group("RevolvingDoor", entrance);
            rev.position = new Vector3(0f, 0f, L.TowerFrontZ);
            Primitive(PrimitiveType.Cylinder, "EnclosureGlass", rev, new Vector3(0f, 1.8f, 0f), new Vector3(3.2f, 1.8f, 3.2f), art.Glass, false);
            Box("RevolvingShaft", rev, new Vector3(0f, 1.8f, 0f), new Vector3(0.12f, 3.6f, 0.12f), art.Brass);
            for (int w = 0; w < 3; w++)
            {
                float rot = w * 120f;
                GameObject wing = Box($"Wing_{w}", rev, new Vector3(0f, 1.8f, 0.75f), new Vector3(0.04f, 3.4f, 1.5f), art.Glass, false);
                wing.transform.localRotation = Quaternion.Euler(0f, rot, 0f);
            }

            // 5. Concierge / Reception Desk (x = 6.5m, z = -3.8m)
            Transform desk = Group("ConciergeDesk", lobby);
            desk.position = new Vector3(6.5f, 0f, -3.8f);
            Box("CounterPlinth", desk, new Vector3(0f, 0.55f, 0f), new Vector3(5.0f, 1.1f, 1.0f), art.GraniteDark);
            Box("CounterTopBrass", desk, new Vector3(0f, 1.15f, 0f), new Vector3(5.2f, 0.1f, 1.2f), art.Brass);
            Box("BackDesk", desk, new Vector3(0f, 0.45f, 1.2f), new Vector3(4.5f, 0.9f, 0.7f), art.Mahogany);
            Box("KeyCardRack", desk, new Vector3(2.0f, 1.6f, 1.5f), new Vector3(0.8f, 0.8f, 0.1f), art.Brass);
            Box("ReservationLedger", desk, new Vector3(-1.0f, 1.22f, 0f), new Vector3(0.45f, 0.04f, 0.35f), art.Leather, false);

            // 6. Security Turnstile & Screening Checkpoint (x = -1 to -5, z = -5.5m)
            Transform checkpoint = Group("SecurityCheckpoint", lobby);
            for (float x = -3.5f; x <= 0.5f; x += 1.4f)
            {
                Transform turnstile = Group($"Turnstile_{x:0.0}", checkpoint);
                turnstile.position = new Vector3(x, 0f, -5.5f);
                Box("Podium", turnstile, new Vector3(0f, 0.5f, 0f), new Vector3(0.24f, 1.0f, 1.2f), art.Steel);
                Box("GlassFlap", turnstile, new Vector3(0f, 0.55f, 0f), new Vector3(0.03f, 0.8f, 0.9f), art.Glass, false);
            }

            // Walk-Through Metal Detector Archway
            Transform arch = Group("MetalDetectorPortal", checkpoint);
            arch.position = new Vector3(-5.2f, 0f, -5.5f);
            Box("PostL", arch, new Vector3(-0.5f, 1.1f, 0f), new Vector3(0.15f, 2.2f, 0.5f), art.Steel);
            Box("PostR", arch, new Vector3( 0.5f, 1.1f, 0f), new Vector3(0.15f, 2.2f, 0.5f), art.Steel);
            Box("Header", arch, new Vector3(0f, 2.25f, 0f), new Vector3(1.15f, 0.15f, 0.5f), art.Steel);
            Box("Indicator", arch, new Vector3(0f, 2.25f, -0.26f), new Vector3(0.2f, 0.08f, 0.02f), art.RedEmergencyEmissive, false);

            // 7. Executive Waiting Lounge (East Wing: x = 11 to 16, z = -7 to -4)
            Transform lounge = Group("WaitingLounge", lobby);
            lounge.position = new Vector3(13.5f, 0f, -6.0f);
            Box("SofaBase", lounge, new Vector3(0f, 0.25f, 0f), new Vector3(2.4f, 0.5f, 0.9f), art.Leather);
            Box("SofaBack", lounge, new Vector3(0f, 0.65f, -0.35f), new Vector3(2.4f, 0.6f, 0.25f), art.Leather);
            Box("SofaArmL", lounge, new Vector3(-1.15f, 0.5f, 0f), new Vector3(0.2f, 0.5f, 0.9f), art.Leather);
            Box("SofaArmR", lounge, new Vector3( 1.15f, 0.5f, 0f), new Vector3(0.2f, 0.5f, 0.9f), art.Leather);

            // Smoked glass coffee table
            Box("CoffeeTableGlass", lounge, new Vector3(0f, 0.4f, 1.0f), new Vector3(1.8f, 0.04f, 0.8f), art.Glass);
            Box("CoffeeTableLegs", lounge, new Vector3(0f, 0.2f, 1.0f), new Vector3(1.6f, 0.38f, 0.6f), art.Brass, false);

            // Architectural Potted Ficus
            Transform plant = Group("LobbyFicus", lounge);
            plant.position = new Vector3(-1.8f, 0f, 0f);
            Primitive(PrimitiveType.Cylinder, "PlanterPot", plant, new Vector3(0f, 0.4f, 0f), new Vector3(0.7f, 0.4f, 0.7f), art.Brass);
            Primitive(PrimitiveType.Cylinder, "Stem", plant, new Vector3(0f, 1.2f, 0f), new Vector3(0.08f, 1.2f, 0.08f), art.Mahogany, false);
            Primitive(PrimitiveType.Sphere, "Foliage", plant, new Vector3(0f, 2.0f, 0f), Vector3.one * 1.1f, art.PlazaPaver, false);

            // 8. Tenant Wayfinding Monument & Building Directory (x = -7.5m, z = -4.5m)
            Transform dir = Group("TenantDirectoryMonument", lobby);
            dir.position = new Vector3(-7.5f, 0f, -4.5f);
            Box("DirectoryPlinth", dir, new Vector3(0f, 0.9f, 0f), new Vector3(2.2f, 1.8f, 0.25f), art.GraniteDark);
            Box("BrassPlaque", dir, new Vector3(0f, 1.0f, -0.14f), new Vector3(1.9f, 1.4f, 0.02f), art.Brass, false);

            // 9. Elevator Vestibule Portals & Lobby Details (4-Car Bank, Cafe, Valet, Security)
            SkyscraperLobbyDetails.Build(lobby, art);

            // Stairwell fire door portal at x = -8.9f, z = -2.4m
            Transform portals = Group("CorePortals", lobby);
            Box("StairDoorFrame", portals, new Vector3(-8.9f, 1.4f, -2.4f), new Vector3(0.3f, 2.8f, 1.4f), art.Steel);
        }
    }
}

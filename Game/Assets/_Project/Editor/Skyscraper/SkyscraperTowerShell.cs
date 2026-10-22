using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the tower shell, street-level arrival plaza, truck parking bay, entrance canopy,
    /// and the grand double-height marble lobby with concierge desk and security turnstiles.
    /// </summary>
    public static class SkyscraperTowerShell
    {
        public static TruckCargo Build(Transform root, SkyscraperArt art)
        {
            Transform exterior = Group("Exterior", root);
            Transform plaza = Group("Plaza", exterior);

            // 1. Street Forecourt & Parking Plaza (Asphalt)
            Box("StreetPavement", plaza, new Vector3(0f, -0.2f, -22f), new Vector3(48f, 0.4f, 26f), art.Asphalt);

            // Sidewalk curb and entrance apron (Concrete)
            Box("CurbApron", plaza, new Vector3(0f, 0.05f, -13f), new Vector3(40f, 0.2f, 8f), art.Concrete);

            // Concrete bollards guarding the entrance
            for (float x = -15f; x <= 15f; x += 3.75f)
            {
                if (Mathf.Abs(x) < 3.0f) continue; // Clear walkway to entrance
                Primitive(PrimitiveType.Cylinder, "Bollard", plaza, new Vector3(x, 0.5f, -16.5f),
                    new Vector3(0.35f, 0.5f, 0.35f), art.Concrete);
            }

            // Company extraction truck at designated parking spot (hidden when loaded in open world)
            Transform truckGroup = Group("Truck", exterior);
            TruckCargo cargo = TruckBuilder.Build(truckGroup, L.TruckRear, L.TruckForward);

            // Stand-in skyline building silhouettes around tower (hidden when loaded in open world)
            Transform skyline = Group("Skyline", exterior);
            Vector3[] distantTowers = {
                new(-90f, 20f, 110f), new(-45f, 35f, 140f), new(40f, 25f, 130f), new(85f, 40f, 100f),
                new(-110f, 15f, -20f), new(120f, 30f, -10f)
            };
            Vector3[] towerDims = {
                new(28f, 70f, 28f), new(34f, 90f, 34f), new(30f, 80f, 30f), new(36f, 100f, 36f),
                new(26f, 60f, 26f), new(32f, 85f, 32f)
            };
            for (int k = 0; k < distantTowers.Length; k++)
                Box($"TowerSilhouette_{k}", skyline, distantTowers[k], towerDims[k], art.Concrete, false);

            // 2. Entrance Canopy & Modern Facade
            Transform entrance = Group("EntranceCanopy", plaza);
            Box("CanopyRoof", entrance, new Vector3(0f, 4.5f, -13.5f), new Vector3(14f, 0.3f, 7f), art.Steel);
            Box("GlassAwning", entrance, new Vector3(0f, 4.4f, -13.5f), new Vector3(13.6f, 0.05f, 6.6f), art.Glass);
            Box("SupportPillarL", entrance, new Vector3(-6.5f, 2.2f, -16.8f), new Vector3(0.4f, 4.4f, 0.4f), art.Steel);
            Box("SupportPillarR", entrance, new Vector3( 6.5f, 2.2f, -16.8f), new Vector3(0.4f, 4.4f, 0.4f), art.Steel);

            // 3. Skyscraper Massive Structural Columns & Curtain Facade
            Transform towerShell = Group("TowerFacade", exterior);
            float halfW = L.TowerHalfWidth;
            float midZ = (L.TowerBackZ + L.TowerFrontZ) * 0.5f;
            float depth = L.TowerBackZ - L.TowerFrontZ;

            // 4 Massive corner concrete pylons
            Vector3 colSize = new(2.2f, L.RoofY, 2.2f);
            Box("CornerPylon_SW", towerShell, new Vector3(-halfW + 1.1f, L.RoofY * 0.5f, L.TowerFrontZ + 1.1f), colSize, art.Concrete);
            Box("CornerPylon_SE", towerShell, new Vector3( halfW - 1.1f, L.RoofY * 0.5f, L.TowerFrontZ + 1.1f), colSize, art.Concrete);
            Box("CornerPylon_NW", towerShell, new Vector3(-halfW + 1.1f, L.RoofY * 0.5f, L.TowerBackZ - 1.1f), colSize, art.Concrete);
            Box("CornerPylon_NE", towerShell, new Vector3( halfW - 1.1f, L.RoofY * 0.5f, L.TowerBackZ - 1.1f), colSize, art.Concrete);

            // Exterior Curtain Glass Wall Panelling
            Box("FacadeGlass_East", towerShell, new Vector3( halfW, L.RoofY * 0.5f, midZ), new Vector3(0.2f, L.RoofY, depth - 2.2f), art.Glass);
            Box("FacadeGlass_West", towerShell, new Vector3(-halfW, L.RoofY * 0.5f, midZ), new Vector3(0.2f, L.RoofY, depth - 2.2f), art.Glass);
            Box("FacadeGlass_North", towerShell, new Vector3(0f, L.RoofY * 0.5f, L.TowerBackZ), new Vector3(halfW * 2f - 2.2f, L.RoofY, 0.2f), art.Glass);
            Box("FacadeGlass_SouthUpper", towerShell, new Vector3(0f, (L.RoofY + L.LobbyHeight) * 0.5f, L.TowerFrontZ),
                new Vector3(halfW * 2f - 2.2f, L.RoofY - L.LobbyHeight, 0.2f), art.Glass);

            // Roof Crown / Parapet Wall
            Box("RoofSlab", towerShell, new Vector3(0f, L.RoofY, midZ), new Vector3(halfW * 2f, 0.5f, depth), art.Concrete);
            Box("ParapetN", towerShell, new Vector3(0f, L.RoofY + 0.8f, L.TowerBackZ), new Vector3(halfW * 2f, 1.2f, 0.4f), art.Steel);
            Box("ParapetS", towerShell, new Vector3(0f, L.RoofY + 0.8f, L.TowerFrontZ), new Vector3(halfW * 2f, 1.2f, 0.4f), art.Steel);
            Box("ParapetE", towerShell, new Vector3( halfW, L.RoofY + 0.8f, midZ), new Vector3(0.4f, 1.2f, depth), art.Steel);
            Box("ParapetW", towerShell, new Vector3(-halfW, L.RoofY + 0.8f, midZ), new Vector3(0.4f, 1.2f, depth), art.Steel);

            // 4. Ground Level Grand Marble Lobby (y: 0 to 6m)
            Transform lobby = Group("Lobby", root);
            Box("LobbyFloor", lobby, new Vector3(0f, -0.1f, midZ), new Vector3(halfW * 2f - 0.4f, 0.2f, depth - 0.4f), art.Marble);
            Box("LobbyCeiling", lobby, new Vector3(0f, L.LobbyHeight, midZ), new Vector3(halfW * 2f - 0.4f, 0.3f, depth - 0.4f), art.CeilingTile);

            // Lobby Entrance Glazing & Revolving Door Shell
            Box("LobbyFrontGlassL", lobby, new Vector3(-9.5f, 2.5f, L.TowerFrontZ), new Vector3(15f, 5f, 0.2f), art.Glass);
            Box("LobbyFrontGlassR", lobby, new Vector3( 9.5f, 2.5f, L.TowerFrontZ), new Vector3(15f, 5f, 0.2f), art.Glass);
            Box("RevolvingCylinder", lobby, new Vector3(0f, 1.8f, L.TowerFrontZ), new Vector3(3.2f, 3.6f, 3.2f), art.Steel, false);

            // Reception Concierge Desk (Marble & Brass)
            Transform desk = Group("ReceptionDesk", lobby);
            Box("DeskFront", desk, new Vector3(6f, 0.6f, -3f), new Vector3(5f, 1.2f, 0.8f), art.Marble);
            Box("DeskTop", desk, new Vector3(6f, 1.22f, -3f), new Vector3(5.2f, 0.08f, 1.0f), art.Brass);
            Box("DeskBackCounter", desk, new Vector3(6f, 0.5f, -1.2f), new Vector3(4.5f, 1.0f, 0.6f), art.Steel);

            // Security Turnstiles (Steel & Glass)
            for (float x = -3.5f; x <= 3.5f; x += 1.75f)
            {
                Box("Turnstile", lobby, new Vector3(x, 0.55f, -5f), new Vector3(0.25f, 1.1f, 1.4f), art.Steel);
                Box("TurnstileGlass", lobby, new Vector3(x, 0.6f, -5f), new Vector3(0.04f, 0.8f, 1.1f), art.Glass, false);
            }

            // Wayfinding Directory Board
            Box("DirectoryBoard", lobby, new Vector3(-6f, 1.8f, -4f), new Vector3(1.6f, 2.4f, 0.15f), art.Steel);

            return cargo;
        }
    }
}

using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the tower's architectural exterior shell, fluted entrance colonnade, porte-cochère,
    /// vertical structural mullions, curtain-glass facade, and rooftop crown parapet.
    /// Integrates the surrounding metropolitan skyline and forecourt plaza.
    /// </summary>
    public static class SkyscraperTowerShell
    {
        public static TruckCargo Build(Transform root, SkyscraperArt art)
        {
            Transform exterior = Group("Exterior", root);

            // 1. Surrounding City Skyline Silhouettes (hidden when streamed in open world)
            SkyscraperSkyline.Build(exterior, art);

            // 2. Broadway Streetscape, Forecourt Plaza, Vehicles, and Van Parking Bay
            TruckCargo cargo = SkyscraperPlaza.Build(exterior, art);
            SkyscraperStreetDetails.Build(exterior, art);

            // 3. Massive Skyscraper Structural Facade & Tower Massing
            Transform facade = Group("TowerFacade", exterior);
            float halfW = L.TowerHalfWidth;
            float midZ = (L.TowerBackZ + L.TowerFrontZ) * 0.5f;
            float depth = L.TowerBackZ - L.TowerFrontZ;

            // 4 Massive Corner Concrete Pylons (50m vertical structural anchors)
            Vector3 colSize = new(2.2f, L.RoofY, 2.2f);
            Box("CornerPylon_SW", facade, new Vector3(-halfW + 1.1f, L.RoofY * 0.5f, L.TowerFrontZ + 1.1f), colSize, art.Concrete);
            Box("CornerPylon_SE", facade, new Vector3( halfW - 1.1f, L.RoofY * 0.5f, L.TowerFrontZ + 1.1f), colSize, art.Concrete);
            Box("CornerPylon_NW", facade, new Vector3(-halfW + 1.1f, L.RoofY * 0.5f, L.TowerBackZ - 1.1f), colSize, art.Concrete);
            Box("CornerPylon_NE", facade, new Vector3( halfW - 1.1f, L.RoofY * 0.5f, L.TowerBackZ - 1.1f), colSize, art.Concrete);

            // Ground-Level Street Colonnade (Fluted Granite Piers along South Facade)
            Transform colonnade = Group("StreetColonnade", facade);
            for (float cx = -14f; cx <= 14f; cx += 7f)
            {
                if (Mathf.Abs(cx) < 3.5f) continue; // Clear main entrance portal
                Box($"GranitePier_{cx:0}", colonnade, new Vector3(cx, 3.0f, L.TowerFrontZ - 0.2f),
                    new Vector3(1.2f, 6.0f, 0.8f), art.GraniteDark);
            }

            // 4. Grand Entrance Porte-Cochère Canopy (z: -17 to -10)
            Transform canopy = Group("PorteCochere", facade);
            Box("CanopyRoofSlab", canopy, new Vector3(0f, 4.6f, -13.5f), new Vector3(14.5f, 0.35f, 7.2f), art.Steel);
            Box("GlassSkylight", canopy, new Vector3(0f, 4.5f, -13.5f), new Vector3(13.8f, 0.05f, 6.5f), art.Glass, false);
            Box("CanopyFascia", canopy, new Vector3(0f, 4.9f, -17.15f), new Vector3(14.6f, 0.6f, 0.2f), art.GraniteDark);
            Box("FasciaBronzeLetters", canopy, new Vector3(0f, 4.9f, -17.26f), new Vector3(10.5f, 0.3f, 0.04f), art.Brass, false);

            // Cantilever support outriggers
            Box("CanopyPillarL", canopy, new Vector3(-6.8f, 2.25f, -16.9f), new Vector3(0.5f, 4.5f, 0.5f), art.Steel);
            Box("CanopyPillarR", canopy, new Vector3( 6.8f, 2.25f, -16.9f), new Vector3(0.5f, 4.5f, 0.5f), art.Steel);

            // Suspended bronze entrance downlight pendants
            for (float lx = -4.5f; lx <= 4.5f; lx += 4.5f)
            {
                Transform pendant = Group($"CanopyPendant_{lx:0}", canopy);
                pendant.position = new Vector3(lx, 4.4f, -13.5f);
                Primitive(PrimitiveType.Cylinder, "PendantStem", pendant, new Vector3(0f, 0.2f, 0f), new Vector3(0.06f, 0.4f, 0.06f), art.Brass, false);
                Primitive(PrimitiveType.Cylinder, "Lantern", pendant, new Vector3(0f, -0.1f, 0f), new Vector3(0.4f, 0.2f, 0.4f), art.LampEmissive, false);
            }

            // 5. Exterior Curtain Glass Enclosure
            Box("FacadeGlass_East", facade, new Vector3( halfW, L.RoofY * 0.5f, midZ), new Vector3(0.2f, L.RoofY, depth - 2.2f), art.Glass);
            Box("FacadeGlass_West", facade, new Vector3(-halfW, L.RoofY * 0.5f, midZ), new Vector3(0.2f, L.RoofY, depth - 2.2f), art.Glass);
            Box("FacadeGlass_North", facade, new Vector3(0f, L.RoofY * 0.5f, L.TowerBackZ), new Vector3(halfW * 2f - 2.2f, L.RoofY, 0.2f), art.Glass);
            Box("FacadeGlass_SouthUpper", facade, new Vector3(0f, (L.RoofY + L.LobbyHeight) * 0.5f, L.TowerFrontZ),
                new Vector3(halfW * 2f - 2.2f, L.RoofY - L.LobbyHeight, 0.2f), art.Glass);

            // 6. Vertical Architectural Mullions (Towering Visual Rhythm)
            Transform mullions = Group("VerticalMullions", facade);
            // East and West facade mullions
            for (float mz = L.TowerFrontZ + 3.5f; mz <= L.TowerBackZ - 3.5f; mz += 4.5f)
            {
                Box($"MullionW_{mz:0}", mullions, new Vector3(-halfW - 0.15f, L.RoofY * 0.5f, mz), new Vector3(0.3f, L.RoofY, 0.35f), art.GraniteDark, false);
                Box($"MullionE_{mz:0}", mullions, new Vector3( halfW + 0.15f, L.RoofY * 0.5f, mz), new Vector3(0.3f, L.RoofY, 0.35f), art.GraniteDark, false);
            }
            // North facade mullions
            for (float mx = -halfW + 4.5f; mx <= halfW - 4.5f; mx += 4.5f)
            {
                Box($"MullionN_{mx:0}", mullions, new Vector3(mx, L.RoofY * 0.5f, L.TowerBackZ + 0.15f), new Vector3(0.35f, L.RoofY, 0.3f), art.GraniteDark, false);
            }

            // 7. Horizontal Floor Spandrel Bands (Expressed Structural Floor Slabs)
            Transform spandrels = Group("FloorSpandrels", facade);
            float[] spandrelYs = { 6.0f, 12.0f, 20.0f, 32.0f, 44.0f };
            foreach (float sy in spandrelYs)
            {
                Box($"BandN_{sy:0}", spandrels, new Vector3(0f, sy - 0.2f, L.TowerBackZ + 0.1f), new Vector3(halfW * 2f, 0.6f, 0.25f), art.Steel, false);
                Box($"BandS_{sy:0}", spandrels, new Vector3(0f, sy - 0.2f, L.TowerFrontZ - 0.1f), new Vector3(halfW * 2f, 0.6f, 0.25f), art.Steel, false);
                Box($"BandE_{sy:0}", spandrels, new Vector3( halfW + 0.1f, sy - 0.2f, midZ), new Vector3(0.25f, 0.6f, depth), art.Steel, false);
                Box($"BandW_{sy:0}", spandrels, new Vector3(-halfW - 0.1f, sy - 0.2f, midZ), new Vector3(0.25f, 0.6f, depth), art.Steel, false);
            }

            // 8. Rooftop Crown & Mechanical Parapet Wall (y = 50m)
            Transform crown = Group("RoofCrown", facade);
            Box("RoofSlab", crown, new Vector3(0f, L.RoofY, midZ), new Vector3(halfW * 2f, 0.5f, depth), art.Concrete);
            Box("ParapetN", crown, new Vector3(0f, L.RoofY + 0.8f, L.TowerBackZ), new Vector3(halfW * 2f, 1.2f, 0.4f), art.Steel);
            Box("ParapetS", crown, new Vector3(0f, L.RoofY + 0.8f, L.TowerFrontZ), new Vector3(halfW * 2f, 1.2f, 0.4f), art.Steel);
            Box("ParapetE", crown, new Vector3( halfW, L.RoofY + 0.8f, midZ), new Vector3(0.4f, 1.2f, depth), art.Steel);
            Box("ParapetW", crown, new Vector3(-halfW, L.RoofY + 0.8f, midZ), new Vector3(0.4f, 1.2f, depth), art.Steel);

            return cargo;
        }
    }
}

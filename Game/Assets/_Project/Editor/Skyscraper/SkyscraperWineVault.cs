using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the Sommelier Wine Vault & Reserve Cellar on Floor 2 (y = 44m):
    /// - Smoked glass vault entrance with heavy brass framing and digital keypad lock.
    /// - Floor-to-ceiling diagonal mahogany wine racks holding reserve vintage bottles.
    /// - Central sommelier tasting table with marble top, crystal decanter, and cellar ledger.
    /// - Climate-control refrigeration compressor with digital LED temperature readout.
    /// - Reinforced timber cradle chocks for the 340 kg jackpot Grand Vintage Cask.
    /// </summary>
    public static class SkyscraperWineVault
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform vault = Group("SommelierWineVault", root);
            float y = L.RestaurantY;
            vault.position = new Vector3(14.0f, y, -2.5f);

            // 1. Vault Glass Enclosure Walls & Brass Framework (x: 11.5 to 16.5, z: -6.5 to 1.5)
            Box("EnclosureGlassW", vault, new Vector3(-2.4f, 2.5f, 0f), new Vector3(0.1f, 5.0f, 7.8f), art.Glass);
            Box("EnclosureGlassS", vault, new Vector3(0f, 2.5f, -3.9f), new Vector3(4.8f, 5.0f, 0.1f), art.Glass);
            Box("BrassHeaderBeam", vault, new Vector3(-2.4f, 4.95f, 0f), new Vector3(0.2f, 0.15f, 7.8f), art.Brass);

            // Vault Pivot Glass Doorway (West side: z = 0f)
            Transform door = Group("VaultPivotDoor", vault);
            door.position = new Vector3(-2.4f, 0f, 0f);
            Box("DoorJamb", door, new Vector3(0f, 1.4f, 0f), new Vector3(0.25f, 2.8f, 1.6f), art.Brass);
            Box("GlassLeaf", door, new Vector3(0f, 1.35f, 0f), new Vector3(0.06f, 2.7f, 1.4f), art.Glass, false);
            Box("TubularHandle", door, new Vector3(-0.06f, 1.1f, 0.55f), new Vector3(0.04f, 0.6f, 0.04f), art.Brass, false);
            Box("KeypadLock", door, new Vector3(-0.08f, 1.25f, -0.6f), new Vector3(0.04f, 0.22f, 0.12f), art.GraniteDark, false);

            // 2. Floor-to-Ceiling Diagonal Mahogany Wine Racks (East and North Walls)
            Transform eastRacks = Group("EastLatticeRacks", vault);
            eastRacks.position = new Vector3(2.2f, 0f, 0f);
            Box("EastRackBacking", eastRacks, new Vector3(0f, 2.2f, 0f), new Vector3(0.35f, 4.4f, 7.4f), art.Mahogany);
            for (float rz = -3.2f; rz <= 3.2f; rz += 0.8f)
            {
                Box($"Divider_{rz:0.0}", eastRacks, new Vector3(-0.18f, 2.2f, rz), new Vector3(0.35f, 4.2f, 0.04f), art.Mahogany, false);
            }
            for (float ry = 0.6f; ry <= 3.8f; ry += 0.55f)
            {
                Box($"Shelf_{ry:0.0}", eastRacks, new Vector3(-0.18f, ry, 0f), new Vector3(0.35f, 0.04f, 7.2f), art.Brass, false);
            }

            // North Wall Horizontal Reserve Bottle Bin
            Transform northRacks = Group("NorthReserveBin", vault);
            northRacks.position = new Vector3(0f, 0f, 3.6f);
            Box("NorthRackBacking", northRacks, new Vector3(0f, 2.2f, 0f), new Vector3(4.4f, 4.4f, 0.35f), art.Mahogany);
            for (float ny = 0.8f; ny <= 3.6f; ny += 0.7f)
            {
                Box($"NorthTier_{ny:0.0}", northRacks, new Vector3(0f, ny, -0.18f), new Vector3(4.2f, 0.05f, 0.35f), art.Brass, false);
            }

            // 3. Central Sommelier Tasting Table
            Transform table = Group("TastingTable", vault);
            table.position = new Vector3(-0.2f, 0f, 0f);
            Box("TableTop", table, new Vector3(0f, 0.85f, 0f), new Vector3(1.6f, 0.08f, 2.6f), art.Marble);
            Box("TableBase", table, new Vector3(0f, 0.42f, 0f), new Vector3(1.3f, 0.84f, 2.3f), art.Mahogany);

            // Tasting Accessories on Table
            Box("DecanterCradle", table, new Vector3(0.3f, 0.95f, 0.4f), new Vector3(0.2f, 0.12f, 0.3f), art.Brass, false);
            Box("CellarInventoryLedger", table, new Vector3(-0.3f, 0.91f, -0.4f), new Vector3(0.35f, 0.04f, 0.45f), art.Leather, false);
            Box("SilverTastevin", table, new Vector3(0.3f, 0.91f, -0.4f), new Vector3(0.12f, 0.03f, 0.12f), art.Steel, false);

            // 4. Climate-Control Refrigeration Compressor
            Transform hvac = Group("CellarChillerUnit", vault);
            hvac.position = new Vector3(-1.8f, 0f, -3.2f);
            Box("CompressorHousing", hvac, new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 0.65f), art.Steel);
            Box("LouverGrill", hvac, new Vector3(0f, 0.6f, 0.34f), new Vector3(0.6f, 1.0f, 0.02f), art.DiamondPlate, false);
            Box("LedTempDisplay", hvac, new Vector3(0f, 1.05f, 0.35f), new Vector3(0.18f, 0.08f, 0.02f), art.RedEmergencyEmissive, false);

            // 5. Heavy Timber Cradle Chocks for the Grand Vintage Cask (Jackpot 1)
            Transform caskChocks = Group("GrandCaskChocks", vault);
            caskChocks.position = new Vector3(-0.2f, 0f, 2.2f);
            Box("ChockLeft", caskChocks, new Vector3(-0.6f, 0.15f, 0f), new Vector3(0.25f, 0.3f, 1.6f), art.Mahogany);
            Box("ChockRight", caskChocks, new Vector3( 0.6f, 0.15f, 0f), new Vector3(0.25f, 0.3f, 1.6f), art.Mahogany);
        }
    }
}

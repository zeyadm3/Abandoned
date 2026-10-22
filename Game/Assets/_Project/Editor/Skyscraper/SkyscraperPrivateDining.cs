using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the Penthouse Executive VIP Dining Suite on Floor 2 (y = 44m): long boardroom banquet table,
    /// high-back leather chairs, climate-controlled wine wall, and the executive wall safe.
    /// </summary>
    public static class SkyscraperPrivateDining
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform suite = Group("PenthouseExecutiveSuite", root);
            float y = L.RestaurantY;

            // Partition Wall separating VIP Suite from Main Dining Room (at x = 6m, z: 4 to 21m)
            Box("VIPWall_West", suite, new Vector3(6f, y + 2.5f, 12f), new Vector3(0.25f, 5.0f, 16f), art.WallPlaster);

            // Double Entry Doorway (x = 6m, z = 7m)
            Transform entry = Group("VIPEntryDoorway", suite);
            entry.position = new Vector3(6f, y, 7f);
            Box("DoorJamb", entry, new Vector3(0f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 2.2f), art.Mahogany);
            Box("DoorPanelL", entry, new Vector3(0f, 1.35f, -0.5f), new Vector3(0.08f, 2.7f, 1.0f), art.Mahogany);
            Box("DoorPanelR", entry, new Vector3(0f, 1.35f,  0.5f), new Vector3(0.08f, 2.7f, 1.0f), art.Mahogany);
            Box("BrassHandles", entry, new Vector3(-0.06f, 1.1f, 0f), new Vector3(0.04f, 0.4f, 0.2f), art.Brass, false);

            // 1. Executive Boardroom / Banquet Table
            Transform table = Group("ExecutiveTable", suite);
            table.position = new Vector3(11f, y, 13f);
            Box("TableTop", table, new Vector3(0f, 0.78f, 0f), new Vector3(3.2f, 0.1f, 7.5f), art.Mahogany);
            Box("TableInlay", table, new Vector3(0f, 0.84f, 0f), new Vector3(2.8f, 0.02f, 7.1f), art.Brass);
            Box("TablePedestal1", table, new Vector3(0f, 0.38f, -2.4f), new Vector3(1.2f, 0.76f, 1.2f), art.Mahogany);
            Box("TablePedestal2", table, new Vector3(0f, 0.38f,  2.4f), new Vector3(1.2f, 0.76f, 1.2f), art.Mahogany);

            // 8 High-Back Leather Executive Armchairs
            for (int r = 0; r < 4; r++)
            {
                float z = -2.4f + r * 1.6f;
                // West side chair
                Transform chairW = Group($"Chair_W_{r}", table);
                chairW.localPosition = new Vector3(-1.9f, 0f, z);
                chairW.localRotation = Quaternion.Euler(0f, 90f, 0f);
                BuildChair(chairW, art);

                // East side chair
                Transform chairE = Group($"Chair_E_{r}", table);
                chairE.localPosition = new Vector3( 1.9f, 0f, z);
                chairE.localRotation = Quaternion.Euler(0f, -90f, 0f);
                BuildChair(chairE, art);
            }

            // Head of Table Chair
            Transform chairHead = Group("Chair_Head", table);
            chairHead.localPosition = new Vector3(0f, 0f, 4.2f);
            chairHead.localRotation = Quaternion.Euler(0f, 180f, 0f);
            BuildChair(chairHead, art);

            // 2. Climate-Controlled Glass Wine Display Wall (East wall, x = 15.5m)
            Transform wineWall = Group("WineDisplayWall", suite);
            wineWall.position = new Vector3(15.5f, y, 13f);
            Box("WallBacking", wineWall, new Vector3(0f, 2.2f, 0f), new Vector3(0.4f, 4.4f, 8f), art.Mahogany);
            Box("GlassFront", wineWall, new Vector3(-0.45f, 2.2f, 0f), new Vector3(0.05f, 4.2f, 7.8f), art.Glass, false);
            for (float sy = 0.8f; sy <= 3.8f; sy += 0.6f)
                Box($"WineRackTier_{sy:0.0}", wineWall, new Vector3(-0.2f, sy, 0f), new Vector3(0.35f, 0.05f, 7.6f), art.Brass, false);

            // 3. Executive Credenza & Wall Safe Alcove (South wall, z = 4.2m)
            Transform credenza = Group("ExecutiveCredenza", suite);
            credenza.position = new Vector3(11f, y, 4.5f);
            Box("CredenzaBase", credenza, new Vector3(0f, 0.5f, 0f), new Vector3(4.5f, 1.0f, 0.8f), art.Mahogany);
            Box("CredenzaTop", credenza, new Vector3(0f, 1.05f, 0f), new Vector3(4.7f, 0.08f, 0.9f), art.Marble);

            // Recessed Safe Frame in the wall above credenza
            Box("SafeAlcove", suite, new Vector3(11f, y + 2.2f, 4.2f), new Vector3(1.4f, 1.4f, 0.4f), art.Steel);
        }

        private static void BuildChair(Transform parent, SkyscraperArt art)
        {
            Box("Seat", parent, new Vector3(0f, 0.48f, 0f), new Vector3(0.55f, 0.12f, 0.55f), art.Leather);
            Box("HighBack", parent, new Vector3(0f, 0.95f, -0.22f), new Vector3(0.55f, 0.85f, 0.10f), art.Leather);
            Box("ArmL", parent, new Vector3(-0.28f, 0.65f, 0.05f), new Vector3(0.08f, 0.25f, 0.45f), art.Mahogany);
            Box("ArmR", parent, new Vector3( 0.28f, 0.65f, 0.05f), new Vector3(0.08f, 0.25f, 0.45f), art.Mahogany);
            Box("Pedestal", parent, new Vector3(0f, 0.22f, 0f), new Vector3(0.1f, 0.44f, 0.1f), art.Brass, false);
            Box("StarBase", parent, new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.08f, 0.5f), art.Brass, false);
        }
    }
}

using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds opulent high-density amenities within Cloudline Tower's grand marble lobby:
    /// - Grand Art-Deco 4-car elevator bank with decorative pediments, analog floor dials, and barricaded out-of-order cars.
    /// - Cloudline Cafe & Espresso Bar with curved granite counter, commercial machine, pastry case, and bar stools.
    /// - Polished brass birdcage luggage trolleys with stacked vintage leather suitcases and valet key podium.
    /// - Security supervisor monitoring desk with multi-screen CRT CCTV monitors, emergency phone, and PA microphone.
    /// - Perimeter architectural wall sconces providing warm ambient upward illumination.
    /// </summary>
    public static class SkyscraperLobbyDetails
    {
        public static void Build(Transform lobby, SkyscraperArt art)
        {
            Transform detailsGroup = Group("LobbyDetails", lobby);

            // 1. Art-Deco 4-Car Elevator Bank Portal (North Wall: z = -2.25m, x: -6m to +6m)
            Transform bank = Group("ElevatorBankPortal", detailsGroup);
            bank.position = new Vector3(0f, 0f, -2.25f);

            // Massive marble and brass wall frieze framing all 4 elevator portals
            Box("PortalFriezeMarble", bank, new Vector3(0f, 2.0f, 0.05f), new Vector3(14.0f, 4.0f, 0.2f), art.Marble);
            Box("PortalCorniceBrass", bank, new Vector3(0f, 4.05f, 0f), new Vector3(14.2f, 0.2f, 0.35f), art.Brass);

            float[] carX = { -4.5f, -1.5f, 1.5f, 4.5f };
            string[] carNames = { "Car1_Service", "Car2_PassengerActive", "Car3_PassengerJammed", "Car4_ExecutiveExpress" };

            for (int i = 0; i < carX.Length; i++)
            {
                float x = carX[i];
                Transform carPortal = Group(carNames[i], bank);
                carPortal.position = new Vector3(x, 0f, 0f);

                // Polished brass door surround frame
                Box("FrameL", carPortal, new Vector3(-1.15f, 1.5f, 0f), new Vector3(0.18f, 3.0f, 0.15f), art.Brass);
                Box("FrameR", carPortal, new Vector3( 1.15f, 1.5f, 0f), new Vector3(0.18f, 3.0f, 0.15f), art.Brass);
                Box("FrameTop", carPortal, new Vector3(0f, 3.05f, 0f), new Vector3(2.48f, 0.18f, 0.15f), art.Brass);

                // Art-Deco Analog Semicircular Floor Dial Indicator above each door
                Transform dial = Group("FloorDial", carPortal);
                dial.position = new Vector3(0f, 3.4f, -0.05f);
                Primitive(PrimitiveType.Cylinder, "DialFace", dial, Vector3.zero, new Vector3(0.7f, 0.02f, 0.35f), art.Brass, false)
                    .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Box("DialNeedle", dial, new Vector3(0f, 0.08f, -0.02f), new Vector3(0.04f, 0.25f, 0.01f), art.RedEmergencyEmissive, false);

                // Specific state for each car
                if (i == 0) // Service Car 1: Out of order yellow crossbar
                {
                    Box("DoorL", carPortal, new Vector3(-0.5f, 1.45f, 0.02f), new Vector3(1.0f, 2.9f, 0.04f), art.Steel);
                    Box("DoorR", carPortal, new Vector3( 0.5f, 1.45f, 0.02f), new Vector3(1.0f, 2.9f, 0.04f), art.Steel);
                    Box("YellowCrossbar", carPortal, new Vector3(0f, 1.3f, -0.08f), new Vector3(2.4f, 0.18f, 0.04f), art.RoadYellow);
                }
                else if (i == 1) // Passenger Car 2: The functional car (gates handled by SkyscraperElevatorBuilder)
                {
                    Box("CallStationPediment", carPortal, new Vector3(1.4f, 1.35f, -0.08f), new Vector3(0.18f, 0.45f, 0.04f), art.Brass);
                    Primitive(PrimitiveType.Sphere, "ButtonUp", carPortal, new Vector3(1.4f, 1.45f, -0.11f), Vector3.one * 0.06f, art.LampEmissive, false);
                    Primitive(PrimitiveType.Sphere, "ButtonDown", carPortal, new Vector3(1.4f, 1.25f, -0.11f), Vector3.one * 0.06f, art.LampEmissive, false);
                }
                else if (i == 2) // Passenger Car 3: Jammed slightly open revealing void
                {
                    Box("DoorL", carPortal, new Vector3(-0.7f, 1.45f, 0.02f), new Vector3(0.9f, 2.9f, 0.04f), art.Steel);
                    Box("DoorR", carPortal, new Vector3( 0.7f, 1.45f, 0.02f), new Vector3(0.9f, 2.9f, 0.04f), art.Steel);
                    Box("VoidShadow", carPortal, new Vector3(0f, 1.45f, 0.15f), new Vector3(0.4f, 2.9f, 0.1f), art.GraniteDark, false);
                }
                else // Executive Express: Boarded up with wood planks
                {
                    Box("DoorL", carPortal, new Vector3(-0.5f, 1.45f, 0.02f), new Vector3(1.0f, 2.9f, 0.04f), art.Steel);
                    Box("DoorR", carPortal, new Vector3( 0.5f, 1.45f, 0.02f), new Vector3(1.0f, 2.9f, 0.04f), art.Steel);
                    GameObject plank1 = Box("Board1", carPortal, new Vector3(0f, 1.1f, -0.06f), new Vector3(2.3f, 0.22f, 0.04f), art.Parquet);
                    plank1.transform.localRotation = Quaternion.Euler(0f, 0f, 8f);
                    GameObject plank2 = Box("Board2", carPortal, new Vector3(0f, 1.7f, -0.06f), new Vector3(2.3f, 0.22f, 0.04f), art.Parquet);
                    plank2.transform.localRotation = Quaternion.Euler(0f, 0f, -8f);
                }
            }

            // 2. Cloudline Gourmet Cafe & Espresso Bar (West Wing: x = -13.5m, z = -6.5m)
            Transform cafe = Group("LobbyCafe", detailsGroup);
            cafe.position = new Vector3(-13.5f, 0f, -6.5f);

            // L-Shaped Granite Counter
            Box("CounterMain", cafe, new Vector3(0f, 0.55f, 0f), new Vector3(4.2f, 1.1f, 0.9f), art.GraniteDark);
            Box("CounterTop", cafe, new Vector3(0f, 1.15f, 0f), new Vector3(4.4f, 0.08f, 1.1f), art.Marble);
            Box("CounterReturn", cafe, new Vector3(1.8f, 0.55f, 1.0f), new Vector3(0.8f, 1.1f, 1.8f), art.GraniteDark);
            Box("FootrailBrass", cafe, new Vector3(0f, 0.2f, -0.55f), new Vector3(4.2f, 0.06f, 0.06f), art.Brass, false);

            // Commercial Dual-Boiler Espresso Machine
            Transform espresso = Group("CounterEspressoUnit", cafe);
            espresso.position = new Vector3(-0.8f, 1.19f, 0f);
            Box("MachineBody", espresso, new Vector3(0f, 0.25f, 0f), new Vector3(0.85f, 0.5f, 0.6f), art.Steel);
            Box("CupWarmer", espresso, new Vector3(0f, 0.52f, 0f), new Vector3(0.8f, 0.04f, 0.55f), art.Brass, false);

            // Glass-Front Pastry Display Case
            Transform pastry = Group("PastryCase", cafe);
            pastry.position = new Vector3(0.8f, 1.19f, 0f);
            Box("CaseGlass", pastry, new Vector3(0f, 0.25f, 0f), new Vector3(1.1f, 0.5f, 0.6f), art.Glass);
            Box("InternalShelf", pastry, new Vector3(0f, 0.25f, 0f), new Vector3(1.05f, 0.02f, 0.55f), art.Brass, false);

            // Wall-Mounted Chalkboard Menu
            Box("ChalkboardMenu", cafe, new Vector3(-2.15f, 2.0f, 0f), new Vector3(0.04f, 1.4f, 1.6f), art.GraniteDark, false);

            // High-Top Bar Stools
            for (float sx = -1.2f; sx <= 1.2f; sx += 1.2f)
            {
                Transform stool = Group($"CafeStool_{sx:0.0}", cafe);
                stool.position = cafe.position + new Vector3(sx, 0f, -1.0f);
                Primitive(PrimitiveType.Cylinder, "Cushion", stool, new Vector3(0f, 0.75f, 0f), new Vector3(0.38f, 0.1f, 0.38f), art.Leather);
                Primitive(PrimitiveType.Cylinder, "Pedestal", stool, new Vector3(0f, 0.36f, 0f), new Vector3(0.08f, 0.72f, 0.08f), art.Steel);
            }

            // 3. Bellhop & Valet Luggage Trolleys (x = 2.5m, z = -7.5m)
            Transform luggage = Group("ValetLuggageStation", detailsGroup);
            luggage.position = new Vector3(2.5f, 0f, -7.5f);

            for (int t = 0; t < 2; t++)
            {
                Transform trolley = Group($"BrassLuggageTrolley_{t}", luggage);
                trolley.position = luggage.position + new Vector3(t * 1.5f, 0f, 0f);

                // Base platform with burgundy carpet
                Box("TrolleyPlatform", trolley, new Vector3(0f, 0.15f, 0f), new Vector3(1.4f, 0.08f, 0.75f), art.CarPaintBurgundy);
                Box("TrolleyBumper", trolley, new Vector3(0f, 0.15f, 0f), new Vector3(1.46f, 0.06f, 0.81f), art.Rubber, false);

                // Birdcage Brass Tubular Arch Frame
                Primitive(PrimitiveType.Cylinder, "PostL", trolley, new Vector3(-0.65f, 0.95f, 0f), new Vector3(0.05f, 0.8f, 0.05f), art.Brass, false);
                Primitive(PrimitiveType.Cylinder, "PostR", trolley, new Vector3( 0.65f, 0.95f, 0f), new Vector3(0.05f, 0.8f, 0.05f), art.Brass, false);
                Box("TopHangingBar", trolley, new Vector3(0f, 1.75f, 0f), new Vector3(1.35f, 0.05f, 0.05f), art.Brass, false);

                // Stacked Vintage Leather Suitcases
                Box("SuitcaseBottom", trolley, new Vector3(0f, 0.32f, 0f), new Vector3(0.85f, 0.24f, 0.5f), art.Leather);
                Box("SuitcaseTop", trolley, new Vector3(0.05f, 0.52f, 0f), new Vector3(0.7f, 0.18f, 0.42f), art.Leather);
            }

            // Valet Key Podium
            Transform valet = Group("ValetKeyPodium", luggage);
            valet.position = luggage.position + new Vector3(-1.4f, 0f, 0f);
            Box("PodiumBody", valet, new Vector3(0f, 0.55f, 0f), new Vector3(0.55f, 1.1f, 0.45f), art.Mahogany);
            Box("KeyCabinet", valet, new Vector3(0f, 1.0f, -0.1f), new Vector3(0.48f, 0.3f, 0.1f), art.Brass, false);

            // 4. Security Supervisor Monitoring Desk (x = 7.5m, z = -5.8m)
            Transform secDesk = Group("SecuritySupervisorConsole", detailsGroup);
            secDesk.position = new Vector3(7.5f, 0f, -5.8f);
            Box("ConsoleDesk", secDesk, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 0.9f), art.Steel);

            // CRT CCTV Surveillance Monitor Bank (4 Monitors in 2x2 grid)
            Transform monitorBank = Group("CctvMonitorBank", secDesk);
            monitorBank.position = new Vector3(0f, 0.92f, 0.1f);
            for (int mx = 0; mx < 2; mx++)
            for (int my = 0; my < 2; my++)
            {
                Transform mon = Group($"Cctv_{mx}_{my}", monitorBank);
                mon.localPosition = new Vector3(-0.45f + mx * 0.9f, 0.22f + my * 0.42f, 0f);
                Box("CrtCase", mon, Vector3.zero, new Vector3(0.42f, 0.36f, 0.38f), art.Steel);
                Box("CrtScreen", mon, new Vector3(0f, 0f, -0.2f), new Vector3(0.36f, 0.3f, 0.02f), art.Glass, false);
            }

            // Emergency Red Telephone & Gooseneck PA Microphone
            Box("EmergencyPhone", secDesk, new Vector3(-0.8f, 0.95f, -0.25f), new Vector3(0.2f, 0.12f, 0.22f), art.CarPaintBurgundy, false);
            Primitive(PrimitiveType.Cylinder, "PaMic", secDesk, new Vector3(0.8f, 1.05f, -0.25f), new Vector3(0.04f, 0.22f, 0.04f), art.Brass, false);

            // 5. Architectural Wall Sconces
            Vector3[] sconcePositions = {
                new(-L.TowerHalfWidth + 0.3f, 3.2f, -8f), new(-L.TowerHalfWidth + 0.3f, 3.2f, -4f),
                new( L.TowerHalfWidth - 0.3f, 3.2f, -8f), new( L.TowerHalfWidth - 0.3f, 3.2f, -4f)
            };
            for (int s = 0; s < sconcePositions.Length; s++)
            {
                Transform sconce = Group($"WallSconce_{s}", detailsGroup);
                sconce.position = sconcePositions[s];
                Box("Bracket", sconce, Vector3.zero, new Vector3(0.12f, 0.35f, 0.18f), art.Brass);
                Primitive(PrimitiveType.Sphere, "Diffuser", sconce, new Vector3(0f, 0.15f, 0f), Vector3.one * 0.2f, art.LampEmissive, false);
            }
        }
    }
}

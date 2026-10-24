using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Skyscraper;
using Unity.Netcode;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the 404-metre vertical elevator shaft, motorized hoist, cab with MovingPlatform,
    /// landing call stations and interlocked passenger doors at 0m, 200m and 404m.
    /// </summary>
    public static class SkyscraperElevatorBuilder
    {
        public static SkyscraperElevator Build(Transform root, SkyscraperArt art)
        {
            Transform shaftGroup = Group("ElevatorShaft", root);

            // 1. Shaft Structural Walls & Steel Guide Rails
            float shaftH = L.RestaurantY + 4f;
            float midY = shaftH * 0.5f;
            Box("ShaftWall_North", shaftGroup, new Vector3(0f, midY, 2.3f), new Vector3(4.8f, shaftH, 0.2f), art.Concrete);
            Box("ShaftWall_West", shaftGroup, new Vector3(-2.3f, midY, 0f), new Vector3(0.2f, shaftH, 4.8f), art.Concrete);
            Box("ShaftWall_East", shaftGroup, new Vector3( 2.3f, midY, 0f), new Vector3(0.2f, shaftH, 4.8f), art.Concrete);

            float previousTop = 0f;
            foreach (float level in new[] { L.GroundY, L.MechanicalY, L.RestaurantY })
            {
                if (level > previousTop)
                    Box("ShaftSouthSpandrel", shaftGroup, new Vector3(0f,(previousTop+level)/2f,-2.3f),new Vector3(4.8f,level-previousTop,.2f),art.Concrete);
                previousTop = level + 3f;
                Box("LandingSill", shaftGroup, new Vector3(0f,level-.175f,-2.05f),new Vector3(2.2f,.35f,.72f),art.LiftSteel);
            }
            Box("ShaftSouthTop",shaftGroup,new Vector3(0f,L.RestaurantY+3.5f,-2.3f),new Vector3(4.8f,1f,.2f),art.Concrete);

            // Vertical steel guide rails for the cab
            Box("GuideRail_L", shaftGroup, new Vector3(-2.15f, midY, 0f), new Vector3(0.12f, shaftH, 0.2f), art.Steel);
            Box("GuideRail_R", shaftGroup, new Vector3( 2.15f, midY, 0f), new Vector3(0.12f, shaftH, 0.2f), art.Steel);

            // Counterweight frame riding the north wall
            Box("Counterweight", shaftGroup, new Vector3(0f, L.RestaurantY * .5f, 2.05f), new Vector3(2.4f, 4.5f, 0.35f), art.Steel);

            // 2. Shaft Head Machine Hoist & Cable Drums (y = 248m)
            Transform hoist = Group("ShaftHoist", shaftGroup);
            hoist.position = new Vector3(0f, L.RestaurantY + 4.5f, 0f);
            Box("MotorBase", hoist, Vector3.zero, new Vector3(3.2f, 0.8f, 2.2f), art.Steel);
            Primitive(PrimitiveType.Cylinder, "CableDrum", hoist, new Vector3(0f, 0.8f, 0f), new Vector3(1.4f, 1.2f, 1.4f), art.Steel)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // 3. The Elevator Cab (at y = 0 initially)
            var cabRoot = new GameObject("ElevatorCab");
            cabRoot.transform.SetParent(shaftGroup, false);
            cabRoot.transform.position = new Vector3(0f, L.GroundY, 0f);

            // Floor deck with MovingPlatform and BoxCollider
            Transform deckGo = Group("CabDeck", cabRoot.transform);
            BoxCollider deckCol = deckGo.gameObject.AddComponent<BoxCollider>();
            deckCol.center = new Vector3(0f, -0.175f, 0f);
            deckCol.size = new Vector3(L.CabDeckSize.x,.35f,L.CabDeckSize.z);
            Box("Visual", deckGo, deckCol.center, deckCol.size, art.LiftSteel, false);
            deckGo.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MovingPlatform platform = deckGo.gameObject.AddComponent<MovingPlatform>();

            // Enclosed Cab Body (y: 0 to 2.8m)
            Transform cabBody = Group("CabBody", deckGo);
            float cabH = 2.8f, cabHalf = cabH * 0.5f;
            Box("CabWall_L", cabBody, new Vector3(-1.65f, cabHalf, 0f), new Vector3(0.1f, cabH, 3.4f), art.LiftSteel);
            Box("CabWall_R", cabBody, new Vector3( 1.65f, cabHalf, 0f), new Vector3(0.1f, cabH, 3.4f), art.LiftSteel);
            Box("CabWall_Back", cabBody, new Vector3(0f, cabHalf, 1.65f), new Vector3(3.4f, cabH, 0.1f), art.LiftSteel);
            Box("CabRoof", cabBody, new Vector3(0f, cabH + 0.05f, 0f), new Vector3(3.4f, 0.1f, 3.4f), art.LiftSteel);
            SkyscraperCabInterior.Build(cabBody,art);
            var cabDoor=SkyscraperCabInterior.Door(deckGo,art,"CabPassengerDoors",new Vector3(0,0,-1.58f),true);
            foreach(float x in new[]{-1.38f,1.38f})
                Box("CabFrontReturn",cabBody,new Vector3(x,1.4f,-1.62f),new Vector3(.45f,2.8f,.1f),art.LiftSteel);

            // 4. Interior Car Destination Button Panel (Inside the cab)
            Transform carPanel = Group("CarButtonPanel", deckGo);
            carPanel.localPosition = new Vector3(1.5f, 1.2f, -0.6f);
            Box("PanelPlate", carPanel, Vector3.zero, new Vector3(0.06f, 0.9f, 0.35f), art.Brass);

            // 5. Landing Gates & Exterior Call Stations for the 3 Floors
            float[] floorYs = { L.GroundY, L.MechanicalY, L.RestaurantY };
            var gates = new GameObject[floorYs.Length];

            for (int f = 0; f < floorYs.Length; f++)
            {
                float y = floorYs[f];
                Transform landing = Group($"Landing_Floor{f}", shaftGroup);
                landing.position = new Vector3(0f, y, -2.3f);

                // Permanent side infill keeps every shaft landing sealed outside the gate aperture.
                foreach (float side in new[] { -1f, 1f })
                    Box("ShaftDoorSideWall",shaftGroup,new Vector3(side*1.95f,y+1.5f,-2.3f),new Vector3(.9f,3f,.2f),art.Concrete);
                // Landing Door Frame & Sliding Gate Panels
                Box("DoorFrameL", landing, new Vector3(-1.3f, 1.4f, 0f), new Vector3(0.4f, 2.8f, 0.2f), art.LiftSteel);
                Box("DoorFrameR", landing, new Vector3( 1.3f, 1.4f, 0f), new Vector3(0.4f, 2.8f, 0.2f), art.LiftSteel);
                Box("DoorHeader", landing, new Vector3(0f, 2.7f, 0f), new Vector3(3.0f, 0.4f, 0.2f), art.LiftSteel);

                // Gate object that is disabled (open) when the cab is parked here
                var gate=SkyscraperCabInterior.Door(landing,art,$"Gate_Floor{f}",new Vector3(0,0,.05f),f==0);
                gates[f]=gate.gameObject;
                Label(landing,"LandingDestination",f==0?"LOBBY":f==1?"MECHANICAL":"APEX RESTAURANT",new Vector3(0,3.18f,-.15f),Quaternion.identity,.032f,new Color(.88f,.8f,.61f));

                // Exterior Call Station Button
                Transform callStation = Group($"CallStation_Floor{f}", landing);
                callStation.position = new Vector3(1.75f, y + 1.2f, -2.55f);
                callStation.rotation = Quaternion.Euler(0f,180f,0f);
                Box("CallPlate", callStation, Vector3.zero, new Vector3(0.2f, 0.35f, 0.06f), art.Brass);
                GameObject btnObj = Box("Button", callStation, new Vector3(0f, 0f, 0.04f), new Vector3(0.1f, 0.1f, 0.04f), art.LampEmissive);
                var callBtn = btnObj.AddComponent<SkyscraperElevatorButton>();
                callBtn.EditorSetup(null, f, true);
            }

            // Interior car buttons inside the cab
            var interiorButtons = new SkyscraperElevatorButton[3];
            float[] btnOffsets = { -0.25f, 0f, 0.25f };
            for (int b = 0; b < 3; b++)
            {
                GameObject carBtn = Box($"CarButton_{b}", carPanel, new Vector3(-0.04f, btnOffsets[b], 0f),
                    new Vector3(0.04f, 0.14f, 0.14f), art.LampEmissive);
                var btn = carBtn.AddComponent<SkyscraperElevatorButton>();
                btn.EditorSetup(null, b, false);
                interiorButtons[b] = btn;
                Label(carPanel,"CarButtonLegend",b==0?"L":b==1?"M":"A",new Vector3(-.085f,btnOffsets[b],.16f),Quaternion.Euler(0,90,0),.024f,Color.white);
            }

            // 6. Assemble the SkyscraperElevator NetworkBehaviour
            cabRoot.AddComponent<NetworkObject>();
            // MovingPlatform carries the complete car; none of its children may be statically batched.
            foreach (Transform child in deckGo.GetComponentsInChildren<Transform>(true))
                UnityEditor.GameObjectUtility.SetStaticEditorFlags(child.gameObject, 0);
            var elevator = cabRoot.AddComponent<SkyscraperElevator>();
            elevator.EditorSetup(platform, floorYs, gates, new Vector2(3.4f, 3.4f), 8.0f, 450f, 1);
            elevator.EditorSetCabDoor(cabDoor);
            Label(cabBody,"CabFloorDisplay","L / LOBBY",new Vector3(0,2.28f,-1.66f),Quaternion.Euler(0,180,0),.022f,new Color(.85f,.82f,.68f));
            Transform display=cabBody.Find("CabFloorDisplay");
            display.gameObject.AddComponent<SkyscraperElevatorDisplay>().EditorSetup(elevator,display.GetComponent<TextMesh>());

            // Connect button references back to the elevator component
            foreach (SkyscraperElevatorButton call in shaftGroup.GetComponentsInChildren<SkyscraperElevatorButton>(true))
            {
                int floor = (int)typeof(SkyscraperElevatorButton).GetField("targetFloor",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(call);
                bool isCall = (bool)typeof(SkyscraperElevatorButton).GetField("isCallStation",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(call);
                call.EditorSetup(elevator, floor, isCall);
            }

            return elevator;
        }
    }
}

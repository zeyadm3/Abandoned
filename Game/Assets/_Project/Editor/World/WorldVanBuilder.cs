using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.World;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the drivable van prefab (Prefabs/World/WorldVan): a host-simulated NetworkObject on four WheelColliders,
    /// the collection truck's body (cab with three seats, cargo box with a bench and a slide-out ramp), headlights,
    /// and registers it with NGO. Its tunables live in Data/World/Resources/VanConfig.
    /// </summary>
    public static class WorldVanBuilder
    {
        public const string PrefabFolder = "Assets/_Project/Prefabs/World";
        public const string PrefabPath = PrefabFolder + "/WorldVan.prefab";
        public const string ConfigFolder = "Assets/_Project/Data/World/Resources";
        public const string ConfigPath = ConfigFolder + "/VanConfig.asset";

        private const float DeckTop = 1.05f, RampRun = 4.4f, BayRear = -2.5f;

        [MenuItem(ToolMenu.OpenWorld + "Build Van", priority = ToolMenu.OpenWorldP + 20)]
        public static void BuildMenu() => Build();

        public static GameObject Build()
        {
            EnsureConfig();
            PolishAssets.EnsureFolder(PrefabFolder);
            var root = new GameObject("WorldVan");
            var networkObject = root.AddComponent<NetworkObject>();
            networkObject.DontDestroyWithOwner = true;
            var nt = root.AddComponent<NetworkTransform>();
            nt.AuthorityMode = NetworkTransform.AuthorityModes.Server;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            nt.InLocalSpace = false;
            nt.Interpolate = true;
            var body = root.AddComponent<Rigidbody>();
            body.mass = 3600f;
            body.linearDamping = .05f;
            body.angularDamping = .3f;

            Transform[] wheelArt = WorldVanArt.Build(root.transform);
            WheelCollider[] wheels = WheelColliders(root.transform);
            Colliders(root.transform);
            (Transform left, Transform right) = WorldVanArt.Doors(root.transform);
            var tailgate = Tailgate(root.transform, left, right);
            Transform[] seats = Seats(root.transform, out Transform[] exits);
            Light[] lights = Lights(root.transform);

            root.AddComponent<VanCondition>();
            var drive = root.AddComponent<VanDrive>();
            drive.EditorSetup(body, wheels, wheelArt, new Vector3(0f, .95f, 1.1f));
            var van = root.AddComponent<WorldVan>();
            var vanSeats = root.AddComponent<VanSeats>();
            vanSeats.EditorSetup(van, seats, exits);
            van.EditorSetup(vanSeats, tailgate, lights, Cargo(root.transform, seats[VanSeats.DriverSeat]));
            root.AddComponent<VanSound>();
            root.AddComponent<VanCargoLock>();
            root.AddComponent<VanDebugView>();
            // The open world's shared state rides on the van: one per world, spawned with it.
            root.AddComponent<WorldState>();
            (NetworkObject[] hazards, NetworkObject monster, GameObject scare) = RoadEventBuilder.Build();
            root.AddComponent<RoadEventDirector>().EditorSetup(van, hazards, monster, scare);
            root.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            Register(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"[WorldVan] Built {PrefabPath}.");
            return prefab;
        }

        private static void EnsureConfig()
        {
            PolishAssets.EnsureFolder(ConfigFolder);
            if (AssetDatabase.LoadAssetAtPath<VanConfig>(ConfigPath) != null) return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<VanConfig>(), ConfigPath);
        }

        private static void Register(GameObject prefab)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkContentBuilder.PrefabListPath);
            if (list == null || list.Contains(prefab)) return;
            list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);
        }

        private static WheelCollider[] WheelColliders(Transform van)
        {
            var wheels = new WheelCollider[4];
            for (int i = 0; i < 4; i++)
            {
                Transform t = GreyboxFactory.Group($"WheelCollider_{i}", van);
                // The collider sits half its travel above the hub, so the wheel rests on its drawn hub.
                t.localPosition = WorldVanArt.Hubs[i] + Vector3.up * .14f;
                var w = t.gameObject.AddComponent<WheelCollider>();
                w.radius = .48f;
                w.mass = 70f;
                w.suspensionDistance = .28f;
                w.forceAppPointDistance = .2f;
                w.suspensionSpring = new JointSpring { spring = 95000f, damper = 9500f, targetPosition = .5f };
                w.forwardFriction = new WheelFrictionCurve { extremumSlip = .4f, extremumValue = 1f, asymptoteSlip = .8f, asymptoteValue = .5f, stiffness = 1.6f };
                w.sidewaysFriction = new WheelFrictionCurve { extremumSlip = .25f, extremumValue = 1f, asymptoteSlip = .5f, asymptoteValue = .75f, stiffness = 1.8f };
                wheels[i] = w;
            }
            return wheels;
        }

        // Simple boxes for the body: the cab, the chassis, and the cargo box (floor, walls, roof) the crew walks into.
        private static void Colliders(Transform van)
        {
            Transform group = GreyboxFactory.Group("Colliders", van);
            Solid(group, "Chassis", new Vector3(0f, .80f, 1.0f), new Vector3(1.7f, .32f, 7.1f));
            Solid(group, "Cab", new Vector3(0f, 1.95f, 4.05f), new Vector3(2.46f, 1.9f, 2.4f));
            Solid(group, "BayFloor", new Vector3(0f, .985f, 0f), new Vector3(2.6f, .13f, 5f));
            Solid(group, "BayWall_L", new Vector3(-1.35f, 2.15f, 0f), new Vector3(.1f, 2.2f, 5f));
            Solid(group, "BayWall_R", new Vector3(1.35f, 2.15f, 0f), new Vector3(.1f, 2.2f, 5f));
            Solid(group, "BayWall_Front", new Vector3(0f, 2.15f, 2.55f), new Vector3(2.8f, 2.2f, .1f));
            Solid(group, "BayRoof", new Vector3(0f, 3.35f, 0f), new Vector3(2.82f, .10f, 5.22f));
        }

        private static BoxCollider Solid(Transform parent, string name, Vector3 centre, Vector3 size)
        {
            Transform t = GreyboxFactory.Group(name, parent);
            t.localPosition = centre;
            var box = t.gameObject.AddComponent<BoxCollider>();
            box.size = size;
            t.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("The van's detailed body collides as simple boxes.");
            return box;
        }

        // The cargo box as the job's truck: loot in the bay counts, anyone in the cab or the box rides home, and the
        // driver's seat is the lever (the horn).
        private static TruckCargo Cargo(Transform van, Transform driver)
        {
            Transform bayT = GreyboxFactory.Group("CargoBay", van);
            var bay = bayT.gameObject.AddComponent<BoxCollider>();
            bay.isTrigger = true;
            bay.center = new Vector3(0f, 2.15f, 0f);
            bay.size = new Vector3(2.6f, 2.2f, 5f);
            var cargo = van.gameObject.AddComponent<TruckCargo>();
            cargo.EditorSetup(bay, new Bounds(new Vector3(0f, 2f, 1.4f), new Vector3(3f, 3.6f, 8.4f)), driver);
            return cargo;
        }

        private static VanTailgate Tailgate(Transform van, Transform left, Transform right)
        {
            float length = Mathf.Sqrt(RampRun * RampRun + DeckTop * DeckTop), angle = Mathf.Atan2(DeckTop, RampRun) * Mathf.Rad2Deg;
            var ramp = GreyboxFactory.Box("Ramp", van, Vector3.zero, new Vector3(2.5f, .08f, length),
                AssetDatabase.LoadAssetAtPath<Material>(CollectionTruckGeometry.Folder + "/Materials/Truck_Dark.mat"));
            Vector3 outPos = new(0f, DeckTop * .5f - .04f, BayRear - RampRun * .5f);
            Vector3 outEuler = new(-angle, 0f, 0f);
            // Stowed: flat under the cargo deck, between the frame rails and the floor.
            Vector3 inPos = new(0f, .55f, BayRear + length * .5f - .3f);
            Transform panelT = GreyboxFactory.Group("RearPanel", van);
            panelT.localPosition = new Vector3(0f, 2.15f, -2.58f);
            var panel = panelT.gameObject.AddComponent<BoxCollider>();
            panel.size = new Vector3(2.7f, 2.2f, .08f);
            panelT.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("The shut rear doors keep the cargo in while the van moves.");
            var gate = van.gameObject.AddComponent<VanTailgate>();
            gate.EditorSetup(ramp.transform, outPos, outEuler, inPos, Vector3.zero, left, right, panel);
            ramp.transform.localPosition = outPos;
            ramp.transform.localEulerAngles = outEuler;
            return gate;
        }

        // Seat eye points (0 driver on the left, 1 passenger, 2 middle, 3 the bench in the cargo box) and the doors.
        private static Transform[] Seats(Transform van, out Transform[] exits)
        {
            Vector3[] eyes = { new(-.63f, 2.14f, 3.28f), new(.63f, 2.14f, 3.28f), new(0f, 2.14f, 3.28f), new(0f, 2.30f, 2.15f) };
            Vector3[] outs = { new(-2.1f, 0f, 3.3f), new(2.1f, 0f, 3.3f), new(-2.1f, 0f, 2.2f), new(0f, 0f, BayRear - RampRun - 1.2f) };
            var seats = new Transform[4];
            exits = new Transform[4];
            Transform group = GreyboxFactory.Group("Seats", van);
            for (int i = 0; i < 4; i++)
            {
                seats[i] = GreyboxFactory.Group($"Seat_{i}", group);
                seats[i].localPosition = eyes[i];
                exits[i] = GreyboxFactory.Group($"Exit_{i}", group);
                exits[i].localPosition = outs[i];
            }
            Door(group, "Door_Left", new Vector3(-1.25f, 1.75f, 3.3f), new Vector3(.08f, 1.3f, 1.05f), 0, 2);
            Door(group, "Door_Right", new Vector3(1.25f, 1.75f, 3.3f), new Vector3(.08f, 1.3f, 1.05f), 1, 2);
            Door(group, "Bench", new Vector3(0f, 1.32f, 2.2f), new Vector3(2.3f, .45f, .5f), 3);
            var bench = GreyboxFactory.Box("BenchSeat", group, new Vector3(0f, 1.32f, 2.2f), new Vector3(2.3f, .45f, .5f),
                AssetDatabase.LoadAssetAtPath<Material>(CollectionTruckGeometry.Folder + "/Materials/Truck_Seat.mat"), false);
            bench.name = "BenchSeatArt";
            return seats;
        }

        private static void Door(Transform parent, string name, Vector3 centre, Vector3 size, params int[] seats)
        {
            BoxCollider box = Solid(parent, name, centre, size);
            box.gameObject.AddComponent<VanSeat>().EditorSetup(seats);
        }

        private static Light[] Lights(Transform van)
        {
            var lights = new Light[4];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                Transform head = GreyboxFactory.Group(i == 0 ? "Headlight_L" : "Headlight_R", van);
                head.localPosition = new Vector3(side * .88f, 1.18f, 5.3f);
                head.localRotation = Quaternion.Euler(6f, 0f, 0f);
                Light l = head.gameObject.AddComponent<Light>();
                l.type = LightType.Spot;
                l.range = 45f;
                l.spotAngle = 70f;
                l.innerSpotAngle = 35f;
                l.intensity = 30f;
                l.color = new Color(1f, .93f, .8f);
                l.shadows = LightShadows.None;
                l.renderingLayerMask = -1;
                lights[i] = l;

                Transform tail = GreyboxFactory.Group(i == 0 ? "TailLight_L" : "TailLight_R", van);
                tail.localPosition = new Vector3(side * 1.25f, .9f, -2.75f);
                Light t = tail.gameObject.AddComponent<Light>();
                t.type = LightType.Point;
                t.range = 4f;
                t.intensity = 2f;
                t.color = new Color(1f, .1f, .05f);
                t.shadows = LightShadows.None;
                t.renderingLayerMask = -1;
                lights[2 + i] = t;
            }
            return lights;
        }
    }
}

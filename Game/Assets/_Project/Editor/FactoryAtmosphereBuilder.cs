using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Presentation-only dressing for the Factory contract. The structural layout, routes, loot sites,
    /// encounters, and hazards stay owned by SalvageLocationBuilder; this pass makes that layout read as
    /// the abandoned Cinderline Foundry during a cold night shift.
    /// </summary>
    public static class FactoryAtmosphereBuilder
    {
        private const string SealPath = "Assets/_Project/Art/Generated/FactoryAtmosphere/FactoryWorksSeal.png";
        private const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public static void Apply(Transform root)
        {
            if (root == null) return;

            Transform pass = Group("FactoryAtmospherePass", root);
            // The entire pass is visual-only. This protects the tested approaches to the Factory's loot,
            // threats, stairs, extraction bay, and all existing systemic hazards.
            pass.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            var art = new Palette();
            Transform exterior = Group("FoundryExterior", pass);
            Transform interiors = Group("IndustrialInteriors", pass);
            Transform wear = Group("IndustrialWear", pass);
            Transform signals = Group("PowerAwareSignals", pass);

            BuildExterior(exterior, art);
            BuildInterior(interiors, root, art);
            BuildWear(wear, art);
            BuildSignals(signals, art);

            // LightFixture controls the live fixtures at runtime. Everything else can be static so the
            // deliberately large machine-room silhouette remains inexpensive to render.
            MarkStatic(exterior);
            MarkStatic(interiors);
            MarkStatic(wear);
        }

        private sealed class Palette
        {
            public readonly Material Concrete;
            public readonly Material Iron;
            public readonly Material Rust;
            public readonly Material Charcoal;
            public readonly Material SafetyYellow;
            public readonly Material SafetyOrange;
            public readonly Material Cable;
            public readonly Material Seal;
            public readonly Material SmokedGlass;
            public readonly Material Oil;
            public readonly Material AmberGlow;
            public readonly Material RedGlow;
            public readonly Material CyanGlow;

            public Palette()
            {
                Texture2D seal = AssetDatabase.LoadAssetAtPath<Texture2D>(SealPath);
                if (seal == null)
                {
                    AssetDatabase.ImportAsset(SealPath, ImportAssetOptions.ForceSynchronousImport);
                    seal = AssetDatabase.LoadAssetAtPath<Texture2D>(SealPath);
                }

                Concrete = PolishAssets.Material("Factory_SootConcrete", new Color(.18f, .185f, .16f), .95f,
                    PolishAssets.Texture("Factory_SootConcreteTexture", tiles: true));
                Iron = PolishAssets.Material("Factory_WetIron", new Color(.14f, .16f, .15f), .52f);
                Rust = PolishAssets.Material("Factory_Oxide", new Color(.31f, .135f, .055f), .76f);
                Charcoal = PolishAssets.Material("Factory_Charcoal", new Color(.035f, .040f, .035f), .58f);
                SafetyYellow = PolishAssets.Material("Factory_SafetyYellow", new Color(.66f, .46f, .075f), .46f);
                SafetyOrange = PolishAssets.Material("Factory_SafetyOrange", new Color(.66f, .20f, .045f), .48f);
                Cable = PolishAssets.Material("Factory_Cable", new Color(.018f, .021f, .017f), .38f);
                Seal = PolishAssets.Material("Factory_CinderlineSeal", Color.white, .72f, seal);
                SmokedGlass = PolishAssets.Material("Factory_SmokedGlass", new Color(.07f, .115f, .11f, .40f), .14f);
                ConfigureTransparent(SmokedGlass);
                Oil = PolishAssets.Material("Factory_OilFilm", new Color(.028f, .042f, .032f), .20f);
                AmberGlow = PolishAssets.Material("Factory_AmberGlow", new Color(.86f, .36f, .055f), .31f, emission: 1.65f);
                RedGlow = PolishAssets.Material("Factory_RedGlow", new Color(.72f, .035f, .012f), .34f, emission: 1.6f);
                CyanGlow = PolishAssets.Material("Factory_CyanGlow", new Color(.08f, .34f, .38f), .32f, emission: 1.4f);
            }
        }

        private static void ConfigureTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
        }

        private static void BuildExterior(Transform exterior, Palette a)
        {
            // This header hangs above the permanently clear cargo entry. Its split side pylons deliberately
            // leave the truck and four player spawns with a clean, immediately-readable approach.
            Box("FoundryHeader", exterior, new Vector3(24f, 6.12f, -.15f), new Vector3(20.8f, .48f, .10f), a.Charcoal, false);
            Box("FoundryHeaderRule", exterior, new Vector3(24f, 5.82f, -.17f), new Vector3(21.0f, .07f, .07f), a.SafetyOrange, false);
            Box("FoundrySeal", exterior, new Vector3(24f, 4.77f, -.18f), new Vector3(2.82f, 2.82f, .07f), a.Seal, false);
            Box("FoundryNameplate", exterior, new Vector3(24f, 3.25f, -.18f), new Vector3(12.4f, .56f, .07f), a.Iron, false);
            Label(exterior, "FoundryName", "CINDERLINE FOUNDRY", new Vector3(24f, 3.25f, -.225f),
                Quaternion.Euler(0f, 180f, 0f), .145f, new Color(.92f, .44f, .14f));
            Label(exterior, "FoundryLine", "NIGHT SHIFT  //  RECEIVING & MILLWORK", new Vector3(24f, 2.79f, -.225f),
                Quaternion.Euler(0f, 180f, 0f), .073f, new Color(.58f, .61f, .51f));
            BuildEntryPylon(exterior, "WestEntryPylon", new Vector3(13.0f, 0f, -.42f), a);
            BuildEntryPylon(exterior, "EastEntryPylon", new Vector3(35.0f, 0f, -.42f), a);

            // North-yard silhouettes create the enormous industrial scale requested for the Factory without
            // opening new exterior play space or placing props in the extraction lane.
            BuildStack(exterior, "StackA", new Vector3(5.5f, 0f, 48.6f), 12.8f, .86f, a);
            BuildStack(exterior, "StackB", new Vector3(12.2f, 0f, 49.6f), 9.9f, .66f, a);
            BuildStack(exterior, "StackC", new Vector3(19.3f, 0f, 48.3f), 15.0f, 1.02f, a);
            BuildCondenser(exterior, "NorthCondenser", new Vector3(39.0f, 0f, 46.9f), a);
            BuildPipeRack(exterior, "NorthPipeRack", new Vector3(28.2f, 0f, 47.7f), 17.6f, a);
            BuildFenceSilhouette(exterior, "NorthYardFence", -2f, 50f, 51.6f, a);
            BuildFenceSide(exterior, "WestYardFence", -2.1f, 42f, 51.6f, a);
            BuildFenceSide(exterior, "EastYardFence", 50.1f, 42f, 51.6f, a);

            Box("WestServiceTower", exterior, new Vector3(-1.6f, 3.15f, 28.2f), new Vector3(2.35f, 6.3f, 4.7f), a.Charcoal, false);
            Box("EastServiceTower", exterior, new Vector3(49.6f, 3.7f, 31.0f), new Vector3(2.15f, 7.4f, 4.2f), a.Charcoal, false);
            Box("WestTowerCap", exterior, new Vector3(-1.6f, 6.42f, 28.2f), new Vector3(2.68f, .18f, 5.05f), a.Rust, false);
            Box("EastTowerCap", exterior, new Vector3(49.6f, 7.52f, 31.0f), new Vector3(2.48f, .18f, 4.55f), a.Rust, false);
        }

        private static void BuildEntryPylon(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform pylon = Group(name, parent);
            Box("Body", pylon, at + new Vector3(0f, 2.3f, 0f), new Vector3(1.46f, 4.6f, .82f), a.Charcoal, false);
            Box("Band", pylon, at + new Vector3(0f, 1.08f, -.43f), new Vector3(1.58f, .26f, .06f), a.SafetyYellow, false);
            Box("WarningPanel", pylon, at + new Vector3(0f, 3.28f, -.43f), new Vector3(.72f, .80f, .05f), a.SafetyOrange, false);
        }

        private static void BuildStack(Transform parent, string name, Vector3 at, float height, float width, Palette a)
        {
            Transform stack = Group(name, parent);
            Box("Shaft", stack, at + new Vector3(0f, height * .5f, 0f), new Vector3(width, height, width), a.Charcoal, false);
            Box("RustBand", stack, at + new Vector3(0f, height * .42f, 0f), new Vector3(width * 1.12f, .28f, width * 1.12f), a.Rust, false);
            Box("Cap", stack, at + new Vector3(0f, height + .10f, 0f), new Vector3(width * 1.26f, .20f, width * 1.26f), a.Iron, false);
            PolishAssets.Shape("SmokeSilhouette", stack, at + new Vector3(0f, height + 1.6f, 0f),
                new Vector3(width * 2.8f, 3.4f, width * 2.8f), a.Charcoal, round: true);
        }

        private static void BuildCondenser(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform condenser = Group(name, parent);
            Box("Housing", condenser, at + new Vector3(0f, 2.3f, 0f), new Vector3(7.8f, 4.6f, 3.8f), a.Iron, false);
            for (int i = 0; i < 5; i++)
            {
                float x = at.x - 2.75f + i * 1.38f;
                PolishAssets.Shape("Fan_" + i, condenser, new Vector3(x, 4.72f, at.z), new Vector3(.98f, .18f, .98f),
                    a.Charcoal, round: true);
            }
            Box("ServiceBand", condenser, at + new Vector3(0f, 1.08f, -1.95f), new Vector3(7.95f, .24f, .06f), a.Rust, false);
        }

        private static void BuildPipeRack(Transform parent, string name, Vector3 at, float length, Palette a)
        {
            Transform rack = Group(name, parent);
            for (int i = 0; i < 5; i++)
            {
                float x = at.x - length * .5f + .8f + i * (length - 1.6f) / 4f;
                Box("Upright_" + i, rack, new Vector3(x, 2.0f, at.z), new Vector3(.14f, 4f, .14f), a.Iron, false);
            }
            PipeRun(rack, "UpperPipe", at + new Vector3(-length * .5f, 3.24f, 0f), at + new Vector3(length * .5f, 3.24f, 0f), .27f, a.Rust);
            PipeRun(rack, "LowerPipe", at + new Vector3(-length * .5f, 2.56f, 0f), at + new Vector3(length * .5f, 2.56f, 0f), .18f, a.Iron);
        }

        private static void BuildFenceSilhouette(Transform parent, string name, float from, float to, float z, Palette a)
        {
            Transform fence = Group(name, parent);
            for (float x = from; x <= to + .01f; x += 3.2f)
                Box("Post", fence, new Vector3(x, 1.35f, z), new Vector3(.08f, 2.7f, .08f), a.Iron, false);
            Box("TopRail", fence, new Vector3((from + to) * .5f, 2.58f, z), new Vector3(to - from, .06f, .06f), a.Rust, false);
            Box("MidRail", fence, new Vector3((from + to) * .5f, 1.42f, z), new Vector3(to - from, .05f, .05f), a.Iron, false);
        }

        private static void BuildFenceSide(Transform parent, string name, float x, float from, float to, Palette a)
        {
            Transform fence = Group(name, parent);
            for (float z = from; z <= to + .01f; z += 3.2f)
                Box("Post", fence, new Vector3(x, 1.35f, z), new Vector3(.08f, 2.7f, .08f), a.Iron, false);
            Box("TopRail", fence, new Vector3(x, 2.58f, (from + to) * .5f), new Vector3(.06f, .06f, to - from), a.Rust, false);
            Box("MidRail", fence, new Vector3(x, 1.42f, (from + to) * .5f), new Vector3(.05f, .05f, to - from), a.Iron, false);
        }

        private static void BuildInterior(Transform interior, Transform root, Palette a)
        {
            BuildMachineHall(interior, a);
            BuildReceivingDock(interior, a);
            BuildBoilerRoom(interior, a);
            BuildUpperRooms(interior, root, a);
            BuildCatwalkDetails(interior, root, a);
        }

        private static void BuildMachineHall(Transform parent, Palette a)
        {
            Transform hall = Group("MachineHall", parent);

            // An overhead travelling crane gives the empty void a huge, dangerous vertical profile. It is a
            // mesh-only silhouette above the existing machines, never a new climbable or blocking structure.
            Transform crane = Group("OverheadGantry", hall);
            Box("WestRunway", crane, new Vector3(16f, 6.15f, 10.55f), new Vector3(27.2f, .22f, .28f), a.Iron, false);
            Box("EastRunway", crane, new Vector3(16f, 6.15f, 25.45f), new Vector3(27.2f, .22f, .28f), a.Iron, false);
            Box("Bridge", crane, new Vector3(16f, 6.43f, 18.0f), new Vector3(.34f, .30f, 15.7f), a.Rust, false);
            Box("BridgeBeam", crane, new Vector3(16f, 6.72f, 18.0f), new Vector3(.66f, .22f, 16.2f), a.Iron, false);
            PipeRun(crane, "HookChain", new Vector3(16f, 6.38f, 18f), new Vector3(16f, 3.72f, 18f), .075f, a.Cable);
            PolishAssets.Shape("Hook", crane, new Vector3(16f, 3.49f, 18f), new Vector3(.45f, .62f, .26f), a.SafetyYellow, round: true);
            Label(crane, "GantryLabel", "BAY A  //  18T", new Vector3(16f, 6.86f, 18f),
                Quaternion.Euler(0f, 90f, 0f), .082f, new Color(.78f, .39f, .12f));

            BuildTurbine(hall, "WestTurbineA", new Vector3(2.1f, 0f, 12.2f), a);
            BuildTurbine(hall, "WestTurbineB", new Vector3(2.1f, 0f, 25.5f), a);
            BuildTurbine(hall, "EastTurbineA", new Vector3(30.15f, 0f, 12.8f), a);
            BuildTurbine(hall, "EastTurbineB", new Vector3(30.15f, 0f, 22.8f), a);
            BuildCableTray(hall, "WestCableTray", new Vector3(1.45f, 3.55f, 18f), new Vector3(1.45f, 3.55f, 27.0f), a);
            BuildCableTray(hall, "EastCableTray", new Vector3(30.55f, 3.55f, 10.4f), new Vector3(30.55f, 3.55f, 26.2f), a);

            // Hazard paint emphasizes the working routes around the existing central conveyors without
            // pretending to introduce a new mechanic or obscuring a salvage pickup.
            BuildFloorLine(hall, "SouthMachineCaution", new Vector3(6.0f, .031f, 10.2f), new Vector3(10.0f, .025f, .16f), a.SafetyYellow);
            BuildFloorLine(hall, "NorthMachineCaution", new Vector3(25.4f, .031f, 25.8f), new Vector3(7.0f, .025f, .16f), a.SafetyYellow);
            BuildFloorLine(hall, "WestMachineCaution", new Vector3(5.35f, .031f, 20.0f), new Vector3(.16f, .025f, 8.8f), a.SafetyOrange);
            BuildFloorLine(hall, "EastMachineCaution", new Vector3(28.7f, .031f, 11.5f), new Vector3(.16f, .025f, 4.5f), a.SafetyOrange);
            Label(hall, "MillWallLabel", "MILL 01  //  LOCKOUT REQUIRED", new Vector3(.16f, 2.48f, 18.0f),
                Quaternion.Euler(0f, 90f, 0f), .09f, new Color(.72f, .42f, .15f));
        }

        private static void BuildTurbine(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform turbine = Group(name, parent);
            Box("Base", turbine, at + new Vector3(0f, .36f, 0f), new Vector3(2.0f, .72f, 1.75f), a.Charcoal, false);
            PolishAssets.Shape("Housing", turbine, at + new Vector3(0f, 1.44f, 0f), new Vector3(1.86f, 1.92f, 1.86f), a.Iron, round: true);
            Box("ServiceBand", turbine, at + new Vector3(0f, 1.44f, -1.02f), new Vector3(1.34f, .20f, .06f), a.Rust, false);
            Box("WarningPanel", turbine, at + new Vector3(0f, 2.15f, -1.03f), new Vector3(.44f, .48f, .05f), a.SafetyOrange, false);
            PipeRun(turbine, "FeedPipe", at + new Vector3(0f, 2.22f, .72f), at + new Vector3(0f, 3.48f, .72f), .20f, a.Iron);
        }

        private static void BuildCableTray(Transform parent, string name, Vector3 from, Vector3 to, Palette a)
        {
            Transform tray = Group(name, parent);
            PipeRun(tray, "CableRun", from, to, .12f, a.Cable);
            Vector3 delta = to - from;
            int rungs = Mathf.Max(2, Mathf.RoundToInt(delta.magnitude / 2.1f));
            for (int i = 0; i <= rungs; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, i / (float)rungs);
                Box("Rung_" + i, tray, at, new Vector3(.72f, .06f, .06f), a.Iron, false);
            }
        }

        private static void BuildFloorLine(Transform parent, string name, Vector3 at, Vector3 size, Material material)
        {
            Box(name, parent, at, size, material, false);
        }

        private static void BuildReceivingDock(Transform parent, Palette a)
        {
            Transform dock = Group("ReceivingDock", parent);
            BuildRollerDoor(dock, "EastDockDoor", new Vector3(47.83f, 2.20f, 6.0f), false, a);
            BuildRollerDoor(dock, "NorthDockDoor", new Vector3(40.0f, 2.20f, 11.83f), true, a);
            Label(dock, "ReceivingLabel", "RECEIVING  //  BAY 02", new Vector3(46.75f, 3.18f, 2.25f),
                Quaternion.Euler(0f, -90f, 0f), .095f, new Color(.82f, .42f, .12f));
            BuildCargoCage(dock, "DockCargoCage", new Vector3(45.0f, 0f, 3.0f), a);
            BuildForkliftSilhouette(dock, new Vector3(43.3f, 0f, 8.0f), a);

            Box("DockSignalPanel", dock, new Vector3(32.16f, 2.08f, 2.4f), new Vector3(.05f, .78f, .44f), a.Charcoal, false);
            Box("DockSignalAmber", dock, new Vector3(32.10f, 2.30f, 2.4f), new Vector3(.045f, .16f, .16f), a.AmberGlow, false);
            Box("DockSignalRed", dock, new Vector3(32.10f, 1.92f, 2.4f), new Vector3(.045f, .16f, .16f), a.RedGlow, false);
            BuildFloorLine(dock, "BayLine", new Vector3(39.6f, .031f, 1.45f), new Vector3(13.4f, .025f, .14f), a.SafetyYellow);
        }

        private static void BuildRollerDoor(Transform parent, string name, Vector3 at, bool horizontal, Palette a)
        {
            Transform door = Group(name, parent);
            Vector3 faceSize = horizontal ? new Vector3(5.0f, 4.4f, .05f) : new Vector3(.05f, 4.4f, 5.0f);
            Box("DoorFace", door, at, faceSize, a.Charcoal, false);
            for (int i = 0; i < 8; i++)
            {
                float y = at.y - 1.8f + i * .50f;
                Vector3 slat = horizontal
                    ? new Vector3(at.x, y, at.z - .035f)
                    : new Vector3(at.x - .035f, y, at.z);
                Vector3 size = horizontal ? new Vector3(4.82f, .055f, .025f) : new Vector3(.025f, .055f, 4.82f);
                Box("Slat_" + i, door, slat, size, a.Iron, false);
            }
            Box("Header", door, at + Vector3.up * 2.28f, horizontal ? new Vector3(5.3f, .18f, .16f) : new Vector3(.16f, .18f, 5.3f), a.Rust, false);
        }

        private static void BuildCargoCage(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform cage = Group(name, parent);
            Box("PalletBase", cage, at + new Vector3(0f, .18f, 0f), new Vector3(2.85f, .36f, 2.35f), a.Rust, false);
            for (int side = 0; side < 2; side++)
            for (int i = 0; i < 3; i++)
            {
                float x = at.x - 1.15f + i * 1.15f;
                float z = at.z + (side == 0 ? -1.0f : 1.0f);
                Box("WirePost_" + side + "_" + i, cage, new Vector3(x, 1.24f, z), new Vector3(.055f, 2.12f, .055f), a.Iron, false);
            }
            Box("BackFrame", cage, at + new Vector3(1.32f, 1.24f, 0f), new Vector3(.07f, 2.12f, 2.1f), a.Iron, false);
            Box("CautionPlate", cage, at + new Vector3(1.36f, 1.52f, 0f), new Vector3(.035f, .46f, .66f), a.SafetyYellow, false);
        }

        private static void BuildForkliftSilhouette(Transform parent, Vector3 at, Palette a)
        {
            Transform forklift = Group("ForkliftSilhouette", parent);
            Box("Chassis", forklift, at + new Vector3(0f, .42f, 0f), new Vector3(1.58f, .84f, 2.45f), a.Charcoal, false);
            Box("Cabin", forklift, at + new Vector3(0f, 1.16f, -.28f), new Vector3(1.32f, .72f, 1.14f), a.Iron, false);
            Box("Mast", forklift, at + new Vector3(0f, 1.55f, 1.30f), new Vector3(1.18f, 3.1f, .11f), a.Rust, false);
            Box("ForkLeft", forklift, at + new Vector3(-.42f, .18f, 2.18f), new Vector3(.10f, .10f, 1.75f), a.Iron, false);
            Box("ForkRight", forklift, at + new Vector3(.42f, .18f, 2.18f), new Vector3(.10f, .10f, 1.75f), a.Iron, false);
            for (int i = 0; i < 4; i++)
                PolishAssets.Shape("Wheel_" + i, forklift,
                    at + new Vector3(i < 2 ? -.78f : .78f, .26f, i % 2 == 0 ? -.72f : .72f),
                    new Vector3(.40f, .40f, .40f), a.Charcoal, round: true);
        }

        private static void BuildBoilerRoom(Transform parent, Palette a)
        {
            Transform boiler = Group("BoilerRoom", parent);
            // Keep pipe density on the walls and above the existing boiler line so the west-door route,
            // the east-side jackpot, and both active threat anchors remain visually and physically legible.
            PipeRun(boiler, "EastWallMain", new Vector3(47.62f, 3.05f, 25.0f), new Vector3(47.62f, 3.05f, 35.9f), .24f, a.Iron);
            PipeRun(boiler, "EastWallReturn", new Vector3(47.48f, 2.35f, 25.0f), new Vector3(47.48f, 2.35f, 35.9f), .14f, a.Rust);
            PipeRun(boiler, "NorthHeader", new Vector3(33.2f, 3.30f, 24.35f), new Vector3(46.4f, 3.30f, 24.35f), .30f, a.Iron);
            for (int i = 0; i < 4; i++)
            {
                float z = 25.7f + i * 2.52f;
                PipeRun(boiler, "EastRiser_" + i, new Vector3(47.54f, .35f, z), new Vector3(47.54f, 3.45f, z), .18f, a.Iron);
                BuildValve(boiler, "EastValve_" + i, new Vector3(47.25f, 2.02f, z), a);
            }
            for (int i = 0; i < 3; i++)
            {
                float x = 34.2f + i * 3.35f;
                Box("BoilerHeader_" + i, boiler, new Vector3(x, 3.22f, 29.48f), new Vector3(1.76f, .22f, .28f), a.Rust, false);
                BuildValve(boiler, "BoilerValve_" + i, new Vector3(x, 2.42f, 29.15f), a);
            }
            Box("PressureBoard", boiler, new Vector3(33.18f, 2.30f, 34.48f), new Vector3(.05f, 1.22f, 2.28f), a.Charcoal, false);
            for (int i = 0; i < 4; i++)
            {
                float z = 33.72f + i * .49f;
                Box("Gauge_" + i, boiler, new Vector3(33.11f, 2.55f, z), new Vector3(.035f, .23f, .23f),
                    i == 3 ? a.RedGlow : a.AmberGlow, false);
            }
            Label(boiler, "BoilerLabel", "BOILER LINE  //  HIGH PRESSURE", new Vector3(46.82f, 3.88f, 29.8f),
                Quaternion.Euler(0f, -90f, 0f), .08f, new Color(.74f, .38f, .10f));
        }

        private static void BuildValve(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform valve = Group(name, parent);
            PolishAssets.Shape("Wheel", valve, at, new Vector3(.46f, .46f, .13f), a.SafetyOrange, round: true);
            Box("Hub", valve, at + new Vector3(0f, 0f, -.10f), new Vector3(.12f, .12f, .10f), a.Iron, false);
        }

        private static void BuildUpperRooms(Transform parent, Transform root, Palette a)
        {
            Transform upper = Group("UpperOffices", parent);
            Transform admin = Group("AdminOffice", upper);
            // The active pickup at (6, 6) retains a wide central floor. Desks hug the back/side wall instead.
            BuildOfficeDesk(admin, "AdminDesk", new Vector3(2.1f, 4f, 1.8f), a);
            Box("AdminGlassWest", admin, new Vector3(.18f, 5.75f, 3.3f), new Vector3(.04f, 1.46f, 2.50f), a.SmokedGlass, false);
            Box("AdminGlassEast", admin, new Vector3(11.82f, 5.75f, 2.0f), new Vector3(.04f, 1.46f, 1.65f), a.SmokedGlass, false);
            Label(admin, "AdminOfficeLabel", "ADMIN  //  TIME OFFICE", new Vector3(1.22f, 6.35f, 4.0f),
                Quaternion.Euler(0f, 90f, 0f), .075f, new Color(.62f, .67f, .57f));

            Transform control = Group("ControlRoom", upper);
            // The front windows leave the middle doorway open; consoles stay on the east wall so they never
            // hide or alter the upper control-room pickup at (42, 6).
            Box("ControlGlassWest", control, new Vector3(38.2f, 5.72f, 7.82f), new Vector3(3.36f, 1.38f, .04f), a.SmokedGlass, false);
            Box("ControlGlassEast", control, new Vector3(45.8f, 5.72f, 7.82f), new Vector3(3.36f, 1.38f, .04f), a.SmokedGlass, false);
            BuildControlConsole(control, "PanelA", new Vector3(46.25f, 4f, 1.75f), a);
            BuildControlConsole(control, "PanelB", new Vector3(46.25f, 4f, 4.45f), a);
            Label(control, "ControlRoomLabel", "MILL CONTROL", new Vector3(42f, 6.46f, 7.78f),
                Quaternion.Euler(0f, 180f, 0f), .105f, new Color(.70f, .42f, .13f));

            Transform breakRoom = Group("NightShiftBreakRoom", upper);
            // This room hosts both an upper loot point and threat marker. Dressing stays hard against the
            // east/west walls, giving that encounter space a deliberately open centre.
            Box("BreakCounter", breakRoom, new Vector3(47.2f, 4.72f, 38.0f), new Vector3(1.2f, 1.44f, 2.7f), a.Iron, false);
            Box("BreakMachineGlow", breakRoom, new Vector3(46.56f, 5.2f, 38.0f), new Vector3(.04f, .46f, .72f), a.CyanGlow, false);
            Box("BreakTable", breakRoom, new Vector3(37.2f, 4.55f, 38.1f), new Vector3(1.8f, 1.1f, 1.4f), a.Charcoal, false);
            Label(breakRoom, "BreakRoomLabel", "NIGHT CREW  //  03:00", new Vector3(42f, 6.35f, 39.80f),
                Quaternion.Euler(0f, 180f, 0f), .09f, new Color(.62f, .61f, .48f));

            // A deliberately lightweight junction box is tied to the weak upper tile, so if the structure
            // simulation collapses that section later, its dressing follows rather than floating in the void.
            Transform junction = Group("CatwalkJunction", upper);
            Box("JunctionBox", junction, new Vector3(37.25f, 4.82f, 21.2f), new Vector3(.52f, .72f, .24f), a.Charcoal, false);
            Box("JunctionLamp", junction, new Vector3(37.25f, 4.82f, 21.06f), new Vector3(.23f, .18f, .04f), a.AmberGlow, false);
            AttachToUpperTile(root, junction, 9, 5);
        }

        private static void BuildOfficeDesk(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform desk = Group(name, parent);
            Box("DeskTop", desk, at + new Vector3(0f, .72f, 0f), new Vector3(2.3f, .14f, 1.05f), a.Iron, false);
            Box("Pedestal", desk, at + new Vector3(-.72f, .36f, 0f), new Vector3(.38f, .72f, .86f), a.Charcoal, false);
            Box("Monitor", desk, at + new Vector3(.46f, 1.13f, -.16f), new Vector3(.70f, .52f, .08f), a.CyanGlow, false);
            Box("Chair", desk, at + new Vector3(1.05f, .42f, .45f), new Vector3(.64f, .84f, .68f), a.Charcoal, false);
        }

        private static void BuildControlConsole(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform console = Group(name, parent);
            Box("Body", console, at + new Vector3(0f, .55f, 0f), new Vector3(1.12f, 1.1f, 1.82f), a.Charcoal, false);
            Box("Screen", console, at + new Vector3(-.60f, .92f, 0f), new Vector3(.05f, .46f, 1.32f), a.CyanGlow, false);
            for (int i = 0; i < 3; i++)
                Box("Button_" + i, console, at + new Vector3(-.64f, .48f, -.48f + i * .48f),
                    new Vector3(.04f, .12f, .12f), i == 2 ? a.RedGlow : a.AmberGlow, false);
        }

        private static void BuildCatwalkDetails(Transform parent, Transform root, Palette a)
        {
            Transform catwalk = Group("CatwalkDetail", parent);
            // The trays sit well above the walking surface and do not touch the two stair shafts. They add
            // a dense industrial ceiling line without stealing either catwalk's actual playable clearance.
            BuildCableTray(catwalk, "SouthTray", new Vector3(12.6f, 5.55f, 1.15f), new Vector3(34.4f, 5.55f, 1.15f), a);
            BuildCableTray(catwalk, "NorthTray", new Vector3(12.6f, 5.55f, 38.8f), new Vector3(34.4f, 5.55f, 38.8f), a);
            BuildCableTray(catwalk, "WestTray", new Vector3(1.15f, 5.55f, 9.6f), new Vector3(1.15f, 5.55f, 14.1f), a);
            BuildCableTray(catwalk, "EastTray", new Vector3(46.85f, 5.55f, 9.6f), new Vector3(46.85f, 5.55f, 14.1f), a);

            // Tile-local safety placards look anchored to the upper catwalk but remain mesh-only.
            Transform placard = Group("NorthCatwalkPlacard", catwalk);
            Box("Plate", placard, new Vector3(28.0f, 4.72f, 39.76f), new Vector3(2.0f, .72f, .04f), a.SafetyYellow, false);
            Label(placard, "PlateText", "CAUTION  //  OPEN MILL BELOW", new Vector3(28.0f, 4.72f, 39.72f),
                Quaternion.Euler(0f, 180f, 0f), .066f, new Color(.10f, .09f, .055f));
            AttachToUpperTile(root, placard, 7, 9);
        }

        private static void AttachToUpperTile(Transform root, Transform decor, int x, int z)
        {
            Transform tile = root.Find($"Levels/UpperFloor/Tile_1_{x}_{z}");
            if (tile == null) return;
            Abandoned.Structure.StructuralSection section = tile.GetComponent<Abandoned.Structure.StructuralSection>();
            if (section != null) decor.gameObject.AddComponent<Abandoned.Structure.SectionProp>().EditorSetup(section);
        }

        private static void BuildWear(Transform parent, Palette a)
        {
            // These oil, soot, and worn-paint cues are deliberately sparse at protected anchors. They sell
            // an unmaintained foundry while leaving the visual read of pickups, threats, and stairs clear.
            Vector3[] stains =
            {
                new(5.0f, .022f, 11.0f), new(23.4f, .022f, 25.7f), new(29.4f, .022f, 12.0f),
                new(45.7f, .022f, 4.0f), new(35.1f, .022f, 26.0f), new(4.1f, 4.022f, 2.4f),
            };
            for (int i = 0; i < stains.Length; i++)
                CustomMallArt.Place("stain", parent, stains[i], i * 47f, new Vector3(.98f, 1f, .98f));

            Vector3[] puddles =
            {
                new(3.4f, .018f, 27.4f), new(29.6f, .018f, 9.7f), new(46.2f, .018f, 11.1f),
                new(47.0f, .018f, 35.6f), new(8.6f, 4.018f, 1.5f),
            };
            for (int i = 0; i < puddles.Length; i++)
                CustomMallArt.Place("puddle", parent, puddles[i], i * 62f, new Vector3(.84f, 1f, .84f));

            CustomMallArt.Place("debris", parent, new Vector3(2.2f, .02f, 6.8f), 38f, new Vector3(.52f, .52f, .52f));
            CustomMallArt.Place("debris", parent, new Vector3(46.0f, .02f, 18.8f), 125f, new Vector3(.46f, .46f, .46f));
            CustomMallArt.Place("crack", parent, new Vector3(25.6f, .022f, 29.5f), 28f, new Vector3(1.16f, 1f, 1.16f));
            CustomMallArt.Place("rebar", parent, new Vector3(47.0f, .02f, 22.4f), 270f, new Vector3(.62f, .62f, .62f));
            for (int i = 0; i < 5; i++)
            {
                float x = 4.0f + i * 8.5f;
                Box("SootPaintFlake_" + i, parent, new Vector3(x, 1.42f + (i % 2) * .20f, 39.83f),
                    new Vector3(2.1f, .54f, .02f), i % 2 == 0 ? a.Concrete : a.Rust, false);
            }
        }

        private static void BuildSignals(Transform parent, Palette a)
        {
            AddSignal(parent, "ReceivingAlarm", new Vector3(32.30f, 2.72f, 6.2f), a, 901, a.RedGlow, new Color(1f, .055f, .02f), false);
            AddSignal(parent, "MillAmber", new Vector3(4.15f, 3.06f, 18.2f), a, 902, a.AmberGlow, new Color(1f, .41f, .08f), true);
            AddSignal(parent, "GantryAlarm", new Vector3(16f, 5.95f, 10.4f), a, 903, a.RedGlow, new Color(1f, .055f, .02f), false);
            AddSignal(parent, "BoilerAlarm", new Vector3(46.85f, 3.88f, 29.8f), a, 904, a.RedGlow, new Color(1f, .055f, .02f), true);
            AddSignal(parent, "ControlStatus", new Vector3(46.6f, 6.12f, 6.85f), a, 905, a.CyanGlow, new Color(.10f, .55f, .62f), false);
        }

        private static void AddSignal(Transform parent, string name, Vector3 at, Palette a, int seed,
            Material glowMaterial, Color lightColor, bool faulty)
        {
            Transform fixture = Group(name, parent);
            fixture.localPosition = at;
            GameObject glow = Box("Glow", fixture, Vector3.zero, new Vector3(.34f, .18f, .12f), glowMaterial, false);
            Light lamp = fixture.gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = lightColor;
            lamp.range = 4.2f;
            lamp.intensity = .42f;
            lamp.shadows = LightShadows.None;
            fixture.gameObject.AddComponent<Abandoned.Structure.LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(),
                faulty, false, seed, glowMaterial.GetColor("_EmissionColor"));
        }

        private static void PipeRun(Transform parent, string name, Vector3 from, Vector3 to, float width, Material material)
        {
            Transform run = Group(name, parent);
            Vector3 delta = to - from;
            run.localPosition = (from + to) * .5f;
            run.localRotation = delta.sqrMagnitude > .0001f
                ? Quaternion.FromToRotation(Vector3.forward, delta.normalized)
                : Quaternion.identity;
            Box("Pipe", run, Vector3.zero, new Vector3(width, width, delta.magnitude), material, false);
        }

        private static void Label(Transform parent, string name, string text, Vector3 at, Quaternion rotation,
            float characterSize, Color color)
        {
            var label = new GameObject(name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = at;
            label.transform.localRotation = rotation;
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = characterSize;
            label.fontSize = 48;
            label.color = color;
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>(FontPath));
        }
    }
}

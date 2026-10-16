using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// A deliberately grounded first visual pass for Blackwater Island Penitentiary.  Everything here is
    /// presentation-only: no colliders, new routes, loot, or encounter logic.  That keeps the authored
    /// prison layout readable while the facility gains its own weathered-island identity.
    /// </summary>
    public static class PrisonAtmosphereBuilder
    {
        private const string EmblemPath = "Assets/_Project/Art/Generated/PrisonAtmosphere/PrisonIslandEmblem.png";
        private const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public static void Apply(Transform root)
        {
            if (root == null) return;

            Transform pass = Group("PrisonAtmospherePass", root);
            // This is defensive as well as documentary: all props below are visual-only, and the pass must
            // never alter the navigation routes validated for the live Prison contract.
            pass.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            var art = new Palette();
            Transform island = Group("IslandExterior", pass);
            Transform institution = Group("InstitutionalDetail", pass);
            Transform decay = Group("SaltAndWaterDecay", pass);
            Transform emergencies = Group("EmergencyLighting", pass);

            BuildIslandExterior(island, art);
            BuildInstitution(institution, root, art);
            BuildDecay(decay, art);
            BuildEmergencyLighting(emergencies, art);

            // Keep the lights out of the static pass: LightFixture changes their illumination at runtime.
            MarkStatic(island);
            MarkStatic(institution);
            MarkStatic(decay);
        }

        private sealed class Palette
        {
            public readonly Material SaltConcrete;
            public readonly Material DampConcrete;
            public readonly Material Iron;
            public readonly Material Rust;
            public readonly Material Cable;
            public readonly Material Paint;
            public readonly Material Water;
            public readonly Material Crest;
            public readonly Material SmokedGlass;
            public readonly Material ColdGlow;
            public readonly Material RedGlow;

            public Palette()
            {
                Texture2D emblem = AssetDatabase.LoadAssetAtPath<Texture2D>(EmblemPath);
                if (emblem == null)
                {
                    AssetDatabase.ImportAsset(EmblemPath, ImportAssetOptions.ForceSynchronousImport);
                    emblem = AssetDatabase.LoadAssetAtPath<Texture2D>(EmblemPath);
                }

                SaltConcrete = PolishAssets.Material("Prison_SaltConcrete", new Color(.19f, .23f, .24f), .94f,
                    PolishAssets.Texture("Prison_SaltConcreteTexture"));
                DampConcrete = PolishAssets.Material("Prison_DampConcrete", new Color(.12f, .17f, .18f), .90f,
                    PolishAssets.Texture("Prison_DampConcreteTexture", tiles: true));
                Iron = PolishAssets.Material("Prison_WetIron", new Color(.12f, .17f, .18f), .57f);
                Rust = PolishAssets.Material("Prison_SaltRust", new Color(.26f, .13f, .08f), .76f);
                Cable = PolishAssets.Material("Prison_Cable", new Color(.025f, .032f, .032f), .42f);
                Paint = PolishAssets.Material("Prison_InstitutionalPaint", new Color(.34f, .38f, .36f), .93f);
                Water = PolishAssets.Material("Prison_BlackWater", new Color(.025f, .075f, .09f), .30f);
                Crest = PolishAssets.Material("Prison_IslandEmblem", Color.white, .73f, emblem);
                SmokedGlass = PolishAssets.Material("Prison_SmokedGlass", new Color(.08f, .15f, .17f, .36f), .16f);
                ConfigureSmokedGlass(SmokedGlass);
                ColdGlow = PolishAssets.Material("Prison_ColdGlow", new Color(.36f, .63f, .82f), .38f, emission: 1.55f);
                RedGlow = PolishAssets.Material("Prison_EmergencyGlow", new Color(.62f, .035f, .015f), .35f, emission: 1.5f);
            }
        }

        private static void ConfigureSmokedGlass(Material material)
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

        private static void BuildIslandExterior(Transform exterior, Palette a)
        {
            // The sea is deliberately outside the playable apron. Its low profile gives the truck approach
            // a real island silhouette without creating an exterior route for threats.
            Box("Blackwater_South", exterior, new Vector3(24f, -1.15f, -31f), new Vector3(116f, .18f, 40f), a.Water, false);
            Box("Blackwater_West", exterior, new Vector3(-28f, -1.15f, 22f), new Vector3(38f, .18f, 92f), a.Water, false);
            Box("Blackwater_East", exterior, new Vector3(76f, -1.15f, 22f), new Vector3(38f, .18f, 92f), a.Water, false);

            Box("NorthSeawall", exterior, new Vector3(24f, .55f, 50f), new Vector3(58f, 1.65f, 1.6f), a.SaltConcrete, false);
            Box("WestBreakwater", exterior, new Vector3(-2.2f, .40f, 30f), new Vector3(1.5f, 1.35f, 38f), a.SaltConcrete, false);
            Box("EastBreakwater", exterior, new Vector3(50.2f, .40f, 30f), new Vector3(1.5f, 1.35f, 38f), a.SaltConcrete, false);
            for (int i = 0; i < 10; i++)
            {
                float x = -1f + i * 5.6f;
                PolishAssets.Shape("SeawallRock_" + i, exterior, new Vector3(x, -.15f, 51.1f + (i % 2) * .35f),
                    new Vector3(3.8f, 1.8f + (i % 3) * .45f, 2.6f), a.DampConcrete, round: true);
            }

            FenceAlongX(exterior, "NorthPerimeter", -1f, 49f, 47.8f, 2.5f, a);
            FenceAlongZ(exterior, "WestPerimeter", -1.0f, 4f, 47.8f, 2.5f, a);
            FenceAlongZ(exterior, "EastPerimeter", 49.0f, 4f, 47.8f, 2.5f, a);
            BuildTower(exterior, "WestGuardTower", new Vector3(-5.2f, 0f, 43.0f), a);
            BuildTower(exterior, "EastGuardTower", new Vector3(53.2f, 0f, 43.0f), a);

            // Gatehouse pieces deliberately leave the central cargo / player approach visibly open.
            Box("GatehouseWest", exterior, new Vector3(16.0f, 1.65f, -.45f), new Vector3(3.1f, 3.3f, 2.1f), a.SaltConcrete, false);
            Box("GatehouseEast", exterior, new Vector3(32.0f, 1.65f, -.45f), new Vector3(3.1f, 3.3f, 2.1f), a.SaltConcrete, false);
            Box("GatehouseWestCap", exterior, new Vector3(16.0f, 3.42f, -.45f), new Vector3(3.5f, .22f, 2.5f), a.Iron, false);
            Box("GatehouseEastCap", exterior, new Vector3(32.0f, 3.42f, -.45f), new Vector3(3.5f, .22f, 2.5f), a.Iron, false);
            Box("IslandCrest", exterior, new Vector3(24f, 6.08f, -.17f), new Vector3(3.15f, 3.15f, .08f), a.Crest, false);
            Box("InstitutionNameplate", exterior, new Vector3(24f, 4.12f, -.18f), new Vector3(10.2f, .52f, .08f), a.Iron, false);
            Label(exterior, "InstitutionName", "BLACKWATER ISLAND PENITENTIARY", new Vector3(24f, 4.12f, -.235f),
                Quaternion.Euler(0f, 180f, 0f), .118f, new Color(.72f, .80f, .78f));
            Label(exterior, "InstitutionDate", "EST. 1914  //  STATE CORRECTIONAL AUTHORITY", new Vector3(24f, 3.58f, -.235f),
                Quaternion.Euler(0f, 180f, 0f), .067f, new Color(.48f, .57f, .56f));

            // A small dead utility annex and cable run make the northern yard feel used before it was abandoned.
            Box("YardUtilityAnnex", exterior, new Vector3(43.2f, 1.15f, 44.8f), new Vector3(5.8f, 2.3f, 3.2f), a.DampConcrete, false);
            Box("YardUtilityRoof", exterior, new Vector3(43.2f, 2.43f, 44.8f), new Vector3(6.25f, .24f, 3.65f), a.Iron, false);
            CableRun(exterior, "YardCable", new Vector3(39.5f, 2.45f, 43.2f), new Vector3(48.2f, 2.45f, 43.2f), a.Cable);
        }

        private static void BuildTower(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform tower = Group(name, parent);
            Box("ConcreteLeg", tower, at + new Vector3(0f, 2.1f, 0f), new Vector3(1.9f, 4.2f, 1.9f), a.SaltConcrete, false);
            Box("Cabin", tower, at + new Vector3(0f, 4.75f, 0f), new Vector3(3.2f, 1.4f, 3.2f), a.Iron, false);
            Box("CabinWindowSouth", tower, at + new Vector3(0f, 4.75f, -1.63f), new Vector3(2.25f, .72f, .05f), a.ColdGlow, false);
            Box("CabinRoof", tower, at + new Vector3(0f, 5.55f, 0f), new Vector3(3.65f, .22f, 3.65f), a.Rust, false);
            PolishAssets.Shape("SearchlightHousing", tower, at + new Vector3(0f, 5.92f, -.45f), new Vector3(.48f, .35f, .48f), a.ColdGlow, round: true);
        }

        private static void BuildInstitution(Transform interior, Transform root, Palette a)
        {
            BuildVisitation(interior, a);
            BuildCellblock(interior, "CellblockA", 18.65f, 3.4f, true, a);
            BuildCellblock(interior, "CellblockB", 29.35f, 44.6f, false, a);
            BuildKitchenAndLaundry(interior, a);
            BuildServiceTunnel(interior, a);
            BuildGalleryRails(interior, root, a);
        }

        private static void BuildVisitation(Transform parent, Palette a)
        {
            Transform room = Group("Visitation", parent);
            CustomMallArt.Place("desk", room, new Vector3(16.4f, .02f, 2.0f), 180f, new Vector3(.78f, .78f, .78f));
            for (int i = 0; i < 4; i++)
            {
                float x = 4.9f + i * 4f;
                Box("BoothFrame_" + i, room, new Vector3(x, 1.38f, 5.78f), new Vector3(2.9f, 2.55f, .10f), a.Iron, false);
                Box("BoothGlass_" + i, room, new Vector3(x, 1.38f, 5.70f), new Vector3(2.42f, 1.96f, .04f), a.SmokedGlass, false);
                Box("PhoneLine_" + i, room, new Vector3(x - .72f, .92f, 5.60f), new Vector3(.06f, 1.04f, .06f), a.Cable, false);
                Box("Handset_" + i, room, new Vector3(x - .72f, 1.48f, 5.52f), new Vector3(.16f, .42f, .10f), a.Iron, false);
            }
            Box("VisitorDeskLamp", room, new Vector3(16.2f, 2.48f, 2.1f), new Vector3(.8f, .08f, .34f), a.RedGlow, false);
            CustomMallArt.Place("debris", room, new Vector3(2.0f, .02f, 1.5f), 38f, new Vector3(.58f, .58f, .58f));
        }

        private static void BuildCellblock(Transform parent, string name, float frontX, float bunkX, bool westBlock, Palette a)
        {
            Transform block = Group(name, parent);
            float direction = westBlock ? -1f : 1f;
            Quaternion labelFacing = Quaternion.Euler(0f, westBlock ? -90f : 90f, 0f);
            for (int cell = 0; cell < 4; cell++)
            {
                float z = 14.35f + cell * 3.55f;
                Box("CellLintel_" + cell, block, new Vector3(frontX, 3.05f, z), new Vector3(.15f, .18f, 3.24f), a.Rust, false);
                Box("CellSill_" + cell, block, new Vector3(frontX, .34f, z), new Vector3(.15f, .16f, 3.24f), a.Iron, false);
                for (int bar = 0; bar < 6; bar++)
                {
                    float barZ = z - 1.18f + bar * .47f;
                    Box("CellBar_" + cell + "_" + bar, block, new Vector3(frontX, 1.67f, barZ), new Vector3(.13f, 2.62f, .10f), a.Iron, false);
                }
                Box("CellNumberBacker_" + cell, block, new Vector3(frontX + direction * .095f, 2.78f, z - 1.01f),
                    new Vector3(.035f, .34f, .78f), a.Rust, false);
                Label(block, "CellNumber_" + cell, (westBlock ? "A" : "B") + "-10" + (cell + 1),
                    new Vector3(frontX + direction * .12f, 2.78f, z - 1.02f), labelFacing, .074f, new Color(.74f, .67f, .52f));
                Box("BunkFrame_" + cell, block, new Vector3(bunkX, .72f, z), new Vector3(1.7f, .16f, 2.05f), a.Iron, false);
                Box("BunkMattress_" + cell, block, new Vector3(bunkX, .88f, z), new Vector3(1.52f, .14f, 1.82f), a.Paint, false);
                Box("UpperBunkFrame_" + cell, block, new Vector3(bunkX, 1.88f, z), new Vector3(1.7f, .14f, 2.05f), a.Iron, false);
                Box("UpperBunkMattress_" + cell, block, new Vector3(bunkX, 2.04f, z), new Vector3(1.52f, .14f, 1.82f), a.Paint, false);
            }
            CableRun(block, "ControlCable", new Vector3(frontX + direction * .28f, 5.88f, 12.45f),
                new Vector3(frontX + direction * .28f, 5.88f, 27.8f), a.Cable);
        }

        private static void BuildKitchenAndLaundry(Transform parent, Palette a)
        {
            Transform rooms = Group("ServiceRooms", parent);
            for (int i = 0; i < 3; i++)
            {
                float x = 31.1f + i * 4.25f;
                Box("KitchenSteelCounter_" + i, rooms, new Vector3(x, 1.01f, 6.25f), new Vector3(3.35f, 1.95f, .62f), a.Iron, false);
                Box("KitchenShelf_" + i, rooms, new Vector3(x, 2.26f, 6.0f), new Vector3(3.12f, .08f, .24f), a.Rust, false);
            }
            for (int i = 0; i < 4; i++)
            {
                float z = 29.45f + i * 1.52f;
                Box("LaundryPipe_" + i, rooms, new Vector3(38.85f, 1.95f, z), new Vector3(.16f, .16f, 1.20f), a.Iron, false);
                Box("LaundryValve_" + i, rooms, new Vector3(38.66f, 1.95f, z), new Vector3(.32f, .32f, .10f), a.Rust, false);
            }
            CustomMallArt.Place("locker", rooms, new Vector3(35.8f, .02f, 34.0f), 180f, new Vector3(.77f, .77f, .77f));
            CustomMallArt.Place("debris", rooms, new Vector3(27.8f, .02f, 34.25f), 125f, new Vector3(.46f, .46f, .46f));
        }

        private static void BuildServiceTunnel(Transform parent, Palette a)
        {
            Transform tunnel = Group("ServiceTunnel", parent);
            PipeRun(tunnel, "MainPipe", new Vector3(2.2f, -1.25f, 26.25f), new Vector3(20.9f, -1.25f, 26.25f), .42f, a.Iron);
            PipeRun(tunnel, "ReturnPipe", new Vector3(2.2f, -1.75f, 27.05f), new Vector3(20.9f, -1.75f, 27.05f), .24f, a.Rust);
            PipeRun(tunnel, "SteamPipe", new Vector3(17.2f, -1.35f, 25.2f), new Vector3(17.2f, -1.35f, 34.3f), .20f, a.Iron);
            for (int i = 0; i < 5; i++)
            {
                float x = 2.7f + i * 4.05f;
                Box("PipeBracket_" + i, tunnel, new Vector3(x, -1.56f, 26.25f), new Vector3(.13f, .80f, .72f), a.Rust, false);
            }
            CustomMallArt.Place("puddle", tunnel, new Vector3(5.7f, -3.97f, 31.2f), 12f, new Vector3(1.3f, 1f, 1.3f));
            CustomMallArt.Place("puddle", tunnel, new Vector3(18.1f, -3.97f, 28.8f), 167f, new Vector3(.92f, 1f, .92f));
            CustomMallArt.Place("debris", tunnel, new Vector3(20.1f, -3.98f, 33.6f), 18f, new Vector3(.42f, .42f, .42f));
        }

        private static void BuildGalleryRails(Transform parent, Transform root, Palette a)
        {
            Transform gallery = Group("GalleryRails", parent);
            for (int row = 0; row < 5; row++)
            {
                int zCell = 3 + row;
                float z = zCell * 4f + 2f;
                Transform west = RailSegment(gallery, "WestGalleryRail_" + row, new Vector3(11.83f, 4.62f, z), a);
                Transform east = RailSegment(gallery, "EastGalleryRail_" + row, new Vector3(36.17f, 4.62f, z), a);
                AttachToUpperTile(root, west, 2, zCell);
                AttachToUpperTile(root, east, 9, zCell);
            }
        }

        private static Transform RailSegment(Transform parent, string name, Vector3 at, Palette a)
        {
            Transform rail = Group(name, parent);
            Box("TopRail", rail, at + Vector3.up * .44f, new Vector3(.11f, .10f, 3.76f), a.Iron, false);
            Box("MiddleRail", rail, at, new Vector3(.08f, .07f, 3.76f), a.Rust, false);
            for (int post = 0; post < 3; post++)
                Box("Post_" + post, rail, at + new Vector3(0f, -.22f, -1.45f + post * 1.45f), new Vector3(.10f, 1.30f, .10f), a.Iron, false);
            return rail;
        }

        private static void AttachToUpperTile(Transform root, Transform decor, int x, int z)
        {
            Transform tile = root.Find($"Levels/UpperFloor/Tile_1_{x}_{z}");
            StructuralSection section = tile != null ? tile.GetComponent<StructuralSection>() : null;
            if (section != null) decor.gameObject.AddComponent<SectionProp>().EditorSetup(section);
        }

        private static void BuildDecay(Transform decay, Palette a)
        {
            // Small, repeated water ingress cues let the prison read as a coastal facility without jumping
            // early to occult imagery. Imported art removes its own colliders in CustomMallArt.Place.
            Vector3[] mould =
            {
                new(1.0f, .02f, 15.4f), new(46.9f, .02f, 19.8f), new(3.2f, .02f, 26.4f),
                new(39.6f, .02f, 34.7f), new(10.3f, -3.96f, 34.4f), new(21.1f, -3.96f, 25.3f),
            };
            for (int i = 0; i < mould.Length; i++)
                CustomMallArt.Place("mould", decay, mould[i], i * 37f, new Vector3(1.08f, 1f, 1.08f));

            Vector3[] stains =
            {
                new(6.6f, .022f, 11.4f), new(16.5f, .022f, 27.3f), new(31.3f, .022f, 18.7f),
                new(44.7f, .022f, 27.4f), new(30.1f, .022f, 35.2f), new(6.5f, -3.96f, 28.4f),
            };
            for (int i = 0; i < stains.Length; i++)
                CustomMallArt.Place("stain", decay, stains[i], i * 51f, new Vector3(.95f, 1f, .95f));

            // A few flaked institutional paint bands give long cellblock walls a purposeful, legible rhythm.
            for (int i = 0; i < 6; i++)
            {
                float z = 13.2f + i * 2.65f;
                Box("WestPaintFlake_" + i, decay, new Vector3(.15f, 1.72f, z), new Vector3(.025f, .55f, 1.38f), a.Paint, false);
                Box("EastPaintFlake_" + i, decay, new Vector3(47.85f, 1.72f, z + .62f), new Vector3(.025f, .38f, .88f), a.Paint, false);
            }
            CustomMallArt.Place("crack", decay, new Vector3(18.3f, .022f, 30.7f), 28f, new Vector3(1.16f, 1f, 1.16f));
            CustomMallArt.Place("rebar", decay, new Vector3(46.7f, .02f, 32.2f), 270f, new Vector3(.68f, .68f, .68f));
        }

        private static void BuildEmergencyLighting(Transform parent, Palette a)
        {
            AddEmergencyLight(parent, "VisitationAlarm", new Vector3(18.5f, 2.72f, 7.65f), a, 811, false);
            AddEmergencyLight(parent, "WestBlockAlarm", new Vector3(2.15f, 5.6f, 24.0f), a, 812, true);
            AddEmergencyLight(parent, "EastBlockAlarm", new Vector3(45.85f, 5.6f, 24.0f), a, 813, false);
            AddEmergencyLight(parent, "TunnelAlarm", new Vector3(21.5f, -1.1f, 33.0f), a, 814, true);
            AddEmergencyLight(parent, "LaundryAlarm", new Vector3(38.45f, 2.72f, 35.75f), a, 815, false);
        }

        private static void AddEmergencyLight(Transform parent, string name, Vector3 at, Palette a, int seed, bool faulty)
        {
            Transform fixture = Group(name, parent);
            fixture.localPosition = at;
            GameObject glow = Box("Glow", fixture, Vector3.zero, new Vector3(.52f, .13f, .20f), a.RedGlow, false);
            Light lamp = fixture.gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, .055f, .018f);
            lamp.range = 4.8f;
            lamp.intensity = .48f;
            lamp.shadows = LightShadows.None;
            fixture.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), faulty, false, seed,
                a.RedGlow.GetColor("_EmissionColor"));
        }

        private static void FenceAlongX(Transform parent, string name, float from, float to, float z, float height, Palette a)
        {
            Transform fence = Group(name, parent);
            for (float x = from; x <= to + .01f; x += 3f)
                Box("Post", fence, new Vector3(x, height * .5f, z), new Vector3(.10f, height, .10f), a.Iron, false);
            Box("TopRail", fence, new Vector3((from + to) * .5f, height - .12f, z), new Vector3(to - from, .08f, .08f), a.Rust, false);
            Box("MidRail", fence, new Vector3((from + to) * .5f, height * .53f, z), new Vector3(to - from, .06f, .06f), a.Iron, false);
        }

        private static void FenceAlongZ(Transform parent, string name, float x, float from, float to, float height, Palette a)
        {
            Transform fence = Group(name, parent);
            for (float z = from; z <= to + .01f; z += 3f)
                Box("Post", fence, new Vector3(x, height * .5f, z), new Vector3(.10f, height, .10f), a.Iron, false);
            Box("TopRail", fence, new Vector3(x, height - .12f, (from + to) * .5f), new Vector3(.08f, .08f, to - from), a.Rust, false);
            Box("MidRail", fence, new Vector3(x, height * .53f, (from + to) * .5f), new Vector3(.06f, .06f, to - from), a.Iron, false);
        }

        private static void CableRun(Transform parent, string name, Vector3 from, Vector3 to, Material material)
        {
            Transform run = Group(name, parent);
            Vector3 delta = to - from;
            run.localPosition = (from + to) * .5f;
            run.localRotation = delta.sqrMagnitude > .0001f ? Quaternion.LookRotation(delta.normalized, Vector3.up) : Quaternion.identity;
            Box("Cable", run, Vector3.zero, new Vector3(.075f, .075f, delta.magnitude), material, false);
        }

        private static void PipeRun(Transform parent, string name, Vector3 from, Vector3 to, float width, Material material)
        {
            Transform run = Group(name, parent);
            Vector3 delta = to - from;
            run.localPosition = (from + to) * .5f;
            run.localRotation = delta.sqrMagnitude > .0001f ? Quaternion.LookRotation(delta.normalized, Vector3.up) : Quaternion.identity;
            Box("Pipe", run, Vector3.zero, new Vector3(width, width, delta.magnitude), material, false);
        }

        private static void Label(Transform parent, string name, string text, Vector3 at, Quaternion rotation, float characterSize, Color color)
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

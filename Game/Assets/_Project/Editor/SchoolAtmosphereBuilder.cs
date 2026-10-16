using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Presentation-only dressing for the School contract. The playable layout remains owned by
    /// SalvageLocationBuilder; this pass gives its rooms the specific, neglected identity of an
    /// American high school while leaving navigation, encounter spaces, and structural gameplay alone.
    /// </summary>
    public static class SchoolAtmosphereBuilder
    {
        private const string CrestPath = "Assets/_Project/Art/Generated/SchoolAtmosphere/SchoolOwlCrest.png";
        private const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public static void Apply(Transform root)
        {
            if (root == null) return;

            Transform pass = Group("SchoolAtmospherePass", root);
            // Every item in this pass is deliberately visual-only. Keeping it out of the bake protects the
            // tested routes to the School's loot sites, threat markers, stairs, and exterior extraction zone.
            pass.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            var art = new Palette();
            Transform facade = Group("CampusFacade", pass);
            Transform rooms = Group("InteriorSetDressing", pass);
            Transform decay = Group("InstitutionalDecay", pass);
            Transform emergencyLighting = Group("EmergencyLighting", pass);

            BuildCampusFacade(facade, art);
            BuildInterior(rooms, root, art);
            BuildDecay(decay, art);
            BuildEmergencyLighting(emergencyLighting, art);

            // Runtime-controlled lights intentionally remain non-static so LightFixture can flicker, dim,
            // and react to the facility power state during play.
            MarkStatic(facade);
            MarkStatic(rooms);
            MarkStatic(decay);
        }

        private sealed class Palette
        {
            public readonly Material HallPaint;
            public readonly Material ScuffedTile;
            public readonly Material Navy;
            public readonly Material Gold;
            public readonly Material Wood;
            public readonly Material LockerSteel;
            public readonly Material Cork;
            public readonly Material Paper;
            public readonly Material Crest;
            public readonly Material SmokedGlass;
            public readonly Material CourtWet;
            public readonly Material Cable;
            public readonly Material ExitGlow;
            public readonly Material AmberGlow;

            public Palette()
            {
                Texture2D crest = AssetDatabase.LoadAssetAtPath<Texture2D>(CrestPath);
                if (crest == null)
                {
                    AssetDatabase.ImportAsset(CrestPath, ImportAssetOptions.ForceSynchronousImport);
                    crest = AssetDatabase.LoadAssetAtPath<Texture2D>(CrestPath);
                }

                HallPaint = PolishAssets.Material("School_HallPaint", new Color(.27f, .31f, .29f), .94f,
                    PolishAssets.Texture("School_HallPaintTexture", tiles: true));
                ScuffedTile = PolishAssets.Material("School_ScuffedTile", new Color(.29f, .31f, .27f), .91f,
                    PolishAssets.Texture("School_ScuffedTileTexture", tiles: true));
                Navy = PolishAssets.Material("School_FadedNavy", new Color(.055f, .09f, .13f), .72f);
                Gold = PolishAssets.Material("School_TarnishedGold", new Color(.43f, .32f, .13f), .58f);
                Wood = PolishAssets.Material("School_StageWood", new Color(.20f, .115f, .065f), .83f);
                LockerSteel = PolishAssets.Material("School_LockerSteel", new Color(.20f, .28f, .29f), .58f);
                Cork = PolishAssets.Material("School_Corkboard", new Color(.30f, .20f, .105f), .91f);
                Paper = PolishAssets.Material("School_AgedPaper", new Color(.48f, .45f, .32f), .92f);
                Crest = PolishAssets.Material("School_OwlCrest", Color.white, .72f, crest);
                SmokedGlass = PolishAssets.Material("School_SmokedGlass", new Color(.105f, .15f, .16f, .42f), .18f);
                ConfigureSmokedGlass(SmokedGlass);
                CourtWet = PolishAssets.Material("School_WetCourt", new Color(.09f, .135f, .12f), .22f);
                Cable = PolishAssets.Material("School_Cable", new Color(.022f, .026f, .023f), .45f);
                ExitGlow = PolishAssets.Material("School_ExitGlow", new Color(.28f, .62f, .38f), .36f, emission: 1.38f);
                AmberGlow = PolishAssets.Material("School_AmberGlow", new Color(.72f, .43f, .16f), .36f, emission: 1.2f);
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

        private static void BuildCampusFacade(Transform facade, Palette a)
        {
            // The header sits above the permanently open southern entry, leaving the truck approach and
            // the player-spawn apron unmistakably clear while giving the building a real civic identity.
            Box("FrontNavyHeader", facade, new Vector3(24f, 6.05f, -.15f), new Vector3(20.4f, .42f, .12f), a.Navy, false);
            Box("FrontGoldRule", facade, new Vector3(24f, 5.77f, -.17f), new Vector3(20.6f, .07f, .08f), a.Gold, false);
            Box("OwlCrest", facade, new Vector3(24f, 4.85f, -.18f), new Vector3(2.85f, 2.85f, .07f), a.Crest, false);
            Box("SchoolNameplate", facade, new Vector3(24f, 3.38f, -.18f), new Vector3(12.2f, .54f, .07f), a.Navy, false);
            Label(facade, "SchoolName", "RIDGEWAY HIGH SCHOOL", new Vector3(24f, 3.38f, -.23f),
                Quaternion.Euler(0f, 180f, 0f), .145f, new Color(.77f, .69f, .43f));
            Label(facade, "SchoolDate", "HOME OF THE OWLS  //  EST. 1958", new Vector3(24f, 2.95f, -.23f),
                Quaternion.Euler(0f, 180f, 0f), .075f, new Color(.48f, .56f, .52f));

            // A few non-playable campus cues read from the approach without turning the lot into an obstacle course.
            BuildFlagpole(facade, "WestFlagpole", new Vector3(5.4f, 0f, -1.35f), a, false);
            BuildFlagpole(facade, "EastFlagpole", new Vector3(42.6f, 0f, -1.35f), a, true);
            Box("WestBusStopPost", facade, new Vector3(5.4f, 1.15f, -7.2f), new Vector3(.09f, 2.3f, .09f), a.LockerSteel, false);
            Box("WestBusStopSign", facade, new Vector3(5.4f, 2.0f, -7.2f), new Vector3(.85f, .62f, .05f), a.Gold, false);
            Label(facade, "WestBusStopText", "BUS", new Vector3(5.4f, 2.0f, -7.24f), Quaternion.Euler(0f, 180f, 0f), .115f, a.Navy.GetColor("_BaseColor"));

            // The northern yard is deliberately just a silhouette: no new exterior routes, but enough shape to
            // stop the roofline feeling isolated against the night sky.
            for (int i = 0; i < 7; i++)
            {
                float x = -3f + i * 9.3f;
                float height = 5.2f + (i % 3) * .95f;
                PolishAssets.Shape("NorthYardTree_" + i, facade, new Vector3(x, height * .5f, 48.7f + (i % 2) * 2.3f),
                    new Vector3(3.1f, height, 3.1f), a.Navy, round: true);
            }
        }

        private static void BuildFlagpole(Transform parent, string name, Vector3 at, Palette a, bool torn)
        {
            Transform pole = Group(name, parent);
            Box("Pole", pole, at + new Vector3(0f, 3.0f, 0f), new Vector3(.09f, 6f, .09f), a.LockerSteel, false);
            PolishAssets.Shape("Finial", pole, at + new Vector3(0f, 6.12f, 0f), new Vector3(.28f, .28f, .28f), a.Gold, round: true);
            Box("Flag", pole, at + new Vector3(torn ? .58f : -.58f, 5.0f, 0f), new Vector3(1.1f, 1.05f, .035f), torn ? a.Navy : a.Gold, false);
        }

        private static void BuildInterior(Transform parent, Transform root, Palette a)
        {
            BuildCorridors(parent, a);
            BuildClassrooms(parent, a);
            BuildCafeteria(parent, a);
            BuildAuditorium(parent, a);
            BuildLibrary(parent, a);
            BuildGymnasium(parent, a);
            BuildScienceWing(parent, root, a);
            BuildArchive(parent, a);
        }

        private static void BuildCorridors(Transform parent, Palette a)
        {
            Transform hall = Group("HallwayIdentity", parent);

            // Dented locker banks turn the otherwise broad circulation space into a recognisable school hall.
            for (int i = 0; i < 5; i++)
            {
                float z = 10.3f + i * 5.0f;
                CustomMallArt.Place("locker", hall, new Vector3(1.05f, .02f, z), 90f, new Vector3(.83f, .83f, .83f));
                if (i < 4)
                    CustomMallArt.Place("locker", hall, new Vector3(46.95f, .02f, z + 1.8f), -90f, new Vector3(.78f, .78f, .78f));
            }

            // Trophy case in the central hall: all framing is flat/non-colliding, so the visual does not affect routes.
            Transform trophies = Group("TrophyCase", hall);
            Box("CaseBack", trophies, new Vector3(23.95f, 1.65f, 11.86f), new Vector3(5.7f, 3.1f, .10f), a.Navy, false);
            Box("CaseGlass", trophies, new Vector3(23.95f, 1.65f, 11.78f), new Vector3(5.25f, 2.52f, .035f), a.SmokedGlass, false);
            for (int i = 0; i < 4; i++)
            {
                float x = 22.15f + i * 1.18f;
                Box("TrophyBase_" + i, trophies, new Vector3(x, .60f, 11.71f), new Vector3(.34f, .10f, .18f), a.Gold, false);
                PolishAssets.Shape("TrophyCup_" + i, trophies, new Vector3(x, 1.0f + (i % 2) * .18f, 11.69f),
                    new Vector3(.22f, .7f + (i % 2) * .18f, .22f), a.Gold, round: true);
            }
            Label(trophies, "TrophyCaseTitle", "RIDGEWAY ATHLETICS", new Vector3(23.95f, 2.72f, 11.70f),
                Quaternion.identity, .095f, new Color(.74f, .65f, .37f));

            BuildBanner(hall, "WestSpiritBanner", new Vector3(.18f, 2.25f, 20.6f), Quaternion.Euler(0f, 90f, 0f), "OWLS", a);
            BuildBanner(hall, "EastSpiritBanner", new Vector3(47.82f, 2.25f, 24.8f), Quaternion.Euler(0f, -90f, 0f), "CLASS OF '93", a);
            Box("CeilingCable", hall, new Vector3(24f, 3.06f, 21.2f), new Vector3(17.6f, .06f, .06f), a.Cable, false);
        }

        private static void BuildBanner(Transform parent, string name, Vector3 at, Quaternion rotation, string text, Palette a)
        {
            Transform banner = Group(name, parent);
            banner.localPosition = at;
            banner.localRotation = rotation;
            Box("Cloth", banner, Vector3.zero, new Vector3(2.1f, 1.35f, .035f), a.Navy, false);
            Box("Trim", banner, new Vector3(0f, -.59f, -.025f), new Vector3(2.22f, .08f, .02f), a.Gold, false);
            Label(banner, "Text", text, new Vector3(0f, .02f, -.03f), Quaternion.identity, .115f, new Color(.70f, .63f, .38f));
        }

        private static void BuildClassrooms(Transform parent, Palette a)
        {
            Transform rooms = Group("Classrooms", parent);
            BuildClassroom(rooms, "ClassroomA", 0.1f, 0.1f, 11.8f, a);
            BuildClassroom(rooms, "ClassroomB", 12.1f, 0.1f, 19.8f, a);
        }

        private static void BuildClassroom(Transform parent, string name, float x0, float z0, float x1, Palette a)
        {
            Transform room = Group(name, parent);
            Box("Chalkboard", room, new Vector3((x0 + x1) * .5f, 1.68f, z0 + .12f), new Vector3(Mathf.Max(3.0f, x1 - x0 - 1.2f), 1.25f, .035f), a.Navy, false);
            Label(room, "Lesson", "FINAL EXAMS  //  MAY 17", new Vector3((x0 + x1) * .5f, 1.68f, z0 + .15f),
                Quaternion.identity, .085f, new Color(.63f, .67f, .60f));
            CustomMallArt.Place("desk", room, new Vector3(x0 + 1.45f, .02f, z0 + 5.85f), 16f, new Vector3(.66f, .66f, .66f));
            CustomMallArt.Place("desk", room, new Vector3(x1 - 1.55f, .02f, z0 + 2.05f), 194f, new Vector3(.58f, .58f, .58f));
            Box("TeacherDesk", room, new Vector3(x0 + 1.35f, .58f, z0 + 1.35f), new Vector3(1.52f, 1.1f, .70f), a.Wood, false);
            Box("NoticeBoard", room, new Vector3(x1 - .13f, 1.65f, z0 + 4.0f), new Vector3(.04f, 1.36f, 2.1f), a.Cork, false);
            Box("Notice_1", room, new Vector3(x1 - .16f, 1.85f, z0 + 3.5f), new Vector3(.018f, .40f, .56f), a.Paper, false);
            Box("Notice_2", room, new Vector3(x1 - .16f, 1.25f, z0 + 4.55f), new Vector3(.018f, .34f, .42f), a.Paper, false);
        }

        private static void BuildCafeteria(Transform parent, Palette a)
        {
            Transform cafeteria = Group("Cafeteria", parent);
            Box("ServingCounter", cafeteria, new Vector3(36.0f, 1.05f, 1.05f), new Vector3(6.2f, 2.1f, .72f), a.LockerSteel, false);
            Box("MenuBoard", cafeteria, new Vector3(36f, 2.42f, 1.45f), new Vector3(4.7f, .72f, .04f), a.Navy, false);
            Label(cafeteria, "MenuText", "LUNCH  //  11:40 - 12:25", new Vector3(36f, 2.42f, 1.40f),
                Quaternion.identity, .078f, new Color(.70f, .62f, .40f));
            CustomMallArt.Place("food_table", cafeteria, new Vector3(33.7f, .02f, 4.7f), 90f, new Vector3(.86f, .86f, .86f));
            CustomMallArt.Place("food_table", cafeteria, new Vector3(37.3f, .02f, 6.25f), 90f, new Vector3(.78f, .78f, .78f));
            CustomMallArt.Place("debris", cafeteria, new Vector3(30.8f, .02f, 2.1f), 42f, new Vector3(.50f, .50f, .50f));
        }

        private static void BuildAuditorium(Transform parent, Palette a)
        {
            Transform auditorium = Group("Auditorium", parent);
            Box("ProjectionScreen", auditorium, new Vector3(8f, 4.25f, 19.82f), new Vector3(7.0f, 3.1f, .04f), a.SmokedGlass, false);
            Box("CurtainWest", auditorium, new Vector3(3.75f, 3.65f, 19.74f), new Vector3(1.1f, 4.15f, .07f), a.Navy, false);
            Box("CurtainEast", auditorium, new Vector3(12.25f, 3.65f, 19.74f), new Vector3(1.1f, 4.15f, .07f), a.Navy, false);
            Label(auditorium, "AssemblyHall", "ASSEMBLY HALL", new Vector3(8f, 5.95f, 19.69f),
                Quaternion.Euler(0f, 180f, 0f), .14f, new Color(.62f, .55f, .32f));
            for (int row = 0; row < 3; row++)
            for (int side = 0; side < 2; side++)
            {
                float x = side == 0 ? 2.0f : 14.0f;
                CustomMallArt.Place("cinema_seat", auditorium, new Vector3(x, .02f, 13.55f + row * 1.95f),
                    side == 0 ? 8f : -8f, new Vector3(.80f, .80f, .80f));
            }
            CustomMallArt.Place("debris", auditorium, new Vector3(3.5f, .02f, 18.1f), 222f, new Vector3(.55f, .55f, .55f));
        }

        private static void BuildLibrary(Transform parent, Palette a)
        {
            Transform library = Group("Library", parent);
            for (int i = 0; i < 3; i++)
            {
                float z = 27.2f + i * 2.55f;
                Box("Bookcase_" + i, library, new Vector3(2.0f, 1.35f, z), new Vector3(1.30f, 2.70f, 1.55f), a.Wood, false);
                Box("BookBand_" + i, library, new Vector3(2.0f, 1.45f, z), new Vector3(1.36f, .10f, 1.62f), a.Gold, false);
            }
            Box("ReferenceDesk", library, new Vector3(13.75f, .65f, 34.1f), new Vector3(2.1f, 1.3f, .80f), a.Wood, false);
            Box("LibrarySign", library, new Vector3(7.3f, 2.5f, 35.83f), new Vector3(5.5f, .46f, .04f), a.Navy, false);
            Label(library, "LibraryText", "RIDGEWAY LIBRARY", new Vector3(7.3f, 2.5f, 35.78f),
                Quaternion.Euler(0f, 180f, 0f), .115f, new Color(.69f, .60f, .36f));
            CustomMallArt.Place("puddle", library, new Vector3(4.7f, .018f, 33.1f), 19f, new Vector3(.92f, 1f, .92f));
            CustomMallArt.Place("debris", library, new Vector3(10.1f, .02f, 26.6f), 114f, new Vector3(.48f, .48f, .48f));
        }

        private static void BuildGymnasium(Transform parent, Palette a)
        {
            Transform gym = Group("Gymnasium", parent);
            // A dull wet sheen and painted court lines create a huge, recently-rained-in gym without changing the floor collision.
            Box("WetCourt", gym, new Vector3(38f, .024f, 28.0f), new Vector3(16.2f, .025f, 21.0f), a.CourtWet, false);
            Box("CenterLine", gym, new Vector3(38f, .045f, 28f), new Vector3(15.6f, .025f, .12f), a.Gold, false);
            Box("CenterMark", gym, new Vector3(38f, .047f, 28f), new Vector3(.13f, .025f, 7.6f), a.Gold, false);
            Box("NorthBackboard", gym, new Vector3(38f, 3.15f, 39.82f), new Vector3(3.1f, 1.82f, .06f), a.SmokedGlass, false);
            Box("NorthRim", gym, new Vector3(38f, 2.58f, 39.62f), new Vector3(.96f, .10f, .48f), a.Gold, false);
            BuildBanner(gym, "GymBannerWest", new Vector3(28.18f, 4.75f, 23.0f), Quaternion.Euler(0f, 90f, 0f), "GO OWLS", a);
            BuildBanner(gym, "GymBannerEast", new Vector3(47.82f, 4.75f, 33.0f), Quaternion.Euler(0f, -90f, 0f), "DISTRICT '93", a);
            for (int i = 0; i < 4; i++)
                Box("EastBleacher_" + i, gym, new Vector3(46.0f, .30f + i * .22f, 21.8f + i * .18f),
                    new Vector3(2.1f, .60f + i * .12f, 6.2f), a.LockerSteel, false);
        }

        private static void BuildScienceWing(Transform parent, Transform root, Palette a)
        {
            Transform science = Group("ScienceWing", parent);
            for (int i = 0; i < 3; i++)
            {
                float x = 10.25f + i * 3.55f;
                Box("LabBench_" + i, science, new Vector3(x, 4.66f, 1.58f), new Vector3(2.55f, 1.32f, .86f), a.LockerSteel, false);
                Box("LabGlass_" + i, science, new Vector3(x, 5.63f, 1.60f), new Vector3(1.42f, .88f, .03f), a.SmokedGlass, false);
            }
            Box("ScienceBoard", science, new Vector3(16f, 5.78f, .12f), new Vector3(5.0f, 1.20f, .04f), a.Navy, false);
            Label(science, "ScienceText", "SCIENCE WING", new Vector3(16f, 5.78f, .15f),
                Quaternion.identity, .12f, new Color(.64f, .68f, .60f));

            // This loose upper-wing cabinet is attached to its structural section so it falls away cleanly if that
            // section collapses; it is intentionally nowhere near the science loot or threat marker.
            Transform cabinet = Group("SuspendedLabCabinet", science);
            Box("Cabinet", cabinet, new Vector3(9.55f, 6.75f, 2.7f), new Vector3(1.45f, .65f, .58f), a.Wood, false);
            AttachToUpperTile(root, cabinet, 2, 0);
        }

        private static void BuildArchive(Transform parent, Palette a)
        {
            Transform archive = Group("BasementArchive", parent);
            for (int i = 0; i < 3; i++)
            {
                float x = 11.85f + i * 1.35f;
                Box("FilingBank_" + i, archive, new Vector3(x, -2.70f, 33.7f), new Vector3(1.06f, 2.6f, 2.95f), a.LockerSteel, false);
                Box("DrawerLine_" + i, archive, new Vector3(x, -2.46f, 32.20f), new Vector3(.88f, .065f, .04f), a.Gold, false);
            }
            Box("ArchivePipe", archive, new Vector3(12.9f, -1.42f, 27.0f), new Vector3(5.6f, .22f, .22f), a.LockerSteel, false);
            Box("ArchivePipeValve", archive, new Vector3(10.2f, -1.42f, 27f), new Vector3(.52f, .52f, .10f), a.Gold, false);
            CustomMallArt.Place("puddle", archive, new Vector3(13.8f, -3.98f, 27.0f), 137f, new Vector3(.98f, 1f, .98f));
            CustomMallArt.Place("debris", archive, new Vector3(2.2f, -3.98f, 34.3f), 83f, new Vector3(.52f, .52f, .52f));
        }

        private static void AttachToUpperTile(Transform root, Transform decor, int x, int z)
        {
            Transform tile = root.Find($"Levels/UpperFloor/Tile_1_{x}_{z}");
            StructuralSection section = tile != null ? tile.GetComponent<StructuralSection>() : null;
            if (section != null) decor.gameObject.AddComponent<SectionProp>().EditorSetup(section);
        }

        private static void BuildDecay(Transform parent, Palette a)
        {
            // Repeated but restrained damage gives every wing a history of leaks, bad ventilation, and neglect.
            Vector3[] mould =
            {
                new(1.0f, .022f, 9.5f), new(46.8f, .022f, 18.4f), new(3.2f, .022f, 31.8f),
                new(29.5f, .022f, 37.5f), new(10.4f, 4.022f, 2.65f), new(14.2f, -3.978f, 34.7f),
            };
            for (int i = 0; i < mould.Length; i++)
                CustomMallArt.Place("mould", parent, mould[i], i * 43f, new Vector3(1.03f, 1f, 1.03f));

            Vector3[] stains =
            {
                new(5.0f, .024f, 17.5f), new(20.8f, .024f, 14.1f), new(29.8f, .024f, 34.8f),
                new(41.4f, .024f, 21.5f), new(20.2f, 4.024f, 3.4f), new(5.2f, -3.976f, 27.9f),
            };
            for (int i = 0; i < stains.Length; i++)
                CustomMallArt.Place("stain", parent, stains[i], i * 59f, new Vector3(.96f, 1f, .96f));

            CustomMallArt.Place("crack", parent, new Vector3(22.2f, .024f, 22.9f), 21f, new Vector3(1.16f, 1f, 1.16f));
            CustomMallArt.Place("glass", parent, new Vector3(30.3f, .026f, 12.3f), 82f, new Vector3(.70f, .70f, .70f));
            CustomMallArt.Place("growth", parent, new Vector3(2.0f, .02f, 42.3f), 12f, new Vector3(.92f, .92f, .92f));
            CustomMallArt.Place("growth", parent, new Vector3(45.5f, .02f, 44.4f), 224f, new Vector3(.76f, .76f, .76f));

            for (int i = 0; i < 5; i++)
            {
                float x = 3.6f + i * 9.0f;
                Box("PeelingPaint_" + i, parent, new Vector3(x, 1.44f + (i % 2) * .23f, 39.83f),
                    new Vector3(2.35f, .60f, .02f), a.HallPaint, false);
            }
        }

        private static void BuildEmergencyLighting(Transform parent, Palette a)
        {
            AddExitSign(parent, "FrontHallExit", new Vector3(24f, 2.72f, 8.10f), Quaternion.identity, a, 671, false);
            AddExitSign(parent, "AuditoriumExit", new Vector3(8f, 2.72f, 11.88f), Quaternion.identity, a, 672, true);
            AddExitSign(parent, "GymExit", new Vector3(38f, 2.72f, 15.88f), Quaternion.identity, a, 673, false);
            AddExitSign(parent, "LibraryExit", new Vector3(8f, 2.72f, 23.90f), Quaternion.Euler(0f, 180f, 0f), a, 674, true);
            AddExitSign(parent, "ScienceExit", new Vector3(16f, 6.72f, 8.10f), Quaternion.identity, a, 675, false);
            AddExitSign(parent, "ArchiveExit", new Vector3(8f, -1.18f, 24.10f), Quaternion.Euler(0f, 180f, 0f), a, 676, true);
        }

        private static void AddExitSign(Transform parent, string name, Vector3 at, Quaternion rotation, Palette a, int seed, bool faulty)
        {
            Transform fixture = Group(name, parent);
            fixture.localPosition = at;
            fixture.localRotation = rotation;
            GameObject glow = Box("Glow", fixture, Vector3.zero, new Vector3(.94f, .26f, .08f), a.ExitGlow, false);
            Label(fixture, "Text", "EXIT", new Vector3(0f, 0f, -.046f), Quaternion.identity, .103f, new Color(.77f, 1f, .79f));
            Light lamp = fixture.gameObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(.42f, .82f, .49f);
            lamp.range = 4.25f;
            lamp.intensity = .32f;
            lamp.shadows = LightShadows.None;
            fixture.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(), faulty, false, seed,
                a.ExitGlow.GetColor("_EmissionColor"));
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

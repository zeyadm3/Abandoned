using System.Linq;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The one list of scenes, first scene first. <see cref="ApplyToEditorSettings"/> copies all of it into
    /// File > Build Profiles (the editor and tests load the TestMap from there); a player gets
    /// <see cref="ForPlayer"/>: the TestMap is a feature-testing map, not a level, so only dev builds
    /// (nettest) carry it. <see cref="SalvageScenes"/> is the set of contract destinations: keep it
    /// separate from the HQ, the training house and the test fixture so new locations cannot accidentally be
    /// omitted from a player.
    /// </summary>
    public static class BuildScenes
    {
        /// <summary>Scenes a contract may send a crew to. Their order is the stable build order.</summary>
        public static readonly string[] SalvageScenes =
        {
            MallBuilder.ScenePath,
            SchoolBuilder.ScenePath,
            FactoryBuilder.ScenePath,
            PrisonBuilder.ScenePath,
            Abandoned.Hospital.HospitalBuilder.ScenePath,
            Abandoned.Motel.MotelBuilder.ScenePath,
            SkyscraperBuilder.ScenePath,
        };

        public static readonly string[] All =
        {
            // The company HQ (M6): builds open here; players join here; contracts drive everyone to a selected location.
            HqBuilder.ScenePath,
            // The open world (Docs/OPEN_WORLD_PLAN.md): the HQ hands over to it as a session starts; places stream into it.
            WorldBuilder.ScenePath,
            MallBuilder.ScenePath,
            SchoolBuilder.ScenePath,
            FactoryBuilder.ScenePath,
            PrisonBuilder.ScenePath,
            Abandoned.Hospital.HospitalBuilder.ScenePath,
            Abandoned.Motel.MotelBuilder.ScenePath,
            SkyscraperBuilder.ScenePath,
            // Open-world stations, streamed in by distance; not contract destinations until the world lists them.
            PoliceStationLayout.ScenePath,
            FireStationLayout.ScenePath,
            // The training job (14 Larch Court): ships in every build, but is never a contract destination.
            TutorialHouseLayout.ScenePath,
            // The hiring's HR office (2026 remake): offline, first launch only; reached by scene name, never by travel.
            Hiring.HrOfficeBuilder.ScenePath,
            TestMapBuilder.ScenePath,
        };

        public static string[] ForPlayer(BuildFlavor flavor) =>
            flavor == BuildFlavor.Dev ? All : All.Where(s => s != TestMapBuilder.ScenePath).ToArray();

        public static void ApplyToEditorSettings()
        {
            EditorBuildSettingsScene[] wanted = All.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            bool same = EditorBuildSettings.scenes.Length == wanted.Length &&
                        EditorBuildSettings.scenes.Zip(wanted, (a, b) => a.path == b.path && a.enabled).All(x => x);
            if (!same) EditorBuildSettings.scenes = wanted;
        }
    }
}

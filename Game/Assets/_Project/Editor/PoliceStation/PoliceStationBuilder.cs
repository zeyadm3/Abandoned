using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the Millbrook 3rd Precinct (Docs/POLICE_STATION_PLAN.md) into Scenes/PoliceStation.unity from scratch:
    /// <see cref="StationBuild"/> with <see cref="PoliceStationRecipe"/>. Re-running it rebuilds the scene.
    /// </summary>
    public static class PoliceStationBuilder
    {
        public const string ScenePath = PoliceStationLayout.ScenePath;

        [MenuItem(ToolMenu.PoliceStation + "Rebuild Police Station", priority = ToolMenu.PoliceStationP)]
        public static void Rebuild() => Build();

        public static void Build() => StationBuild.Build(new PoliceStationRecipe());
    }
}

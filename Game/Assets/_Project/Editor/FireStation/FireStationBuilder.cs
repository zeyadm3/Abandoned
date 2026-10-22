using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Millbrook Fire Station No. 4 (Docs/FIRE_STATION_PLAN.md) into Scenes/FireStation.unity from scratch:
    /// <see cref="StationBuild"/> with <see cref="FireStationRecipe"/>. Re-running it rebuilds the scene.
    /// </summary>
    public static class FireStationBuilder
    {
        public const string ScenePath = FireStationLayout.ScenePath;

        [MenuItem(ToolMenu.FireStation + "Rebuild Fire Station", priority = ToolMenu.FireStationP)]
        public static void Rebuild() => Build();

        public static void Build() => StationBuild.Build(new FireStationRecipe());

        [MenuItem(ToolMenu.FireStation + "Add Fire Station Loot", priority = ToolMenu.FireStationP + 20)]
        public static void AddLoot() => FireStationLoot.Kit.EnsureContent();
    }
}

using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>The Prison location's scene path and the entry point batch commands call; it builds the island prison.</summary>
    public static class PrisonBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Prison.unity";

        public static void Create() => IslandPrisonBuilder.Build();

        public static void Build() => IslandPrisonBuilder.Build();
    }
}

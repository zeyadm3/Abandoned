namespace Abandoned.EditorTools
{
    /// <summary>The Factory location's scene path and the entry point batch commands call; it builds the Iron Factory.</summary>
    public static class FactoryBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Factory.unity";
        public static void Build() => IronFactoryBuilder.Build();
    }
}

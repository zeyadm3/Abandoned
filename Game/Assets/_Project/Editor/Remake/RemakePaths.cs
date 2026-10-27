namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Where the 2026 remake's art lives (Docs/REMAKE_2026.md). Each area folder holds its FBX files, a Textures/
    /// folder (&lt;Set&gt;_BaseColor / _Normal / _Mask from Tools/Blender/remake/core/bake.py) and the Materials/ built
    /// from them. Shared materials (glass, emissive, screens, hair cards, decals) sit in Shared/.
    /// </summary>
    public static class RemakePaths
    {
        public const string Root = "Assets/_Project/Art/Remake";
        public const string Shared = Root + "/Shared";
        public const string Characters = Root + "/Characters";
        public const string Threats = Root + "/Threats";
        public const string Gear = Root + "/Gear";
        public const string Truck = Root + "/Truck";
        public const string Loot = Root + "/Loot";
        public const string Maps = Root + "/Maps";
        public const string Onboarding = Root + "/Onboarding";
        public const string Icons = Root + "/Icons";

        public static bool IsRemake(string assetPath) => assetPath.StartsWith(Root + "/");
        public static bool IsSkinned(string assetPath) => assetPath.StartsWith(Characters + "/") || assetPath.StartsWith(Threats + "/") ||
                                                          assetPath.StartsWith(Onboarding + "/Characters/");
        public static bool IsAnimated(string assetPath) => assetPath.StartsWith(Threats + "/") || assetPath.Contains("/Anim/");
        public static bool NeedsReadable(string assetPath) => assetPath.StartsWith(Loot + "/") || assetPath.StartsWith(Gear + "/") ||
                                                              assetPath.StartsWith(Maps + "/");

        /// <summary>The folder of the area an asset belongs to (the first folder under Root, plus one level for Maps).</summary>
        public static string AreaFolder(string assetPath)
        {
            string rest = assetPath.Substring(Root.Length + 1);
            string[] parts = rest.Split('/');
            int depth = parts[0] == "Maps" && parts.Length > 2 ? 2 : 1;
            return Root + "/" + string.Join("/", parts, 0, System.Math.Min(depth, parts.Length - 1));
        }
    }
}

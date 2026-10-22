namespace Abandoned.EditorTools
{
    /// <summary>
    /// Every Tools > Abandoned path and priority band, so the menu stays in the order Docs/TOOL_MENU.md lays out.
    /// Unity draws a separator wherever two neighbouring priorities differ by more than 10, and a submenu sorts by
    /// its items' priorities, so each group owns its own band: keep a new item inside its group's band.
    /// </summary>
    public static class ToolMenu
    {
        public const string Root = "Tools/Abandoned/";

        public const string Maps = Root + "Maps/";
        public const string Mall = Maps + "Mall/";
        public const string MallUpdates = Mall + "Updates/";
        public const string School = Maps + "School/";
        public const string Factory = Maps + "Iron Factory/";
        public const string Prison = Maps + "Island Prison/";
        public const string Hq = Maps + "HQ/";
        public const string HqUpdates = Hq + "Updates/";
        public const string Tutorial = Maps + "Tutorial House/";
        public const string TestMap = Maps + "Test Map/";
        public const string Hospital = Maps + "Hospital/";
        public const int HospitalP = 1700;
        public const string Motel = Maps + "Motel/";
        public const int MotelP = 1750;
        public const string PoliceStation = Maps + "Police Station/";
        public const int PoliceStationP = 1800;
        public const string FireStation = Maps + "Fire Station/";
        public const int FireStationP = 1850;
        public const string Skyscraper = Maps + "Skyscraper Restaurant/";
        public const int SkyscraperP = 1950;

        public const string Content = Root + "Content/";
        public const string Data = Content + "Data/";
        public const string Prefabs = Content + "Prefabs/";
        public const string Loot = Content + "Loot/";
        public const string Audio = Content + "Audio/";
        public const string Characters = Content + "Characters/";
        public const string Features = Content + "Features/";

        public const string Art = Root + "Art/";
        public const string UI = Root + "UI/";
        public const string Checks = Root + "Checks/";
        public const string Project = Root + "Project/";
        public const string Fixes = Project + "Fixes/";
        public const string Build = Root + "Build/";
        public const string Store = Root + "Store/";

        // Priority bands (Docs/TOOL_MENU.md). Within a band: +0 the main action, +20/+40 for later sections.
        public const int RebuildAllP = 0;
        public const int MallP = 1000, SchoolP = 1100, FactoryP = 1200, PrisonP = 1300, HqP = 1400, TutorialP = 1500, TestMapP = 1600;
        public const int DataP = 2000, PrefabsP = 2100, LootP = 2200, AudioP = 2300, CharactersP = 2400, FeaturesP = 2500;
        public const int ArtP = 3000, UiP = 4000, ChecksP = 5000, ProjectP = 6000, BuildP = 7000, StoreP = 8000;
    }
}

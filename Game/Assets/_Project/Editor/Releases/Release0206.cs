using Abandoned.Threats;
using UnityEditor;

namespace Abandoned.EditorTools
{
    /// <summary>0.20.6 threat forecast: the company upgrade (truck catalog, append only) and the forecast asset pointing at the threat catalog.</summary>
    public static class Release0206
    {
        [MenuItem(ToolMenu.Features + "0.20.6 Threat Forecast", priority = ToolMenu.FeaturesP + 19)]
        public static void Author()
        {
            CompanyContentBuilder.CreateTruckUpgrades();
            var forecast = ReleaseKit.Config<ThreatForecast>("Assets/_Project/Data/Threats/Resources", ThreatForecast.ResourcePath);
            forecast.EditorSetup(AssetDatabase.LoadAssetAtPath<ThreatCatalog>(ThreatContentBuilder.CatalogPath));
            EditorUtility.SetDirty(forecast);
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

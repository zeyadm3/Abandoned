using System.Collections.Generic;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The achievements (M9.5) as data in Data/Core/Resources/Achievements: created when missing, existing
    /// ones keep their inspector tuning. Steam API names match what the partner site will need.
    /// </summary>
    public static class AchievementsBuilder
    {
        public const string Folder = "Assets/_Project/Data/Core/Achievements";
        public const string CatalogPath = "Assets/_Project/Data/Core/Resources/Achievements.asset";

        private static readonly (string id, string name, string description, string stat, long threshold)[] All =
        {
            ("first_job", "First Day on the Job", "Finish a run.", Achievements.StatRuns, 1),
            ("made_it_out", "Made It Out", "Get on the truck before it leaves.", Achievements.StatEscapes, 1),
            ("career_salvager", "Career Salvager", "Make it out of 25 runs.", Achievements.StatEscapes, 25),
            ("quota_crusher", "Quota Crusher", "Meet the quota 10 times.", Achievements.StatQuotaMet, 10),
            ("six_figures", "Six Figures", "Haul $100,000 over your career.", Achievements.StatHaul, 100000),
            ("millionaire", "Millionaire", "Haul $1,000,000 over your career.", Achievements.StatHaul, 1000000),
            ("the_greed_item", "The Greed Item", "Get a jackpot out of a building.", Achievements.StatJackpots, 1),
            ("gravity_wins", "Gravity Wins", "Be there when 10 floors give way.", Achievements.StatCollapses, 10),
            ("occupational_hazard", "Occupational Hazard", "Die on the job.", Achievements.StatDeaths, 1),
            ("medic", "Medic", "Get a downed crewmate back on their feet.", Achievements.StatRevives, 1),
            ("timber", "Timber!", "Watch the Hunter fall through a floor.", Achievements.StatHunterFalls, 1),
            ("lights_out", "Lights Out", "Make it out of a job with the power off or after dark.", Achievements.StatDarkEscapes, 1),
        };

        [MenuItem("Tools/Abandoned/Build Achievements")]
        public static void CreateMissing()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Core", "Achievements");
            var list = new List<AchievementDefinition>();
            foreach ((string id, string name, string description, string stat, long threshold) in All)
            {
                string path = $"{Folder}/Achievement_{id}.asset";
                var a = AssetDatabase.LoadAssetAtPath<AchievementDefinition>(path);
                if (a == null)
                {
                    a = ScriptableObject.CreateInstance<AchievementDefinition>();
                    a.EditorSetup(id, name, description, stat, threshold, "ACH_" + id.ToUpperInvariant());
                    AssetDatabase.CreateAsset(a, path);
                }
                list.Add(a);
            }
            var catalog = SerializedWiring.LoadOrCreateAsset<AchievementCatalog>(CatalogPath);
            catalog.EditorSet(list);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}

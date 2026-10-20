using System;
using System.IO;
using System.Text;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.6 Steam app and release build. What needs the user (the Steam Direct fee, creating the app, the
    /// partner site, the review) can't be done here; this prepares everything around it: the achievements list
    /// for the partner site, the credits screen regenerated from Docs/ASSET_CREDITS.md, and a one-command App ID
    /// switch (<c>Tools/unity.sh set-app-id &lt;id&gt;</c>) after which <c>build-release</c> stops refusing.
    /// </summary>
    public static class Release0246
    {
        [MenuItem(ToolMenu.Store + "Prepare Steam Release", priority = ToolMenu.StoreP + 40)]
        public static void Author()
        {
            UiContentBuilder.CreateMissing(); // regenerates the credits screen (Noto fonts, voices)
            ExportAchievements();
            BuildScript.ApplyPlayerSettings();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        // Tools/Steam/achievements.csv: what the partner site's achievement page asks for, row by row.
        private static void ExportAchievements()
        {
            string dir = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Tools", "Steam");
            Directory.CreateDirectory(dir);
            var csv = new StringBuilder("api_name,display_name,description,stat,threshold,icon_achieved,icon_unachieved\n");
            foreach (AchievementDefinition a in Achievements.Catalog.Items)
            {
                if (a == null) continue;
                string Q(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
                csv.Append($"{a.SteamName},{Q(a.DisplayName)},{Q(a.Description)},{a.Stat},{a.Threshold},AchievementIcons/{a.SteamName}.png,AchievementIcons/{a.SteamName}_locked.png\n");
            }
            File.WriteAllText(Path.Combine(dir, "achievements.csv"), csv.ToString());
            Debug.Log($"[Steam0246] {Achievements.Catalog.Items.Count} achievements written to Tools/Steam/achievements.csv.");
        }

        /// <summary>
        /// Batch: <c>Tools/unity.sh set-app-id 1234560</c>. Sets the game's own App ID in NetworkConfig (and as the
        /// demo's store target), rewrites Game/steam_appid.txt to match, and saves.
        /// </summary>
        public static void SetAppIdAndExit() => ReleaseKit.RunAndExit(() =>
        {
            string arg = Environment.GetEnvironmentVariable("ABANDONED_APP_ID");
            if (!uint.TryParse(arg, out uint id) || id == 0 || id == 480) throw new ArgumentException($"Give a real App ID (got '{arg}').");
            Set(NetworkContentBuilder.ConfigPath, "<SteamAppId>k__BackingField", id);
            Set("Assets/_Project/Data/Core/Resources/DemoConfig.asset", "<StoreAppId>k__BackingField", id);
            NetworkContentBuilder.WriteSteamAppIdFile(id);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Steam0246] App ID set to {id}. build-release will now build depots.");
        });

        private static void Set(string path, string field, uint value)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) return;
            var so = new SerializedObject(asset);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) return;
            p.longValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }
    }
}

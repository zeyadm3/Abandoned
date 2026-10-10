using System;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What every 0.16.1+ version step shares: creating its tunables asset when missing, and running a step
    /// from batch mode (<c>Tools/unity.sh exec Abandoned.EditorTools.Release0161.AuthorAndExit</c>).
    /// </summary>
    public static class ReleaseKit
    {
        public const string ExtractionResources = "Assets/_Project/Data/Extraction/Resources";
        public const string EquipmentResources = "Assets/_Project/Data/Equipment/Resources";
        public const string GearPrefabsPath = EquipmentResources + "/GearPrefabs.asset";
        public const string GearPrefabFolder = "Assets/_Project/Prefabs/Equipment";

        /// <summary>Network prefabs added by 0.17+ steps that aren't gear (append only).</summary>
        public static readonly string[] ExtraNetworkPrefabs =
        {
            // @@netprefabs
        };

        /// <summary>
        /// Gear data after a step adds an item: the catalog (append only), the icons, the tool tunables, the
        /// gear prefab registry, NGO's prefab list and the sound bank's rows.
        /// </summary>
        public static void Gear()
        {
            Config<Abandoned.Equipment.ToolConfig>(EquipmentResources, Abandoned.Equipment.ToolConfig.ResourcePath);
            Config<Abandoned.Equipment.GearPrefabs>(EquipmentResources, Abandoned.Equipment.GearPrefabs.ResourcePath);
            EquipmentContentBuilder.CreateMissing();
            UiGearIconBuilder.Create();
            UiContentBuilder.BuildIcons();
            NetworkContentBuilder.RegisterNetworkPrefabs();
            SoundBankBuilder.CreateMissing();
            BuildScript.ApplyPlayerSettings();
        }

        /// <summary>Registers a gear prefab under its kind.</summary>
        public static void RegisterGear(Abandoned.Equipment.EquipmentKind kind, GameObject prefab)
        {
            var registry = Config<Abandoned.Equipment.GearPrefabs>(EquipmentResources, Abandoned.Equipment.GearPrefabs.ResourcePath);
            registry.EditorSet(kind, prefab.GetComponent<Unity.Netcode.NetworkObject>());
            EditorUtility.SetDirty(registry);
        }

        /// <summary>The config at <paramref name="folder"/>/<paramref name="name"/>.asset, made with code defaults when missing.</summary>
        public static T Config<T>(string folder, string name) where T : ScriptableObject
        {
            PolishAssets.EnsureFolder(folder);
            return SerializedWiring.LoadOrCreateAsset<T>($"{folder}/{name}.asset");
        }

        public static void RunAndExit(Action step)
        {
            try
            {
                step();
                AssetDatabase.SaveAssets();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }


        /// <summary>Batch: only stamp the version into Player Settings (a step with no content of its own).</summary>
        public static void ApplySettingsAndExit() => RunAndExit(BuildScript.ApplyPlayerSettings);
    }
}

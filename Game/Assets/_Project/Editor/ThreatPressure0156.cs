using Abandoned.Threats;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.15.6 threat tuning and additive spawn coverage; run after original threat generators.</summary>
    public static class ThreatPressure0156
    {
        private const string Data = "Assets/_Project/Data/Threats/";
        private const string SpawnGroup = "_THREAT_PRESSURE_0156";

        public static void Apply()
        {
            var pacing = Edit(Data + "Resources/ThreatPacingConfig.asset");
            Set(pacing, "OpeningDelay", 12f); Set(pacing, "OpeningCount", 2); Set(pacing, "OpeningStagger", 4f);
            Set(pacing, "SpawnRetry", 3f); Set(pacing, "FirstJobOpeningDelay", 20f);
            pacing.FindProperty("<ArrivalInterval>k__BackingField").vector2Value = new Vector2(28f, 42f);
            Set(pacing, "BaseAlive", 4); Set(pacing, "PerPlayer", 1); Set(pacing, "MaxAlive", 10);
            Set(pacing, "DriftAfter", 10f); Set(pacing, "DriftRange", 16f); Set(pacing, "DriftScatter", 9f);
            Set(pacing, "RelocateDistance", 34f); Set(pacing, "RelocateAfter", 55f);
            Array(pacing, "LevelGapScale", 1.15f, 1f, 0.75f);
            Array(pacing, "LevelOpeningScale", 1.2f, 1f, 0.75f);
            Array(pacing, "LevelDamageScale", 0.9f, 1.1f, 1.25f);
            IntArray(pacing, "LevelAliveBonus", -1, 0, 1); IntArray(pacing, "LevelOpeningExtras", 0, 1, 2);
            pacing.ApplyModifiedPropertiesWithoutUndo();

            var blind = Edit(Data + "BlindOneConfig.asset");
            Set(blind, "Hearing", 1.35f); Set(blind, "WallDamping", 0.72f); Set(blind, "Memory", 12f);
            Set(blind, "WalkSpeed", 2.2f); Set(blind, "HuntSpeed", 5.7f); Set(blind, "HuntThreshold", 0.32f);
            Set(blind, "HuntNoiseCount", 2); Set(blind, "HuntWindow", 6f); Set(blind, "HuntForget", 9f);
            Set(blind, "Linger", 2.5f); Set(blind, "AttackRange", 1.35f); Set(blind, "AttackPause", 1.6f);
            blind.ApplyModifiedPropertiesWithoutUndo();
            var hunter = Edit(Data + "HunterConfig.asset");
            Set(hunter, "PatrolSpeed", 2.7f); Set(hunter, "ChaseSpeed", 5.9f); Set(hunter, "SightRange", 30f);
            Set(hunter, "SightAngle", 140f); Set(hunter, "CrouchSightRange", 10f); Set(hunter, "CloseSense", 4f);
            Set(hunter, "LoseAfter", 5f); Set(hunter, "SearchSeconds", 12f); Set(hunter, "AttackRange", 1.4f);
            hunter.ApplyModifiedPropertiesWithoutUndo();
            var stalker = Edit(Data + "StalkerConfig.asset");
            Set(stalker, "FollowDistance", 7f); Set(stalker, "FollowSpeed", 4.2f); Set(stalker, "RushSpeed", 7.2f);
            Set(stalker, "IsolationRadius", 11f); Set(stalker, "RushAfter", 5.5f); Set(stalker, "AttackRange", 1.35f);
            Set(stalker, "RetargetInterval", 3f); stalker.ApplyModifiedPropertiesWithoutUndo();

            Definition("BlindOne", 0, 1.4f, 70f, 2.2f, 5.7f, 1.35f, 1.8f, 26f);
            Definition("Stalker", 0, 1.15f, 80f, 4.2f, 7.2f, 1.35f, 2f, 28f);
            Definition("Collector", 0, 0f, 0f, 2.8f, 5f, 1.2f, 2.5f, 22f);
            Definition("Crawlers", 0, 1.1f, 18f, 1.9f, 4.8f, 1.65f, 2.5f, 24f);
            Definition("Weight", 1, 0f, 0f, 1.15f, 1.65f, 1.25f, 2.5f, 28f);
            Definition("Thing", 2, 0f, 70f, 1.8f, 5.8f, 1.35f, 3.2f, 26f);
            Definition("Hunter", 2, 0f, 1000f, 2.7f, 5.9f, 1.4f, 2.5f, 30f);
            var final = Edit(Data + "LastHunterDefinition.asset");
            Set(final, "SpawnWarningSeconds", 15f); final.ApplyModifiedPropertiesWithoutUndo();
            var thing = Edit(Data + "ThingDefinition.asset");
            Set(thing, "SpecialInterval", 10f); thing.ApplyModifiedPropertiesWithoutUndo();
            var danger = Edit("Assets/_Project/Data/Extraction/DangerConfig.asset");
            Set(danger, "LevelInterval", 120f); danger.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[ThreatPressure0156] Opening 2/3/4 after 14.4/12/9s (first job20s), arrivals28-42s, solo caps4/5/6; warning15s.");
        }

        public static void AuthorSpawns(Transform mall)
        {
            Transform previous = mall.Find(SpawnGroup);
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            var group = new GameObject(SpawnGroup).transform;
            group.SetParent(mall, false);
            // Existing protected store aisles and balcony lanes stay empty: these are markers only.
            Vector2[] aisles = { new(6,6), new(18,6), new(38,6), new(50,6), new(6,22), new(14,22),
                new(42,22), new(50,22), new(18,18), new(38,18), new(18,30), new(38,30), new(30,34) };
            int count = 0;
            for (int floor = 0; floor < 2; floor++)
                foreach (Vector2 at in aisles) Point(group, new Vector3(at.x, floor * 4f, at.y), ref count);
            foreach (float z in new[] { 30f, 34f, 42f, 46f }) Point(group, new Vector3(42f, -4f, z), ref count);
            ThreatCatalog catalog = AssetDatabase.LoadAssetAtPath<ThreatCatalog>(Data + "ThreatCatalog.asset");
            foreach (ThreatDirector director in Object.FindObjectsByType<ThreatDirector>(FindObjectsSortMode.None))
            {
                if (director.gameObject.scene != mall.gameObject.scene) continue;
                director.SetupCatalog(catalog);
                var serialized = new SerializedObject(director);
                serialized.FindProperty("minSpawnDistance").floatValue = 14f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(director);
            }
            Debug.Log($"[ThreatPressure0156] Added {count} route spawn markers; all placements remain host-selected and hidden.");
        }

        private static void Point(Transform parent, Vector3 at, ref int count)
        {
            var point = new GameObject($"ThreatPressure_{count:00}");
            point.transform.SetParent(parent, false); point.transform.position = at;
            point.AddComponent<ThreatSpawnPoint>(); count++;
        }
        private static void Definition(string name, int danger, float opening, float damage, float walk,
            float chase, float range, float cooldown, float sense)
        {
            var definition = Edit(Data + name + "Definition.asset");
            Set(definition, "MinimumDanger", danger); Set(definition, "OpeningWeight", opening);
            Set(definition, "Damage", damage); Set(definition, "WalkSpeed", walk); Set(definition, "ChaseSpeed", chase);
            Set(definition, "AttackRange", range); Set(definition, "AttackCooldown", cooldown); Set(definition, "SenseRange", sense);
            Set(definition, "SpawnWarningSeconds", 15f); definition.ApplyModifiedPropertiesWithoutUndo();
        }
        private static SerializedObject Edit(string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null) throw new System.InvalidOperationException("Missing threat tuning asset: " + path);
            return new SerializedObject(asset);
        }
        private static void Set(SerializedObject value, string field, float number) =>
            value.FindProperty("<" + field + ">k__BackingField").floatValue = number;
        private static void Set(SerializedObject value, string field, int number) =>
            value.FindProperty("<" + field + ">k__BackingField").intValue = number;
        private static void Array(SerializedObject value, string field, params float[] numbers)
        {
            SerializedProperty list = value.FindProperty("<" + field + ">k__BackingField"); list.arraySize = numbers.Length;
            for (int i = 0; i < numbers.Length; i++) list.GetArrayElementAtIndex(i).floatValue = numbers[i];
        }
        private static void IntArray(SerializedObject value, string field, params int[] numbers)
        {
            SerializedProperty list = value.FindProperty("<" + field + ">k__BackingField"); list.arraySize = numbers.Length;
            for (int i = 0; i < numbers.Length; i++) list.GetArrayElementAtIndex(i).intValue = numbers[i];
        }
    }
}

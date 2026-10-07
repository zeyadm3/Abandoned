using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class HorrorContentBuilder
    {
        public const string Path="Assets/_Project/Data/Extraction/HorrorConfig.asset";
        public static void Create()
        {
            CustomMallArt.Prepare();
            var config=SerializedWiring.LoadOrCreateAsset<HorrorConfig>(Path);
            var data=new SerializedObject(config);
            data.FindProperty("<ScareModel>k__BackingField").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Custom/Environment/MannequinWrong.fbx");
            data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(config);
            var danger=SerializedWiring.LoadOrCreateAsset<DangerConfig>(NetworkContentBuilder.DangerConfigPath);
            var d=new SerializedObject(danger);
            d.FindProperty("<LevelInterval>k__BackingField").floatValue=110;
            d.FindProperty("<AgingInterval>k__BackingField").floatValue=9;
            d.FindProperty("<AgingDamage>k__BackingField").floatValue=0.10f;
            d.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(danger);
            var root=PrefabUtility.LoadPrefabContents(NetworkContentBuilder.RunStatePrefabPath);
            var horror=root.GetComponent<RunHorrorDirector>()??root.AddComponent<RunHorrorDirector>();
            SerializedWiring.Set(horror,"config",config);
            PrefabUtility.SaveAsPrefabAsset(root,NetworkContentBuilder.RunStatePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        }
    }
}

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
            const string effects="Assets/_Project/Prefabs/Effects";
            if(!AssetDatabase.IsValidFolder(effects)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs","Effects");
            GameObject shape=CustomMallArt.Place("MannequinWrong",null,Vector3.zero,Quaternion.identity);
            data.FindProperty("<ScareModel>k__BackingField").objectReferenceValue=PrefabUtility.SaveAsPrefabAsset(shape,effects+"/HorrorScareModel.prefab");
            Object.DestroyImmediate(shape);
            data.FindProperty("<ScareInterval>k__BackingField").floatValue=80f;
            data.FindProperty("<PressureInterval>k__BackingField").floatValue=18f;
            data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(config);
            var danger=SerializedWiring.LoadOrCreateAsset<DangerConfig>(NetworkContentBuilder.DangerConfigPath);
            var d=new SerializedObject(danger);
            d.FindProperty("<LevelInterval>k__BackingField").floatValue=110;
            // QA D-02: at 9 s / 10 % a danger-3 building lost ~20 sections a minute on top of faster decay.
            d.FindProperty("<AgingInterval>k__BackingField").floatValue=15;
            d.FindProperty("<AgingDamage>k__BackingField").floatValue=0.08f;
            d.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(danger);
            var root=PrefabUtility.LoadPrefabContents(NetworkContentBuilder.RunStatePrefabPath);
            var horror=root.GetComponent<RunHorrorDirector>()??root.AddComponent<RunHorrorDirector>();
            SerializedWiring.Set(horror,"config",config);
            PrefabUtility.SaveAsPrefabAsset(root,NetworkContentBuilder.RunStatePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        }
    }
}

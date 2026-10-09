using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MallGameplayData
    {
        public static void Apply()
        {
            var config=AssetDatabase.LoadAssetAtPath<Abandoned.Extraction.HorrorConfig>(HorrorContentBuilder.Path);
            if(config==null)throw new System.InvalidOperationException("Build horror content before the mall.");
            var data=new SerializedObject(config);
            data.FindProperty("<Building>k__BackingField").boundsValue=MallLayout.BuildingBounds;
            data.FindProperty("<ArrivalFallback>k__BackingField").vector3Value=new Vector3(28,0,8);
            data.FindProperty("<ArrivalFigure>k__BackingField").vector3Value=new Vector3(26,4,34);
            data.FindProperty("<ArrivalDoor>k__BackingField").vector3Value=new Vector3(40,4,34);
            data.FindProperty("<RoarPoint>k__BackingField").vector3Value=new Vector3(28,5.8f,24);
            data.FindProperty("<SectorCell>k__BackingField").vector3Value=new Vector3(12,4,12);
            data.FindProperty("<Sectors>k__BackingField").intValue=6;
            data.FindProperty("<SectorsLitAtMaxDanger>k__BackingField").intValue=2;
            data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(config);
        }
    }
}

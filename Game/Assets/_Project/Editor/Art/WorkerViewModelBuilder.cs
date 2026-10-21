using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.WorkerVisualMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>First-person sleeves and gloves: the jacket fabric and tape, and the worker's own glove model.</summary>
    public static class WorkerViewModelBuilder
    {
        private const string Folder = "Assets/_Project/Prefabs/Resources/Characters";

        public static void Save()
        {
            PolishAssets.EnsureFolder(Folder);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                string suffix = i == 0 ? "L" : "R";
                Transform sleeve = new GameObject("CrewSleeve" + suffix).transform;
                F.Form("TailoredSleeve", sleeve, Vector3.zero, new Vector3(.13f, 1f, .14f), CharacterSurfaceLibrary.Get("CrewFabric"), "ViewSleeve");
                Box("SleeveSeam", sleeve, new Vector3(side * .057f, -.02f, .004f), new Vector3(.005f, .84f, .006f), CharacterSurfaceLibrary.Get("CrewTrim"), false);
                for (int fold = 0; fold < 5; fold++)
                {
                    float y = .12f + fold * .055f;
                    GameObject ring = F.Form("CompressedClothFold", sleeve, new Vector3(0f, y, .001f),
                        new Vector3(.107f - fold * .003f, .025f, .116f - fold * .003f), CharacterSurfaceLibrary.Get("CrewFabric"), "Tube");
                    ring.transform.localRotation = Quaternion.Euler(0f, 0f, side * (fold % 2 == 0 ? 3f : -3f));
                }
                F.Form("ReflectiveWristBand", sleeve, new Vector3(0f, .395f, 0f), new Vector3(.099f, .055f, .106f), CharacterSurfaceLibrary.Get("Tape"), "Tube");
                F.Form("KnittedCuff", sleeve, new Vector3(0f, .465f, 0f), new Vector3(.091f, .09f, .096f), CharacterSurfaceLibrary.Get("CrewTrim"), "Tube");
                F.Merge(sleeve, "ViewSleeve" + suffix + "V3");
                PrefabUtility.SaveAsPrefabAsset(sleeve.gameObject, Folder + "/CrewSleeve" + suffix + ".prefab");
                Object.DestroyImmediate(sleeve.gameObject);

                Transform glove = new GameObject("CrewGlove" + suffix).transform;
                // The worker's own glove (Crew_GloveL/R: origin at the wrist, fingers along -Y, palm toward +Z).
                Transform fit = Group("Fit", glove);
                fit.localPosition = new Vector3(0f, 0f, -.035f);
                fit.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.back);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>($"{WorkerVisualBuilder.ArtFolder}/Crew_Glove{suffix}.fbx");
                if (model != null) WorkerVisualBuilder.Dress(Object.Instantiate(model, fit, false));
                else Debug.LogError("Crew glove model missing (run Tools/Blender/crew/crew_export.py).");
                F.Merge(glove, "ViewGlove" + suffix + "V4");
                PrefabUtility.SaveAsPrefabAsset(glove.gameObject, Folder + "/CrewGlove" + suffix + ".prefab");
                Object.DestroyImmediate(glove.gameObject);
            }
        }
    }
}

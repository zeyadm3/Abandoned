using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The remade vans and the Island Prison launch (Tools/Blender/remake/vehicles). New maps get the vans through
    /// <see cref="Vehicles.Dress"/>, which prefers them; this refits the saved maps: every Kenney van or delivery van is
    /// swapped for the remade one at the same footprint, and the launch's code-built hull and wheelhouse dressing are
    /// replaced (the wheelhouse's collider block stays). Colliders and gameplay components are untouched.
    /// </summary>
    public static class VehicleRemake
    {
        public const string Folder = RemakePaths.Root + "/Vehicles";

        public static GameObject Model(string name) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/Vehicle_{name}.fbx");

        [MenuItem(ToolMenu.Art + "Remake: Refit Vans and the Launch in Saved Maps", priority = ToolMenu.ArtP + 106)]
        public static void RefitSaved()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.ImportAsset(Folder, ImportAssetOptions.ImportRecursive);
            RemakeMaterials.EnsureModelMaterials(Folder);
            foreach (string path in AvOverhaul.MapScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int vans = RefitVans(scene);
                bool boat = false;
                var launches = new List<Transform>();
                foreach (GameObject root in scene.GetRootGameObjects())
                    launches.AddRange(root.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name == "Truck" && t.parent != null && t.parent.name == IslandBoat.GroupName));
                foreach (Transform launch in launches)
                    if (DressBoat(launch)) boat = true;
                if (vans == 0 && !boat) continue;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Remake] {scene.name}: {vans} van(s) refitted{(boat ? ", the launch refitted" : "")}.");
            }
        }

        /// <summary>Every placed Kenney van and delivery van, swapped for the remade one at the same footprint.</summary>
        public static int RefitVans(Scene scene)
        {
            var old = new HashSet<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (MeshFilter f in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    string path = f.sharedMesh != null ? AssetDatabase.GetAssetPath(f.sharedMesh) : "";
                    if (path != Vehicles.Folder + Vehicles.Van + ".fbx" && path != Vehicles.Folder + Vehicles.Delivery + ".fbx") continue;
                    GameObject instance = PrefabUtility.GetOutermostPrefabInstanceRoot(f.gameObject);
                    if (instance != null) old.Add(instance);
                }
            int done = 0;
            foreach (GameObject instance in old)
            {
                string kind = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(instance)).EndsWith(Vehicles.Delivery + ".fbx")
                    ? Vehicles.Delivery : Vehicles.Van;
                GameObject model = Model(kind);
                // The fitted holder ModelFit made ("Model"), and the object it sits in.
                Transform holder = instance.transform.parent;
                if (model == null || holder == null || holder.parent == null) continue;
                Transform parent = holder.parent;
                Bounds was = ModelFit.LocalBounds(holder.gameObject, parent);
                Object.DestroyImmediate(holder.gameObject);
                ModelFit.Place(model, parent, was.center, was.size, 0f, standOnFloor: true);
                done++;
            }
            return done;
        }

        /// <summary>The launch (IslandBoat's "Truck" root): the remade hull in the swaying HullArt group, the wheelhouse
        /// dressing over the solid block. False when the models aren't built yet.</summary>
        public static bool DressBoat(Transform root)
        {
            GameObject hull = Model("boat_hull"), house = Model("boat_wheelhouse");
            Transform hullArt = root.Find("HullArt");
            if (hull == null || house == null || hullArt == null) return false;
            for (int i = hullArt.childCount - 1; i >= 0; i--) Object.DestroyImmediate(hullArt.GetChild(i).gameObject);
            Place(hull, hullArt);
            foreach (string name in new[] { "WheelhouseRoof", "Window_F", "Window_S", "Mast", "MastLamp", "Yard", "Funnel", "Lifebelt", "RM_Wheelhouse" })
                foreach (Transform t in root.Cast<Transform>().Where(c => c.name == name).ToList())
                    Object.DestroyImmediate(t.gameObject);
            Transform block = root.Find("Wheelhouse");
            if (block != null)
            {
                Object.DestroyImmediate(block.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(block.GetComponent<MeshFilter>());
            }
            Place(house, root).name = "RM_Wheelhouse";
            return true;
        }

        private static GameObject Place(GameObject model, Transform parent)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            foreach (Collider c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            return go;
        }
    }
}

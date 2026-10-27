using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Company;
using Abandoned.Extraction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The remade collection truck (Tools/Blender/remake/truck, Art/Remake/Truck): the truck cosmetic catalog
    /// (Data/Company/Resources/TruckCosmetics), each upgrade's visual, the garage's preview truck, and the body itself on
    /// the trucks of the saved maps. The truck's functional parts (cargo trigger, deck, ramp, ignition, display, lights,
    /// hinged rear doors) stay the ones CollectionTruck0157 fits; only the art group changes.
    /// </summary>
    public static class TruckRemakeBuilder
    {
        public const string DataFolder = "Assets/_Project/Data/Company/TruckCosmetics";
        public const string CatalogPath = "Assets/_Project/Data/Company/Resources/TruckCosmetics.asset";
        public const string PreviewPath = "Assets/_Project/Prefabs/Resources/TruckPreview.prefab";

        public static bool Available => AssetDatabase.LoadAssetAtPath<GameObject>(Part("Body")) != null;

        private static string Part(string name) => $"{RemakePaths.Truck}/Truck_{name}.fbx";

        private readonly struct Item
        {
            public readonly string Id, Name, About, Part;
            public readonly TruckCosmeticSlot Slot;
            public readonly int Price, Level;
            public readonly bool Stock;
            public readonly Color Colour;
            public readonly float Gloss;
            public readonly SoundId Horn;

            public Item(TruckCosmeticSlot slot, string id, string name, string about, int price = 0, int level = 1, bool stock = false,
                string part = null, Color colour = default, float gloss = .45f, SoundId horn = SoundId.Horn)
            {
                Slot = slot; Id = id; Name = name; About = about; Price = price; Level = level; Stock = stock; Part = part;
                Colour = colour == default ? Color.white : colour; Gloss = gloss; Horn = horn;
            }
        }

        // Append only: the order is the network index (64 at most).
        private static readonly Item[] Items =
        {
            new(TruckCosmeticSlot.Paint, "paint_company", "Company Orange", "The colour on every invoice.", stock: true, colour: new Color(.86f, .42f, .1f)),
            new(TruckCosmeticSlot.Paint, "paint_white", "Fleet White", "Shows every scratch. Shows every stain.", 1500, colour: new Color(.86f, .86f, .84f), gloss: .55f),
            new(TruckCosmeticSlot.Paint, "paint_red", "Fire Engine Red", "Gloss red, borrowed from the station's spare can.", 2500, 2, colour: new Color(.62f, .06f, .05f), gloss: .7f),
            new(TruckCosmeticSlot.Paint, "paint_navy", "Midnight Navy", "For the night jobs.", 2500, 2, colour: new Color(.1f, .14f, .3f), gloss: .55f),
            new(TruckCosmeticSlot.Paint, "paint_green", "Forest Green", "Matte. Hides in the trees, which helps nobody.", 2500, 3, colour: new Color(.14f, .26f, .14f), gloss: .2f),
            new(TruckCosmeticSlot.Paint, "paint_yellow", "School Bus Yellow", "Every monster can see you coming.", 3000, 3, colour: new Color(.93f, .68f, .06f), gloss: .5f),
            new(TruckCosmeticSlot.Paint, "paint_black", "Gloss Black", "Like a hearse. On purpose.", 4000, 5, colour: new Color(.04f, .04f, .045f), gloss: .85f),
            new(TruckCosmeticSlot.Paint, "paint_matteblack", "Matte Black", "No shine, no glare, no excuses.", 4000, 5, colour: new Color(.06f, .06f, .065f), gloss: .1f),
            new(TruckCosmeticSlot.Paint, "paint_teal", "Harbour Teal", "From a dock crew's truck.", 3500, 4, colour: new Color(.08f, .42f, .44f), gloss: .5f),
            new(TruckCosmeticSlot.Paint, "paint_purple", "Royal Purple", "Ten quotas met. The boss almost smiled.", 6000, 8, colour: new Color(.34f, .14f, .48f), gloss: .7f),
            new(TruckCosmeticSlot.Paint, "paint_rust", "Rust Bucket", "Not paint. Just the truck, unprotected.", 1000, 1, colour: new Color(.42f, .2f, .1f), gloss: .1f),
            new(TruckCosmeticSlot.Paint, "paint_gold", "Gold Flake", "A million hauled. Spend it here.", 15000, 12, colour: new Color(.9f, .68f, .22f), gloss: .9f),
            new(TruckCosmeticSlot.Livery, "livery_stripes", "Racing Stripes", "Three stripes down the box. Not faster.", 2000, 2, part: "Livery_stripes"),
            new(TruckCosmeticSlot.Livery, "livery_flames", "Flames", "Hand-cut flames. Hand-cut by someone shaking.", 3500, 4, part: "Livery_flames"),
            new(TruckCosmeticSlot.Livery, "livery_hazard", "Hazard Chevrons", "Hazard stripes all the way round. Accurate.", 2500, 3, part: "Livery_hazard"),
            new(TruckCosmeticSlot.Livery, "livery_camo", "Camouflage", "Army surplus wrap. The monsters aren't fooled.", 3000, 4, part: "Livery_camo"),
            new(TruckCosmeticSlot.Livery, "livery_grime", "Road Grime", "Never washed. Now it's a look.", 800, 1, part: "Livery_grime"),
            new(TruckCosmeticSlot.Livery, "livery_company", "Company Panel", "The company's colours, framed. No name, they never paid for it.", 1200, 1, part: "Livery_company"),
            new(TruckCosmeticSlot.Bumper, "bumper_bullbar", "Chrome Bull Bar", "Pushes nothing out of the way. Looks like it would.", 4000, 3, part: "Bumper_bullbar"),
            new(TruckCosmeticSlot.Bumper, "bumper_pushbar", "Push Bar", "Rubber-faced, for nudging other trucks. Allegedly.", 3000, 2, part: "Bumper_pushbar"),
            new(TruckCosmeticSlot.Bumper, "bumper_plough", "Snow Plough", "For the Michigan jobs. The plough is cosmetic. The snow isn't.", 6000, 6, part: "Bumper_plough"),
            new(TruckCosmeticSlot.Roof, "roof_beacons", "Amber Beacons", "Two spinning beacons. They don't spin. They glow.", 2500, 2, part: "Roof_beacons"),
            new(TruckCosmeticSlot.Roof, "roof_rack", "Roof Rack", "A rack with a jerry can and a tarp on it. Both empty.", 3000, 3, part: "Roof_rack"),
            new(TruckCosmeticSlot.Wheels, "rims_steel", "Steel Wheels", "Pressed steel, painted, chipped.", stock: true, part: "Rims_steel"),
            new(TruckCosmeticSlot.Wheels, "rims_chrome", "Chrome Wheels", "Ten-spoke chrome. Polished once.", 5000, 4, part: "Rims_chrome"),
            new(TruckCosmeticSlot.Wheels, "rims_black", "Black Alloys", "Six-spoke black. For the matte trucks.", 4500, 4, part: "Rims_black"),
            new(TruckCosmeticSlot.Wheels, "rims_gold", "Gold Wheels", "You've made it. The truck hasn't.", 12000, 10, part: "Rims_gold"),
            new(TruckCosmeticSlot.Mudflaps, "flaps_rubber", "Rubber Mud Flaps", "Black rubber, as issued.", stock: true, part: "Mudflaps_rubber"),
            new(TruckCosmeticSlot.Mudflaps, "flaps_chrome", "Chrome Mud Flaps", "Mirror finish, nothing to see in them.", 1500, 2, part: "Mudflaps_chrome"),
            new(TruckCosmeticSlot.Mudflaps, "flaps_hazard", "Hazard Mud Flaps", "Yellow and black, for the reversing.", 1200, 1, part: "Mudflaps_hazard"),
            new(TruckCosmeticSlot.Ornament, "orn_bulldog", "Chrome Bulldog", "A bulldog on the cab roof. Loyal to nobody.", 2000, 2, part: "Ornament_bulldog"),
            new(TruckCosmeticSlot.Ornament, "orn_skull", "Bone Skull", "Plastic. Probably.", 2500, 3, part: "Ornament_skull"),
            new(TruckCosmeticSlot.Ornament, "orn_eagle", "Chrome Eagle", "Wings out, over the windscreen.", 3000, 4, part: "Ornament_eagle"),
            new(TruckCosmeticSlot.Ornament, "orn_bobble", "Dashboard Bobblehead", "A tiny worker nodding at everything you say.", 800, 1, part: "Ornament_bobblehead"),
            new(TruckCosmeticSlot.Horn, "horn_stock", "Stock Horn", "Two reeds, out of tune.", stock: true, horn: SoundId.Horn),
            new(TruckCosmeticSlot.Horn, "horn_air", "Air Horn", "A blast they hear three floors down.", 2500, 2, horn: SoundId.HornAir),
            new(TruckCosmeticSlot.Horn, "horn_train", "Train Horn", "Three chimes. The neighbours have opinions.", 5000, 5, horn: SoundId.HornTrain),
            new(TruckCosmeticSlot.Horn, "horn_goose", "Goose Horn", "Honk honk. Nobody takes you seriously now.", 1500, 1, horn: SoundId.HornGoose),
            new(TruckCosmeticSlot.Horn, "horn_deep", "Ship's Horn", "Off a tug in the bay. Rattles the windows.", 6000, 6, horn: SoundId.HornDeep),
        };

        [MenuItem(ToolMenu.Data + "Create Truck Cosmetics (remake)", priority = ToolMenu.DataP + 9)]
        public static void Build()
        {
            if (!Available)
            {
                Debug.LogWarning("[Remake] no remade truck yet (run Tools/Blender/remake/truck/truck.py).");
                return;
            }
            PolishAssets.EnsureFolder(DataFolder);
            RemakeMaterials.EnsureModelMaterials(RemakePaths.Truck);
            PolishAssets.EnsureFolder("Assets/_Project/Data/Company/Resources");
            var list = new List<TruckCosmeticDefinition>();
            foreach (Item item in Items)
            {
                string path = $"{DataFolder}/TruckCosmetic_{item.Id}.asset";
                var d = AssetDatabase.LoadAssetAtPath<TruckCosmeticDefinition>(path);
                if (d == null)
                {
                    d = ScriptableObject.CreateInstance<TruckCosmeticDefinition>();
                    AssetDatabase.CreateAsset(d, path);
                }
                GameObject part = item.Part != null ? AssetDatabase.LoadAssetAtPath<GameObject>(Part(item.Part)) : null;
                d.EditorSetup(item.Id, item.Name, item.About, item.Slot, item.Stock, item.Price, item.Level, item.Colour, item.Gloss, part, item.Horn);
                EditorUtility.SetDirty(d);
                list.Add(d);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<TruckCosmeticCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TruckCosmeticCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.EditorSet(list);
            EditorUtility.SetDirty(catalog);

            // Each owned upgrade shows on the truck.
            foreach (string guid in AssetDatabase.FindAssets("t:TruckUpgradeDefinition"))
            {
                var u = AssetDatabase.LoadAssetAtPath<TruckUpgradeDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (u == null) continue;
                u.EditorSetVisual(AssetDatabase.LoadAssetAtPath<GameObject>(Part("Up_" + u.Id)));
                EditorUtility.SetDirty(u);
            }
            SavePreview();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Remake] truck cosmetics: {list.Count}.");
        }

        /// <summary>The remade body under `art` (a truck's art group) plus its TruckLook; returns false without the art.</summary>
        public static bool Install(Transform truckRoot, Transform art, bool preview = false)
        {
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(Part("Body"));
            if (body == null) return false;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(body, art);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            Transform parts = GreyboxFactory.Group("TruckFittings", truckRoot);
            var look = truckRoot.GetComponent<TruckLook>();
            if (look == null) look = truckRoot.gameObject.AddComponent<TruckLook>();
            look.EditorSetup(renderers, parts, preview);
            return true;
        }

        private static void SavePreview()
        {
            PolishAssets.EnsureFolder("Assets/_Project/Prefabs/Resources");
            CollectionTruckMaterials.Prepare();
            var root = new GameObject("TruckPreview");
            Transform art = GreyboxFactory.Group("Art", root.transform);
            Install(root.transform, art, true);
            // The hinged rear doors are the game's own; the preview shows them shut.
            CollectionTruckCargoRedo.Doors(GreyboxFactory.Group("Doors", art), null);
            PrefabUtility.SaveAsPrefabAsset(root, PreviewPath);
            Object.DestroyImmediate(root);
        }

        /// <summary>Refits the remade truck on every saved map that has one (boats keep their hull).</summary>
        [MenuItem(ToolMenu.Art + "Remake: Refit Trucks in Saved Maps", priority = ToolMenu.ArtP + 101)]
        public static void RefitSavedMaps()
        {
            if (!Available) return;
            string[] scenes = { "Mall", "School", "Factory", "Tutorial", "Skyscraper", "Hospital", "Motel", "PoliceStation", "FireStation" };
            foreach (string name in scenes)
            {
                string path = $"Assets/_Project/Scenes/{name}.unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int refitted = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                foreach (TruckCargo cargo in root.GetComponentsInChildren<TruckCargo>(true))
                {
                    if (new SerializedObject(cargo).FindProperty("boat") is SerializedProperty boat && boat.boolValue) continue;
                    CollectionTruck0157.Upgrade(cargo.transform);
                    refitted++;
                }
                if (refitted > 0) EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Remake] {name}: {refitted} truck(s) refitted.");
            }
        }
    }
}

using System.Collections.Generic;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Original Blender environment kit; authored in metres with floor/base-centre pivots.</summary>
    public static class CustomMallArt
    {
        public const string Folder = "Assets/_Project/Art/Custom/Environment";
        private static readonly Dictionary<string, Material> Materials = new();
        private static readonly Dictionary<string, Color> Palette = new()
        {
            ["Concrete"] = new(.35f, .36f, .34f), ["Plaster"] = new(.49f, .49f, .43f),
            ["Tile"] = new(.43f, .47f, .45f), ["Trim"] = new(.18f, .24f, .23f),
            ["Rust"] = new(.30f, .19f, .13f), ["Metal"] = new(.25f, .28f, .28f),
            ["Rubber"] = new(.07f, .075f, .07f), ["Wood"] = new(.30f, .24f, .16f),
            ["Glass"] = new(.22f, .34f, .34f), ["Ceiling"] = new(.49f, .47f, .40f),
            ["Paper"] = new(.61f, .57f, .43f), ["Cloth"] = new(.26f, .28f, .24f),
            ["Mould"] = new(.11f, .17f, .10f), ["Stain"] = new(.18f, .18f, .12f),
            ["Water"] = new(.12f, .20f, .21f), ["Dirt"] = new(.22f, .23f, .16f),
            ["LightAmber"] = new(.68f, .50f, .27f), ["LightRed"] = new(.51f, .055f, .025f),
            ["Chalk"] = new(.64f, .61f, .48f), ["Black"] = new(.04f, .045f, .04f),
            ["Brass"] = new(.42f, .34f, .16f),
        };

        public static void Prepare()
        {
            PolishAssets.EnsureFolder(Folder + "/Materials");
            Materials.Clear();
            foreach (string key in Palette.Keys) Material(key);
        }

        public static Material Material(string key)
        {
            key = key.Replace("ENV_", "").Replace(" (Instance)", "");
            if (Materials.TryGetValue(key, out Material cached) && cached != null) return cached;
            string path = $"{Folder}/Materials/ENV_{key}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                PolishAssets.EnsureFolder(Folder + "/Materials");
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "ENV_" + key };
                AssetDatabase.CreateAsset(material, path);
            }
            Configure(material, key);
            Materials[key] = material;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void Configure(Material material, string key)
        {
            key = key.Replace("ENV_", "").Replace(" (Instance)", "");
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/Textures/ENV_{key}.png");
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", texture != null ? Color.white : Palette.GetValueOrDefault(key, Color.gray));
            material.SetFloat("_Smoothness", key is "Water" or "Glass" ? .76f : key is "Metal" or "Brass" ? .45f : .11f);
            material.SetFloat("_Metallic", key is "Metal" or "Brass" or "Rust" ? .65f : 0f);
            // Glass and flat grime decals have no thickness, so both faces stay visible by torchlight. Lettering
            // (Chalk) and notices (Paper) are closed meshes: single-sided, so no sign reads mirrored from behind.
            material.SetFloat("_Cull", key is "Glass" or "Mould" or "Water" or "Stain" ? (float)CullMode.Off : (float)CullMode.Back);
            if (key is "LightAmber" or "LightRed")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Palette[key] * 1.5f);
            }
            GeneratedMaterialRepair.Apply(material);
        }

        private static readonly Dictionary<string, string> Aliases = new()
        {
            ["AbandonedBag"] = "AbandonedBackpack", ["SalvageGear"] = "Suitcase", ["NoticeBoard"] = "MissingPoster",
            ["floor"] = "FloorTile", ["wall"] = "WallSolid", ["door_wall"] = "WallDoor",
            ["wide_wall"] = "WallWide", ["window_wall"] = "WallWindow", ["storefront"] = "Storefront",
            ["railing"] = "AtriumRailing", ["pillar"] = "Pillar", ["stairs"] = "ServiceStairs",
            ["escalator"] = "Escalator", ["shutter"] = "RollerShutter", ["light_fixture"] = "FluorescentFixture",
            ["puddle"] = "Puddle", ["debris"] = "DebrisPile", ["barricade"] = "Barricade",
            ["mannequin"] = "Mannequin", ["shelf"] = "RetailShelf", ["counter"] = "Counter",
            ["bench"] = "Bench", ["kiosk"] = "Kiosk", ["fountain"] = "Fountain",
            ["ceiling"] = "CeilingGrid", ["sagging_ceiling"] = "CeilingSagging", ["mould"] = "MouldPatch",
            ["stain"] = "WaterStain", ["crack"] = "FloorCrack", ["rebar"] = "ExposedRebar",
            ["glass"] = "BrokenGlass", ["growth"] = "Overgrowth", ["locker"] = "DepotLockers",
            ["desk"] = "SecurityDesk", ["cinema_seat"] = "CinemaSeat", ["food_table"] = "FoodCourtTable",
            ["backpack"] = "AbandonedBackpack", ["suitcase"] = "Suitcase", ["symbol"] = "StrangeSymbol",
        };

        public static GameObject Model(string name)
        {
            name = Aliases.GetValueOrDefault(name, name);
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{name}.fbx");
        }

        public static GameObject Place(string name, Transform parent, Vector3 position, float yaw = 0f, Vector3? scale = null)
            => Place(name, parent, position, Quaternion.Euler(0f, yaw, 0f), scale ?? Vector3.one);

        public static GameObject Place(string name, Transform parent, Vector3 localPosition, Quaternion localRotation)
            => Place(name, parent, localPosition, localRotation, Vector3.one);

        public static GameObject Place(string name, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 scale)
        {
            GameObject model = Model(name);
            if(model==null)throw new System.InvalidOperationException($"Custom environment model missing: {name}. Regenerate Tools/Blender/environment.py.");
            // FBX roots may carry centimetre/axis conversion. Place a neutral metre-space holder,
            // never overwrite those import corrections with the level's requested transform.
            var instance=new GameObject(name);
            instance.transform.SetParent(parent,false);
            instance.transform.SetLocalPositionAndRotation(localPosition,localRotation);
            instance.transform.localScale=scale;
            var geometry=(GameObject)PrefabUtility.InstantiatePrefab(model,instance.transform);
            geometry.name="Geometry";
            foreach(Collider collider in geometry.GetComponentsInChildren<Collider>())Object.DestroyImmediate(collider);
            foreach(Renderer renderer in geometry.GetComponentsInChildren<Renderer>())
            {
                Material[] mapped=renderer.sharedMaterials;
                for(int i=0;i<mapped.Length;i++)
                {
                    if(mapped[i]==null)throw new System.InvalidOperationException($"Missing material in environment model {name}.");
                    if(mapped[i].name.StartsWith("ENV_"))mapped[i]=Material(mapped[i].name);
                }
                renderer.sharedMaterials=mapped;
            }
            return instance;
        }

        /// <summary>Fit a visual to a local-space box while preserving the holder's gameplay collider.</summary>
        public static Transform Fit(GameObject holder, string name, Vector3 localSize, bool standOnFloor = false)
        {
            HidePrimitive(holder);
            Transform model = Place(name, holder.transform, Vector3.zero, Quaternion.identity).transform;
            Bounds bounds = ModelFit.LocalBounds(model.gameObject, holder.transform);
            Vector3 size = bounds.size;
            model.localScale = new Vector3(localSize.x / Mathf.Max(.001f, size.x), localSize.y / Mathf.Max(.001f, size.y), localSize.z / Mathf.Max(.001f, size.z));
            bounds = ModelFit.LocalBounds(model.gameObject, holder.transform);
            model.localPosition -= bounds.center;
            if (standOnFloor) model.localPosition += Vector3.up * (bounds.extents.y - localSize.y * .5f);
            return model;
        }

        public static void HidePrimitive(GameObject holder)
        {
            Renderer renderer = holder.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        public static Transform SectionDecor(string name, Transform parent, Vector3 position, Quaternion rotation, StructuralSection support)
        {
            Transform decor = Place(name, parent, position, rotation).transform;
            if (support != null) decor.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            foreach (Renderer renderer in decor.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode = name is "Puddle" or "WaterStain" or "MouldPatch" or "FloorCrack" or "FloodWater" ? ShadowCastingMode.Off : ShadowCastingMode.On;
            return decor;
        }

        /// <summary>World-space placement convenience for scene builders with unit-scale decoration roots.</summary>
        public static Transform At(string name, Transform parent, Vector3 position, float yaw = 0f, Vector3? scale = null)
        {
            Transform model = Place(name, parent, Vector3.zero, Quaternion.identity, scale ?? Vector3.one).transform;
            model.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            return model;
        }
    }
}

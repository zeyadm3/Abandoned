using System.Collections.Generic;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Original low-poly models for the island's loot, built inside each definition's own Size box (parts placed as
    /// fractions of it), so the art never changes a collider, a door clearance or a floor load. One mesh per material
    /// per model, like the factory's. Only fills in items without a model.
    /// </summary>
    public static class IslandLootModels
    {
        private const string Folder = IslandPrisonArt.Folder + "/Loot";

        public static int AssignMissing()
        {
            PolishAssets.EnsureFolder(Folder);
            int made = 0;
            foreach (IslandLootItems.Item item in IslandLootItems.Items)
            {
                LootDefinition definition = LootCatalogBuilder.Load(item.Id);
                if (definition == null || definition.Model != null) continue;
                var root = new GameObject(item.Id);
                try
                {
                    Build(root.transform, definition);
                    Combine(root, item.Id);
                    GameObject model = PrefabUtility.SaveAsPrefabAsset(root, $"{Folder}/{item.Id}.prefab");
                    definition.EditorSetModel(model, 0f, definition.Size);
                    EditorUtility.SetDirty(definition);
                    made++;
                }
                finally { Object.DestroyImmediate(root); }
            }
            return made;
        }

        private static void Build(Transform root, LootDefinition d)
        {
            Vector3 size = d.Size;
            Material brass = PolishAssets.Material("IslandLoot_Brass", new Color(.66f, .5f, .2f), .35f);
            Material steel = PolishAssets.Material("IslandLoot_Steel", new Color(.5f, .52f, .54f), .4f);
            Material black = PolishAssets.Material("IslandLoot_Black", new Color(.07f, .07f, .08f), .6f);
            Material wood = PolishAssets.Material("IslandLoot_Wood", new Color(.36f, .22f, .12f), .7f);
            Material dark = PolishAssets.Material("IslandLoot_DarkWood", new Color(.16f, .1f, .07f), .55f);
            Material paper = PolishAssets.Material("IslandLoot_Paper", new Color(.8f, .76f, .64f), .9f);
            Material glass = PolishAssets.Material("IslandLoot_Glass", new Color(.72f, .84f, .82f), .1f);
            Material cloth = PolishAssets.Material("IslandLoot_Cloth", new Color(.5f, .44f, .3f), .95f);
            Material ivory = PolishAssets.Material("IslandLoot_Ivory", new Color(.86f, .84f, .76f), .4f);
            Material red = PolishAssets.Material("IslandLoot_Red", new Color(.72f, .16f, .12f), .7f);
            Material paint = PolishAssets.Material("IslandLoot_" + d.Id, d.Color, .7f);
            void P(string name, Vector3 at, Vector3 dims, Material m, bool round = false) =>
                PolishAssets.Shape(name, root, Vector3.Scale(size, at), Vector3.Scale(size, dims), m, round);

            switch (d.Id)
            {
                case "soap_pistol":
                    P("Grip", new(-.3f, -.15f, 0f), new(.3f, .7f, 1f), paint);
                    P("Barrel", new(.15f, .25f, 0f), new(.7f, .4f, .9f), paint);
                    P("Guard", new(-.05f, -.2f, 0f), new(.2f, .3f, .6f), paint, true);
                    break;
                case "dummy_head":
                    P("Head", new(0f, .05f, 0f), new(.85f, .9f, .9f), paint, true);
                    P("Hair", new(0f, .35f, -.05f), new(.86f, .3f, .85f), dark, true);
                    P("Nose", new(0f, .05f, .45f), new(.12f, .15f, .12f), paint);
                    P("Neck", new(0f, -.42f, 0f), new(.4f, .16f, .4f), paper);
                    break;
                case "spoon_digger":
                    P("Handle", new(-.15f, 0f, 0f), new(.7f, 1f, .4f), steel);
                    P("Bowl", new(.35f, 0f, 0f), new(.3f, 1f, 1f), steel, true);
                    P("Wrap", new(-.35f, 0f, 0f), new(.25f, 1.4f, .6f), cloth);
                    break;
                case "cigarette_carton":
                    P("Box", Vector3.zero, Vector3.one, paint);
                    P("Band", new(0f, 0f, .01f), new(1.01f, .3f, 1.01f), red);
                    break;
                case "homemade_radio":
                    P("Case", Vector3.zero, Vector3.one, wood);
                    P("Coil", new(-.2f, .5f, 0f), new(.3f, .3f, .5f), brass, true);
                    P("Dial", new(.25f, .1f, .5f), new(.25f, .3f, .05f), black, true);
                    P("Wire", new(.3f, .6f, 0f), new(.05f, .6f, .05f), steel);
                    break;
                case "inmate_harmonica":
                    P("Cover", Vector3.zero, Vector3.one, steel);
                    P("Comb", new(0f, 0f, .2f), new(1.02f, .5f, .6f), wood);
                    break;
                case "raincoat_raft":
                    P("Hull", new(0f, -.1f, 0f), new(1f, .6f, 1f), paint, true);
                    P("Seam", new(0f, .2f, 0f), new(.9f, .1f, .9f), black, true);
                    P("Paddle", new(.3f, .35f, 0f), new(.6f, .1f, .12f), wood);
                    break;
                case "guard_keyring":
                    P("Ring", Vector3.zero, new(1f, .6f, 1f), steel, true);
                    for (int i = 0; i < 5; i++) P("Key", new(Mathf.Cos(i * 1.2f) * .35f, 0f, Mathf.Sin(i * 1.2f) * .35f), new(.35f, .5f, .15f), brass);
                    break;
                case "riot_shield":
                    P("Panel", Vector3.zero, new(1f, 1f, .5f), paint);
                    P("Window", new(0f, .25f, .02f), new(.6f, .25f, .52f), glass);
                    P("Handle", new(0f, 0f, -.5f), new(.2f, .4f, .5f), black);
                    break;
                case "guard_radio":
                    P("Body", new(0f, -.1f, 0f), new(1f, .8f, 1f), paint);
                    P("Antenna", new(.25f, .4f, 0f), new(.25f, .3f, .3f), black);
                    P("Grille", new(0f, .05f, .5f), new(.8f, .3f, .05f), steel);
                    break;
                case "nightstick":
                    P("Shaft", Vector3.zero, new(1f, 1f, 1f), paint, true);
                    P("Grip", new(0f, 0f, -.35f), new(1.1f, 1.1f, .3f), black, true);
                    break;
                case "gas_mask":
                    P("Face", new(0f, 0f, .1f), new(.9f, .95f, .8f), paint, true);
                    P("EyeL", new(-.2f, .15f, .48f), new(.25f, .2f, .05f), glass, true);
                    P("EyeR", new(.2f, .15f, .48f), new(.25f, .2f, .05f), glass, true);
                    P("Filter", new(0f, -.3f, .45f), new(.4f, .35f, .3f), steel, true);
                    break;
                case "armoury_gun_safe":
                    P("Body", Vector3.zero, Vector3.one, paint);
                    P("Door", new(0f, 0f, .5f), new(.85f, .9f, .04f), steel);
                    P("Dial", new(.15f, .1f, .53f), new(.2f, .1f, .06f), brass, true);
                    P("Handle", new(-.2f, .1f, .54f), new(.06f, .2f, .06f), brass);
                    break;
                case "warden_portrait":
                    P("Frame", Vector3.zero, Vector3.one, brass);
                    P("Canvas", new(0f, 0f, .3f), new(.85f, .88f, .5f), dark);
                    P("Face", new(0f, .15f, .6f), new(.3f, .3f, .1f), ivory, true);
                    break;
                case "silver_tea_set":
                    P("Tray", new(0f, -.42f, 0f), new(1f, .12f, 1f), steel);
                    P("Pot", new(-.2f, .05f, 0f), new(.4f, .8f, .55f), steel, true);
                    P("Spout", new(-.42f, .1f, 0f), new(.2f, .12f, .12f), steel);
                    P("Jug", new(.25f, -.1f, .15f), new(.22f, .45f, .3f), steel, true);
                    P("Cup", new(.3f, -.25f, -.25f), new(.18f, .2f, .25f), ivory, true);
                    break;
                case "gramophone":
                    P("Box", new(0f, -.35f, 0f), new(1f, .3f, 1f), wood);
                    P("Platter", new(0f, -.18f, 0f), new(.8f, .04f, .8f), black, true);
                    P("Arm", new(.1f, 0f, -.2f), new(.08f, .4f, .08f), brass);
                    P("Horn", new(.05f, .25f, .1f), new(.8f, .5f, .8f), brass, true);
                    break;
                case "warden_pocket_watch":
                    P("Case", Vector3.zero, new(1f, .9f, 1f), brass, true);
                    P("Face", new(0f, 0f, .4f), new(.75f, .7f, .2f), ivory, true);
                    P("Bow", new(0f, .5f, 0f), new(.4f, .2f, .5f), brass, true);
                    break;
                case "warden_piano":
                    P("Case", new(0f, .05f, -.1f), new(1f, .9f, .8f), paint);
                    P("Keybed", new(0f, -.08f, .3f), new(.92f, .06f, .4f), paint);
                    P("Keys", new(0f, -.03f, .32f), new(.88f, .03f, .3f), ivory);
                    P("Lid", new(0f, .48f, -.1f), new(1.02f, .04f, .82f), paint);
                    P("LegL", new(-.42f, -.35f, .35f), new(.05f, .3f, .1f), paint);
                    P("LegR", new(.42f, -.35f, .35f), new(.05f, .3f, .1f), paint);
                    P("Pedals", new(0f, -.47f, .1f), new(.2f, .04f, .2f), brass);
                    break;
                case "ships_bell":
                    P("Lip", new(0f, -.35f, 0f), new(1f, .3f, 1f), paint, true);
                    P("Waist", new(0f, 0f, 0f), new(.8f, .6f, .8f), paint, true);
                    P("Crown", new(0f, .38f, 0f), new(.5f, .2f, .5f), paint, true);
                    P("Bracket", new(0f, .48f, 0f), new(.8f, .05f, .2f), black);
                    break;
                case "diving_helmet":
                    P("Dome", new(0f, .1f, 0f), new(.9f, .85f, .9f), paint, true);
                    P("Corselet", new(0f, -.38f, 0f), new(1f, .25f, 1f), paint, true);
                    P("Port", new(0f, .1f, .45f), new(.35f, .35f, .1f), glass, true);
                    P("PortL", new(-.43f, .12f, 0f), new(.1f, .25f, .25f), glass, true);
                    P("PortR", new(.43f, .12f, 0f), new(.1f, .25f, .25f), glass, true);
                    break;
                case "brass_sextant":
                    P("Frame", new(0f, -.1f, 0f), new(1f, .7f, .3f), paint);
                    P("Arc", new(0f, -.4f, 0f), new(1f, .15f, .4f), paint, true);
                    P("Scope", new(0f, .3f, .3f), new(.6f, .15f, .2f), black, true);
                    P("Mirror", new(.1f, .2f, 0f), new(.15f, .2f, .5f), glass);
                    break;
                case "signal_lamp":
                    P("Body", new(0f, 0f, 0f), new(1f, .8f, 1f), paint, true);
                    P("Lens", new(0f, 0f, .48f), new(.8f, .65f, .1f), glass, true);
                    P("Shutter", new(0f, 0f, .52f), new(.85f, .7f, .03f), steel);
                    P("Grip", new(0f, .48f, 0f), new(.6f, .1f, .2f), black);
                    break;
                case "life_ring":
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        P("Seg", new(Mathf.Cos(a) * .38f, 0f, Mathf.Sin(a) * .38f), new(.3f, 1f, .3f), i % 2 == 0 ? paint : ivory, true);
                    }
                    break;
                case "ship_in_bottle":
                    P("Bottle", Vector3.zero, new(.85f, 1f, 1f), glass, true);
                    P("Neck", new(.45f, 0f, 0f), new(.15f, .45f, .45f), glass, true);
                    P("Cork", new(.52f, 0f, 0f), new(.06f, .35f, .35f), wood);
                    P("Hull", new(-.05f, -.2f, 0f), new(.45f, .2f, .3f), dark);
                    P("Sails", new(-.05f, .15f, 0f), new(.25f, .5f, .05f), ivory);
                    break;
                case "fresnel_lens":
                    for (int i = 0; i < 6; i++)
                        P("Ring", new(0f, -.4f + i * .16f, 0f), new(1f - Mathf.Abs(i - 2.5f) * .08f, .14f, 1f - Mathf.Abs(i - 2.5f) * .08f), glass, true);
                    P("Frame_A", new(0f, 0f, 0f), new(.06f, 1f, 1.02f), brass);
                    P("Frame_B", new(0f, 0f, 0f), new(1.02f, 1f, .06f), brass);
                    P("Base", new(0f, -.47f, 0f), new(.7f, .06f, .7f), brass, true);
                    break;
                default:
                    P("Body", Vector3.zero, Vector3.one, paint);
                    break;
            }
        }

        // One mesh per material, saved beside the prefab.
        private static void Combine(GameObject root, string id)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] parts = root.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter part in parts)
            {
                Material m = part.GetComponent<Renderer>().sharedMaterial;
                if (!groups.TryGetValue(m, out List<CombineInstance> list)) groups[m] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = part.sharedMesh, transform = root.transform.worldToLocalMatrix * part.transform.localToWorldMatrix });
            }
            foreach (MeshFilter part in parts) Object.DestroyImmediate(part.gameObject);
            foreach (KeyValuePair<Material, List<CombineInstance>> pair in groups)
            {
                string path = $"{Folder}/{id}_{pair.Key.name}.asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh == null) { mesh = new Mesh { name = id + "_" + pair.Key.name }; AssetDatabase.CreateAsset(mesh, path); }
                else mesh.Clear();
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                EditorUtility.SetDirty(mesh);
                var go = new GameObject(pair.Key.name);
                go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
            }
        }
    }
}

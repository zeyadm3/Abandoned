using System.Collections.Generic;
using Abandoned.Loot;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Original low-poly models for the factory loot, built inside each definition's own Size box (parts are
    /// placed as fractions of it), so the art never changes a collider, a door clearance or a floor load.
    /// One mesh per material per model, like the mall's valuables. Only fills in items without a model.
    /// </summary>
    public static class IronFactoryLootModels
    {
        private const string Folder = "Assets/_Project/Art/Factory/Loot";

        public static int AssignMissing()
        {
            PolishAssets.EnsureFolder(Folder);
            int made = 0;
            foreach (IronFactoryLootItems.Item item in IronFactoryLootItems.Items)
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
            Material iron = PolishAssets.Material("FactoryLoot_Iron", new Color(.24f, .23f, .22f), .55f);
            Material steel = PolishAssets.Material("FactoryLoot_Steel", new Color(.46f, .48f, .5f), .4f);
            Material rust = PolishAssets.Material("FactoryLoot_Rust", new Color(.36f, .18f, .09f), .85f);
            Material brass = PolishAssets.Material("FactoryLoot_Brass", new Color(.66f, .5f, .2f), .35f);
            Material wood = PolishAssets.Material("FactoryLoot_Wood", new Color(.42f, .27f, .14f), .8f);
            Material paper = PolishAssets.Material("FactoryLoot_Paper", new Color(.78f, .74f, .62f), .9f);
            Material black = PolishAssets.Material("FactoryLoot_Black", new Color(.07f, .07f, .08f), .6f);
            Material glass = PolishAssets.Material("FactoryLoot_Glass", new Color(.75f, .82f, .8f), .1f);
            Material paint = PolishAssets.Material("FactoryLoot_" + d.Id, d.Color, .7f);
            void P(string name, Vector3 at, Vector3 dims, Material m, bool round = false) =>
                PolishAssets.Shape(name, root, Vector3.Scale(size, at), Vector3.Scale(size, dims), m, round);

            switch (d.Id)
            {
                case "iron_ingot":
                    P("Body", new(0f, 0f, 0f), new(1f, .8f, .85f), iron);
                    P("Top", new(0f, .42f, 0f), new(.85f, .16f, .7f), rust);
                    break;
                case "steel_billet":
                    P("Body", Vector3.zero, new(1f, 1f, 1f), steel);
                    P("Scale", new(0f, .3f, 0f), new(.9f, .45f, .96f), rust);
                    break;
                case "cast_gear":
                    P("Hub", Vector3.zero, new(.35f, 1f, .35f), iron, true);
                    P("Rim", Vector3.zero, new(.86f, .8f, .86f), iron, true);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        P("Tooth", new(Mathf.Cos(a) * .45f, 0f, Mathf.Sin(a) * .45f), new(.12f, .8f, .12f), rust);
                    }
                    break;
                case "chain_coil":
                    for (int i = 0; i < 4; i++) P("Turn", new(0f, -.38f + i * .25f, 0f), new(1f - i * .12f, .22f, 1f - i * .12f), iron, true);
                    break;
                case "anvil":
                    P("Base", new(0f, -.32f, 0f), new(.55f, .36f, .9f), iron);
                    P("Waist", new(0f, 0f, 0f), new(.35f, .3f, .55f), iron);
                    P("Face", new(.05f, .33f, 0f), new(.75f, .34f, 1f), steel);
                    P("Horn", new(-.43f, .33f, 0f), new(.25f, .2f, .5f), steel, true);
                    break;
                case "founders_bell":
                    P("Lip", new(0f, -.38f, 0f), new(1f, .24f, 1f), paint, true);
                    P("Waist", new(0f, -.02f, 0f), new(.78f, .6f, .78f), paint, true);
                    P("Crown", new(0f, .36f, 0f), new(.5f, .2f, .5f), paint, true);
                    P("Canons", new(0f, .46f, 0f), new(.18f, .08f, .3f), brass);
                    break;
                case "brass_gauge":
                    P("Case", Vector3.zero, new(1f, 1f, .7f), brass, true);
                    P("Glass", new(0f, 0f, .38f), new(.8f, .8f, .1f), glass, true);
                    P("Stem", new(0f, -.55f, 0f), new(.2f, .3f, .3f), brass);
                    break;
                case "foreman_stopwatch":
                    P("Case", Vector3.zero, new(1f, .85f, 1f), steel, true);
                    P("Crown", new(0f, .48f, 0f), new(.25f, .15f, .3f), steel);
                    break;
                case "micrometer_set":
                    P("Case", new(0f, -.2f, 0f), new(1f, .6f, 1f), wood);
                    P("Lid", new(0f, .3f, 0f), new(1f, .4f, 1f), paint);
                    P("Clasp", new(0f, 0f, .5f), new(.15f, .3f, .05f), brass);
                    break;
                case "torque_wrench":
                    P("Shaft", new(.08f, 0f, 0f), new(.84f, .5f, .5f), steel);
                    P("Grip", new(.38f, 0f, 0f), new(.24f, .8f, .8f), black);
                    P("Head", new(-.42f, 0f, 0f), new(.16f, 1f, 1f), steel, true);
                    break;
                case "welding_mask":
                    P("Shell", new(0f, 0f, .05f), new(1f, 1f, .9f), black, true);
                    P("Lens", new(0f, .1f, .46f), new(.5f, .2f, .08f), glass);
                    P("Band", new(0f, .1f, -.3f), new(.9f, .15f, .4f), paint);
                    break;
                case "machinist_vise":
                    P("Base", new(0f, -.35f, 0f), new(1f, .3f, 1f), paint);
                    P("FixedJaw", new(-.25f, .1f, 0f), new(.3f, .6f, .9f), paint);
                    P("MovingJaw", new(.2f, .1f, 0f), new(.25f, .55f, .9f), paint);
                    P("Screw", new(.42f, 0f, 0f), new(.4f, .15f, .15f), steel);
                    break;
                case "casting_pattern":
                    P("Body", new(0f, -.1f, 0f), new(.9f, .7f, .8f), wood);
                    P("Boss", new(.15f, .32f, 0f), new(.4f, .36f, .5f), wood, true);
                    P("Core", new(-.3f, .2f, 0f), new(.2f, .5f, .3f), paint);
                    break;
                case "steam_governor":
                    P("Column", new(0f, -.05f, 0f), new(.14f, .9f, .14f), brass);
                    P("Base", new(0f, -.44f, 0f), new(.6f, .12f, .6f), iron);
                    P("BallA", new(-.33f, .12f, 0f), new(.34f, .2f, .34f), brass, true);
                    P("BallB", new(.33f, .12f, 0f), new(.34f, .2f, .34f), brass, true);
                    P("Cap", new(0f, .44f, 0f), new(.3f, .1f, .3f), brass, true);
                    break;
                case "electric_motor":
                    P("Housing", new(0f, .05f, 0f), new(.9f, .85f, .8f), paint, true);
                    P("Feet", new(0f, -.42f, 0f), new(1f, .16f, .7f), iron);
                    P("Shaft", new(0f, .05f, .48f), new(.15f, .15f, .1f), steel);
                    P("Box", new(0f, .48f, 0f), new(.3f, .12f, .3f), iron);
                    break;
                case "flywheel":
                    P("Rim", Vector3.zero, new(1f, 1f, 1f), iron, true);
                    P("Web", Vector3.zero, new(.78f, .5f, .78f), rust, true);
                    P("Hub", Vector3.zero, new(.2f, 1.1f, .2f), steel, true);
                    break;
                case "crucible":
                    P("Pot", new(0f, -.05f, 0f), new(.85f, .85f, .85f), paint, true);
                    P("Lip", new(0f, .4f, 0f), new(.92f, .1f, .92f), iron, true);
                    P("Slag", new(0f, .43f, 0f), new(.7f, .05f, .7f), black, true);
                    P("Trunnion", new(0f, .15f, 0f), new(1f, .12f, .14f), steel);
                    break;
                case "production_ledgers":
                    for (int i = 0; i < 4; i++) P("Ledger", new(0f, -.38f + i * .25f, 0f), new(.95f - i * .04f, .22f, .95f), i % 2 == 0 ? paint : wood);
                    P("Pages", new(.47f, 0f, 0f), new(.04f, .95f, .9f), paper);
                    break;
                case "typewriter":
                    P("Body", new(0f, -.15f, 0f), new(1f, .6f, .8f), black);
                    P("Carriage", new(0f, .3f, -.2f), new(1f, .3f, .3f), black);
                    P("Platen", new(0f, .3f, -.2f), new(.9f, .2f, .2f), paint, true);
                    P("Keys", new(0f, -.02f, .3f), new(.8f, .1f, .35f), steel);
                    P("Sheet", new(0f, .48f, -.25f), new(.5f, .35f, .02f), paper);
                    break;
                case "company_seal":
                    P("Base", new(0f, -.4f, 0f), new(1f, .2f, 1f), iron);
                    P("Frame", new(0f, 0f, -.25f), new(.7f, .7f, .3f), paint);
                    P("Lever", new(0f, .35f, .2f), new(.15f, .2f, .8f), steel);
                    break;
                case "foremans_safe":
                    P("Body", Vector3.zero, new(1f, 1f, 1f), paint);
                    P("Door", new(0f, .02f, .5f), new(.8f, .82f, .04f), iron);
                    P("Dial", new(.1f, .1f, .53f), new(.2f, .15f, .05f), steel, true);
                    P("Handle", new(-.2f, .1f, .54f), new(.05f, .25f, .05f), brass);
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

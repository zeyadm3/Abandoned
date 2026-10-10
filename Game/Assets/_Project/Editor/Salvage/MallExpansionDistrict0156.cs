using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Additive estate blocks between the established industrial district and wooded horizon.</summary>
    public static class MallExpansionDistrict0156
    {
        private static Material masonry, metal, glass, rust, safety;
        public static int Build(Transform parent)
        {
            Transform district = GreyboxFactory.Group("AdditionalEstate0156", parent);
            district.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Closed background premises beyond the estate fence.");
            district.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            masonry = MeridianWorldSurroundings.Material("Expansion0156Brick", "FacadeStone", new(.31f, .27f, .23f));
            metal = SalvageMaterials.Get("Metal"); glass = SalvageMaterials.Get("Glass");
            rust = SalvageMaterials.Get("Rust"); safety = SalvageMaterials.Get("Safety");
            for (int i = 0; i < 12; i++)
            {
                float angle = (i * 30f + 13f) * Mathf.Deg2Rad;
                Transform block = GreyboxFactory.Group("EstateBlock_" + i, district);
                block.localPosition = new Vector3(28 + Mathf.Cos(angle) * 378f, -.46f, 24 + Mathf.Sin(angle) * 378f);
                block.localRotation = Quaternion.Euler(0, 90 - (i * 30 + 13), 0);
                if (i % 3 == 0) Sawtooth(block, new(32, 15 + i % 2 * 3, 21));
                else if (i % 3 == 1) Office(block, new(23, 29 + i % 4 * 4, 18));
                else Utility(block, new(28, 17 + i % 3 * 2, 22));
                Batch(block, "EstateBlock_" + i);
            }
            SiteOffice(district, new(-38, -.46f, -46), 0, "WestClearanceCompound");
            SiteOffice(district, new(109, -.46f, -35), 90, "EastClearanceCompound");
            return 14;
        }

        private static GameObject Part(Transform root, string name, Vector3 at, Vector3 size, Material material,
            Vector3 euler = default)
        {
            GameObject part = GreyboxFactory.Box(name, root, at, size, material, false);
            part.transform.localRotation = Quaternion.Euler(euler);
            part.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            return part;
        }

        private static void Sawtooth(Transform root, Vector3 size)
        {
            Part(root, "Masonry workshop shell", Vector3.up * size.y * .5f, size, masonry);
            for (int bay = 0; bay < 4; bay++)
            {
                float x = -size.x * .375f + bay * size.x * .25f;
                Part(root, "Sawtooth roof slope", new(x, size.y + 1.35f, 0), new(size.x * .265f, .18f, size.z + .4f), metal, new(0, 0, 20));
                Part(root, "High northlight glazing", new(x + size.x * .12f, size.y + 1.35f, 0), new(.09f, 2.6f, size.z * .89f), glass);
                Part(root, "Loading bay shutter", new(x, 2.8f, -size.z * .5f - .04f), new(4.2f, 5.6f, .08f), metal);
                Part(root, "Upper factory window", new(x, size.y - 2.3f, -size.z * .5f - .08f), new(5.1f, 1.5f, .10f), glass);
            }
            Part(root, "Roof extractor", new(0, size.y + 4.4f, size.z * .23f), new(1.5f, 5.8f, 1.5f), rust);
        }

        private static void Office(Transform root, Vector3 size)
        {
            Part(root, "Vacant masonry apartment offices", Vector3.up * size.y * .5f, size, masonry);
            int floors = Mathf.FloorToInt(size.y / 3.6f);
            for (int floor = 0; floor < floors; floor++)
            {
                float y = 1.8f + floor * 3.6f;
                Part(root, "Window ribbon", new(0, y, -size.z * .5f - .04f), new(size.x * .88f, 1.8f, .07f), glass);
                for (int bay = 0; bay < 6; bay++)
                    Part(root, "Deep window pier", new(-size.x * .42f + bay * size.x * .168f, y, -size.z * .5f - .12f), new(.3f, 2.15f, .3f), metal);
                Part(root, "Floor string course", new(0, y + 1.45f, -size.z * .5f - .08f), new(size.x + .2f, .2f, .24f), metal);
            }
            Part(root, "Flat roof parapet", new(0, size.y + .4f, 0), new(size.x + .3f, .8f, size.z + .3f), metal);
            Part(root, "Rooftop stair core", new(-size.x * .3f, size.y + 2.2f, size.z * .2f), new(4.4f, 3.7f, 4.1f), masonry);
            Part(root, "Ground boarded entry", new(0, 2.4f, -size.z * .5f - .17f), new(3.6f, 4.8f, .11f), rust);
        }

        private static void Utility(Transform root, Vector3 size)
        {
            Part(root, "Low utility works", new(0, size.y * .30f, 0), new(size.x, size.y * .60f, size.z), metal);
            Part(root, "Raised machine hall", new(-size.x * .24f, size.y * .76f, 0), new(size.x * .5f, size.y * .31f, size.z * .77f), masonry);
            for (int tank = 0; tank < 3; tank++)
            {
                float x = size.x * .25f, z = -size.z * .28f + tank * size.z * .28f;
                GameObject vessel = GreyboxFactory.Primitive(PrimitiveType.Cylinder, "Cold industrial silo", root,
                    new(x, size.y * .75f, z), new(4.7f, size.y * .44f, 4.7f), rust, false);
                vessel.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                Part(root, "Silo service bridge", new(x, size.y * 1.17f, z), new(5.4f, .15f, 1.1f), metal);
            }
            Part(root, "Water tower riser", new(-size.x * .29f, size.y + 5f, size.z * .21f), new(2.6f, 12f, 2.6f), metal);
            Part(root, "Roof water tank", new(-size.x * .29f, size.y + 12f, size.z * .21f), new(7.5f, 4.7f, 7.5f), rust);
        }

        private static void SiteOffice(Transform parent, Vector3 at, float yaw, string name)
        {
            Transform office = GreyboxFactory.Group(name, parent);
            office.SetLocalPositionAndRotation(at, Quaternion.Euler(0, yaw, 0));
            Part(office, "Portable clearance office", new(0, 1.45f, 0), new(12, 2.9f, 5.4f), metal);
            Part(office, "Site cabin roof", new(0, 2.95f, 0), new(12.4f, .2f, 5.8f), rust);
            for (int i = 0; i < 4; i++)
            {
                float x = -4.6f + i * 2.5f;
                Part(office, "Boarded cabin window", new(x, 1.85f, -2.74f), new(1.3f, .85f, .04f), glass);
                Part(office, "Window security grille", new(x, 1.85f, -2.81f), new(1.43f, .06f, .08f), metal);
            }
            Part(office, "Locked site cabin door", new(4.7f, 1.1f, -2.78f), new(.95f, 2.2f, .10f), rust);
            foreach (float x in new[] { -5.5f, -1.5f, 2.5f, 5.5f })
            {
                Part(office, "Orange site barrier post", new(x, .7f, -4), new(.15f, 1.4f, .15f), safety);
                Part(office, "Stacked recovered steel", new(x, .23f, 3.9f), new(1.9f, .46f, 1.4f), rust);
            }
            Part(office, "Temporary barrier rail", new(0, 1.15f, -4), new(11.2f, .13f, .08f), safety);
            Batch(office, name);
        }

        private static void Batch(Transform root, string name)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            MeshFilter[] sources = root.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter source in sources)
            {
                Material material = source.GetComponent<MeshRenderer>().sharedMaterial;
                if (!groups.TryGetValue(material, out var parts)) groups[material] = parts = new();
                parts.Add(new CombineInstance { mesh = source.sharedMesh,
                    transform = root.worldToLocalMatrix * source.transform.localToWorldMatrix });
            }
            var renderers = new List<Renderer>();
            foreach (var pair in groups.OrderBy(pair => pair.Key.name, System.StringComparer.Ordinal))
            {
                var mesh = new Mesh { name = name + "_" + pair.Key.name, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray(), true, true);
                mesh.RecalculateBounds();
                mesh = SalvageAssetBuilder.SaveMesh(mesh, $"{SalvageExpansionModels0156.Folder}/Meshes/{mesh.name}.asset");
                Transform draw = GreyboxFactory.Group(pair.Key.name, root);
                draw.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = draw.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = pair.Key;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                GameObjectUtility.SetStaticEditorFlags(draw.gameObject, StaticEditorFlags.OccludeeStatic);
                renderers.Add(renderer);
            }
            foreach (MeshFilter source in sources) Object.DestroyImmediate(source.gameObject);
            var lod = root.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(.008f, renderers.ToArray()) });
            lod.RecalculateBounds();
        }
    }
}

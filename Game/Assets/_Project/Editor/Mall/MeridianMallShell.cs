using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>A purpose-built shopping centre: visible tenants around a shared two-storey court.</summary>
    public static class MeridianMallShell
    {
        public static void Build(Transform mall)
        {
            Transform walls = Group("Walls", mall);
            for (int floor = 0; floor < Floors; floor++) Walls(walls, floor);
            Columns(Group("Columns", mall));
            Roof(Group("Roof", mall));
            Facade(Group("MallFacade", mall));
            MallBasementBuilder.Build(mall);
        }

        private static void Walls(Transform parent, int floor)
        {
            Transform outside = Group($"Envelope_{floor}", parent), partitions = Group($"Tenants_{floor}", parent);
            Transform rails = Group($"Balustrades_{floor}", parent);
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                foreach (Vector2Int side in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                {
                    Vector2Int next = cell + side;
                    Vector3 at = TileTopCenter(cell, floor) + new Vector3(side.x, 0, side.y) * 2;
                    if (!InFootprint(next))
                    {
                        bool entry = floor == 0 && side == Vector2Int.down && (x == 6 || x == 7);
                        Transform bay = Oriented($"Outside_{floor}_{x}_{z}", outside, at, side);
                        if (entry)
                            Box("MainEntranceHeader", bay, new Vector3(0, 3.73f, 0), new Vector3(4, .54f, .32f), MeridianRetailMaterials.Stone);
                        else
                        {
                            Box("ClosedExteriorWall", bay, new Vector3(0, 2, 0), new Vector3(4, 4, .3f), MeridianRetailMaterials.Pale);
                            if (side == Vector2Int.down)
                                Box("SealedFacadeWindow", bay, new Vector3(0, 2.1f, .17f), new Vector3(3.45f, 2.15f, .045f), MeridianRetailMaterials.Dark, false);
                        }
                        continue;
                    }
                    if (side == Vector2Int.left || side == Vector2Int.down) continue;
                    bool holeA = IsVoid(cell, floor), holeB = IsVoid(next, floor);
                    if (holeA && holeB) continue;
                    if (holeA || holeB)
                    {
                        if (IsFlightOpening(cell, next, floor)) continue;
                        Rail(rails, at, side, holeA ? next : cell, floor);
                        continue;
                    }
                    Zone? a = ZoneAt(cell, floor), b = ZoneAt(next, floor);
                    if (!a.HasValue || !b.HasValue || SameSpace(a.Value, b.Value)) continue;
                    bool tenantA = a.Value.Kind == Kind.Store, tenantB = b.Value.Kind == Kind.Store;
                    bool glass = tenantA != tenantB && a.Value.Tag != "stock" && b.Value.Tag != "stock";
                    Zone tenant = tenantA ? a.Value : b.Value;
                    Transform panel = Oriented($"Partition_{floor}_{x}_{z}", partitions, at, tenantA ? side : -side);
                    bool door = DoorBetween(cell, next, floor).HasValue;
                    if (glass) Storefront(panel, tenant, door);
                    else
                    {
                        Box("TenantPartition", panel, new Vector3(0, 1.88f, 0), new Vector3(4, 3.75f, .18f), MeridianRetailMaterials.Pale);
                        Box("AgedSkirting", panel, new Vector3(0, .17f, 0), new Vector3(4, .34f, .21f), MeridianRetailMaterials.Stone);
                    }
                    if (door)
                    {
                        // Service partitions only use authored internal openings; the envelope is always sealed.
                        if (!glass)
                        {
                            Object.DestroyImmediate(panel.Find("TenantPartition").gameObject);
                            Object.DestroyImmediate(panel.Find("AgedSkirting").gameObject);
                            Box("ServiceHeader", panel, new Vector3(0, 3.57f, 0), new Vector3(4, .35f, .22f), MeridianRetailMaterials.Pale);
                        }
                        MeridianDressingSigns.Sign(panel, new Vector3(0, 3.17f, .23f), Vector3.forward, tenant.DisplayName, 3.35f, .32f);
                    }
                }
            }
            MarkStatic(outside);
            MarkStatic(partitions);
            KeepGlazingTransparent(partitions);
        }

        private static void Storefront(Transform panel, Zone tenant, bool doorway)
        {
            Material accent = MeridianRetailMaterials.Accent(tenant.Tag);
            Box("TenantFascia", panel, new Vector3(0, 3.37f, .02f), new Vector3(4, .68f, .28f), accent);
            Box("SoffitLip", panel, new Vector3(0, 3.03f, .13f), new Vector3(4, .055f, .32f), MeridianRetailMaterials.Brass, false);
            if (doorway) return;
            Box("StorefrontPlinth", panel, new Vector3(0, .22f, 0), new Vector3(4, .44f, .18f), MeridianRetailMaterials.Stone);
            Box("ClosedRetailGlazing", panel, new Vector3(0, 1.73f, .025f), new Vector3(3.92f, 2.58f, .08f), Glass());
            foreach (float x in new[] { -1.97f, 0f, 1.97f })
                Box("GlazingMullion", panel, new Vector3(x, 1.74f, .09f), new Vector3(.045f, 2.62f, .11f), MeridianRetailMaterials.Brass);
        }

        private static void Rail(Transform parent, Vector3 at, Vector2Int side, Vector2Int cell, int floor)
        {
            Transform rail = Oriented($"Rail_{floor}_{cell.x}_{cell.y}_{side}", parent, at, side);
            Box("BalconyGlass", rail, new Vector3(0, .57f, 0), new Vector3(3.9f, 1.06f, .085f), Glass());
            foreach (float x in new[] { -1.95f, 1.95f })
                Box("BalconyPost", rail, new Vector3(x, .57f, 0), new Vector3(.075f, 1.15f, .13f), MeridianRetailMaterials.Brass);
            Box("BalconyHandrail", rail, new Vector3(0, 1.13f, 0), new Vector3(4f, .08f, .14f), MeridianRetailMaterials.Brass);
            Box("BalconyStoneFascia", rail, new Vector3(0, -.22f, 0), new Vector3(3.96f, .35f, .13f), MeridianRetailMaterials.Stone, false);
            StructuralSection support = MeridianDressingUtility.Support(parent.root, cell, floor);
            if (support != null) rail.gameObject.AddComponent<SectionProp>().EditorSetup(support);
        }

        private static void Columns(Transform parent)
        {
            foreach (float x in new[] { 19.7f, 36.3f })
            foreach (float z in new[] { 15.7f, 32.3f })
            {
                Transform column = Group("AtriumPier", parent);
                column.localPosition = new Vector3(x, 0, z);
                Box("LimestonePier", column, new Vector3(0, 4, 0), new Vector3(.62f, 8, .62f), MeridianRetailMaterials.Stone);
                Box("PierCapital", column, new Vector3(0, 7.76f, 0), new Vector3(.94f, .36f, .94f), MeridianRetailMaterials.Stone);
                Box("PierFoot", column, new Vector3(0, .3f, 0), new Vector3(.76f, .6f, .76f), MeridianRetailMaterials.Dark);
            }
            MarkStatic(parent);
        }

        private static void Roof(Transform parent)
        {
            float y = Floors * StoryHeight;
            for (int x = 0; x < TilesX; x++)
            for (int z = 0; z < TilesZ; z++)
            {
                var cell = new Vector2Int(x, z);
                if (IsAtriumVoid(cell)) continue;
                Box("RoofSlab", parent, TileTopCenter(cell, Floors) + Vector3.up * .15f, new Vector3(4, .3f, 4), MeridianRetailMaterials.Pale);
            }
            Transform skylight = Group("ClosedVaultedSkylight", parent);
            for (int n = 0; n <= 4; n++)
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    GameObject rib = Box("VaultRafter", skylight, new Vector3(28 + side * 4, y + .85f, 16 + n * 4),
                        new Vector3(8.12f, .13f, .11f), MeridianRetailMaterials.Brass, false);
                    rib.transform.localRotation = Quaternion.Euler(0, 0, -side * 9f);
                }
                Box("SkylightPurlin", skylight, new Vector3(20 + n * 4, y + 1.48f - Mathf.Abs(n - 2) * .63f, 24),
                    new Vector3(.075f, .12f, 16), MeridianRetailMaterials.Dark, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject glass = Box("SealedSkylightPane", skylight, new Vector3(28 + side * 4, y + .86f, 24),
                    new Vector3(8.04f, .055f, 16), Glass());
                glass.transform.localRotation = Quaternion.Euler(0, 0, -side * 9f);
            }
            ClosedSkylightEnds(skylight, y);
            MarkStatic(parent);
            KeepGlazingTransparent(parent);
        }

        private static void ClosedSkylightEnds(Transform skylight, float y)
        {
            const string path = "Assets/_Project/Art/Generated/MeridianWorld/ClosedSkylightGable.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = "ClosedSkylightGable" };
                mesh.vertices = new[] { new Vector3(-8, .2f, 0), new Vector3(0, 1.5f, 0), new Vector3(8, .2f, 0) };
                mesh.triangles = new[] { 0, 1, 2 };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, path);
            }
            foreach (float z in new[] { 16f, 32f })
            {
                Transform end = Group("SealedSkylightEnd", skylight);
                end.localPosition = new Vector3(28, y, z);
                end.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                end.gameObject.AddComponent<MeshRenderer>().sharedMaterial = Glass();
                end.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
        }

        private static void Facade(Transform parent)
        {
            Box("MainEntranceMarquee", parent, new Vector3(28, 3.58f, -2), new Vector3(16, .3f, 4.2f), MeridianRetailMaterials.Stone);
            Box("MarqueeFascia", parent, new Vector3(28, 3.67f, -4.12f), new Vector3(16.3f, .48f, .12f), MeridianRetailMaterials.Accent("electronics"));
            foreach (float x in new[] { 23.7f, 32.3f })
                Box("MainEntryStoneReveal", parent, new Vector3(x, 1.75f, -.1f), new Vector3(.32f, 3.5f, .55f), MeridianRetailMaterials.Stone);
            MeridianDressingSigns.Sign(parent, new Vector3(28, 6.6f, -.23f), Vector3.back, "MERIDIAN MALL", 15f, .95f);
            MeridianDressingSigns.Sign(parent, new Vector3(28, 3.22f, .22f), Vector3.forward, "EXIT", 1.35f, .28f);
            foreach (float z in new[] { 0f, 48f })
                Box("LongFacadeCoping", parent, new Vector3(28, 8.4f, z), new Vector3(56.4f, .32f, .46f), MeridianRetailMaterials.Stone);
            foreach (float x in new[] { 0f, 56f })
                Box("SideFacadeCoping", parent, new Vector3(x, 8.4f, 24), new Vector3(.46f, .32f, 48), MeridianRetailMaterials.Stone);
            MarkStatic(parent);
        }

        private static Transform Oriented(string name, Transform parent, Vector3 at, Vector2Int direction)
        {
            Transform root = Group(name, parent);
            root.localPosition = at;
            root.localRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
            return root;
        }

        private static Material Glass()
        {
            const string path = "Assets/_Project/Art/Generated/MeridianWorld/ClosedMallGlazing.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "ClosedMallGlazing", enableInstancing = true };
            material.SetColor("_BaseColor", new Color(.37f, .5f, .51f, .16f));
            material.SetFloat("_Surface", 1); material.SetFloat("_SrcBlend", 5); material.SetFloat("_DstBlend", 10);
            material.SetFloat("_ZWrite", 0); material.SetFloat("_Smoothness", .55f); material.SetFloat("_Cull", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = 3000; material.SetShaderPassEnabled("ShadowCaster", false);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void KeepGlazingTransparent(Transform root)
        {
            foreach(Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if(renderer.sharedMaterials.Any(m=>m!=null&&m.renderQueue>=3000))
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);
        }
    }
}

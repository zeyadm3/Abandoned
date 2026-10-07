using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Sparse, readable abandonment: faded shop signs, water damage, empty bays and fallen ceiling fragments.</summary>
    public static class MallDecayBuilder
    {
        private static readonly Dictionary<string, string> Names = new()
        {
            ["electronics"] = "VOLT ELECTRONICS", ["clothing"] = "THREAD & CO.", ["jewelry"] = "AUREL JEWELERS",
            ["office"] = "MANAGEMENT", ["stock"] = "SERVICE / STOCK", ["furniture"] = "PALMER HOME",
            ["toys"] = "SMALL WORLD TOYS", ["cinema"] = "CINEMA 3", ["gallery"] = "NORTH GALLERY", ["food"] = "FOOD COURT",
        };

        public static void Place(Transform mall)
        {
            Transform dressing = Group("Decay", mall);
            Transform tiles = mall.Find(MallBuilder.TilesGroup);
            Material trim = PolishAssets.Material("Mall_Dado", new Color(0.27f, 0.31f, 0.3f));
            Material plaster = PolishAssets.Material("Mall_FallenCeiling", new Color(0.62f, 0.61f, 0.53f));
            Material stain = PolishAssets.Stain();
            Material notice = PolishAssets.Material("Mall_NoticePaper", new Color(0.63f, 0.59f, 0.44f), texture: PolishAssets.Texture("PaperAge"));
            Material sign = PolishAssets.Material("Mall_SignFace", new Color(0.2f, 0.29f, 0.3f), texture: PolishAssets.Texture("SignFade"));

            foreach (Door door in Doors)
            {
                Zone? a = ZoneAt(door.A, door.Floor), b = ZoneAt(door.B, door.Floor);
                Zone? store = a.HasValue && a.Value.Kind == Kind.Store ? a : b.HasValue && b.Value.Kind == Kind.Store ? b : null;
                if (!store.HasValue || !Names.TryGetValue(store.Value.Tag, out string title)) continue;
                Vector3 center = (TileTopCenter(door.A, door.Floor) + TileTopCenter(door.B, door.Floor)) * 0.5f;
                Vector2Int away = store.Value.Contains(door.A) ? door.B - door.A : door.A - door.B;
                Vector3 normal = new(away.x, 0f, away.y);
                Sign(dressing, center + normal * 0.13f + Vector3.up * 3.65f, normal, title, sign, 3.5f, 0.3f, 0.12f);
                // Exit route is authored; its label never promises a procedural route that could be missing.
                if (store.Value.Tag == "stock")
                    Sign(dressing, center + normal * 0.14f + Vector3.up * 2.65f, normal, "SERVICE STAIRS", trim, 1.45f, 0.23f, 0.08f);
            }

            for (int floor = 0; floor < Floors; floor++)
            foreach (Zone zone in ZonesByFloor[floor])
            for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
            for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
            {
                var cell = new Vector2Int(x, z);
                if (IsVoid(cell, floor) || Flights.Any(f => f.Tiles.Contains(cell) && (f.Floor == floor || f.Floor == floor - 1))) continue;
                StructuralSection support = tiles.Find($"Floor_{floor}/{MallBuilder.TileName(cell, floor)}")?.GetComponent<StructuralSection>();
                if (support == null) continue;
                int hash = Hash(cell, floor);
                if(hash%7==0)
                {
                    GameObject decay=CustomMallArt.Place(hash%3==0?"BrokenGlass":hash%3==1?"debris":"Overgrowth",dressing,TileTopCenter(cell,floor)+new Vector3(1.3f,0.02f,1.3f),Quaternion.Euler(0,hash%360,0));
                    decay.AddComponent<SectionProp>().EditorSetup(support);
                }

                foreach (Vector2Int side in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                {
                    Vector2Int next = cell + side;
                    if (zone.Contains(next) || DoorBetween(cell, next, floor).HasValue || IsFlightOpening(cell, next, floor)) continue;
                    if (!HasWall(zone, cell, next, floor)) continue;
                    Vector3 normal = new(side.x, 0f, side.y);
                    Vector3 center = TileTopCenter(cell, floor) + normal * (Tile * 0.5f - MallWalls.Thickness * 0.5f - 0.015f);
                    Transform wallDetail = Group($"WallDetail_{floor}_{x}_{z}_{side.x}_{side.y}", dressing);
                    wallDetail.position = center;
                    wallDetail.rotation = Quaternion.LookRotation(-normal);
                    Box("DadoRail", wallDetail, Vector3.up * 0.95f, new Vector3(3.95f, 0.07f, 0.04f), trim, false);
                    Box("Skirting", wallDetail, Vector3.up * 0.08f, new Vector3(3.95f, 0.13f, 0.04f), trim, false);
                    if ((hash + side.x * 5 + side.y * 13) % 7 == 0)
                    {
                        Decal("WaterMark", wallDetail, new Vector3(0.7f, 2.5f, 0.025f), Quaternion.identity, new Vector2(1.4f, 2.2f), stain);
                        Box("OldNotice", wallDetail, new Vector3(-0.65f, 1.75f, 0.028f), new Vector3(0.43f, 0.59f, 0.01f), notice, false);
                    }
                    if (hash % 17 == 0 && zone.Kind == Kind.Store)
                    {
                        // Decorative fragments hug the wall and have no colliders/load; the carry lanes remain intact.
                        Transform fragments = Group($"Fragments_{floor}_{x}_{z}", dressing);
                        fragments.position = TileTopCenter(cell, floor) + normal * 1.58f;
                        fragments.rotation = Quaternion.LookRotation(-normal);
                        for (int i = 0; i < 3; i++)
                        {
                            GameObject scrap = Box("CeilingScrap", fragments, new Vector3(-0.32f + i * 0.28f, 0.017f + i * 0.009f, (i % 2) * 0.13f),
                                new Vector3(0.37f, 0.025f, 0.23f), plaster, false);
                            scrap.transform.localRotation = Quaternion.Euler(0f, hash % 30 + i * 35f, i % 2 == 0 ? 0f : 4f);
                        }
                        fragments.gameObject.AddComponent<SectionProp>().EditorSetup(support);
                    }
                }
                if (hash % 11 == 0 && zone.Kind == Kind.Store)
                {
                    Transform patch = Group($"FloorDamp_{floor}_{x}_{z}", dressing);
                    patch.position = TileTopCenter(cell, floor) + new Vector3(1.1f, 0.012f, 1.2f);
                    Decal("DampPatch", patch, Vector3.zero, Quaternion.Euler(90f, hash % 360, 0f), new Vector2(1.15f, 0.8f), stain);
                    patch.gameObject.AddComponent<SectionProp>().EditorSetup(support);
                }
            }

            // The entrance identity and a shuttered lease notice reinforce a mall, without a new gameplay obstacle.
            Sign(dressing, new Vector3(28f, 3.72f, -0.25f), Vector3.back, "MERIDIAN", sign, 6.5f, 0.42f, 0.18f);
            foreach (Renderer renderer in dressing.GetComponentsInChildren<Renderer>())
            {
                if (renderer.GetComponentInParent<SectionProp>() != null) continue;
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            }
        }

        private static bool HasWall(Zone zone, Vector2Int cell, Vector2Int next, int floor)
        {
            if (InGrid(next))
            {
                Zone? other = ZoneAt(next, floor);
                return !IsVoid(next, floor) && other.HasValue && !SameSpace(zone, other.Value);
            }
            foreach (var opening in Exterior)
            {
                if (opening.floor != floor) continue;
                bool same = opening.side switch
                {
                    'S' => next.y < 0 && opening.column == cell.x,
                    'N' => next.y >= TilesZ && opening.column == cell.x,
                    'W' => next.x < 0 && opening.column == cell.y,
                    _ => next.x >= TilesX && opening.column == cell.y,
                };
                if (same) return false;
            }
            return true;
        }

        private static void Sign(Transform parent, Vector3 position, Vector3 front, string text, Material material, float width, float height, float textSize)
        {
            Transform panel = Group("Sign_" + text.Replace(' ', '_'), parent);
            panel.SetPositionAndRotation(position, Quaternion.LookRotation(front));
            Box("FadedFascia", panel, Vector3.zero, new Vector3(width, height, 0.055f), material, false);
            var label = new GameObject("Letters").AddComponent<TextMesh>();
            label.transform.SetParent(panel, false);
            label.transform.localPosition = Vector3.forward * 0.032f;
            label.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = textSize;
            label.fontSize = 64;
            label.color = new Color(0.63f, 0.65f, 0.58f);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf");
            WorldTextMaterial.Apply(label, font);
        }

        private static void Decal(string name, Transform parent, Vector3 position, Quaternion rotation, Vector2 size, Material material)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.SetLocalPositionAndRotation(position, rotation);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            var renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static int Hash(Vector2Int tile, int floor) => (int)((uint)(tile.x * 73856093 ^ tile.y * 19349663 ^ floor * 83492791) & 0x7fffffff);
    }
}

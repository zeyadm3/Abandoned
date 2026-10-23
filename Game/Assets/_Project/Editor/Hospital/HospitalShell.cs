using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using P = Abandoned.EditorTools.StationPlan;
namespace Abandoned.Hospital
{
    /// <summary>The envelope follows the actual wings, rather than enclosing the grid in a box.</summary>
    public static class HospitalShell
    {
        public static void Build(Transform root, HospitalKit kit, HospitalStructure floors)
        {
            StationContext c = floors.Context;
            var palette = new HospitalPalette(kit);
            Transform shell = HospitalKit.Group("Shell", root);
            Transform partitions = HospitalKit.Group("Partitions", root);
            Transform roof = HospitalKit.Group("Roof", root);
            Transform guards = HospitalKit.Group("GalleryGuards", root);
            Vector2Int [] directions =
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };
            foreach (var floor in c.Plan.Floors)
            {
                foreach (Vector2Int cell in floor.Value)
                {
                    int f = floor.Key;
                    foreach (Vector2Int step in directions)
                    {
                        Vector2Int other = cell + step;
                        bool has = c.Plan.HasFloor(f, other);
                        if (has)
                        {
                            if (step == Vector2Int.left || step == Vector2Int.down)
                            {
                                continue;
                            }
                            Interior(partitions, c, f, cell, other, step);
                            continue;
                        }
                        if (c.Plan.OpenEdges.Contains(P.Edge(f, cell, other)))
                        {
                            continue;
                        }
                        Vector3 at =(P.CellCentre(cell, f) + P.CellCentre(other, f)) * .5f;
                        if (c.Plan.IsHole(f, other))
                        {
                            Guard(guards, kit, floors, cell, f, at, step);
                            continue;
                        }
                        if (f <0)
                        {
                            Retaining(shell, kit, at, step, c.Art.Concrete);
                        }
                        else
                        {
                            Panel(shell, kit, palette, c.Plan, f, at, step);
                        }
                    }
                    bool lantern = f == 1 && cell.x >= 6 && cell.x <8 && cell.y <9;
                    if (f >= 0 && ! c.Plan.HasFloor(f + 1, cell) && ! c.Plan.IsHole(f + 1, cell) && ! lantern)
                    {
                        Vector3 at = P.CellCentre(cell, f) + Vector3.up * 3.88f;
                        kit.Box("TerraceRoof", roof, at, new Vector3(4f, .24f, 4f), palette.White);
                        kit.Box("RoofShadowEdge", roof, at + Vector3.up * .16f, new Vector3(4f, .08f, 4f), palette.Dark, false);
                    }
                }
            }
            HospitalLandmark.Build(root, kit, palette);
            GreyboxFactory.MarkStatic(shell);
            GreyboxFactory.MarkStatic(partitions);
            GreyboxFactory.MarkStatic(roof);
            // Clear glass is not a baked occluder; losing a ward must reveal the space beyond.
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == palette.Glass)
                {
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
                }
            }
        }
        private static void Interior(Transform parent, StationContext c, int f, Vector2Int a, Vector2Int b, Vector2Int step)
        {
            P.Room left = c.Plan.RoomAt(f, a),
            right = c.Plan.RoomAt(f, b);
            if (left == right)
            {
                return;
            }
            if (c.Plan.OpenEdges.Contains(P.Edge(f, a, b)))
            {
                return;
            }
            bool alongX = step == Vector2Int.up;
            Vector3 mid =(P.CellCentre(a, f) + P.CellCentre(b, f)) * .5f;
            float line = alongX ? mid.z : mid.x;
            float start = alongX ? a.x * 4f : a.y * 4f;
            var holes = new List<StationWalls.Hole>();
            P.Doorway door = c.Plan.DoorOn(f, a, b);
            if (door != null)
            {
                holes.Add(new StationWalls.Hole(start + 2f, door.Width, f * 4f, f * 4f + door.Height));
            }
            Material material = c.Art.For((left ?? right).Finish);
            StationWalls.Run(parent, "ClinicalPartition", alongX, line, start, start + 4f, f * 4f, f * 4f + 3.75f, .2f, material, holes, c.Art);
        }
        private static void Panel(Transform parent, HospitalKit k, HospitalPalette a, P plan, int floor, Vector3 at, Vector2Int step)
        {
            Transform t = HospitalKit.Group("WingFacade", parent);
            t.position = at;
            t.rotation = Quaternion.LookRotation(new Vector3(step.x, 0f, step.y));
            bool alongX = step.y != 0;
            var holes = new List<Vector2>();
            foreach (P.Opening opening in plan.Facade)
            {
                bool south = step == Vector2Int.down && at.z <.1f && opening.Side == P.Side.South;
                bool north = step == Vector2Int.up && at.z> 47.9f && opening.Side == P.Side.North;
                if ((! south && ! north) || opening.Top <= floor * 4f || opening.Bottom >= floor * 4f + 3.75f)
                {
                    continue;
                }
                float mid = alongX ? at.x : at.z;
                float l = Mathf.Max(- 2f, opening.Centre - opening.Width * .5f - mid);
                float r = Mathf.Min(2f, opening.Centre + opening.Width * .5f - mid);
                if (r> l)
                {
                    holes.Add(step.y <0 || step.x> 0 ? new Vector2(- r, - l) : new Vector2(l, r));
                }
            }
            if (holes.Count> 0)
            {
                Vector2 hole = holes [0];
                if (hole.x> - 1.99f)
                {
                    k.Box("EntryPier", t, new Vector3((- 2f + hole.x) * .5f, 1.88f, 0f), new Vector3(hole.x + 2f, 3.75f, .24f), a.Stone);
                }
                if (hole.y <1.99f)
                {
                    k.Box("EntryPier", t, new Vector3((hole.y + 2f) * .5f, 1.88f, 0f), new Vector3(2f - hole.y, 3.75f, .24f), a.Stone);
                }
                k.Box("EntryLintel", t, new Vector3(0f, 3.6f, 0f), new Vector3(4f, .3f, .24f), a.White);
                return;
            }
            k.Box("OpaqueSpandrel", t, new Vector3(0f, .42f, 0f), new Vector3(4f, .84f, .24f), a.Stone);
            k.Box("WindowHead", t, new Vector3(0f, 3.5f, 0f), new Vector3(4f, .5f, .24f), a.White);
            bool publicGallery = at.x >= 20f && at.x <= 36f && floor <2;
            k.Box("CurtainGlazing", t, new Vector3(0f, 2.04f, 0f), new Vector3(3.76f, 2.4f, .075f), publicGallery ? a.Glass : a.Blue);
            foreach (float side in new []
            {
                - 1.92f, 1.92f
            })
            {
                k.Box("StoneFin", t, new Vector3(side, 1.94f, .16f), new Vector3(.16f, 3.88f, .5f), floor <2 ? a.White : a.Bronze);
            }
            k.Box("RecessedShadowBand", t, new Vector3(0f, 3.85f, .12f), new Vector3(4f, .12f, .16f), a.Dark, false);
            if (floor <2)
            {
                return;
            }
            // The upper wings carry the reference's geometric glazing without becoming transparent glass boxes.
            foreach (float angle in new []
            {
                - 36f, 36f
            })
            {
                GameObject brace = k.Box("DiamondMullion", t, new Vector3(0f, 2.05f, .15f), new Vector3(.065f, 3.4f, .07f), a.Bronze, false);
                brace.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        private static void Retaining(Transform parent, HospitalKit k, Vector3 at, Vector2Int step, Material material)
        {
            k.Box("BasementRetaining", parent, at + Vector3.up * 1.875f, step.x == 0 ? new Vector3(4f, 3.75f, .25f) : new Vector3(.25f, 3.75f, 4f), material);
        }
        private static void Guard(Transform parent, HospitalKit k, HospitalStructure floors, Vector2Int cell, int f, Vector3 at, Vector2Int step)
        {
            Transform t = HospitalKit.Group("AtriumGuard", parent);
            Vector3 along = step.x == 0 ? Vector3.right : Vector3.forward;
            k.Rail(t, at - along * 2f, at + along * 2f);
            t.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Thin continuous safety collision behind the visible gallery rails.");
            k.Attach(t, floors.At(P.CellCentre(cell, f)));
        }
    }
}

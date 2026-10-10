using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The mall's hiding lockers (0.16.0): steel lockers against walls that were found bare in the saved
    /// 0.15.8c mall, one bank in most stores, on both service corridors and in the basement. Every site is
    /// checked again when it is built (solid things, dressing, floor, carry routes, loot and monster spawn
    /// points); one that fails is left out and reported, never forced in.
    /// </summary>
    public static class MallLockerBuilder
    {
        public const float Width = 0.8f, Depth = 0.7f, Height = 2f;
        private const float Sheet = 0.02f, Pitch = 0.82f;

        // A bank: lockers side by side against one wall. The wall is a constant X (running along Z) or a constant Z.
        private readonly struct Bank
        {
            public readonly string Name;
            public readonly int Floor, Out;
            public readonly bool AlongZ;
            public readonly float Wall, From;
            public readonly int Count;

            /// <param name="wall">World coordinate of the wall's face.</param>
            /// <param name="outward">+1 or -1: which way the lockers face along the other axis.</param>
            /// <param name="from">Centre of the first locker along the wall; the rest follow at one pitch each.</param>
            public Bank(string name, int floor, bool alongZ, float wall, int outward, float from, int count)
            {
                Name = name; Floor = floor; AlongZ = alongZ; Wall = wall; Out = outward; From = from; Count = count;
            }
        }

        private static readonly Bank[] Banks =
        {
            // Ground.
            new("ServiceCorridor_G", 0, false, 32.09f, 1, 49.59f, 2),
            new("ServiceCorridorEnd_G", 0, true, 55.85f, -1, 33f, 1),
            new("TitanTools", 0, true, 55.85f, -1, 21.04f, 2),
            new("NordicHabitat_G", 0, true, 55.85f, -1, 5.54f, 2),
            new("ApexCircuit", 0, true, 0.15f, 1, 5.04f, 2),
            new("VogueThread", 0, true, 0.15f, 1, 22.54f, 2),
            new("FoodCourt", 0, false, 47.85f, -1, 27.54f, 2),
            // Upper floor.
            new("ServiceCorridor_U", 1, false, 32.09f, 1, 49.59f, 2),
            new("Cinema", 1, false, 15.91f, -1, 6.04f, 2),
            new("CinemaWest", 1, true, 0.15f, 1, 4f, 1),
            new("ToyArchive", 1, false, 16.09f, 1, 47.3f, 2),
            new("ToyArchiveEast", 1, true, 55.85f, -1, 21.54f, 2),
            new("NordicHabitat_U", 1, true, 55.85f, -1, 5.54f, 2),
            new("ClosedOffices", 1, true, 0.15f, 1, 30.05f, 1),
            // Basement.
            new("Basement", -1, true, 40f, 1, 28.04f, 2),
        };

        // Groups under the mall that are the building itself (or its air): never in a locker's way.
        private static readonly HashSet<string> Building = new()
        {
            "Tiles", "MallCeilings", "Roof", "Walls", "Basement", "Exterior", "MallFacade", "_LIGHTING_VOLUMETRICS",
            "MeridianAtmosphere", "MeridianFixtures", MeridianWorldSurroundings.GroupName, MeridianMountainHorizon.GroupName,
            MallRooms0160.GroupName,
        };

        public static int Build(Transform mall, Transform parent)
        {
            Physics.SyncTransforms();
            var routes = new MallRouteClearance(mall);
            List<Bounds> dressing = Dressing(mall);
            int built = 0, left = 0;
            foreach (Bank bank in Banks)
            for (int i = 0; i < bank.Count; i++)
            {
                float along = bank.From + i * Pitch, y = MallLayout.FloorY(bank.Floor);
                Vector3 at = bank.AlongZ ? new Vector3(bank.Wall, y, along) : new Vector3(along, y, bank.Wall);
                Vector3 outward = bank.AlongZ ? new Vector3(bank.Out, 0f, 0f) : new Vector3(0f, 0f, bank.Out);
                string problem = Problem(routes, dressing, at, outward, out StructuralSection floor);
                if (problem != null)
                {
                    left++;
                    Debug.LogWarning($"[Rooms0160] Locker {bank.Name} #{i} at {at} left out: {problem}.");
                    continue;
                }
                Locker(parent, $"Locker_{bank.Name}_{i}", built++, at, outward, floor);
            }
            Debug.Log($"[Rooms0160] {built} hiding lockers placed, {left} left out.");
            return built;
        }

        // Why a locker can't stand here, or null when it can.
        private static string Problem(MallRouteClearance routes, List<Bounds> dressing, Vector3 at, Vector3 outward, out StructuralSection floor)
        {
            floor = null;
            Quaternion facing = Quaternion.LookRotation(outward);
            // The cabinet, a little clear of the wall and floor it stands against; then where someone stands
            // to climb in and steps out to.
            Vector3 bodyCentre = at + outward * (Depth * 0.5f + 0.04f) + Vector3.up * 1.05f;
            var bodyHalf = new Vector3(Width * 0.5f - 0.03f, 0.9f, Depth * 0.5f - 0.04f);
            Vector3 frontCentre = at + outward * (Depth + 0.55f) + Vector3.up * 1f;
            var frontHalf = new Vector3(Width * 0.5f - 0.03f, 0.85f, 0.5f);
            if (Physics.CheckBox(bodyCentre, bodyHalf, facing, ~0, QueryTriggerInteraction.Ignore)) return "something solid stands there";
            if (Physics.CheckBox(frontCentre, frontHalf, facing, ~0, QueryTriggerInteraction.Ignore)) return "something solid stands in front of it";
            Bounds body = WorldBox(bodyCentre, bodyHalf, facing), front = WorldBox(frontCentre, frontHalf, facing);
            foreach (Bounds piece in dressing)
                if (piece.Intersects(body) || piece.Intersects(front)) return "dressing stands there";
            string conflict = routes.Conflict(body);
            if (conflict != null) return conflict;
            Vector3 under = at + outward * (Depth * 0.5f) + Vector3.up * 0.5f;
            if (!Physics.Raycast(under, Vector3.down, out RaycastHit ground, 0.8f, ~0, QueryTriggerInteraction.Ignore)) return "there is no floor under it";
            if (!Physics.Raycast(at + outward * (Depth + 0.6f) + Vector3.up * 0.5f, Vector3.down, 0.8f, ~0, QueryTriggerInteraction.Ignore))
                return "there is no floor in front of it";
            floor = ground.collider.GetComponentInParent<StructuralSection>();
            return null;
        }

        // The world box round an oriented one (the lockers are square to the building, so it is the same box).
        private static Bounds WorldBox(Vector3 centre, Vector3 half, Quaternion facing)
        {
            Vector3 x = facing * new Vector3(half.x, 0f, 0f), z = facing * new Vector3(0f, 0f, half.z);
            var size = new Vector3(Mathf.Abs(x.x) + Mathf.Abs(z.x), half.y, Mathf.Abs(x.z) + Mathf.Abs(z.z)) * 2f;
            return new Bounds(centre, size);
        }

        // Everything standing in the mall that isn't the building: fittings, stock, posters, the truck's side of things.
        private static List<Bounds> Dressing(Transform mall)
        {
            var found = new List<Bounds>();
            foreach (Transform group in mall)
            {
                if (Building.Contains(group.name)) continue;
                foreach (Renderer renderer in group.GetComponentsInChildren<Renderer>(false))
                {
                    if (!renderer.enabled || renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer) continue;
                    // Light shafts and other see-through volumes take up no room.
                    Material material = renderer.sharedMaterial;
                    if (material == null || material.renderQueue >= 3000 || renderer.name.Contains("Beam")) continue;
                    found.Add(renderer.bounds);
                }
            }
            return found;
        }

        private static void Locker(Transform parent, string name, int index, Vector3 at, Vector3 outward, StructuralSection floor)
        {
            Transform root = Group(name, parent);
            // Z points out of the door; the origin is on the floor against the wall.
            root.SetPositionAndRotation(at, Quaternion.LookRotation(outward));
            Material steel = SalvageMaterials.Get("Metal"), dark = SalvageMaterials.Get("Plastic"), worn = SalvageMaterials.Get("Rust");

            // Thin sheets, not one block: whoever is inside sees the inside of them.
            Transform shell = Group("Shell", root);
            float run = Depth - Sheet, middle = Sheet + run * 0.5f;
            Box("Back", shell, new Vector3(0f, Height * 0.5f, Sheet * 0.5f), new Vector3(Width, Height, Sheet), steel, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Side", shell, new Vector3(side * (Width - Sheet) * 0.5f, Height * 0.5f, middle), new Vector3(Sheet, Height, run), steel, false);
            Box("Top", shell, new Vector3(0f, Height - Sheet * 0.5f, middle), new Vector3(Width, Sheet, run), steel, false);
            Box("Plinth", shell, new Vector3(0f, 0.04f, middle), new Vector3(Width, 0.08f, run), worn, false);
            MallRoomsMeshes.Merge(shell, "LockerShell");

            // The door hangs on its left edge and opens outward. Slats at eye height: the view out, and the light in.
            float doorWidth = Width - 0.04f, centre = doorWidth * 0.5f;
            Transform door = Group("Door", root);
            door.localPosition = new Vector3(-centre, 0f, Depth);
            Box("Lower", door, new Vector3(centre, 0.72f, 0f), new Vector3(doorWidth, 1.28f, Sheet), steel, false);
            Box("Upper", door, new Vector3(centre, 1.86f, 0f), new Vector3(doorWidth, 0.22f, Sheet), steel, false);
            foreach (float edge in new[] { 0.03f, doorWidth - 0.03f })
                Box("SlatStile", door, new Vector3(edge, 1.555f, 0f), new Vector3(0.06f, 0.39f, Sheet), steel, false);
            for (int i = 0; i < 5; i++)
            {
                GameObject slat = Box("Slat", door, new Vector3(centre, 1.405f + i * 0.075f, 0f), new Vector3(doorWidth - 0.12f, 0.018f, 0.035f), steel, false);
                slat.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            }
            Box("Handle", door, new Vector3(doorWidth - 0.09f, 1.05f, 0.03f), new Vector3(0.03f, 0.16f, 0.04f), dark, false);
            Box("Vent", door, new Vector3(centre, 0.3f, 0.012f), new Vector3(doorWidth * 0.5f, 0.1f, 0.006f), dark, false);
            MallRoomsMeshes.Merge(door, "LockerDoor");

            // One solid block: nothing walks or sees into it, and a monster's line of sight stops at it.
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, Height * 0.5f, Depth * 0.5f);
            solid.size = new Vector3(Width, Height, Depth);
            // Placed after the NavMesh was baked: it cuts its own footprint out.
            var carve = root.gameObject.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.center = solid.center;
            carve.size = solid.size;
            carve.carving = true;

            // Feet at floor level (not up on the plinth): standing eyes then look out between two slats.
            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(0f, 0.02f, Depth * 0.5f);
            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(0f, 0.05f, Depth + 0.6f);
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            // Gone with its floor (and back with it next run), like any fitting.
            if (floor != null) root.gameObject.AddComponent<SectionProp>().EditorSetup(floor);
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(index, door, inside, exit, floor);
        }
    }
}

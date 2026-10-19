using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using Plan = Abandoned.EditorTools.IronFactoryMachinePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The factory's hiding places, all run by <see cref="HidingLocker"/> (same rules as the mall's lockers,
    /// only the look and the prompt differ): worker lockers, maintenance bays under the presses, cold annealing
    /// furnaces and electrical cabinets. Each is built in the locker's own space: the origin on the floor at the
    /// back, Z out through the door, a door hinged on its left edge with slats at eye height to look out through.
    /// </summary>
    public static class IronFactoryHiding
    {
        private const float Sheet = .03f;

        public static int Build(Transform root, IronFactoryArt art, IronFactoryClearance clearance)
        {
            Transform group = Group("HidingPlaces", root);
            int index = 0;
            foreach (Plan.Spot spot in Plan.Spots)
            {
                string problem = clearance.Problem(Plan.SpotBounds(spot));
                if (problem != null)
                {
                    Debug.LogWarning($"[IronFactory] Left out the {spot.Kind} at {spot.Back}: it would block {problem}.");
                    continue;
                }
                // HidingLocker indices run 0-126 per level; the plan holds far fewer.
                Spot(group, index++, spot, art);
            }
            return index;
        }

        private static void Spot(Transform parent, int index, Plan.Spot spot, IronFactoryArt art)
        {
            Vector3 size = Plan.SpotSize(spot.Kind);
            float w = size.x, h = size.y, d = size.z;
            Transform root = Group($"{spot.Kind}_{index:00}", parent);
            root.SetPositionAndRotation(new Vector3(spot.Back.x, 0f, spot.Back.y), Quaternion.LookRotation(new Vector3(spot.Out.x, 0f, spot.Out.y)));

            Material shell = spot.Kind switch
            {
                HideSpotKind.Furnace => art.Lining,
                HideSpotKind.Machine => art.Safety,
                HideSpotKind.Cabinet => art.Steel,
                _ => art.Rust,
            };
            float wall = spot.Kind == HideSpotKind.Furnace ? .3f : Sheet;
            float doorWidth = spot.Kind == HideSpotKind.Furnace ? 1f : w - .04f;
            float doorHeight = spot.Kind == HideSpotKind.Furnace ? 1.95f : h - .04f;

            // Walls you see from inside, not one block: the occupant looks at them.
            Transform body = Group("Shell", root);
            float run = d - wall, middle = wall + run * .5f;
            Box("Back", body, new Vector3(0f, h * .5f, wall * .5f), new Vector3(w, h, wall), shell, false);
            foreach (float side in new[] { -1f, 1f })
                Box("Side", body, new Vector3(side * (w - wall) * .5f, h * .5f, middle), new Vector3(wall, h, run), shell, false);
            Box("Top", body, new Vector3(0f, h - wall * .5f, middle), new Vector3(w, wall, run), shell, false);
            if (spot.Kind == HideSpotKind.Furnace)
            {
                // A brick front with the door opening in it, and a flue.
                float jamb = (w - doorWidth) * .5f;
                foreach (float side in new[] { -1f, 1f })
                    Box("Front", body, new Vector3(side * (doorWidth + jamb) * .5f, h * .5f, d - wall * .5f), new Vector3(jamb, h, wall), shell, false);
                Box("Lintel", body, new Vector3(0f, (doorHeight + h) * .5f, d - wall * .5f), new Vector3(doorWidth, h - doorHeight, wall), shell, false);
                Box("Flue", body, new Vector3(0f, h + .9f, d * .4f), new Vector3(.45f, 1.8f, .45f), art.Steel, false);
            }
            if (spot.Kind == HideSpotKind.Machine)
                Box("Hazard", body, new Vector3(0f, h - .12f, d + .01f), new Vector3(w, .2f, .02f), art.Steel, false);

            // L5: glowing pilot light on each hiding spot (emissive only: zero dynamic light cost, readable in darkness)
            PilotLight(body, spot.Kind, w, h, d, wall);

            Transform door = Door(root, spot.Kind == HideSpotKind.Furnace ? art.Steel : shell, doorWidth, doorHeight, d, art);

            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, h * .5f, d * .5f);
            solid.size = new Vector3(w, h, d);
            var carve = root.gameObject.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.center = solid.center;
            carve.size = solid.size;
            carve.carving = true;

            // Feet on the floor: standing eyes look out between the slats.
            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(0f, .02f, Mathf.Min(d * .5f, .55f));
            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(0f, .05f, d + .6f);
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            // On the machinery floor, which never falls: nothing to vanish with.
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(index, door, inside, exit, null, spot.Kind);
        }

        // Hinged on its left edge at the front; HidingLocker swings it.
        private static Transform Door(Transform root, Material material, float width, float height, float depth, IronFactoryArt art)
        {
            float centre = width * .5f;
            Transform door = Group("Door", root);
            door.localPosition = new Vector3(-centre, 0f, depth);
            const float slatsFrom = 1.4f, slatsTo = 1.8f;
            Box("Lower", door, new Vector3(centre, slatsFrom * .5f, 0f), new Vector3(width, slatsFrom, Sheet), material, false);
            Box("Upper", door, new Vector3(centre, (slatsTo + height) * .5f, 0f), new Vector3(width, height - slatsTo, Sheet), material, false);
            for (int i = 0; i < 5; i++)
            {
                GameObject slat = Box("Slat", door, new Vector3(centre, slatsFrom + .04f + i * .075f, 0f),
                    new Vector3(width - .12f, .018f, .035f), material, false);
                slat.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            }
            Box("Handle", door, new Vector3(width - .1f, 1.05f, .03f), new Vector3(.03f, .16f, .04f), art.Bed, false);
            return door;
        }

        private static void PilotLight(Transform body, HideSpotKind kind, float w, float h, float d, float wall)
        {
            switch (kind)
            {
                case HideSpotKind.Furnace:
                    // Faint blue pilot burner flame inside the cold hearth
                    Material flame = Emissive("IronFactory_PilotFlame", new Color(.2f, .7f, 1f), new Color(.4f, 1.8f, 2.5f));
                    Box("PilotBurner", body, new Vector3(-.35f, .12f, wall + .35f), new Vector3(.05f, .12f, .05f), flame, false);
                    break;
                case HideSpotKind.Cabinet:
                    // Amber fuse indicator lamp on the front face
                    Material fuse = Emissive("IronFactory_FuseLamp", new Color(1f, .6f, .1f), new Color(2.5f, 1.4f, .2f));
                    Box("FuseLamp", body, new Vector3(w * .5f - .14f, h - .22f, d + .01f), new Vector3(.04f, .04f, .02f), fuse, false);
                    break;
                case HideSpotKind.Machine:
                    // Dim caution yellow strip on the maintenance bay opening
                    Material caution = Emissive("IronFactory_CautionLamp", new Color(1f, .85f, .2f), new Color(2.2f, 1.8f, .3f));
                    Box("CautionMarker", body, new Vector3(0f, h - .06f, d + .02f), new Vector3(.24f, .04f, .02f), caution, false);
                    break;
                default: // Lockers
                    // Pale green inspection tag lamp above the locker frame
                    Material tag = Emissive("IronFactory_LockerTag", new Color(.4f, .9f, .5f), new Color(.8f, 2.2f, 1f));
                    Box("TagLight", body, new Vector3(0f, h - .08f, d + .01f), new Vector3(.18f, .04f, .02f), tag, false);
                    break;
            }
        }

        private static Material Emissive(string name, Color baseCol, Color emitCol)
        {
            Material m = GetMaterial(name, baseCol);
            m.SetColor("_EmissionColor", emitCol);
            m.EnableKeyword("_EMISSION");
            return m;
        }
    }
}

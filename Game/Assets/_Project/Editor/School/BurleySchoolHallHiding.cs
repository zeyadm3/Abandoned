using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Four usable tall lockers beside protected hall floors, with the existing host-authoritative hiding system.</summary>
    public static class BurleySchoolHallHiding
    {
        public static void Build(Transform parent, BurleySchoolArt art)
        {
            int index = 0;
            for (int floor = 0; floor < 2; floor++)
            foreach (float x in new[] { 29.65f, 57.5f })
                Locker(parent, index++, new Vector3(x, floor * L.StoryHeight, 39.78f), art);
        }

        private static void Locker(Transform parent, int index, Vector3 at, BurleySchoolArt art)
        {
            const float width = .92f, height = 2.15f, depth = .84f, sheet = .025f;
            Transform root = Group($"TallHidingLocker_{index:00}", parent);
            root.SetPositionAndRotation(at, Quaternion.Euler(0f, 180f, 0f));
            Box("BackSheet", root, new Vector3(0f, height * .5f, sheet * .5f), new Vector3(width, height, sheet), art.LockerGreen, false);
            foreach (float side in new[] { -1f, 1f })
                Box("SideSheet", root, new Vector3(side * (width - sheet) * .5f, height * .5f, depth * .5f), new Vector3(sheet, height, depth), art.LockerGreen, false);
            Box("TopSheet", root, new Vector3(0f, height - .02f, depth * .5f), new Vector3(width, .04f, depth), art.LockerGreen, false);
            Box("BaseSheet", root, new Vector3(0f, .025f, depth * .5f), new Vector3(width, .05f, depth), art.BrickDark, false);
            Box("HangingRail", root, new Vector3(0f, 1.99f, .32f), new Vector3(.8f, .025f, .025f), art.MetalSteel, false);

            Transform door = Group("HingedDoor", root);
            door.localPosition = new Vector3(-width * .5f, 0f, depth);
            Box("LowerDoor", door, new Vector3(width * .5f, .7f, 0f), new Vector3(width - .035f, 1.4f, sheet), art.LockerGreen, false);
            Box("UpperDoor", door, new Vector3(width * .5f, 1.995f, 0f), new Vector3(width - .035f, .27f, sheet), art.LockerGreen, false);
            foreach (float side in new[] { -1f, 1f })
                Box("VentStile", door, new Vector3(width * .5f + side * .407f, 1.62f, 0f), new Vector3(.06f, .44f, sheet), art.LockerGreen, false);
            for (int slat = 0; slat < 6; slat++)
            {
                GameObject blade = Box("SightSlat", door, new Vector3(width * .5f, 1.43f + slat * .073f, 0f),
                    new Vector3(.76f, .022f, .04f), art.LockerGreen, false);
                blade.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            }
            Box("HandleRecess", door, new Vector3(width - .13f, 1.06f, .022f), new Vector3(.09f, .26f, .02f), art.BrickDark, false);
            Box("Handle", door, new Vector3(width - .13f, 1.06f, .054f), new Vector3(.025f, .18f, .045f), art.MetalSteel, false);
            Box("InspectionMarker", door, new Vector3(width * .5f, 2.04f, .02f), new Vector3(.16f, .035f, .015f), art.ExitGlow, false);

            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, height * .5f, depth * .5f);
            solid.size = new Vector3(width, height, depth);
            var obstacle = root.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = solid.center;
            obstacle.size = solid.size;
            obstacle.carving = true;
            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(0f, .05f, .43f);
            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(0f, .05f, depth + .7f);
            // All four stand on the explicitly protected hallway strip, so no collapsing floor is referenced.
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(index, door, inside, exit, null);
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }
    }
}

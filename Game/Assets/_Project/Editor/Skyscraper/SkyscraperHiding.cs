using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Installs 11 themed hiding places across the Skyscraper using HidingLocker.
    /// Distributed across Ground Lobby, Mechanical Deck, Restaurant Dining, Kitchen, and Penthouse VIP.
    /// </summary>
    public static class SkyscraperHiding
    {
        public static int Build(Transform root, SkyscraperArt art)
        {
            Transform group = Group("HidingPlaces", root);
            int index = 0;

            // 1. Ground Lobby
            Spot(group, index++, new Vector3(6f, L.GroundY, -1.0f), Vector3.back, HideSpotKind.Cabinet, art);
            Spot(group, index++, new Vector3(-6f, L.GroundY, -2.5f), Vector3.forward, HideSpotKind.Locker, art);

            // 2. Flooded Floor (Story 3, y = 12m)
            Spot(group, index++, new Vector3(-6f, 12.0f, 2.0f), Vector3.forward, HideSpotKind.Cabinet, art);

            // 3. Mechanical Deck (y = 20m)
            Spot(group, index++, new Vector3(-6f, L.MechanicalY, 6f), Vector3.forward, HideSpotKind.Cabinet, art);
            Spot(group, index++, new Vector3( 6f, L.MechanicalY, 6f), Vector3.forward, HideSpotKind.Cabinet, art);
            Spot(group, index++, new Vector3(10f, L.MechanicalY, 10f), Vector3.back, HideSpotKind.Machine, art);

            // 4. Looted Office Floor (Story 8, y = 32m)
            Spot(group, index++, new Vector3(-6f, 32.0f, 6.0f), Vector3.forward, HideSpotKind.Closet, art);

            // 5. Restaurant Floor (y = 44m)
            // Waiter Service Credenzas in Dining Room
            Spot(group, index++, new Vector3(-4f, L.RestaurantY, 17f), Vector3.back, HideSpotKind.Cabinet, art);
            Spot(group, index++, new Vector3( 4f, L.RestaurantY,  3f), Vector3.forward, HideSpotKind.Cabinet, art);

            // Lounge Bar Cabinet
            Spot(group, index++, new Vector3(12.5f, L.RestaurantY, -2f), Vector3.left, HideSpotKind.Cabinet, art);

            // Kitchen Staff Lockers
            Spot(group, index++, new Vector3(-14.5f, L.RestaurantY, -6.5f), Vector3.forward, HideSpotKind.Locker, art);
            Spot(group, index++, new Vector3(-13.5f, L.RestaurantY, -6.5f), Vector3.forward, HideSpotKind.Locker, art);

            // Cold Store Walk-in Refrigeration Bay
            Spot(group, index++, new Vector3(-14.0f, L.RestaurantY, -4.5f), Vector3.right, HideSpotKind.Closet, art);

            // Penthouse Executive Wardrobe Closet
            Spot(group, index++, new Vector3(14.5f, L.RestaurantY, 6.0f), Vector3.left, HideSpotKind.Closet, art);

            // 6. Roof Terrace Window Washer Locker (y = 50m)
            Spot(group, index++, new Vector3(2f, L.RoofY, 19f), Vector3.back, HideSpotKind.Locker, art);

            return index;
        }

        private static void Spot(Transform parent, int index, Vector3 at, Vector3 forward, HideSpotKind kind, SkyscraperArt art)
        {
            Vector3 size = kind switch
            {
                HideSpotKind.Locker => new(0.85f, 2.0f, 0.85f),
                HideSpotKind.Closet => new(1.2f, 2.2f, 1.0f),
                HideSpotKind.Machine => new(1.4f, 1.8f, 1.4f),
                _ => new(1.1f, 1.8f, 0.9f) // Cabinet
            };

            float w = size.x, h = size.y, d = size.z;
            Transform root = Group($"{kind}_{index:00}", parent);
            root.SetPositionAndRotation(at, Quaternion.LookRotation(forward));

            Material mat = kind switch
            {
                HideSpotKind.Locker => art.Steel,
                HideSpotKind.Machine => art.Steel,
                HideSpotKind.Closet => art.Mahogany,
                _ => art.Mahogany // Cabinet
            };

            Transform shell = Group("Shell", root);
            Box("Back", shell, new Vector3(0f, h * 0.5f, 0.02f), new Vector3(w, h, 0.04f), mat, false);
            Box("SideL", shell, new Vector3(-w * 0.48f, h * 0.5f, d * 0.5f), new Vector3(0.04f, h, d), mat, false);
            Box("SideR", shell, new Vector3( w * 0.48f, h * 0.5f, d * 0.5f), new Vector3(0.04f, h, d), mat, false);
            Box("Top", shell, new Vector3(0f, h - 0.02f, d * 0.5f), new Vector3(w, 0.04f, d), mat, false);

            // Hinged Door
            Transform door = Group("Door", root);
            door.localPosition = new Vector3(-w * 0.48f, 0f, d);
            Box("DoorLeaf", door, new Vector3(w * 0.5f, h * 0.5f, 0.02f), new Vector3(w * 0.96f, h * 0.96f, 0.04f), mat, false);
            Box("Handle", door, new Vector3(w * 0.85f, h * 0.5f, 0.06f), new Vector3(0.04f, 0.15f, 0.06f), art.Brass, false);

            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, h * 0.5f, d * 0.5f);
            solid.size = size;

            var carve = root.gameObject.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.center = solid.center;
            carve.size = solid.size;
            carve.carving = true;

            Transform inside = Group("Inside", root);
            inside.localPosition = new Vector3(0f, 0.02f, d * 0.5f);

            Transform exit = Group("Exit", root);
            exit.localPosition = new Vector3(0f, 0.05f, d + 0.6f);

            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            root.gameObject.AddComponent<HidingLocker>().EditorSetup(index, door, inside, exit, null, kind);
        }
    }
}

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

            Spot(group,index++,new Vector3(15,0,18),Vector3.left,HideSpotKind.Cabinet,art);
            foreach(float y in new[]{120f,200f,280f})
                Spot(group,index++,new Vector3(16,y,18),Vector3.left,HideSpotKind.Locker,art);
            Spot(group,index++,new Vector3(-17,404f,8),Vector3.right,HideSpotKind.Locker,art);
            Spot(group,index++,new Vector3(17,404f,11),Vector3.left,HideSpotKind.Cabinet,art);
            Spot(group,index++,new Vector3(16,404f,19.5f),Vector3.left,HideSpotKind.Closet,art);
            Spot(group,index++,new Vector3(14,410f,18),Vector3.back,HideSpotKind.Locker,art);

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

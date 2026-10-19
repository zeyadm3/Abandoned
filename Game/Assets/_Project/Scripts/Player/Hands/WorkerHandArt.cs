using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Player
{
    /// <summary>Loads the authored hand art; the fallback keeps existing saved players usable until art is rebuilt.</summary>
    public static class WorkerHandArt
    {
        public static Transform Sleeve(Transform parent, int side) => Build(parent, side, sleeve: true);
        public static Transform Glove(Transform parent, int side) => Build(parent, side, sleeve: false);

        private static Transform Build(Transform parent, int side, bool sleeve)
        {
            string name = (sleeve ? "CrewSleeve" : "CrewGlove") + (side == 0 ? "L" : "R");
            GameObject source = Resources.Load<GameObject>("Characters/" + name);
            GameObject root;
            if (source != null) root = Object.Instantiate(source, parent, false);
            else
            {
                root = new GameObject(name);
                root.transform.SetParent(parent, false);
                GameObject body = GameObject.CreatePrimitive(sleeve ? PrimitiveType.Capsule : PrimitiveType.Cube);
                Collider collider = body.GetComponent<Collider>();
                if (collider != null) { collider.enabled = false; Object.Destroy(collider); }
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = sleeve ? new Vector3(.11f, .5f, .12f) : new Vector3(.09f, .05f, .12f);
                body.GetComponent<Renderer>().sharedMaterial = CharacterSurfaceLibrary.Get(sleeve ? "CrewFabric" : "Leather");
            }
            root.name = "FirstPersonHands_" + name;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }
            return root.transform;
        }
    }
}

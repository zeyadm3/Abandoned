using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelSolids
    {
        public static void Fit(Transform root)
        {
            foreach (SectionProp binding in root.GetComponentsInChildren<SectionProp>())
            {
                Transform piece = binding.transform;
                HidingLocker hide = piece.GetComponentInChildren<HidingLocker>();
                if (hide != null)
                {
                    if (piece.GetComponent<IntentionalInvisibleCollider>() == null)
                    {
                        piece.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Closed hiding occupancy shell with an animated louvred door.");
                    }
                    continue;
                }
                BoxCollider footprint = piece.GetComponent<BoxCollider>();
                MeshFilter [] parts = piece.GetComponentsInChildren<MeshFilter>();
                if (footprint == null || parts.Length == 0)
                {
                    continue;
                }
                Object.DestroyImmediate(footprint);
                // Every fitted Motel part is an unscaled cube mesh on its own scaled transform.
                foreach (MeshFilter part in parts)
                {
                    if (part.sharedMesh == null || part.GetComponent<Collider>() != null)
                    {
                        continue;
                    }
                    BoxCollider fitted = part.gameObject.AddComponent<BoxCollider>();
                    fitted.center = part.sharedMesh.bounds.center;
                    fitted.size = part.sharedMesh.bounds.size;
                }
            }
            // The alcove cabinet stands on courtyard ground rather than a section, but uses the same containment.
            foreach (HidingLocker hide in root.GetComponentsInChildren<HidingLocker>())
            {
                if (hide.GetComponent<IntentionalInvisibleCollider>() == null)
                {
                    hide.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Closed hiding occupancy shell with an animated door.");
                }
            }
        }
    }
}

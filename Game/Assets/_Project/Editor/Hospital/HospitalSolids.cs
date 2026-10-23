using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
namespace Abandoned.Hospital
{
    /// <summary>Collision follows fitted objects, leaving the air under a table as air.</summary>
    public static class HospitalSolids
    {
        public static void Fit(HospitalStructure floors)
        {
            foreach (Transform room in floors.Context.Props.Root)
            {
                foreach (Transform piece in room)
                {
                    HidingLocker hide = piece.GetComponentInChildren<HidingLocker>();
                    if (hide != null)
                    {
                        if (piece.GetComponent<IntentionalInvisibleCollider>() == null)
                        {
                            piece.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("The hiding container uses a closed occupancy shell while its door animates.");
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
                    foreach (MeshFilter part in parts)
                    {
                        if (part.sharedMesh == null || part.GetComponent<Collider>() != null)
                        {
                            continue;
                        }
                        var solid = part.gameObject.AddComponent<MeshCollider>();
                        solid.sharedMesh = part.sharedMesh;
                    }
                }
            }
        }
    }
}

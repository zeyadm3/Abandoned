using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Registers structural sections with the host-authoritative StructureSimulation.
    /// Manages load capacity, fracture stages, collapse debris, and SectionNavCarver AI updates.
    /// </summary>
    public static class SkyscraperStructure
    {
        public static StructureSimulation Apply(StructureSceneSetup setup,
            IEnumerable<Transform> restaurantSlabs,
            HashSet<Transform> collapsibleDining,
            IEnumerable<Transform> mechPieces,
            HashSet<Transform> collapsibleCatwalks,
            IEnumerable<Transform> stairLandings,
            HashSet<Transform> collapsibleLandings,
            IEnumerable<Transform> intermediatePieces = null,
            HashSet<Transform> collapsibleIntermediate = null)
        {
            // 1. Restaurant Floor Tiles
            foreach (Transform slab in restaurantSlabs)
            {
                bool collapsible = collapsibleDining.Contains(slab);
                setup.AddSection(slab, SectionType.Floor, collapsible, health: collapsible ? 0.65f : 1.0f,
                    capacity: collapsible ? 0.55f : 1.0f);

                if (collapsible)
                    SectionNavCarver.EditorAdd(slab.gameObject, slab.GetComponent<BoxCollider>().size);
                else if (slab.position.z < 6f || Mathf.Abs(slab.position.x) < 2.1f)
                    slab.gameObject.AddComponent<SectionKeepClear>(); // Keep elevator lobby clear
            }

            // 2. Mechanical Catwalks
            foreach (Transform piece in mechPieces)
            {
                bool collapsible = collapsibleCatwalks.Contains(piece);
                setup.AddSection(piece, SectionType.Walkway, collapsible, health: collapsible ? 0.60f : 1.0f,
                    capacity: collapsible ? 0.50f : 1.0f);

                if (collapsible)
                    SectionNavCarver.EditorAdd(piece.gameObject, piece.GetComponent<BoxCollider>().size);
            }

            // 3. Stairwell Landings
            foreach (Transform landing in stairLandings)
            {
                bool collapsible = collapsibleLandings.Contains(landing);
                if (!collapsible) landing.gameObject.AddComponent<SectionKeepClear>();
                setup.AddSection(landing, SectionType.Stair, collapsible, health: collapsible ? 0.70f : 1.0f,
                    capacity: collapsible ? 0.60f : 1.0f);

                if (collapsible)
                    SectionNavCarver.EditorAdd(landing.gameObject, landing.GetComponent<BoxCollider>().size);
            }

            // 4. Intermediate & Roof Sections
            if (intermediatePieces != null)
            {
                foreach (Transform piece in intermediatePieces)
                {
                    bool collapsible = collapsibleIntermediate != null && collapsibleIntermediate.Contains(piece);
                    setup.AddSection(piece, SectionType.Floor, collapsible, health: collapsible ? 0.65f : 1.0f,
                        capacity: collapsible ? 0.55f : 1.0f);

                    if (collapsible)
                        SectionNavCarver.EditorAdd(piece.gameObject, piece.GetComponent<BoxCollider>().size);
                }
            }

            // Tower stability tuned for high-altitude sway and structural suspense
            return setup.AddSimulation(stability: 0.70f, seed: 6842);
        }
    }
}

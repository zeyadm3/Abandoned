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
                    SectionNavCarver.EditorAdd(slab.gameObject, new Vector3(3.9f, 0.4f, 3.9f));
                else if (Mathf.Abs(slab.position.x) < 3.0f && slab.position.z < -2.0f)
                    slab.gameObject.AddComponent<SectionKeepClear>(); // Keep elevator lobby clear
            }

            // 2. Mechanical Catwalks
            foreach (Transform piece in mechPieces)
            {
                bool collapsible = collapsibleCatwalks.Contains(piece);
                setup.AddSection(piece, SectionType.Walkway, collapsible, health: collapsible ? 0.60f : 1.0f,
                    capacity: collapsible ? 0.50f : 1.0f);

                if (collapsible)
                    SectionNavCarver.EditorAdd(piece.gameObject, new Vector3(6.0f, 0.16f, 1.4f));
            }

            // 3. Stairwell Landings
            foreach (Transform landing in stairLandings)
            {
                bool collapsible = collapsibleLandings.Contains(landing);
                setup.AddSection(landing, SectionType.Stair, collapsible, health: collapsible ? 0.70f : 1.0f,
                    capacity: collapsible ? 0.60f : 1.0f);

                if (collapsible)
                    SectionNavCarver.EditorAdd(landing.gameObject, new Vector3(5.6f, 0.3f, 1.2f));
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
                        SectionNavCarver.EditorAdd(piece.gameObject, new Vector3(4.0f, 0.3f, 4.0f));
                }
            }

            // Tower stability tuned for high-altitude sway and structural suspense
            return setup.AddSimulation(stability: 0.70f, seed: 6842);
        }
    }
}

using System.Collections.Generic;
using Abandoned.Structure;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Floor-mounted furniture follows the same replicated collapse as its supporting slab.</summary>
    public static class BurleySchoolInteriorSupport
    {
        public static void BindHangingFixtures(Transform root, BurleySchoolStructure.Result structure)
        {
            foreach (LightFixture fixture in root.GetComponentsInChildren<LightFixture>(true))
            {
                Vector3 at = fixture.transform.position;
                if (at.y < 3.1f || at.y > 3.8f) continue;
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / BurleySchoolLayout.Tile), Mathf.FloorToInt(at.z / BurleySchoolLayout.Tile));
                foreach (BurleySchoolStructure.TilePiece tile in structure.Pieces)
                {
                    if (tile.Floor != 1 || !tile.Collapsible || tile.Cell != cell) continue;
                    var prop = Abandoned.EditorTools.SerializedWiring.GetOrAdd<SectionProp>(fixture.gameObject);
                    prop.EditorSetup(tile.Transform.GetComponent<StructuralSection>());
                    break;
                }
            }
        }

        public static void Bind(Transform root, BurleySchoolStructure.Result structure)
        {
            Transform interiors = root.Find("SchoolInteriors");
            if (interiors == null) return;
            var candidates = new HashSet<Transform>();
            foreach (MeshRenderer renderer in interiors.GetComponentsInChildren<MeshRenderer>(true))
            {
                Transform item = renderer.transform.parent;
                if (item == null || Mathf.Abs(item.position.y - BurleySchoolLayout.StoryHeight) > .03f) continue;
                Bounds bounds = renderer.bounds;
                foreach (Renderer part in item.GetComponentsInChildren<Renderer>(true)) bounds.Encapsulate(part.bounds);
                if (bounds.min.y < 3.85f || bounds.min.y > 4.18f || bounds.max.y < 4.25f) continue;
                candidates.Add(item);
            }

            foreach (Transform item in candidates)
            {
                // A classroom container must not hide all of its desks because one distant slab gives way.
                bool container = false;
                foreach (Transform other in candidates)
                    if (other != item && other.IsChildOf(item)) { container = true; break; }
                if (container) continue;
                int x = Mathf.FloorToInt(item.position.x / BurleySchoolLayout.Tile);
                int z = Mathf.FloorToInt(item.position.z / BurleySchoolLayout.Tile);
                foreach (BurleySchoolStructure.TilePiece tile in structure.Pieces)
                {
                    if (tile.Floor != 1 || !tile.Collapsible || tile.Cell != new Vector2Int(x, z)) continue;
                    var prop = Abandoned.EditorTools.SerializedWiring.GetOrAdd<SectionProp>(item.gameObject);
                    prop.EditorSetup(tile.Transform.GetComponent<StructuralSection>());
                    break;
                }
            }
        }
    }
}

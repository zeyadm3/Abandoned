using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ's sectional car garage door. Pivot: bottom centre of the opening, forward = outside; the
    /// overhead tracks run back inside. <see cref="open"/> rolls the sections up the side tracks and back
    /// under the ceiling, each section carrying its own collider, so any position blocks exactly what
    /// you see. Set it in the inspector; its placement is saved with the HQ scene (HqPlacementSync).
    /// </summary>
    [ExecuteAlways]
    public class GarageDoor : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float open;
        [SerializeField] private Transform[] sections;
        [SerializeField, Min(0.1f)] private float width = 8f;
        [SerializeField, Min(0.1f)] private float height = 3.4f;

        public float Open => open;

        private void OnEnable() => Apply();

        private void OnValidate() => Apply();

        /// <summary>Lays every section along the track: up the opening, then back overhead.</summary>
        public void Apply()
        {
            if (sections == null || sections.Length == 0) return;
            float section = height / sections.Length, lift = open * height;
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i] == null) continue;
                // Distance of this section's bottom edge along the track from the floor.
                float along = i * section + lift;
                if (along + section <= height)
                    sections[i].SetLocalPositionAndRotation(new Vector3(0f, along, 0f), Quaternion.identity);
                else if (along >= height)
                    sections[i].SetLocalPositionAndRotation(new Vector3(0f, height, -(along - height)), Quaternion.Euler(-90f, 0f, 0f));
                else
                {
                    // Rounding the top of the track: tip it partway over.
                    float t = (along + section - height) / section;
                    sections[i].SetLocalPositionAndRotation(new Vector3(0f, along, -t * section * 0.5f), Quaternion.Euler(-90f * t, 0f, 0f));
                }
            }
        }

        private void OnDrawGizmos()
        {
            // The opening it's meant to fill, so placing it by eye in the HQ is easy.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.65f, 0.25f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, 0.05f));
            Gizmos.DrawRay(new Vector3(0f, height * 0.5f, 0f), Vector3.forward * 1.2f);
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform[] doorSections, float openingWidth, float openingHeight, float openAmount)
        {
            sections = doorSections;
            width = openingWidth;
            height = openingHeight;
            open = openAmount;
            Apply();
        }

        public void EditorSetOpen(float amount)
        {
            open = Mathf.Clamp01(amount);
            Apply();
        }
#endif
    }
}

using UnityEngine;

namespace Abandoned.Structure
{
    // Serialized authoring dimensions let editor audits share the flight the builder actually made.
    public sealed class SectionFlightGeometry : MonoBehaviour
    {
        [SerializeField] private float length = 8f;
        [SerializeField] private float rise = 4f;
        [SerializeField] private float clearWidth = 3f;
        public float Length => length;
        public float Rise => rise;
        public float ClearWidth => clearWidth;
#if UNITY_EDITOR
        public void EditorSetup(float run,float height,float width) { length=run;rise=height;clearWidth=width; }
#endif
    }
}

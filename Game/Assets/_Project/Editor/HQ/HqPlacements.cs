using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Hand-placed HQ pieces that survive regeneration: HqBuilder rebuilds the scene from code, then puts
    /// these back where you left them. Written by HqPlacementSync when the HQ scene is saved.
    /// </summary>
    public class HqPlacements : ScriptableObject
    {
        public const string Path = "Assets/_Project/Data/Company/HqPlacements.asset";

        // Default: closing the garage's own 8 x 3.4 m opening in the south wall, facing the yard.
        [SerializeField] private Vector3 garageDoorPosition = new(6f, 0f, -0.1f);
        [SerializeField] private Vector3 garageDoorEuler = new(0f, 180f, 0f);
        [SerializeField] private Vector3 garageDoorScale = Vector3.one;
        [SerializeField, Range(0f, 1f)] private float garageDoorOpen;

        public Vector3 GarageDoorPosition => garageDoorPosition;
        public Quaternion GarageDoorRotation => Quaternion.Euler(garageDoorEuler);
        public Vector3 GarageDoorScale => garageDoorScale;
        public float GarageDoorOpen => garageDoorOpen;

        public bool SetGarageDoor(Transform door, float open)
        {
            Vector3 euler = door.eulerAngles;
            bool changed = door.position != garageDoorPosition || euler != garageDoorEuler || door.localScale != garageDoorScale
                           || !Mathf.Approximately(open, garageDoorOpen);
            garageDoorPosition = door.position;
            garageDoorEuler = euler;
            garageDoorScale = door.localScale;
            garageDoorOpen = open;
            return changed;
        }

        public void ResetGarageDoor()
        {
            garageDoorPosition = new Vector3(6f, 0f, -0.1f);
            garageDoorEuler = new Vector3(0f, 180f, 0f);
            garageDoorScale = Vector3.one;
            garageDoorOpen = 0f;
        }
    }
}

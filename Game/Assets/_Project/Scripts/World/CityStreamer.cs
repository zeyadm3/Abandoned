using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Shows only the part of the city anyone can see (Docs/CITY_PLAN.md, "How it stays fast"). Every cell was built
    /// twice: near, the full thing with its colliders, and far, the same ground and massing merged into one mesh of
    /// one material. Close to the viewer the near copy is on, further out the far one, and past that nothing at all.
    ///
    /// Cells are compared in this object's own space, so when the city hangs under the world's movable root
    /// (<see cref="WorldRoot"/>) and the land shifts under an anchor change, the distances still come out right.
    /// A slice of the cells is looked at each frame rather than all of them, so a 6 km city never costs a spike.
    /// </summary>
    public class CityStreamer : MonoBehaviour
    {
        [Tooltip("Centre of each cell, in this object's space.")]
        [SerializeField] private Vector3[] centres = new Vector3[0];
        [Tooltip("The full cell: ground, roads, massing and every collider.")]
        [SerializeField] private GameObject[] near = new GameObject[0];
        [Tooltip("The same cell merged into one mesh, no colliders.")]
        [SerializeField] private GameObject[] far = new GameObject[0];
        [Tooltip("Within this the full cell is on. Keep it under the fog, or the swap is visible.")]
        [SerializeField] private float nearRange = 700f;
        [Tooltip("Past this a cell is switched off entirely. Keep it under the camera's far plane.")]
        [SerializeField] private float farRange = 1300f;
        [Tooltip("A cell has to move this much past a range before it switches back, so nothing flickers on a kerb.")]
        [SerializeField] private float hysteresis = 60f;
        [Tooltip("Cells looked at per frame. The whole city is walked every few frames.")]
        [SerializeField] private int perFrame = 96;

        private int cursor;
        private bool[] isNear, isFar;

        /// <summary>What the city is drawn around: the local player, or the van. Falls back to the main camera.</summary>
        public static Transform Viewer { get; set; }

        public static CityStreamer Current { get; private set; }

        /// <summary>How many cells show their near copy right now (the F1 line and the budget test read it).</summary>
        public int NearCells { get; private set; }

        /// <summary>The cells, for the F1 view and the budget test. Read only: the builder owns these.</summary>
        public System.Collections.Generic.IReadOnlyList<Vector3> Centres => centres;
        public System.Collections.Generic.IReadOnlyList<GameObject> NearCopies => near;
        public System.Collections.Generic.IReadOnlyList<GameObject> FarCopies => far;
        public float NearRange => nearRange;
        public float FarRange => farRange;

        private void Awake()
        {
            Current = this;
            isNear = new bool[near.Length];
            isFar = new bool[far.Length];
            // Everything starts off, so the first pass only ever has to turn things on.
            for (int i = 0; i < near.Length; i++)
            {
                if (near[i] != null) near[i].SetActive(false);
                if (i < far.Length && far[i] != null) far[i].SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            Transform eye = Viewer != null ? Viewer : Camera.main != null ? Camera.main.transform : null;
            if (eye == null || centres.Length == 0) return;
            Vector3 at = transform.InverseTransformPoint(eye.position);
            int count = Mathf.Min(perFrame, centres.Length);
            for (int n = 0; n < count; n++)
            {
                int i = cursor;
                cursor = (cursor + 1) % centres.Length;
                Apply(i, Distance(centres[i], at));
            }
        }

        // Flat distance: a cell two streets away is just as near whether you are on a roof or in the road.
        private static float Distance(Vector3 centre, Vector3 at) =>
            new Vector2(centre.x - at.x, centre.z - at.z).magnitude;

        private void Apply(int i, float distance)
        {
            bool wantNear = distance < (isNear[i] ? nearRange + hysteresis : nearRange);
            bool wantFar = !wantNear && distance < (isFar[i] ? farRange + hysteresis : farRange);
            if (wantNear != isNear[i])
            {
                isNear[i] = wantNear;
                if (near[i] != null) near[i].SetActive(wantNear);
                NearCells += wantNear ? 1 : -1;
            }
            if (wantFar == isFar[i]) return;
            isFar[i] = wantFar;
            if (i < far.Length && far[i] != null) far[i].SetActive(wantFar);
        }

        /// <summary>Called by the city builder once every cell is made.</summary>
        public void EditorSetup(Vector3[] cellCentres, GameObject[] nearCells, GameObject[] farCells)
        {
            centres = cellCentres;
            near = nearCells;
            far = farCells;
        }
    }
}

using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A level's own "inside the building" box, for levels too big for <see cref="HorrorConfig.Building"/>
    /// (one box shared by every level, sized for the mall). <see cref="RunHorrorDirector"/> uses it when the
    /// scene has one: who the total collapse catches, where the horror events may land.
    /// </summary>
    public class LevelBuildingBounds : MonoBehaviour
    {
        [SerializeField] private Bounds bounds = new(new Vector3(28f, 4f, 24f), new Vector3(56f, 16f, 48f));

        public static LevelBuildingBounds Current { get; private set; }
        public Bounds Bounds => bounds;

        private void OnEnable() => Current = this;

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

#if UNITY_EDITOR
        public void EditorSetup(Bounds building) => bounds = building;
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Gizmos.color = new Color(.4f, .8f, 1f, .35f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

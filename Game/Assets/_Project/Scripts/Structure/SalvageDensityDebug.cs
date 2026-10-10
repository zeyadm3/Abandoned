using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>F1 construction counters and slab-sized stock-batch bounds for the density pass.</summary>
    public sealed class SalvageDensityDebug : MonoBehaviour
    {
        [SerializeField] private int stockProps, packingDetails, sectionBatches;

#if UNITY_EDITOR
        public void EditorSetup(int props, int details, int batches)
        { stockProps = props; packingDetails = details; sectionBatches = batches; }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 380, 210, 375, 22),
                $"MALL STOCK: {stockProps} props · {packingDetails} packing details · {sectionBatches} batches");
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            Transform batches = transform.Find("SectionRenderBatches");
            if (batches == null) return;
            Gizmos.color = new Color(.35f, .75f, .80f, .4f);
            foreach (Transform batch in batches)
                Gizmos.DrawWireCube(batch.position + Vector3.up * 1.25f, new Vector3(4, 2.5f, 4));
        }
#endif
    }
}

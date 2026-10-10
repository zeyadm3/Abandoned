using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Loot
{
    /// <summary>
    /// Optional clearance metadata and focus presentation on an existing network loot prefab.
    /// Value, weight and carrying remain owned by LootItem, LootDefinition and the interaction handler.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LootItem), typeof(Grabbable))]
    public sealed class SalvageProp : MonoBehaviour
    {
        [SerializeField] private SalvageCategory category;
        [Tooltip("Optional collider-free outline shell child. Its material/mesh must already draw an outline.")]
        [SerializeField] private GameObject outlineVisual;

        private LootItem loot;
        private Grabbable grabbable;
        private NetworkPlayer localPlayer;
        private PlayerInteractor interactor;

        public SalvageCategory Category => category;
        public bool ValueReady => loot != null && loot.IsInitialized;
        public int SalvageValue => ValueReady ? loot.CurrentValue : 0;
        public float WeightKg => loot != null && loot.Definition != null ? loot.GameplayWeight : 0f;
        public bool IsFocused => isActiveAndEnabled && loot != null && !loot.IsShattered &&
            grabbable != null && !grabbable.IsPocketed && localPlayer != null && !localPlayer.IsDead &&
            interactor != null && interactor.isActiveAndEnabled && interactor.Target == grabbable;

        private void Awake()
        {
            loot = GetComponent<LootItem>();
            grabbable = GetComponent<Grabbable>();
            // A bad Inspector reference must never disable the pickup root or another scene object.
            if (outlineVisual != null &&
                (outlineVisual == gameObject || !outlineVisual.transform.IsChildOf(transform)))
            {
                Debug.LogError("[SalvageProp] Outline Visual must be a child of this loot prefab.", this);
                outlineVisual = null;
            }
            SetOutline(false);
        }

        private void LateUpdate()
        {
            ResolveLocalPlayer();
            SetOutline(IsFocused);
        }

        private void OnDisable() => SetOutline(false);

        /// <summary>
        /// Optional interaction-button entry point. True means a request was sent, not that the host
        /// accepted it. PlayerInteractor already sends the normal Interact input; do not wire it twice.
        /// </summary>
        public bool RequestPickup()
        {
            ResolveLocalPlayer();
            if (!IsFocused || localPlayer.Carrier == null) return false;
            // Keep host validation, physics ownership and shared carry on the existing path.
            InteractionService.Handler.RequestPickup(localPlayer.Carrier, grabbable);
            return true;
        }

        private void ResolveLocalPlayer()
        {
            if (localPlayer == NetworkPlayer.Local) return;
            localPlayer = NetworkPlayer.Local;
            interactor = localPlayer != null ? localPlayer.GetComponent<PlayerInteractor>() : null;
        }

        private void SetOutline(bool visible)
        {
            if (outlineVisual != null && outlineVisual.activeSelf != visible)
                outlineVisual.SetActive(visible);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || loot == null || loot.Definition == null) return;
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = IsFocused ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, loot.Definition.Size);
            Gizmos.matrix = previous;
        }
    }
}

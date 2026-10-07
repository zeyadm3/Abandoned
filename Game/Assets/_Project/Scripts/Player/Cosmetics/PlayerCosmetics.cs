using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Player
{
    /// <summary>
    /// A player's outfit (GDD 18) on every machine: the owner writes its choice, everyone tints the
    /// coverall (body and ragdoll parts) and puts the hat on the head (the standing body's crown, or
    /// the ragdoll's head). Your own hat only casts a shadow, so it never blocks your view.
    /// </summary>
    public class PlayerCosmetics : NetworkBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private CosmeticCatalog catalog;
        [SerializeField] private Transform body;
        [SerializeField] private Transform ragdollHead;
        [Tooltip("Renderers with this material are the coverall.")]
        [SerializeField] private Material coverallMaterial;

        private readonly NetworkVariable<CosmeticChoice> choice = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Renderer[] coverall;
        private MaterialPropertyBlock block;
        private Transform hatAnchor;
        private GameObject hat, accessory;
        private bool accessoryOnBody;
        private CosmeticChoice shown;
        private bool applied;

        public CosmeticCatalog Catalog => catalog;
        public CosmeticChoice Choice => choice.Value;
        public GameObject HatInstance => hat;

        private void Awake()
        {
            var found = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial == coverallMaterial) found.Add(r);
            coverall = found.ToArray();
            hatAnchor = new GameObject("HatAnchor").transform;
            hatAnchor.SetParent(transform, false);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner && catalog != null) choice.Value = catalog.FromProfile();
            Apply();
        }

        /// <summary>Owner: put on an outfit (and remember it).</summary>
        public void Wear(CosmeticChoice outfit)
        {
            if (!IsOwner || catalog == null) return;
            choice.Value = outfit;
            PlayerProfile.Wear(catalog.Coverall(outfit.Coverall)?.Id, catalog.Hat(outfit.Hat)?.Id, catalog.Accessory(outfit.Accessory)?.Id);
            Apply();
        }

        private void Update()
        {
            if (!applied || !shown.Equals(choice.Value)) Apply();
        }

        private void LateUpdate()
        {
            if (hat == null && accessory == null) return;
            bool down = ragdollHead != null && ragdollHead.gameObject.activeInHierarchy;
            if (down)
                hatAnchor.SetPositionAndRotation(ragdollHead.position + ragdollHead.up * 0.11f, ragdollHead.rotation);
            else if (body != null)
                hatAnchor.SetPositionAndRotation(body.TransformPoint(Vector3.up) - body.up * 0.04f, body.rotation);
            // A rucksack hung off a ragdoll's head would float: it stays out of sight until they're up.
            if (accessory != null && accessoryOnBody && accessory.activeSelf == down) accessory.SetActive(!down);
        }

        private void Apply()
        {
            if (catalog == null) return;
            CosmeticChoice c = choice.Value;
            shown = c;
            applied = true;
            CosmeticDefinition suit = catalog.Coverall(c.Coverall);
            block ??= new MaterialPropertyBlock();
            foreach (Renderer r in coverall)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor(BaseColor, suit != null ? suit.Color : Color.white);
                r.SetPropertyBlock(block);
            }

            if (hat != null) Destroy(hat);
            if (accessory != null) Destroy(accessory);
            hat = Wearable(catalog.Hat(c.Hat));
            CosmeticDefinition extra = catalog.Accessory(c.Accessory);
            accessory = Wearable(extra);
            accessoryOnBody = extra != null && extra.BodyMounted;
            LateUpdate();
        }

        private GameObject Wearable(CosmeticDefinition d)
        {
            if (d == null || d.HatPrefab == null) return null;
            GameObject worn = Instantiate(d.HatPrefab, hatAnchor, false);
            foreach (Collider col in worn.GetComponentsInChildren<Collider>()) Destroy(col);
            // Inside your own head it would fill the screen when you look around; you still see its shadow.
            if (IsOwner)
                foreach (Renderer r in worn.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            return worn;
        }

#if UNITY_EDITOR
        public void EditorSetup(CosmeticCatalog cosmetics, Transform bodyVisual, Transform head, Material suitMaterial)
        {
            catalog = cosmetics;
            body = bodyVisual;
            ragdollHead = head;
            coverallMaterial = suitMaterial;
        }
#endif
    }
}

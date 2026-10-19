using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
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
        [Abandoned.Core.OptionalReference, SerializeField] private Transform standingHead;
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
        private NetworkPlayer player;
        private int defaultSeat = -1;

        public CosmeticCatalog Catalog => catalog;

        /// <summary>The wardrobe's last purchase result ("" while none), for its status line.</summary>
        public string PurchaseMessage { get; private set; } = "";
        public bool PurchasePending { get; private set; }
        public CosmeticChoice Choice => choice.Value;
        public GameObject HatInstance => hat;

        private void Awake()
        {
            player = GetComponent<NetworkPlayer>();
            var found = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial == coverallMaterial) found.Add(r);
            coverall = found.ToArray();
            hatAnchor = new GameObject("HatAnchor").transform;
            hatAnchor.SetParent(transform, false);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner && catalog != null)
            {
                choice.Value = catalog.FromProfile();
                ApplyCrewDefault();
            }
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

        // ---- Wardrobe purchases (0.12.5): the owner asks, the host charges the company, the owner keeps it. ----

        public CosmeticDefinition Definition(CosmeticKind kind, int index) => catalog == null ? null : kind switch
        {
            CosmeticKind.Coverall => catalog.Coverall(index),
            CosmeticKind.Hat => catalog.Hat(index),
            _ => catalog.Accessory(index),
        };

        /// <summary>Owner: buy a wardrobe item with company money (the host checks the price and the funds).</summary>
        public void RequestBuy(CosmeticKind kind, int index)
        {
            CosmeticDefinition d = Definition(kind, index);
            if (!IsOwner || PurchasePending || d == null || d.Unlock != CosmeticUnlock.Buy || PlayerProfile.Owns(d)) return;
            PurchasePending = true;
            PurchaseMessage = Loc.T("Signing the requisition...");
            BuyRpc(kind, index);
        }

        [Rpc(SendTo.Server)]
        private void BuyRpc(CosmeticKind kind, int index, RpcParams rpcParams = default)
        {
            CosmeticDefinition d = Definition(kind, index);
            CompanyService company = CompanyService.Current;
            if (d == null || d.Unlock != CosmeticUnlock.Buy) BuyResultRpc(kind, index, false, Loc.Mark("That isn't for sale."));
            else if (company == null || !company.IsSpawned) BuyResultRpc(kind, index, false, Loc.Mark("Only at the HQ: the company pays for it."));
            else if (!company.Allows(rpcParams.Receive.SenderClientId, CrewRule.Spend)) BuyResultRpc(kind, index, false, CompanyService.Refusal(CrewRule.Spend));
            else if (!company.TryCharge(d.Price, d.DisplayName)) BuyResultRpc(kind, index, false, Loc.Wire("The company can't afford ${0}.", d.Price.ToString("N0")));
            else BuyResultRpc(kind, index, true, "");
        }

        [Rpc(SendTo.Owner)]
        private void BuyResultRpc(CosmeticKind kind, int index, bool bought, string reason)
        {
            PurchasePending = false;
            CosmeticDefinition d = Definition(kind, index);
            if (!bought || d == null)
            {
                PurchaseMessage = Loc.FromWire(reason); // the host answers in English
                return;
            }
            PlayerProfile.Grant(d);
            PurchaseMessage = Loc.T("Bought: {0}.", d.LocalName);
            Wear(kind switch
            {
                CosmeticKind.Coverall => choice.Value.WithCoverall(index),
                CosmeticKind.Hat => choice.Value.WithHat(index),
                _ => choice.Value.WithAccessory(index),
            });
        }

        private void Update()
        {
            ApplyCrewDefault();
            if (!applied || !shown.Equals(choice.Value)) Apply();
        }

        private void ApplyCrewDefault()
        {
            if (!IsSpawned || !IsOwner || catalog == null || player == null || player.CrewSeat < 0
                || player.CrewSeat == defaultSeat || !string.IsNullOrEmpty(PlayerProfile.Coverall)) return;
            defaultSeat = player.CrewSeat;
            string[] colors = { "orange", "yellow", "navy", "blue" };
            int index = CosmeticCatalog.IndexOf(catalog.Coveralls, colors[Mathf.Clamp(defaultSeat, 0, colors.Length - 1)]);
            if (index >= 0 && PlayerProfile.Unlocked(catalog.Coverall(index))) choice.Value = choice.Value.WithCoverall(index);
        }

        private void LateUpdate()
        {
            if (hat == null && accessory == null) return;
            bool down = ragdollHead != null && ragdollHead.gameObject.activeInHierarchy;
            if (down)
                hatAnchor.SetPositionAndRotation(ragdollHead.position + ragdollHead.up * 0.11f, ragdollHead.rotation);
            else if (standingHead != null)
                hatAnchor.SetPositionAndRotation(standingHead.position, standingHead.rotation);
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
            UpdateCosmeticsShadowMode();
            LateUpdate();
        }

        private bool isThirdPerson;

        /// <summary>Switches owner hat and accessory visibility between first person (shadows only) and third person (fully rendered).</summary>
        public void SetThirdPerson(bool thirdPerson)
        {
            isThirdPerson = thirdPerson;
            UpdateCosmeticsShadowMode();
        }

        private void UpdateCosmeticsShadowMode()
        {
            if (!IsOwner) return;
            ShadowCastingMode mode = isThirdPerson ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
            if (hat != null)
                foreach (Renderer r in hat.GetComponentsInChildren<Renderer>())
                    if (r != null) r.shadowCastingMode = mode;
            if (accessory != null)
                foreach (Renderer r in accessory.GetComponentsInChildren<Renderer>())
                    if (r != null) r.shadowCastingMode = mode;
        }

        private GameObject Wearable(CosmeticDefinition d)
        {
            if (d == null || d.HatPrefab == null) return null;
            GameObject worn = Instantiate(d.HatPrefab, hatAnchor, false);
            foreach (Collider col in worn.GetComponentsInChildren<Collider>()) Destroy(col);
            // Inside your own head it would fill the screen when you look around; you still see its shadow.
            if (IsOwner)
            {
                ShadowCastingMode mode = isThirdPerson ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
                foreach (Renderer r in worn.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = mode;
            }
            return worn;
        }

#if UNITY_EDITOR
        public void EditorSetup(CosmeticCatalog cosmetics, Transform bodyVisual, Transform head, Material suitMaterial, Transform liveHead = null)
        {
            catalog = cosmetics;
            body = bodyVisual;
            standingHead = liveHead;
            ragdollHead = head;
            coverallMaterial = suitMaterial;
        }
#endif
    }
}

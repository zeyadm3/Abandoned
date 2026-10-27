using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The company's truck as it looks (2026 remake), on every machine: the fitted cosmetics from the company state
    /// (parts modelled in truck space, dropped at the truck root), the paint on the body's tint material, and a part
    /// for every truck upgrade the company owns. Nothing here touches the truck's cargo, ramp or extraction. Without a
    /// company (a game not started at the HQ) the truck wears its stock fittings.
    /// </summary>
    public class TruckLook : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");

        [Tooltip("The body renderers; their \"_Tint\" material takes the paint.")]
        [SerializeField] private Renderer[] body;
        [Tooltip("Where fitted parts are placed (truck space).")]
        [SerializeField] private Transform parts;
        [Tooltip("Preview trucks (the garage) show what they are told, not the company's truck.")]
        [SerializeField] private bool preview;

        private readonly List<GameObject> fitted = new();
        private MaterialPropertyBlock block;
        private ulong shownLook = ulong.MaxValue, shownOwned;
        private int shownUpgrades = -1;

        /// <summary>The truck the crew will leave in (the one nearest the job's cargo bay).</summary>
        public static TruckLook Current { get; private set; }

        /// <summary>The horn of the fitted horn cosmetic (the stock horn without one).</summary>
        public SoundId Horn { get; private set; } = SoundId.Horn;

        /// <summary>The horn the departing truck sounds.</summary>
        public static SoundId CurrentHorn => Current != null ? Current.Horn : SoundId.Horn;

        private void OnEnable()
        {
            if (!preview) Current = this;
        }

        private void OnDisable()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            if (preview) return;
            CompanyService company = CompanyService.Current;
            ulong look = company != null ? company.State.TruckLook : 0UL;
            int upgrades = company != null ? company.State.TruckUpgrades : 0;
            if (look == shownLook && upgrades == shownUpgrades) return;
            shownLook = look;
            shownUpgrades = upgrades;
            var state = new CompanyNetState { TruckLook = look, TruckUpgrades = upgrades };
            Show(state, company != null ? company.TruckUpgrades : null);
        }

        /// <summary>Fit a look (the garage's preview passes the selection being tried).</summary>
        public void Show(CompanyNetState state, TruckUpgradeCatalog upgrades)
        {
            TruckCosmeticCatalog catalog = TruckCosmeticCatalog.Load();
            foreach (GameObject g in fitted)
                if (g != null) Destroy(g);
            fitted.Clear();
            if (catalog == null) return;
            Transform host = parts != null ? parts : transform;
            for (int s = 0; s < TruckCosmeticCatalog.Slots; s++)
            {
                var slot = (TruckCosmeticSlot)s;
                TruckCosmeticDefinition d = catalog.At(state.Fitted(slot, catalog));
                if (d == null) continue;
                if (slot == TruckCosmeticSlot.Paint) Paint(d.Color, d.Smoothness);
                else if (slot == TruckCosmeticSlot.Horn) Horn = d.Horn;
                else if (d.Part != null) fitted.Add(Spawn(d.Part, host));
            }
            if (upgrades == null) return;
            for (int i = 0; i < upgrades.Items.Count && i < TruckUpgradeCatalog.MaxUpgrades; i++)
            {
                TruckUpgradeDefinition u = upgrades.Items[i];
                if (u != null && u.VisualPrefab != null && (state.TruckUpgrades & (1 << i)) != 0) fitted.Add(Spawn(u.VisualPrefab, host));
            }
        }

        private static GameObject Spawn(GameObject prefab, Transform host)
        {
            GameObject g = Instantiate(prefab, host, false);
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            foreach (Collider c in g.GetComponentsInChildren<Collider>()) Destroy(c);
            return g;
        }

        private void Paint(Color colour, float smoothness)
        {
            block ??= new MaterialPropertyBlock();
            if (body == null) return;
            // Each LOD orders its materials its own way: the paint goes on whichever slot holds the tint material.
            foreach (Renderer r in body)
            {
                if (r == null) continue;
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null || !mats[i].name.EndsWith("_Tint")) continue;
                    r.GetPropertyBlock(block, i);
                    block.SetColor(BaseColor, colour);
                    block.SetFloat(Smoothness, smoothness);
                    r.SetPropertyBlock(block, i);
                }
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Renderer[] bodyRenderers, Transform partsRoot, bool isPreview = false)
        {
            body = bodyRenderers;
            parts = partsRoot;
            preview = isPreview;
        }
#endif
    }
}

using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.Company
{
    /// <summary>The scale shows the cargo receipt; the reinforced intake offloads any loose recovered salvage.</summary>
    public class ScrapDepot : MonoBehaviour, IUsable
    {
        [OptionalReference, SerializeField] private BoxCollider receivingBay;
        [OptionalReference, SerializeField] private TextMesh readout;

        private readonly Collider[] hits = new Collider[128];
        private readonly HashSet<LootItem> items = new();
        private ScreenPanel screen;
        private bool open;
        private float nextReadout;
        private int waitingItems, waitingValue;
        private float waitingWeight;

        public static ScrapDepot Intake { get; private set; }
        public BoxCollider ReceivingBay => receivingBay;

        private void OnEnable()
        {
            if (receivingBay != null) Intake = this;
        }

        public string UsePrompt(GameObject user)
        {
            if (CompanyService.Current == null) return null;
            return receivingBay != null ? "Unload recovered salvage / cargo receipt" : "Read the scrap weight and quota receipt";
        }

        public void Use(GameObject user)
        {
            CompanyService company = CompanyService.Current;
            if (company == null || !company.IsSpawned) return;
            if (receivingBay != null) company.RequestScrapIntake();
            open = true;
            GameAudio.Play(SoundId.UiClick, transform.position, 0.7f);
        }

        public bool Contains(Vector3 worldPosition)
        {
            if (receivingBay == null) return false;
            Vector3 local = receivingBay.transform.InverseTransformPoint(worldPosition) - receivingBay.center;
            Vector3 half = receivingBay.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        public IReadOnlyCollection<LootItem> ItemsInside()
        {
            items.Clear();
            if (receivingBay == null) return items;
            Vector3 centre = receivingBay.transform.TransformPoint(receivingBay.center);
            Vector3 half = Vector3.Scale(receivingBay.size, receivingBay.transform.lossyScale) * 0.5f;
            int count = Physics.OverlapBoxNonAlloc(centre, half, hits, receivingBay.transform.rotation,
                LayerMask.GetMask(GameLayers.Loot), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                LootItem item = hits[i].GetComponentInParent<LootItem>();
                if (item == null || item.IsShattered || !item.gameObject.activeInHierarchy || !Contains(item.transform.position)) continue;
                Grabbable grabbable = item.GetComponent<Grabbable>();
                if (grabbable != null && (grabbable.IsPocketed || grabbable.Holder != null ||
                    grabbable.Shared != null && grabbable.Shared.CarrierCount > 0)) continue;
                items.Add(item);
            }
            return items;
        }

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            if ((readout != null || open) && Time.unscaledTime >= nextReadout)
            {
                nextReadout = Time.unscaledTime + 0.5f;
                waitingItems = waitingValue = 0;
                waitingWeight = 0f;
                if (Intake != null)
                    foreach (LootItem item in Intake.ItemsInside())
                    {
                        waitingItems++;
                        waitingWeight += item.GameplayWeight;
                        waitingValue += item.CurrentValue;
                    }
                if (readout != null)
                    readout.text = waitingItems > 0 ? $"INTAKE LOAD\n{waitingWeight:0.0} kg\n${waitingValue:N0}"
                        : company == null || company.LastOutcome.Run <= 0 ? "DEPOT READY\n0.0 kg\nNO LOAD ON FILE"
                        : $"LAST LOAD\n{(company.LastLoadedWeight >= 0f ? $"{company.LastLoadedWeight:0.0} kg" : "WEIGHT N/A")}\n${company.LastOutcome.Haul:N0}";
            }
            if (open && (company == null || Core.PadInput.BackPressed)) open = false;
            CursorOwner.Set(this, open);
            if (screen == null || screen.Root.panel == null)
            {
                if (!open || (screen = ScreenPanel.Create(wide: false)) == null) return;
                StyleSheet styles = Resources.Load<StyleSheet>("HqStations");
                if (styles != null) screen.Panel.styleSheets.Add(styles);
                screen.Panel.AddToClassList("scrap-depot");
            }
            screen.Show(open);
            if (!open || company == null) return;
            screen.Build($"{company.LastOutcome.Run}|{company.LastLoadedWeight}|{company.LastLoadedItems}|{company.Selected}|{CompanyService.CrewSize}|{company.State.Money}|{company.IntakeItems}|{company.IntakeWeight}|{waitingItems}|{waitingWeight}|{waitingValue}",
                panel => Fill(panel, company));
        }

        private void Fill(VisualElement panel, CompanyService company)
        {
            MenuKit.Text(panel, "SCRAP & QUOTA DEPOT", "heading");
            MenuKit.Text(panel, CompanySave.DefaultName.ToUpperInvariant(), "subtitle");
            OutcomeNet receipt = company.LastOutcome;
            if (receipt.Run > 0)
            {
                MenuKit.Text(panel, "CARGO RECEIPT / SETTLED", "section");
                MenuKit.Text(panel, $"Appraised salvage: ${receipt.Haul:N0}");
                MenuKit.Text(panel, $"Quota: ${receipt.Quota:N0} / {(receipt.QuotaMet ? "MET" : "MISSED")}");
                MenuKit.Text(panel, company.LastLoadedWeight >= 0f
                    ? $"Loaded weight: {company.LastLoadedWeight:0.0} kg / {company.LastLoadedItems} items" : "Loaded weight: not recorded");
                MenuKit.Text(panel, $"Payout: ${receipt.Payout:N0} / quota penalty: ${receipt.Penalty:N0}");
                if (receipt.Costs > 0) MenuKit.Text(panel, $"Operating costs: ${receipt.Costs:N0}");
                MenuKit.Text(panel, "Haul already credited. This receipt stays with the company after unloading.");
            }
            else MenuKit.Text(panel, "No recovered load on file. Take a contract and bring the truck home.");

            MenuKit.Text(panel, "INTAKE", "section");
            MenuKit.Text(panel, waitingItems > 0 ? $"Waiting in intake: {waitingItems} items / {waitingWeight:0.0} kg / ${waitingValue:N0}"
                : "No loose salvage in intake.");
            if (company.IntakeItems > 0) MenuKit.Text(panel, $"Unloaded here: {company.IntakeItems} items / {company.IntakeWeight:0.0} kg");
            MenuKit.Text(panel, "Use the reinforced chute to offload loose recovered salvage. Cargo credit stays on the settled receipt.");
            if (receivingBay != null)
                MenuKit.Button(panel, "Unload salvage", company.RequestScrapIntake, SoundId.UiConfirm, small: true);

            if (company.Selected >= 0 && company.Selected < company.Board.Count)
            {
                var contract = company.Board[company.Selected];
                MenuKit.Text(panel, "NEXT LOAD", "section");
                MenuKit.Text(panel, $"{contract.Location} / quota ${company.QuotaFor(contract):N0} for {CompanyService.CrewSize} crew");
            }
            MenuKit.Text(panel, $"Company balance: ${company.State.Money:N0}", "section");
            MenuKit.Button(panel, "Close receipt", () => open = false, SoundId.UiBack);
        }

        public void EditorSetup(BoxCollider intakeBay, TextMesh scaleReadout = null)
        {
            receivingBay = intakeBay;
            readout = scaleReadout;
            if (receivingBay != null) Intake = this;
        }

        private void OnDisable()
        {
            if (Intake == this) Intake = null;
            open = false;
            CursorOwner.Set(this, false);
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && !DebugView.Visible || receivingBay == null) return;
            Gizmos.color = new Color(0.85f, 0.65f, 0.25f, 0.8f);
            Gizmos.matrix = receivingBay.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(receivingBay.center, receivingBay.size);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Intake = null;
    }
}

using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>Compact field instruments, driven by the owning player's real state.</summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private PlayerInputReader inputReader;
        private VisualElement vitals, healthBar, breathBar, power, batteryBar, rail, details, frame, recording;
        private Label hp, breath, battery, held, switched, selectedName, selectedStats, tape, elapsed;
        private readonly Label[] pocketKeys = new Label[EquipState.SlotCount];
        private readonly Label[] pocketNames = new Label[EquipState.SlotCount];
        private readonly VisualElement[] entries = new VisualElement[EquipState.SlotCount];
        private int lastSlot = -1, lastGear = -2;
        private float switchUntil;

        private void Update()
        {
            bool alive = player != null && player.IsSpawned && player.IsOwner && !player.IsDead;
            if (vitals == null || vitals.panel == null)
            {
                if (!alive || HudLayer.Root == null) return;
                Build();
            }
            bool inventory = alive && inputReader.Current.InventoryHeld;
            MenuKit.Show(vitals, alive); MenuKit.Show(power, alive); MenuKit.Show(rail, alive);
            MenuKit.Show(frame, alive); MenuKit.Show(recording, alive);
            MenuKit.Show(details, inventory);
            if (!alive) { MenuKit.Show(held, false); MenuKit.Show(switched, false); return; }
            HudLayer.Root.EnableInClassList("field-hud--compact", HudLayer.Root.layout.width < 1000f || HudLayer.Root.layout.height < 650f);
            float health = Mathf.Clamp01(player.Health01);
            float reserve = stamina.Max > 0 ? Mathf.Clamp01(stamina.Current / stamina.Max) : 1f;
            FieldHud.Set(hp, $"{Mathf.CeilToInt(health * 100)}%");
            FieldHud.Set(breath, $"{Mathf.CeilToInt(reserve * 100)}%");
            UiKit.SetBar(healthBar, health, player.Health <= player.HealthConfig.CriticalHp ? "bad" : null);
            UiKit.SetBar(breathBar, reserve, stamina.IsExhausted ? "bad" : null);
            bool hasLight = equipment.Has(EquipmentKind.Flashlight);
            FieldHud.Set(battery, hasLight ? $"{Mathf.CeilToInt(equipment.Battery01 * 100)}%" : "--");
            UiKit.SetSegments(batteryBar, hasLight ? equipment.Battery01 : 0f, equipment.Battery01 < .2f ? "bad" : null);
            FieldHud.Set(tape, $"TAPE {Mathf.Abs((SessionTravel.Current != null ? SessionTravel.Current.TravelId : 0) % 100):00}");
            RunState run = RunState.Current;
            float seconds = run != null && run.IsSpawned ? Mathf.Max(0f, run.State.Window - run.WindowRemaining) : Time.timeSinceLevelLoad;
            FieldHud.Set(elapsed, $"{(int)seconds / 3600:00}:{(int)seconds / 60 % 60:00}:{(int)seconds % 60:00}");
            Pockets(inventory);
            Grabbable item = carrier.Held;
            MenuKit.Show(held, item != null && !inventory);
            if (item != null)
            {
                string text = item.LocalName;
                if (carrier.IsSharing) text = Loc.T("{0} · {1} carrying", text, item.Shared.CarrierCount);
                else if (carrier.IsDragging) text = Loc.T("{0} · Dragging", text);
                FieldHud.Set(held, text);
            }
            int slot = equipment.State.Active, gear = equipment.State[slot];
            if (slot != lastSlot || gear != lastGear)
            {
                lastSlot = slot; lastGear = gear; switchUntil = Time.unscaledTime + 1.7f;
                FieldHud.Set(switched, equipment.InHand != null ? equipment.InHand.LocalName : "");
            }
            MenuKit.Show(switched, !inventory && item == null && equipment.InHand != null && Time.unscaledTime < switchUntil);
        }
        private void Build()
        {
            foreach (VisualElement element in new[] { vitals, power, rail, details, frame, recording, held, switched }) HudLayer.Remove(element);
            HudLayer.Root.AddToClassList("field-hud");
            frame = HudLayer.Add(new VisualElement(), "field-frame");
            frame.generateVisualContent += FieldHud.PaintFrame;
            recording = HudLayer.Add(new VisualElement(), "field-recording");
            VisualElement rec = FieldHud.Element(recording, "field-rec");
            FieldHud.Text(rec, "REC", "field-rec__text");
            FieldHud.Element(rec, "field-rec__dot");
            tape = FieldHud.Text(recording, "TAPE 00", "field-recording__line");
            elapsed = FieldHud.Text(recording, "00:00:00", "field-recording__line");
            vitals = HudLayer.Add(new VisualElement(), "h-vitals", "field-vitals", "field-panel");
            VisualElement row = FieldHud.Element(vitals, "field-meter-row");
            FieldHud.Text(row, Loc.T("HP"), "field-meter-caption");
            healthBar = UiKit.Bar(row, "field-meter", "field-health");
            hp = FieldHud.Text(row, "", "field-meter-number");
            row = FieldHud.Element(vitals, "field-meter-row", "field-meter-row--last");
            FieldHud.Text(row, Loc.T("STAMINA"), "field-meter-caption");
            breathBar = UiKit.Bar(row, "field-meter", "field-stamina");
            breath = FieldHud.Text(row, "", "field-meter-number");
            power = HudLayer.Add(new VisualElement(), "field-power", "field-panel");
            FieldHud.Text(power, Loc.T("POWER"), "field-power__caption");
            battery = FieldHud.Text(power, "", "field-power__number");
            batteryBar = UiKit.Segments(power, 7, "field-battery");
            rail = HudLayer.Add(new VisualElement(), "field-pockets");
            for (int slot = 0; slot < pocketKeys.Length; slot++) pocketKeys[slot] = FieldHud.Text(rail, $"{slot + 1:00}", "field-pocket");
            details = HudLayer.Add(new VisualElement(), "field-inventory", "field-panel");
            FieldHud.Text(details, Loc.T("POCKETS"), "field-inventory__title");
            for (int slot = 0; slot < entries.Length; slot++)
            {
                entries[slot] = FieldHud.Element(details, "field-inventory__slot");
                FieldHud.Text(entries[slot], InputBindings.Display("HandSlot" + (slot + 1)), "field-inventory__key");
                pocketNames[slot] = FieldHud.Text(entries[slot], "", "field-inventory__name");
            }
            selectedName = FieldHud.Text(details, "", "field-inventory__selected");
            selectedStats = FieldHud.Text(details, "", "field-inventory__stats");
            FieldHud.Text(details, Loc.T("Wheel / 1–4 · Select pocket"), "field-inventory__hint");
            FieldHud.Text(details, Loc.T("[{0}] Close · [{1}] Throw selected", InputBindings.Display("Inventory"), InputBindings.Display("Drop")), "field-inventory__hint");
            held = HudLayer.Label("field-carry");
            switched = HudLayer.Label("field-switch");
        }
        private void Pockets(bool inspecting)
        {
            int selected = equipment.State.Active;
            for (int slot = 0; slot < pocketKeys.Length; slot++)
            {
                pocketKeys[slot].EnableInClassList("field-pocket--active", slot == selected);
                entries[slot].EnableInClassList("field-inventory__slot--active", slot == selected);
                Grabbable loot = carrier.Inventory.AtSlot(slot);
                EquipmentDefinition tool = equipment.InSlot(slot);
                if (inspecting) FieldHud.Set(pocketNames[slot], tool != null ? tool.LocalName : loot != null ? loot.LocalName : Loc.T("Empty"));
            }
            if (!inspecting) return;
            EquipmentDefinition chosen = equipment.InHand;
            Grabbable item = carrier.Inventory.AtSlot(selected);
            FieldHud.Set(selectedName, chosen != null ? chosen.LocalName : item != null ? item.LocalName : Loc.T("Empty"));
            string stats = "";
            if (item != null && item.TryGetComponent(out LootItem valuable) && LootTags.Knows(valuable))
                stats = Loc.T("{0:0.#} kg · {1} · {2}", item.Weight, LootTags.FragilityText(valuable.Definition.Fragility), LootTags.ValueText(valuable));
            FieldHud.Set(selectedStats, stats);
        }
        private void OnDisable()
        {
            foreach (VisualElement element in new[] { vitals, power, rail, details, frame, recording, held, switched }) MenuKit.Show(element, false);
        }
        private void OnDestroy()
        {
            foreach (VisualElement element in new[] { vitals, power, rail, details, frame, recording, held, switched }) HudLayer.Remove(element);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible || player == null || !player.IsOwner) return;
            GUI.Label(new Rect(12, 180, 620, 22), $"POCKETS {carrier.Inventory.OccupiedCount}/4 | active {equipment.State.Active + 1} | handling {carrier.HandlingWeight:0.#}kg");
            GUI.Label(new Rect(12, 202, 620, 22), $"WORLD TOOLS {equipment.DroppedToolCount} | peek {player.Peek01:0.00} | concealed {player.IsConcealed}");
        }
#endif
#if UNITY_EDITOR
        public void EditorSetup(NetworkPlayer owner, PlayerStamina playerStamina, PlayerCarrier playerCarrier, PlayerEquipment gear, PlayerInputReader reader)
        {
            player = owner; stamina = playerStamina; carrier = playerCarrier; equipment = gear; inputReader = reader;
        }
#endif
    }
}

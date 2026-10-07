using System.Collections.Generic;
using System.Text;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The player's own HUD (UI step 2), R.E.P.O.-style and out of the way: stamina bottom left (with what
    /// you're carrying and whether it slows you), the two hand slots and the pocket slots bottom right (gear
    /// and loot pictures, values on the pockets), and the full pockets list while the Inventory key is held.
    /// Owner only; everything it reads is already on this machine.
    /// </summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private PlayerInputReader inputReader;

        private VisualElement staminaBlock, staminaBar, healthRow, healthBar, batteryRow, batteryBar, slots, pocketsRow;
        private Label staminaLabel, healthLabel, batteryLabel, load, pocketsDetail;
        private readonly VisualElement[] hands = new VisualElement[2];
        private readonly List<VisualElement> pockets = new();
        private readonly StringBuilder key = new();
        private string shownKey;
        private float showSlotsUntil;
        private int shownActiveSlot = -1;

        private bool Mine => player != null && player.IsSpawned && player.IsOwner;

        private void Update()
        {
            bool show = Mine && !player.IsDead;
            if (staminaBlock == null || staminaBlock.panel == null)
            {
                if (!show || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(staminaBlock, show);
            if (!show)
            {
                MenuKit.Show(slots, false);
                MenuKit.Show(pocketsDetail, false);
                return;
            }

            bool depleted = stamina.Max > 0f && stamina.Current < stamina.Max - 0.01f;
            bool hurt = player.Health < player.MaxHealth - 0.01f;
            MenuKit.Show(healthRow, hurt);
            if (hurt)
            {
                Set(healthLabel, $"VITALS  {Mathf.CeilToInt(player.Health):0}");
                UiKit.SetBar(healthBar, player.Health01, player.Health01 <= player.HealthConfig.LowHealthThreshold ? "bad" : null);
            }
            MenuKit.Show(staminaLabel, depleted);
            MenuKit.Show(staminaBar, depleted);
            Set(staminaLabel, stamina.IsExhausted ? "BREATH  /  EXHAUSTED" : "BREATH");
            UiKit.SetBar(staminaBar, stamina.Max > 0f ? stamina.Current / stamina.Max : 1f, stamina.IsExhausted ? "bad" : null);
            bool batteryRelevant = equipment.Has(EquipmentKind.Flashlight) && (equipment.LightOn || equipment.Battery01 < 0.25f);
            MenuKit.Show(batteryRow, batteryRelevant);
            if (batteryRelevant)
            {
                Set(batteryLabel, equipment.Battery01 <= 0f ? "LIGHT  /  NO POWER" : $"LIGHT  {Mathf.CeilToInt(equipment.Battery01 * 100f)}%");
                UiKit.SetBar(batteryBar, equipment.Battery01, equipment.Battery01 < 0.25f ? "bad" : null);
            }
            Load();
            Slots();
            PlayerInputFrame input = inputReader.Current;
            if (shownActiveSlot != equipment.State.Active || input.Slot1Pressed || input.Slot2Pressed || input.UsePressed)
            {
                shownActiveSlot = equipment.State.Active;
                showSlotsUntil = Time.time + 3f;
            }
            bool detail = input.InventoryHeld;
            MenuKit.Show(slots, detail || input.UseHeld || Time.time < showSlotsUntil);
            MenuKit.Show(pocketsDetail, detail);
            if (detail) Set(pocketsDetail, PocketsText());
        }

        private void Build()
        {
            HudLayer.Remove(staminaBlock);
            HudLayer.Remove(slots);
            HudLayer.Remove(pocketsDetail);
            pockets.Clear();
            shownKey = null;
            staminaBlock = HudLayer.Add(new VisualElement(), "player-stamina");
            healthRow = new VisualElement { pickingMode = PickingMode.Ignore };
            healthRow.AddToClassList("player-vital");
            staminaBlock.Add(healthRow);
            healthLabel = Text(healthRow, "VITALS", "player-stamina__label");
            healthBar = UiKit.Bar(healthRow, "player-vital__bar");
            staminaLabel = Text(staminaBlock, "STAMINA", "player-stamina__label");
            staminaBar = UiKit.Bar(staminaBlock);
            batteryRow = new VisualElement { pickingMode = PickingMode.Ignore };
            batteryRow.AddToClassList("player-vital");
            staminaBlock.Add(batteryRow);
            batteryLabel = Text(batteryRow, "LIGHT", "player-stamina__label");
            batteryBar = UiKit.Bar(batteryRow, "player-vital__bar");
            load = Text(staminaBlock, "", "player-load");
            slots = HudLayer.Add(new VisualElement(), "player-slots");
            for (int i = 0; i < 2; i++) hands[i] = Slot(slots, $"{i + 1}", hand: true);
            var gap = new VisualElement { pickingMode = PickingMode.Ignore };
            gap.AddToClassList("slot-gap");
            slots.Add(gap);
            pocketsRow = new VisualElement { pickingMode = PickingMode.Ignore };
            pocketsRow.style.flexDirection = FlexDirection.Row;
            pocketsRow.style.alignItems = Align.FlexEnd;
            slots.Add(pocketsRow);
            pocketsDetail = HudLayer.Label("hud-panel", "pockets-detail");
        }

        private static VisualElement Slot(VisualElement parent, string keyText, bool hand)
        {
            var slot = new VisualElement { pickingMode = PickingMode.Ignore };
            slot.AddToClassList("slot");
            if (hand) slot.AddToClassList("slot--hand");
            var icon = new VisualElement { pickingMode = PickingMode.Ignore, name = "icon" };
            icon.style.width = hand ? 48f : 40f;
            icon.style.height = hand ? 48f : 40f;
            icon.style.flexShrink = 0;
            icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            icon.AddToClassList("slot__icon");
            slot.Add(icon);
            if (keyText != null) Text(slot, keyText, "slot__key");
            var value = Text(slot, "", "slot__value");
            value.name = "value";
            parent.Add(slot);
            return slot;
        }

        // What you're carrying weighs on you: say so when it slows you down.
        private void Load()
        {
            float weight = carrier.CarriedWeight;
            float speed = carrier.Config.SpeedMultiplierFor(weight);
            string text = carrier.IsSharing ? $"CARRYING {carrier.Held.DisplayName.ToUpperInvariant()} TOGETHER"
                : carrier.IsDragging ? $"DRAGGING {carrier.Held.DisplayName.ToUpperInvariant()}"
                : weight > 0.5f ? $"CARRYING {weight:0} KG" + (speed < 0.85f ? "  -  SLOWED" : "")
                : "";
            Set(load, text);
            MenuKit.Show(load, text.Length > 0);
        }

        private void Slots()
        {
            // Rebuilt only when something changes: gear in hand, the active slot, pocket contents and values.
            key.Clear().Append(equipment.State.Slot0).Append(',').Append(equipment.State.Slot1).Append(',').Append(equipment.State.Active)
                .Append('|').Append(carrier.Inventory.Capacity).Append('|').Append(LootTags.ValuesVisible);
            foreach (Grabbable g in carrier.Inventory.Items)
                key.Append('|').Append(g != null ? g.GetInstanceID() : 0).Append(':').Append(g != null && g.TryGetComponent(out IValuable v) ? v.CurrentValue : 0);
            string now = key.ToString();
            if (now == shownKey) return;
            shownKey = now;

            for (int i = 0; i < 2; i++)
            {
                EquipmentDefinition d = equipment.InSlot(i);
                Fill(hands[i], d != null ? "item/" + d.Id : null, null);
                hands[i].EnableInClassList("slot--active", d != null && equipment.State.Active == i);
                hands[i].tooltip = d != null ? d.DisplayName : "";
            }
            int capacity = carrier.Inventory.Capacity;
            while (pockets.Count < capacity) pockets.Add(Slot(pocketsRow, null, hand: false));
            for (int i = 0; i < pockets.Count; i++)
            {
                MenuKit.Show(pockets[i], i < capacity);
                Grabbable g = i < carrier.Inventory.Count ? carrier.Inventory.Items[i] : null;
                LootItem item = g != null ? g.GetComponent<LootItem>() : null;
                Fill(pockets[i], item != null ? "loot/" + item.Definition.Id : g != null ? "board/pouch" : null,
                    item != null && LootTags.ValuesVisible ? Money.Short(item.CurrentValue) : null);
            }
        }

        private static void Fill(VisualElement slot, string iconId, string value)
        {
            VisualElement icon = slot.Q("icon");
            Texture2D t = iconId != null ? UiIcons.Get(iconId) : null;
            icon.style.backgroundImage = t != null ? new StyleBackground(t) : new StyleBackground(StyleKeyword.None);
            slot.EnableInClassList("slot--empty", iconId == null);
            Set(slot.Q<Label>("value"), value ?? "");
        }

        private string PocketsText()
        {
            var text = new StringBuilder();
            text.AppendLine($"<b>POCKETS</b>  {carrier.Inventory.Count}/{carrier.Inventory.Capacity}");
            int total = 0;
            foreach (Grabbable item in carrier.Inventory.Items)
            {
                int value = item.TryGetComponent(out IValuable v) ? v.CurrentValue : 0;
                total += value;
                text.AppendLine(LootTags.ValuesVisible ? $"{item.DisplayName}   <color=#abb59a>${value:N0}</color>" : item.DisplayName);
            }
            if (LootTags.ValuesVisible) text.AppendLine($"<b>TOTAL</b>  <color=#abb59a>${total:N0}</color>");
            text.Append($"<color=#a5a196>{InputBindings.Display("Drop")} while holding {InputBindings.Display("Inventory")}: drop the last one</color>");
            return text.ToString();
        }

        private static Label Text(VisualElement parent, string text, string cls)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(cls);
            parent.Add(label);
            return label;
        }

        private static void Set(Label label, string text)
        {
            if (label != null && label.text != text) label.text = text;
        }

        private void OnDisable()
        {
            foreach (VisualElement e in new[] { staminaBlock, slots, pocketsDetail }) if (e != null) MenuKit.Show(e, false);
        }

        private void OnDestroy()
        {
            HudLayer.Remove(staminaBlock);
            HudLayer.Remove(slots);
            HudLayer.Remove(pocketsDetail);
        }

#if UNITY_EDITOR
        public void EditorSetup(NetworkPlayer owner, PlayerStamina playerStamina, PlayerCarrier playerCarrier, PlayerEquipment gear, PlayerInputReader reader)
        {
            player = owner;
            stamina = playerStamina;
            carrier = playerCarrier;
            equipment = gear;
            inputReader = reader;
        }
#endif
    }
}

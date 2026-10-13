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
    /// Owner-only survival instruments: a draining silhouette, pulse trace, breath reserve and named
    /// gear/pocket slips. Holding Inventory opens an inspection ledger without changing input.
    /// </summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private PlayerInputReader inputReader;

        private VisualElement vitals, body, pulse, breath, breathBar, batteryRow, batteryBar, slots, pocketsRow, details, detailIcon;
        private Label healthValue, healthMax, healthState, breathValue, breathState, batteryValue, load, slotHint;
        private Label pocketCount, detailName, detailStats, detailTotal, detailHint;
        private readonly VisualElement[] hands = new VisualElement[2];
        private readonly List<VisualElement> pockets = new();
        private readonly List<Vector2> clippedBody = new();
        private readonly StringBuilder key = new();
        private string shownKey;
        private float nextTrace, lastBreath;
        // What the number labels last showed, so their text is only rebuilt when a figure changes.
        private const int Unshown = int.MinValue;
        private int shownHealth = Unshown, shownMax = Unshown, shownBreath = Unshown, shownBattery = Unshown, shownLoad = Unshown;

        // Original recovery-crew silhouette, filled from the feet up as health drains.
        private static readonly Vector2[] BodyContour =
        {
            new(.41f, .02f), new(.58f, .02f), new(.65f, .07f), new(.65f, .18f), new(.57f, .24f),
            new(.58f, .28f), new(.78f, .31f), new(.91f, .55f), new(.94f, .66f), new(.85f, .69f),
            new(.76f, .55f), new(.67f, .41f), new(.67f, .62f), new(.66f, .77f), new(.72f, .98f),
            new(.55f, .98f), new(.49f, .74f), new(.43f, .98f), new(.27f, .98f), new(.33f, .76f),
            new(.32f, .62f), new(.33f, .41f), new(.23f, .55f), new(.14f, .69f), new(.05f, .66f),
            new(.08f, .55f), new(.21f, .31f), new(.41f, .28f), new(.42f, .24f), new(.34f, .18f),
            new(.34f, .07f)
        };

        private bool Mine => player != null && player.IsSpawned && player.IsOwner;

        private void Update()
        {
            bool show = Mine && !player.IsDead;
            if (vitals == null || vitals.panel == null)
            {
                if (!show || HudLayer.Root == null) return;
                Build();
            }
            MenuKit.Show(vitals, show);
            MenuKit.Show(slots, show);
            MenuKit.Show(details, show && inputReader.Current.InventoryHeld);
            if (!show) return;
            bool compact = HudLayer.Root.layout.width < 1180f || HudLayer.Root.layout.height < 740f;
            vitals.EnableInClassList("h-vitals--compact", compact);
            slots.EnableInClassList("h-inventory--compact", compact);
            details.EnableInClassList("h-inspection--compact", compact);
            LayoutInventory(compact);

            PlayerHealthConfig config = player.HealthConfig;
            int health = Mathf.CeilToInt(player.Health), max = Mathf.CeilToInt(config.MaxHealth);
            if (health != shownHealth) Set(healthValue, (shownHealth = health).ToString("000"));
            if (max != shownMax) Set(healthMax, $"/ {shownMax = max:000}");
            bool critical = player.Health <= config.CriticalHp;
            bool warning = player.Health <= config.WarningHp;
            vitals.EnableInClassList("h-vitals--warning", warning);
            vitals.EnableInClassList("h-vitals--critical", critical);
            Set(healthState, player.Health <= config.NearDeathHp ? Loc.T("FAILING") : critical ? Loc.T("CRITICAL") : warning ? Loc.T("WOUNDED") : Loc.T("STABLE"));
            float reserve = stamina.Max > 0f ? Mathf.Clamp01(stamina.Current / stamina.Max) : 1f;
            int breathPercent = Mathf.CeilToInt(reserve * 100f);
            if (breathPercent != shownBreath) Set(breathValue, $"{shownBreath = breathPercent:00}%");
            Set(breathState, stamina.IsExhausted ? Loc.T("EXHAUSTED") : reserve >= .999f ? Loc.T("STEADY") : stamina.Current > lastBreath + .01f ? Loc.T("RECOVERING") : Loc.T("STRAINED"));
            breath.EnableInClassList("h-breath--exhausted", stamina.IsExhausted);
            UiKit.SetBar(breathBar, reserve, stamina.IsExhausted ? "bad" : null);
            lastBreath = stamina.Current;
            bool light = equipment.Has(EquipmentKind.Flashlight) && (equipment.LightOn || equipment.Battery01 < .25f);
            MenuKit.Show(batteryRow, light);
            if (light)
            {
                // Rounded up, so 0 only ever means a flat battery.
                int battery = equipment.Battery01 <= 0f ? 0 : Mathf.CeilToInt(equipment.Battery01 * 100f);
                if (battery != shownBattery) Set(batteryValue, (shownBattery = battery) == 0 ? Loc.T("NO POWER") : $"{battery:00}%");
                batteryRow.EnableInClassList("h-power--low", equipment.Battery01 < .25f);
                UiKit.SetBar(batteryBar, equipment.Battery01, equipment.Battery01 < .25f ? "bad" : null);
            }
            Load();
            Slots();
            if (inputReader.Current.InventoryHeld) Inspect();
            if (Time.unscaledTime >= nextTrace)
            {
                nextTrace = Time.unscaledTime + .08f;
                body.MarkDirtyRepaint();
                pulse.MarkDirtyRepaint();
                breathBar.MarkDirtyRepaint();
            }
        }

        private void Build()
        {
            HudLayer.Remove(vitals);
            HudLayer.Remove(slots);
            HudLayer.Remove(details);
            pockets.Clear();
            shownKey = null;
            shownHealth = shownMax = shownBreath = shownBattery = shownLoad = Unshown;
            vitals = HudLayer.Add(new VisualElement(), "player-stamina", "h-vitals");
            Text(vitals, Loc.T("SURVIVAL"), "h-instrument-title");
            VisualElement row = Element(vitals, "h-vitals__row");
            body = Element(row, "h-vitals__body");
            body.generateVisualContent += PaintBody;
            VisualElement readout = Element(row, "h-vitals__readout");
            Text(readout, Loc.T("VITAL SIGNS"), "h-micro");
            VisualElement number = Element(readout, "h-vitals__number");
            healthValue = Text(number, "100", "h-vitals__value");
            healthMax = Text(number, "/ 100", "h-vitals__max");
            pulse = Element(readout, "h-vitals__pulse");
            pulse.generateVisualContent += PaintPulse;
            healthState = Text(readout, Loc.T("STABLE"), "h-vitals__state");
            breath = Element(vitals, "h-breath");
            row = Element(breath, "h-instrument-row");
            Text(row, Loc.T("BREATH"), "h-instrument-caption");
            breathValue = Text(row, "100%", "h-instrument-value");
            breathBar = UiKit.Bar(breath, "h-breath__bar");
            breathBar.generateVisualContent += PaintBreath;
            breathState = Text(breath, Loc.T("STEADY"), "h-breath__state");
            batteryRow = Element(vitals, "h-power");
            row = Element(batteryRow, "h-instrument-row");
            Text(row, Loc.T("LIGHT"), "h-instrument-caption");
            batteryValue = Text(row, "", "h-instrument-value");
            batteryBar = UiKit.Bar(batteryRow, "h-power__bar");
            load = Text(vitals, "", "player-load", "h-vitals__load");

            slots = HudLayer.Add(new VisualElement(), "player-slots", "h-inventory");
            row = Element(slots, "h-inventory__header");
            Text(row, Loc.T("ON YOUR PERSON"), "h-instrument-title");
            slotHint = Text(row, "", "h-micro");
            VisualElement gear = Element(slots, "h-inventory__gear");
            for (int i = 0; i < hands.Length; i++) hands[i] = Slot(gear, true);
            row = Element(slots, "h-inventory__header");
            Text(row, Loc.T("POCKETS"), "h-micro");
            pocketCount = Text(row, "", "h-micro");
            pocketsRow = Element(slots, "h-inventory__pockets");

            details = HudLayer.Add(new VisualElement(), "h-inspection");
            Text(details, Loc.T("POCKET CONTENTS"), "h-instrument-title");
            row = Element(details, "h-inspection__selected");
            detailIcon = UiKit.Icon(row, "board/pouch", "large");
            VisualElement copy = Element(row, "h-inspection__copy");
            detailName = Text(copy, "", "h-inspection__name");
            detailStats = Text(copy, "", "h-inspection__stats");
            detailTotal = Text(details, "", "h-inspection__total");
            detailHint = Text(details, "", "h-inspection__hint");
        }

        private static VisualElement Slot(VisualElement parent, bool hand)
        {
            VisualElement slot = Element(parent, "slot", "h-item");
            slot.EnableInClassList("slot--hand", hand);
            slot.EnableInClassList("h-item--gear", hand);
            slot.EnableInClassList("h-item--pocket", !hand);
            Text(slot, "", "slot__key", "h-item__key").name = "key";
            VisualElement icon = Element(slot, "slot__icon", "h-item__icon");
            icon.name = "icon";
            Text(slot, "", "h-item__name").name = "name";
            Text(slot, "", "slot__value", "h-item__value").name = "value";
            return slot;
        }

        private void Load()
        {
            float weight = carrier.CarriedWeight;
            bool slowed = weight > .5f && carrier.Config.SpeedMultiplierFor(weight) < .85f;
            // Everything the line shows, in one number: the weight to a tenth, how it is held, and the flags.
            int state = ((Mathf.RoundToInt(weight * 10f) * 2 + (weight > .5f ? 1 : 0)) * 4 +
                         (carrier.IsSharing ? 2 : carrier.IsDragging ? 1 : 0)) * 2 + (slowed ? 1 : 0);
            if (state == shownLoad) return;
            shownLoad = state;
            string text = carrier.IsSharing ? Loc.T("SHARED LOAD / {0:0.#} KG", weight)
                : carrier.IsDragging ? Loc.T("DRAGGING / {0:0.#} KG", weight)
                : weight > .5f ? Loc.T("LOAD / {0:0.#} KG", weight) : "";
            Set(load, slowed ? Loc.T("{0} / SLOWED", text) : text);
            load.EnableInClassList("h-vitals__load--heavy", slowed);
            MenuKit.Show(load, text.Length > 0);
        }

        private void Slots()
        {
            string hand1 = InputBindings.Display("HandSlot1"), hand2 = InputBindings.Display("HandSlot2");
            string inventoryKey = InputBindings.Display("Inventory");
            key.Clear().Append(equipment.State.Slot0).Append(',').Append(equipment.State.Slot1).Append(',').Append(equipment.State.Active)
                .Append('|').Append(carrier.Inventory.Capacity).Append('|').Append(LootTags.ValuesVisible).Append('|').Append(carrier.Inventory.SelectedIndex)
                .Append('|').Append(hand1).Append('|').Append(hand2).Append('|').Append(inventoryKey);
            foreach (Grabbable g in carrier.Inventory.Items)
                key.Append('|').Append(g != null ? g.GetInstanceID() : 0).Append(':').Append(g != null && g.TryGetComponent(out IValuable v) ? v.CurrentValue : 0);
            string now = key.ToString();
            if (now == shownKey) return;
            shownKey = now;
            Set(slotHint, Loc.T("[{0}] INSPECT", inventoryKey));
            Set(pocketCount, $"{carrier.Inventory.Count:00} / {carrier.Inventory.Capacity:00}");
            for (int i = 0; i < hands.Length; i++)
            {
                EquipmentDefinition d = equipment.InSlot(i);
                bool active = d != null && equipment.State.Active == i;
                Fill(hands[i], d != null ? "item/" + d.Id : null, d != null ? d.LocalName : Loc.T("Empty hand"), active ? Loc.T("IN HAND") : d != null ? Loc.T("STOWED") : "--");
                Set(hands[i].Q<Label>("key"), $"[{(i == 0 ? hand1 : hand2)}]");
                hands[i].EnableInClassList("slot--active", active);
                hands[i].EnableInClassList("h-item--last", i == hands.Length - 1);
            }
            int capacity = Mathf.Max(0, carrier.Inventory.Capacity);
            while (pockets.Count < capacity) pockets.Add(Slot(pocketsRow, false));
            for (int i = 0; i < pockets.Count; i++)
            {
                MenuKit.Show(pockets[i], i < capacity);
                Grabbable g = i < carrier.Inventory.Count ? carrier.Inventory.Items[i] : null;
                LootItem item = g != null ? g.GetComponent<LootItem>() : null;
                Fill(pockets[i], item != null && item.Definition != null ? "loot/" + item.Definition.Id : g != null ? "board/pouch" : null,
                    g != null ? g.LocalName : Loc.T("Empty"), item != null && LootTags.ValuesVisible ? Money.Short(item.CurrentValue) : g != null ? Loc.T("{0:0.#}kg", g.Weight) : "--");
                Set(pockets[i].Q<Label>("key"), $"P{i + 1:00}");
                pockets[i].EnableInClassList("slot--active", g != null && i == carrier.Inventory.SelectedIndex);
                pockets[i].EnableInClassList("h-item--last", i == capacity - 1);
            }
        }

        private void LayoutInventory(bool compact)
        {
            VisualElement root = HudLayer.Root;
            float viewportWidth = root.layout.width, viewportHeight = root.layout.height;
            if (!HudLayer.Finite(viewportWidth) || !HudLayer.Finite(viewportHeight) || viewportWidth <= 0f || viewportHeight <= 0f) return;
            float inset = compact ? 20f : 28f;
            float vitalEdge = HudLayer.TryLocalRect(vitals, root, out Rect vitalBounds) ? vitalBounds.xMax : compact ? 216f : 244f;
            float centerReserve = Mathf.Min(220f, viewportWidth * .30f);
            float available = Mathf.Max(1f, viewportWidth - vitalEdge - inset - 16f - centerReserve);
            int count = Mathf.Max(1, carrier.Inventory.Capacity);
            float desired = Mathf.Max(compact ? 388f : 460f, count * 64f + (count - 1) * 6f);
            float width = Mathf.Min(desired, available);
            slots.style.width = width;
            slots.EnableInClassList("h-inventory--dense", (width - (count - 1) * 6f) / count < 70f);
            details.style.width = Mathf.Min(width, 460f);
            // Anchor to the rendered strip, including compact bottom inset and font metrics.
            if (HudLayer.TryLocalRect(slots, root, out Rect inventoryBounds))
                details.style.bottom = viewportHeight - inventoryBounds.yMin + 12f;
        }

        private static void Fill(VisualElement slot, string iconId, string name, string value)
        {
            Texture2D texture = iconId != null ? UiIcons.Get(iconId) ?? UiIcons.Get("board/pouch") : null;
            slot.Q("icon").style.backgroundImage = texture != null ? new StyleBackground(texture) : new StyleBackground(StyleKeyword.None);
            slot.EnableInClassList("slot--empty", iconId == null);
            Set(slot.Q<Label>("name"), name);
            Set(slot.Q<Label>("value"), value ?? "");
        }

        private void Inspect()
        {
            Grabbable selected = carrier.Inventory.Selected;
            LootItem loot = selected != null ? selected.GetComponent<LootItem>() : null;
            UiKit.SetIcon(detailIcon, loot != null && loot.Definition != null ? "loot/" + loot.Definition.Id : "board/pouch");
            Set(detailName, selected != null ? selected.LocalName : Loc.T("Nothing recovered"));
            int value = selected != null && selected.TryGetComponent(out IValuable valuable) ? valuable.CurrentValue : 0;
            Set(detailStats, selected == null ? Loc.T("Your pockets are empty.") : LootTags.ValuesVisible
                ? Loc.T("POCKET {0:00} / {1:0.#} KG / ${2:N0}", carrier.Inventory.SelectedIndex + 1, selected.Weight, value)
                : Loc.T("POCKET {0:00} / {1:0.#} KG", carrier.Inventory.SelectedIndex + 1, selected.Weight));
            int total = 0;
            foreach (Grabbable item in carrier.Inventory.Items)
                if (item != null && item.TryGetComponent(out IValuable v)) total += v.CurrentValue;
            Set(detailTotal, LootTags.ValuesVisible ? Loc.T("RECOVERED VALUE / ${0:N0}", total) : Loc.T("CARRIED WEIGHT / {0:0.#} KG", carrier.Inventory.TotalWeight));
            Set(detailHint, Loc.T("Wheel / choose pocket\nHold [{0}] + [{1}] / drop selected", InputBindings.Display("Inventory"), InputBindings.Display("Drop")));
        }

        private void PaintBody(MeshGenerationContext context)
        {
            if (player == null || body == null) return;
            Painter2D painter = context.painter2D;
            Color color = healthValue.resolvedStyle.color;
            Vector2 size = body.contentRect.size;
            Vector2 origin = new(4f, 3f), scale = new(size.x - 22f, size.y - 6f);
            if (scale.x <= 0f || scale.y <= 0f) return;
            Path(painter, BodyContour, origin, scale, new Color(color.r, color.g, color.b, .22f), false);
            clippedBody.Clear();
            float edge = 1f - Mathf.Clamp01(player.Health01);
            for (int i = 0; i < BodyContour.Length; i++)
            {
                Vector2 a = BodyContour[i], b = BodyContour[(i + 1) % BodyContour.Length];
                bool insideA = a.y >= edge, insideB = b.y >= edge;
                if (insideA) clippedBody.Add(a);
                if (insideA != insideB) clippedBody.Add(Vector2.Lerp(a, b, (edge - a.y) / (b.y - a.y)));
            }
            Path(painter, clippedBody, origin, scale, new Color(color.r, color.g, color.b, .55f), true);
            painter.lineWidth = 2f;
            for (int i = 0; i < 10; i++)
            {
                painter.strokeColor = i < Mathf.CeilToInt(player.Health01 * 10f) ? color : new Color(color.r, color.g, color.b, .17f);
                painter.BeginPath();
                painter.MoveTo(new Vector2(size.x - 12f, size.y - 7f - i * (size.y - 14f) / 10f));
                painter.LineTo(new Vector2(size.x - 4f, size.y - 7f - i * (size.y - 14f) / 10f));
                painter.Stroke();
            }
        }

        private static void Path(Painter2D painter, IReadOnlyList<Vector2> points, Vector2 origin, Vector2 scale, Color color, bool fill)
        {
            if (points.Count < 3) return;
            painter.BeginPath();
            painter.MoveTo(origin + Vector2.Scale(points[0], scale));
            for (int i = 1; i < points.Count; i++) painter.LineTo(origin + Vector2.Scale(points[i], scale));
            painter.ClosePath();
            if (fill) { painter.fillColor = color; painter.Fill(); }
            else { painter.lineWidth = 1.3f; painter.strokeColor = color; painter.Stroke(); }
        }

        private void PaintPulse(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            Vector2 size = pulse.contentRect.size;
            if (size.x <= 0f || size.y <= 0f) return;
            Color color = healthValue.resolvedStyle.color;
            float beat = GameSettings.ReduceFlashing ? 0f : PlayerPulse.Beat;
            painter.strokeColor = new Color(color.r, color.g, color.b, .55f + beat * .35f);
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, size.y * .65f));
            for (int i = 1; i <= 48; i++)
            {
                float t = i / 48f;
                float spike = t > .34f && t < .46f ? Mathf.Sin((t - .34f) / .12f * Mathf.PI * 2f) * .47f : 0f;
                painter.LineTo(new Vector2(size.x * t, size.y * (.65f - spike * (.8f + beat * .2f))));
            }
            painter.Stroke();
        }

        private void PaintBreath(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            Vector2 size = breathBar.contentRect.size;
            if (size.x <= 0f || size.y <= 0f || stamina == null) return;
            float reserve = stamina.Max > 0f ? Mathf.Clamp01(stamina.Current / stamina.Max) : 1f;
            painter.strokeColor = breathValue.resolvedStyle.color;
            painter.lineWidth = 1.5f;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0f, size.y * .5f));
            for (int i = 1; i <= 64; i++)
            {
                float t = i / 64f;
                float amplitude = .12f + (1f - reserve) * .28f;
                float wave = Mathf.Sin(t * Mathf.PI * (stamina.IsExhausted ? 12f : 6f)) * amplitude;
                painter.LineTo(new Vector2(size.x * t, size.y * (.5f + wave)));
                if (t >= reserve) break;
            }
            painter.Stroke();
        }

        private static VisualElement Element(VisualElement parent, params string[] classes)
        {
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            foreach (string cls in classes) element.AddToClassList(cls);
            parent.Add(element);
            return element;
        }

        private static Label Text(VisualElement parent, string text, params string[] classes)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            foreach (string cls in classes) label.AddToClassList(cls);
            parent.Add(label);
            return label;
        }

        private static void Set(Label label, string text) { if (label != null && label.text != text) label.text = text; }
        private void OnDisable() { foreach (VisualElement element in new[] { vitals, slots, details }) if (element != null) MenuKit.Show(element, false); }
        private void OnDestroy() { HudLayer.Remove(vitals); HudLayer.Remove(slots); HudLayer.Remove(details); }

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

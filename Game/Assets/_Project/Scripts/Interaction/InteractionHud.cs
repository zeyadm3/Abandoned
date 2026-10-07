using System.Text;
using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Interaction
{
    /// <summary>
    /// The player's interaction HUD (M8.1): crosshair, what you can do with what you're looking at or
    /// holding, rejection hints, the throw charge, and the pockets (Inventory key). Keys follow the
    /// player's bindings. F1 adds carry debug numbers (OnGUI, debug only).
    /// </summary>
    public class InteractionHud : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerInputReader inputReader;

        private GUIStyle box;
        private readonly StringBuilder text = new();
        private VisualElement crosshair, charge, chargeFill;
        private Label prompt, inventory;

        /// <summary>Tests: the prompt under the crosshair (null when none).</summary>
        public string Prompt { get; private set; }

        private void Update()
        {
            Prompt = BuildPrompt(out bool hint);
            if (crosshair == null)
            {
                if (UI.HudLayer.Root == null) return;
                crosshair = UI.HudLayer.Add(new VisualElement(), "hud-crosshair");
                prompt = UI.HudLayer.Label("hud-prompt");
                charge = UI.HudLayer.Add(new VisualElement(), "hud-charge");
                chargeFill = new VisualElement { pickingMode = PickingMode.Ignore };
                chargeFill.AddToClassList("hud-charge__fill");
                charge.Add(chargeFill);
                inventory = UI.HudLayer.Label("hud-panel", "hud-inventory");
            }
            UI.MenuKit.Show(prompt, Prompt != null);
            if (Prompt != null && prompt.text != Prompt) prompt.text = Prompt;
            prompt.EnableInClassList("hud-prompt--hint", hint);
            UI.MenuKit.Show(charge, interactor.Charge > 0f);
            chargeFill.style.width = Length.Percent(interactor.Charge * 100f);
            bool pockets = inputReader.Current.InventoryHeld;
            UI.MenuKit.Show(inventory, pockets);
            if (pockets) inventory.text = InventoryText();
        }

        private void OnDisable()
        {
            foreach (VisualElement e in new[] { crosshair, prompt, charge, inventory }) if (e != null) UI.MenuKit.Show(e, false);
        }

        private void OnEnable()
        {
            if (crosshair != null) UI.MenuKit.Show(crosshair, true);
        }

        private void OnDestroy()
        {
            UI.HudLayer.Remove(crosshair);
            UI.HudLayer.Remove(prompt);
            UI.HudLayer.Remove(charge);
            UI.HudLayer.Remove(inventory);
        }

        private string BuildPrompt(out bool hint)
        {
            hint = false;
            string interact = InputBindings.Display("Interact"), use = InputBindings.Display("Use"), drop = InputBindings.Display("Drop");
            if (carrier.HintVisible) { hint = true; return carrier.Hint; }
            if (interactor.Target != null && interactor.Target.Shared != null)
                return $"[{interact}] Grab {Describe(interactor.Target)} - {SharedCarryText.Crew(interactor.Target.Shared.CarrierCount, interactor.Target.Shared.RequiredCarriers)}";
            if (interactor.Target != null) return $"[{interact}] Pick up {Describe(interactor.Target)}";
            if (carrier.Held == null && interactor.UseTarget?.UsePrompt(interactor.gameObject) is string usable) return $"[{interact}] {usable}";
            if (carrier.IsSharing) return $"{carrier.Held.DisplayName}: {SharedCarryText.Of(carrier.Held.Shared)}   [{drop}] let go";
            if (carrier.IsDragging) return $"Dragging {Describe(carrier.Held)}   [{drop}] let go";
            if (carrier.Held != null) return $"Holding {Describe(carrier.Held)}   [{use}] throw   [{drop}] drop";
            return null;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, richText = true };
            DrawDebug();
        }

        private string InventoryText()
        {
            text.Clear();
            text.AppendLine($"<b>POCKETS</b> {carrier.Inventory.Count}/{carrier.Inventory.Capacity}   ({InputBindings.Display("Drop")} drops the last)");
            int total = 0;
            foreach (Grabbable item in carrier.Inventory.Items)
            {
                int value = item.TryGetComponent(out IValuable v) ? v.CurrentValue : 0;
                total += value;
                text.AppendLine($"  {item.DisplayName}  ${value:N0}");
            }
            text.Append($"<b>TOTAL</b> ${total:N0}");
            return text.ToString();
        }

        private void DrawDebug()
        {
            string held = carrier.Held != null ? $"{carrier.Held.DisplayName} ({carrier.Held.CarryClass}, {carrier.Held.Weight:F1} kg)" : "-";
            string content =
                "<b>CARRY</b>\n" +
                $"Held       {held}\n" +
                $"Carried    {carrier.CarriedWeight:F1} kg\n" +
                $"Speed x    {carrier.Config.SpeedMultiplierFor(carrier.CarriedWeight):F2}\n" +
                $"Charge     {interactor.Charge:F2}" + SharedDebug();
            var gc = new GUIContent(content);
            Vector2 size = box.CalcSize(gc);
            GUI.Box(new Rect(Screen.width - size.x - 10, 230, size.x, size.y), gc, box);
        }

        private string SharedDebug()
        {
            if (!carrier.IsSharing) return "";
            SharedCarryable shared = carrier.Held.Shared;
            int point = shared.IndexOf(carrier);
            float tether = point >= 0 ? Vector3.ProjectOnPlane(carrier.transform.position - shared.AnchorFor(point), Vector3.up).magnitude : 0f;
            return $"\nShared     point {point}, {shared.CarrierCount}/{shared.RequiredCarriers}, share {shared.SharePerCarrier:0} kg" +
                   $"\nCrew cap   {shared.CarrierMaxSpeed:0.0} m/s, tether {tether:0.00}/{shared.Config.TetherSlack:0.0} m";
        }

        private static string Describe(Grabbable g)
        {
            string value = g.TryGetComponent(out IValuable v) ? $"${v.CurrentValue:N0} · " : "";
            return $"{g.DisplayName} ({value}{g.Weight:0.#} kg)";
        }
    }
}

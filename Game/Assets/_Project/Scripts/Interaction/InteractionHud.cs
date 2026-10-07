using System.Text;
using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// Placeholder HUD until the UI milestone: crosshair, pickup prompt, rejection hints, throw
    /// charge bar, and the Tab inventory with pocket total. F1 adds carry debug numbers.
    /// </summary>
    public class InteractionHud : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerInputReader inputReader;

        private GUIStyle centered;
        private GUIStyle box;
        private readonly StringBuilder text = new();

        private void OnGUI()
        {
            centered ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, richText = true };
            box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, richText = true };

            float cx = Screen.width / 2f, cy = Screen.height / 2f;
            GUI.Label(new Rect(cx - 10, cy - 12, 20, 24), "+", centered);

            string prompt = null;
            if (carrier.HintVisible) prompt = $"<color=#FF8060>{carrier.Hint}</color>";
            else if (interactor.Target != null && interactor.Target.Shared != null)
                prompt = $"[{InputBindings.Display("Interact")}] Grab {Describe(interactor.Target)} - {SharedCarryText.Crew(interactor.Target.Shared.CarrierCount, interactor.Target.Shared.RequiredCarriers)}";
            else if (interactor.Target != null) prompt = $"[{InputBindings.Display("Interact")}] Pick up {Describe(interactor.Target)}";
            else if (carrier.Held == null && interactor.UseTarget?.UsePrompt(interactor.gameObject) is string use) prompt = $"[{InputBindings.Display("Interact")}] {use}";
            else if (carrier.IsSharing) prompt = $"{carrier.Held.DisplayName}: {SharedCarryText.Of(carrier.Held.Shared)}   [RMB] let go";
            else if (carrier.IsDragging) prompt = $"Dragging {Describe(carrier.Held)}   [RMB] let go";
            else if (carrier.Held != null) prompt = $"Holding {Describe(carrier.Held)}   [LMB] throw   [RMB] drop";
            if (prompt != null) GUI.Label(new Rect(cx - 300, cy + 30, 600, 26), prompt, centered);

            if (interactor.Charge > 0f)
            {
                GUI.Box(new Rect(cx - 60, cy + 60, 120, 10), GUIContent.none);
                GUI.Box(new Rect(cx - 60, cy + 60, 120 * interactor.Charge, 10), GUIContent.none);
            }

            if (inputReader.Current.InventoryHeld) DrawInventory();
            if (DebugView.Visible) DrawDebug();
        }

        private void DrawInventory()
        {
            text.Clear();
            text.AppendLine($"<b>POCKETS</b> {carrier.Inventory.Count}/{carrier.Inventory.Capacity}   (RMB drops last)");
            int total = 0;
            foreach (Grabbable item in carrier.Inventory.Items)
            {
                int value = item.TryGetComponent(out IValuable v) ? v.CurrentValue : 0;
                total += value;
                text.AppendLine($"  {item.DisplayName,-18} ${value:N0}");
            }
            text.Append($"<b>Total</b> ${total:N0}");
            var content = new GUIContent(text.ToString());
            Vector2 size = box.CalcSize(content);
            GUI.Box(new Rect(Screen.width / 2f - size.x / 2f, Screen.height - size.y - 40, size.x, size.y), content, box);
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

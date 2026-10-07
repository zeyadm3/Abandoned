using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Interaction
{
    /// <summary>
    /// The player's interaction HUD (M8.1; key caps in UI step 2): crosshair (a ring on something usable),
    /// what you can do with what you're looking at or holding, rejection hints and the throw charge. Keys
    /// follow the player's bindings. The pockets moved to UI.PlayerHud. F1 adds carry debug numbers.
    /// </summary>
    public class InteractionHud : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerInputReader inputReader;

        private GUIStyle box;
        private VisualElement crosshair, charge, chargeFill, prompt;
        private string shownPrompt;

        /// <summary>Tests: the prompt under the crosshair (null when none).</summary>
        public string Prompt { get; private set; }

        private void Update()
        {
            Prompt = BuildPrompt(out bool hint);
            if (crosshair == null)
            {
                if (UI.HudLayer.Root == null) return;
                crosshair = UI.HudLayer.Add(new VisualElement(), "hud-crosshair");
                prompt = UI.HudLayer.Add(new VisualElement(), "hud-prompt-row");
                charge = UI.HudLayer.Add(new VisualElement(), "hud-charge");
                chargeFill = new VisualElement { pickingMode = PickingMode.Ignore };
                chargeFill.AddToClassList("hud-charge__fill");
                charge.Add(chargeFill);
            }
            // Key caps for [key] markers; rebuilt only when the words change.
            // The pockets list (Inventory held) takes the bottom of the screen; the prompt steps aside.
            UI.MenuKit.Show(prompt, Prompt != null && !inputReader.Current.InventoryHeld);
            if (Prompt != shownPrompt)
            {
                shownPrompt = Prompt;
                UI.UiKit.Prompt(prompt, Prompt);
                prompt.BringToFront(); // over any value tags behind it
            }
            prompt.EnableInClassList("hud-prompt-row--hint", hint);
            bool target = interactor.Target != null || (carrier.Held == null && interactor.UseTarget != null);
            crosshair.EnableInClassList("hud-crosshair--target", target);
            UI.MenuKit.Show(charge, interactor.Charge > 0f);
            chargeFill.style.width = Length.Percent(interactor.Charge * 100f);
        }

        private void OnDisable()
        {
            foreach (VisualElement e in new[] { crosshair, prompt, charge }) if (e != null) UI.MenuKit.Show(e, false);
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
        }

        private string BuildPrompt(out bool hint)
        {
            hint = false;
            string interact = InputBindings.Display("Interact"), use = InputBindings.Display("Use"), drop = InputBindings.Display("Drop"), rotate = InputBindings.Display("Rotate");
            if (carrier.HintVisible) { hint = true; return carrier.Hint; }
            if (interactor.IsPlacing)
            {
                hint = interactor.PlacementBlocked;
                return interactor.PlacementBlocked ? $"No clear support - release [{drop}] to reposition" : $"[{drop}] hold to set down gently / release to cancel";
            }
            if (interactor.IsRotating) return $"[{rotate}] hold + mouse to rotate   release to look";
            if (interactor.Target != null && interactor.Target.Shared != null)
                return $"[{interact}] Grab {Describe(interactor.Target)} - {SharedCarryText.Crew(interactor.Target.Shared.CarrierCount, interactor.Target.Shared.RequiredCarriers)}";
            if (interactor.Target != null) return $"[{interact}] Pick up {Describe(interactor.Target)}";
            if (carrier.Held == null && interactor.UseTarget?.UsePrompt(interactor.gameObject) is string usable) return $"[{interact}] {usable}";
            if (carrier.IsSharing) return $"{carrier.Held.DisplayName}: {SharedCarryText.Of(carrier.Held.Shared)}   [{drop}] let go";
            if (carrier.IsDragging) return $"Dragging {Describe(carrier.Held)}   [{drop}] let go";
            if (carrier.Held != null) return $"[{use}] hold to throw   [{drop}] tap: drop / hold: set down   [{rotate}] rotate";
            return null;
        }

        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, richText = true };
            DrawDebug();
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

        // The value tag over the item says what it's worth (UI.LootTags); the prompt says what it is and weighs.
        private static string Describe(Grabbable g) => $"{g.DisplayName} ({g.Weight:0.#} kg)";
    }
}

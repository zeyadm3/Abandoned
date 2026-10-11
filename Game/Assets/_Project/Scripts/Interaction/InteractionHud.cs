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
        private VisualElement crosshair, charge, chargeFill, prompt, promptRow;
        private Label context, chargeReadout;
        private string shownPrompt;

        /// <summary>Tests: the prompt under the crosshair (null when none).</summary>
        public string Prompt { get; private set; }

        /// <summary>
        /// Shown instead of everything else while set: the player is somewhere the usual prompts don't apply
        /// (hiding in a locker, where the only thing to do is climb out).
        /// </summary>
        public string Override { get; set; }

        private void Update()
        {
            Prompt = BuildPrompt(out bool hint);
            if (crosshair == null || crosshair.panel == null)
            {
                if (UI.HudLayer.Root == null) return;
                UI.HudLayer.Remove(crosshair);
                UI.HudLayer.Remove(prompt);
                UI.HudLayer.Remove(charge);
                crosshair = UI.HudLayer.Add(new VisualElement(), "hud-crosshair", "h-reticle");
                foreach (string corner in new[] { "nw", "ne", "sw", "se" })
                {
                    var bracket = new VisualElement { pickingMode = PickingMode.Ignore };
                    bracket.AddToClassList("h-reticle__corner");
                    bracket.AddToClassList("h-reticle__corner--" + corner);
                    crosshair.Add(bracket);
                }
                prompt = UI.HudLayer.Add(new VisualElement(), "h-interaction");
                context = new Label("RECOVER") { pickingMode = PickingMode.Ignore };
                context.AddToClassList("h-interaction__context");
                prompt.Add(context);
                promptRow = new VisualElement { pickingMode = PickingMode.Ignore };
                promptRow.AddToClassList("hud-prompt-row");
                promptRow.AddToClassList("h-interaction__actions");
                prompt.Add(promptRow);
                charge = UI.HudLayer.Add(new VisualElement(), "hud-charge", "h-throw");
                chargeReadout = new Label("THROW / 00%") { pickingMode = PickingMode.Ignore };
                chargeReadout.AddToClassList("h-throw__label");
                charge.Add(chargeReadout);
                chargeFill = new VisualElement { pickingMode = PickingMode.Ignore };
                chargeFill.AddToClassList("hud-charge__fill");
                charge.Add(chargeFill);
                shownPrompt = null;
            }
            // Key caps for [key] markers; rebuilt only when the words change.
            // The pockets list (Inventory held) takes the bottom of the screen; the prompt steps aside.
            UI.MenuKit.Show(prompt, Prompt != null && !inputReader.Current.InventoryHeld);
            if (Prompt != shownPrompt)
            {
                shownPrompt = Prompt;
                UI.UiKit.Prompt(promptRow, Prompt);
                prompt.BringToFront(); // over any value tags behind it
            }
            string caption = hint ? "OBSTRUCTED" : !string.IsNullOrEmpty(Override) ? "INTERACT" : interactor.IsPlacing ? "SET DOWN"
                : interactor.IsRotating ? "ORIENT" : interactor.Target != null ? "RECOVER" : carrier.Held != null ? "CARRY" : "INTERACT";
            if (context.text != caption) context.text = caption;
            prompt.EnableInClassList("h-interaction--hint", hint);
            promptRow.EnableInClassList("hud-prompt-row--hint", hint);
            // Use the same measured lane as dispatch and captions when corner instruments intrude.
            VisualElement root = UI.HudLayer.Root;
            float promptY = root.layout.height * .55f;
            bool crowded = UI.HudLayer.TryLocalRect(root.Q(className: "h-inventory"), root, out Rect inventory) &&
                           inventory.yMin < promptY + Mathf.Max(110f, prompt.layout.height);
            if (crowded) UI.HudLayer.FitCenterLane(prompt, root, 720f);
            else { prompt.style.left = Length.Percent(22f); prompt.style.right = Length.Percent(22f); }
            bool target = interactor.Target != null || (carrier.Held == null && interactor.UseTarget != null);
            crosshair.EnableInClassList("hud-crosshair--target", target);
            // Settings > Gameplay > Crosshair: off hides the dot, but the ring still shows on something usable.
            crosshair.EnableInClassList("hud-crosshair--hidden", !GameSettings.ShowCrosshair && !target);
            UI.MenuKit.Show(charge, interactor.Charge > 0f);
            float strength = Mathf.Clamp01(interactor.Charge);
            chargeFill.style.width = Length.Percent(strength * 100f);
            string chargeText = $"THROW / {Mathf.CeilToInt(strength * 100f):00}%";
            if (chargeReadout.text != chargeText) chargeReadout.text = chargeText;
            if (UI.HudLayer.TryLocalRect(prompt, root, out Rect actions))
            {
                charge.style.top = actions.yMax + 8f;
                charge.style.left = actions.center.x;
            }
            else
            {
                charge.style.top = Length.Percent(62f);
                charge.style.left = Length.Percent(50f);
            }
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
            if (!string.IsNullOrEmpty(Override)) return Override;
            if (carrier.HintVisible) { hint = true; return carrier.Hint; }
            if (interactor.IsPlacing)
            {
                hint = interactor.PlacementBlocked;
                return interactor.PlacementBlocked ? $"No support / release [{drop}] to reposition" : $"[{drop}] hold to place / release to cancel";
            }
            if (interactor.IsRotating) return $"[{rotate}] hold + mouse / rotate   release / look";
            if (interactor.Target != null && interactor.Target.Shared != null)
                return $"[{interact}] Grab {Describe(interactor.Target)} - {SharedCarryText.Crew(interactor.Target.Shared.CarrierCount, interactor.Target.Shared.RequiredCarriers)}";
            if (interactor.Target != null) return $"[{interact}] Pick up {Describe(interactor.Target)}";
            if (carrier.Held == null && interactor.UseTarget?.UsePrompt(interactor.gameObject) is string usable) return $"[{interact}] {usable}";
            if (carrier.IsSharing) return $"{carrier.Held.DisplayName}: {SharedCarryText.Of(carrier.Held.Shared)}   [{drop}] let go";
            if (carrier.IsDragging) return $"Dragging {Describe(carrier.Held)}   [{drop}] let go";
            if (carrier.Held != null) return $"[{use}] hold / throw   [{drop}] tap / drop, hold / place   [{rotate}] rotate";
            return null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 14, richText = true };
            DrawDebug();
        }
#endif

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

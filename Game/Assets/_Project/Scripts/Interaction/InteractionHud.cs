using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Interaction
{
    /// <summary>Compact object-anchored prompts and a small deliberate-throw charge.</summary>
    public class InteractionHud : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerInputReader inputReader;
        private VisualElement crosshair, charge, chargeFill, prompt, promptRow;
        private string shownPrompt;
        private Vector3 lastAnchor;
        public string Prompt { get; private set; }
        public string Override { get; set; }
        private void Update()
        {
            Prompt = BuildPrompt();
            if (crosshair == null || crosshair.panel == null)
            {
                if (UI.HudLayer.Root == null) return;
                UI.HudLayer.Remove(crosshair); UI.HudLayer.Remove(prompt); UI.HudLayer.Remove(charge);
                crosshair = UI.HudLayer.Add(new VisualElement(), "hud-crosshair", "h-reticle", "field-reticle");
                foreach (string corner in new[] { "nw", "ne", "sw", "se" })
                {
                    var bracket = new VisualElement { pickingMode = PickingMode.Ignore };
                    bracket.AddToClassList("h-reticle__corner");
                    bracket.AddToClassList("h-reticle__corner--" + corner);
                    crosshair.Add(bracket);
                }
                prompt = UI.HudLayer.Add(new VisualElement(), "field-prompt");
                promptRow = UI.FieldHud.Element(prompt, "hud-prompt-row");
                charge = UI.HudLayer.Add(new VisualElement(), "field-throw");
                chargeFill = UI.FieldHud.Element(charge, "field-throw-fill");
                shownPrompt = null;
            }
            bool show = Prompt != null && !inputReader.Current.InventoryHeld;
            UI.MenuKit.Show(prompt, show);
            if (Prompt != shownPrompt)
            {
                shownPrompt = Prompt;
                UI.UiKit.Prompt(promptRow, Prompt);
            }
            bool target = interactor.Target != null || interactor.UseTarget != null;
            crosshair.EnableInClassList("hud-crosshair--target", target);
            crosshair.EnableInClassList("hud-crosshair--hidden", !GameSettings.ShowCrosshair && !target);
            if (show)
            {
                Camera view = Camera.main;
                if (interactor.Target != null)
                {
                    Bounds bounds = interactor.Target.GetBounds();
                    lastAnchor = bounds.center + (view != null ? view.transform.right : Vector3.right) * (bounds.extents.magnitude * .6f + .1f);
                }
                else if (interactor.UseTarget is Component usable)
                    lastAnchor = usable.GetComponent<Collider>() is Collider collider ? collider.bounds.center : usable.transform.position + Vector3.up;
                else if (carrier.Held != null) lastAnchor = carrier.Held.GetBounds().center;
                UI.HudLayer.Place(prompt, lastAnchor, view);
                VisualElement root = UI.HudLayer.Root;
                float width = UI.HudLayer.Finite(prompt.layout.width) ? prompt.layout.width : 200;
                float height = UI.HudLayer.Finite(prompt.layout.height) ? prompt.layout.height : 32;
                prompt.style.left = Mathf.Clamp(prompt.style.left.value.value, 12, Mathf.Max(12, root.layout.width - width - 18));
                prompt.style.top = Mathf.Clamp(prompt.style.top.value.value, height * .5f + 12, Mathf.Max(height * .5f + 12, root.layout.height - height * .5f - 12));
            }
            UI.MenuKit.Show(charge, interactor.Charge > 0f);
            chargeFill.style.width = Length.Percent(Mathf.Clamp01(interactor.Charge) * 100f);
        }
        private string BuildPrompt()
        {
            if (!string.IsNullOrEmpty(Override)) return Override;
            if (carrier.HintVisible) return carrier.Hint;
            string interact = InputBindings.Display("Interact"), drop = InputBindings.Display("Drop");
            if (interactor.IsPlacing) return interactor.PlacementBlocked ? Loc.T("No clear place to set it down") : Loc.T("SET DOWN");
            if (interactor.IsRotating) return Loc.T("ORIENT");
            if (interactor.Target != null)
                return Loc.T("[{0}] Pick up {1}", interact, interactor.Target.LocalName);
            if (interactor.UseTarget?.UsePrompt(interactor.gameObject) is string usable)
                return $"[{interact}] {usable}";
            return null;
        }
        private void OnDisable()
        {
            UI.MenuKit.Show(crosshair, false); UI.MenuKit.Show(prompt, false); UI.MenuKit.Show(charge, false);
        }
        private void OnEnable() => UI.MenuKit.Show(crosshair, true);
        private void OnDestroy()
        {
            UI.HudLayer.Remove(crosshair); UI.HudLayer.Remove(prompt); UI.HudLayer.Remove(charge);
        }
    }
}

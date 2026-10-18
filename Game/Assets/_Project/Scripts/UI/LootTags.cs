using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The local scan reveals loot prices for a few seconds. Focused and held tags, pockets and the run
    /// tally share that reveal window, keeping exploration clear until the player asks for information.
    /// </summary>
    public partial class LootTags : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerInputReader inputReader;
        [Tooltip("Scan reach (m), how long its tags stay (s), and the wait before the next ping (s).")]
        [SerializeField, Min(2f)] private float scanRange = 20f;
        [SerializeField, Min(1f)] private float scanSeconds = 5f;
        [SerializeField, Min(0.2f)] private float scanCooldown = 2.5f;
        [SerializeField, Range(1, 64)] private int maxScanTags = 14;
        [Tooltip("Through walls only this close (m); further away the loot has to be in sight.")]
        [SerializeField, Min(0f)] private float throughWalls = 7f;

        private sealed class Tag
        {
            public VisualElement Root;
            public Label Value, Name;
            public VisualElement Meta;
            public LootItem Item;
            public float ShowAt, HideAt;
            public string Key;
        }

        private readonly List<Tag> scanTags = new();
        private Tag focusTag;
        private VisualElement ring;
        private InputAction scanAction;
        private float nextScan;
        private int sightMask;
        private static float revealUntil;
        private static NetworkPlayer revealPlayer;
        private static int revealTravel = -1;
        private static readonly HashSet<LootItem> inspected = new();
        public static bool Knows(LootItem item) => item != null && (item.IsAppraised || inspected.Contains(item));
        public static string ValueText(LootItem item)
        {
            if (item == null || item.Definition == null) return "--";
            if (item.IsAppraised) return "$" + item.CurrentValue.ToString("N0");
            LootDefinition definition = item.Definition;
            NetworkLoot network = NetworkLoot.Of(item.GetComponent<Grabbable>());
            uint identity = network != null ? (uint)network.NetworkObjectId : unchecked((uint)item.GetInstanceID());
            identity = unchecked((identity ^ 0x9e3779b9u) * 2654435761u);
            float bias = .85f + identity % 1000 / 1000f * .3f;
            float estimate = (definition.ValueMin * .5f + definition.ValueMax * .5f) * (definition.ConditionMin + definition.ConditionMax) * .5f * bias;
            if (item.FullValue > 0) estimate *= (float)item.CurrentValue / item.FullValue;
            int step = estimate >= 10000 ? 500 : estimate >= 1000 ? 100 : estimate >= 100 ? 10 : 5;
            return (Mathf.Max(0, Mathf.RoundToInt(estimate / step) * step)).ToString("N0") + "~";
        }

        /// <summary>Local Q-scan reveal, shared by exploration HUDs; financial screens stay contextual.</summary>
        public static bool ValuesVisible => revealPlayer != null && revealPlayer == NetworkPlayer.Local &&
                                            revealPlayer.IsSpawned && !revealPlayer.IsDead && Time.time < revealUntil &&
                                            revealTravel == (SessionTravel.Current != null ? SessionTravel.Current.TravelId : -1);

        private void Awake() =>
            sightMask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");

        /// <summary>Tests/F1: tags the last scan put up (still showing).</summary>
        public int ScanCount { get; private set; }
        /// <summary>Tests: the loot the focus tag is on (look-at or held), or null.</summary>
        public LootItem Focus { get; private set; }

        private bool Mine => player != null && player.IsSpawned && player.IsOwner;

        private void Update()
        {
            if (!Mine || HudLayer.Root == null) return;
            DropStale();
            Camera camera = Camera.main;
            bool alive = !player.IsDead;
            scanAction ??= inputReader.Actions?.FindAction("Gameplay/Scan");
            if (alive && scanAction != null && scanAction.WasPressedThisFrame() && inputReader.enabled && !Core.CursorOwner.UiActive) Scan();
            UpdateFocus(camera, alive && ValuesVisible);
            UpdateScan(camera);
            UpdateReady(alive);
        }

        // The HUD was emptied (a new language): tags and the reminder are made again, worded anew.
        private void DropStale()
        {
            if (ready != null && ready.panel == null) { ready = scanCap = null; scanBinding = null; }
            if (ring != null && ring.panel == null) ring = null;
            if (focusTag != null && focusTag.Root.panel == null) focusTag = null;
            if (scanTags.Count > 0 && scanTags[0].Root.panel == null) scanTags.Clear();
        }

        // A quiet reminder that the scan exists, when it's ready.
        private VisualElement ready, scanCap;
        private string scanBinding;

        private void UpdateReady(bool alive)
        {
            if (ready == null)
            {
                ready = HudLayer.Add(new VisualElement(), "scan-ready");
                if (ready == null) return;
                var text = new Label(Loc.T("SCAN FOR LOOT")) { pickingMode = PickingMode.Ignore };
                text.AddToClassList("scan-ready__text");
                ready.Add(text);
            }
            string binding = InputBindings.Display("Scan");
            if (binding != scanBinding)
            {
                scanBinding = binding;
                scanCap?.RemoveFromHierarchy();
                scanCap = UiKit.Key(null, binding);
                ready.Insert(0, scanCap);
            }
            MenuKit.Show(ready, false); // Scan is available through its binding; no permanent reminder panel.
        }

        // ---- The tag on what you look at or hold ----

        private void UpdateFocus(Camera camera, bool alive)
        {
            focusTag ??= MakeTag(scan: false);
            Grabbable g = interactor.Target != null ? interactor.Target : carrier.Held;
            LootItem item = alive && g != null ? g.GetComponent<LootItem>() : null;
            if (item == null || item.Definition == null || camera == null)
            {
                Focus = null;
                Hide(focusTag);
                return;
            }
            Focus = item;
            inspected.Add(item);
            Fill(focusTag, item, full: true);
            focusTag.Root.RemoveFromClassList("loot-tag--hidden");
            focusTag.Root.RemoveFromClassList("loot-tag--held");
            focusTag.Root.style.right = StyleKeyword.Null;
            Place(focusTag, g.GetBounds(), camera);
        }

        // ---- Tags ----

        private Tag MakeTag(bool scan)
        {
            var t = new Tag { Root = new VisualElement { pickingMode = PickingMode.Ignore } };
            t.Root.AddToClassList("loot-tag");
            t.Root.AddToClassList("loot-tag--hidden");
            if (scan) t.Root.AddToClassList("loot-tag--scan");
            t.Value = new Label { pickingMode = PickingMode.Ignore };
            t.Value.AddToClassList("loot-tag__value");
            t.Root.Add(t.Value);
            t.Name = new Label { pickingMode = PickingMode.Ignore };
            t.Name.AddToClassList("loot-tag__name");
            t.Root.Add(t.Name);
            t.Meta = new VisualElement { pickingMode = PickingMode.Ignore };
            t.Meta.AddToClassList("loot-tag__meta");
            t.Root.Add(t.Meta);
            HudLayer.World?.Add(t.Root);
            MenuKit.Show(t.Root, false);
            return t;
        }

        private static void Fill(Tag t, LootItem item, bool full)
        {
            t.Item = item;
            LootDefinition d = item.Definition;
            string valueText = ValueText(item);
            int value = item.CurrentValue;
            string key = $"{d.Id}|{valueText}|{full}";
            if (key == t.Key) return;
            t.Key = key;
            t.Value.text = valueText;
            t.Name.text = d.LocalName;
            // Before appraisal, styling must not reveal a hidden price tier or jackpot.
            int tier = item.IsAppraised ? Money.Tier(value) : 0;
            for (int i = 1; i <= 3; i++) t.Root.EnableInClassList($"loot-tag--t{i}", tier == i);
            t.Root.EnableInClassList("loot-tag--broken", item.FullValue > 0 && value < item.FullValue * 0.5f);
            t.Meta.Clear();
            MenuKit.Show(t.Name, full);
            MenuKit.Show(t.Meta, full);
            if (!full) return;
            string fragility = FragilityText(d.Fragility);
            UiKit.Tag(t.Meta, Loc.T("{0:0.#} kg", d.GameplayWeight)).AddToClassList("tag--dim");
            UiKit.Tag(t.Meta, fragility).AddToClassList(d.Fragility >= Fragility.High ? "tag--warn" : "tag--dim");
        }

        public static string FragilityText(Fragility fragility) => fragility switch
            {
                Fragility.None => Loc.T("NOT FRAGILE"),
                Fragility.Low => Loc.T("LOW FRAGILITY"),
                Fragility.Medium => Loc.T("MEDIUM FRAGILITY"),
                Fragility.High => Loc.T("FRAGILE"),
                _ => Loc.T("VERY FRAGILE"),
            };

        private static void Place(Tag t, Bounds bounds, Camera camera)
        {
            MenuKit.Show(t.Root, true);
            HudLayer.Place(t.Root, bounds.center + Vector3.up * (bounds.extents.y + 0.12f), camera);
            VisualElement world = HudLayer.World;
            if (world == null || !HudLayer.Finite(t.Root.layout.width) || !HudLayer.Finite(t.Root.layout.height)) return;
            // World tags translate by half their width and their full height; keep the resulting box inside the view.
            float half = t.Root.layout.width * 0.5f;
            float x = t.Root.style.left.value.value, y = t.Root.style.top.value.value;
            if (!HudLayer.Finite(x) || !HudLayer.Finite(y)) return;
            t.Root.style.left = Mathf.Clamp(x, half + 12f, Mathf.Max(half + 12f, world.layout.width - half - 12f));
            t.Root.style.top = Mathf.Clamp(y, t.Root.layout.height + 12f, Mathf.Max(t.Root.layout.height + 12f, world.layout.height - 12f));
        }

        private static void Hide(Tag t)
        {
            if (t?.Root == null) return;
            t.Root.AddToClassList("loot-tag--hidden");
            MenuKit.Show(t.Root, false);
        }

        private void OnDisable()
        {
            if (revealPlayer == player) { revealUntil = 0f; revealPlayer = null; }
            Hide(focusTag);
            foreach (Tag t in scanTags) Hide(t);
            if (ready != null) MenuKit.Show(ready, false);
            if (ring != null) MenuKit.Show(ring, false);
        }

        private void OnDestroy()
        {
            HudLayer.Remove(focusTag?.Root);
            HudLayer.Remove(ring);
            HudLayer.Remove(ready);
            foreach (Tag t in scanTags) HudLayer.Remove(t.Root);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { revealUntil = 0f; revealPlayer = null; revealTravel = -1; inspected.Clear(); }

#if UNITY_EDITOR
        public void EditorSetup(NetworkPlayer owner, PlayerInteractor playerInteractor, PlayerCarrier playerCarrier, PlayerInputReader reader)
        {
            player = owner;
            interactor = playerInteractor;
            carrier = playerCarrier;
            inputReader = reader;
        }
#endif
    }
}

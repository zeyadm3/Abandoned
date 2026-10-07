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
    public class LootTags : MonoBehaviour
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
            Camera camera = Camera.main;
            bool alive = !player.IsDead;
            scanAction ??= inputReader.Actions?.FindAction("Gameplay/Scan");
            if (alive && scanAction != null && scanAction.WasPressedThisFrame() && inputReader.enabled && !Core.CursorOwner.UiActive) Scan();
            UpdateFocus(camera, alive && ValuesVisible);
            UpdateScan(camera);
            UpdateReady(alive);
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
                var text = new Label("SCAN FOR LOOT") { pickingMode = PickingMode.Ignore };
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
            MenuKit.Show(ready, alive && !ValuesVisible && Time.time >= nextScan && (carrier.Held != null || interactor.Target != null));
        }

        // ---- The tag on what you look at or hold ----

        private void UpdateFocus(Camera camera, bool alive)
        {
            focusTag ??= MakeTag(scan: false);
            Grabbable g = carrier.Held != null ? carrier.Held : interactor.Target;
            LootItem item = alive && g != null ? g.GetComponent<LootItem>() : null;
            if (item == null || item.Definition == null || camera == null)
            {
                Focus = null;
                Hide(focusTag);
                return;
            }
            Focus = item;
            Fill(focusTag, item, full: true);
            focusTag.Root.RemoveFromClassList("loot-tag--hidden");
            bool held = carrier.Held != null;
            focusTag.Root.EnableInClassList("loot-tag--held", held);
            if (held)
            {
                focusTag.Root.style.left = StyleKeyword.Null;
                focusTag.Root.style.top = StyleKeyword.Null;
                MenuKit.Show(focusTag.Root, true);
            }
            else Place(focusTag, g.GetBounds(), camera);
        }

        // ---- The scan ----

        /// <summary>Ping: tag the loot around you (the Scan key; tests call it directly).</summary>
        public void Scan()
        {
            if (Time.time < nextScan || !Mine || player.IsDead) return;
            nextScan = Time.time + scanCooldown;
            revealUntil = Time.time + scanSeconds + 0.45f;
            revealPlayer = player;
            revealTravel = SessionTravel.Current != null ? SessionTravel.Current.TravelId : -1;
            Camera camera = Camera.main;
            Audio.GameAudio.PlayUi(Audio.SoundId.ScanPing, 0.7f);
            Ring();
            foreach (Tag t in scanTags) { Hide(t); t.Item = null; }
            if (camera == null) return;

            var found = new List<(LootItem item, float distance)>();
            Vector3 eye = camera.transform.position;
            foreach (LootItem item in FindObjectsByType<LootItem>(FindObjectsSortMode.None))
            {
                if (item == null || item.Definition == null || item.IsShattered || item.Definition.Utility) continue;
                if (item.TryGetComponent(out Grabbable g) && (g.IsPocketed || g.IsHeldBy(carrier))) continue;
                float d = Vector3.Distance(eye, item.transform.position);
                if (d > scanRange) continue;
                Vector3 v = camera.WorldToViewportPoint(item.transform.position);
                if (v.z <= 0f || v.x < -0.05f || v.x > 1.05f || v.y < -0.05f || v.y > 1.05f) continue;
                if (d > throughWalls && Physics.Linecast(eye, item.transform.position, sightMask, QueryTriggerInteraction.Ignore)) continue;
                found.Add((item, d));
            }
            found.Sort((a, b) => a.distance.CompareTo(b.distance));
            int n = Mathf.Min(found.Count, maxScanTags);
            while (scanTags.Count < n) scanTags.Add(MakeTag(scan: true));
            for (int i = 0; i < n; i++)
            {
                Tag t = scanTags[i];
                t.Item = found[i].item;
                // The ring reaches the far ones a moment later.
                t.ShowAt = Time.time + found[i].distance / scanRange * 0.45f;
                t.HideAt = t.ShowAt + scanSeconds;
                Fill(t, t.Item, full: false);
            }
            ScanCount = n;
        }

        private void UpdateScan(Camera camera)
        {
            int live = 0;
            foreach (Tag t in scanTags)
            {
                Grabbable g = t.Item != null ? t.Item.GetComponent<Grabbable>() : null;
                if (!ValuesVisible || t.Item == null || camera == null || Time.time >= t.HideAt || t.Item.IsShattered || t.Item == Focus || (g != null && g.IsPocketed))
                {
                    Hide(t);
                    if (t.Item != null && Time.time >= t.HideAt) t.Item = null;
                    continue;
                }
                if (Time.time < t.ShowAt) continue;
                live++;
                if (g != null) Place(t, g.GetBounds(), camera);
                // In on the first shown frame's next frame (so it pops), out over the last moments.
                bool wasHidden = t.Root.ClassListContains("loot-tag--hidden");
                bool fading = t.HideAt - Time.time < 0.4f;
                if (fading) t.Root.AddToClassList("loot-tag--hidden");
                else if (wasHidden) t.Root.schedule.Execute(() => t.Root.RemoveFromClassList("loot-tag--hidden")).StartingIn(16);
            }
            ScanCount = live;
        }

        private void Ring()
        {
            if (ring == null)
            {
                ring = HudLayer.Add(new VisualElement(), "scan-ring");
                if (ring == null) return;
            }
            ring.RemoveFromClassList("scan-ring--out");
            ring.style.display = DisplayStyle.Flex;
            ring.schedule.Execute(() => ring.AddToClassList("scan-ring--out")).StartingIn(16);
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
            int value = item.CurrentValue;
            string key = $"{d.Id}|{value}|{full}";
            if (key == t.Key) return;
            t.Key = key;
            t.Value.text = $"${value:N0}";
            t.Name.text = d.DisplayName;
            int tier = d.Jackpot ? 3 : Money.Tier(value);
            for (int i = 1; i <= 3; i++) t.Root.EnableInClassList($"loot-tag--t{i}", tier == i);
            t.Root.EnableInClassList("loot-tag--broken", item.FullValue > 0 && value < item.FullValue * 0.5f);
            t.Meta.Clear();
            MenuKit.Show(t.Name, full);
            MenuKit.Show(t.Meta, full);
            if (!full) return;
            string carry = d.CarryClass switch
            {
                CarryClass.Pocket => "POCKET",
                CarryClass.OneHand => "ONE HAND",
                CarryClass.TwoHand => "TWO HANDS",
                CarryClass.Heavy => "HEAVY - 2 PEOPLE",
                _ => "HUGE - A WHOLE CREW",
            };
            UiKit.Tag(t.Meta, carry).AddToClassList("tag--dim");
            if (d.Fragility >= Fragility.High) UiKit.Tag(t.Meta, d.Fragility == Fragility.Extreme ? "VERY FRAGILE" : "FRAGILE").AddToClassList("tag--warn");
            if (d.Jackpot) UiKit.Tag(t.Meta, "JACKPOT");
        }

        private static void Place(Tag t, Bounds bounds, Camera camera)
        {
            MenuKit.Show(t.Root, true);
            HudLayer.Place(t.Root, bounds.center + Vector3.up * (bounds.extents.y + 0.12f), camera);
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
        private static void ResetStatics() { revealUntil = 0f; revealPlayer = null; revealTravel = -1; }

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

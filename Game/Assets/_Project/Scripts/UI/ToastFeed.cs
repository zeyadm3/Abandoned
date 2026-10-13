using Abandoned.Core;
using System.Collections.Generic;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// Toasts (UI step 3): short notices that slide in top right under the clock and leave on their own:
    /// achievements, crewmates joining and leaving. Up to three at once, newest at the bottom. One per session
    /// (on the menu document's object); anything calls <see cref="Show"/>.
    /// </summary>
    public class ToastFeed : MonoBehaviour
    {
        private const float Seconds = 4.5f;
        private const int MaxShown = 3;

        public enum Kind { Info, Good, Warn }

        private sealed class Toast
        {
            public VisualElement Root;
            public float Until;
            public float Height = 100f;
        }

        private static readonly Queue<(string title, string body, string icon, Kind kind)> Waiting = new();
        private readonly List<Toast> shown = new();
        private readonly HashSet<ulong> crew = new();
        private readonly List<Vector2> blocked = new();
        private VisualElement stack;
        private bool crewKnown;

        /// <summary>Tests: the titles on screen now.</summary>
        public IEnumerable<string> Titles
        {
            get
            {
                foreach (Toast t in shown) yield return t.Root.Q<Label>("title")?.text;
            }
        }

        public static void Show(string title, string body, string icon = null, Kind kind = Kind.Info) => Waiting.Enqueue((title, body, icon, kind));

        private void Update()
        {
            Crew();
            VisualElement hud = HudLayer.Root;
            if (stack != null && stack.parent != hud)
            {
                HudLayer.Remove(stack);
                stack = null;
                shown.Clear();
            }
            bool compact = hud != null && (hud.layout.width < 1180f || hud.layout.height < 740f);
            PlaceStack(hud, compact);
            for (int i = shown.Count - 1; i >= 0; i--)
            {
                Toast t = shown[i];
                if (Time.time < t.Until) continue;
                t.Root.AddToClassList("toast--out");
                VisualElement root = t.Root;
                root.schedule.Execute(() => root.RemoveFromHierarchy()).StartingIn(220);
                shown.RemoveAt(i);
            }
            int limit = compact ? 1 : MaxShown;
            while (Waiting.Count > 0 && shown.Count < limit)
            {
                if (stack == null && (stack = HudLayer.Add(new VisualElement(), "toast-stack")) == null) return;
                (string title, string body, string icon, Kind kind) = Waiting.Dequeue();
                shown.Add(new Toast { Root = Build(title, body, icon, kind), Until = Time.time + Seconds });
                stack.BringToFront(); // over banners
                Audio.GameAudio.PlayUi(kind == Kind.Good ? Audio.SoundId.UiConfirm : Audio.SoundId.UiToast, kind == Kind.Good ? 0.6f : 1f);
            }
            PlaceStack(hud, compact, false);
        }

        private void PlaceStack(VisualElement hud, bool compact, bool pauseHidden = true)
        {
            if (stack == null || hud == null || !HudLayer.Finite(hud.layout.width) || !HudLayer.Finite(hud.layout.height)) return;
            float right = compact ? 20f : 28f, left = compact ? 20f : 28f;
            if (HudLayer.TryLocalRect(hud.Q(className: "h-recovery"), hud, out Rect recovery))
                right = hud.layout.width - recovery.xMax;
            else if (HudLayer.TryLocalRect(hud.Q(className: "h-inventory"), hud, out Rect inventory))
                right = hud.layout.width - inventory.xMax;
            if (HudLayer.TryLocalRect(hud.Q(className: "h-vitals"), hud, out Rect vitals)) left = vitals.xMin;
            float width = Mathf.Min(330f, hud.layout.width * 0.28f);
            Vector2 rightRoom = FreeSpace(hud, hud.layout.width - right - width, width);
            Vector2 leftRoom = FreeSpace(hud, left, width);
            float needed = 0f;
            foreach (Toast notice in shown)
            {
                if (notice.Root.resolvedStyle.display != DisplayStyle.None && HudLayer.Finite(notice.Root.layout.height) && notice.Root.layout.height > 0f)
                    notice.Height = notice.Root.layout.height + 8f;
                needed += notice.Height;
            }
            bool useLeft = rightRoom.y - rightRoom.x < needed && leftRoom.y - leftRoom.x > rightRoom.y - rightRoom.x;
            Vector2 room = useLeft ? leftRoom : rightRoom;
            stack.style.top = room.x;
            stack.style.left = useLeft ? new StyleLength(left) : new StyleLength(StyleKeyword.Auto);
            stack.style.right = useLeft ? new StyleLength(StyleKeyword.Auto) : new StyleLength(right);
            stack.style.width = width;
            float remaining = room.y - room.x;
            foreach (Toast notice in shown)
            {
                bool visible = hud.resolvedStyle.display != DisplayStyle.None && remaining >= notice.Height;
                MenuKit.Show(notice.Root, visible);
                if (visible) remaining -= notice.Height;
                else if (pauseHidden) notice.Until += Time.deltaTime;
            }
        }

        private Vector2 FreeSpace(VisualElement hud, float left, float width)
        {
            blocked.Clear();
            foreach (VisualElement item in hud.Children())
            {
                if (!(item.ClassListContains("h-recovery") || item.ClassListContains("h-inventory") ||
                      item.ClassListContains("h-inspection") || item.ClassListContains("instrument-hud") ||
                      item.ClassListContains("crew-voice") || item.ClassListContains("crew-transmissions") ||
                      item.ClassListContains("field-note") || item.ClassListContains("hq-brief"))) continue;
                if (HudLayer.TryLocalRect(item, hud, out Rect rect) && rect.xMin < left + width && rect.xMax > left)
                    blocked.Add(new Vector2(rect.yMin - 12f, rect.yMax + 12f));
            }
            // Chat sits beside the HUD to retain keyboard focus while typing.
            VisualElement parent = hud.parent;
            VisualElement chat = parent?.Q(className: "crew-transmissions");
            if (HudLayer.TryLocalRect(chat, parent, out Rect chatRect))
            {
                chatRect = parent.ChangeCoordinatesTo(hud, chatRect);
                if (chatRect.xMin < left + width && chatRect.xMax > left)
                    blocked.Add(new Vector2(chatRect.yMin - 12f, chatRect.yMax + 12f));
            }
            blocked.Sort((a, b) => a.x.CompareTo(b.x));
            float start = 28f, end = hud.layout.height - 40f;
            Vector2 best = new(start, start);
            foreach (Vector2 interval in blocked)
            {
                float stop = Mathf.Min(end, interval.x);
                if (stop - start > best.y - best.x) best = new Vector2(start, stop);
                start = Mathf.Max(start, interval.y);
            }
            if (end - start > best.y - best.x) best = new Vector2(start, end);
            return best;
        }

        private VisualElement Build(string title, string body, string icon, Kind kind)
        {
            var root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("toast");
            root.AddToClassList("toast--in");
            root.EnableInClassList("toast--good", kind == Kind.Good);
            root.EnableInClassList("toast--warn", kind == Kind.Warn);
            if (icon != null) UiKit.Icon(root, icon).AddToClassList("toast__icon");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("toast__text");
            root.Add(text);
            var t = new Label(title) { pickingMode = PickingMode.Ignore, name = "title" };
            t.AddToClassList("toast__title");
            text.Add(t);
            if (!string.IsNullOrEmpty(body))
            {
                var b = new Label(body) { pickingMode = PickingMode.Ignore };
                b.AddToClassList("toast__body");
                text.Add(b);
            }
            stack.Add(root);
            root.schedule.Execute(() => root.RemoveFromClassList("toast--in")).StartingIn(16);
            return root;
        }

        // Crewmates arriving and leaving (every machine sees the same players come and go).
        private void Crew()
        {
            var now = new HashSet<ulong>();
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.IsSpawned) now.Add(p.OwnerClientId);
            if (now.Count == 0)
            {
                crew.Clear();
                crewKnown = false;
                return;
            }
            if (crewKnown)
            {
                foreach (ulong id in now)
                    if (!crew.Contains(id)) Show(Loc.T("PLAYER {0} JOINED", id + 1), Loc.T("Another pair of hands."), "icon/multiplayer", Kind.Good);
                foreach (ulong id in crew)
                    if (!now.Contains(id)) Show(Loc.T("PLAYER {0} LEFT", id + 1), Loc.T("One less on the crew."), "icon/exitRight", Kind.Warn);
            }
            crew.Clear();
            crew.UnionWith(now);
            crewKnown = true;
        }

        private void OnDestroy() => HudLayer.Remove(stack);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Waiting.Clear();
    }
}

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
        }

        private static readonly Queue<(string title, string body, string icon, Kind kind)> Waiting = new();
        private readonly List<Toast> shown = new();
        private readonly HashSet<ulong> crew = new();
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
            for (int i = shown.Count - 1; i >= 0; i--)
            {
                Toast t = shown[i];
                if (Time.time < t.Until) continue;
                t.Root.AddToClassList("toast--out");
                VisualElement root = t.Root;
                root.schedule.Execute(() => root.RemoveFromHierarchy()).StartingIn(220);
                shown.RemoveAt(i);
            }
            while (Waiting.Count > 0 && shown.Count < MaxShown)
            {
                if (stack == null && (stack = HudLayer.Add(new VisualElement(), "toast-stack")) == null) return;
                (string title, string body, string icon, Kind kind) = Waiting.Dequeue();
                shown.Add(new Toast { Root = Build(title, body, icon, kind), Until = Time.time + Seconds });
                stack.BringToFront(); // over banners
                Audio.GameAudio.PlayUi(kind == Kind.Good ? Audio.SoundId.UiConfirm : Audio.SoundId.UiOpen, 0.6f);
            }
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
                    if (!crew.Contains(id)) Show($"PLAYER {id + 1} JOINED", "Another pair of hands.", "icon/multiplayer", Kind.Good);
                foreach (ulong id in crew)
                    if (!now.Contains(id)) Show($"PLAYER {id + 1} LEFT", "One less on the crew.", "icon/exitRight", Kind.Warn);
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

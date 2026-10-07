using System.Collections.Generic;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The chat on screen (UI step 10): recent lines bottom left, fading after a while; T opens a line to type
    /// (the player stands still meanwhile), Enter sends, Esc closes. Only during a game and not over menus.
    /// On the session object, beside the menus.
    /// </summary>
    public class ChatView : MonoBehaviour
    {
        private const int MaxLines = 7;
        private const float LineSeconds = 10f;

        private sealed class Line
        {
            public Label Label;
            public float Time;
        }

        private readonly List<Line> lines = new();
        private VisualElement root, log;
        private TextField field;
        private PlayerInputReader frozen;

        /// <summary>Typing: the menus ignore Esc (it closes the chat) and the player stands still.</summary>
        public static bool Typing { get; private set; }

        private void OnEnable() => NetworkChat.Received += Add;

        private void OnDisable()
        {
            NetworkChat.Received -= Add;
            Close();
        }

        private void Update()
        {
            MenuUi menu = MenuUi.Current;
            bool inGame = menu != null && menu.Bootstrap != null && menu.Bootstrap.IsRunning && menu.Showing == MenuScreen.None && NetworkPlayer.Local != null;
            if (!inGame && Typing) Close();
            Keyboard k = Keyboard.current;
            if (inGame && !Typing && k != null && k.tKey.wasPressedThisFrame && !Core.CursorOwner.UiActive) Open();
            else if (Typing && k != null && k.escapeKey.wasPressedThisFrame) Close();
            else if (Typing && k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) Send();
            Fade(inGame);
        }

        private bool Build()
        {
            if (root != null) return true;
            VisualElement hud = HudLayer.Root;
            if (hud == null) return false;
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("chat");
            hud.parent.Add(root); // beside the HUD, so it stays while typing
            log = MenuKit.Scroll(root, "chat__log");
            log.style.maxHeight = 260f;
            log.style.flexGrow = 0;
            field = new TextField { maxLength = NetworkChat.MaxLength };
            field.AddToClassList("field");
            field.AddToClassList("chat__field");
            root.Add(field);
            MenuKit.Show(field, false);
            return true;
        }

        private void Open()
        {
            if (!Build()) return;
            Typing = true;
            field.value = "";
            MenuKit.Show(field, true);
            root.AddToClassList("chat--open");
            // Stand still while typing (W, A, S, D are letters now).
            frozen = NetworkPlayer.Local != null ? NetworkPlayer.Local.GetComponent<PlayerInputReader>() : null;
            if (frozen != null) frozen.Override = default(PlayerInputFrame);
            Core.CursorOwner.Set(this, true);
            field.schedule.Execute(() => field.Focus()).StartingIn(16);
        }

        private void Close()
        {
            if (!Typing) return;
            Typing = false;
            if (field != null) MenuKit.Show(field, false);
            root?.RemoveFromClassList("chat--open");
            if (frozen != null) frozen.Override = null;
            frozen = null;
            Core.CursorOwner.Set(this, false);
            Core.CursorOwner.RequestCapture();
        }

        private void Send()
        {
            string text = field.value;
            Close();
            if (string.IsNullOrWhiteSpace(text) || NetworkPlayer.Local == null) return;
            NetworkPlayer.Local.GetComponent<NetworkChat>()?.Say(text);
        }

        private void Add(ulong sender, string text, bool ghost)
        {
            if (!Build()) return;
            var label = new Label { pickingMode = PickingMode.Ignore, enableRichText = true };
            label.AddToClassList("chat__line");
            // The sender's name in their coverall colour; the text as plain words (no markup from players).
            Color color = Color.white;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && p.OwnerClientId == sender && p.GetComponent<PlayerCosmetics>() is PlayerCosmetics look && look.Catalog != null
                    && look.Catalog.Coverall(look.Choice.Coverall) is CosmeticDefinition suit)
                    color = suit.Color;
            string name = $"PLAYER {sender + 1}" + (ghost ? " (GHOST)" : "");
            label.text = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.Lerp(color, Color.white, 0.25f))}><b>{name}</b></color>  <noparse>{text}</noparse>";
            log.Add(label);
            if (log is ScrollView scroll) scroll.schedule.Execute(() => scroll.ScrollTo(label)).StartingIn(16);
            lines.Add(new Line { Label = label, Time = Time.unscaledTime });
            while (lines.Count > MaxLines * 3)
            {
                lines[0].Label.RemoveFromHierarchy();
                lines.RemoveAt(0);
            }
            Audio.GameAudio.PlayUi(Audio.SoundId.UiClick, 0.4f);
        }

        // Recent lines stay a while and fade; with the chat open, the last few all show.
        private void Fade(bool inGame)
        {
            if (root == null) return;
            MenuKit.Show(root, inGame);
            for (int i = 0; i < lines.Count; i++)
            {
                Line l = lines[i];
                bool recent = i >= lines.Count - MaxLines;
                float age = Time.unscaledTime - l.Time;
                float alpha = Typing ? (recent ? 1f : 0f) : recent ? Mathf.Clamp01((LineSeconds - age) / 1.5f) : 0f;
                l.Label.style.opacity = alpha;
                l.Label.style.display = alpha > 0f ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnDestroy() => root?.RemoveFromHierarchy();
    }
}

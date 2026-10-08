using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The threat files on the HQ desk computer (same amber CRT as the supply terminal): the list of subjects
    /// on the left, the chosen file on the right. Files this player hasn't opened yet show as unknown.
    /// Esc or LOG OFF closes it. On the session object, so it needs nothing placed in a scene.
    /// </summary>
    public class ThreatFilesScreen : MonoBehaviour
    {
        private ScreenPanel screen;
        private int selected = -1;

        private void Update()
        {
            if (ThreatFilesTerminal.Open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ThreatFilesTerminal.Open = false;
            bool open = ThreatFilesTerminal.Open;
            CursorOwner.Set(this, open);
            if (screen == null || screen.Root.panel == null)
            {
                if (!open) return;
                if ((screen = ScreenPanel.Create(wide: true)) == null) return;
                screen.Panel.AddToClassList("crt");
            }
            screen.Show(open);
            if (!open) return;
            if (selected < 0) selected = FirstMet();
            screen.Build($"files|{selected}|{ThreatFiles.MetCount}", Fill);
        }

        private static int FirstMet()
        {
            for (int i = 0; i < ThreatFiles.Order.Length; i++) if (ThreatFiles.Met(ThreatFiles.Order[i])) return i;
            return 0;
        }

        private void Fill(VisualElement panel)
        {
            var area = new VisualElement();
            area.AddToClassList("crt__screen");
            panel.Add(area);
            Crt(area, $"{Company.CompanySave.DefaultName.ToUpperInvariant()} - INCIDENT FILES v1.3", "crt__title");
            Crt(area, $"FILES OPEN {ThreatFiles.MetCount}/{ThreatFiles.Order.Length}     A FILE OPENS WHEN YOU MEET ITS SUBJECT", "crt__line");
            Crt(area, new string('=', 72), "crt__rule");

            var columns = new VisualElement();
            columns.style.flexDirection = FlexDirection.Row;
            columns.style.alignItems = Align.FlexStart;
            area.Add(columns);
            var list = new VisualElement();
            list.style.width = 330f;
            list.style.flexShrink = 0;
            list.style.marginRight = 24f;
            columns.Add(list);
            var file = new VisualElement();
            file.style.flexGrow = 1;
            file.style.flexBasis = 0;
            file.style.minWidth = 0;
            columns.Add(file);

            for (int i = 0; i < ThreatFiles.Order.Length; i++)
            {
                int index = i;
                bool met = ThreatFiles.Met(ThreatFiles.Order[i]);
                var row = new Button(() =>
                {
                    GameAudio.PlayUi(SoundId.UiClick);
                    selected = index;
                });
                row.AddToClassList("crt__row");
                row.EnableInClassList("crt__row--locked", !met);
                string name = met ? ThreatFiles.For(ThreatFiles.Order[i]).Name.ToUpperInvariant() : "??????";
                Crt(row, (i == selected ? "> " : "  ") + name, "crt__name");
                list.Add(row);
            }

            ThreatKind kind = ThreatFiles.Order[Mathf.Clamp(selected, 0, ThreatFiles.Order.Length - 1)];
            if (ThreatFiles.Met(kind))
            {
                ThreatFiles.Entry entry = ThreatFiles.For(kind);
                Crt(file, entry.Name.ToUpperInvariant(), "crt__title");
                Crt(file, entry.Summary, "crt__about");
                Crt(file, "HOW IT MOVES", "crt__line").style.marginTop = 14f;
                Crt(file, entry.Movement, "crt__about");
                Crt(file, "HOW TO SURVIVE IT", "crt__line").style.marginTop = 14f;
                Crt(file, entry.Counter, "crt__about");
            }
            else
            {
                Crt(file, "NO ENCOUNTER ON FILE", "crt__title");
                Crt(file, "The company does not pay for rumours. Come back when you have seen it.", "crt__about");
            }

            Crt(area, new string('-', 72), "crt__rule");
            var logOff = new Button(() =>
            {
                GameAudio.PlayUi(SoundId.UiBack);
                ThreatFilesTerminal.Open = false;
            }) { text = "[ LOG OFF ]" };
            logOff.AddToClassList("crt__button");
            area.Add(logOff);
        }

        private static Label Crt(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ThreatFilesTerminal.Open = false;
            screen?.Show(false);
        }
    }
}

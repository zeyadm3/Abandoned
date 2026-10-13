using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// An inspectable company field guide: contents on the left, encounter notes on the right.
    /// Its worn pages retain the player's encounter unlocks; Esc or closing the book releases the cursor.
    /// </summary>
    public class ThreatFilesScreen : MonoBehaviour
    {
        private ScreenPanel screen;
        private int selected = -1;

        private void Update()
        {
            if (ThreatFilesTerminal.Open && PadInput.BackPressed)
                ThreatFilesTerminal.Open = false;
            bool open = ThreatFilesTerminal.Open;
            CursorOwner.Set(this, open);
            if (screen == null || screen.Root.panel == null)
            {
                if (!open) return;
                if ((screen = ScreenPanel.Create(wide: true)) == null) return;
                screen.Panel.AddToClassList("field-guide");
                screen.Panel.style.width = 1160f;
                screen.Panel.style.maxWidth = Length.Percent(94f);
                StyleSheet styles = Resources.Load<StyleSheet>("HqStations");
                if (styles != null) screen.Panel.styleSheets.Add(styles);
                screen.ApplyArchiveStyles();
            }
            screen.Show(open);
            if (!open) return;
            if (selected < 0) selected = FirstMet();
            screen.Build($"guide|{selected}|{ThreatFiles.MetCount}", Fill);
        }

        private static int FirstMet()
        {
            for (int i = 0; i < ThreatFiles.Order.Length; i++) if (ThreatFiles.Met(ThreatFiles.Order[i])) return i;
            return 0;
        }

        private void Fill(VisualElement panel)
        {
            Text(panel, Loc.T("THREAT ARCHIVE"), "field-guide__cover-title");
            Text(panel, Loc.T("{0} / FIELD OBSERVATIONS", Company.CompanySave.DefaultName.ToUpperInvariant()), "field-guide__cover-note");
            var columns = new VisualElement();
            columns.AddToClassList("field-guide__spread");
            panel.Add(columns);
            var list = new VisualElement();
            list.AddToClassList("field-guide__contents");
            columns.Add(list);
            var file = new VisualElement();
            file.AddToClassList("field-guide__page");
            columns.Add(file);

            Text(list, Loc.T("SUBJECT INDEX"), "field-guide__section");
            Text(list, Loc.T("{0} of {1} subjects observed", ThreatFiles.MetCount, ThreatFiles.Order.Length), "field-guide__note");
            Text(list, Loc.T("Notes appear after your first encounter. Unseen subjects stay unidentified."), "field-guide__body");

            for (int i = 0; i < ThreatFiles.Order.Length; i++)
            {
                int index = i;
                bool met = ThreatFiles.Met(ThreatFiles.Order[i]);
                var row = new Button(() =>
                {
                    GameAudio.PlayUi(SoundId.UiClick);
                    selected = index;
                });
                row.AddToClassList("field-guide__entry");
                row.EnableInClassList("field-guide__entry--locked", !met);
                row.EnableInClassList("field-guide__entry--selected", i == selected);
                string name = met ? ThreatFiles.For(ThreatFiles.Order[i]).Name : Loc.T("Unidentified subject");
                Text(row, $"{i + 1:00}  {name}", "field-guide__entry-name");
                list.Add(row);
            }

            ThreatKind kind = ThreatFiles.Order[Mathf.Clamp(selected, 0, ThreatFiles.Order.Length - 1)];
            if (ThreatFiles.Met(kind))
            {
                ThreatFiles.Entry entry = ThreatFiles.For(kind);
                Text(file, Loc.T("OBSERVATION {0:00}", selected + 1), "field-guide__note");
                Text(file, entry.Name.ToUpperInvariant(), "field-guide__title");
                Text(file, entry.Summary, "field-guide__body");
                Text(file, Loc.T("MOVEMENT & BEHAVIOUR"), "field-guide__section");
                Text(file, entry.Movement, "field-guide__body");
                Text(file, Loc.T("SURVIVAL NOTES"), "field-guide__section");
                Text(file, entry.Counter, "field-guide__body");
            }
            else
            {
                Text(file, Loc.T("OBSERVATION {0:00}", selected + 1), "field-guide__note");
                Text(file, Loc.T("IDENTITY UNKNOWN"), "field-guide__title");
                Text(file, Loc.T("No first-hand account on file. Survive an encounter to recover this record."), "field-guide__body");
                Text(file, Loc.T("NO OBSERVATION / NO SURVIVAL NOTES"), "field-guide__redacted");
            }

            var paging = MenuKit.Row(file);
            paging.AddToClassList("field-guide__paging");
            PageButton(paging, Loc.T("Previous entry"), () => selected = Mathf.Max(0, selected - 1), selected > 0);
            Text(paging, $"{selected + 1} / {ThreatFiles.Order.Length}", "field-guide__page-number");
            PageButton(paging, Loc.T("Next entry"), () => selected = Mathf.Min(ThreatFiles.Order.Length - 1, selected + 1), selected < ThreatFiles.Order.Length - 1);

            var close = new Button(() =>
            {
                GameAudio.PlayUi(SoundId.UiBack);
                ThreatFilesTerminal.Open = false;
            }) { text = Loc.T("Seal archive") };
            close.AddToClassList("field-guide__close");
            panel.Add(close);
        }

        private static void PageButton(VisualElement parent, string text, System.Action turn, bool enabled)
        {
            var button = new Button(() =>
            {
                GameAudio.PlayUi(SoundId.UiClick);
                turn();
            }) { text = text };
            button.AddToClassList("field-guide__turn");
            button.SetEnabled(enabled);
            parent.Add(button);
        }

        private static Label Text(VisualElement parent, string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            l.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(l);
            return l;
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ThreatFilesTerminal.Open = false;
            screen?.Show(false);
        }

        private void OnDestroy() => screen?.Remove();
    }
}

using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>
    /// The drive (UI step 9): when the session travels, every machine covers the cut with a dark screen, the
    /// van rumbling, where you're going (the job's terms on the way out, the HQ on the way back) and a tip,
    /// for a few seconds and until the level is in. On the session object, over everything else.
    /// </summary>
    public class TravelScreen : MonoBehaviour
    {
        private const float MinSeconds = 3.2f;

        private static readonly string[] Tips =
        {
            "Scan as soon as you're inside: prices first, plans second.", // @loc
            "Weak floors creak before they crack and crack before they go. Listen.", // @loc
            "The truck's board shows the haul. Load it, read it, argue about it.", // @loc
            "A rope and pulley over a hole beats carrying a piano down the stairs.", // @loc
            "Bolt cutters are quiet. Crowbars are not.", // @loc
            "Two people on a heavy item go at the slower one's pace. Choose your partner.", // @loc
            "The Blind One hears your voice chat. Whisper. Or don't, and run.", // @loc
            "Locked stores hold the good stock.", // @loc
        };

        private VisualElement root, progress;
        private Label where, terms, status, tip, direction;
        private int seenTravel = -1;
        private float until;
        private Audio.SoundLoop rumble;

        /// <summary>Tests: the drive is on screen.</summary>
        public bool Showing { get; private set; }

        private void OnEnable() => SessionTravel.Arrived += OnArrived;

        private void OnArrived(string level)
        {
            SessionTravel travel = SessionTravel.Current;
            if (travel == null || !travel.IsSpawned) return;
            int id = travel.TravelId;
            if (seenTravel < 0) { seenTravel = id; return; }
            if (id == seenTravel) return;
            seenTravel = id;
            Begin(level);
        }

        private void Update()
        {
            SessionTravel travel = SessionTravel.Current;
            if (travel == null || !travel.IsSpawned)
            {
                seenTravel = -1;
                Set(false);
                return;
            }
            int id = travel.TravelId;
            if (seenTravel < 0) seenTravel = id; // joining: no drive to show
            else if (id != seenTravel)
            {
                seenTravel = id;
                Begin(travel.Level);
            }
            if (Showing && (root == null || root.panel == null)) Refresh(travel.Level);
            Set(Showing && (Time.unscaledTime < until || !SessionTravel.LevelReady || !Loaded(travel.Level)));
            if (Showing && status != null)
            {
                status.text = Status(travel);
                UiKit.SetBar(progress, travel.LoadProgress);
            }
        }

        // QA B-04/B-05: what's holding the drive up, so a long wait isn't a mystery.
        private static string Status(SessionTravel travel)
        {
            if (!Loaded(travel.Level) || travel.LoadProgress < 1f) return Loc.T("LOADING  {0:P0}", travel.LoadProgress);
            string waiting = travel.WaitingFor;
            return string.IsNullOrEmpty(waiting) ? Loc.T("MANIFEST VERIFIED / ARRIVING") : Loc.T("WAITING FOR {0}", waiting.ToUpperInvariant());
        }

        private static bool Loaded(string level) => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == level;

        private void Begin(string level)
        {
            until = Time.unscaledTime + MinSeconds;
            Set(true);
            Refresh(level);
        }

        private void Refresh(string level)
        {
            if ((root == null || root.panel == null) && !Build()) return;
            bool home = level == CompanyService.HomeLevel;
            direction.text = home ? Loc.T("RETURN TRANSFER / CREW RECOVERY") : Loc.T("OUTBOUND TRANSFER / SALVAGE DETAIL");
            where.text = home ? Loc.T("HEADQUARTERS") : Location(level);
            CompanyService company = CompanyService.Current;
            if (!home && company != null && company.Active.IsValid)
            {
                Contracts.Contract c = company.Active;
                terms.text = Loc.T("{0}\nQUOTA ${1:N0}   /   STABILITY {2:P0}\nPOWER {3}   /   DEPARTURE WINDOW {4:0} MIN", Loc.T(c.ModifierName).ToUpperInvariant(), company.QuotaFor(c), c.Stability, (c.PowerOff ? Loc.T("OFF") : Loc.T("ON")), c.WindowSeconds / 60f);
            }
            else terms.text = home ? Loc.T("CREW RETURN IN PROGRESS\nPayday is on the answering machine.") : Loc.T("AWAITING DISPATCH RECORD");
            tip.text = Loc.T("FIELD NOTE / {0}", Loc.T(Tips[Random.Range(0, Tips.Length)]));
            Set(true);
        }

        private static string Location(string level) => level == Contracts.ContractGenerator.MallScene ? Contracts.ContractGenerator.MallLocation.ToUpperInvariant() : level.ToUpperInvariant();

        private bool Build()
        {
            VisualElement top = HudLayer.Root?.parent;
            if (top == null) return false;
            root?.RemoveFromHierarchy();
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(root);
            root.style.flexDirection = FlexDirection.Column;
            root.AddToClassList("travel");
            root.AddToClassList("abandoned-transfer");
            top.Add(root);
            Text(root, "ABANDONED", "transfer__brand");
            var middle = new VisualElement { pickingMode = PickingMode.Ignore };
            middle.AddToClassList("travel__middle");
            middle.AddToClassList("transfer__dossier");
            root.Add(middle);
            direction = Text(middle, "transfer__direction");
            where = Text(middle, "travel__where");
            terms = Text(middle, "travel__terms");
            progress = UiKit.Bar(middle, "transfer__progress");
            status = Text(middle, "transfer__status");
            status.enableRichText = false;
            tip = Text(root, "travel__tip");
            Text(root, Loc.T("ABANDONED SALVAGING / PERSONNEL TRANSPORT"), "transfer__footer");
            return true;
        }

        private static Label Text(VisualElement parent, string cls)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.style.whiteSpace = WhiteSpace.Normal;
            l.style.minWidth = 0f;
            l.style.flexShrink = 0;
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
        }

        private static Label Text(VisualElement parent, string value, string cls)
        {
            Label label = Text(parent, cls);
            label.text = value;
            return label;
        }

        private void Set(bool on)
        {
            Showing = on;
            Audio.MusicPlayer.SetTravel(on);
            if (root != null)
            {
                MenuKit.Show(root, on);
                if (on) root.BringToFront();
            }
            Rumble(on);
        }

        // The drive's engine bed while the screen is up (Audio: TravelEngineLoop).
        private void Rumble(bool on)
        {
            if (Application.isBatchMode) return;
            if (rumble == null)
            {
                if (!on) return;
                rumble = Audio.GameAudio.Loop(Audio.SoundId.TravelEngineLoop, transform, "TravelEngine");
            }
            rumble.Level = Mathf.MoveTowards(rumble.Level, on ? 0.25f : 0f, Time.unscaledDeltaTime * 0.6f);
        }

        private void OnDisable()
        {
            SessionTravel.Arrived -= OnArrived;
            Set(false);
            if (rumble != null) rumble.Level = 0f;
        }

        private void OnDestroy()
        {
            Audio.MusicPlayer.SetTravel(false);
            root?.RemoveFromHierarchy();
        }
    }
}

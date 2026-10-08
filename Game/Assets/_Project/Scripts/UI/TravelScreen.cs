using Abandoned.Company;
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
            "Scan as soon as you're inside: prices first, plans second.",
            "Weak floors creak before they crack and crack before they go. Listen.",
            "The truck's board shows the haul. Load it, read it, argue about it.",
            "A rope and pulley over a hole beats carrying a piano down the stairs.",
            "Bolt cutters are quiet. Crowbars are not.",
            "Two people on a heavy item go at the slower one's pace. Choose your partner.",
            "The Blind One hears your voice chat. Whisper. Or don't, and run.",
            "Locked stores hold the good stock.",
        };

        private VisualElement root;
        private Label where, terms, status, tip;
        private int seenTravel = -1;
        private float until;
        private AudioSource rumble;

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
            if (Showing && root == null) Refresh(travel.Level);
            Set(Showing && (Time.unscaledTime < until || !SessionTravel.LevelReady || !Loaded(travel.Level)));
            if (Showing && status != null) status.text = Status(travel);
        }

        // QA B-04/B-05: what's holding the drive up, so a long wait isn't a mystery.
        private static string Status(SessionTravel travel)
        {
            if (!Loaded(travel.Level) || travel.LoadProgress < 1f) return $"LOADING  {travel.LoadProgress:P0}";
            string waiting = travel.WaitingFor;
            return string.IsNullOrEmpty(waiting) ? "" : $"WAITING FOR {waiting.ToUpperInvariant()}";
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
            if (root == null && !Build()) return;
            bool home = level == CompanyService.HomeLevel;
            where.text = home ? "HEADING BACK TO THE HQ" : $"DRIVING TO: {Location(level)}";
            CompanyService company = CompanyService.Current;
            if (!home && company != null && company.Active.IsValid)
            {
                Contracts.Contract c = company.Active;
                terms.text = $"{c.ModifierName.ToUpperInvariant()}  -  QUOTA ${c.Quota:N0}  -  STABILITY {c.Stability:P0}  -  POWER {(c.PowerOff ? "OFF" : "ON")}  -  {c.WindowSeconds / 60f:0} MIN";
            }
            else terms.text = home ? "Payday's on the answering machine." : "";
            tip.text = "TIP: " + Tips[Random.Range(0, Tips.Length)];
            Set(true);
        }

        private static string Location(string level) => level == Contracts.ContractGenerator.MallScene ? Contracts.ContractGenerator.MallLocation.ToUpperInvariant() : level.ToUpperInvariant();

        private bool Build()
        {
            VisualElement top = HudLayer.Root?.parent;
            if (top == null) return false;
            root = new VisualElement { pickingMode = PickingMode.Ignore };
            UiKit.FillScreen(root);
            root.style.flexDirection = FlexDirection.Column;
            root.AddToClassList("travel");
            top.Add(root);
            UiKit.Hazard(root).AddToClassList("travel__tape");
            var middle = new VisualElement { pickingMode = PickingMode.Ignore };
            middle.AddToClassList("travel__middle");
            root.Add(middle);
            var van = new VisualElement { pickingMode = PickingMode.Ignore };
            van.AddToClassList("travel__van");
            UiKit.Icon(van, "item/transport_van", "large");
            middle.Add(van);
            where = Text(middle, "travel__where");
            terms = Text(middle, "travel__terms");
            status = Text(middle, "travel__terms");
            status.enableRichText = false;
            var road = new VisualElement { pickingMode = PickingMode.Ignore };
            road.AddToClassList("travel__road");
            middle.Add(road);
            tip = Text(root, "travel__tip");
            UiKit.Hazard(root).AddToClassList("travel__tape");
            // The van bounces along while it's up.
            van.schedule.Execute(() => van.style.translate = new Translate(0, Mathf.Sin(Time.unscaledTime * 9f) * 3f + Mathf.Sin(Time.unscaledTime * 23f) * 1f)).Every(30);
            return true;
        }

        private static Label Text(VisualElement parent, string cls)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.style.whiteSpace = WhiteSpace.Normal;
            l.style.maxWidth = Length.Percent(88f);
            l.style.flexShrink = 0;
            l.AddToClassList(cls);
            parent.Add(l);
            return l;
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

        // A low engine drone (synthesised once) while the screen is up.
        private void Rumble(bool on)
        {
            if (Application.isBatchMode) return;
            if (rumble == null)
            {
                if (!on) return;
                rumble = gameObject.AddComponent<AudioSource>();
                rumble.loop = true;
                rumble.spatialBlend = 0f;
                rumble.playOnAwake = false;
                rumble.clip = Engine();
            }
            float target = on ? 0.25f * Audio.AudioLevels.Sfx * Audio.AudioLevels.BackgroundDuck : 0f;
            rumble.volume = Mathf.MoveTowards(rumble.volume, target, Time.unscaledDeltaTime * 0.6f);
            if (rumble.volume > 0f && !rumble.isPlaying) rumble.Play();
            else if (rumble.volume <= 0f && rumble.isPlaying) rumble.Stop();
        }

        private static AudioClip Engine()
        {
            const int rate = 22050;
            const float seconds = 2f;
            var data = new float[(int)(rate * seconds)];
            var random = new System.Random(4);
            float lp = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate;
                lp += (((float)random.NextDouble() * 2f - 1f) - lp) * 0.05f;
                // 30 Hz firing (whole cycles over 2 s, so it loops) with road noise under it.
                float engine = Mathf.Sin(2f * Mathf.PI * 30f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.25f;
                data[i] = (engine * (0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 2f * t)) + lp * 2.2f) * 0.5f;
            }
            AudioClip clip = AudioClip.Create("Travel_Engine", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDisable()
        {
            SessionTravel.Arrived -= OnArrived;
            Set(false);
            if (rumble == null) return;
            rumble.Stop();
            rumble.volume = 0f;
        }

        private void OnDestroy()
        {
            Audio.MusicPlayer.SetTravel(false);
            root?.RemoveFromHierarchy();
        }
    }
}

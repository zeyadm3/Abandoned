using Abandoned.Core;
using System.Collections.Generic;
using System.Text;
using Abandoned.Networking;
using Abandoned.Threats;
using UnityEngine;
using UnityEngine.UIElements;

namespace Abandoned.Equipment
{
    /// <summary>
    /// The motion detector in hand (GDD 12, M10.5): lists what's moving within range (monsters and other
    /// players) with distance and direction, and ticks faster the closer the nearest one is. Reads the
    /// replicated positions every machine already has, so it's owner-only and local.
    /// </summary>
    public class MotionDetectorHud : MonoBehaviour
    {
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private Transform eye;
        [SerializeField, Min(2f)] private float range = 22f;
        [Tooltip("Slower than this (m/s) doesn't register: standing still hides you from it.")]
        [SerializeField, Min(0f)] private float minSpeed = 0.5f;
        [SerializeField, Min(0.05f)] private float sampleInterval = 0.25f;

        private readonly Dictionary<Transform, Vector3> last = new();
        private readonly List<(float distance, string text)> contacts = new();
        private readonly StringBuilder text = new();
        private static readonly string[] LowerPanels = { "h-inspection", "instrument-hud--stress" };
        private VisualElement panel, signal;
        private VisualElement layoutHud, lower;
        private Label readout, count;
        private float nextSample, nextTick;

        public int Contacts => contacts.Count;

        private bool Active => equipment != null && equipment.IsOwner && equipment.InHand != null && equipment.InHand.Kind == EquipmentKind.MotionDetector;

        private void Update()
        {
            if (!Active)
            {
                last.Clear();
                contacts.Clear();
                if (panel != null) UI.MenuKit.Show(panel, false);
                ReleaseLayout();
                return;
            }
            if (Time.time >= nextSample) Sample(Time.time - (nextSample - sampleInterval));
            Tick();
            Show();
        }

        private void Sample(float dt)
        {
            // Despawned monsters leave dead keys behind; start over now and then.
            if (last.Count > 64) last.Clear();
            nextSample = Time.time + sampleInterval;
            dt = Mathf.Max(dt, 0.05f);
            contacts.Clear();
            Vector3 me = transform.position;
            Transform view = eye != null ? eye : transform;
            foreach (Threat t in Threat.All)
                if (t != null) Consider(t.transform, me, view, dt);
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && !p.IsOwner && !p.IsDead) Consider(p.transform, me, view, dt);
            contacts.Sort((a, b) => a.distance.CompareTo(b.distance));
        }

        private void Consider(Transform target, Vector3 me, Transform view, float dt)
        {
            Vector3 at = target.position;
            bool moving = last.TryGetValue(target, out Vector3 before) && (at - before).magnitude / dt > minSpeed;
            last[target] = at;
            Vector3 d = at - me;
            float distance = d.magnitude;
            if (!moving || distance > range) return;
            contacts.Add((distance, $"{distance:0} M {Direction(view, d)}"));
        }

        private static string Direction(Transform view, Vector3 d)
        {
            string level = d.y > 2.5f ? Loc.T("ABOVE") : d.y < -2.5f ? Loc.T("BELOW") : "";
            Vector3 flat = Vector3.ProjectOnPlane(d, Vector3.up);
            if (flat.sqrMagnitude < 1f) return level.Length > 0 ? level : Loc.T("HERE");
            float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(view.forward, Vector3.up), flat, Vector3.up);
            string side = angle switch
            {
                > -22.5f and <= 22.5f => Loc.T("AHEAD"),
                > 22.5f and <= 67.5f => Loc.T("AHEAD-RIGHT"),
                > 67.5f and <= 112.5f => Loc.T("RIGHT"),
                > 112.5f and <= 157.5f => Loc.T("BEHIND-RIGHT"),
                < -22.5f and >= -67.5f => Loc.T("AHEAD-LEFT"),
                < -67.5f and >= -112.5f => Loc.T("LEFT"),
                < -112.5f and >= -157.5f => Loc.T("BEHIND-LEFT"),
                _ => Loc.T("BEHIND"),
            };
            return level.Length > 0 ? side + " " + level : side;
        }

        // The tick speeds up as the nearest contact closes in.
        private void Tick()
        {
            if (contacts.Count == 0 || Time.time < nextTick) return;
            float nearest = contacts[0].distance;
            nextTick = Time.time + Mathf.Lerp(0.25f, 1.4f, nearest / range);
            Audio.GameAudio.PlayUi(Audio.SoundId.DetectorBlip);
        }

        private void Show()
        {
            if (panel == null || panel.panel == null)
            {
                UI.HudLayer.Remove(panel);
                panel = UI.HudLayer.Add(new VisualElement(), "hud-panel", "hud-motion", "instrument-hud", "instrument-hud--motion");
                if (panel == null) return;
                UI.MenuKit.Text(panel, Loc.T("MOTION SENSOR"), "instrument-hud__title");
                UI.MenuKit.Text(panel, Loc.T("PASSIVE / RANGE {0:0} M", range), "instrument-hud__meta");
                signal = UI.UiKit.Segments(panel, 12, "instrument-hud__signal");
                count = UI.MenuKit.Text(panel, "", "instrument-hud__count");
                readout = UI.MenuKit.Text(panel, "", "instrument-hud__readout");
            }
            text.Clear();
            if (contacts.Count == 0) text.Append(Loc.T("NO MOVEMENT DETECTED"));
            for (int i = 0; i < contacts.Count && i < 4; i++)
            {
                if (i > 0) text.Append('\n');
                text.Append(i + 1).Append(" / ").Append(contacts[i].text);
            }
            string s = text.ToString();
            UI.MenuKit.Show(panel, true);
            panel.EnableInClassList("instrument-hud--contact", contacts.Count > 0);
            count.text = Loc.T("MOVING CONTACTS / {0:00}", contacts.Count);
            UI.UiKit.SetSegments(signal, contacts.Count > 0 ? 1f - contacts[0].distance / range : 0f);
            if (readout.text != s) readout.text = s;
            Position();
        }

        private void Position()
        {
            VisualElement hud = UI.HudLayer.Root;
            if (!Active || panel == null || !UI.HudLayer.TryLocalRect(hud, hud, out Rect area) || area.height <= 36f) return;
            VisualElement anchor = hud.Q(className: "h-inventory");
            bool found = UI.HudLayer.TryLocalRect(anchor, hud, out Rect bounds);
            foreach (string cls in LowerPanels)
            {
                VisualElement candidate = hud.Q(className: cls);
                if (!UI.HudLayer.TryLocalRect(candidate, hud, out Rect rect) || found && rect.yMin >= bounds.yMin) continue;
                anchor = candidate;
                bounds = rect;
                found = true;
            }
            BindLayout(hud, anchor);
            panel.style.left = StyleKeyword.Auto;
            panel.style.right = found ? Mathf.Max(0f, area.width - bounds.xMax) : 28f;
            panel.style.top = found ? new StyleLength(StyleKeyword.Auto) : new StyleLength(Length.Percent(48f));
            panel.style.bottom = found ? new StyleLength(Mathf.Clamp(area.height - bounds.yMin + 12f, 24f, area.height - 12f)) : new StyleLength(StyleKeyword.Auto);
        }

        private void BindLayout(VisualElement hud, VisualElement anchor)
        {
            if (layoutHud == hud && lower == anchor) return;
            ReleaseLayout();
            layoutHud = hud;
            lower = anchor;
            layoutHud?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            lower?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent _) => Position();

        private void ReleaseLayout()
        {
            layoutHud?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            lower?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            layoutHud = lower = null;
        }

        private void OnDisable()
        {
            if (panel != null) UI.MenuKit.Show(panel, false);
            ReleaseLayout();
        }

        private void OnDestroy()
        {
            ReleaseLayout();
            UI.HudLayer.Remove(panel);
        }
    }
}

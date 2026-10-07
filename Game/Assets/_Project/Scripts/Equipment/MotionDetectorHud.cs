using System.Collections.Generic;
using System.Text;
using Abandoned.Networking;
using Abandoned.Threats;
using UnityEngine;

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
        private UnityEngine.UIElements.Label panel;
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
                return;
            }
            if (Time.time >= nextSample) Sample(Time.time - (nextSample - sampleInterval));
            Tick();
            Show();
        }

        private void Sample(float dt)
        {
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
            string level = d.y > 2.5f ? " ABOVE" : d.y < -2.5f ? " BELOW" : "";
            Vector3 flat = Vector3.ProjectOnPlane(d, Vector3.up);
            if (flat.sqrMagnitude < 1f) return level.Length > 0 ? level.Trim() : "HERE";
            float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(view.forward, Vector3.up), flat, Vector3.up);
            string side = angle switch
            {
                > -22.5f and <= 22.5f => "AHEAD",
                > 22.5f and <= 67.5f => "AHEAD-RIGHT",
                > 67.5f and <= 112.5f => "RIGHT",
                > 112.5f and <= 157.5f => "BEHIND-RIGHT",
                < -22.5f and >= -67.5f => "AHEAD-LEFT",
                < -67.5f and >= -112.5f => "LEFT",
                < -112.5f and >= -157.5f => "BEHIND-LEFT",
                _ => "BEHIND",
            };
            return side + level;
        }

        // The tick speeds up as the nearest contact closes in.
        private void Tick()
        {
            if (contacts.Count == 0 || Time.time < nextTick) return;
            float nearest = contacts[0].distance;
            nextTick = Time.time + Mathf.Lerp(0.25f, 1.4f, nearest / range);
            Audio.GameAudio.PlayUi(Audio.SoundId.UiClick);
        }

        private void Show()
        {
            if (panel == null && (panel = UI.HudLayer.Label("hud-panel", "hud-motion")) == null) return;
            text.Clear().Append("MOTION");
            if (contacts.Count == 0) text.Append("\n<color=#888888>NOTHING MOVING</color>");
            for (int i = 0; i < contacts.Count && i < 4; i++) text.Append('\n').Append(contacts[i].text);
            string s = text.ToString();
            UI.MenuKit.Show(panel, true);
            if (panel.text != s) panel.text = s;
        }

        private void OnDisable()
        {
            if (panel != null) UI.MenuKit.Show(panel, false);
        }

        private void OnDestroy() => UI.HudLayer.Remove(panel);
    }
}

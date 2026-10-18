using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Threats;
using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// A lamp hanging on a chain (Iron Factory). It swings when something heavy hits nearby (a press slamming, a
    /// section coming down) and when a monster walks the catwalk right above it, so a lamp swaying in still air is
    /// a tell. Every machine swings its own lamps (cosmetic, never networked); its light swings with it, so the
    /// pool of light slides across the floor. The pivot is the top of the chain.
    /// </summary>
    public class SwingingLamp : MonoBehaviour
    {
        private const float Gravity = 9.81f;
        private const float Damping = .35f;
        private const float MaxAngle = 30f;
        private const float ThreatReach = 3.5f;
        private const float ThreatCheckSeconds = .25f;

        private static readonly List<SwingingLamp> Live = new();
        private static bool listening;

        [Tooltip("Chain length (m): longer swings slower.")]
        [SerializeField, Min(.3f)] private float chain = 1.2f;

        // Swing angles about x and z (degrees) and their speeds.
        private Vector2 angle, speed;
        private Quaternion rest;
        private float nextThreatCheck;

        private void Awake() => rest = transform.localRotation;

        private void OnEnable()
        {
            Live.Add(this);
            if (listening) return;
            listening = true;
            CameraShake.Impact += OnImpact;
            CameraShake.Collapse += OnCollapse;
        }

        private void OnDisable()
        {
            Live.Remove(this);
            if (Live.Count > 0 || !listening) return;
            listening = false;
            CameraShake.Impact -= OnImpact;
            CameraShake.Collapse -= OnCollapse;
        }

        /// <summary>Push the lamp away from a point: the harder and closer, the bigger the swing (degrees/s).</summary>
        public void Push(Vector3 from, float strength)
        {
            Vector3 away = transform.position - from;
            away.y = 0f;
            if (away.sqrMagnitude < .01f) away = Vector3.right;
            away.Normalize();
            // A push along +z tips the lamp about +x; along +x, about -z.
            speed += new Vector2(away.z, -away.x) * strength;
        }

        private static void OnImpact(Vector3 at, float momentum) => PushAll(at, momentum, 400f, 14f);
        private static void OnCollapse(Vector3 at, float momentum) => PushAll(at, momentum, 1500f, 24f);

        private static void PushAll(Vector3 at, float momentum, float reference, float reach)
        {
            float strength = Mathf.Clamp(momentum / reference, .2f, 3f) * 40f;
            foreach (SwingingLamp lamp in Live)
            {
                if (lamp == null) continue;
                float d = Vector3.Distance(lamp.transform.position, at);
                if (d < reach) lamp.Push(at, strength * (1f - d / reach));
            }
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (Time.time >= nextThreatCheck)
            {
                nextThreatCheck = Time.time + ThreatCheckSeconds;
                FeelThreats();
            }
            if (angle.sqrMagnitude < .0004f && speed.sqrMagnitude < .0004f) return;
            // A pendulum, in degrees: the pull back is g/L * sin(angle), and the air takes some each second.
            float k = Gravity / chain * Mathf.Rad2Deg;
            speed.x += (-k * Mathf.Sin(angle.x * Mathf.Deg2Rad) - Damping * speed.x) * dt;
            speed.y += (-k * Mathf.Sin(angle.y * Mathf.Deg2Rad) - Damping * speed.y) * dt;
            angle += speed * dt;
            angle.x = Mathf.Clamp(angle.x, -MaxAngle, MaxAngle);
            angle.y = Mathf.Clamp(angle.y, -MaxAngle, MaxAngle);
            transform.localRotation = rest * Quaternion.Euler(angle.x, 0f, angle.y);
        }

        // Something heavy on the walkway overhead nudges the chain as it passes.
        private void FeelThreats()
        {
            Vector3 here = transform.position;
            foreach (Threat t in Threat.All)
            {
                if (t == null) continue;
                Vector3 p = t.transform.position;
                if (p.y < here.y || p.y > here.y + 4f) continue;
                if (new Vector2(p.x - here.x, p.z - here.z).sqrMagnitude > ThreatReach * ThreatReach) continue;
                Push(p, 6f);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(float chainLength) => chain = chainLength;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Live.Clear(); listening = false; }
    }
}

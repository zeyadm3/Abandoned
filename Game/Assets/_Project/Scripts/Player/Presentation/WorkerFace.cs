using System;
using Abandoned.Voice;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// The worker's face (2026 remake, after the handover): blinks now and then, and the jaw follows speech: a crewmate's
    /// voice chat as this machine hears it, or whatever sets <see cref="Loudness"/> (the hiring's HR manager). Uses the
    /// head's Blink and JawOpen blend shapes (Tools/Blender/remake/crew/face_shapes.py); a head without them is left alone.
    /// Local only: nothing here is networked.
    /// </summary>
    public class WorkerFace : MonoBehaviour
    {
        private WorkerLook look;
        private NetworkVoice voice;
        private Mesh shown;
        private int blink = -1, jaw = -1;
        private float nextBlink, blinkStart = -1f, jawWeight;

        /// <summary>0-1 speech loudness; when unset, the crewmate's voice chat drives the jaw.</summary>
        public Func<float> Loudness { get; set; }

        private void Awake()
        {
            look = GetComponent<WorkerLook>();
            voice = GetComponentInParent<NetworkVoice>();
            nextBlink = Time.time + UnityEngine.Random.Range(1f, 4f);
        }

        private void LateUpdate()
        {
            SkinnedMeshRenderer head = look != null ? look.Head : null;
            if (head == null || head.sharedMesh == null) return;
            if (head.sharedMesh != shown)
            {
                shown = head.sharedMesh;
                blink = shown.GetBlendShapeIndex("Blink");
                jaw = shown.GetBlendShapeIndex("JawOpen");
            }
            if (blink >= 0) head.SetBlendShapeWeight(blink, Blink() * 100f);
            if (jaw >= 0)
            {
                float target = Mathf.Clamp01(Speech()) * (.55f + .45f * Mathf.Abs(Mathf.Sin(Time.time * 13f)));
                jawWeight = Mathf.Lerp(jawWeight, target, 20f * Time.deltaTime);
                head.SetBlendShapeWeight(jaw, jawWeight * 100f);
            }
        }

        // Shut over 70 ms, open over 120 ms; every 2-6 s, now and then twice.
        private float Blink()
        {
            if (blinkStart < 0f && Time.time >= nextBlink)
            {
                blinkStart = Time.time;
                nextBlink = Time.time + (UnityEngine.Random.value < .15f ? .3f : UnityEngine.Random.Range(2f, 6f));
            }
            if (blinkStart < 0f) return 0f;
            float t = Time.time - blinkStart;
            if (t >= .19f)
            {
                blinkStart = -1f;
                return 0f;
            }
            return t < .07f ? t / .07f : 1f - (t - .07f) / .12f;
        }

        private float Speech()
        {
            if (Loudness != null) return Loudness();
            if (voice == null || voice.IsOwner || !voice.IsSpeaking) return 0f;
            return voice.LastLevel * 6f;
        }
    }
}

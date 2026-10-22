using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Audio
{
    /// <summary>Local atmosphere only: none of these cues emit AI hearing events.</summary>
    public sealed class MotelSoundscape : MonoBehaviour
    {
        [SerializeField] private MotelSoundConfig config;
        [SerializeField] private Transform [] machines;
        [SerializeField] private LightFixture [] machinePower;
        [SerializeField] private Transform [] televisions;
        [SerializeField] private LightFixture [] televisionPower;
        [SerializeField] private StructuralSection [] televisionSupports;
        [SerializeField] private Transform [] gallery;
        [SerializeField] private Transform [] drips;
        private readonly List<SoundLoop> poweredLoops = new();
        private readonly List<SoundLoop> windLoops = new();
        private readonly List<SoundLoop> waterLoops = new();
        private float nextTelevision;
        private int television;
        private void Start()
        {
            if (Application.isBatchMode || config == null)
            {
                return;
            }
            for (int i = 0; i <machines.Length; i++)
            {
                SoundLoop loop = GameAudio.Loop(i == 0 ? SoundId.HumLoop : SoundId.VentDrone, machines [i], i == 0 ? "NeonTransformer" : "IceCompressor", spatial : true);
                loop.SetRange(1f, i == 0 ? 16f : 9f);
                loop.Pitch = i == 0 ? 1.15f : .82f;
                poweredLoops.Add(loop);
            }
            foreach (Transform point in gallery)
            {
                SoundLoop loop = GameAudio.Loop(SoundId.WindLoop, point, "GalleryDraft", spatial : true);
                loop.SetRange(1f, 13f);
                windLoops.Add(loop);
            }
            foreach (Transform point in drips)
            {
                SoundLoop loop = GameAudio.Loop(SoundId.WaterDrip, point, "PlumbingDrip", spatial : true);
                loop.SetRange(1f, 7f);
                waterLoops.Add(loop);
            }
            nextTelevision = Time.time + 18f;
        }
        private void Update()
        {
            if (Application.isBatchMode || config == null)
            {
                return;
            }
            for (int i = 0; i <poweredLoops.Count; i++)
            {
                bool lit = i <machinePower.Length && machinePower [i] != null && machinePower [i].Lit;
                poweredLoops [i].Level = lit ?(i == 0 ? config.NeonLevel : config.IceLevel) : 0f;
            }
            foreach (SoundLoop loop in windLoops)
            {
                loop.Level = config.WindLevel;
            }
            foreach (SoundLoop loop in waterLoops)
            {
                loop.Level = config.DripLevel;
            }
            if (Time.time <nextTelevision || televisions.Length == 0)
            {
                return;
            }
            nextTelevision = Time.time + Random.Range(config.TelevisionEvery.x, config.TelevisionEvery.y);
            int index = television++% televisions.Length;
            // A dead circuit silences the unseen set; wind and leaking pipes continue without power.
            bool floorPresent = index >= televisionSupports.Length || televisionSupports [index] == null || ! televisionSupports [index].IsCollapsed;
            if (floorPresent && index <televisionPower.Length && televisionPower [index] != null && televisionPower [index].Lit)
            {
                GameAudio.Play(SoundId.RadioStatic, televisions [index].position, config.TelevisionLevel, 13f);
            }
        }
        private void OnDisable()
        {
            Silence(poweredLoops);
            Silence(windLoops);
            Silence(waterLoops);
        }
        private static void Silence(List<SoundLoop> loops)
        {
            foreach (SoundLoop loop in loops)
            {
                loop.Level = 0f;
            }
        }
        private void OnDrawGizmos()
        {
            if (! DebugView.Visible)
            {
                return;
            }
            Draw(machines, Color.yellow);
            Draw(televisions, Color.magenta);
            Draw(gallery, Color.white);
            Draw(drips, Color.cyan);
        }
        private static void Draw(Transform [] points, Color colour)
        {
            if (points == null)
            {
                return;
            }
            Gizmos.color = colour;
            foreach (Transform point in points)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, .35f);
                }
            }
        }
        #if UNITY_EDITOR
        public void EditorSetup(MotelSoundConfig settings, Transform [] motors, LightFixture [] motorPower, Transform [] sets, LightFixture [] setPower, StructuralSection [] setSupports, Transform [] drafts, Transform [] leaks)
        {
            config = settings;
            machines = motors;
            machinePower = motorPower;
            televisions = sets;
            televisionPower = setPower;
            televisionSupports = setSupports;
            gallery = drafts;
            drips = leaks;
        }
        #endif
    }
}

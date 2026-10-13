using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A patch of broken glass, loose tiles or a puddle (0.17.3), a scene object on every machine. Each
    /// machine plays the crunch, clack or splash for every player walking through it (from the players'
    /// synced positions); the host also turns each step into noise for the monsters. Crouching is quiet.
    /// </summary>
    public class NoisyGround : MonoBehaviour
    {
        private static readonly List<NoisyGround> Live = new();

        [SerializeField] private GroundKind kind;
        [Tooltip("Half the patch's size on the floor (x, z), in its own space.")]
        [SerializeField] private Vector2 halfSize = new(0.8f, 0.8f);
        [Tooltip("The power sector over it (puddles are live while it has power, 0.17.5).")]
        [SerializeField] private int sector;

        private readonly Dictionary<NetworkPlayer, (Vector3 last, float walked)> walkers = new();
        private readonly List<NetworkPlayer> gone = new();
        private readonly Dictionary<NetworkPlayer, float> nextShock = new();
        private float nextBuzz;

        /// <summary>Water with power in its sector (0.17.5): it shocks whoever stands in it.</summary>
        public bool IsLive => kind == GroundKind.Puddle && halfSize.x > 1.2f && SectorPower.Live(sector); // floods only, not puddles

        public static IReadOnlyList<NoisyGround> All => Live;
        public GroundKind Kind => kind;
        public int Sector => sector;

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        /// <summary>Is a point (feet) standing on the patch?</summary>
        public bool Covers(Vector3 feet)
        {
            Vector3 local = transform.InverseTransformPoint(feet);
            return Mathf.Abs(local.x) <= halfSize.x && Mathf.Abs(local.z) <= halfSize.y && local.y > -0.4f && local.y < 0.6f;
        }

        private void Update()
        {
            gone.Clear();
            foreach (var pair in walkers) if (pair.Key == null || !Covers(pair.Key.transform.position)) gone.Add(pair.Key);
            foreach (NetworkPlayer p in gone) walkers.Remove(p);
            GroundConfig config = GroundConfig.Current;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || !p.IsSpawned || p.IsDead || p.IsHiding) continue;
                Vector3 feet = p.transform.position;
                if (!Covers(feet)) continue;
                if (!walkers.TryGetValue(p, out var w)) w = (feet, config.Stride); // the first step onto it sounds
                Vector3 step = feet - w.last;
                step.y = 0f;
                w.walked += step.magnitude;
                w.last = feet;
                if (w.walked >= config.Stride)
                {
                    w.walked = 0f;
                    Step(p, feet, config);
                }
                walkers[p] = w;
                if (IsLive) Shock(p, config);
            }
            // Live water buzzes and spits; you hear it before you step in it.
            if (IsLive && Time.time >= nextBuzz)
            {
                nextBuzz = Time.time + 1.1f;
                GameAudio.Play(SoundId.WaterBuzz, transform.position, 1f);
            }
        }

        // Every machine: the zap, for whoever stands in live water; the host also takes the health, again and
        // again, until they get out or the power goes.
        private void Shock(NetworkPlayer p, GroundConfig config)
        {
            if (nextShock.TryGetValue(p, out float next) && Time.time < next) return;
            nextShock[p] = Time.time + config.ShockInterval;
            GameAudio.Play(SoundId.WaterZap, p.transform.position + Vector3.up * 0.3f, 1f);
            if (GameAuthority.IsHost) p.DealDamage(config.ShockDamage * config.ShockInterval, Loc.Mark("Electrocuted in live water"));
        }

        private void Step(NetworkPlayer p, Vector3 feet, GroundConfig config)
        {
            bool crouched = p.State.Crouching;
            float share = crouched ? config.CrouchShare : 1f;
            SoundId id = kind switch { GroundKind.BrokenGlass => SoundId.GlassCrunch, GroundKind.LooseTiles => SoundId.TileClack, _ => SoundId.PuddleSplash };
            GameAudio.Play(id, feet, crouched ? 0.35f : 1f);
            if (GameAuthority.IsHost) NoiseSystem.Emit(feet, config.StepNoise(kind) * share, NoiseSource.Other);
        }

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = kind == GroundKind.Puddle ? new Color(0.2f, 0.5f, 1f, 0.7f) : new Color(1f, 0.8f, 0.3f, 0.7f);
            Gizmos.DrawWireCube(Vector3.up * 0.05f, new Vector3(halfSize.x * 2f, 0.1f, halfSize.y * 2f));
        }

#if UNITY_EDITOR
        public void EditorSetup(GroundKind groundKind, Vector2 half, int powerSector)
        {
            kind = groundKind;
            halfSize = half;
            sector = powerSector;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}

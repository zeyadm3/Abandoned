using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// A wall breaker cabinet for one power sector (0.16.1): E throws it (host-checked by
    /// <see cref="RunBreakers"/>); its handle stands up while the sector is lit and its lamp glows.
    /// Scene object on every machine; nothing here is networked itself.
    /// </summary>
    public class BreakerPanel : MonoBehaviour, IUsable, IHintSource
    {
        private static readonly List<BreakerPanel> Live = new();

        [Tooltip("The power sector it restores (HorrorConfig.SectorOf of the lights it feeds).")]
        [SerializeField, Range(0, RunBreakers.MaxSectors - 1)] private int sector;
        [SerializeField] private Transform handle;
        [OptionalReference, SerializeField] private Renderer lamp;

        private bool shownOn;
        private MaterialPropertyBlock block;

        public static IReadOnlyList<BreakerPanel> All => Live;
        public int Sector => sector;
        public HintId Hint => HintId.Breaker;

        public static BreakerPanel ForSector(int s)
        {
            foreach (BreakerPanel p in Live) if (p != null && p.sector == s) return p;
            return null;
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        private void Update()
        {
            bool on = RunBreakers.Current != null && RunBreakers.Current.Restored(sector);
            if (on == shownOn) return;
            shownOn = on;
            if (handle != null) handle.localRotation = Quaternion.Euler(on ? -40f : 40f, 0f, 0f);
            if (lamp != null)
            {
                block ??= new MaterialPropertyBlock();
                lamp.GetPropertyBlock(block);
                Color c = on ? new Color(0.3f, 1f, 0.35f) : new Color(0.9f, 0.15f, 0.1f);
                block.SetColor("_BaseColor", c);
                block.SetColor("_EmissionColor", c * 3f);
                lamp.SetPropertyBlock(block);
            }
        }

        public string UsePrompt(GameObject user)
        {
            RunBreakers run = RunBreakers.Current;
            if (run == null) return null;
            if (run.Restored(sector)) return $"Breaker on ({run.Remaining(sector):0} s left)";
            float wait = run.Cooldown(sector);
            if (wait > 0f) return $"Breaker cooling down ({wait:0} s)";
            return "Throw the breaker (lights this sector, loudly)";
        }

        public void Use(GameObject user) => RunBreakers.Current?.RequestThrow(sector);

        private void OnDrawGizmos()
        {
            if (!DebugView.Visible && Application.isPlaying) return;
            Gizmos.color = shownOn ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + transform.up * 1.4f, new Vector3(0.6f, 0.8f, 0.3f));
        }

#if UNITY_EDITOR
        public void EditorSetup(int powerSector, Transform lever, Renderer indicator)
        {
            sector = powerSector;
            handle = lever;
            lamp = indicator;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}

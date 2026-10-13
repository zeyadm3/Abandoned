using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// One end of a vent duct (0.16.4): crouch and press E to crawl through to the other end. While in the
    /// duct a player counts as hidden (most monsters lose them; nothing follows them in), the duct rattles,
    /// and nothing bigger than a pocket comes along. Scene object on every machine; the crawl itself is the
    /// player's host-written hiding value (<see cref="NetworkPlayer.InVent"/>).
    /// </summary>
    public class VentGrille : MonoBehaviour, IUsable, IHintSource
    {
        private static readonly List<VentGrille> Live = new();

        [Tooltip("The duct this grille opens: both ends share it.")]
        [SerializeField, Range(0, 26)] private int duct;
        [SerializeField, Range(0, 1)] private int end;
        [Tooltip("Where a player stands after crawling out here.")]
        [SerializeField] private Transform outPoint;

        public int Duct => duct;
        public int End => end;
        public Vector3 OutPoint => outPoint != null ? outPoint.position : transform.position + transform.forward * 0.8f;
        public HintId Hint => HintId.Vent;

        public static VentGrille Find(int ductIndex, int ductEnd)
        {
            foreach (VentGrille g in Live) if (g != null && g.duct == ductIndex && g.end == ductEnd) return g;
            return null;
        }

        /// <summary>Seconds to crawl the duct (straight-line distance between its two grilles).</summary>
        public static float CrawlSeconds(int ductIndex)
        {
            VentGrille a = Find(ductIndex, 0), b = Find(ductIndex, 1);
            VentConfig config = VentConfig.Current;
            if (a == null || b == null) return config.MinSeconds;
            return Mathf.Max(config.MinSeconds, Vector3.Distance(a.transform.position, b.transform.position) / config.CrawlSpeed);
        }

        private void OnEnable() => Live.Add(this);
        private void OnDisable() => Live.Remove(this);

        public string UsePrompt(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned || me.IsHiding || Find(duct, 1 - end) == null) return null;
            if (me.Carrier != null && me.Carrier.Held != null) return Loc.T("Too big to take through the vent");
            if (!me.State.Crouching) return Loc.T("Crouch to crawl into the vent");
            return Loc.T("Crawl into the vent");
        }

        public void Use(GameObject user)
        {
            NetworkPlayer me = user != null ? user.GetComponentInParent<NetworkPlayer>() : null;
            if (me == null || !me.IsSpawned || me.IsHiding) return;
            if (me.Carrier != null && me.Carrier.Held != null) return;
            if (!me.State.Crouching)
            {
                me.Carrier?.ShowHint(Loc.T("Crouch ({0}) to fit into the vent", InputBindings.Display("Crouch")));
                return;
            }
            me.RequestVent(duct, end);
        }

#if UNITY_EDITOR
        public void EditorSetup(int ductIndex, int ductEnd, Transform exit)
        {
            duct = ductIndex;
            end = ductEnd;
            outPoint = exit;
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Live.Clear();
    }
}

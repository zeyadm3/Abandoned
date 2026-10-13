using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// Opens a threat file the first time a monster comes near the local player (alive or already a ghost
    /// where they fell). Local and cosmetic, on the session object; checks run twice a second.
    /// </summary>
    public class ThreatFilesTracker : MonoBehaviour
    {
        private const float CheckEvery = 0.5f, NoticeDistance = 16f;

        private float nextCheck;

        private void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + CheckEvery;
            NetworkPlayer me = NetworkPlayer.Local;
            if (me == null || !me.IsSpawned) return;
            Vector3 at = me.transform.position;
            foreach (Threat t in Threat.All)
            {
                if (t == null || t.Definition == null) continue;
                if ((t.transform.position - at).sqrMagnitude > NoticeDistance * NoticeDistance) continue;
                // Names only: what it does is in the file, back at the office.
                if (ThreatFiles.MarkMet(t.Definition.Kind))
                    UI.ToastFeed.Show(Loc.T("THREAT FILE OPENED"), Loc.T("{0}: read it on the office computer at the HQ.", ThreatFiles.For(t.Definition.Kind).Name));
            }
        }
    }
}

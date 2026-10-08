using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Abandoned.UI
{
    /// <summary>The scan (Q): a sonar sweep that tags loot around you for a while.</summary>
    public partial class LootTags
    {
        // ---- The scan ----

        /// <summary>Ping: tag the loot around you (the Scan key; tests call it directly).</summary>
        public void Scan()
        {
            if (Time.time < nextScan || !Mine || player.IsDead) return;
            nextScan = Time.time + scanCooldown;
            revealUntil = Time.time + scanSeconds + 0.45f;
            revealPlayer = player;
            revealTravel = SessionTravel.Current != null ? SessionTravel.Current.TravelId : -1;
            Camera camera = Camera.main;
            Audio.GameAudio.PlayUi(Audio.SoundId.ScanPing, 0.7f);
            Ring();
            foreach (Tag t in scanTags) { Hide(t); t.Item = null; }
            if (camera == null) return;

            var found = new List<(LootItem item, float distance)>();
            Vector3 eye = camera.transform.position;
            foreach (LootItem item in FindObjectsByType<LootItem>(FindObjectsSortMode.None))
            {
                if (item == null || item.Definition == null || item.IsShattered || item.Definition.Utility) continue;
                if (item.TryGetComponent(out Grabbable g) && (g.IsPocketed || g.IsHeldBy(carrier))) continue;
                float d = Vector3.Distance(eye, item.transform.position);
                if (d > scanRange) continue;
                Vector3 v = camera.WorldToViewportPoint(item.transform.position);
                if (v.z <= 0f || v.x < -0.05f || v.x > 1.05f || v.y < -0.05f || v.y > 1.05f) continue;
                if (d > throughWalls && Physics.Linecast(eye, item.transform.position, sightMask, QueryTriggerInteraction.Ignore)) continue;
                found.Add((item, d));
            }
            found.Sort((a, b) => a.distance.CompareTo(b.distance));
            int n = Mathf.Min(found.Count, maxScanTags);
            while (scanTags.Count < n) scanTags.Add(MakeTag(scan: true));
            for (int i = 0; i < n; i++)
            {
                Tag t = scanTags[i];
                t.Item = found[i].item;
                // The ring reaches the far ones a moment later.
                t.ShowAt = Time.time + found[i].distance / scanRange * 0.45f;
                t.HideAt = t.ShowAt + scanSeconds;
                Fill(t, t.Item, full: false);
            }
            ScanCount = n;
        }

        private void UpdateScan(Camera camera)
        {
            int live = 0;
            foreach (Tag t in scanTags)
            {
                Grabbable g = t.Item != null ? t.Item.GetComponent<Grabbable>() : null;
                if (!ValuesVisible || t.Item == null || camera == null || Time.time >= t.HideAt || t.Item.IsShattered || t.Item == Focus || (g != null && g.IsPocketed))
                {
                    Hide(t);
                    if (t.Item != null && Time.time >= t.HideAt) t.Item = null;
                    continue;
                }
                if (Time.time < t.ShowAt) continue;
                live++;
                if (g != null) Place(t, g.GetBounds(), camera);
                // In on the first shown frame's next frame (so it pops), out over the last moments.
                bool wasHidden = t.Root.ClassListContains("loot-tag--hidden");
                bool fading = t.HideAt - Time.time < 0.4f;
                if (fading) t.Root.AddToClassList("loot-tag--hidden");
                else if (wasHidden) t.Root.schedule.Execute(() => t.Root.RemoveFromClassList("loot-tag--hidden")).StartingIn(16);
            }
            ScanCount = live;
        }

        private void Ring()
        {
            if (ring == null)
            {
                ring = HudLayer.Add(new VisualElement(), "scan-ring");
                if (ring == null) return;
            }
            ring.RemoveFromClassList("scan-ring--out");
            ring.style.display = DisplayStyle.Flex;
            ring.schedule.Execute(() => ring.AddToClassList("scan-ring--out")).StartingIn(16);
        }
    }
}

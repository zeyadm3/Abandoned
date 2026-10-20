using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// This machine's markers for the training guide: the lesson's target (with a blip when a new one appears and
    /// a ping when this player reaches it) and, while it is in the house, the monster in rust.
    /// </summary>
    public sealed class TutorialWaypoint
    {
        private const float ArriveRadius = 1.6f;

        private readonly TutorialMarker target = new(false), danger = new(true);
        private TutorialGuide aimedAt;
        private string label = "", dangerLabel;
        private float relabelAt;
        private bool arrived;

        public void Tick(TutorialGuide g, bool show)
        {
            Camera cam = Camera.main;
            if (!show || cam == null)
            {
                Hide();
                return;
            }
            if (g.Kind != aimedAt.Kind || g.Target != aimedAt.Target || !g.Key.Equals(aimedAt.Key) || g.Follow != aimedAt.Follow)
            {
                aimedAt = g;
                arrived = false;
                label = null;
                if (g.Kind is not (TutorialGuideKind.None or TutorialGuideKind.Wait)) GameAudio.PlayUi(SoundId.PingBlip, 0.22f);
            }
            if (TutorialGuideText.TryTarget(g, out Vector3 point))
            {
                // Worded once a second, not every frame (an item's name needs its object, which may arrive late).
                if (label == null || target.Stale || Time.unscaledTime >= relabelAt)
                {
                    label = TutorialGuideText.Label(g);
                    relabelAt = Time.unscaledTime + 1f;
                }
                target.Show(Above(g, point), label, cam, g.Kind == TutorialGuideKind.Away);
                NetworkPlayer me = NetworkPlayer.Local;
                if (!arrived && g.Kind is not (TutorialGuideKind.Away or TutorialGuideKind.Look) && me != null && Reached(me.transform.position, point))
                {
                    arrived = true;
                    GameAudio.PlayUi(SoundId.ScanPing, 0.28f);
                }
            }
            else target.Hide();
            if (TutorialGuideText.Spawned(g.Danger) is NetworkObject monster)
            {
                if (dangerLabel == null || danger.Stale) dangerLabel = Loc.T("IT CAN HEAR YOU");
                danger.Show(monster.transform.position + Vector3.up * 2.3f, dangerLabel, cam);
            }
            else danger.Hide();
        }

        public void Hide()
        {
            target.Hide();
            danger.Hide();
        }

        public void Remove()
        {
            target.Remove();
            danger.Remove();
        }

        private static bool Reached(Vector3 feet, Vector3 point)
        {
            Vector2 flat = new(feet.x - point.x, feet.z - point.z);
            return flat.magnitude <= ArriveRadius && Mathf.Abs(feet.y - point.y) < 2.2f;
        }

        // Where the marker hangs: above the thing, clear of it, at about eye height for places.
        private static Vector3 Above(TutorialGuide g, Vector3 point)
        {
            float up = g.Target switch
            {
                TutorialGuideTarget.Item => ItemTop(g),
                TutorialGuideTarget.Area => 0.6f,
                TutorialGuideTarget.Crate => 1.2f,
                TutorialGuideTarget.Door => 1.0f,
                TutorialGuideTarget.Breaker => 0.55f,
                TutorialGuideTarget.Closet => 2.0f,
                TutorialGuideTarget.Lever => 0.45f,
                TutorialGuideTarget.Truck => 1.8f,
                _ => 1.6f,
            };
            return point + Vector3.up * up;
        }

        private static float ItemTop(TutorialGuide g)
        {
            LootItem item = TutorialGuideText.Followed(g) is NetworkObject o ? o.GetComponent<LootItem>() : null;
            return item != null && item.Definition != null ? item.Definition.Size.y * 0.5f + 0.35f : 0.8f;
        }
    }
}

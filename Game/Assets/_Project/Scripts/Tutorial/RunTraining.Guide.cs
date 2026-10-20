using System.Collections.Generic;
using Abandoned.Loot;
using Abandoned.Threats;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The guide everyone's arrows and progress bar follow: the host works it out a few times a second
    /// (<see cref="TutorialGuides"/>) and writes it only when something worth showing changed.
    /// </summary>
    public partial class RunTraining
    {
        private readonly NetworkVariable<TutorialGuide> guide = new();
        private readonly TutorialGuideState guideState = new();
        private float leaveSetAt = -1f, leaveFrom = -1f;

        /// <summary>Every machine: the current lesson's target and progress.</summary>
        public TutorialGuide Guide => guide.Value;

        /// <summary>Host: the training's monster, while it is in the house.</summary>
        public Threat VisitorThreat => visitor != null && visitor.IsSpawned ? visitor : null;

        private void TickGuide()
        {
            TutorialStepDefinition s = CurrentStep;
            TutorialGuide next = s != null && !finished.Value ? TutorialGuides.Compute(this, s, guideState) : default;
            if (!next.Equals(guide.Value)) guide.Value = next;
        }

        /// <summary>Host: how far the visitor is through leaving (the wait, then the walk out), 0..1.</summary>
        public float VisitorLeaving
        {
            get
            {
                if (visitor == null || !visitor.IsSpawned) return visitorCame ? 1f : 0f;
                if (leaveAt < 0f) return 0f;
                if (Time.time < leaveAt)
                    return 0.6f * Mathf.Clamp01(1f - (leaveAt - Time.time) / Mathf.Max(1f, leaveAt - leaveSetAt));
                TutorialSpot exit = TutorialSpot.Find(leaveSpot);
                float walked = (Time.time - leaveAt) / Mathf.Max(1f, TutorialConfig.Current.ThreatLeaveTimeout);
                if (exit == null) return 0.6f + 0.4f * Mathf.Clamp01(walked);
                float d = Vector3.Distance(visitor.transform.position, exit.transform.position);
                if (leaveFrom < 0f) leaveFrom = Mathf.Max(d, 1f);
                return 0.6f + 0.4f * Mathf.Clamp01(Mathf.Max(1f - d / leaveFrom, walked));
            }
        }

        /// <summary>Host: is any of the training's salvage in hands or pockets and not yet aboard?</summary>
        public bool AnyTrainingLootInHands()
        {
            foreach (KeyValuePair<string, LootItem> pair in items)
                if (pair.Value != null && IsHeld(pair.Value) && !Aboard(pair.Value)) return true;
            return false;
        }

        /// <summary>Host: the key of the whole, unloaded training item nearest anyone (null when every one is aboard).</summary>
        public string NearestUnloadedKey()
        {
            string best = null;
            float bestDistance = float.MaxValue;
            foreach (KeyValuePair<string, LootItem> pair in items)
            {
                LootItem item = pair.Value;
                if (item == null || item.IsShattered || IsHeld(item) || Aboard(item)) continue;
                float d = TutorialGuides.NearestPlayer(this, item.transform.position, out _);
                if (d >= bestDistance) continue;
                bestDistance = d;
                best = pair.Key;
            }
            return best;
        }

        // A new lesson: its bars start from where everyone stands now.
        private void ResetGuide()
        {
            guideState.Reset();
            leaveFrom = -1f;
        }
    }
}

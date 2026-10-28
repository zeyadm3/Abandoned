using Abandoned.Extraction;
using Abandoned.Structure;
using Abandoned.Threats;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Host: the scripted moments. The visitor (one monster) comes in, wanders near the "threat_patrol" spot, and
    /// later walks out and is gone; a section armed to give way falls under the first player to stand on it; doors
    /// slide on cue. Everything else in the house stays still in training (no decay, no reroutes, no waves).
    /// </summary>
    public partial class RunTraining
    {
        private Threat visitor;
        private bool visitorCame;
        private float leaveAt = -1f;
        private string leaveSpot;
        private StructuralSection armed;
        private StructureSimulation structure;

        /// <summary>Host: the lesson's monster came in this run and has gone again (or never needed to come).</summary>
        public bool VisitorGone => visitor == null || !visitor.IsSpawned;

        private StructureSimulation Structure => structure != null ? structure : structure = FindAnyObjectByType<StructureSimulation>();

        private void StartStep(TutorialStepDefinition s)
        {
            if (s == null) return;
            CaptureBaselines(s);
            ResetGuide();
            switch (s.Beat)
            {
                case TutorialBeat.SpawnThreat:
                    SpawnVisitor(s.BeatKey);
                    break;
                case TutorialBeat.ThreatLeave:
                    leaveSpot = s.BeatKey;
                    leaveSetAt = Time.time;
                    leaveAt = Time.time + (s.BeatAmount > 0f ? s.BeatAmount : TutorialConfig.Current.ThreatLeaveAfter);
                    break;
                case TutorialBeat.ArmCollapse:
                    armed = TutorialSection.Find(s.BeatKey);
                    if (armed == null) Debug.LogWarning($"[Training] '{s.Id}' arms section '{s.BeatKey}', which the house doesn't have.");
                    break;
                case TutorialBeat.Collapse:
                    Fail(TutorialSection.Find(s.BeatKey));
                    break;
                case TutorialBeat.ShutDoor:
                case TutorialBeat.OpenDoor:
                    RunDoors.Current?.ServerSet(Mathf.RoundToInt(s.BeatAmount), s.Beat == TutorialBeat.ShutDoor);
                    break;
            }
        }

        private void TickBeats()
        {
            if (armed != null)
            {
                if (armed.IsCollapsed) armed = null;
                else if (AnyoneOn(armed))
                {
                    Fail(armed);
                    armed = null;
                }
            }
            if (visitor == null || !visitor.IsSpawned) return;
            TutorialSpot patrol = TutorialSpot.Find("threat_patrol");
            if (leaveAt < 0f || Time.time < leaveAt)
            {
                // It stays in the lesson's part of the house; it still has to hear or see someone to come for them.
                if (patrol != null) visitor.RoamHint = patrol.transform.position;
                return;
            }
            TutorialSpot exit = TutorialSpot.Find(leaveSpot);
            if (exit != null) visitor.RoamHint = exit.transform.position;
            bool outside = exit == null || Vector3.Distance(visitor.transform.position, exit.transform.position) <= TutorialConfig.Current.ThreatExitRadius;
            if (outside || Time.time >= leaveAt + TutorialConfig.Current.ThreatLeaveTimeout) DismissVisitor();
        }

        private void SpawnVisitor(string spotKey)
        {
            DismissVisitor();
            TutorialSpot spot = TutorialSpot.Find(spotKey);
            ThreatDirector director = ThreatDirector.Current;
            if (spot == null || director == null)
            {
                Debug.LogWarning($"[Training] No monster: spot '{spotKey}' {(spot == null ? "missing" : "found")}, director {(director == null ? "missing" : "found")}.");
                return;
            }
            Pose pose = spot.Pose;
            visitor = director.SpawnScripted(TutorialConfig.Current.VisitorKind, pose.position, pose.rotation);
            visitorCame = visitor != null;
            leaveAt = -1f;
        }

        private void DismissVisitor()
        {
            if (visitor != null && visitor.IsSpawned) visitor.NetworkObject.Despawn(true);
            visitor = null;
            leaveAt = -1f;
        }

        private void Fail(StructuralSection section)
        {
            if (section == null || section.IsCollapsed || Structure == null) return;
            if (!Structure.StartFailing(section)) Debug.LogWarning($"[Training] {section.name} could not be made to fall (can it collapse?).");
            else Debug.Log($"[Training] {section.name} gives way.");
        }
    }
}

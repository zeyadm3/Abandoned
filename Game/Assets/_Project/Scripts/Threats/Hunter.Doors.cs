using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>
    /// The Hunter at a shut door (0.16.0): a door stops it seeing you, but one shut between it and where it
    /// last saw you it breaks down, in a few blows the whole building hears. That wait is what shutting a
    /// door buys; the door hangs open for the rest of the job.
    /// </summary>
    public partial class Hunter
    {
        // The shut door it is breaking down, the blows landed on it, and whether it has reached it yet.
        private SlidingDoor breaching;
        private int bashes;
        private float nextBash, nextDoorLook;
        private bool atDoor;

        // Charging at this player (the one it saw last), or already at the door they went through.
        protected override bool Pursuing(NetworkPlayer p) =>
            target == p && (state.Value == HunterState.Chase || state.Value == HunterState.Breach);

        // A door shut between it and where it last saw someone: it goes to the door and breaks it down
        // (0.16.0). True while it is busy with one; the chase waits on it and doesn't time out meanwhile.
        private bool AtShutDoor()
        {
            atDoor = false;
            RunDoors doors = RunDoors.Current;
            if (doors == null || !doors.Ready)
            {
                breaching = null;
                return false;
            }
            // Someone opened it, or what it is after is no longer behind it.
            if (breaching != null && (!doors.IsShut(breaching.Index) || !breaching.Between(transform.position, lastSeen))) breaching = null;
            DoorConfig doorConfig = DoorConfig.Current;
            if (breaching == null)
            {
                if (Time.time < nextDoorLook) return false;
                nextDoorLook = Time.time + 0.2f;
                breaching = SlidingDoor.ShutBetween(transform.position, lastSeen, doorConfig.BashNoticeRange);
                if (breaching == null) return false;
                bashes = 0;
                nextBash = 0f;
            }
            lostAt = Time.time;
            if (breaching.DistanceTo(transform.position) > doorConfig.BashReach)
            {
                agent.isStopped = false;
                Steer(agent, breaching.StandPoint(transform.position, 1.2f));
                nextBash = 0f;
                return true;
            }
            atDoor = true;
            agent.isStopped = true;
            Vector3 face = breaching.Centre - transform.position;
            face.y = 0f;
            if (face.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(face), 360f * Time.deltaTime);
            // It winds up before the first blow too: that pause is the time a shut door buys.
            if (nextBash <= 0f) nextBash = Time.time + doorConfig.BashInterval;
            if (Time.time < nextBash) return true;
            nextBash = Time.time + doorConfig.BashInterval;
            bashes++;
            GetComponent<ThreatAnimationSync>()?.ServerAttack();
            if (bashes < doorConfig.BashHits) doors.ServerBash(breaching.Index);
            else
            {
                doors.ServerBreak(breaching.Index);
                breaching = null;
            }
            return true;
        }
    }
}

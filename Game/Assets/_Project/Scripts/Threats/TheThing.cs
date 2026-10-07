using Abandoned.Equipment;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>Undisclosed rule: sustained flashlight inspection wakes it; darkness eventually releases its fixation.</summary>
    public class TheThing : RoamingThreat
    {
        private readonly NetworkVariable<ThreatMotion> motion = new();
        private NetworkPlayer fixation;
        private float exposure, releaseAt;
        public override string DisplayName => "The Thing";
        public override string DeathLine => "You showed the Thing where you were.";
        public override ThreatMotion DesiredMotion => motion.Value;
        protected override void HostTick()
        {
            NetworkPlayer lit = null;
            foreach (NetworkPlayer p in NetworkPlayer.All)
                if (p != null && !p.IsDead && p.NetworkManager == NetworkManager && !Sheltered(PositionOf(p)) && Illuminates(p)) { lit = p; break; }
            if (lit != null)
            {
                if (fixation != lit) exposure = 0f;
                fixation = lit;
                exposure += Time.deltaTime * Aggression;
                releaseAt = Time.time + Interval;
            }
            else exposure = Mathf.Max(0f, exposure - Time.deltaTime * 0.25f);
            if (fixation == null || fixation.IsDead || Sheltered(PositionOf(fixation)) || Time.time > releaseAt)
            {
                fixation = null; exposure = 0f; motion.Value = ThreatMotion.Idle; Wander(); return;
            }
            if (exposure < 2.4f)
            {
                motion.Value = ThreatMotion.Special; Stop();
                Vector3 face = Vector3.ProjectOnPlane(PositionOf(fixation) - transform.position, Vector3.up);
                if (face.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(face);
                return;
            }
            motion.Value = ThreatMotion.Chase;
            Move(PositionOf(fixation), ChaseSpeed);
            if (DamageWithinReach(Reach, 70f) != null) { fixation = null; exposure = 0f; }
        }
        private bool Illuminates(NetworkPlayer p)
        {
            PlayerEquipment equipment = p.GetComponent<PlayerEquipment>();
            if (equipment == null || !equipment.State.LightOn || !equipment.Has(EquipmentKind.Flashlight) || !equipment.HandsFreeForLight) return false;
            Vector3 to = transform.position + Vector3.up * 1.4f - (PositionOf(p) + Vector3.up * 1.5f);
            Vector3 aim = Quaternion.Euler(p.State.Pitch, p.transform.eulerAngles.y, 0f) * Vector3.forward;
            return to.magnitude < SenseRange && Vector3.Angle(aim, to) < 26f && Sees(p);
        }
    }
}

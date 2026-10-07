using Abandoned.Equipment;
using Abandoned.Interaction;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Threats
{
    /// <summary>A small dark-loving swarm. A beam scatters it; reaching a carrier tears their hand loot loose.</summary>
    public class CrawlerSwarm : RoamingThreat
    {
        private readonly NetworkVariable<ThreatMotion> motion = new();
        private float scatterUntil, nextKnock;
        private Vector3 fleeFrom;
        private Light[] areaLights;
        protected override void Awake() { base.Awake(); areaLights = FindObjectsByType<Light>(FindObjectsSortMode.None); }
        public override string DisplayName => "Crawlers";
        public override string DeathLine => "The Crawlers dragged you down.";
        public override ThreatMotion DesiredMotion => motion.Value;
        protected override void HostTick()
        {
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.IsDead || p.NetworkManager != NetworkManager) continue;
                PlayerEquipment e = p.GetComponent<PlayerEquipment>();
                Vector3 to = transform.position + Vector3.up * 0.3f - (PositionOf(p) + Vector3.up * 1.5f);
                Vector3 aim = Quaternion.Euler(p.State.Pitch, p.transform.eulerAngles.y, 0f) * Vector3.forward;
                if (e != null && e.State.LightOn && e.Has(EquipmentKind.Flashlight) && e.HandsFreeForLight && to.magnitude < 16f && Vector3.Angle(to, aim) < 30f && Sees(p))
                { scatterUntil = Time.time + 2.5f; fleeFrom = PositionOf(p); }
            }
            if (Time.time < scatterUntil)
            {
                motion.Value = ThreatMotion.Special;
                Vector3 away = Vector3.ProjectOnPlane(transform.position - fleeFrom, Vector3.up).normalized;
                Move(transform.position + away * 9f, ChaseSpeed * 1.2f);
                return;
            }
            NetworkPlayer prey = Nearest(SenseRange);
            if (prey == null || BrightAt(transform.position)) { motion.Value = ThreatMotion.Idle; Wander(); return; }
            motion.Value = ThreatMotion.Chase;
            Move(PositionOf(prey), ChaseSpeed);
            if (Time.time < nextKnock) return;
            NetworkPlayer bitten = DamageWithinReach(Reach, 8f);
            if (bitten == null) return;
            nextKnock = Time.time + 4f;
            PlayerCarrier carrier = bitten.GetComponent<PlayerCarrier>();
            if (carrier != null && carrier.Held != null)
            {
                NetworkSharedCarry shared = NetworkSharedCarry.Of(carrier.Held);
                if (carrier.IsSharing && shared != null) SharedCarryServer.TryLetGo(shared, carrier);
                else
                {
                    NetworkLoot loot = carrier.Held.GetComponent<NetworkLoot>();
                    if (loot != null) LootServerActions.TryRelease(loot, carrier, (bitten.transform.forward + Vector3.up) * 2.5f, false, null);
                }
            }
        }
        private bool BrightAt(Vector3 point)
        {
            foreach (Light l in areaLights)
                if (l != null && l.enabled && l.type == LightType.Point && l.intensity > 0.8f && Vector3.Distance(l.transform.position, point) < Mathf.Min(4f, l.range * 0.4f)) return true;
            return false;
        }
    }
}

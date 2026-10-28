using Abandoned.Core;
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
        private float scatterUntil, nextKnock, lastSightAt;
        private Vector3 fleeFrom, lastKnown;
        private NetworkPlayer prey;
        private Light[] areaLights;
        protected override void Awake()
        {
            base.Awake();
            gameObject.AddComponent<ArachnophobiaSwap>(); // 0.19.7
        }
        public override string DisplayName => Loc.Mark("Crawlers");
        public override string DeathLine => Loc.Mark("The Crawlers dragged you down.");
        public override ThreatMotion DesiredMotion => motion.Value;
        // Small enough to pour in through the slats: a locker is no shelter from them (0.16.0).
        protected override bool FindsHider(NetworkPlayer p) => true;
        protected override void HostTick()
        {
            if (TickWithdrawal(Agent, WalkSpeed, prey != null, lastKnown))
            {
                prey = null;
                motion.Value = ThreatMotion.Idle;
                return;
            }
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.IsDead || p.NetworkManager != NetworkManager) continue;
                PlayerEquipment e = p.GetComponent<PlayerEquipment>();
                Vector3 to = transform.position + Vector3.up * 0.3f - (PositionOf(p) + Vector3.up * 1.5f);
                Vector3 aim = Quaternion.Euler(p.State.Pitch, p.transform.eulerAngles.y, 0f) * Vector3.forward;
                float range = Definition != null ? Definition.LightRepelRange : 16f, cone = Definition != null ? Definition.LightRepelAngle : 30f;
                if (e != null && e.LightOn && to.magnitude < range && Vector3.Angle(to, aim) < cone && Sees(p))
                {
                    scatterUntil = Time.time + (Definition != null ? Definition.ScatterSeconds : 2.5f);
                    fleeFrom = PositionOf(p);
                }
            }
            if (Time.time < scatterUntil)
            {
                prey = null;
                motion.Value = ThreatMotion.Special;
                Vector3 away = Vector3.ProjectOnPlane(transform.position - fleeFrom, Vector3.up).normalized;
                Move(transform.position + away * 9f, ChaseSpeed * 1.2f);
                return;
            }
            if (scatterUntil > 0f)
            {
                scatterUntil = 0f;
                BeginWithdrawal(Agent, fleeFrom);
                motion.Value = ThreatMotion.Idle;
                return;
            }
            NetworkPlayer seen = Nearest(SenseRange);
            if (seen != null) { prey = seen; lastKnown = SpotOf(seen); lastSightAt = Time.time; }
            if (prey != null && (prey.IsDead || !prey.IsSpawned || Sheltered(PositionOf(prey))
                || BrightAt(transform.position) || Time.time - lastSightAt >= ThreatPacingConfig.Current.LostSightSearchSeconds))
            {
                BeginWithdrawal(Agent, lastKnown);
                prey = null;
                motion.Value = ThreatMotion.Idle;
                return;
            }
            if (prey == null || BrightAt(transform.position)) { motion.Value = ThreatMotion.Idle; Wander(); return; }
            motion.Value = seen != null ? ThreatMotion.Chase : ThreatMotion.Walk;
            Move(lastKnown, seen != null ? ChaseSpeed : WalkSpeed);
            if (Time.time < nextKnock) return;
            NetworkPlayer bitten = DamageWithinReach(Reach, 8f);
            if (bitten == null) return;
            nextKnock = Time.time + (Definition != null ? Definition.AttackCooldown : 2.5f) / Mathf.Min(Aggression, 1.6f);
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
            // Host only, so clients never pay for the scene-wide search.
            areaLights ??= FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in areaLights)
                if (l != null && l.enabled && l.type == LightType.Point && l.intensity > 0.8f && Vector3.Distance(l.transform.position, point) < Mathf.Min(4f, l.range * 0.4f)) return true;
            return false;
        }
    }
}

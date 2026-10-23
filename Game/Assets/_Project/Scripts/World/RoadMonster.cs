using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// A thing on the road (host-driven, everyone sees it through its NetworkTransform). It stands in the road facing the
    /// van; when the van slows or stops near it, it runs at it, and if it reaches the van it tears at it: the van takes a
    /// knock and someone aboard is hurt, then it is gone into the trees. Hit at speed, it is thrown aside (a knock to the
    /// van too). Gone on its own once the van is far past. No NavMesh: it only ever crosses the open road.
    /// </summary>
    public class RoadMonster : NetworkBehaviour
    {
        [SerializeField] private float noticeRange = 45f, runSpeed = 9f, reach = 3.2f;
        [Tooltip("Hitting it faster than this (m/s) throws it aside; slower and it gets hold.")]
        [SerializeField] private float strikeSpeed = 9f;
        [SerializeField] private float hurt = 22f, vanKnock = 11f, clearBehind = 400f;

        private enum Mode { Waiting, Running, Done }
        private Mode mode;
        private float nextGrowl;

        private void Update()
        {
            if (!IsSpawned) return;
            if (!IsServer || mode == Mode.Done) return;
            WorldVan van = WorldVan.Current;
            if (van == null) return;
            Vector3 toVan = van.Drive.Body.ClosestPointOnBounds(transform.position) - transform.position;
            toVan.y = 0f;
            float d = toVan.magnitude, speed = van.Drive.Body.linearVelocity.magnitude;
            if (Vector3.Distance(van.transform.position, transform.position) > clearBehind) { NetworkObject.Despawn(true); return; }
            if (d < reach)
            {
                if (speed > strikeSpeed) Struck(van);
                else Attack(van);
                return;
            }
            if (mode == Mode.Waiting && d < noticeRange && speed < strikeSpeed * 1.5f) mode = Mode.Running;
            if (Time.time >= nextGrowl && d < noticeRange * 1.5f)
            {
                nextGrowl = Time.time + Random.Range(2.5f, 4.5f);
                GrowlRpc();
            }
            Vector3 dir = toVan.normalized;
            if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
            if (mode != Mode.Running) return;
            Vector3 next = transform.position + dir * runSpeed * Time.deltaTime;
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            if (Physics.Raycast(next + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 8f, mask, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<WorldVan>() == null) next.y = hit.point.y;
            transform.position = next;
        }

        // It gets hold of the van: a knock to the van and one of the crew aboard hurt.
        private void Attack(WorldVan van)
        {
            mode = Mode.Done;
            van.Condition.Knock(vanKnock);
            NetworkPlayer victim = null;
            int count = 0;
            foreach (NetworkPlayer p in NetworkPlayer.All)
            {
                if (p == null || p.IsDead || Vector3.Distance(p.transform.position, van.transform.position) > 9f) continue;
                if (Random.Range(0, ++count) == 0) victim = p;
            }
            if (victim != null) victim.DealDamage(hurt, Loc.Mark("Torn at through the van's window"));
            ScreamRpc();
            NetworkObject.Despawn(true);
        }

        private void Struck(WorldVan van)
        {
            mode = Mode.Done;
            van.Condition.Knock(vanKnock * .5f);
            ScreamRpc();
            NetworkObject.Despawn(true);
        }

        [Rpc(SendTo.Everyone)]
        private void GrowlRpc() => GameAudio.Play(SoundId.Whisper, transform.position + Vector3.up * 1.6f, 1f);

        [Rpc(SendTo.Everyone)]
        private void ScreamRpc() => GameAudio.Play(SoundId.Scream, transform.position + Vector3.up * 1.6f, 1f);
    }
}

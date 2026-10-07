using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Threats
{
    /// <summary>The host chooses the rig animation; every peer starts the same clip on server time.</summary>
    public class ThreatAnimationSync : NetworkBehaviour
    {
        [SerializeField] private Animator animator;
        private readonly NetworkVariable<ThreatMotion> motion = new();
        private readonly NetworkVariable<double> beganAt = new();
        private readonly NetworkVariable<float> playback = new(1f);
        private Threat threat;
        private NavMeshAgent agent;
        private float attackUntil;
        private ThreatMotion shown = (ThreatMotion)255;

        private void Awake() { threat = GetComponent<Threat>(); agent = GetComponent<NavMeshAgent>(); }
        public void ServerAttack() { if (IsServer) attackUntil = Time.time + 0.8f; }
        private void LateUpdate()
        {
            if (!IsSpawned || animator == null) return;
            if (IsServer)
            {
                ThreatMotion wanted = Time.time < attackUntil ? ThreatMotion.Attack : threat.DesiredMotion;
                float speed = agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
                if (wanted == ThreatMotion.Idle && speed > 0.08f) wanted = ThreatMotion.Walk;
                if (motion.Value != wanted) { beganAt.Value = NetworkManager.ServerTime.Time; motion.Value = wanted; }
                float pace = wanted == ThreatMotion.Walk || wanted == ThreatMotion.Chase ? Mathf.Clamp(speed / (wanted == ThreatMotion.Chase ? 4f : 1.8f), 0.65f, 2f) : 1f;
                if (Mathf.Abs(playback.Value - pace) > 0.15f) playback.Value = pace;
            }
            animator.speed = playback.Value;
            if (shown == motion.Value) return;
            shown = motion.Value;
            string stateName = shown.ToString();
            float clipLength = 1f;
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == stateName) { clipLength = Mathf.Max(0.01f, clip.length); break; }
            float phase = (float)(NetworkManager.ServerTime.Time - beganAt.Value) * playback.Value / clipLength;
            float normalizedTime = shown == ThreatMotion.Attack ? Mathf.Clamp(phase, 0f, 0.99f) : Mathf.Repeat(phase, 1f);
            animator.CrossFade(stateName, 0.12f, 0, normalizedTime);
        }
#if UNITY_EDITOR
        public void EditorSetup(Animator rig) => animator = rig;
#endif
    }
}

using UnityEngine;

namespace Abandoned.Threats
{
    public abstract partial class Threat
    {
        // Host: when this monster can next be dazzled (island prison lighthouse).
        private float dazeReadyAt;

        /// <summary>
        /// Host: a blinding light swept across it (the island lighthouse). It flinches away and can't strike for
        /// <paramref name="seconds"/>; then it can't be dazzled again for <paramref name="cooldown"/> more, so a
        /// light buys a moment, never safety. Returns whether it took. Its senses and pathing are untouched.
        /// </summary>
        public bool Daze(float seconds, float cooldown, Vector3 lightDirection)
        {
            if (!IsServer || !IsSpawned || Time.time < dazeReadyAt) return false;
            nextAttack = Mathf.Max(nextAttack, Time.time + seconds);
            dazeReadyAt = Time.time + seconds + cooldown;
            FlinchRpc(lightDirection.sqrMagnitude > 1e-6f ? lightDirection.normalized : -transform.forward);
            return true;
        }
    }
}

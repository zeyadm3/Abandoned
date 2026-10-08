using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>Local corpse physics and a fixed floor beam persist until revival or the next job.</summary>
    public class PlayerCorpsePresentation : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer player;
        [SerializeField] private PlayerRagdoll ragdoll;
        [SerializeField] private PlayerEquipment equipment;
        // QA B-12: the medkit measures to the owner's real body; a local copy that tumbles its own way can end up
        // metres from it. Keep the copy's pelvis within this of the replicated body position.
        private const float MaxCorpseDrift = 0.6f;

        private GameObject remoteCorpse, corpseLamp;
        private Transform remotePelvis;
        private float settleAt;
        private bool corpseSettled;

        private void OnEnable() => NetworkPlayer.DeathChanged += OnDeathChanged;

        private void OnDeathChanged(NetworkPlayer changed, bool dead)
        {
            if (changed != player) return;
            if (!dead) { Clear(); return; }
            Transform anchor = ragdoll.Head;
            if (!player.IsOwner)
            {
                remoteCorpse = Instantiate(ragdoll.RagdollRoot.gameObject, transform.position, transform.rotation);
                remoteCorpse.name = $"{player.name} corpse (local)";
                remoteCorpse.SetActive(true);
                remotePelvis = null;
                foreach (Transform part in remoteCorpse.GetComponentsInChildren<Transform>(true))
                {
                    part.gameObject.layer = GameLayers.DebrisLayer;
                    if (part.name == ragdoll.Head.name) anchor = part;
                    if (part.name == ragdoll.Pelvis.name) remotePelvis = part;
                }
                FollowRealBody();
                // A clone's sensor still references the live player, so it must never report damage.
                foreach (RagdollLandingSensor sensor in remoteCorpse.GetComponentsInChildren<RagdollLandingSensor>(true)) Destroy(sensor);
                foreach (Rigidbody part in remoteCorpse.GetComponentsInChildren<Rigidbody>(true))
                {
                    part.isKinematic = false;
                    part.linearVelocity = Vector3.zero;
                    part.angularVelocity = Vector3.zero;
                }
                ragdoll.NormalBody.SetActive(false);
                settleAt = Time.time + 12f;
                corpseSettled = false;
            }
            if (equipment == null || !equipment.LightSwitchedOn || equipment.Flashlight == null) return;
            corpseLamp = Instantiate(equipment.Flashlight.gameObject, anchor);
            corpseLamp.name = "Dropped flashlight";
            corpseLamp.transform.localPosition = new Vector3(0.18f, -0.12f, 0.15f);
            corpseLamp.transform.localRotation = Quaternion.Euler(65f, 20f, 0f);
            Light beam = corpseLamp.GetComponent<Light>();
            beam.enabled = true;
            beam.intensity = equipment.FlashlightConfig.Intensity;
        }

        // Move the whole copy so its pelvis is over the real body (owners replicate where it lies).
        private void FollowRealBody()
        {
            if (remoteCorpse == null || remotePelvis == null || !ragdoll.IsRagdolled) return;
            Vector3 drift = ragdoll.BodyPosition - remotePelvis.position;
            if (drift.sqrMagnitude <= MaxCorpseDrift * MaxCorpseDrift) return;
            remoteCorpse.transform.position += drift;
            foreach (Rigidbody part in remoteCorpse.GetComponentsInChildren<Rigidbody>())
                if (!part.isKinematic) part.linearVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        private void Update()
        {
            FollowRealBody();
            if (remoteCorpse == null || corpseSettled || Time.time < settleAt) return;
            // Cosmetic corpses stop consuming physics after settling; they never affect structural load.
            corpseSettled = true;
            foreach (Rigidbody part in remoteCorpse.GetComponentsInChildren<Rigidbody>()) part.isKinematic = true;
        }

        private void LateUpdate()
        {
            if (corpseLamp != null)
                corpseLamp.transform.rotation = Quaternion.Euler(72f, player.transform.eulerAngles.y + 20f, 0f);
        }

        private void Clear()
        {
            if (corpseLamp != null) Destroy(corpseLamp);
            if (remoteCorpse != null) Destroy(remoteCorpse);
            if (!player.IsOwner && ragdoll != null) ragdoll.NormalBody.SetActive(true);
            corpseLamp = null;
            remoteCorpse = null;
            remotePelvis = null;
        }

        private void OnDisable()
        {
            NetworkPlayer.DeathChanged -= OnDeathChanged;
            Clear();
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// The player as a structural load: body weight plus everything carried (held and pocketed),
    /// pressed down through the feet while grounded, or through the pelvis while ragdolled. Dragged
    /// items rest their own weight instead. Landings are reported as impacts.
    /// </summary>
    public class CarrierLoad : MonoBehaviour, ILoadSource
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerRagdoll ragdoll;

        private const float RagdollRestingSpeed = 1.5f;

        public float LoadWeight
        {
            get
            {
                // A tumbling ragdoll isn't resting on anything yet; a lying one is.
                bool resting = ragdoll.IsRagdolled
                    ? ragdoll.Pelvis.linearVelocity.sqrMagnitude < RagdollRestingSpeed * RagdollRestingSpeed
                    : motor.IsGrounded;
                return resting ? motor.Config.BodyWeight + carrier.CarriedWeight : 0f;
            }
        }

        public void GetLoadPoints(List<LoadPoint> points)
        {
            if (ragdoll.IsRagdolled)
            {
                points.Add(new LoadPoint(ragdoll.Pelvis.position));
                return;
            }
            // Centre plus four points around the feet, so standing on a seam loads both sections.
            Vector3 feet = transform.position;
            float r = motor.Config.Radius * 0.7f;
            points.Add(new LoadPoint(feet));
            points.Add(new LoadPoint(feet + new Vector3(r, 0f, 0f)));
            points.Add(new LoadPoint(feet + new Vector3(-r, 0f, 0f)));
            points.Add(new LoadPoint(feet + new Vector3(0f, 0f, r)));
            points.Add(new LoadPoint(feet + new Vector3(0f, 0f, -r)));
        }

        private void OnEnable()
        {
            LoadSources.Register(this);
            motor.Landed += OnLanded;
        }

        private void OnDisable()
        {
            LoadSources.Unregister(this);
            motor.Landed -= OnLanded;
        }

        // GDD 6.1: falling players are an impact source. The structure (host) decides what it does.
        private void OnLanded(float fallHeight, float impactSpeed) =>
            StructureSignals.RaiseImpact(transform.position, (motor.Config.BodyWeight + carrier.CarriedWeight) * impactSpeed);
    }
}

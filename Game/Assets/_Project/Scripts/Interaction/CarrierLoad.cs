using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// The player as a structural load: body weight plus everything carried (held and pocketed),
    /// pressed down through the feet while grounded. Dragged items rest their own weight instead.
    /// </summary>
    public class CarrierLoad : MonoBehaviour, ILoadSource
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerCarrier carrier;
        [SerializeField] private PlayerRagdoll ragdoll;

        public float LoadWeight =>
            motor.IsGrounded && !ragdoll.IsRagdolled ? motor.Config.BodyWeight + carrier.CarriedWeight : 0f;

        public void GetLoadPoints(List<LoadPoint> points)
        {
            // Centre plus four points around the feet, so standing on a seam loads both sections.
            Vector3 feet = transform.position;
            float r = motor.Config.Radius * 0.7f;
            points.Add(new LoadPoint(feet));
            points.Add(new LoadPoint(feet + new Vector3(r, 0f, 0f)));
            points.Add(new LoadPoint(feet + new Vector3(-r, 0f, 0f)));
            points.Add(new LoadPoint(feet + new Vector3(0f, 0f, r)));
            points.Add(new LoadPoint(feet + new Vector3(0f, 0f, -r)));
        }

        private void OnEnable() => LoadSources.Register(this);

        private void OnDisable() => LoadSources.Unregister(this);
    }
}

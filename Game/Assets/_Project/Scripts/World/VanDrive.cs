using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// The van's wheels and engine: built-in WheelColliders, rear-wheel drive, front steering that narrows with speed,
    /// anti-roll bars and a little downforce. Only the host simulates (<see cref="SetSimulated"/>); every other machine
    /// follows the van's synced pose and only turns its drawn wheels.
    /// </summary>
    public class VanDrive : MonoBehaviour
    {
        // Wheels in this order: front left, front right, rear left, rear right.
        [SerializeField] private Rigidbody body;
        [SerializeField] private WheelCollider[] wheels = new WheelCollider[4];
        [SerializeField] private Transform[] wheelArt = new Transform[4];
        [SerializeField] private Vector3 centreOfMass = new(0f, .9f, 1.2f);

        private VanConfig config;
        private Quaternion[] artRest;
        private float steerAngle, spin;
        private bool simulated = true;

        public Rigidbody Body => body;
        public float ForwardSpeed => Vector3.Dot(Velocity, transform.forward);
        public float SpeedKmh => Velocity.magnitude * 3.6f;
        /// <summary>Front wheel angle as a fraction of full lock.</summary>
        public float SteerFraction => config.MaxSteer > 0f ? steerAngle / config.MaxSteer : 0f;
        public bool Grounded { get; private set; }

        // Clients don't simulate the body; their speed comes from how the synced pose moves.
        private Vector3 lastPosition, followedVelocity;
        private Vector3 Velocity => simulated ? body.linearVelocity : followedVelocity;

        private void Awake()
        {
            config = VanConfig.Load();
            body.mass = config.Mass;
            body.centerOfMass = centreOfMass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            artRest = new Quaternion[wheelArt.Length];
            for (int i = 0; i < wheelArt.Length; i++) if (wheelArt[i] != null) artRest[i] = wheelArt[i].localRotation;
            lastPosition = transform.position;
        }

        /// <summary>Host: simulate the body and wheels. Clients: a kinematic shell that follows the network.</summary>
        public void SetSimulated(bool on)
        {
            simulated = on;
            // An interpolated body writes its own pose back to the transform: start it where the van was placed.
            body.position = transform.position;
            body.rotation = transform.rotation;
            lastPosition = transform.position;
            body.isKinematic = !on;
            body.interpolation = on ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
            foreach (WheelCollider w in wheels) if (w != null) w.enabled = on;
        }

        /// <summary>Host, each physics step: apply the driver's hands. <paramref name="engine"/> false = no drive at all.</summary>
        public void Step(VanDriveInput input, bool engine, float dt)
        {
            if (!simulated) return;
            float forward = ForwardSpeed, speed = body.linearVelocity.magnitude;
            float throttle = engine ? input.Throttle : 0f;
            float drive = 0f, brake = 0f;
            if (throttle > .05f)
            {
                // Rolling backwards and pressing forward: brake first, then drive.
                if (forward < -.5f) brake = config.BrakeTorque * throttle;
                else drive = config.MotorTorque * throttle * Mathf.Clamp01(1f - forward / config.TopSpeed);
            }
            else if (throttle < -.05f)
            {
                if (forward > .8f) brake = config.BrakeTorque * -throttle;
                else drive = config.MotorTorque * throttle * Mathf.Clamp01(1f + forward / config.ReverseSpeed);
            }
            else brake = config.IdleBrakeTorque;

            float lockAtSpeed = Mathf.Lerp(config.MaxSteer, config.SteerAtTopSpeed, Mathf.Clamp01(speed / config.TopSpeed));
            steerAngle = Mathf.MoveTowards(steerAngle, input.Steer * lockAtSpeed, config.SteerRate * dt);

            Grounded = false;
            for (int i = 0; i < wheels.Length; i++)
            {
                WheelCollider w = wheels[i];
                bool front = i < 2;
                if (front) w.steerAngle = steerAngle;
                w.motorTorque = front ? 0f : drive;
                w.brakeTorque = brake + (!front && input.Handbrake ? config.BrakeTorque : 0f);
                Grounded |= w.isGrounded;
            }
            AntiRoll(wheels[0], wheels[1]);
            AntiRoll(wheels[2], wheels[3]);
            if (Grounded) body.AddForce(-transform.up * config.Downforce * speed * speed);
        }

        // Pushes the more compressed side's wheel down and the other up, so the box body doesn't heel over in a bend.
        private void AntiRoll(WheelCollider left, WheelCollider right)
        {
            float travelL = 1f, travelR = 1f;
            bool groundL = left.GetGroundHit(out WheelHit hitL), groundR = right.GetGroundHit(out WheelHit hitR);
            if (groundL) travelL = (-left.transform.InverseTransformPoint(hitL.point).y - left.radius) / left.suspensionDistance;
            if (groundR) travelR = (-right.transform.InverseTransformPoint(hitR.point).y - right.radius) / right.suspensionDistance;
            float force = (travelL - travelR) * config.AntiRoll;
            if (groundL) body.AddForceAtPosition(left.transform.up * -force, left.transform.position);
            if (groundR) body.AddForceAtPosition(right.transform.up * force, right.transform.position);
        }

        /// <summary>Clients: the steering the host reports, for the drawn front wheels.</summary>
        public void ShowSteer(float fraction) => steerAngle = fraction * config.MaxSteer;

        private void LateUpdate()
        {
            if (!simulated && Time.deltaTime > 0f)
            {
                followedVelocity = (transform.position - lastPosition) / Time.deltaTime;
                lastPosition = transform.position;
            }
            // Drawn wheels: the host shows where the colliders' wheels really are; clients roll them at the van's speed.
            spin = Mathf.Repeat(spin + ForwardSpeed / .48f * Mathf.Rad2Deg * Time.deltaTime, 360f);
            for (int i = 0; i < wheelArt.Length; i++)
            {
                Transform art = wheelArt[i];
                if (art == null) continue;
                if (simulated && wheels[i] != null && wheels[i].enabled)
                {
                    wheels[i].GetWorldPose(out Vector3 position, out _);
                    art.position = position;
                }
                art.localRotation = Quaternion.Euler(0f, i < 2 ? steerAngle : 0f, 0f) * Quaternion.Euler(spin, 0f, 0f) * artRest[i];
            }
        }

        public void EditorSetup(Rigidbody rigidbody, WheelCollider[] colliders, Transform[] art, Vector3 centre)
        {
            body = rigidbody;
            wheels = colliders;
            wheelArt = art;
            centreOfMass = centre;
        }
    }
}

using UnityEngine;

namespace Abandoned.Interaction
{
    /// <summary>
    /// One physics step of a shared carry, on the machine that simulates the item (the host). Each
    /// carrier's point is pulled toward that carrier's desired hold position: the average pull moves
    /// the item, the differences turn it (yaw when they walk around each other, tilt when one holds
    /// higher) and make it rock when they pull in different directions. Accelerations, not forces,
    /// so it ignores the clamped Rigidbody mass (CLAUDE.md: gameplay weight is separate).
    /// </summary>
    public static class SharedCarryForces
    {
        private static readonly Vector3[] Points = new Vector3[SharedCarryable.MaxPoints];
        private static readonly Vector3[] Pulls = new Vector3[SharedCarryable.MaxPoints];

        public static void Step(SharedCarryable item, Rigidbody body, float dt, float time)
        {
            SharedCarryConfig cfg = item.Config;
            bool lifted = item.IsLifted;
            // A full crew holds it up; with too few it's dragged and falls if it's over nothing.
            body.useGravity = !lifted;

            int n = 0;
            Vector3 meanPull = Vector3.zero, centroid = Vector3.zero;
            for (int i = 0; i < item.PointCount; i++)
            {
                if (item.CarrierAt(i) == null) continue;
                Vector3 point = item.PointWorld(i);
                Vector3 pull = item.TargetFor(i) - point;
                if (!lifted) pull.y = 0f;
                item.LastPull[i] = pull;
                Points[n] = point;
                Pulls[n] = pull;
                meanPull += pull;
                centroid += point;
                n++;
            }
            if (n == 0) return;
            meanPull /= n;
            centroid /= n;

            Vector3 v = body.linearVelocity;
            Vector3 accel;
            if (lifted)
            {
                accel = Vector3.ClampMagnitude(meanPull * cfg.Spring - v * cfg.Damping, cfg.MaxAcceleration);
            }
            else
            {
                accel = meanPull * cfg.DragSpring - new Vector3(v.x, 0f, v.z) * cfg.DragDamping;
                accel = Vector3.ClampMagnitude(new Vector3(accel.x, 0f, accel.z), cfg.DragMaxAcceleration);
                if (item.IsNudgeOnly)
                {
                    // Out of nudge budget: nothing pulls it further away, it only slides back or around.
                    Vector3 offset = body.position - item.NudgeOrigin;
                    accel = WithoutOutward(accel, offset, cfg.NudgeRadius);
                    body.linearVelocity = WithoutOutward(v, offset, cfg.NudgeRadius);
                }
            }
            if (lifted) body.AddForce(accel, ForceMode.Acceleration);
            // Dragged: pull at floor level, where friction acts, so a tall rack slides instead of tipping over.
            else body.AddForceAtPosition(accel * body.mass, FloorPoint(item, body), ForceMode.Force);
            item.LastAcceleration = accel;
            CapHorizontalSpeed(body, lifted ? item.GroupMaxSpeed * cfg.SpeedSlack : item.CarrierMaxSpeed * cfg.SpeedSlack);

            body.angularVelocity = Vector3.Lerp(body.angularVelocity, TargetSpin(item, body, n, meanPull, centroid, lifted, time),
                1f - Mathf.Exp(-(lifted ? cfg.AngularResponse : cfg.DragAngularResponse) * dt));
        }

        /// <summary>Removes the part of <paramref name="vector"/> that leads further out once the horizontal offset reaches the radius.</summary>
        public static Vector3 WithoutOutward(Vector3 vector, Vector3 offset, float radius)
        {
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance < radius || distance < 1e-4f) return vector;
            Vector3 outward = offset / distance;
            float along = Vector3.Dot(vector, outward);
            return along > 0f ? vector - outward * along : vector;
        }

        private static Vector3 FloorPoint(SharedCarryable item, Rigidbody body)
        {
            Vector3 com = body.worldCenterOfMass;
            return new Vector3(com.x, item.Grabbable.GetBounds().min.y + 0.05f, com.z);
        }

        private static Vector3 TargetSpin(SharedCarryable item, Rigidbody body, int n, Vector3 meanPull, Vector3 centroid,
            bool lifted, float time)
        {
            SharedCarryConfig cfg = item.Config;
            Vector3 spin = Vector3.zero;
            float strain = 0f;
            Vector3 carrierLine = Vector3.zero;
            if (n >= 2)
            {
                // Small-angle best fit: the rotation that best reduces each point's pull beyond the shared one.
                Vector3 torque = Vector3.zero;
                float inertia = 0f;
                for (int k = 0; k < n; k++)
                {
                    Vector3 r = Points[k] - centroid;
                    Vector3 relative = Pulls[k] - meanPull;
                    torque += Vector3.Cross(r, relative);
                    inertia += r.sqrMagnitude;
                    strain += new Vector3(relative.x, 0f, relative.z).magnitude;
                }
                spin = torque / Mathf.Max(inertia, 1e-3f) * cfg.TurnGain;
                strain /= n;
                carrierLine = Vector3.ProjectOnPlane(Points[0] - centroid, Vector3.up).normalized;
            }

            Vector3 up = body.rotation * Vector3.up;
            spin += Vector3.Cross(up, Vector3.up) * (lifted ? cfg.UprightGain : cfg.DragUprightGain);
            // Carriers pulling apart or sideways: the load rocks about the line between them.
            if (lifted && n >= 2) spin += carrierLine * (strain * cfg.WobbleGain * Mathf.Sin(2f * Mathf.PI * cfg.WobbleFrequency * time));
            return spin;
        }

        private static void CapHorizontalSpeed(Rigidbody body, float cap)
        {
            if (float.IsInfinity(cap)) return;
            Vector3 v = body.linearVelocity;
            var flat = new Vector3(v.x, 0f, v.z);
            if (flat.magnitude <= cap) return;
            flat = flat.normalized * cap;
            body.linearVelocity = new Vector3(flat.x, v.y, flat.z);
        }
    }
}

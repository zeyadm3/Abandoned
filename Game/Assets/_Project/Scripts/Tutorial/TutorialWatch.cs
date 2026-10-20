using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// This machine's own player, measured for the personal lessons (how far they turned and walked, how long they
    /// sprinted and crouched, a jump, a scan, the pockets, a throw), counted from the start of the lesson. When the
    /// lesson's amount is reached it tells the host once. Local only; ticked every frame by <see cref="TutorialHud"/>.
    /// </summary>
    public sealed class TutorialWatch
    {
        private const float ThrowWindow = 0.35f, PocketsHold = 0.4f;

        private int watching = -1;
        private bool reported;
        private float turned, walked, sprinted, crouched, pockets, lastUseWhileHolding = -10f;
        private bool jumped, scanned, threw;
        private float lastYaw;
        private Vector3 lastPosition;
        private Grabbable held;

        public void Tick(RunTraining training)
        {
            NetworkPlayer me = NetworkPlayer.Local;
            if (training == null || me == null || !me.IsSpawned || me.IsDead) return;
            TutorialStepDefinition step = training.CurrentStep;
            if (training.Step != watching) Begin(training.Step, me);
            if (step == null || !step.Personal || reported) return;
            Measure(me, step.Check == TutorialCheck.Throw && step.Amount > 0f ? step.Amount : 3f);
            if (!Met(step)) return;
            reported = true;
            training.ReportDone(training.Step);
        }

        private void Begin(int step, NetworkPlayer me)
        {
            watching = step;
            reported = false;
            turned = walked = sprinted = crouched = pockets = 0f;
            jumped = scanned = threw = false;
            lastYaw = me.transform.eulerAngles.y;
            lastPosition = me.transform.position;
            held = me.Carrier != null ? me.Carrier.Held : null;
        }

        private void Measure(NetworkPlayer me, float throwSpeed)
        {
            float dt = Time.deltaTime;
            float yaw = me.transform.eulerAngles.y;
            turned += Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw));
            lastYaw = yaw;
            Vector3 at = me.transform.position;
            Vector3 flat = new(at.x - lastPosition.x, 0f, at.z - lastPosition.z);
            // A teleport (a chapter reset) is not a walk.
            if (flat.magnitude < 2f) walked += flat.magnitude;
            lastPosition = at;
            PlayerMotor motor = me.Motor;
            if (motor != null)
            {
                if (motor.IsSprinting && motor.HorizontalSpeed > 0.5f) sprinted += dt;
                if (motor.IsCrouching) crouched += dt;
                if (motor.IsAirborne && motor.IsGrounded == false && motor.Velocity.y > 0.5f) jumped = true;
            }
            PlayerInputReader input = me.GetComponent<PlayerInputReader>();
            if (input != null)
            {
                PlayerInputFrame frame = input.Current;
                pockets = input.InventoryOpen ? pockets + dt : 0f;
                if (frame.JumpPressed) jumped = true;
                if (frame.UsePressed && me.Carrier != null && me.Carrier.Held != null) lastUseWhileHolding = Time.time;
            }
            scanned |= LootTags.ValuesVisible;
            WatchThrow(me, throwSpeed);
        }

        // Let go of something with the use button, or watch it fly off fast.
        private void WatchThrow(NetworkPlayer me, float throwSpeed)
        {
            Grabbable now = me.Carrier != null ? me.Carrier.Held : null;
            if (now == held) return;
            Grabbable released = held;
            held = now;
            if (released == null || now != null) return;
            if (Time.time - lastUseWhileHolding <= ThrowWindow || released.Velocity.magnitude >= throwSpeed) threw = true;
        }

        /// <summary>This player's progress through the current personal lesson, 0..1, and the numbers behind it.</summary>
        public float Progress(TutorialStepDefinition s, out TutorialReadout readout, out float value, out float goal)
        {
            readout = TutorialReadout.None;
            value = goal = 0f;
            if (s == null || !s.Personal) return 0f;
            if (reported) return 1f;
            switch (s.Check)
            {
                case TutorialCheck.Look: return Measured(TutorialReadout.Degrees, turned, s.Amount > 0f ? s.Amount : 120f, out readout, out value, out goal);
                case TutorialCheck.Walk: return Measured(TutorialReadout.Metres, walked, s.Amount > 0f ? s.Amount : 4f, out readout, out value, out goal);
                case TutorialCheck.Sprint: return Measured(TutorialReadout.Seconds, sprinted, s.Amount > 0f ? s.Amount : 1.5f, out readout, out value, out goal);
                case TutorialCheck.Crouch: return Measured(TutorialReadout.Seconds, crouched, s.Amount > 0f ? s.Amount : 1f, out readout, out value, out goal);
                case TutorialCheck.Pockets: return Mathf.Clamp01(pockets / PocketsHold);
                case TutorialCheck.Jump: return jumped ? 1f : 0f;
                case TutorialCheck.Scan: return scanned ? 1f : 0f;
                // Something in hand is half of a throw.
                case TutorialCheck.Throw: return threw ? 1f : held != null ? 0.5f : 0f;
                default: return 0f;
            }
        }

        private static float Measured(TutorialReadout kind, float done, float need, out TutorialReadout readout, out float value, out float goal)
        {
            readout = kind;
            value = Mathf.Min(done, need);
            goal = need;
            return need > 0f ? Mathf.Clamp01(done / need) : 1f;
        }

        private bool Met(TutorialStepDefinition s)
        {
            switch (s.Check)
            {
                case TutorialCheck.Look: return turned >= (s.Amount > 0f ? s.Amount : 120f);
                case TutorialCheck.Walk: return walked >= (s.Amount > 0f ? s.Amount : 4f);
                case TutorialCheck.Sprint: return sprinted >= (s.Amount > 0f ? s.Amount : 1.5f);
                case TutorialCheck.Crouch: return crouched >= (s.Amount > 0f ? s.Amount : 1f);
                case TutorialCheck.Jump: return jumped;
                case TutorialCheck.Scan: return scanned;
                case TutorialCheck.Pockets: return pockets >= PocketsHold;
                case TutorialCheck.Throw: return threw;
                default: return false;
            }
        }
    }
}

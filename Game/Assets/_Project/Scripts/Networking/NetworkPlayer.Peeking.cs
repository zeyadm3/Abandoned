using Abandoned.Extraction;
using Abandoned.Player;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    public partial class NetworkPlayer
    {
        private readonly NetworkVariable<float> hidePeek = new();
        private Vector3 hidingEyeRest;
        private float nextPeekReport, sentPeek;
        public float Peek01 => hidePeek.Value;
        public bool IsConcealed => IsHiding && (InVent || Peek01 < .08f &&
            (HidingLocker.ByIndex(HidingIn) is HidingLocker spot && spot.ConcealsOccupant));
        public Vector3 SightPoint
        {
            get
            {
                Vector3 eye = look != null ? look.EyeRoot.position : transform.position + Vector3.up * 1.2f;
                HidingLocker spot = IsHiding && !InVent ? HidingLocker.ByIndex(HidingIn) : null;
                if (spot == null || IsOwner) return eye;
                float side = Mathf.Clamp(Mathf.DeltaAngle(spot.Facing.eulerAngles.y, transform.eulerAngles.y) / DoorConfig.Current.HideLookAngle, -1f, 1f);
                return eye + transform.TransformVector(new Vector3(side * DoorConfig.Current.PeekDistance, .03f * Peek01, Peek01 * DoorConfig.Current.PeekDistance));
            }
        }

        private void TickOwnerPeek(PlayerInputFrame input)
        {
            if (look == null || hiddenIn == null) return;
            DoorConfig config = DoorConfig.Current;
            float centre = hiddenIn.Facing.eulerAngles.y;
            if (!input.RotateHeld) look.ReturnHidingLook(centre, config.PeekReturnSpeed * Time.deltaTime);
            float side = Mathf.Clamp(Mathf.DeltaAngle(centre, transform.eulerAngles.y) / config.HideLookAngle, -1f, 1f);
            float amount = Mathf.Clamp01(Mathf.Abs(side) + Mathf.Abs(look.Pitch) / 70f);
            look.EyeRoot.localPosition = hidingEyeRest + new Vector3(side * config.PeekDistance, .03f * amount, amount * config.PeekDistance);
            if (Time.time < nextPeekReport || Mathf.Abs(amount - sentPeek) < .01f) return;
            nextPeekReport = Time.time + .1f;
            sentPeek = amount;
            SetPeekRpc(amount);
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SetPeekRpc(float amount)
        {
            if (!IsHiding || InVent || !float.IsFinite(amount)) return;
            hidePeek.Value = Mathf.Clamp01(amount);
        }
    }
}

using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Networking
{
    // Developer tool (user, 2026-10-09): hold Shift + Control + Option (Alt) + Command (Windows key) together to
    // hurt yourself, for testing the vitals bar, the low-health feedback and death. Modifiers only, so nothing
    // in the game or the editor is bound to it by accident. Editor and Development builds only (Core.DevTools):
    // a shareable build never reads the keys, and a shareable host ignores the request from a dev client.
    public partial class NetworkPlayer
    {
        private const float DevHurtStep = 5f, DevHurtRepeat = 0.5f;
        private const string DevHurtCause = "Developer key";

        private float nextDevHurt;

        // Owner: one step at once when the keys go down, then one every half second while they stay down.
        private void TickDevHurt()
        {
            if (!DevTools.Enabled || IsDead) return;
            Keyboard keys = Keyboard.current;
            bool held = keys != null && keys.shiftKey.isPressed && keys.ctrlKey.isPressed && keys.altKey.isPressed
                        && (keys.leftMetaKey.isPressed || keys.rightMetaKey.isPressed);
            if (!held)
            {
                nextDevHurt = 0f;
                return;
            }
            if (Time.unscaledTime < nextDevHurt) return;
            nextDevHurt = Time.unscaledTime + DevHurtRepeat;
            if (IsServer) DealDamage(DevHurtStep, DevHurtCause);
            else DevHurtRpc();
        }

        // No amount crosses the wire: damage stays the host's to decide (see DealDamage), even for this.
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void DevHurtRpc()
        {
            if (DevTools.Enabled) DealDamage(DevHurtStep, DevHurtCause);
        }
    }
}

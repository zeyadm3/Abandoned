using Abandoned.Core;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>
    /// Dev only: holds the van's controls for a while without a keyboard (editor checks driven from scripts, nettests).
    /// Runs after the seats so its hands win over the seated player's own (idle) input that frame.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VanTestDriver : MonoBehaviour
    {
        public float Throttle, Steer, Seconds;
        public bool Handbrake;

        /// <summary>A test driver is holding the controls (the seated player's own hands stay off them).</summary>
        public static bool Driving { get; private set; }

        private void LateUpdate()
        {
            Driving = DevTools.Enabled && Seconds > 0f && WorldVan.Current != null;
            if (!Driving) return;
            Seconds -= Time.deltaTime;
            WorldVan.Current.SendDrive(new VanDriveInput(Throttle, Steer, Handbrake), false);
        }

        /// <summary>Drive like this for <paramref name="seconds"/> (adds the driver to the scene if needed).</summary>
        public static VanTestDriver Hold(float throttle, float steer, float seconds, bool handbrake = false)
        {
            VanTestDriver d = FindAnyObjectByType<VanTestDriver>() ?? new GameObject("VanTestDriver").AddComponent<VanTestDriver>();
            d.Throttle = throttle;
            d.Steer = steer;
            d.Seconds = seconds;
            d.Handbrake = handbrake;
            return d;
        }
    }
}

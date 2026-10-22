using Unity.Netcode;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>One frame of the driver's hands: throttle (back = brake, then reverse), steering and the handbrake.</summary>
    public struct VanDriveInput : INetworkSerializable
    {
        private sbyte throttle, steer;
        private bool handbrake;

        public VanDriveInput(float throttle, float steer, bool handbrake)
        {
            this.throttle = (sbyte)Mathf.RoundToInt(Mathf.Clamp(throttle, -1f, 1f) * 127f);
            this.steer = (sbyte)Mathf.RoundToInt(Mathf.Clamp(steer, -1f, 1f) * 127f);
            this.handbrake = handbrake;
        }

        /// <summary>Nobody at the wheel: no throttle, handbrake on.</summary>
        public static VanDriveInput Parked => new(0f, 0f, true);

        public float Throttle => throttle / 127f;
        public float Steer => steer / 127f;
        public bool Handbrake => handbrake;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref throttle);
            serializer.SerializeValue(ref steer);
            serializer.SerializeValue(ref handbrake);
        }
    }
}

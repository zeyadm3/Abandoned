using System;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Equipment
{
    public struct DroppedToolState : INetworkSerializable, IEquatable<DroppedToolState>
    {
        public int Id, CatalogIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref CatalogIndex);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
        }
        public bool Equals(DroppedToolState other) => Id == other.Id && CatalogIndex == other.CatalogIndex && Position == other.Position && Rotation == other.Rotation;
    }
}

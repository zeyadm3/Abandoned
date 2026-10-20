using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// Where the current lesson wants the crew and how far along it is, written by the host a few times a second
    /// (<see cref="TutorialGuides"/>, <see cref="TutorialMeter"/>) so every machine draws the same arrows, marker
    /// and progress bar. Personal lessons (look, sprint) fill their bar on each player's own machine instead.
    /// </summary>
    public struct TutorialGuide : INetworkSerializable, IEquatable<TutorialGuide>
    {
        public TutorialGuideKind Kind;
        public TutorialGuideTarget Target;
        /// <summary>The target's key: an area, item, section or equipment id, or a door or closet number.</summary>
        public FixedString32Bytes Key;
        /// <summary>The network object to follow (a salvage item that moves), 0 = none.</summary>
        public ulong Follow;
        /// <summary>The training's monster while it is in the house, 0 = none: everyone sees where it is.</summary>
        public ulong Danger;
        /// <summary>The target (the followed object's last known place when there is one).</summary>
        public Vector3 Point;
        /// <summary>A first stop on the way (the foot of the truck's ramp), when <see cref="HasVia"/>.</summary>
        public Vector3 Via;
        public bool HasVia;
        /// <summary>The lesson's progress, 0..1000.</summary>
        public ushort Progress;
        public TutorialReadout Readout;
        public float Value, Goal;

        public float Fraction => Progress / 1000f;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Kind);
            s.SerializeValue(ref Target);
            s.SerializeValue(ref Key);
            s.SerializeValue(ref Follow);
            s.SerializeValue(ref Danger);
            s.SerializeValue(ref Point);
            s.SerializeValue(ref Via);
            s.SerializeValue(ref HasVia);
            s.SerializeValue(ref Progress);
            s.SerializeValue(ref Readout);
            s.SerializeValue(ref Value);
            s.SerializeValue(ref Goal);
        }

        // The host rounds before writing (TutorialGuides.Quantise), so equal means "nothing worth sending".
        public bool Equals(TutorialGuide o) =>
            Kind == o.Kind && Target == o.Target && Key.Equals(o.Key) && Follow == o.Follow && Danger == o.Danger &&
            Point == o.Point && Via == o.Via && HasVia == o.HasVia && Progress == o.Progress && Readout == o.Readout &&
            Value == o.Value && Goal == o.Goal;

        public override bool Equals(object obj) => obj is TutorialGuide other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Kind, Target, Follow, Point, Progress);
    }
}

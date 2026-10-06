using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// How fast something is really moving. A networked copy that only follows the network is kinematic
    /// and moved by setting its transform, so its Rigidbody reports zero velocity; this gives the motion
    /// other machines' players should be hit by.
    /// </summary>
    public interface IVelocitySource
    {
        Vector3 Velocity { get; }
    }
}

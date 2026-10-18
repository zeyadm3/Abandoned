using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Marks lights the building's power doesn't run (moonlight through a hole in the roof, molten iron's glow):
    /// the power controller leaves every light under this alone, so a power-off contract or a blackout never
    /// switches them off.
    /// </summary>
    public class PowerIndependentLight : MonoBehaviour
    {
    }
}

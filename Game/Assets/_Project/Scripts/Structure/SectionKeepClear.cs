using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>
    /// Marks a section on the permanent haul / escape route (protected corridors, service landings). Solid
    /// rubble never lands on it: a section falling onto it leaves only the loose, walk-through chunks, so the
    /// way out can't be walled off. Added by the mall builder.
    /// </summary>
    [DisallowMultipleComponent]
    public class SectionKeepClear : MonoBehaviour
    {
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Kept on a level's "_MapSite" object (Editor/Art/MapSite): the level's own pieces the site pass switched off
    /// because it put something better in their place (a code-built tree swapped for a scanned one). Running the pass
    /// again switches them back on first, so it can always be redone or undone. Does nothing at run time.
    /// </summary>
    public class MapSiteRecord : MonoBehaviour
    {
        [SerializeField] private List<GameObject> replaced = new();

        public IReadOnlyList<GameObject> Replaced => replaced;

#if UNITY_EDITOR
        public void EditorAdd(GameObject original) => replaced.Add(original);
#endif
    }
}

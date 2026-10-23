using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The open world on its own top-level button, beside Rebuild New Maps (user, 2026-10-23: "one button to rebuild the 5
    /// new maps and a different button to rebuild the world"). Reads every map's saved scene (its ground, van spot, sky
    /// and fog), so run it after rebuilding any map the world shows. Same step as Rebuild All Maps' last one.
    /// </summary>
    public static class RebuildWorld
    {
        private const string Title = "Rebuild World";

        [MenuItem(ToolMenu.Root + Title, priority = ToolMenu.RebuildWorldP)]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(Title, "Leave Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog(Title,
                    "Rebuilds the open world (the land, the roads, every place's site and how it looks from the road) from the " +
                    "maps' saved scenes. No map is rebuilt: run Rebuild New Maps first if you changed one of them.\n\n" +
                    "This takes a while and Unity will look frozen.", "Rebuild", "Cancel"))
                return;
            RebuildAllMaps.RunSteps(Title, new List<(string name, Action run)> { RebuildAllMaps.OpenWorldStep });
        }
    }
}

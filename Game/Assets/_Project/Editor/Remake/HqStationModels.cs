using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// The HQ's working stations get remade models (Tools/Blender/remake/hq/hq_stations.py): the crew-issue gear rack
    /// and the supply shop terminal. Their colliders and scripts stay; only their drawing changes, recorded in the
    /// scene's <see cref="KitSwapRecord"/> like the map kit, and redone whenever <see cref="KitSwap.Apply"/> runs on the
    /// HQ. The terminal keeps its amber screen (TerminalFace/AmberCRT), framed by the new housing.
    /// </summary>
    public static class HqStationModels
    {
        public const string Folder = RemakePaths.Maps + "/HQ";

        public static int Dress(Scene scene, KitSwapRecord record)
        {
            GameObject rackModel = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/Hq_gear_rack.fbx");
            GameObject terminalModel = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/Hq_shop_terminal.fbx");
            if (rackModel == null || terminalModel == null) return 0;
            int done = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform rack = Find(root.transform, "GearRack");
                if (rack != null && rack.GetComponent<Abandoned.Equipment.GearRack>() != null)
                {
                    KitSwap.Replace(record, Drawn(rack), rackModel, rack, "RM_GearRack");
                    done++;
                }
                Transform terminal = Find(root.transform, "ShopTerminal");
                Transform face = terminal != null && terminal.parent != null ? terminal.parent.Find("TerminalFace") : null;
                if (terminal != null && face != null)
                {
                    var drawn = new List<MeshRenderer>();
                    if (terminal.TryGetComponent(out MeshRenderer own)) drawn.Add(own);
                    drawn.AddRange(Drawn(face).Where(r => r.name != "AmberCRT"));
                    KitSwap.Replace(record, drawn, terminalModel, face, "RM_ShopTerminal");
                    done++;
                }
            }
            return done;
        }

        // Everything the station draws, except lettering (TextMesh) and anything the remake already put there.
        private static List<MeshRenderer> Drawn(Transform t) =>
            t.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r => r.GetComponent<TextMesh>() == null && r.GetComponent<MeshFilter>() != null && !r.transform.name.StartsWith("RM_")
                            && r.GetComponentInParent<LODGroup>() == null)
                .ToList();

        private static Transform Find(Transform at, string name)
        {
            if (at.name == name) return at;
            foreach (Transform child in at)
                if (Find(child, name) is Transform found) return found;
            return null;
        }
    }
}

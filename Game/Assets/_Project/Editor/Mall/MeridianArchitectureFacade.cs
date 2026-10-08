using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    public static class MeridianArchitectureFacade
    {
        public static void Place(Transform mall, Transform group)
        {
            Transform walls = mall.Find("Walls");
            if (walls == null) return;
            foreach (Transform panel in walls.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Outside_")))
            {
                if (!MeridianArchitectureDetails.ReadPanel(panel.name, out int floor, out Vector2Int cell, out char side)) continue;
                Vector3 front = side switch { 'S' => Vector3.back, 'N' => Vector3.forward, 'E' => Vector3.right, _ => Vector3.left };
                bool opening = MallLayout.Exterior.Any(e => e.floor == floor && e.side == side && e.column == (side == 'E' || side == 'W' ? cell.y : cell.x)
                    && (MallLayout.ExteriorPosition(e) - panel.position).sqrMagnitude < 0.01f);
                if (opening)
                {
                    MeridianArchitectureDetails.Header(group, panel.position, front, MallWalls.WideWidth, MallWalls.WideHeight, MallLayout.StoryHeight);
                    continue;
                }
                bool anchorFront = side == 'S' || side == 'W' && cell.y < 6;
                if (anchorFront)
                {
                    GameObject model = Prop("FacadeFins", group, panel.position - front * 0.035f, Quaternion.LookRotation(front), null,
                        new Vector3(1f, MallLayout.StoryHeight / 3.75f, 1f));
                    MeridianArchitectureDetails.Permanent(model);
                }
                MeridianArchitectureTrim.WallBands(group, panel.position, front, floor == 0);
            }
            var entrance = MallLayout.Exterior.First(e => e.floor == 0 && e.side == 'S' && !e.window);
            Vector3 entranceAt = MallLayout.ExteriorPosition(entrance);
            Vector3 canopyAt = new(MallLayout.EntranceTarget.x, 0f, entranceAt.z - 2f);
            GameObject canopy = Prop("EntranceCanopy", group, canopyAt, Quaternion.Euler(0f, 180f, 0f));
            MeridianArchitectureDetails.Permanent(canopy);
        }
    }
}

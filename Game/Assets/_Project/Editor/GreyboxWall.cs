using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds a straight greybox wall panel, optionally with a door or window cut into it.
    /// </summary>
    public static class GreyboxWall
    {
        private const float FrameWidth = 0.1f;

        /// <param name="start">Panel start on the wall centreline, at the panel's base height.</param>
        /// <param name="direction">Axis-aligned direction the panel runs in from <paramref name="start"/>.</param>
        public static void Panel(Transform parent, string name, Vector3 start, Vector3 direction,
            float length, float height, float thickness, WallOpening? hole, Material wall, Material frame)
        {
            Transform panel = new GameObject(name).transform;
            panel.SetParent(parent, false);
            panel.localPosition = start + direction * (length / 2f);
            panel.localRotation = Quaternion.FromToRotation(Vector3.right, direction);

            // Pieces are in panel-local space: X along the wall, Y up from the panel base.
            if (hole == null)
            {
                Box("Solid", panel, new Vector3(0f, height / 2f, 0f), new Vector3(length, height, thickness), wall);
                return;
            }

            WallOpening o = hole.Value;
            float side = (length - o.Width) / 2f;
            float top = o.Bottom + o.Height;
            Box("Left", panel, new Vector3(-(o.Width + side) / 2f, height / 2f, 0f), new Vector3(side, height, thickness), wall);
            Box("Right", panel, new Vector3((o.Width + side) / 2f, height / 2f, 0f), new Vector3(side, height, thickness), wall);
            if (top < height)
                Box("Above", panel, new Vector3(0f, (top + height) / 2f, 0f), new Vector3(o.Width, height - top, thickness), wall);
            if (o.Bottom > 0f)
                Box("Below", panel, new Vector3(0f, o.Bottom / 2f, 0f), new Vector3(o.Width, o.Bottom, thickness), wall);

            // Visual-only frame, slightly proud of the wall, so openings read clearly in greybox.
            float ft = thickness + 0.04f, fh = FrameWidth / 2f;
            float midY = o.Bottom + o.Height / 2f;
            Box("Frame_L", panel, new Vector3(-o.Width / 2f - fh, midY, 0f), new Vector3(FrameWidth, o.Height, ft), frame, false);
            Box("Frame_R", panel, new Vector3(o.Width / 2f + fh, midY, 0f), new Vector3(FrameWidth, o.Height, ft), frame, false);
            Box("Frame_Top", panel, new Vector3(0f, top + fh, 0f), new Vector3(o.Width + 2 * FrameWidth, FrameWidth, ft), frame, false);
            if (o.Bottom > 0f)
                Box("Frame_Sill", panel, new Vector3(0f, o.Bottom - fh, 0f), new Vector3(o.Width + 2 * FrameWidth, FrameWidth, ft), frame, false);
        }
    }
}

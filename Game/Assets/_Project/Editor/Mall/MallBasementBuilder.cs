using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    public static class MallBasementBuilder
    {
        public static bool AboveBasement(Vector3 at) => Basement.Contains(new Vector2Int(Mathf.FloorToInt(at.x/Tile),Mathf.FloorToInt(at.z/Tile)));
        public static void Build(Transform mall)
        {
            var basement=Group("Basement",mall);Material m=MeridianArt.Material("Block");
            Box("Retaining_W",basement,new Vector3(39.88f,-2,36),new Vector3(.24f,4,24),m);
            Box("Retaining_E",basement,new Vector3(56.12f,-2,36),new Vector3(.24f,4,24),m);
            Box("Retaining_S",basement,new Vector3(48,-2,23.88f),new Vector3(16,4,.24f),m);
            Box("Retaining_N",basement,new Vector3(48,-2,48.12f),new Vector3(16,4,.24f),m);
            MarkStatic(basement);
            // Flooding stays shallow and passable, outside the permanent lane and approaches.
            Box("Flood",basement,new Vector3(50,-3.965f,28),new Vector3(11.7f,.025f,7.7f),MeridianArt.Material("Water"),false);
        }
    }
}

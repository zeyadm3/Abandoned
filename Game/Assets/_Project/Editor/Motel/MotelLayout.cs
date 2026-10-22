using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelLayout
    {
        public const string ScenePath = "Assets/_Project/Scenes/Motel.unity";
        public const float Width = 64,
        Depth = 28,
        Storey = 4;
        // The courtyard, pool and van apron sit south of z=12 and remain outside the collapse envelope.
        public static readonly Bounds Bounds = new(new Vector3(32, 4, 20), new Vector3(72, 8, 16));
        public static readonly Vector3 [] Crew =
        {
            new(28, 0, - 14),
            new(30, 0, - 14),
            new(34, 0, - 14),
            new(36, 0, - 14)
        };
        public static float RoomX(int room) => room <4 ? room * 7 : 36 +(room - 4) * 7;
        public static int RoomNumber(int level, int room) =>(level + 1) * 100 + room + 1;
        public static bool UpperCell(int x, int z) => x >= 0 && x <16 && z >= 3 && z <7;
        public static bool Protected(int x, int z) => z <= 4;
    }
}

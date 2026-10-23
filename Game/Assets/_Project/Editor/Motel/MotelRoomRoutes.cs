using UnityEngine;
namespace Abandoned.Motel
{
    /// <summary>Connecting pairs offer an escape through the next room when the gallery is occupied.</summary>
    public static class MotelRoomRoutes
    {
        public static bool Partition(Transform parent, MotelKit k, float x, int level)
        {
            bool connects = level == 0 ?(x == 14f || x == 50f) :(x == 7f || x == 57f);
            if (! connects)
            {
                return false;
            }
            float y = level * 4f;
            const float centre = 21.8f,
            width = 2.2f;
            float left = centre - width * .5f,
            right = centre + width * .5f;
            Transform wall = MotelKit.Group("ConnectingRoomWall", parent);
            k.Box("FrontPartition", wall, new Vector3(x, y + 1.82f,(16f + left) * .5f), new Vector3(.18f, 3.64f, left - 16f), k.Wall);
            k.Box("RearPartition", wall, new Vector3(x, y + 1.82f,(28f + right) * .5f), new Vector3(.18f, 3.64f, 28f - right), k.Wall);
            k.Box("DoorHeader", wall, new Vector3(x, y + 3.22f, centre), new Vector3(.18f, .84f, width), k.Wall);
            foreach (float z in new []
            {
                left, right
            })
            {
                k.Box("DoorTrim", wall, new Vector3(x, y + 1.4f, z), new Vector3(.26f, 2.8f, .07f), k.Wood, false);
            }
            k.Box("RemovedDoorHingePlate", wall, new Vector3(x + .14f, y + 1.8f, left + .03f), new Vector3(.035f, .14f, .06f), k.Metal, false);
            return true;
        }
    }
}

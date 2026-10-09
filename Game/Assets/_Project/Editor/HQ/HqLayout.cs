using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>One authored floor plan shared by the HQ shell, operations, stock and lights.</summary>
    public static class HqLayout
    {
        public const float Width = 36f, Depth = 28f, HallWidth = 24f;
        public const float Eave = 6.4f, Ridge = 8.2f, WingHeight = 3.8f;
        public const float WarehouseTrussBottom = 6.2f, ServiceCeiling = WingHeight;
        public const float DispatchBack = 10f, CrewBack = 18f;
        public static readonly Vector3 Dispatch = new(30f, 0f, 5.1f);
        public static readonly Vector3 Maintenance = new(31f, 0f, 26.1f);
        public static readonly Vector3 Depot = new(15.3f, 0f, 5.8f);
        public static readonly Vector3 GearRack = new(35.15f, 0f, 21.6f);
        public static readonly Vector3 Lockers = new(35.15f, 0f, 14.1f);
        public static readonly Vector3 Van = new(6f, 1.25f, 7f);
        public static readonly Vector3 Logistics = new(35.78f, 2.05f, 5.2f);
        public static readonly Vector3 Pegboard = new(31f, 2.04f, 27.8f);
        public static readonly Vector3 MenuEye = new(22.1f, 1.65f, 2.6f);
        public static readonly Vector3 MenuFocus = new(10f, 2.65f, 20f);
    }
}

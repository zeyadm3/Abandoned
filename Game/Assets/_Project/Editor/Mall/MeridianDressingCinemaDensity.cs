using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>The west cinema has a fixed projection screen, fitted ticket desk and rows of bolted seats.</summary>
    public static class MeridianDressingCinemaDensity
    {
        public static void Place(MeridianDressingClearance clearance, Transform group)
        {
            const int floor = 2;
            clearance.Place("CinemaScreen", group, floor, new Vector3(10f, 8f, 16.4f), Quaternion.identity);
            clearance.Place("TicketCounter", group, floor, new Vector3(10f, 8f, 9.1f), Quaternion.identity);
            clearance.Place("ServiceCart", group, floor, new Vector3(5.3f, 8f, 9.4f), Quaternion.Euler(0f, 12f, 0f));
            foreach (float x in new[] { 5.2f, 6.5f, 7.8f, 9.1f, 10.6f })
            for (int row = 0; row < 9; row++)
                clearance.Place("CinemaSeat", group, floor, new Vector3(x, 8f, 18f + row * 1.5f), Quaternion.Euler(0f, 180f, 0f));
        }
    }
}

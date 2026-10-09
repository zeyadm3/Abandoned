using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MeridianTenantCinema
    {
        public static void Build(MeridianTenantPlacement fit, Transform room)
        {
            // One auditorium has a single wide screen. Its middle aisle also leaves the threat spawn free.
            fit.Model("CinemaScreen", room, 1, 8, 12.15f, 180, new Vector3(3.65f, .92f, 1));
            foreach (float z in new[] { 8.65f, 10.2f })
            {
                foreach (float x in new[] { 1.3f, 2.3f, 3.3f, 4.3f, 5.3f, 6.3f, 12.5f, 13.5f, 14.5f, 15.5f })
                    fit.Model("CinemaSeat", room, 1, x, z);
            }
            // The open lobby is separated from seating by the permanent transverse haul aisle.
            fit.Model("TicketCounter", room, 1, 21, 2.15f, 90);
            fit.Model("FoodCourtCounter", room, 1, 18.3f, 10.4f, 180);
            fit.Model("FoodServiceStation", room, 1, 21.8f, 10.4f, 180);
            fit.Model("ConcourseBench", room, 1, 4.5f, 1.05f);
            fit.Model("ConcourseBench", room, 1, 10.5f, 1.05f);
            fit.Model("MallBin", room, 1, 15.2f, 2.1f);
            fit.Model("AdvertisingLightbox", room, 1, 17.2f, 2.0f, 90);
        }
    }
}

using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Stripped merchandising backers make occupied retail walls read as former display runs.</summary>
    public static class SalvageDensityBackdrops
    {
        public static void Build(SalvageDensityPlacement fit)
        {
            Transform tech = fit.Zone("A_StrippedWallDisplays");
            foreach (float x in new[] { 2f, 6f, 10f, 14f, 18f, 22f })
                Backer(fit, tech, 0, x, .19f, "Plastic", 0);
            foreach (float x in new[] { 2.5f, 8f, 14f })
                Backer(fit, tech, 0, x, 15.78f, "Metal", 180);

            for (int floor = 0; floor <= 1; floor++)
            {
                Transform showroom = fit.Zone("B_InterruptedShowroomWalls_" + floor);
                foreach (float x in new[] { 34.7f, 40.8f, 46.8f, 52.8f })
                    Backer(fit, showroom, floor, x, .19f, "Wood", 0);
                foreach (float x in new[] { 43f, 48f, 53f })
                    Backer(fit, showroom, floor, x, 15.78f, "Wood", 180);
            }

            Transform workshop = fit.Zone("C_StrippedToolDisplayWalls");
            foreach (float x in new[] { 42.7f, 47.2f, 52.4f })
                Backer(fit, workshop, 0, x, 16.20f, "Metal", 0);
            Transform apparel = fit.Zone("D_StrippedFashionDisplayWalls");
            foreach (float x in new[] { 2f, 6f, 10f, 14f })
                Backer(fit, apparel, 0, x, 16.20f, "Paint", 0);
            Transform entry = fit.Zone("E_ClearancePaperTrail");
            fit.Scatter(entry, 0, new Rect(24.35f, 2.1f, 7.3f, 9.3f), 38, 155021);
        }

        private static void Backer(SalvageDensityPlacement fit, Transform parent, int floor,
            float x, float z, string finish, float yaw)
        {
            Vector3 rotation = new(0, yaw, 0);
            float direction = yaw == 0 ? 1 : -1;
            fit.Box(parent, "Depleted merchandising backer", floor, new Vector3(x, 1.55f, z),
                new Vector3(2.7f, 2.55f, .028f), finish, rotation, false);
            foreach (float y in new[] { .66f, 1.34f, 2.02f, 2.72f })
            {
                fit.Box(parent, "Removed shelf mounting rail", floor, new Vector3(x, y, z + direction * .026f),
                    new Vector3(2.5f, .036f, .027f), "Metal", rotation, false);
                foreach (float side in new[] { -1f, 1f })
                    fit.Box(parent, "Remaining display bracket", floor,
                        new Vector3(x + side * 1.03f, y - .042f, z + direction * .074f),
                        new Vector3(.026f, .09f, .14f), "Metal", rotation, false);
            }
            fit.Box(parent, "Faded removed fascia strip", floor, new Vector3(x, 3.04f, z),
                new Vector3(2.85f, .16f, .034f), "Paper", rotation, false);
        }
    }
}

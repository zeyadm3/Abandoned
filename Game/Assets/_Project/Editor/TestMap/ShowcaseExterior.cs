using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Outside the TestMap warehouse: a concrete apron with a kerb along the front, a path round the west side to the
    /// gallery, street lamps, a covered car in a bay, road barriers, wall packs over the doors, bins and plant. The
    /// players' spawn row and the loading lane in front of the big door stay clear.
    /// </summary>
    public static class ShowcaseExterior
    {
        private const float PavingTop = .05f; // a 3 cm kerb above the car park's 2 cm asphalt

        public static void Build(Transform root)
        {
            Transform outside = Group("Outside", root);
            Transform paving = Group("Paving", outside);
            Material pavement = ShowcaseMaterials.Pavement;
            Slab(paving, "Apron", new Vector3(-2.75f, 0f, -2f), new Vector3(22.75f, 0f, -.31f), pavement);
            Slab(paving, "Path_W", new Vector3(-2.75f, 0f, -.31f), new Vector3(-.75f, 0f, 17.75f), pavement);
            Slab(paving, "Path_N", new Vector3(-2.75f, 0f, 17.75f), new Vector3(12f, 0f, 19.75f), pavement);
            MarkStatic(paving);

            Transform props = Group("Props", outside);
            ShowcaseModels.Place("covered_car", props, new Vector3(2.5f, .02f, -12.6f), 0f, new Vector3(2.4f, 2f, 5.4f));
            for (int i = 0; i < 3; i++)
                ShowcaseModels.Place("concrete_road_barrier_02", props, new Vector3(21.6f, 0f, -4.5f - i * 3.6f), 90f, new Vector3(3.5f, 1.3f, 1f));
            ShowcaseModels.Place("water_manhole_cover", props, new Vector3(10f, .02f, -15f), 0f, new Vector3(.9f, .1f, .9f), solid: false);
            ShowcaseModels.Place("Barrel_01", props, new Vector3(20.95f, 0f, 9.2f), 20f, new Vector3(.75f, 1.1f, .75f));
            ShowcaseModels.Place("barrel_03", props, new Vector3(21.1f, 0f, 10.2f), 75f, new Vector3(.75f, 1.1f, .75f));
            ShowcaseModels.Place("trashbag", props, new Vector3(21f, 0f, 11.2f), 0f, new Vector3(.7f, .7f, .7f));
            ShowcaseModels.Place("utility_box_01", props, new Vector3(21f, 0f, 3f), 270f, new Vector3(1.2f, 1.6f, .7f));
            ShowcaseModels.Place("exterior_aircon_unit", props, new Vector3(4f, 7.35f, 12f), 0f, new Vector3(1.2f, 1.3f, 1.2f));
            ShowcaseModels.Place("exterior_aircon_unit", props, new Vector3(15f, 7.35f, 12.4f), 180f, new Vector3(1.2f, 1.3f, 1.2f));

            Transform lights = Group("Lights", outside);
            int seed = 700;
            StreetLamp(lights, new Vector3(-1.6f, .02f, -4.5f), 90f, seed++);
            StreetLamp(lights, new Vector3(21.6f, .02f, -15f), 270f, seed++);
            // The skin's outer face is 0.305 m out from the walls' inner faces.
            ShowcaseLighting.WallPack(lights, "WallPack_Door", new Vector3(6f, 2.8f, -.31f), Vector3.back, seed++);
            ShowcaseLighting.WallPack(lights, "WallPack_Loading", new Vector3(14f, 4.45f, -.31f), Vector3.back, seed++);
            ShowcaseLighting.WallPack(lights, "WallPack_Path", new Vector3(-.31f, 2.8f, 9f), Vector3.left, seed++);
            ShowcaseLighting.WallPack(lights, "WallPack_Gallery",
                new Vector3(10f, 3.7f, ShowcaseGallery.Min.z - ShowcaseGallery.Wall - .005f), Vector3.back, seed);
        }

        // A paving slab from min to max (x, z) whose top is at PavingTop.
        private static void Slab(Transform parent, string name, Vector3 min, Vector3 max, Material material)
        {
            const float thick = .15f;
            Box(name, parent, new Vector3((min.x + max.x) / 2f, PavingTop - thick / 2f, (min.z + max.z) / 2f),
                new Vector3(max.x - min.x, thick, max.z - min.z), material);
        }

        private static void StreetLamp(Transform parent, Vector3 at, float yaw, int seed)
        {
            GameObject lamp = ShowcaseModels.Place("street_lamp_01", parent, at, yaw, new Vector3(2.5f, 9f, 2.5f));
            if (lamp == null) return;
            Bounds b = ShowcaseModels.LocalBounds(lamp.transform);
            // The head is somewhere over the bounds' top: light from just under it, straight down.
            var under = new Vector3(b.center.x, b.max.y - .3f, b.center.z);
            GameObject lens = Box("Lens", lamp.transform, under + Vector3.up * .02f, new Vector3(.3f, .01f, .3f), ShowcaseMaterials.Glow, false);
            lens.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ShowcaseLighting.Fit(lamp.transform, under, Quaternion.Euler(90f, 0f, 0f), ShowcaseLighting.Sodium, 16f, 125f,
                lens.GetComponent<Renderer>(), false, false, seed);
        }
    }
}

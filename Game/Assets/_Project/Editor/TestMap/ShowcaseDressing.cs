using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the TestMap's warehouse holds: shelving on the north wall, barrels, crates, a pallet in the atrium, an office
    /// upstairs. Everything keeps clear of what the play tests walk (both front doors, the stair approach and landing,
    /// the upper north corridor the server rack is dragged along, every upper tile's centre) and of the loot, which
    /// still rests on the same three surfaces (crate, desk, pedestal) as before, now dressed as real objects.
    /// </summary>
    public static class ShowcaseDressing
    {
        private const float Upper = 4f;

        // id, floor point, yaw, largest size it may take (x across its yaw, y, z).
        private static readonly (string id, Vector3 at, float yaw, Vector3 fit)[] Warehouse =
        {
            ("worn_metal_rack", new(1.6f, 0f, 15.35f), 180f, new(2.6f, 2.6f, 1f)),
            ("steel_frame_shelves_01", new(4.4f, 0f, 15.35f), 180f, new(2.6f, 2.6f, 1f)),
            ("steel_frame_shelves_02", new(7.2f, 0f, 15.35f), 180f, new(2.6f, 2.6f, 1f)),
            ("steel_frame_shelves_01", new(10f, 0f, 15.35f), 180f, new(2.6f, 2.6f, 1f)),
            ("worn_metal_rack", new(12.8f, 0f, 15.35f), 180f, new(2.6f, 2.6f, 1f)),
            ("portable_generator", new(18.8f, 0f, 15f), 180f, new(1.2f, 1f, .9f)),
            ("propane_tank", new(17f, 0f, 15.4f), 0f, new(.5f, 1.3f, .5f)),
            ("propane_tank", new(17.6f, 0f, 15.5f), 40f, new(.5f, 1.3f, .5f)),
            ("metal_jerrycan", new(19.4f, 0f, 13f), 75f, new(.5f, .5f, .3f)),
            ("Barrel_01", new(.75f, 0f, 7.3f), 0f, new(.75f, 1.1f, .75f)),
            ("Barrel_01", new(.75f, 0f, 8.3f), 130f, new(.75f, 1.1f, .75f)),
            ("barrel_03", new(1.6f, 0f, 7.8f), 60f, new(.75f, 1.1f, .75f)),
            ("plastic_crate_01", new(1.65f, 0f, .5f), 8f, new(.7f, .5f, .55f)),
            ("WetFloorSign_01", new(4.3f, 0f, 2.6f), 200f, new(.45f, .75f, .45f)),
            ("hand_truck", new(8.3f, 0f, .45f), 0f, new(.6f, 1.3f, .6f)),
            ("metal_tool_chest", new(11.45f, 0f, .4f), 180f, new(.8f, 1.2f, .55f)),
            ("multi_cleaner_5_litre", new(12.15f, 0f, .3f), 20f, new(.3f, .4f, .3f)),
            ("wooden_crate_01", new(15.55f, 0f, 4.7f), 12f, new(.9f, .9f, .9f)),
            ("tool_cart", new(3.4f, 0f, 12.8f), 90f, new(1f, 1.1f, .65f)),
            ("cement_bag", new(10.6f, 0f, 13.6f), 85f, new(.7f, .25f, .45f)),
            ("cement_bag", new(11.35f, 0f, 13.75f), 95f, new(.7f, .25f, .45f)),
            ("old_tyre", new(9.4f, 0f, 9.5f), 0f, new(.8f, .3f, .8f)),
            ("metal_trash_can", new(15.6f, 0f, 10.4f), 0f, new(.6f, .9f, .6f)),
            ("trashbag", new(15.4f, 0f, 11.4f), 30f, new(.7f, .7f, .7f)),
            ("plastic_broom", new(19.7f, 0f, 1.2f), 0f, new(.4f, 1.5f, .4f)),
        };

        // Upstairs: id, floor point, yaw, fit, and the upper tile (x, z) it stands on.
        private static readonly (string id, Vector3 at, float yaw, Vector3 fit, Vector2Int tile)[] Offices =
        {
            ("metal_office_desk", new(2f, Upper, .5f), 180f, new(1.6f, .85f, .8f), new(0, 0)),
            ("GreenChair_01", new(2f, Upper, 1.25f), 160f, new(.6f, 1f, .6f), new(0, 0)),
            ("SchoolChair_01", new(.5f, Upper, 3.2f), 90f, new(.55f, .95f, .55f), new(0, 0)),
            ("drawer_cabinet", new(8.8f, Upper, .4f), 180f, new(.6f, 1.4f, .7f), new(2, 0)),
            ("wooden_bookshelf_worn", new(11f, Upper, .35f), 180f, new(1.2f, 2.2f, .5f), new(2, 0)),
            ("cardboard_box_01", new(7.2f, Upper, .5f), 25f, new(.6f, .6f, .6f), new(1, 0)),
            ("metal_trash_can", new(12.6f, Upper, .4f), 0f, new(.5f, .8f, .5f), new(3, 0)),
            ("Sofa_01", new(.6f, Upper, 10f), 90f, new(2.2f, 1f, 1f), new(0, 2)),
            ("drawer_cabinet", new(.45f, Upper, 12.8f), 90f, new(.6f, 1.4f, .7f), new(0, 3)),
        };

        public static void Dress(Transform root)
        {
            Transform warehouse = Group("Warehouse", Group("Dressing", root));
            foreach (var (id, at, yaw, fit) in Warehouse) ShowcaseModels.Place(id, warehouse, at, yaw, fit);
            Pallet(warehouse, new Vector3(8f, 0f, 8f), 8f);
            GameObject box = ShowcaseModels.Place("cardboard_box_01", warehouse, new Vector3(.6f, 0f, .6f), 0f, new Vector3(.7f, .7f, .7f));
            On(box, "cardboard_box_01", warehouse, new Vector3(.05f, 0f, .02f), 17f, new Vector3(.6f, .6f, .6f));

            Transform offices = root.Find("Dressing");
            offices = Group("Offices", offices);
            GameObject desk = null;
            foreach (var (id, at, yaw, fit, tile) in Offices)
            {
                GameObject placed = ShowcaseModels.Place(id, offices, at, yaw, fit, support: UpperSection(root, tile));
                if (id == "metal_office_desk") desk = placed;
            }
            StructuralSection deskTile = UpperSection(root, new Vector2Int(0, 0));
            On(desk, "desk_lamp_arm_01", offices, new Vector3(-.55f, 0f, -.12f), 200f, new Vector3(.4f, .6f, .4f), deskTile);
            On(desk, "binder_notebook", offices, new Vector3(.1f, 0f, -.05f), 12f, new Vector3(.35f, .08f, .3f), deskTile);
            On(desk, "clipboard", offices, new Vector3(.45f, 0f, .08f), -20f, new Vector3(.3f, .05f, .4f), deskTile);
            On(desk, "stationery_supplies", offices, new Vector3(-.2f, 0f, .15f), 0f, new Vector3(.25f, .2f, .2f), deskTile);

            DressLootSurfaces(root);
        }

        /// <summary>The upper floor section at tile (x, z) (floor or balcony), or null.</summary>
        public static StructuralSection UpperSection(Transform root, Vector2Int tile)
        {
            Transform t = root.Find($"UpperFloor/Tiles/Tile_U_{tile.x}_{tile.y}") ?? root.Find($"UpperFloor/Tiles/Balcony_U_{tile.x}_{tile.y}");
            return t != null ? t.GetComponent<StructuralSection>() : null;
        }

        // The populator's crate, desk and pedestal stay where the loot expects them, with the same colliders; their
        // grey boxes become a crate, an office desk and a cast plinth.
        private static void DressLootSurfaces(Transform root)
        {
            Transform props = root.Find("Props");
            if (props == null) return;
            Transform crate = props.Find("Crate"), desk = props.Find("Desk"), pedestal = props.Find("Pedestal");
            if (crate != null && Hide(crate))
                ShowcaseModels.Stretch("wooden_crate_02", props, Base(crate), 0f, crate.localScale, solid: false);
            if (desk != null && Hide(desk))
                ShowcaseModels.Stretch("metal_office_desk", props, Base(desk), 0f, desk.localScale, solid: false);
            if (pedestal != null) pedestal.GetComponent<Renderer>().sharedMaterial = ShowcaseMaterials.Plinth;
        }

        private static Vector3 Base(Transform box) => box.position - Vector3.up * box.localScale.y / 2f;

        private static bool Hide(Transform box)
        {
            // Only once the model is really there: a missing download keeps the old box visible.
            if (!ShowcaseModels.Has(box.name == "Crate" ? "wooden_crate_02" : "metal_office_desk")) return false;
            Object.DestroyImmediate(box.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(box.GetComponent<MeshFilter>());
            return true;
        }

        // Model id resting on top of <below>, offset in <below>'s yaw.
        private static void On(GameObject below, string id, Transform parent, Vector3 offset, float yaw, Vector3 fit, StructuralSection support = null)
        {
            if (below == null) return;
            Bounds b = ShowcaseModels.LocalBounds(below.transform);
            Vector3 at = below.transform.TransformPoint(new Vector3(b.center.x + offset.x, b.max.y + .002f, b.center.z + offset.z));
            ShowcaseModels.Place(id, parent, at, below.transform.eulerAngles.y + yaw, fit, solid: false, support);
        }

        // A code-built timber pallet with a crate and a box on it.
        private static void Pallet(Transform parent, Vector3 at, float yaw)
        {
            var pallet = Group("Pallet", parent);
            pallet.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material wood = ShowcaseMaterials.Pallet;
            foreach (float x in new[] { -.5f, 0f, .5f })
                Box($"Bearer_{x}", pallet, new Vector3(x, .05f, 0f), new Vector3(.1f, .1f, 1f), wood, false);
            for (int i = 0; i < 7; i++)
                Box($"Board_{i}", pallet, new Vector3(0f, .11f, -.45f + i * .15f), new Vector3(1.2f, .02f, .1f), wood, false);
            pallet.gameObject.AddComponent<BoxCollider>().center = new Vector3(0f, .06f, 0f);
            pallet.GetComponent<BoxCollider>().size = new Vector3(1.2f, .12f, 1f);
            MarkStatic(pallet);
            GameObject crate = ShowcaseModels.Place("wooden_crate_02", parent, pallet.TransformPoint(new Vector3(-.2f, .121f, 0f)), yaw + 4f, new Vector3(.8f, .8f, .8f));
            ShowcaseModels.Place("cardboard_box_01", parent, pallet.TransformPoint(new Vector3(.38f, .121f, .2f)), yaw - 9f, new Vector3(.42f, .42f, .42f));
            On(crate, "cardboard_box_01", parent, Vector3.zero, 21f, new Vector3(.5f, .5f, .5f));
        }
    }
}

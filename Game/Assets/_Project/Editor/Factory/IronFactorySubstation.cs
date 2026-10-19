using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The high-voltage electrical substation in the south-west yard (x = 14, z = -15).
    /// In heavy metallurgy, melting and rolling require massive electrical feeds. This compound
    /// features a crushed rock gravel yard, chain-link security fence, twin 50MVA step-down
    /// transformers with cooling radiator banks and high-voltage ceramic bushings, an overhead
    /// busbar lattice structure with disconnect switches, and a control relay building.
    /// </summary>
    public static class IronFactorySubstation
    {
        private static readonly Vector3 Center = new(15f, 0f, -16f);
        private const float Width = 18f;
        private const float Depth = 12f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform station = Group("ElectricalSubstation", root);
            station.position = Center;

            Material gravelMat = GetMaterial("IronFactory_SubstationGravel", new Color(.28f, .27f, .25f));
            Material porcelain = GetMaterial("IronFactory_PorcelainInsulator", new Color(.38f, .2f, .14f));
            Material copper = GetMaterial("IronFactory_CopperBus", new Color(.72f, .45f, .28f));
            Material warningMat = GetMaterial("IronFactory_WarningYellow", new Color(.85f, .75f, .15f));

            // Crushed stone gravel bed with concrete retaining curbs
            Box("GravelBed", station, Vector3.up * .04f, new Vector3(Width, .08f, Depth), gravelMat, false);
            foreach (float sx in new[] { -1f, 1f })
                Box("Curb_X", station, new Vector3(sx * (Width * .5f - .1f), .15f, 0f), new Vector3(.2f, .3f, Depth), art.Concrete, true);
            foreach (float sz in new[] { -1f, 1f })
                Box("Curb_Z", station, new Vector3(0f, .15f, sz * (Depth * .5f - .1f)), new Vector3(Width, .3f, .2f), art.Concrete, true);

            // Perimeter chainlink security fence around the substation
            BuildSubstationFence(station, art, warningMat);

            // Twin main step-down power transformers
            foreach (float x in new[] { -4.5f, 2.5f })
                BuildTransformer(station, new Vector3(x, 0f, 0f), art, porcelain, copper);

            // Overhead steel lattice busbar gantry spanning across the transformers
            BuildBusbarGantry(station, art, porcelain, copper);

            // Small brick/metal control relay building in east corner of the yard
            Transform relay = Group("RelayBuilding", station);
            relay.localPosition = new Vector3(Width * .5f - 2.4f, 0f, -Depth * .5f + 2.2f);
            Box("Walls", relay, Vector3.up * 1.6f, new Vector3(3.2f, 3.2f, 2.8f), art.RoomWall, true);
            Box("Roof", relay, Vector3.up * 3.25f, new Vector3(3.5f, .15f, 3.1f), art.Steel, true);
            Box("Door", relay, new Vector3(-1.61f, 1.1f, 0f), new Vector3(.04f, 2.2f, 1.1f), art.Steel, false);
            // Amber pilot light over relay door
            Box("Lamp", relay, new Vector3(-1.65f, 2.4f, 0f), new Vector3(.12f, .12f, .12f), art.Safety, false);

            station.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(station);
        }

        private static void BuildTransformer(Transform parent, Vector3 at, IronFactoryArt art, Material porcelain, Material copper)
        {
            Transform tf = Group($"Transformer_{at.x:0.0}", parent);
            tf.localPosition = at;

            // Concrete mounting plinth
            Box("Plinth", tf, Vector3.up * .2f, new Vector3(4.2f, .4f, 3.8f), art.Concrete, true);

            // Main steel oil tank body
            Box("TankBody", tf, Vector3.up * 2f, new Vector3(2.6f, 3.2f, 2.4f), art.Rust, true);

            // Side radiator cooling fins (heat dissipation banks)
            foreach (float side in new[] { -1f, 1f })
            {
                Transform rad = Group($"Radiator_{side:0.0}", tf);
                rad.localPosition = new Vector3(0f, 2f, side * 1.55f);
                for (float fx = -1f; fx <= 1f; fx += .25f)
                    Box("Fin", rad, new Vector3(fx, 0f, 0f), new Vector3(.04f, 2.6f, .65f), art.Steel, false);
                // Top and bottom manifold pipes
                Box("HeaderTop", rad, Vector3.up * 1.35f, new Vector3(2.2f, .18f, .18f), art.Steel, false);
                Box("HeaderBtm", rad, Vector3.down * 1.35f, new Vector3(2.2f, .18f, .18f), art.Steel, false);
            }

            // Top cylindrical oil conservator tank
            Primitive(PrimitiveType.Cylinder, "Conservator", tf, new Vector3(0f, 4.1f, -.5f),
                new Vector3(.85f, 2.2f, .85f), art.Steel, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Three tall high-voltage ceramic insulator bushings with arcing horns
            for (int i = 0; i < 3; i++)
            {
                float x = -1f + i * 1f;
                Transform bushing = Group($"Bushing_{i}", tf);
                bushing.localPosition = new Vector3(x, 3.6f, .5f);
                bushing.localRotation = Quaternion.Euler(15f, 0f, 0f);

                // Stacked ceramic shed discs
                Primitive(PrimitiveType.Cylinder, "Shed_1", bushing, Vector3.up * .3f, new Vector3(.35f, .15f, .35f), porcelain, false);
                Primitive(PrimitiveType.Cylinder, "Shed_2", bushing, Vector3.up * .6f, new Vector3(.32f, .15f, .32f), porcelain, false);
                Primitive(PrimitiveType.Cylinder, "Shed_3", bushing, Vector3.up * .9f, new Vector3(.28f, .15f, .28f), porcelain, false);
                // Terminal cap & arcing horn
                Box("Terminal", bushing, Vector3.up * 1.1f, new Vector3(.08f, .25f, .08f), copper, false);
                Box("ArcingHorn", bushing, new Vector3(.08f, .8f, 0f), new Vector3(.02f, .6f, .02f), copper, false);
            }

            // Local control marshalling cabinet on tank face
            Box("ControlCabinet", tf, new Vector3(-1.32f, 1.4f, 0f), new Vector3(.08f, 1.2f, .9f), art.Safety, false);
        }

        private static void BuildBusbarGantry(Transform parent, IronFactoryArt art, Material porcelain, Material copper)
        {
            Transform gantry = Group("BusbarGantry", parent);
            const float gantryH = 7.5f;

            // Two A-frame lattice support towers
            foreach (float z in new[] { -3f, 3f })
            {
                Transform tower = Group($"GantryPillar_{z:0.0}", gantry);
                tower.localPosition = new Vector3(0f, 0f, z);
                Box("Pillar_L", tower, new Vector3(-5.5f, gantryH * .5f, 0f), new Vector3(.25f, gantryH, .25f), art.Steel, false);
                Box("Pillar_R", tower, new Vector3(5.5f, gantryH * .5f, 0f), new Vector3(.25f, gantryH, .25f), art.Steel, false);
                Box("TopBeam", tower, new Vector3(0f, gantryH, 0f), new Vector3(11.5f, .35f, .35f), art.Steel, false);
            }

            // Longitudinal overhead copper busbars connecting the gantries
            for (float x = -4.5f; x <= 4.5f; x += 3f)
            {
                Box("Busbar", gantry, new Vector3(x, gantryH + .4f, 0f), new Vector3(.04f, .04f, 7.5f), copper, false);
                // Hanging insulator strings
                foreach (float z in new[] { -3f, 3f })
                    Primitive(PrimitiveType.Cylinder, "HangingInsulator", gantry, new Vector3(x, gantryH - .3f, z),
                        new Vector3(.22f, .6f, .22f), porcelain, false);
            }
        }

        private static void BuildSubstationFence(Transform parent, IronFactoryArt art, Material warningMat)
        {
            Transform fence = Group("SecurityFence", parent);
            const float fenceH = 2.8f;
            Vector2[] corners =
            {
                new(-Width * .5f, -Depth * .5f), new(Width * .5f, -Depth * .5f),
                new(Width * .5f, Depth * .5f), new(-Width * .5f, Depth * .5f)
            };

            for (int side = 0; side < 4; side++)
            {
                Vector2 a = corners[side], b = corners[(side + 1) % 4];
                Vector3 from = new(a.x, 0f, a.y), to = new(b.x, 0f, b.y);
                float length = Vector3.Distance(from, to);
                Vector3 mid = (from + to) * .5f;
                Transform line = Group($"Side_{side}", fence);
                line.SetPositionAndRotation(mid, Quaternion.LookRotation((to - from).normalized));

                // Mesh guard panel so chainlink texture is visible
                Box("MeshPanel", line, Vector3.up * (fenceH * .5f), new Vector3(.03f, fenceH, length), art.Guard, false);

                // Top angled barbed wire outrigger arms
                int posts = Mathf.CeilToInt(length / 3f);
                for (int i = 0; i <= posts; i++)
                {
                    float pz = -length * .5f + length * i / posts;
                    Box("Post", line, new Vector3(0f, fenceH * .5f, pz), new Vector3(.08f, fenceH, .08f), art.Steel, false);
                    GameObject outrigger = Box("Outrigger", line, new Vector3(-.18f, fenceH + .2f, pz), new Vector3(.04f, .45f, .04f), art.Steel, false);
                    outrigger.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
                }

                // Barbed wire strands
                foreach (float y in new[] { fenceH + .1f, fenceH + .25f, fenceH + .4f })
                    Box("BarbedStrand", line, new Vector3(-.18f, y, 0f), new Vector3(.02f, .02f, length), art.Steel, false);

                // Warning placard on south and east face
                if (side == 0 || side == 1)
                {
                    Transform sign = Group("DangerSign", line);
                    sign.localPosition = new Vector3(-.04f, 1.8f, 0f);
                    Box("Board", sign, Vector3.zero, new Vector3(.02f, .55f, .8f), warningMat, false);
                    Box("DangerBar", sign, new Vector3(-.015f, .12f, 0f), new Vector3(.01f, .18f, .7f), art.Bed, false);
                }
            }
        }
    }
}

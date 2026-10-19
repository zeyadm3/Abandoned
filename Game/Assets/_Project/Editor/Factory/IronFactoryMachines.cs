using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IronFactoryMachinePlan.MachineKind;
using Plan = Abandoned.EditorTools.IronFactoryMachinePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The dead machines of the machinery floor, built from original low-poly blocks: presses, rolling-mill
    /// stands and roller tables, ore hoppers, casting moulds, lathes, mills, boilers, conveyors and stacked
    /// ingots. Each is one solid box for physics and the NavMesh (monsters path round it, sight stops at it)
    /// with its look built beside it. They are dead for now; step 4 brings some back with the power.
    /// </summary>
    public static class IronFactoryMachines
    {
        public static List<(Plan.Machine plan, Transform root)> Build(Transform root, IronFactoryArt art, IronFactoryClearance clearance)
        {
            Transform group = Group("Machinery", root);
            var built = new List<(Plan.Machine, Transform)>();
            for (int i = 0; i < Plan.Machines.Length; i++)
            {
                Plan.Machine m = Plan.Machines[i];
                Vector3 size = Plan.Size(m);
                var footprint = new Bounds(new Vector3(m.At.x, size.y * .5f, m.At.y), size);
                string problem = clearance.Problem(footprint);
                if (problem != null)
                {
                    Debug.LogWarning($"[IronFactory] Left out {m.Kind} at {m.At}: it would block {problem}.");
                    continue;
                }
                built.Add((m, Machine(group, $"{m.Kind}_{i:00}", m, art)));
            }
            MarkStatic(group);
            return built;
        }

        private static Transform Machine(Transform parent, string name, Plan.Machine m, IronFactoryArt art)
        {
            Vector3 s = Plan.Size(m.Kind);
            if (m.Length > 0f) s.z = m.Length;
            Transform root = Group(name, parent);
            root.SetPositionAndRotation(new Vector3(m.At.x, 0f, m.At.y), Quaternion.Euler(0f, m.Turned ? 90f : 0f, 0f));
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.up * s.y * .5f;
            solid.size = s;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

            Transform v = Group("Visual", root);
            float w = s.x, h = s.y, d = s.z;
            switch (m.Kind)
            {
                case K.Press:
                    Part(v, "Plinth", 0f, .3f, 0f, w, .6f, d, art.Steel);
                    Part(v, "Stripe", 0f, .62f, -d * .5f + .02f, w, .08f, .04f, art.Safety);
                    foreach (float x in new[] { -1f, 1f })
                        Part(v, "Column", x * (w * .5f - .35f), h * .5f, 0f, .6f, h, .7f, art.Steel);
                    Part(v, "Bed", 0f, .85f, 0f, w * .7f, .5f, d * .7f, art.Rust);
                    Part(v, "Ram", 0f, h * .55f, 0f, w * .55f, 1.1f, d * .6f, art.Rust);
                    Part(v, "Crown", 0f, h - .45f, 0f, w, .9f, d * .85f, art.Steel);
                    break;
                case K.MillStand:
                    foreach (float x in new[] { -1f, 1f })
                        Part(v, "Housing", x * (w * .5f - .3f), h * .5f, 0f, .6f, h, d, art.Steel);
                    Part(v, "Cap", 0f, h - .25f, 0f, w, .5f, d, art.Steel);
                    Part(v, "RollTop", 0f, 1.9f, 0f, w - 1.2f, .55f, .55f, art.Rust);
                    Part(v, "RollBottom", 0f, 1.25f, 0f, w - 1.2f, .55f, .55f, art.Rust);
                    Part(v, "Screwdown", 0f, h + .3f, 0f, .4f, .6f, .4f, art.Safety);
                    break;
                case K.RollerTable:
                    Part(v, "Frame", 0f, .35f, 0f, w, .7f, d, art.Steel);
                    for (float z = -d * .5f + .3f; z < d * .5f; z += .6f)
                        Part(v, "Roller", 0f, .8f, z, w - .1f, .12f, .14f, art.Rust);
                    break;
                case K.Hopper:
                    foreach (float x in new[] { -1f, 1f })
                    foreach (float z in new[] { -1f, 1f })
                        Part(v, "Leg", x * (w * .5f - .15f), 1.1f, z * (d * .5f - .15f), .25f, 2.2f, .25f, art.Steel);
                    Part(v, "Bin", 0f, 3.3f, 0f, w, 2.2f, d, art.Rust);
                    Part(v, "Chute", 0f, 1.8f, 0f, w * .35f, .9f, d * .35f, art.Steel);
                    Part(v, "Rim", 0f, h - .05f, 0f, w + .1f, .1f, d + .1f, art.Safety);
                    break;
                case K.MouldRow:
                    Part(v, "Trough", 0f, .25f, 0f, w, .5f, d, art.Lining);
                    for (int i = 0; i < 5; i++)
                        Part(v, "Mould", -w * .4f + i * w * .2f, .52f, 0f, w * .16f, .06f, d * .7f, art.Bed);
                    break;
                case K.Lathe:
                    Part(v, "Bed", 0f, .45f, 0f, w, .9f, d * .7f, art.Steel);
                    Part(v, "Headstock", -w * .5f + .45f, .95f, 0f, .9f, 1.1f, d, art.Steel);
                    Part(v, "Chuck", -w * .5f + 1f, 1.1f, 0f, .2f, .45f, .45f, art.Rust);
                    Part(v, "Tailstock", w * .5f - .3f, 1.05f, 0f, .4f, .4f, .5f, art.Steel);
                    Part(v, "Guard", -w * .5f + .45f, h - .05f, 0f, .9f, .1f, d, art.Safety);
                    break;
                case K.Mill:
                    Part(v, "Column", 0f, h * .5f, d * .25f, w * .5f, h, d * .5f, art.Steel);
                    Part(v, "Table", 0f, .85f, -d * .15f, w, .25f, d * .7f, art.Rust);
                    Part(v, "Knee", 0f, .4f, -d * .15f, w * .5f, .8f, d * .5f, art.Steel);
                    Part(v, "Head", 0f, h - .4f, -d * .1f, w * .55f, .7f, d * .6f, art.Steel);
                    break;
                case K.Boiler:
                    Primitive(PrimitiveType.Cylinder, "Drum", v, new Vector3(0f, h * .5f, 0f), new Vector3(w, h * .5f, d), art.Rust, false);
                    Part(v, "Firebox", 0f, .6f, -d * .45f, w * .6f, 1.2f, .3f, art.Lining);
                    Part(v, "Flue", 0f, h + 1.5f, 0f, .4f, 3f, .4f, art.Steel);
                    break;
                case K.SteamHammer:
                    Part(v, "Anvil", 0f, .6f, 0f, w * .65f, 1.2f, d * .65f, art.Steel);
                    Part(v, "BottomDie", 0f, 1.25f, 0f, w * .35f, .25f, d * .35f, art.Rust);
                    foreach (float x in new[] { -1f, 1f })
                        Part(v, "Column", x * (w * .5f - .4f), h * .52f, 0f, .7f, h * .8f, .85f, art.Steel);
                    Part(v, "Crown", 0f, h - .4f, 0f, w, .7f, d * .8f, art.Steel);
                    Primitive(PrimitiveType.Cylinder, "Cylinder", v, new Vector3(0f, h + .8f, 0f), new Vector3(1.1f, .9f, 1.1f), art.Rust, false);
                    Part(v, "Ram", 0f, h * .48f, 0f, w * .4f, 1.3f, d * .45f, art.Rust);
                    Part(v, "SteamFeed", w * .35f, h + .8f, 0f, .18f, 1.4f, .18f, art.Steel);
                    break;
                case K.SoakingPit:
                    Part(v, "Curb", 0f, .4f, 0f, w, .8f, d, art.Lining);
                    Part(v, "Hearth", 0f, .15f, 0f, w - .8f, .3f, d - .8f, art.Bed);
                    Part(v, "CoverS", 0f, .85f, -d * .25f, w - .2f, .18f, d * .48f, art.Steel);
                    Part(v, "CoverN", 0f, .9f, d * .35f, w - .2f, .18f, d * .48f, art.Steel);
                    Part(v, "HeatGlow", 0f, .22f, d * .08f, w - .9f, .05f, .7f, art.Lava);
                    Part(v, "EyeLug", -.8f, 1.05f, -d * .25f, .12f, .22f, .12f, art.Steel);
                    Part(v, "EyeLug", .8f, 1.05f, -d * .25f, .12f, .22f, .12f, art.Steel);
                    break;
                case K.CastingBed:
                    Part(v, "Frame", 0f, .35f, 0f, w, .7f, d, art.Steel);
                    for (float z = -d * .5f + .5f; z < d * .5f; z += .8f)
                        Part(v, "SkidRail", 0f, .75f, z, w - .1f, .1f, .12f, art.Rust);
                    for (float z = -d * .4f; z < d * .4f; z += 1.4f)
                        Part(v, "Billet", 0f, .9f, z, w * .75f, .22f, .35f, art.Lava);
                    break;
                case K.Pallet:
                    Part(v, "Pallet", 0f, .07f, 0f, w, .14f, d, art.Steel);
                    Part(v, "Ingots", 0f, .14f + (h - .14f) * .5f, 0f, w * .9f, h - .14f, d * .9f, art.Rust);
                    Part(v, "Strap", 0f, .14f + (h - .14f) * .5f, 0f, .05f, h - .12f, d * .92f, art.Safety);
                    break;
                default: // conveyor
                    Part(v, "Frame", 0f, .4f, 0f, w, .8f, d, art.Steel);
                    Part(v, "Belt", 0f, .83f, 0f, w - .1f, .05f, d, art.Bed);
                    foreach (float x in new[] { -1f, 1f })
                        Part(v, "Lip", x * (w * .5f - .03f), .95f, 0f, .06f, .2f, d, art.Safety);
                    break;
            }
            return root;
        }

        private static void Part(Transform v, string name, float x, float y, float z, float w, float h, float d, Material m) =>
            Box(name, v, new Vector3(x, y, z), new Vector3(w, h, d), m, false);
    }
}

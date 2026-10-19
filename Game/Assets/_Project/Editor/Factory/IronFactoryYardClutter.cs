using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Heavy industrial clutter, scrap bins, cable spools, dumpsters, and parked yard machinery
    /// placed around the dock parking lot and exterior perimeter. Replaces empty asphalt voids with
    /// rich, believable steelworks clutter.
    /// </summary>
    public static class IronFactoryYardClutter
    {
        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform yardClutter = Group("YardClutter", root);

            Material woodMat = GetMaterial("IronFactory_SpoolWood", new Color(.38f, .3f, .22f));
            Material dumpsterMat = GetMaterial("IronFactory_DumpsterGreen", new Color(.16f, .25f, .22f));
            Material scrapMat = GetMaterial("IronFactory_HeavyScrap", new Color(.25f, .2f, .18f));

            // Scrap metal storage bunker bin at west side of dock lot (x = 42, z = -10)
            BuildScrapBunker(yardClutter, new Vector3(42f, 0f, -10f), art, scrapMat);

            // Roll-off industrial scrap dumpster at (x = 35, z = -7)
            BuildDumpster(yardClutter, new Vector3(35f, 0f, -7f), 15f, art, dumpsterMat, scrapMat);

            // Giant heavy timber cable spools
            BuildCableSpool(yardClutter, new Vector3(47f, 0f, -13f), false, art, woodMat);
            BuildCableSpool(yardClutter, new Vector3(74f, 0f, -12f), true, art, woodMat);

            // Concrete Jersey barrier line along the dock lot perimeter
            BuildJerseyBarrierRun(yardClutter, art);

            // Rusted yard forklift parked on the east dock apron (x = 71, z = -6.5)
            BuildParkedForklift(yardClutter, new Vector3(71f, 0f, -6.5f), -20f, art);

            yardClutter.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            MarkStatic(yardClutter);
        }

        private static void BuildScrapBunker(Transform parent, Vector3 pos, IronFactoryArt art, Material scrapMat)
        {
            Transform bunker = Group("ScrapBunker", parent);
            bunker.position = pos;

            // 3-sided heavy concrete retaining walls
            const float bw = 5.5f, bd = 4.2f, bh = 1.8f, tw = .4f;
            Box("Wall_Back", bunker, new Vector3(0f, bh * .5f, bd * .5f - tw * .5f), new Vector3(bw, bh, tw), art.Concrete, true);
            Box("Wall_Left", bunker, new Vector3(-bw * .5f + tw * .5f, bh * .5f, 0f), new Vector3(tw, bh, bd), art.Concrete, true);
            Box("Wall_Right", bunker, new Vector3(bw * .5f - tw * .5f, bh * .5f, 0f), new Vector3(tw, bh, bd), art.Concrete, true);

            // Piled scrap steel offcuts, turned chips, and pipe segments inside
            Box("ScrapPileBase", bunker, new Vector3(0f, .5f, 0f), new Vector3(bw - 1.2f, 1f, bd - 1.2f), scrapMat, true);
            Box("ScrapMound", bunker, new Vector3(0f, 1.1f, .4f), new Vector3(bw - 2f, .8f, bd - 2f), art.Rust, false);
            // Some pipe segments sticking out
            GameObject pipe1 = Primitive(PrimitiveType.Cylinder, "Pipe_1", bunker, new Vector3(-.8f, 1.3f, 0f), new Vector3(.3f, 2.4f, .3f), art.Steel, false);
            pipe1.transform.localRotation = Quaternion.Euler(30f, 20f, 0f);
            GameObject pipe2 = Primitive(PrimitiveType.Cylinder, "Pipe_2", bunker, new Vector3(.9f, 1.2f, .5f), new Vector3(.25f, 2f, .25f), art.Rust, false);
            pipe2.transform.localRotation = Quaternion.Euler(40f, -35f, 0f);
        }

        private static void BuildDumpster(Transform parent, Vector3 pos, float yaw, IronFactoryArt art, Material dumpsterMat, Material scrapMat)
        {
            Transform d = Group("RollOffDumpster", parent);
            d.position = pos;
            d.rotation = Quaternion.Euler(0f, yaw, 0f);

            const float dw = 2.4f, dl = 5.2f, dh = 1.6f;
            // Dumpster tub body
            Box("TubFloor", d, Vector3.up * .15f, new Vector3(dw, .3f, dl), art.Steel, true);
            Box("TubSide_L", d, new Vector3(-dw * .5f + .08f, dh * .5f, 0f), new Vector3(.16f, dh, dl), dumpsterMat, true);
            Box("TubSide_R", d, new Vector3(dw * .5f - .08f, dh * .5f, 0f), new Vector3(.16f, dh, dl), dumpsterMat, true);
            Box("TubFront", d, new Vector3(0f, dh * .5f, dl * .5f - .08f), new Vector3(dw, dh, .16f), dumpsterMat, true);
            Box("TubBack", d, new Vector3(0f, dh * .5f, -dl * .5f + .08f), new Vector3(dw, dh, .16f), dumpsterMat, true);

            // External structural reinforcing channels
            for (float z = -dl * .4f; z <= dl * .4f; z += 1.2f)
            {
                Box("Rib_L", d, new Vector3(-dw * .5f - .04f, dh * .5f, z), new Vector3(.08f, dh, .12f), art.Rust, false);
                Box("Rib_R", d, new Vector3(dw * .5f + .04f, dh * .5f, z), new Vector3(.08f, dh, .12f), art.Rust, false);
            }

            // Scrap metal overflow inside the dumpster
            Box("DumpsterScrap", d, new Vector3(0f, dh * .65f, 0f), new Vector3(dw - .4f, .9f, dl - .5f), scrapMat, true);
        }

        private static void BuildCableSpool(Transform parent, Vector3 pos, bool tipped, IronFactoryArt art, Material woodMat)
        {
            Transform spool = Group(tipped ? "CableSpool_Tipped" : "CableSpool_Upright", parent);
            spool.position = pos;

            const float r = 1.2f, width = 1.3f;
            if (!tipped)
            {
                // Upright spool resting on both circular wood flanges
                foreach (float side in new[] { -width * .5f, width * .5f })
                    Primitive(PrimitiveType.Cylinder, "Flange", spool, new Vector3(0f, r, side),
                        new Vector3(r * 2f, .08f, r * 2f), woodMat, true).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                // Central drum wound with dark cable
                Primitive(PrimitiveType.Cylinder, "CableWinding", spool, new Vector3(0f, r, 0f),
                    new Vector3(r * 1.3f, width - .16f, r * 1.3f), art.Bed, true).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                // Tipped on its side flat on the asphalt
                spool.rotation = Quaternion.Euler(0f, 25f, 0f);
                Primitive(PrimitiveType.Cylinder, "FlangeBottom", spool, Vector3.up * .04f, new Vector3(r * 2f, .08f, r * 2f), woodMat, true);
                Primitive(PrimitiveType.Cylinder, "CableWinding", spool, Vector3.up * (width * .5f), new Vector3(r * 1.3f, width * .5f, r * 1.3f), art.Bed, true);
                Primitive(PrimitiveType.Cylinder, "FlangeTop", spool, Vector3.up * width, new Vector3(r * 2f, .08f, r * 2f), woodMat, true);
            }
        }

        private static void BuildJerseyBarrierRun(Transform parent, IronFactoryArt art)
        {
            Transform run = Group("JerseyBarriers", parent);
            // Placed along the edge of the dock approach
            float[] xCoords = { 24f, 27f, 30f, 33f };
            foreach (float x in xCoords)
            {
                Transform b = Group($"Barrier_{x:0}", run);
                b.localPosition = new Vector3(x, 0f, -22f);
                // Standard concrete K-rail / Jersey barrier profile
                Box("Base", b, Vector3.up * .2f, new Vector3(2.8f, .4f, .75f), art.Concrete, true);
                Box("Upper", b, Vector3.up * .6f, new Vector3(2.8f, .45f, .35f), art.Concrete, true);
                // Yellow safety reflective patch
                Box("Reflector", b, new Vector3(0f, .55f, .19f), new Vector3(.4f, .12f, .02f), art.Safety, false);
            }
        }

        private static void BuildParkedForklift(Transform parent, Vector3 pos, float yaw, IronFactoryArt art)
        {
            Transform fl = Group("ParkedForklift", parent);
            fl.position = pos;
            fl.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Chassis and counterweight
            Box("Chassis", fl, Vector3.up * .45f, new Vector3(1.3f, .5f, 2.2f), art.Rust, true);
            Box("Counterweight", fl, new Vector3(0f, .65f, -.9f), new Vector3(1.25f, .7f, .6f), art.Bed, true);

            // 4 heavy rubber tires
            foreach (float sx in new[] { -.65f, .65f })
            {
                Primitive(PrimitiveType.Cylinder, "Tire_F", fl, new Vector3(sx, .35f, .7f), new Vector3(.7f, .22f, .7f), art.Bed, true)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Primitive(PrimitiveType.Cylinder, "Tire_R", fl, new Vector3(sx, .3f, -.7f), new Vector3(.6f, .2f, .6f), art.Bed, true)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // Overhead protective roll cage
            Box("RollCageRoof", fl, Vector3.up * 2.1f, new Vector3(1.1f, .06f, 1.2f), art.Steel, false);
            foreach (float cx in new[] { -.5f, .5f })
            foreach (float cz in new[] { -.55f, .55f })
                Box("CagePost", fl, new Vector3(cx, 1.4f, cz), new Vector3(.06f, 1.4f, .06f), art.Safety, false);

            // Front vertical lifting mast
            Transform mast = Group("LiftMast", fl);
            mast.localPosition = new Vector3(0f, 1.1f, 1.15f);
            foreach (float mx in new[] { -.45f, .45f })
                Box("MastRail", mast, new Vector3(mx, 0f, 0f), new Vector3(.1f, 2.2f, .1f), art.Steel, false);
            Box("ForkCarriage", mast, Vector3.down * .85f, new Vector3(1.1f, .3f, .12f), art.Steel, false);

            // Heavy forks resting flat on the asphalt
            foreach (float fx in new[] { -.3f, .3f })
                Box("ForkTine", fl, new Vector3(fx, .04f, 1.8f), new Vector3(.12f, .05f, 1.2f), art.Rust, false);
        }
    }
}

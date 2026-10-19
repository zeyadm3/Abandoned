using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The elevated Rolling Mill Control Pulpit on catwalk tier 2 (10 m above the casting floor).
    /// In heavy iron and steel mills, the pulpit is the armored cabin where operators sat above the heat
    /// and flying scale to control blooming stands, reversing rolls, and shearing tables.
    /// Features slanted heat-shield observation windows, dual-seat control consoles with lever banks,
    /// analog pressure gauges, CRT monitor mimic displays, and emergency trip switches.
    /// </summary>
    public static class IronFactoryPulpit
    {
        private static readonly Vector3 Center = new(99f, 10f, 74f);
        private const float Width = 6f;   // along X
        private const float Depth = 4.4f; // along Z
        private const float Height = 3.2f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform pulpit = Group("ControlPulpit", root);
            pulpit.position = Center;

            Material consoleMetal = GetMaterial("IronFactory_ConsoleSteel", new Color(.22f, .24f, .26f));
            Material windowGlass = GetMaterial("IronFactory_PulpitGlass", new Color(.18f, .32f, .28f));
            Material screenGreen = GetMaterial("IronFactory_ScreenGreen", new Color(.1f, .45f, .2f));
            screenGreen.SetColor("_EmissionColor", new Color(.15f, .8f, .35f));
            screenGreen.EnableKeyword("_EMISSION");

            // Outer structural diagonal support braces under the cabin floor (from 7 m up to 10 m)
            foreach (float xOff in new[] { -2.2f, 2.2f })
            {
                GameObject brace = Box("UnderBrace", pulpit, new Vector3(xOff, -1.5f, 0f), new Vector3(.25f, 3.2f, .25f), art.Steel, false);
                brace.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            }

            // Floor slab
            Box("Floor", pulpit, Vector3.up * .08f, new Vector3(Width, .16f, Depth), art.Steel, true);

            // Ceiling / roof
            Box("Roof", pulpit, Vector3.up * Height, new Vector3(Width + .4f, .2f, Depth + .4f), art.Steel, true);

            // North and East solid walls
            Box("Wall_N", pulpit, new Vector3(0f, Height * .5f, Depth * .5f - .1f), new Vector3(Width, Height, .2f), art.Steel, true);
            Box("Wall_E", pulpit, new Vector3(Width * .5f - .1f, Height * .5f, 0f), new Vector3(.2f, Height, Depth), art.Steel, true);

            // South wall with doorway connecting to the tier-2 catwalk
            float doorW = 1.4f, doorH = 2.4f;
            float sLeft = (Width - doorW) * .5f;
            Box("Wall_S_L", pulpit, new Vector3(-Width * .5f + sLeft * .5f, Height * .5f, -Depth * .5f + .1f),
                new Vector3(sLeft, Height, .2f), art.Steel, true);
            Box("Wall_S_R", pulpit, new Vector3(Width * .5f - sLeft * .5f, Height * .5f, -Depth * .5f + .1f),
                new Vector3(sLeft, Height, .2f), art.Steel, true);
            Box("Wall_S_Lintel", pulpit, new Vector3(0f, doorH + (Height - doorH) * .5f, -Depth * .5f + .1f),
                new Vector3(doorW, Height - doorH, .2f), art.Steel, true);

            // West observation wall: half-wall bulkhead + slanted viewing windows looking over the plant floor
            float sillH = 1.1f;
            Box("Wall_W_Bulkhead", pulpit, new Vector3(-Width * .5f + .1f, sillH * .5f, 0f),
                new Vector3(.2f, sillH, Depth), art.Steel, true);
            Box("Wall_W_Header", pulpit, new Vector3(-Width * .5f + .1f, Height - .25f, 0f),
                new Vector3(.2f, .5f, Depth), art.Steel, true);

            // Angled observation window frame and glass panes
            for (float zOff = -Depth * .4f; zOff <= Depth * .4f; zOff += 1.4f)
            {
                Box("WindowMullion", pulpit, new Vector3(-Width * .5f + .1f, sillH + (Height - sillH - .5f) * .5f, zOff),
                    new Vector3(.18f, Height - sillH - .5f, .12f), art.Steel, false);
            }
            Box("WindowGlass", pulpit, new Vector3(-Width * .5f + .1f, sillH + (Height - sillH - .5f) * .5f, 0f),
                new Vector3(.05f, Height - sillH - .5f, Depth - .4f), windowGlass, false);

            // --- Inside the Pulpit ---
            // Main operator console table facing west toward the observation windows
            Transform console = Group("MainConsole", pulpit);
            console.localPosition = new Vector3(-Width * .5f + 1.1f, .55f, 0f);
            Box("DeskBase", console, Vector3.zero, new Vector3(1.2f, .9f, Depth - 1.2f), consoleMetal, true);
            Box("DeskSlant", console, new Vector3(.1f, .52f, 0f), new Vector3(.9f, .15f, Depth - 1.3f), consoleMetal, false);

            // Control levers and dial gauges on the desk
            for (float z = -1.2f; z <= 1.2f; z += .8f)
            {
                Box("LeverStand", console, new Vector3(-.2f, .65f, z), new Vector3(.2f, .12f, .25f), art.Steel, false);
                GameObject lever = Box("LeverGrip", console, new Vector3(-.2f, .82f, z), new Vector3(.04f, .26f, .04f), art.Safety, false);
                lever.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

                // Twin circular dial gauges
                Primitive(PrimitiveType.Cylinder, "Gauge_1", console, new Vector3(.25f, .75f, z - .15f),
                    new Vector3(.22f, .04f, .22f), art.Safety, false).transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
                Primitive(PrimitiveType.Cylinder, "Gauge_2", console, new Vector3(.25f, .75f, z + .15f),
                    new Vector3(.22f, .04f, .22f), art.Safety, false).transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            }

            // Dual CRT monitor screens on the console
            foreach (float z in new[] { -.7f, .7f })
            {
                Transform monitor = Group($"Monitor_{z:0.0}", console);
                monitor.localPosition = new Vector3(.35f, .95f, z);
                Box("Housing", monitor, Vector3.zero, new Vector3(.45f, .4f, .5f), consoleMetal, false);
                Box("Screen", monitor, new Vector3(-.22f, 0f, 0f), new Vector3(.03f, .32f, .42f), screenGreen, false);
            }

            // Two swivel operator chairs
            foreach (float z in new[] { -.8f, .8f })
            {
                Transform chair = Group($"Chair_{z:0.0}", pulpit);
                chair.localPosition = new Vector3(-Width * .5f + 2.2f, 0f, z);
                Primitive(PrimitiveType.Cylinder, "Pedestal", chair, Vector3.up * .25f, new Vector3(.4f, .5f, .4f), art.Steel, false);
                Box("Seat", chair, Vector3.up * .55f, new Vector3(.5f, .1f, .5f), art.Bed, false);
                Box("Backrest", chair, new Vector3(.22f, .85f, 0f), new Vector3(.1f, .5f, .48f), art.Bed, false);
            }

            // Wall-mounted Emergency Stop trip switch box on North wall
            Transform eStop = Group("EmergencyStopBox", pulpit);
            eStop.localPosition = new Vector3(1.2f, 1.6f, Depth * .5f - .25f);
            Box("Box", eStop, Vector3.zero, new Vector3(.55f, .65f, .22f), art.Safety, false);
            Primitive(PrimitiveType.Cylinder, "RedButton", eStop, Vector3.forward * .14f, new Vector3(.18f, .1f, .18f), art.Rust, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            // Overhead warm fluorescent desk light
            Transform lightRig = Group("CabinLight", pulpit);
            lightRig.localPosition = new Vector3(-.2f, Height - .3f, 0f);
            Box("Fixture", lightRig, Vector3.zero, new Vector3(.35f, .12f, 1.8f), art.Steel, false);
            Box("Tube", lightRig, Vector3.down * .08f, new Vector3(.15f, .06f, 1.6f), art.Lamp, false);
            Light bulb = lightRig.gameObject.AddComponent<Light>();
            bulb.type = LightType.Point;
            bulb.range = 5.5f;
            bulb.intensity = 1.4f;
            bulb.color = new Color(1f, .92f, .78f);
            bulb.shadows = LightShadows.None;

            pulpit.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(pulpit);
        }
    }
}

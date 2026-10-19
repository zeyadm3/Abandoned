using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The historic Cargo Train Station in front of the Iron Factory (Docs/IRON_FACTORY_PLAN.md).
    /// Before the plant was abandoned and marked for demolition, all cast iron, steel billets, and heavy
    /// machinery parts manufactured inside were hauled out onto these raised platforms and loaded directly
    /// onto freight trains.
    /// Now, the plant is dead and scheduled for salvage: the cargo station, cantilevered canopies, loading
    /// gantry hoist, platform scales, and dual tracks remain, but there is NO train — only rusted empty
    /// tracks with buffer stops, cancelled shipment boards, and forgotten freight dollies.
    /// </summary>
    public static class IronFactoryCargoStation
    {
        private const float Gauge = 1.435f;
        private const float PlatformDepth = 5f;
        private const float CanopyHeight = 5.2f;

        // Dual through-tracks running in front of the platform
        private const float Track1Z = -7.5f;
        private const float Track2Z = -10.5f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform station = Group("CargoTrainStation", root);

            Material ballast = GetMaterial("IronFactory_Ballast", new Color(.16f, .15f, .14f));
            Material oakDunnage = GetMaterial("IronFactory_OakDunnage", new Color(.32f, .24f, .16f));
            Material stationSignMat = GetMaterial("IronFactory_StationSign", new Color(.12f, .18f, .15f));
            Material scaleBronze = GetMaterial("IronFactory_ScaleBronze", new Color(.65f, .55f, .28f));

            // Central opening for the truck haul route: x in [53, 67]
            float westStart = 4f, westEnd = L.DockX - 7f;   // 4 to 53 m
            float eastStart = L.DockX + 7f, eastEnd = L.Width * L.Tile - 4f; // 67 to 116 m

            // 1. Raised West & East Freight Platforms
            BuildPlatform(station, "WestPlatform", westStart, westEnd, art, oakDunnage);
            BuildPlatform(station, "EastPlatform", eastStart, eastEnd, art, oakDunnage);

            // 2. Cantilevered Weatherproof Steel Station Canopies over platforms
            BuildCanopy(station, "WestCanopy", westStart, westEnd, art);
            BuildCanopy(station, "EastCanopy", eastStart, eastEnd, art);

            // 3. Dual Empty Rail Tracks (No train — disused and rusted)
            BuildEmptyTrack(station, "Track_PlatformFace", Track1Z, 0f, L.Width * L.Tile, art, ballast);
            BuildEmptyTrack(station, "Track_PassingSiding", Track2Z, 0f, L.Width * L.Tile, art, ballast);

            // 4. Timber crossing planks where the truck and crew cross the tracks
            BuildTrackCrossing(station, L.DockX - 6.5f, L.DockX + 6.5f, art);

            // 5. Overhead Loading Gantry Hoist on West Platform (x = 28)
            BuildLoadingGantry(station, new Vector3(28f, 0f, -4.5f), art);

            // 6. Inset Heavy Platform Cargo Scale on West Platform (x = 44)
            BuildPlatformScale(station, new Vector3(44f, .02f, -3.2f), art, scaleBronze);

            // 7. Abandoned Freight Dollies & Hand Trucks
            BuildFreightDolly(station, new Vector3(36f, .02f, -2.5f), 15f, art);
            BuildFreightDolly(station, new Vector3(82f, .02f, -3.2f), -35f, art);
            BuildHandTruck(station, new Vector3(98f, .02f, -1.2f), 70f, art);

            // 8. Station Identity Signage & Cancelled Shipment Slate
            BuildStationSignage(station, new Vector3(40f, CanopyHeight - .5f, -5.2f), stationSignMat, art);
            BuildDispatchSlate(station, new Vector3(westEnd - .2f, 1.8f, -2f), art);

            // 9. Central dock threshold plate and edge
            Box("DockPlate", station, new Vector3(L.DockX, .015f, 0f), new Vector3(L.DockWidth - .4f, .03f, 1.6f), art.Steel, false);
            Box("DockEdge", station, new Vector3(L.DockX, .035f, -.85f), new Vector3(L.DockWidth - .4f, .02f, .12f), art.Safety, false);

            station.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            MarkStatic(station);
        }

        private static void BuildPlatform(Transform parent, string name, float fromX, float toX, IronFactoryArt art, Material dunnageMat)
        {
            Transform p = Group(name, parent);
            float length = toX - fromX;
            float midX = (fromX + toX) * .5f;
            float midZ = -PlatformDepth * .5f; // -2.5m

            // Main platform deck flush with factory floor (y = 0)
            Box("PlatformDeck", p, new Vector3(midX, -.1f, midZ), new Vector3(length, .2f, PlatformDepth), art.Concrete, true);

            // Track-facing concrete retaining wall with heavy curb coping
            Box("TrackFacingCurb", p, new Vector3(midX, -.2f, -PlatformDepth + .15f), new Vector3(length, .5f, .3f), art.Concrete, true);
            // Yellow safety warning strip along the platform edge
            Box("SafetyEdge", p, new Vector3(midX, .01f, -PlatformDepth + .12f), new Vector3(length, .02f, .22f), art.Safety, false);

            // Sloped equipment ramps on outer ends for rolling freight dollies up from the yard
            float rampX = fromX < 50f ? fromX + 2f : toX - 2f;
            Box("EndRamp", p, new Vector3(rampX, -.12f, midZ), new Vector3(4f, .2f, PlatformDepth - .6f), art.Concrete, true);

            // Oak staging dunnage timbers where iron ingots were once stacked for loading
            int dunnageIdx = 0;
            for (float x = fromX + 6f; x < toX - 6f; x += 12f)
            {
                Box("Dunnage_A", p, new Vector3(x, .06f, -2.2f), new Vector3(.25f, .12f, 2.6f), dunnageMat, false);
                Box("Dunnage_B", p, new Vector3(x + 1.4f, .06f, -2.2f), new Vector3(.25f, .12f, 2.6f), dunnageMat, false);
                if (dunnageIdx % 2 == 0) BuildStagedIngots(p, new Vector3(x + .7f, .18f, -2.2f), art);
                else BuildStagedCoils(p, new Vector3(x + .7f, .36f, -2.2f), art);
                dunnageIdx++;
            }
        }

        private static void BuildCanopy(Transform parent, string name, float fromX, float toX, IronFactoryArt art)
        {
            Transform canopy = Group(name, parent);
            float length = toX - fromX;
            float midX = (fromX + toX) * .5f;

            // Cantilevered corrugated steel roof sloping slightly back
            Box("RoofSlab", canopy, new Vector3(midX, CanopyHeight, -3.2f), new Vector3(length + .6f, .15f, 6.2f), art.Steel, true);

            // Structural support portal posts along the platform edge every 8m
            for (float x = fromX + 4f; x < toX; x += 8f)
            {
                Box("CanopyPost", canopy, new Vector3(x, CanopyHeight * .5f, -PlatformDepth + .3f),
                    new Vector3(.25f, CanopyHeight, .25f), art.Steel, true);
                // Angled knee-brace to roof truss
                GameObject brace = Box("KneeBrace", canopy, new Vector3(x, CanopyHeight - .6f, -PlatformDepth + .8f),
                    new Vector3(.12f, 1.2f, .12f), art.Steel, false);
                brace.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);

                // Industrial hanging gooseneck station lamps under the canopy eave
                Transform lamp = Group("PlatformLamp", canopy);
                lamp.localPosition = new Vector3(x, CanopyHeight - .35f, -PlatformDepth);
                Box("Arm", lamp, Vector3.zero, new Vector3(.06f, .06f, .6f), art.Steel, false);
                Box("Shade", lamp, new Vector3(0f, -.12f, .3f), new Vector3(.4f, .15f, .4f), art.Steel, false);
                Box("Bulb", lamp, new Vector3(0f, -.2f, .3f), new Vector3(.18f, .08f, .18f), art.Lamp, false);
            }
        }

        private static void BuildEmptyTrack(Transform parent, string name, float trackZ, float fromX, float toX, IronFactoryArt art, Material ballast)
        {
            Transform track = Group(name, parent);
            float length = toX - fromX;
            float midX = (fromX + toX) * .5f;

            // Continuous ballast stone bed
            Box("BallastBed", track, new Vector3(midX, -.16f, trackZ), new Vector3(length, .1f, Gauge + 1.2f), ballast, false);

            // Running steel rails
            foreach (float side in new[] { -1f, 1f })
                Box("Rail", track, new Vector3(midX, -.05f, trackZ + side * Gauge * .5f),
                    new Vector3(length, .12f, .07f), art.Steel, false);

            // Creosote timber ties (sleepers) spaced along the run
            for (float x = fromX + .4f; x < toX; x += .9f)
                Box("Tie", track, new Vector3(x, -.12f, trackZ), new Vector3(.22f, .08f, Gauge + .9f), art.Rust, false);

            // Buffer stops at track ends
            foreach (float endX in new[] { fromX + 1f, toX - 1f })
            {
                Transform buffer = Group("BufferStop", track);
                buffer.localPosition = new Vector3(endX, .35f, trackZ);
                Box("BufferBlock", buffer, Vector3.zero, new Vector3(.7f, .85f, Gauge + .5f), art.Safety, true);
                Box("TargetDisc", buffer, new Vector3(endX < 50f ? .38f : -.38f, 0f, 0f), new Vector3(.04f, .45f, .45f), art.Rust, false);
            }
        }

        private static void BuildTrackCrossing(Transform parent, float fromX, float toX, IronFactoryArt art)
        {
            Transform crossing = Group("TrackCrossingPlanks", parent);
            float len = toX - fromX, midX = (fromX + toX) * .5f;

            // Wooden flangeway planks flush with the rail tops for truck and crew crossing
            foreach (float z in new[] { Track1Z, Track2Z })
            {
                Box("CentrePlank", crossing, new Vector3(midX, -.04f, z), new Vector3(len, .08f, Gauge - .16f), art.Rust, false);
                Box("OuterPlank_N", crossing, new Vector3(midX, -.04f, z + Gauge * .5f + .3f), new Vector3(len, .08f, .5f), art.Rust, false);
                Box("OuterPlank_S", crossing, new Vector3(midX, -.04f, z - Gauge * .5f - .3f), new Vector3(len, .08f, .5f), art.Rust, false);
            }
        }

        private static void BuildLoadingGantry(Transform parent, Vector3 pos, IronFactoryArt art)
        {
            Transform gantry = Group("LoadingGantryCrane", parent);
            gantry.position = pos;

            // Overhead I-beam projecting out over Track 1
            Box("HoistBeam", gantry, new Vector3(0f, CanopyHeight - .15f, -1.8f), new Vector3(.3f, .45f, 4.5f), art.Safety, false);

            // Motorized hoist trolley parked on the beam
            Transform trolley = Group("HoistTrolley", gantry);
            trolley.localPosition = new Vector3(0f, CanopyHeight - .4f, -2.8f);
            Box("Carriage", trolley, Vector3.zero, new Vector3(.6f, .35f, .55f), art.Steel, false);
            Box("Cable", trolley, Vector3.down * 1f, new Vector3(.04f, 2f, .04f), art.Steel, false);

            // Spreader lifting bar with sling chains for hoisting iron ingots & billets
            Transform spreader = Group("LiftingSpreader", trolley);
            spreader.localPosition = Vector3.down * 2.1f;
            Box("Beam", spreader, Vector3.zero, new Vector3(2.2f, .18f, .18f), art.Safety, false);
            foreach (float side in new[] { -.9f, .9f })
                Box("Hook", spreader, new Vector3(side, -.35f, 0f), new Vector3(.1f, .55f, .12f), art.Rust, false);
        }

        private static void BuildPlatformScale(Transform parent, Vector3 pos, IronFactoryArt art, Material dialMat)
        {
            Transform scale = Group("PlatformFreightScale", parent);
            scale.position = pos;

            // Inset cast iron weighing platform
            Box("WeighPlate", scale, Vector3.zero, new Vector3(1.8f, .04f, 1.8f), art.Bed, false);
            Box("PlateRim", scale, Vector3.zero, new Vector3(1.95f, .02f, 1.95f), art.Steel, false);

            // Dial scale indicator column
            Transform dial = Group("ScaleColumn", scale);
            dial.localPosition = new Vector3(0f, 0f, 1.1f);
            Box("Pillar", dial, Vector3.up * .65f, new Vector3(.25f, 1.3f, .25f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "DialHead", dial, Vector3.up * 1.55f, new Vector3(.65f, .12f, .65f), dialMat, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildFreightDolly(Transform parent, Vector3 pos, float yaw, IronFactoryArt art)
        {
            Transform dolly = Group("FreightDolly", parent);
            dolly.position = pos;
            dolly.rotation = Quaternion.Euler(0f, yaw, 0f);

            // Low heavy steel deck on 4 cast iron wheels
            Box("Platform", dolly, Vector3.up * .22f, new Vector3(1.2f, .08f, 2.2f), art.Rust, true);
            foreach (float sx in new[] { -.55f, .55f })
            foreach (float sz in new[] { -.85f, .85f })
                Primitive(PrimitiveType.Cylinder, "Wheel", dolly, new Vector3(sx, .11f, sz),
                    new Vector3(.22f, .06f, .22f), art.Steel, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Push handle upright
            Box("HandleBar", dolly, new Vector3(0f, .85f, -1.05f), new Vector3(1.1f, .05f, .05f), art.Steel, false);
            foreach (float sx in new[] { -.5f, .5f })
                Box("HandlePost", dolly, new Vector3(sx, .52f, -1.05f), new Vector3(.05f, .65f, .05f), art.Steel, false);
        }

        private static void BuildHandTruck(Transform parent, Vector3 pos, float yaw, IronFactoryArt art)
        {
            Transform ht = Group("TwoWheelHandTruck", parent);
            ht.position = pos;
            ht.rotation = Quaternion.Euler(15f, yaw, 0f); // Leaning against post

            Box("Frame_L", ht, new Vector3(-.22f, .7f, 0f), new Vector3(.04f, 1.4f, .04f), art.Safety, false);
            Box("Frame_R", ht, new Vector3(.22f, .7f, 0f), new Vector3(.04f, 1.4f, .04f), art.Safety, false);
            Box("NosePlate", ht, new Vector3(0f, .04f, .22f), new Vector3(.5f, .04f, .4f), art.Steel, false);
            foreach (float sx in new[] { -.28f, .28f })
                Primitive(PrimitiveType.Cylinder, "Wheel", ht, new Vector3(sx, .12f, 0f),
                    new Vector3(.24f, .06f, .24f), art.Bed, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private static void BuildStationSignage(Transform parent, Vector3 pos, Material boardMat, IronFactoryArt art)
        {
            Transform sign = Group("StationNameSign", parent);
            sign.position = pos;

            // Long suspended enamel station sign board
            Box("SignBoard", sign, Vector3.zero, new Vector3(6.5f, .85f, .08f), boardMat, false);
            Box("SignBorder", sign, Vector3.zero, new Vector3(6.65f, .95f, .04f), art.Safety, false);

            // Hanger chains to canopy
            foreach (float x in new[] { -2.4f, 2.4f })
                Box("HangerRod", sign, Vector3.up * .45f + Vector3.right * x, new Vector3(.03f, .85f, .03f), art.Steel, false);
        }

        private static void BuildDispatchSlate(Transform parent, Vector3 pos, IronFactoryArt art)
        {
            Transform slate = Group("CancelledDispatchBoard", parent);
            slate.position = pos;

            // Wall-mounted chalkboard slate with cancelled shipping notices
            Box("Board", slate, Vector3.zero, new Vector3(.1f, 1.6f, 1.2f), art.Bed, false);
            Box("Frame", slate, Vector3.zero, new Vector3(.12f, 1.75f, 1.35f), art.Rust, false);
        }

        private static void BuildStagedIngots(Transform parent, Vector3 pos, IronFactoryArt art)
        {
            Transform stack = Group("StagedPigIron", parent);
            stack.localPosition = pos;
            Box("Ingots_Lower", stack, Vector3.zero, new Vector3(1.1f, .16f, 2.1f), art.Rust, true);
            Box("Ingots_Upper", stack, Vector3.up * .16f, new Vector3(.9f, .14f, 1.8f), art.Rust, true);
            foreach (float z in new[] { -.6f, .6f })
                Box("SteelBand", stack, new Vector3(0f, .08f, z), new Vector3(1.14f, .34f, .03f), art.Steel, false);
        }

        private static void BuildStagedCoils(Transform parent, Vector3 pos, IronFactoryArt art)
        {
            Transform coils = Group("StagedSteelCoils", parent);
            coils.localPosition = pos;
            foreach (float z in new[] { -.55f, .55f })
            {
                GameObject coil = Primitive(PrimitiveType.Cylinder, "Coil", coils, new Vector3(0f, 0f, z),
                    new Vector3(.72f, .45f, .72f), art.Rust, true);
                coil.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }
    }
}

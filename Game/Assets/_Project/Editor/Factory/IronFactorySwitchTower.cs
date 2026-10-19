using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The historic 2-storey Railway Interlocking Switch Tower and track signaling infrastructure
    /// overlooking the Cargo Train Station tracks (x = 114, z = -14).
    /// Before the plant was abandoned, the switchman worked in this elevated cabin, throwing manual
    /// mechanical levers to route freight trains carrying molten iron pigs and steel coils from the
    /// blast furnace onto the mainline.
    /// Now, the signals stand dark, the levers are locked in place, and the empty tracks lead nowhere.
    /// </summary>
    public static class IronFactorySwitchTower
    {
        private static readonly Vector3 Center = new(114f, 0f, -14.5f);

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform tower = Group("SwitchTower", root);
            tower.position = Center;

            Material brickMat = GetMaterial("IronFactory_TowerBrick", new Color(.34f, .22f, .18f));
            Material woodCabin = GetMaterial("IronFactory_TowerWood", new Color(.24f, .22f, .2f));
            Material cabinWindow = GetMaterial("IronFactory_TowerGlass", new Color(.18f, .22f, .24f));
            Material signalHeadMat = GetMaterial("IronFactory_SignalDark", new Color(.1f, .1f, .1f));

            // 1. Concrete foundation footing
            Box("Foundation", tower, Vector3.up * .2f, new Vector3(4.6f, .4f, 4.2f), art.Concrete, true);

            // 2. Ground floor brick equipment/relay house (h = 3.2m)
            Transform relayFloor = Group("RelayFloor", tower);
            relayFloor.localPosition = Vector3.up * .4f;
            Box("BrickWalls", relayFloor, Vector3.up * 1.6f, new Vector3(4.2f, 3.2f, 3.8f), brickMat, true);
            // Steel ground access door facing North (toward platform)
            Box("SteelDoor", relayFloor, new Vector3(0f, 1.1f, 1.92f), new Vector3(1.1f, 2.2f, .06f), art.Steel, false);
            // Louvered ventilation grille
            Box("VentLouver", relayFloor, new Vector3(1.4f, 2.4f, 1.92f), new Vector3(.8f, .6f, .04f), art.Bed, false);

            // 3. Upper interlocking operator cabin (h = 3.0m, y = 3.6m to 6.6m)
            Transform cabin = Group("InterlockingCabin", tower);
            cabin.localPosition = Vector3.up * 3.6f;

            // Cabin floor deck cantilevered slightly
            Box("CabinFloor", cabin, Vector3.up * .1f, new Vector3(4.8f, .2f, 4.4f), woodCabin, true);
            // Lower wainscot wall
            Box("Wainscot", cabin, Vector3.up * .6f, new Vector3(4.6f, .8f, 4.2f), woodCabin, true);

            // Wrap-around multi-pane observation windows offering 360 overview of the train tracks
            Box("Window_N", cabin, new Vector3(0f, 1.8f, 2.11f), new Vector3(4.2f, 1.6f, .05f), cabinWindow, false);
            Box("Window_W", cabin, new Vector3(-2.31f, 1.8f, 0f), new Vector3(.05f, 1.6f, 3.8f), cabinWindow, false);
            Box("Window_S", cabin, new Vector3(0f, 1.8f, -2.11f), new Vector3(4.2f, 1.6f, .05f), cabinWindow, false);
            Box("Window_E", cabin, new Vector3(2.31f, 1.8f, -.6f), new Vector3(.05f, 1.6f, 2.6f), cabinWindow, false);

            // Corner structural mullions
            foreach (float sx in new[] { -2.3f, 2.3f })
            foreach (float sz in new[] { -2.1f, 2.1f })
                Box("Mullion", cabin, new Vector3(sx, 1.8f, sz), new Vector3(.18f, 1.6f, .18f), woodCabin, false);

            // Overhanging hipped steel roof with cast iron stove chimney pipe
            Box("RoofSlab", cabin, Vector3.up * 2.8f, new Vector3(5.2f, .3f, 4.8f), art.Steel, true);
            Primitive(PrimitiveType.Cylinder, "ChimneyPipe", cabin, new Vector3(1.2f, 3.6f, -1f),
                new Vector3(.2f, 1.6f, .2f), art.Rust, false);

            // Cabin interior: mechanical lever frame & schematic track diagram
            Transform frame = Group("LeverFrame", cabin);
            frame.localPosition = new Vector3(0f, 1f, 1.4f);
            Box("FrameBase", frame, Vector3.zero, new Vector3(2.4f, .5f, .4f), art.Bed, false);
            for (int i = 0; i < 6; i++)
            {
                Material leverMat = (i % 2 == 0) ? art.Safety : art.Steel;
                GameObject lev = Box("Lever", frame, new Vector3(-.9f + i * .36f, .4f, 0f),
                    new Vector3(.05f, .7f, .05f), leverMat, false);
                lev.transform.localRotation = Quaternion.Euler(-18f + i * 5f, 0f, 0f);
            }

            // Suspended illuminated track model board
            Box("TrackModelBoard", cabin, new Vector3(0f, 2.2f, .4f), new Vector3(2.2f, .65f, .08f), art.Bed, false);

            // 4. Exterior steel access stairway leading to upper cabin door
            Transform stairs = Group("ExteriorStairs", tower);
            stairs.localPosition = new Vector3(2.6f, 0f, 0f);
            Box("LandingDeck", stairs, new Vector3(0f, 3.6f, 1f), new Vector3(1.2f, .12f, 1.4f), art.Grating, true);
            for (int step = 0; step < 12; step++)
            {
                float t = step / 12f;
                Box("Step", stairs, new Vector3(0f, t * 3.5f, 1f - t * 3.2f), new Vector3(1f, .08f, .28f), art.Steel, false);
            }

            // 5. Trackside Searchlight Railway Block Signal (x = 111, z = -12.5)
            Transform signal = Group("SearchlightSignal", tower);
            signal.position = new Vector3(111f, 0f, -12.5f);
            Box("SignalBase", signal, Vector3.up * .3f, new Vector3(.6f, .6f, .6f), art.Concrete, true);
            Box("SignalMast", signal, Vector3.up * 3.5f, new Vector3(.18f, 6.4f, .18f), art.Steel, false);
            // 2-head signal target roundels with dark unlit lenses
            foreach (float sy in new[] { 5.2f, 6.4f })
            {
                Transform head = Group("SignalHead", signal);
                head.localPosition = Vector3.up * sy;
                Box("TargetRoundel", head, new Vector3(0f, 0f, .15f), new Vector3(.85f, .85f, .04f), signalHeadMat, false);
                Box("HoodVisor", head, new Vector3(0f, .3f, .32f), new Vector3(.3f, .08f, .35f), signalHeadMat, false);
                Primitive(PrimitiveType.Cylinder, "DarkLens", head, new Vector3(0f, 0f, .22f),
                    new Vector3(.3f, .06f, .3f), signalHeadMat, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            // 6. Manual Ground-Throw Railway Switch Stands with Target Discs
            BuildSwitchStand(tower, new Vector3(106f, 0f, -8.8f), art);
            BuildSwitchStand(tower, new Vector3(22f, 0f, -8.8f), art);

            // 7. Trackside instrument relay case (silver aluminum box)
            Transform relayCase = Group("RelayCase", tower);
            relayCase.position = new Vector3(109f, 0f, -12.2f);
            Box("CasePier", relayCase, Vector3.up * .2f, new Vector3(.8f, .4f, .5f), art.Concrete, true);
            Box("InstrumentBox", relayCase, Vector3.up * 1f, new Vector3(.9f, 1.2f, .6f), art.Steel, true);

            tower.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            MarkStatic(tower);
        }

        private static void BuildSwitchStand(Transform parent, Vector3 worldPos, IronFactoryArt art)
        {
            Transform stand = Group("GroundThrowSwitchStand", parent);
            stand.position = worldPos;

            // Heavy tie extension plate
            Box("MountPlate", stand, Vector3.up * .02f, new Vector3(.3f, .04f, 1.4f), art.Steel, false);
            // Switch stand vertical shaft column
            Box("Shaft", stand, Vector3.up * .6f, new Vector3(.08f, 1.2f, .08f), art.Rust, false);
            // Weighted throw lever arm
            GameObject lever = Box("ThrowLever", stand, new Vector3(0f, .3f, .35f), new Vector3(.06f, .06f, .7f), art.Steel, false);
            lever.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            Primitive(PrimitiveType.Cylinder, "CounterWeight", lever.transform, new Vector3(0f, 0f, .35f),
                new Vector3(.2f, .2f, .2f), art.Rust, false);

            // Circular target disc indicator (red/yellow)
            Box("TargetDisc", stand, Vector3.up * 1.15f, new Vector3(.02f, .4f, .4f), art.Safety, false);
            // Square kerosene switch lantern atop the shaft
            Box("Lantern", stand, Vector3.up * 1.38f, new Vector3(.18f, .22f, .18f), art.Rust, false);
        }
    }
}

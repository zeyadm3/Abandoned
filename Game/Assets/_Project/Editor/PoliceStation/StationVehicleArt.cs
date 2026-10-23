using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The materials every station vehicle shares (glass, trim, tyres, lenses, chrome), made once per station in its own
    /// material folder through <see cref="StationArt.Plain"/>, and a handful of weathered paints for parked cars.
    /// </summary>
    public sealed class StationVehicleArt
    {
        public readonly Material Glass, DustyGlass, Crazed, Trim, Tyre, Chrome, SteelWheel, Rust, Well, Under, Seat;
        public readonly Material HeadLens, TailLens, Amber, BlueLens, Plate, Livery, Burnt, Dust;
        public readonly Material[] Paints;

        private static StationArt last;
        private static StationVehicleArt cached;

        public static StationVehicleArt For(StationArt a)
        {
            if (cached == null || last != a)
            {
                cached = new StationVehicleArt(a);
                last = a;
            }
            return cached;
        }

        private StationVehicleArt(StationArt a)
        {
            Glass = a.Plain("CarGlass", new Color(.05f, .065f, .075f), .93f, .2f);
            DustyGlass = a.Plain("CarGlassDusty", new Color(.24f, .24f, .22f), .35f, .05f);
            Crazed = a.Plain("CarGlassCrazed", new Color(.42f, .45f, .46f), .55f, .1f);
            Trim = a.Plain("CarTrim", new Color(.045f, .045f, .05f), .3f);
            Tyre = a.Plain("CarTyre", new Color(.055f, .055f, .058f), .15f);
            Chrome = a.Plain("CarChrome", new Color(.8f, .81f, .82f), .88f, .95f);
            SteelWheel = a.Plain("CarSteelWheel", new Color(.32f, .33f, .34f), .45f, .7f);
            Rust = a.Plain("CarRust", new Color(.3f, .16f, .08f), .2f, .3f);
            Well = a.Plain("CarWell", new Color(.02f, .02f, .02f), .1f);
            Under = a.Plain("CarUnder", new Color(.05f, .045f, .04f), .1f);
            Seat = a.Plain("CarSeat", new Color(.12f, .11f, .1f), .2f);
            HeadLens = a.Plain("CarHeadLens", new Color(.82f, .85f, .86f), .95f, .1f);
            TailLens = a.Plain("CarTailLens", new Color(.5f, .03f, .03f), .9f);
            Amber = a.Plain("CarAmberLens", new Color(.82f, .42f, .05f), .9f);
            BlueLens = a.Plain("CarBlueLens", new Color(.05f, .16f, .7f), .9f);
            Plate = a.Plain("CarPlate", new Color(.84f, .83f, .76f), .45f, .3f);
            Livery = a.Plain("CarLiveryWhite", new Color(.86f, .87f, .86f), .6f);
            Burnt = a.Plain("CarBurntShell", new Color(.08f, .065f, .055f), .08f, .25f);
            Dust = a.Plain("CarRoofDust", new Color(.44f, .41f, .36f), .05f);
            Paints = new[]
            {
                a.Plain("PaintMaroon", new Color(.3f, .06f, .07f), .55f, .35f), a.Plain("PaintNavy", new Color(.08f, .12f, .22f), .55f, .35f),
                a.Plain("PaintBeige", new Color(.62f, .55f, .42f), .45f, .2f), a.Plain("PaintSilver", new Color(.55f, .57f, .58f), .65f, .6f),
                a.Plain("PaintForest", new Color(.12f, .22f, .15f), .5f, .3f), a.Plain("PaintWhite", new Color(.78f, .78f, .76f), .55f, .2f),
                a.Plain("PaintTeal", new Color(.1f, .3f, .32f), .5f, .3f), a.Plain("PaintBrown", new Color(.28f, .18f, .1f), .4f, .25f),
                a.Plain("PaintSky", new Color(.42f, .55f, .66f), .55f, .3f), a.Plain("PaintBlack", new Color(.04f, .04f, .045f), .6f, .3f),
            };
        }

        /// <summary>A parked car's paint, by seed.</summary>
        public Material Paint(int seed) => Paints[Mathf.Abs(seed) % Paints.Length];
    }
}

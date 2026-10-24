using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The traced numbers behind <see cref="CityPlan"/>: every coordinate here was read off the user's drawing and
    /// converted by <see cref="CityPlan.FromDrawing"/>, so a point can always be checked back against the image.
    /// Kept apart from the plan's types and helpers to stay inside the 300-line rule.
    /// </summary>
    public static partial class CityPlan
    {
        /// <summary>The drawing pixel that sits on the world origin: the centre of its drawn city.</summary>
        public static readonly Vector2 DrawingCentre = new(830f, 517f);

        /// <summary>The city band. Outside it the world is the wooded outskirts it already has.</summary>
        public static readonly Rect Bounds = Rect.MinMaxRect(-3020f, -1520f, 3020f, 1520f);

        /// <summary>
        /// The coast, north to south, with the sea to its west. Traced from the drawing's left edge, then run past
        /// the band at both ends so the shoreline never stops inside the world.
        /// </summary>
        public static readonly Vector2[] Coast =
        {
            new(-2120f, 1900f), new(-2158f, 1503f), new(-2236f, 972f), new(-2314f, 452f), new(-2392f, 88f),
            new(-2496f, -432f), new(-2600f, -951f), new(-2678f, -1420f), new(-2710f, -1900f),
        };

        /// <summary>
        /// The river's centreline, west to east. It enters from the sea, runs south of the Circus and leaves at the
        /// south-east corner, which is what splits the city into its two street grids.
        /// </summary>
        public static readonly Vector2[] River =
        {
            new(-2500f, 400f), new(-2350f, 374f), new(-1612f, 270f), new(-676f, 140f), new(156f, 36f),
            new(884f, -68f), new(1508f, -432f), new(2132f, -744f), new(2756f, -1056f), new(3200f, -1280f),
        };

        /// <summary>
        /// The Circus: the drawing's circle north of the river. Its park island, the ring road round it, and the
        /// bearings of the five arterials that meet there. The drawn circle is read as the whole junction and the
        /// blocks round it, so the island itself is a park, not a traffic island.
        /// </summary>
        public static readonly Vector2 CircusCentre = new(754f, 452f);
        public const float CircusIsland = 100f, CircusRing = 130f;
        public static readonly float[] CircusArms = { 14f, 86f, 158f, 230f, 302f };

        /// <summary>The two parks the drawing marks green.</summary>
        public static readonly Rect[] Parks =
        {
            Rect.MinMaxRect(2650f, -150f, 2970f, 270f),   // East park, by the hospital
            Rect.MinMaxRect(860f, -1080f, 1220f, -720f),  // South park, below the river
        };

        /// <summary>
        /// The roads the drawing names rather than ones a district grid makes: the shore road, both embankments and
        /// the highway that leaves the south-east corner past the motel. The Circus arms are built from
        /// <see cref="CircusArms"/>, not listed here.
        /// </summary>
        public static readonly Arterial[] Named =
        {
            new("Shore Road", new Vector2[]
            {
                new(-2000f, 1500f), new(-2090f, 950f), new(-2170f, 430f), new(-2250f, 60f),
                new(-2350f, -450f), new(-2455f, -970f), new(-2530f, -1430f),
            }),
            new("North Embankment", new Vector2[]
            {
                new(-2300f, 500f), new(-1600f, 396f), new(-670f, 266f), new(160f, 162f),
                new(890f, 58f), new(1520f, -306f), new(2145f, -618f), new(2770f, -930f),
            }),
            new("South Embankment", new Vector2[]
            {
                new(-2320f, 248f), new(-1620f, 144f), new(-686f, 14f), new(146f, -90f),
                new(874f, -194f), new(1492f, -558f), new(2118f, -870f), new(2742f, -1182f),
            }),
            new("East Highway", new Vector2[]
            {
                new(2300f, -1180f), new(2760f, -1340f), new(3100f, -1430f), new(3600f, -1560f),
            }, 9f),
        };

        /// <summary>
        /// The districts, in the order a point is tested against them: the tight, named quarters first, so the big
        /// residential fills only take what is left.
        /// </summary>
        public static readonly District[] Districts =
        {
            new("Downtown", CityKind.Downtown, Box(300f, 120f, 1500f, 900f), -4f, new Vector2(80f, 180f)),
            new("Retail Core", CityKind.Retail, Box(-800f, 40f, 300f, 760f), -4f, new Vector2(12f, 20f)),
            new("Civic Quarter", CityKind.Civic, Box(1500f, 180f, 2250f, 1000f), -4f, new Vector2(14f, 28f)),
            new("Harbour", CityKind.Waterfront, Box(-2600f, -1450f, -1700f, 30f), -13f, new Vector2(8f, 18f)),
            new("Riverside Works", CityKind.Industrial, Box(1650f, -1450f, 2900f, -620f), -13f, new Vector2(10f, 24f)),
            new("Northfields", CityKind.Suburb, Box(1900f, 1000f, 3000f, 1510f), -4f, new Vector2(6f, 10f)),
            new("Eastside", CityKind.Residential, Box(2250f, -620f, 3010f, 1000f), -4f, new Vector2(8f, 14f)),
            new("Old Town", CityKind.Residential, Box(-1700f, -1510f, 1650f, -60f), -13f, new Vector2(8f, 14f)),
            new("Northgate", CityKind.Residential, Box(-2150f, 760f, 1900f, 1510f), -4f, new Vector2(8f, 14f)),
            new("Westbank", CityKind.Residential, Box(-2150f, 40f, -800f, 760f), -4f, new Vector2(8f, 14f)),
        };

        /// <summary>
        /// Where each salvage location stands. The approach bearing is the side its road comes in on, so step 6 can
        /// reach a site without cutting through its own ground. The prison is the one site out in the water: the
        /// drawing's sea is what finally lets Gallows Rock be an island.
        /// </summary>
        public static readonly Site[] Sites =
        {
            new("HQ", new Vector2(-2150f, -900f), 90f, "Harbour"),
            new("Mall", new Vector2(-300f, 200f), 0f, "Retail Core"),
            new("Skyscraper", new Vector2(900f, 600f), 270f, "Downtown"),
            new("PoliceStation", new Vector2(1750f, 750f), 180f, "Civic Quarter"),
            new("FireStation", new Vector2(1600f, -200f), 0f, "Civic Quarter"),
            new("Hospital", new Vector2(2750f, 500f), 270f, "Eastside"),
            new("School", new Vector2(2500f, 1350f), 180f, "Northfields"),
            new("Factory", new Vector2(2300f, -1150f), 0f, "Riverside Works"),
            new("Motel", new Vector2(2900f, -1400f), 315f, "Riverside Works"),
            new("Prison", new Vector2(-3850f, -250f), 90f, "Harbour"),
        };

        /// <summary>A rectangle as the four-point polygon a <see cref="District"/> wants.</summary>
        private static Vector2[] Box(float xMin, float zMin, float xMax, float zMax) => new Vector2[]
        {
            new(xMin, zMin), new(xMax, zMin), new(xMax, zMax), new(xMin, zMax),
        };
    }
}

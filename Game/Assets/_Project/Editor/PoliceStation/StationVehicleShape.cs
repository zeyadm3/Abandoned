using UnityEngine;
using R = Abandoned.EditorTools.StationMeshes.Ring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The body lines of each <see cref="StationVehicleKind"/>: the lower body, the glasshouse and the roof skin as lofts
    /// (<see cref="StationMeshes.Ring"/>, local metres, +z forward, ground at y 0), and the numbers the details hang off:
    /// axles, wheel size, the beltline, the pillars' feet and the roof's edges.
    /// </summary>
    public sealed class StationVehicleShape
    {
        public string Key;
        public R[] Body, Glass, Roof;
        public float Half, Radius, Front, Rear, Belt, Top, GlassFront, GlassRear, RoofFront, RoofRear, RoofHalf;
        /// <summary>The pickup's bed (z from, z to; 0, 0 for none).</summary>
        public float BedFrom, BedTo;

        private R[] arched;

        /// <summary>
        /// The body with its wheel arches cut: over each axle the underside rises in a curve to clear the tyre, so the
        /// wheel shows whole from the side. Rings inside an arch are replaced by ones interpolated from the body line.
        /// </summary>
        public R[] Arched
        {
            get
            {
                if (arched != null) return arched;
                var rings = new System.Collections.Generic.List<R>();
                float reach = Radius + .12f;
                foreach (R r in Body)
                    if (Mathf.Abs(r.Z - Front) > reach && Mathf.Abs(r.Z - Rear) > reach) rings.Add(r);
                foreach (float axle in new[] { Rear, Front })
                {
                    float clear = 2f * Radius + .08f;
                    (float dz, float bottom)[] arch =
                    {
                        (-reach, -1f), (-Radius - .02f, clear * .72f), (-Radius * .62f, clear * .95f), (0f, clear),
                        (Radius * .62f, clear * .95f), (Radius + .02f, clear * .72f), (reach, -1f),
                    };
                    foreach ((float dz, float bottom) in arch)
                    {
                        R b = Sample(axle + dz);
                        float low = bottom < 0f ? b.Bottom : Mathf.Min(Mathf.Max(b.Bottom, bottom), b.Top - .18f);
                        rings.Add(new R(b.Z, low, b.Top, b.BottomHalf, b.TopHalf, b.Round));
                    }
                }
                rings.Sort((x, y) => x.Z.CompareTo(y.Z));
                return arched = rings.ToArray();
            }
        }

        /// <summary>The body's cross-section at <paramref name="z"/>, interpolated between its rings.</summary>
        public R Sample(float z)
        {
            if (z <= Body[0].Z) return Body[0];
            for (int i = 1; i < Body.Length; i++)
            {
                R a = Body[i - 1], b = Body[i];
                if (z > b.Z) continue;
                float t = Mathf.InverseLerp(a.Z, b.Z, z);
                return new R(z, Mathf.Lerp(a.Bottom, b.Bottom, t), Mathf.Lerp(a.Top, b.Top, t), Mathf.Lerp(a.BottomHalf, b.BottomHalf, t),
                    Mathf.Lerp(a.TopHalf, b.TopHalf, t), Mathf.Lerp(a.Round, b.Round, t));
            }
            return Body[Body.Length - 1];
        }

        public float Nose => Body[Body.Length - 1].Z;
        public float Tail => Body[0].Z;
        public float Length => Nose - Tail;

        public static StationVehicleShape For(StationVehicleKind kind) => kind switch
        {
            StationVehicleKind.Hatchback => Hatchback,
            StationVehicleKind.Suv => Suv,
            StationVehicleKind.Pickup => Pickup,
            _ => Sedan,
        };

        private static readonly StationVehicleShape Sedan = new()
        {
            Key = "Sedan", Half = .9f, Radius = .32f, Front = 1.42f, Rear = -1.38f, Belt = .97f, Top = 1.41f,
            GlassFront = 1.1f, GlassRear = -1.3f, RoofFront = .5f, RoofRear = -.66f, RoofHalf = .67f,
            Body = new[]
            {
                new R(-2.4f, .4f, .78f, .78f, .74f, 3f), new R(-2.32f, .3f, .94f, .86f, .83f, 5f), new R(-1.95f, .27f, .99f, .89f, .86f, 6f),
                new R(-1f, .26f, .99f, .9f, .87f, 7f), new R(.2f, .26f, .97f, .9f, .87f, 7f), new R(1.2f, .26f, .93f, .9f, .86f, 7f),
                new R(1.95f, .28f, .85f, .88f, .84f, 6f), new R(2.3f, .32f, .78f, .84f, .78f, 5f), new R(2.44f, .4f, .7f, .76f, .68f, 3f),
            },
            Glass = new[]
            {
                new R(-1.32f, .95f, .99f, .8f, .76f, 3f), new R(-1f, .95f, 1.33f, .82f, .66f, 4f), new R(-.6f, .95f, 1.41f, .83f, .66f, 5f),
                new R(.4f, .95f, 1.41f, .83f, .66f, 5f), new R(.72f, .94f, 1.3f, .83f, .68f, 4f), new R(1.12f, .92f, .96f, .82f, .79f, 3f),
            },
            Roof = new[] { new R(-.7f, 1.33f, 1.425f, .72f, .672f, 6f), new R(.48f, 1.33f, 1.425f, .72f, .672f, 6f) },
        };

        private static readonly StationVehicleShape Hatchback = new()
        {
            Key = "Hatch", Half = .88f, Radius = .3f, Front = 1.25f, Rear = -1.25f, Belt = .98f, Top = 1.44f,
            GlassFront = 1f, GlassRear = -1.9f, RoofFront = .38f, RoofRear = -1.55f, RoofHalf = .69f,
            Body = new[]
            {
                new R(-2.02f, .4f, .82f, .8f, .76f, 3f), new R(-1.95f, .3f, 1f, .86f, .84f, 5f), new R(-1.2f, .27f, 1f, .88f, .86f, 6f),
                new R(.2f, .27f, .97f, .88f, .86f, 6f), new R(1.1f, .27f, .92f, .88f, .85f, 6f), new R(1.75f, .3f, .84f, .85f, .8f, 5f),
                new R(2.02f, .4f, .72f, .76f, .68f, 3f),
            },
            Glass = new[]
            {
                new R(-1.92f, .97f, 1f, .82f, .78f, 3f), new R(-1.82f, .97f, 1.38f, .84f, .68f, 4f), new R(-1.5f, .97f, 1.44f, .84f, .68f, 5f),
                new R(.3f, .97f, 1.44f, .84f, .68f, 5f), new R(.62f, .96f, 1.3f, .84f, .7f, 4f), new R(1.02f, .93f, .97f, .83f, .8f, 3f),
            },
            Roof = new[] { new R(-1.55f, 1.35f, 1.455f, .73f, .69f, 6f), new R(.36f, 1.35f, 1.455f, .73f, .69f, 6f) },
        };

        private static readonly StationVehicleShape Suv = new()
        {
            Key = "Suv", Half = .92f, Radius = .37f, Front = 1.4f, Rear = -1.35f, Belt = 1.1f, Top = 1.76f,
            GlassFront = 1.22f, GlassRear = -2.15f, RoofFront = .5f, RoofRear = -2.1f, RoofHalf = .77f,
            Body = new[]
            {
                new R(-2.3f, .45f, .95f, .84f, .8f, 4f), new R(-2.22f, .4f, 1.1f, .9f, .88f, 6f), new R(-1f, .38f, 1.12f, .92f, .9f, 8f),
                new R(.6f, .38f, 1.1f, .92f, .9f, 8f), new R(1.6f, .4f, 1.02f, .92f, .88f, 7f), new R(2.15f, .44f, .95f, .88f, .82f, 5f),
                new R(2.3f, .5f, .88f, .8f, .74f, 4f),
            },
            Glass = new[]
            {
                new R(-2.18f, 1.08f, 1.12f, .86f, .84f, 4f), new R(-2.12f, 1.08f, 1.72f, .88f, .76f, 6f), new R(.2f, 1.08f, 1.76f, .88f, .76f, 6f),
                new R(.7f, 1.07f, 1.62f, .88f, .78f, 5f), new R(1.25f, 1.04f, 1.1f, .88f, .86f, 4f),
            },
            Roof = new[] { new R(-2.1f, 1.62f, 1.79f, .81f, .77f, 8f), new R(.42f, 1.62f, 1.79f, .81f, .77f, 8f) },
        };

        private static readonly StationVehicleShape Pickup = new()
        {
            Key = "Pickup", Half = .94f, Radius = .38f, Front = 1.65f, Rear = -1.75f, Belt = 1.07f, Top = 1.64f,
            GlassFront = 1.6f, GlassRear = .12f, RoofFront = 1.05f, RoofRear = .12f, RoofHalf = .79f, BedFrom = -2.56f, BedTo = -.05f,
            Body = new[]
            {
                new R(-2.6f, .45f, .75f, .9f, .9f, 6f), new R(-2.55f, .42f, .75f, .94f, .94f, 8f), new R(-.05f, .42f, .75f, .94f, .94f, 8f),
                new R(.05f, .42f, 1.08f, .94f, .92f, 8f), new R(1.6f, .42f, 1.06f, .94f, .9f, 8f), new R(2.35f, .45f, .98f, .9f, .84f, 6f),
                new R(2.6f, .52f, .9f, .82f, .74f, 4f),
            },
            Glass = new[]
            {
                new R(.1f, 1.04f, 1.1f, .86f, .84f, 4f), new R(.16f, 1.04f, 1.62f, .88f, .78f, 6f), new R(.95f, 1.04f, 1.62f, .88f, .78f, 6f),
                new R(1.25f, 1.03f, 1.48f, .88f, .8f, 5f), new R(1.62f, 1f, 1.06f, .88f, .86f, 4f),
            },
            Roof = new[] { new R(.12f, 1.54f, 1.66f, .82f, .79f, 8f), new R(1.05f, 1.54f, 1.66f, .82f, .79f, 8f) },
        };
    }
}

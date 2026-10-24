using System.Collections.Generic;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Street = Abandoned.EditorTools.StationStreet;
using T = Abandoned.EditorTools.BurleySchoolTown;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The street life of Burley's neighbourhood under snow: sodium lamps, the signals at Madison, stop signs at Henry and
    /// Concord, a power line down Rose Hill, a bus shelter, hydrants and bins, cars left at the kerb where the plow buried
    /// them, broken plow banks along every kerb, and the city's snowplow abandoned on Rose Hill.
    /// </summary>
    public static class BurleySchoolTownStreet
    {
        public static void Build(Transform s, Transform marks, BurleySchoolArt art, StationArt a, StationRingStock stock, StationRingSpec spec)
        {
            Lamps(s, a);
            Junctions(s, a);
            var poles = new List<Vector3>();
            foreach (float x in new[] { -62f, -34f, -6f, 14f, 40f, 70f, 100f, 130f, 160f, 190f, 220f, 256f })
                poles.Add(new Vector3(x, T.Top, -21.9f));
            Street.PowerLine(s, a, poles);
            Street.BusShelter(s, a, new Vector3(60f, T.Top, -21.1f), 0f);
            Street.Bench(s, a, new Vector3(66f, T.Top, -21.6f), 0f);
            Street.NewsBoxes(s, a, new Vector3(53f, T.Top, -21.7f), 0f);
            Street.Mailbox(s, a, new Vector3(50f, T.Top, -21.8f), 0f);
            foreach ((float x0, float x1) in new[] { (T.West, 21.5f), (70f, 235.5f), (244.5f, T.East) })
                Plan.Kerbside(s, a, new Vector3(x0, T.Top, -20.6f), Vector3.right, x1 - x0, 0f, 21 + Mathf.RoundToInt(x0));
            Plan.Kerbside(s, a, new Vector3(-26f, T.Top, 2f), Vector3.forward, 220f, 90f, 23);
            Plan.Kerbside(s, a, new Vector3(248f, T.Top, 2f), Vector3.forward, 220f, -90f, 25);
            Cars(s, stock, spec);
            Banks(marks, art);
            Plow(s, a, art, new Vector3(188f, T.Top, -12.4f), 262f);
        }

        // Lit lamps down both sides of Rose Hill (none in the loop's mouth, where the truck stands) and one side of the
        // avenues, every lamp on a sidewalk clear of the crossings.
        private static void Lamps(Transform s, StationArt a)
        {
            Row(s, a, true, -9.0f, T.West, T.East, 34f, Vector3.back, new Vector2(-24f, -8f), new Vector2(18f, 74f), new Vector2(229f, 251f));
            Row(s, a, true, -19.0f, T.West, T.East, 48f, Vector3.forward, new Vector2(15f, 37f), new Vector2(54f, 68f), new Vector2(229f, 251f));
            Row(s, a, false, -23.0f, -4f, T.North, 40f, Vector3.right);
            Row(s, a, false, 245.0f, -4f, T.North, 40f, Vector3.left);
            Street.Lamp(s, a, new Vector3(30.9f, T.Top, -48f), new Vector3(26f, T.Top, -48f), 7f);
        }

        private static void Row(Transform s, StationArt a, bool alongX, float line, float from, float to, float spacing, Vector3 road, params Vector2[] skip)
        {
            for (float u = from + spacing * .5f; u < to - 2f; u += spacing)
            {
                bool clear = true;
                foreach (Vector2 k in skip) clear &= u < k.x || u > k.y;
                if (!clear) continue;
                Vector3 foot = alongX ? new Vector3(u, T.Top, line) : new Vector3(line, T.Top, u);
                Street.Lamp(s, a, foot, foot + road * 5f, 7f);
            }
        }

        private static void Junctions(Transform s, StationArt a)
        {
            Street.TrafficLight(s, a, new Vector3(244.7f, T.Top, -9.3f), 180f);
            Street.TrafficLight(s, a, new Vector3(235.3f, T.Top, -18.7f), 0f);
            Street.StopSign(s, a, new Vector3(-13.1f, T.Top, -5.1f), 0f);
            Street.StopSign(s, a, new Vector3(21.1f, T.Top, -22.9f), 180f);
            Street.StreetSign(s, a, new Vector3(-22.9f, T.Top, -5.1f), 0f);
            Street.StreetSign(s, a, new Vector3(30.9f, T.Top, -22.9f), 0f);
            Street.StreetSign(s, a, new Vector3(231.9f, T.Top, -5.1f), 0f);
        }

        // Cars along both kerbs of every street, noses the way the traffic ran, never in a junction, a crossing, the bus
        // lane's or service drive's mouths, or the loop's frontage.
        private static void Cars(Transform s, StationRingStock stock, StationRingSpec spec)
        {
            var rng = new System.Random(9701);
            Kerb(s, stock, spec, rng, true, -17.3f, T.West, T.East, 270f, new Vector2(13f, 39f), new Vector2(52f, 68f), new Vector2(227f, 253f));
            Kerb(s, stock, spec, rng, true, -10.7f, T.West, T.East, 90f, new Vector2(-27f, -9f), new Vector2(16f, 76f), new Vector2(178f, 198f), new Vector2(227f, 253f));
            Kerb(s, stock, spec, rng, false, -21.4f, -3f, T.North - 2f, 180f);
            Kerb(s, stock, spec, rng, false, -14.6f, -3f, T.North - 2f, 0f, new Vector2(14f, 28f), new Vector2(48f, 62f), new Vector2(96f, 112f));
            Kerb(s, stock, spec, rng, false, 236.6f, T.South + 2f, T.North - 2f, 180f, new Vector2(-27f, -3f));
            Kerb(s, stock, spec, rng, false, 243.4f, T.South + 2f, T.North - 2f, 0f, new Vector2(-27f, -3f));
            Kerb(s, stock, spec, rng, false, 22.6f, T.South + 2f, -26f, 180f);
        }

        private static void Kerb(Transform s, StationRingStock stock, StationRingSpec spec, System.Random rng, bool alongX, float line, float from, float to,
            float yaw, params Vector2[] skip)
        {
            for (float u = from + 4f + (float)rng.NextDouble() * 6f; u < to - 4f; u += 9f + (float)rng.NextDouble() * 24f)
            {
                bool clear = true;
                foreach (Vector2 k in skip) clear &= u < k.x || u > k.y;
                if (!clear) continue;
                Vector3 at = alongX ? new Vector3(u, T.Top, line) : new Vector3(line, T.Top, u);
                stock.Place(s, spec.Cars[rng.Next(spec.Cars.Count)], at, yaw + (float)rng.NextDouble() * 4f - 2f);
            }
        }

        // What the plow pushed to the kerbs: broken drifts along each road edge, on the road side, walk-over (no collider).
        private static void Banks(Transform marks, BurleySchoolArt art)
        {
            Transform banks = Group("PlowBanks", marks);
            var rng = new System.Random(9711);
            foreach ((float x0, float x1) in new[] { (T.West, -22.5f), (-13.5f, 20f), (72f, 235.5f), (244.5f, T.East) })
                Bank(banks, art, rng, new Vector3(x0, 0f, -9.5f), new Vector3(x1, 0f, -9.5f), Vector3.back);
            foreach ((float x0, float x1) in new[] { (T.West, 17f), (35f, 235.5f), (244.5f, T.East) })
                Bank(banks, art, rng, new Vector3(x0, 0f, -18.5f), new Vector3(x1, 0f, -18.5f), Vector3.forward);
            Bank(banks, art, rng, new Vector3(-22.5f, 0f, -4f), new Vector3(-22.5f, 0f, T.North), Vector3.right);
            foreach ((float z0, float z1) in new[] { (-4f, 18f), (24f, 52f), (58f, 100f), (108f, T.North) })
                Bank(banks, art, rng, new Vector3(-13.5f, 0f, z0), new Vector3(-13.5f, 0f, z1), Vector3.left);
            foreach ((float z0, float z1) in new[] { (T.South, -23f), (-4f, T.North) })
            {
                Bank(banks, art, rng, new Vector3(235.5f, 0f, z0), new Vector3(235.5f, 0f, z1), Vector3.right);
                Bank(banks, art, rng, new Vector3(244.5f, 0f, z0), new Vector3(244.5f, 0f, z1), Vector3.left);
            }
            Bank(banks, art, rng, new Vector3(21.5f, 0f, T.South), new Vector3(21.5f, 0f, -23f), Vector3.right);
            Bank(banks, art, rng, new Vector3(30.5f, 0f, T.South), new Vector3(30.5f, 0f, -23f), Vector3.left);
        }

        private static void Bank(Transform parent, BurleySchoolArt art, System.Random rng, Vector3 from, Vector3 to, Vector3 intoRoad)
        {
            Vector3 d = to - from;
            float length = d.magnitude;
            d /= length;
            for (float u = (float)rng.NextDouble() * 2f; u < length - 2f;)
            {
                float piece = Mathf.Min(4f + (float)rng.NextDouble() * 8f, length - u), wide = .7f + (float)rng.NextDouble() * .5f;
                float high = .25f + (float)rng.NextDouble() * .3f;
                Vector3 centre = from + d * (u + piece * .5f) + intoRoad * (wide * .5f - .05f) + Vector3.up * (T.Top + high * .5f - .06f);
                GameObject drift = Box("PlowDrift", parent, centre, new Vector3(wide, high, piece), art.SnowDrift, false);
                drift.transform.rotation = Quaternion.LookRotation(d, Vector3.up) * Quaternion.Euler(0f, 0f, (float)rng.NextDouble() * 4f - 2f);
                u += piece + .4f + (float)rng.NextDouble() * 2.5f;
            }
        }

        // The city's plow truck, its blade down, left where it stalled: a box van in highway orange with a blade and a
        // salt spreader, solid.
        private static void Plow(Transform s, StationArt a, BurleySchoolArt art, Vector3 at, float yaw)
        {
            Transform t = Group("AbandonedSnowplow", s);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material orange = a.Plain("PlowOrange", new Color(.85f, .38f, .06f), .4f, .2f);
            StationApparatus.BoxVan(t, a, orange, a.DarkSteel, 7);
            GameObject blade = Box("Blade", t, new Vector3(0f, .55f, 4.1f), new Vector3(3.4f, .95f, .25f), orange, false);
            blade.transform.localRotation = Quaternion.Euler(-8f, 18f, 0f);
            Box("BladeEdge", t, new Vector3(0f, .1f, 4.15f), new Vector3(3.3f, .12f, .3f), a.DarkSteel, false).transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
            Box("BladeFrame", t, new Vector3(0f, .6f, 3.55f), new Vector3(.9f, .4f, 1f), a.DarkSteel, false);
            Box("Spreader", t, new Vector3(0f, .9f, -3.4f), new Vector3(1.6f, .9f, .9f), a.DarkSteel, false);
            Box("BladeSnow", t, new Vector3(-.3f, .25f, 4.8f), new Vector3(3f, .5f, 1.2f), art.SnowDrift, false);
            var solid = t.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.4f, .4f);
            solid.size = new Vector3(2.4f, 2.8f, 7.6f);
        }
    }
}

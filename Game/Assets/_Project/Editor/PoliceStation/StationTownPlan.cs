using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Site = Abandoned.EditorTools.StationSite;
using V = Abandoned.EditorTools.StationVehicleWear;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Helpers the station towns are laid out with: a row of buildings along a street front, cars parked along a kerb,
    /// lamps and hydrants down a sidewalk, ground lots between the streets. Everything is in world metres; the groups
    /// passed in sit at the origin (under <c>Exterior</c>).
    /// </summary>
    public static class StationTownPlan
    {
        public readonly struct Lot
        {
            public readonly K Kind;
            public readonly float Width, Depth, Gap;
            public readonly int Storeys;
            public Lot(K kind, float width, float depth, int storeys = 2, float gap = 0f)
            { Kind = kind; Width = width; Depth = depth; Storeys = storeys; Gap = gap; }
        }

        /// <summary>Buildings side by side from <paramref name="start"/> along <paramref name="along"/>, fronts facing
        /// <paramref name="yaw"/> (their front's normal), each lot followed by its gap (an alley or a side yard).</summary>
        public static void Row(Transform parent, StationArt a, Vector3 start, Vector3 along, float yaw, IList<Lot> lots, int seed)
        {
            along.y = 0f;
            along.Normalize();
            float cursor = 0f;
            for (int i = 0; i < lots.Count; i++)
            {
                Lot l = lots[i];
                Vector3 front = start + along * (cursor + l.Width * .5f);
                StationTownBuildings.Build(parent, l.Kind, front, yaw, l.Width, l.Depth, l.Storeys, seed + i * 7, a);
                cursor += l.Width + l.Gap;
            }
        }

        /// <summary>Cars along a kerb from <paramref name="start"/>, <paramref name="length"/> metres along
        /// <paramref name="along"/>, noses toward <paramref name="along"/>; a few gaps, a covered one, some flat or smashed.
        /// Stretches in <paramref name="clear"/> (distances along the kerb, from-to) stay empty: driveways, the van's way in.</summary>
        public static void ParkedCars(Transform parent, StationArt a, Vector3 start, Vector3 along, float length, int seed, params Vector2[] clear)
        {
            along.y = 0f;
            along.Normalize();
            float yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            StationVehicleArt v = StationVehicleArt.For(a);
            int n = 0;
            for (float d = 3f; d < length - 3f; d += 6.4f)
            {
                int roll = Mathf.Abs(seed * 37 + n * 11);
                n++;
                bool blocked = false;
                foreach (Vector2 c in clear) if (d > c.x - 3.2f && d < c.y + 3.2f) blocked = true;
                if (blocked || roll % 5 == 0) continue;
                Vector3 at = start + along * d + Vector3.down * .02f;
                float turn = (roll % 3 - 1) * 2.5f;
                if (roll % 11 == 3) { Site.CoveredCar(parent, at, yaw + turn, a); continue; }
                var kind = (StationVehicleKind)(new[] { 0, 2, 3, 4, 0, 2 }[roll % 6]);
                V wear = V.Dusty;
                if (roll % 4 == 1) wear |= V.FlatFront;
                if (roll % 7 == 2) wear |= V.Smashed;
                if (roll % 13 == 5) wear = V.Burnt;
                if (roll % 9 == 4) wear |= V.DoorOpen;
                StationVehicles.Car(parent, "ParkedCar", at, yaw + turn, kind, v.Paint(roll), a, wear, roll);
            }
        }

        /// <summary>Street lamps every <paramref name="spacing"/> metres down a sidewalk line, heads toward <paramref name="road"/>.</summary>
        public static void Lamps(Transform parent, StationArt a, Vector3 start, Vector3 along, float length, float spacing, Vector3 road, float intensity = 8f)
        {
            along.y = 0f;
            along.Normalize();
            for (float d = spacing * .5f; d < length; d += spacing)
            {
                Vector3 foot = start + along * d;
                StationStreet.Lamp(parent, a, foot, foot + road, intensity);
            }
        }

        /// <summary>Hydrants, bins and the odd litter heap down a sidewalk line.</summary>
        public static void Kerbside(Transform parent, StationArt a, Vector3 start, Vector3 along, float length, float yaw, int seed)
        {
            along.y = 0f;
            along.Normalize();
            int i = 0;
            for (float d = 11f; d < length - 4f; d += 19f, i++)
            {
                Vector3 p = start + along * d;
                switch ((seed + i) % 5)
                {
                    case 0: StationStreet.Hydrant(parent, a, p, yaw); break;
                    case 1: StationScans.Put(parent, "metal_trash_can", p, yaw, new Vector3(.6f, 1f, .6f), solid: true); break;
                    case 2: StationStreet.Litter(parent, a, p, seed + i); break;
                    case 3: StationStreet.ParkingMeter(parent, a, p, yaw); break;
                    default: StationScans.Put(parent, "utility_box_01", p, yaw, new Vector3(1.2f, 1.6f, .7f), solid: true); break;
                }
            }
        }

        /// <summary>A painted picket fence from <paramref name="from"/> to <paramref name="to"/>, waist high, solid.</summary>
        public static void Picket(Transform parent, StationArt a, Vector3 from, Vector3 to, Material paint)
        {
            Transform f = GreyboxFactory.Group("PicketFence", parent);
            Vector3 d = to - from;
            float len = d.magnitude;
            f.SetPositionAndRotation(from, Quaternion.LookRotation(d.normalized, Vector3.up));
            for (float z = .06f; z < len; z += .14f)
                GreyboxFactory.Box("Picket", f, new Vector3(0f, .5f, z), new Vector3(.025f, 1f, .08f), paint, false);
            foreach (float y in new[] { .3f, .8f }) GreyboxFactory.Box("Rail", f, new Vector3(0f, y, len * .5f), new Vector3(.03f, .07f, len), paint, false);
            for (float z = 0f; z <= len + .01f; z += 2.4f) GreyboxFactory.Box("Post", f, new Vector3(0f, .55f, Mathf.Min(z, len)), new Vector3(.08f, 1.1f, .08f), paint, false);
            var solid = f.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, .55f, len * .5f);
            solid.size = new Vector3(.1f, 1.1f, len);
        }

        /// <summary>A strip of something laid on a lot (a driveway, a path, a slab), a centimetre proud of the ground.</summary>
        public static void Paving(Transform parent, Rect r, Material m) =>
            GreyboxFactory.Box("Paving", parent, new Vector3(r.center.x, -.015f, r.center.y), new Vector3(r.width, .01f, r.height), m);

        /// <summary>A lot of ground between the streets (yards, backlots, the ground under the buildings).</summary>
        public static void Ground(Transform root, string name, Rect r, Material m, SurfaceMaterial surface) =>
            Site.Slab(Site.GroundSlabs(root), name, r, m, surface);
    }
}

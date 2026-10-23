using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.StationMeshes;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The streets round a station: roads and sidewalks as ground slabs (the open world's survey reads them as the
    /// site's ground), their markings, and the street furniture: scanned lamps that still light, power lines with
    /// sagging wires, traffic lights, signs without words, a bus shelter, benches, mailboxes, dumpsters, hydrants and the
    /// litter of a street nobody sweeps. Everything but the slabs lives under the group it is given (under
    /// <c>Exterior</c>, so off the NavMesh).
    /// </summary>
    public static class StationStreet
    {
        /// <summary>A road on <paramref name="r"/>: asphalt, a double yellow centre line, edge lines and drains.</summary>
        public static void Road(Transform root, Transform marks, StationArt a, string name, Rect r, bool alongX)
        {
            Site.Slab(Site.GroundSlabs(root), name, r, a.Asphalt, SurfaceMaterial.Asphalt);
            float length = alongX ? r.width : r.height, half = (alongX ? r.height : r.width) * .5f;
            Vector3 centre = new(r.center.x, -.014f, r.center.y);
            Vector3 along = alongX ? Vector3.right : Vector3.forward, across = alongX ? Vector3.forward : Vector3.right;
            Material yellow = a.Safety;
            foreach (float off in new[] { -.12f, .12f })
                Box("CentreLine", marks, centre + across * off, alongX ? new Vector3(length, .01f, .1f) : new Vector3(.1f, .01f, length), yellow, false);
            foreach (float s in new[] { -1f, 1f })
            {
                Box("EdgeLine", marks, centre + across * (s * (half - .5f)), alongX ? new Vector3(length, .01f, .1f) : new Vector3(.1f, .01f, length), a.Paint, false);
                for (float d = 6f; d < length - 3f; d += 18f)
                {
                    Vector3 p = centre + along * (d - length * .5f) + across * (s * (half - .25f));
                    Box("Drain", marks, p + Vector3.up * .002f, alongX ? new Vector3(.9f, .01f, .4f) : new Vector3(.4f, .01f, .9f), a.DarkSteel, false);
                }
            }
            for (float d = 9f; d < length - 4f; d += 23f)
                StationScans.Put(marks, "water_manhole_cover", centre + along * (d - length * .5f) + across * (half * .35f) + Vector3.up * .005f, 0f,
                    new Vector3(.9f, .1f, .9f));
            // Tar patches and oil where cars stood: darker rectangles on the asphalt.
            for (float d = 4f; d < length - 3f; d += 13.7f)
            {
                GameObject patch = Box("Patch", marks, centre + along * (d - length * .5f) + across * (Mathf.Sin(d) * half * .6f) + Vector3.up * .001f,
                    new Vector3(1.6f + Mathf.Repeat(d, 1.3f), .01f, 1.1f + Mathf.Repeat(d * 1.7f, 1.2f)), a.Oil, false);
                patch.transform.localRotation = Quaternion.Euler(0f, d * 17f, 0f);
            }
        }

        /// <summary>A sidewalk on <paramref name="r"/>: concrete in flags, a kerb along the road side.</summary>
        public static void Sidewalk(Transform root, Transform marks, StationArt a, string name, Rect r, bool alongX, float kerbSide)
        {
            Site.Slab(Site.GroundSlabs(root), name, r, a.Sidewalk, SurfaceMaterial.Concrete);
            float length = alongX ? r.width : r.height;
            Vector3 centre = new(r.center.x, -.014f, r.center.y);
            Vector3 along = alongX ? Vector3.right : Vector3.forward, across = alongX ? Vector3.forward : Vector3.right;
            float half = (alongX ? r.height : r.width) * .5f;
            for (float d = 1.6f; d < length; d += 1.6f)
                Box("Joint", marks, centre + along * (d - length * .5f), alongX ? new Vector3(.025f, .01f, half * 2f) : new Vector3(half * 2f, .01f, .025f), a.Grime, false);
            Vector3 kerb = centre + across * (kerbSide * (half - .12f));
            Site.Kerb(marks, kerb - along * (length * .5f), kerb + along * (length * .5f), a);
        }

        /// <summary>Zebra stripes across a road at <paramref name="centre"/>, the road running <paramref name="alongX"/>.</summary>
        public static void Crosswalk(Transform marks, StationArt a, Vector3 centre, bool alongX, float roadWidth)
        {
            for (float o = -roadWidth * .5f + .8f; o < roadWidth * .5f - .6f; o += 1f)
                Box("Zebra", marks, new Vector3(centre.x + (alongX ? 0f : o), -.013f, centre.z + (alongX ? o : 0f)),
                    alongX ? new Vector3(3f, .01f, .5f) : new Vector3(.5f, .01f, 3f), a.Paint, false);
            Box("StopLine", marks, new Vector3(centre.x + (alongX ? 2.2f : 0f), -.013f, centre.z + (alongX ? 0f : 2.2f)),
                alongX ? new Vector3(.3f, .01f, roadWidth * .45f) : new Vector3(roadWidth * .45f, .01f, .3f), a.Paint, false);
        }

        /// <summary>A street lamp (the scanned cobra-head lamp) with its sodium light still on; a code-built one if the scan is missing.</summary>
        public static void Lamp(Transform parent, StationArt a, Vector3 foot, Vector3 toward, float intensity = 9f)
        {
            Vector3 dir = toward - foot;
            dir.y = 0f;
            float yaw = dir.sqrMagnitude > .01f ? Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg : 0f;
            GameObject lamp = StationScans.Put(parent, "street_lamp_01", foot, yaw, new Vector3(2.5f, 9f, 2.5f), solid: true);
            if (lamp == null)
            {
                StationLighting.StreetLamp(parent, foot, toward, a, intensity);
                return;
            }
            Bounds b = ShowcaseModels.LocalBounds(lamp.transform);
            // The head is somewhere over the bounds' top: light from just under it, straight down (as the showcase does).
            var headLocal = new Vector3(b.center.x, b.max.y - .3f, b.center.z);
            GameObject lens = Box("Lens", lamp.transform, headLocal + Vector3.up * .02f, new Vector3(.3f, .01f, .45f), a.Lamp, false);
            lens.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Transform aim = Group("Light", lamp.transform);
            aim.localPosition = headLocal - Vector3.up * .05f;
            aim.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 18f;
            l.spotAngle = 120f;
            l.intensity = intensity;
            l.color = new Color(1f, .7f, .38f);
            l.shadows = LightShadows.None;
        }

        /// <summary>Power poles at <paramref name="feet"/> (in order) with three conductors sagging between their crossarms.</summary>
        public static void PowerLine(Transform parent, StationArt a, IList<Vector3> feet)
        {
            var tops = new List<Vector3[]>();
            for (int i = 0; i < feet.Count; i++)
            {
                Vector3 next = i + 1 < feet.Count ? feet[i + 1] : feet[i] + (feet[i] - feet[Mathf.Max(0, i - 1)]);
                Vector3 run = next - feet[i];
                run.y = 0f;
                float yaw = run.sqrMagnitude > .01f ? Mathf.Atan2(run.x, run.z) * Mathf.Rad2Deg : 0f;
                Transform pole = StationScans.Pole(parent, feet[i], yaw + 90f, i % 3 == 1);
                tops.Add(pole != null ? Attachments(pole, feet[i]) : Fallback(parent, a, feet[i], yaw));
            }
            Material wire = StationVehicleArt.For(a).Trim;
            for (int i = 0; i + 1 < tops.Count; i++)
                for (int k = 0; k < 3; k++)
                {
                    Vector3 from = tops[i][k], to = tops[i + 1][k];
                    float sag = Vector3.Distance(from, to) * .035f;
                    Vector3 prev = from;
                    for (int s = 1; s <= 8; s++)
                    {
                        float u = s / 8f;
                        Vector3 p = Vector3.Lerp(from, to, u) + Vector3.down * (sag * 4f * u * (1f - u));
                        M.Strut(parent, "Wire", prev, p, .018f, wire);
                        prev = p;
                    }
                }
        }

        // Where the wires leave a pole: the ends and middle of its widest crossarm.
        private static Vector3[] Attachments(Transform pole, Vector3 foot)
        {
            Bounds arm = default;
            float widest = 0f;
            foreach (Renderer r in pole.GetComponentsInChildren<Renderer>())
            {
                if (!r.name.Contains("connection_large")) continue;
                float w = Mathf.Max(r.bounds.size.x, r.bounds.size.z);
                if (w > widest) { widest = w; arm = r.bounds; }
            }
            if (widest == 0f) return new[] { foot + Vector3.up * 9f, foot + Vector3.up * 9f, foot + Vector3.up * 9f };
            Vector3 axis = arm.size.x > arm.size.z ? Vector3.right : Vector3.forward;
            float half = Mathf.Max(arm.extents.x, arm.extents.z) * .85f;
            var top = new Vector3(arm.center.x, arm.max.y, arm.center.z);
            return new[] { top - axis * half, top, top + axis * half };
        }

        // A plain timber pole and crossarm when the scanned kit is missing.
        private static Vector3[] Fallback(Transform parent, StationArt a, Vector3 foot, float yaw)
        {
            Transform p = Group("PowerPole", parent);
            p.SetPositionAndRotation(foot, Quaternion.Euler(0f, yaw, 0f));
            Primitive(PrimitiveType.Cylinder, "Pole", p, new Vector3(0f, 4.6f, 0f), new Vector3(.3f, 4.6f, .3f), a.DarkWood);
            Box("Crossarm", p, new Vector3(0f, 8.6f, 0f), new Vector3(2.2f, .12f, .12f), a.DarkWood, false);
            return new[] { p.TransformPoint(new Vector3(-1f, 8.7f, 0f)), p.TransformPoint(new Vector3(0f, 8.7f, 0f)), p.TransformPoint(new Vector3(1f, 8.7f, 0f)) };
        }

        public static void Hydrant(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            if (StationScans.Put(parent, "fire_hydrant", at, yaw, new Vector3(.6f, 1f, .6f), solid: true) != null) return;
            Material paint = a.Plain("Hydrant", new Color(.75f, .55f, .1f), .4f, .2f);
            M.Draw(parent, "Hydrant", M.Lathe(a, "Hydrant", new[]
            {
                new Vector2(0f, 0f), new Vector2(.2f, 0f), new Vector2(.2f, 0f), new Vector2(.2f, .1f), new Vector2(.15f, .12f), new Vector2(.15f, .62f),
                new Vector2(.19f, .66f), new Vector2(.19f, .7f), new Vector2(.14f, .78f), new Vector2(.06f, .85f), new Vector2(0f, .88f),
            }, 16), at, new Vector3(0f, yaw, 0f), Vector3.one, paint);
        }

        /// <summary>A traffic signal on a mast arm over the road: a pole, the arm, two dark signal heads with hoods.</summary>
        public static void TrafficLight(Transform parent, StationArt a, Vector3 at, float yaw, float reach = 5f)
        {
            Transform t = Group("TrafficSignal", parent);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material pole = a.Plain("SignalPole", new Color(.2f, .22f, .2f), .4f, .5f), housing = a.Plain("SignalHousing", new Color(.75f, .62f, .1f), .4f);
            Primitive(PrimitiveType.Cylinder, "Pole", t, new Vector3(0f, 3.4f, 0f), new Vector3(.24f, 3.4f, .24f), pole);
            Box("Base", t, new Vector3(0f, .2f, 0f), new Vector3(.5f, .4f, .5f), a.Concrete);
            M.Strut(t, "Arm", new Vector3(0f, 6.2f, 0f), new Vector3(0f, 6.4f, reach), .12f, pole);
            M.Strut(t, "Brace", new Vector3(0f, 5.2f, 0f), new Vector3(0f, 6.25f, reach * .5f), .05f, pole);
            foreach (float z in new[] { reach * .55f, reach - .3f }) SignalHead(t, a, housing, new Vector3(0f, 5.6f, z), 0f);
            SignalHead(t, a, housing, new Vector3(.25f, 2.9f, 0f), 90f);
            Box("WalkSignal", t, new Vector3(-.25f, 2.6f, 0f), new Vector3(.3f, .32f, .2f), housing, false);
            Box("PushButton", t, new Vector3(0f, 1.1f, .14f), new Vector3(.1f, .14f, .06f), housing, false);
        }

        private static void SignalHead(Transform t, StationArt a, Material housing, Vector3 at, float yaw)
        {
            Transform h = Group("SignalHead", t);
            h.localPosition = at;
            h.localRotation = Quaternion.Euler(0f, yaw + 180f, 0f);
            Box("Case", h, Vector3.zero, new Vector3(.36f, 1.05f, .25f), housing, false);
            Box("Backplate", h, new Vector3(0f, 0f, -.13f), new Vector3(.6f, 1.25f, .02f), a.Rubber, false);
            Material[] lenses = { a.Plain("LensRed", new Color(.3f, .04f, .03f), .9f), a.Plain("LensAmber", new Color(.35f, .22f, .03f), .9f), a.Plain("LensGreen", new Color(.04f, .2f, .1f), .9f) };
            for (int i = 0; i < 3; i++)
            {
                float y = .33f - i * .33f;
                GameObject lens = Primitive(PrimitiveType.Cylinder, "Lens", h, new Vector3(0f, y, .13f), new Vector3(.22f, .01f, .22f), lenses[i], false);
                lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Box("Visor", h, new Vector3(0f, y + .12f, .22f), new Vector3(.26f, .02f, .18f), housing, false);
            }
        }

        /// <summary>A stop sign: an octagon on its post (no lettering, the paint long gone to a dull red).</summary>
        public static void StopSign(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("StopSign", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Post", s, new Vector3(0f, 1.2f, 0f), new Vector3(.06f, 2.4f, .04f), a.Steel);
            Mesh oct = M.Lathe(a, "Octagon", new[] { new Vector2(.4f, 0f), new Vector2(0f, 0f) }, 8);
            // Spun a sixteenth so a flat edge is on top, then stood up to face +z.
            Quaternion stand = Quaternion.Euler(90f, 0f, 0f) * Quaternion.Euler(0f, 22.5f, 0f);
            M.Draw(s, "Rim", oct, new Vector3(0f, 2.2f, .03f), Vector3.zero, Vector3.one, a.White).transform.localRotation = stand;
            M.Draw(s, "Face", oct, new Vector3(0f, 2.2f, .036f), Vector3.zero, Vector3.one * .9f, a.Red).transform.localRotation = stand;
        }

        /// <summary>A pair of street-name blades on a post (blank: the names wore off).</summary>
        public static void StreetSign(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("StreetSign", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material green = a.Plain("StreetSignGreen", new Color(.08f, .3f, .17f), .5f);
            Box("Post", s, new Vector3(0f, 1.5f, 0f), new Vector3(.06f, 3f, .06f), a.Steel);
            Box("BladeA", s, new Vector3(.45f, 2.9f, 0f), new Vector3(.9f, .2f, .02f), green, false);
            Box("BladeB", s, new Vector3(0f, 2.65f, .45f), new Vector3(.02f, .2f, .9f), green, false);
            Box("Border", s, new Vector3(.45f, 2.9f, .012f), new Vector3(.86f, .16f, .004f), a.White, false);
        }

        public static void BusShelter(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("BusShelter", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material frame = a.Plain("ShelterFrame", new Color(.18f, .2f, .22f), .45f, .5f);
            foreach (float x in new[] { -1.8f, 1.8f })
            foreach (float z in new[] { -.7f, .7f })
                Box("Post", s, new Vector3(x, 1.25f, z), new Vector3(.08f, 2.5f, .08f), frame);
            Box("Roof", s, new Vector3(0f, 2.55f, 0f), new Vector3(3.9f, .1f, 1.7f), frame, false);
            Box("Back", s, new Vector3(0f, 1.3f, -.7f), new Vector3(3.6f, 2f, .02f), a.Glass, false);
            Box("BackCracked", s, new Vector3(1f, 1.4f, -.69f), new Vector3(1.2f, 1.1f, .005f), StationVehicleArt.For(a).Crazed, false);
            Box("Side", s, new Vector3(-1.8f, 1.3f, 0f), new Vector3(.02f, 2f, 1.3f), a.Glass, false);
            Box("AdPanel", s, new Vector3(1.8f, 1.3f, 0f), new Vector3(.12f, 1.8f, 1.2f), a.White, false);
            Box("AdFace", s, new Vector3(1.87f, 1.3f, 0f), new Vector3(.01f, 1.6f, 1.05f), a.Accent, false);
            Box("Seat", s, new Vector3(-.3f, .45f, -.45f), new Vector3(2.4f, .05f, .35f), a.Steel, false);
            Box("SeatLeg", s, new Vector3(-.3f, .22f, -.5f), new Vector3(2.2f, .44f, .04f), frame, false);
        }

        public static void Bench(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("StreetBench", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material iron = a.Plain("CastIron", new Color(.1f, .12f, .1f), .35f, .5f);
            foreach (float x in new[] { -.75f, .75f })
            {
                Box("Leg", s, new Vector3(x, .23f, .05f), new Vector3(.06f, .46f, .5f), iron);
                Box("BackLeg", s, new Vector3(x, .65f, -.2f), new Vector3(.06f, .5f, .05f), iron, false);
            }
            for (int i = 0; i < 3; i++) Box("Slat", s, new Vector3(0f, .46f, -.12f + i * .14f), new Vector3(1.8f, .04f, .1f), a.Wood, false);
            for (int i = 0; i < 2; i++) Box("BackSlat", s, new Vector3(0f, .65f + i * .16f, -.22f), new Vector3(1.8f, .1f, .03f), a.Wood, false);
        }

        public static void Mailbox(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("Mailbox", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material blue = a.Plain("MailBlue", new Color(.06f, .14f, .34f), .45f, .2f);
            foreach (float x in new[] { -.22f, .22f }) Box("Leg", s, new Vector3(x, .15f, 0f), new Vector3(.05f, .3f, .45f), blue);
            Box("Body", s, new Vector3(0f, .75f, 0f), new Vector3(.5f, .9f, .5f), blue);
            GameObject top = Primitive(PrimitiveType.Cylinder, "Top", s, new Vector3(0f, 1.2f, 0f), new Vector3(.5f, .25f, .5f), blue, false);
            top.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Box("Slot", s, new Vector3(0f, 1.05f, .25f), new Vector3(.3f, .12f, .02f), a.Rubber, false);
        }

        public static void NewsBoxes(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("NewsBoxes", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material[] paints = { a.Red, a.Plain("NewsYellow", new Color(.8f, .65f, .12f), .4f), a.Plain("NewsBlue", new Color(.15f, .3f, .55f), .4f) };
            for (int i = 0; i < 3; i++)
            {
                float x = -.55f + i * .55f;
                Box("Box", s, new Vector3(x, .55f, 0f), new Vector3(.48f, .75f, .42f), paints[i]);
                Box("Window", s, new Vector3(x, .7f, .215f), new Vector3(.38f, .3f, .01f), StationTownArt.For(a).Window, false);
                Box("Pedestal", s, new Vector3(x, .09f, 0f), new Vector3(.4f, .18f, .36f), a.DarkSteel, false);
            }
        }

        public static void Dumpster(Transform parent, StationArt a, Vector3 at, float yaw, bool open)
        {
            Transform s = Group("Dumpster", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material green = a.Plain("DumpsterGreen", new Color(.12f, .25f, .14f), .3f, .2f);
            Box("Bin", s, new Vector3(0f, .75f, 0f), new Vector3(3.6f, 1.2f, 1.7f), green);
            Box("Rim", s, new Vector3(0f, 1.38f, 0f), new Vector3(3.7f, .06f, 1.8f), a.DarkSteel, false);
            foreach (float x in new[] { -1.6f, 1.6f }) Box("Caster", s, new Vector3(x, .08f, 0f), new Vector3(.15f, .16f, 1.4f), a.Rubber, false);
            foreach (float x in new[] { -.9f, .9f })
            {
                GameObject lid = Box("Lid", s, new Vector3(x, open ? 1.7f : 1.43f, open ? -.95f : 0f), new Vector3(1.75f, .04f, 1.8f), a.Rubber, false);
                if (open) lid.transform.localRotation = Quaternion.Euler(-70f, 0f, 0f);
            }
            Box("Rust", s, new Vector3(1.2f, .45f, .86f), new Vector3(1f, .4f, .01f), a.Plain("RustStreak", new Color(.33f, .17f, .08f), .1f), false);
            if (open) StationScans.Put(s, "trashbag", s.TransformPoint(new Vector3(.3f, 1.2f, 0f)), yaw, new Vector3(.7f, .7f, .7f));
        }

        public static void Bollard(Transform parent, StationArt a, Vector3 at)
        {
            GameObject g = M.Draw(parent, "Bollard", M.Lathe(a, "Bollard", new[]
            {
                new Vector2(0f, 0f), new Vector2(.11f, 0f), new Vector2(.11f, 0f), new Vector2(.11f, .85f), new Vector2(.12f, .88f), new Vector2(.09f, .95f), new Vector2(0f, .98f),
            }, 14), at, Vector3.zero, Vector3.one, a.Safety);
            var c = g.AddComponent<CapsuleCollider>();
            c.center = new Vector3(0f, .5f, 0f);
            c.height = 1f;
            c.radius = .12f;
        }

        public static void ParkingMeter(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("ParkingMeter", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Post", s, new Vector3(0f, .6f, 0f), new Vector3(.06f, 1.2f, .06f), a.DarkSteel);
            Box("Head", s, new Vector3(0f, 1.35f, 0f), new Vector3(.2f, .32f, .16f), a.Steel, false);
            Box("Dome", s, new Vector3(0f, 1.42f, .08f), new Vector3(.15f, .12f, .01f), StationTownArt.For(a).Window, false);
        }

        /// <summary>A red fire-alarm call box on its post (the fire department's street box).</summary>
        public static void AlarmBox(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("AlarmBox", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Primitive(PrimitiveType.Cylinder, "Post", s, new Vector3(0f, .8f, 0f), new Vector3(.18f, .8f, .18f), a.Red);
            Box("Box", s, new Vector3(0f, 1.75f, 0f), new Vector3(.3f, .45f, .22f), a.Red, false);
            Box("Pull", s, new Vector3(0f, 1.75f, .115f), new Vector3(.12f, .14f, .02f), a.White, false);
            Primitive(PrimitiveType.Sphere, "Globe", s, new Vector3(0f, 2.1f, 0f), Vector3.one * .2f, a.Plain("AlarmGlobe", new Color(.5f, .08f, .06f), .85f), false);
        }

        /// <summary>The litter of a street left alone: bags, boxes, a crate, a tyre, scattered round <paramref name="at"/>.</summary>
        public static void Litter(Transform parent, StationArt a, Vector3 at, int seed)
        {
            string[] ids = { "trashbag", "cardboard_box_01", "trashbag", "wooden_crate_01", "old_tyre", "plastic_crate_01", "trashbag" };
            int n = 2 + Mathf.Abs(seed) % 3;
            for (int i = 0; i < n; i++)
            {
                int roll = Mathf.Abs(seed * 31 + i * 17);
                float ang = roll % 360, r = .4f + (roll % 7) * .15f;
                Vector3 p = at + new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad) * r, 0f, Mathf.Sin(ang * Mathf.Deg2Rad) * r);
                StationScans.Put(parent, ids[roll % ids.Length], p, roll % 180, new Vector3(.8f, .8f, .8f));
            }
        }

        /// <summary>Concrete barriers across a road (the scanned barrier), turned a little each.</summary>
        public static void Barriers(Transform parent, Vector3 from, Vector3 to, int count)
        {
            Vector3 d = to - from;
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg + 90f;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = Vector3.Lerp(from, to, count == 1 ? .5f : i / (count - 1f));
                StationScans.Put(parent, "concrete_road_barrier_02", p, yaw + (i % 3 - 1) * 6f, new Vector3(3.5f, 1.3f, 1f), solid: true);
            }
        }

        /// <summary>A sawhorse barricade, orange and white, the kind left round a hole in the road.</summary>
        public static void Sawhorse(Transform parent, StationArt a, Vector3 at, float yaw)
        {
            Transform s = Group("Sawhorse", parent);
            s.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Material orange = a.Plain("BarricadeOrange", new Color(.85f, .35f, .06f), .4f);
            foreach (float x in new[] { -.8f, .8f })
            foreach (float z in new[] { -.25f, .25f })
                M.Strut(s, "Leg", new Vector3(x, 0f, z), new Vector3(x, 1f, 0f), .05f, a.White);
            for (int i = 0; i < 2; i++)
            {
                Box("Board", s, new Vector3(0f, .6f + i * .35f, 0f), new Vector3(1.9f, .2f, .03f), a.White);
                for (int k = 0; k < 4; k++)
                {
                    GameObject stripe = Box("Stripe", s, new Vector3(-.7f + k * .46f, .6f + i * .35f, .018f), new Vector3(.18f, .2f, .005f), orange, false);
                    stripe.transform.localRotation = Quaternion.Euler(0f, 0f, 40f);
                }
            }
        }
    }
}

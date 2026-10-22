using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// TestMap showcase light fittings. Each is a real <see cref="LightFixture"/> (the game's own: it flickers when
    /// faulty, ShadowDirector picks the nearest ones to cast shadows, LightingRebuild calibrates its brightness to the
    /// floor it lights) with a scanned fitting for a body. Fittings under a collapsible floor go with it.
    /// </summary>
    public static class ShowcaseLighting
    {
        public static readonly Color Fluorescent = new(1f, .93f, .82f);
        public static readonly Color Tungsten = new(1f, .8f, .58f);
        public static readonly Color Sodium = new(1f, .7f, .42f);
        public static readonly Color Gallery = new(1f, .96f, .9f);

        /// <summary>Late afternoon under thin cloud: a low warm sun through the windows, a cool sky, light haze.</summary>
        public static readonly LevelAtmosphere.Mood Mood = new(
            sky: new Color(.5f, .54f, .6f), equator: new Color(.4f, .41f, .42f), ground: new Color(.22f, .21f, .19f),
            fog: new Color(.56f, .58f, .61f), fogDensity: .006f, bloom: .15f, vignette: .18f, grain: .08f,
            saturation: -4f, contrast: 10f, sunColor: new Color(1f, .86f, .68f), sunIntensity: 1.6f, sunEuler: new Vector3(24f, -38f, 0f));

        /// <summary>A ceiling fitting of model <paramref name="model"/> whose top is at <paramref name="ceiling"/>, throwing a
        /// spot straight down.</summary>
        public static LightFixture Ceiling(Transform parent, string model, Vector3 ceiling, float yaw, Vector3 fit, Color colour,
            int seed, bool faulty = false, bool dead = false, StructuralSection support = null)
        {
            GameObject body = ShowcaseModels.Hang(model, parent, ceiling, yaw, fit, support);
            Transform holder = body != null ? body.transform : Fallback(parent, ceiling, support);
            float bottom = body != null ? ShowcaseModels.LocalBounds(holder).min.y : -.1f;
            Renderer glow = Glow(holder, bottom);
            return Fit(holder, new Vector3(0f, bottom - .03f, 0f), Quaternion.Euler(90f, 0f, 0f), colour, 12f, 112f, glow, faulty, dead, seed);
        }

        /// <summary>A small dark track spot on the ceiling at <paramref name="at"/>, aimed at <paramref name="target"/>.</summary>
        public static LightFixture Spot(Transform parent, string name, Vector3 at, Vector3 target, float angle, Color colour, int seed)
        {
            var holder = Group(name, parent);
            holder.position = at;
            Quaternion aim = Quaternion.LookRotation(target - at);
            Box("Track", holder, new Vector3(0f, .03f, 0f), new Vector3(.06f, .03f, .3f), ShowcaseMaterials.Housing, false);
            GameObject can = Primitive(PrimitiveType.Cylinder, "Can", holder, Vector3.zero, new Vector3(.11f, .09f, .11f), ShowcaseMaterials.Housing, false);
            can.transform.rotation = aim * Quaternion.Euler(90f, 0f, 0f);
            can.transform.position = at + aim * Vector3.forward * .02f - Vector3.up * .08f;
            GameObject lens = Primitive(PrimitiveType.Cylinder, "Lens", holder, Vector3.zero, new Vector3(.08f, .004f, .08f), ShowcaseMaterials.Glow, false);
            lens.transform.rotation = can.transform.rotation;
            lens.transform.position = can.transform.position + aim * Vector3.forward * .095f;
            var lamp = Fit(holder, holder.InverseTransformPoint(lens.transform.position + aim * Vector3.forward * .02f), aim,
                colour, Vector3.Distance(at, target) * 2.2f, angle, lens.GetComponent<Renderer>(), false, false, seed);
            lamp.Lamp.innerSpotAngle = angle * .55f;
            MarkStatic(holder);
            return lamp;
        }

        /// <summary>An outdoor wall pack over a door: a dark box with a lit lens, throwing light down and out.</summary>
        public static LightFixture WallPack(Transform parent, string name, Vector3 at, Vector3 outward, int seed)
        {
            var holder = Group(name, parent);
            holder.SetPositionAndRotation(at, Quaternion.LookRotation(outward));
            Box("Housing", holder, new Vector3(0f, 0f, .07f), new Vector3(.3f, .22f, .14f), ShowcaseMaterials.Housing, false);
            GameObject lens = Box("Lens", holder, new Vector3(0f, -.1f, .09f), new Vector3(.24f, .02f, .1f), ShowcaseMaterials.Glow, false);
            Quaternion down = Quaternion.LookRotation(Vector3.down * .8f + outward * .6f);
            var lamp = Fit(holder, new Vector3(0f, -.13f, .1f), Quaternion.Inverse(holder.rotation) * down, Sodium, 14f, 120f,
                lens.GetComponent<Renderer>(), false, false, seed, local: true);
            MarkStatic(holder);
            return lamp;
        }

        /// <summary>A spot light at a position and rotation (world, or the holder's with <paramref name="local"/>), wired into
        /// a LightFixture on the holder.</summary>
        public static LightFixture Fit(Transform holder, Vector3 localPosition, Quaternion rotation, Color colour, float range, float angle,
            Renderer glow, bool faulty, bool dead, int seed, bool local = false)
        {
            var lampRoot = Group("Light", holder);
            lampRoot.localPosition = localPosition;
            if (local) lampRoot.localRotation = rotation;
            else lampRoot.rotation = rotation;
            Light lamp = lampRoot.gameObject.AddComponent<Light>();
            lamp.type = LightType.Spot;
            lamp.range = range;
            lamp.spotAngle = angle;
            lamp.intensity = 5f; // LightingRebuild sets the real value from the floor it reaches
            lamp.color = colour;
            lamp.shadows = LightShadows.None; // ShadowDirector turns them on for the nearest fixtures
            var fixture = holder.gameObject.AddComponent<LightFixture>();
            fixture.EditorSetup(lamp, glow, faulty, dead, seed, ShowcaseModels.TubeGlow);
            return fixture;
        }

        // The scanned fitting's own lit tubes, or else a small diffuser under it.
        private static Renderer Glow(Transform holder, float bottom)
        {
            foreach (Renderer r in holder.GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial != null && r.sharedMaterial.IsKeywordEnabled("_EMISSION")) return r;
            GameObject disc = Primitive(PrimitiveType.Cylinder, "Diffuser", holder, new Vector3(0f, bottom - .004f, 0f),
                new Vector3(.18f, .004f, .18f), ShowcaseMaterials.Glow, false);
            disc.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return disc.GetComponent<Renderer>();
        }

        // A plain batten if the scanned fitting was never downloaded.
        private static Transform Fallback(Transform parent, Vector3 ceiling, StructuralSection support)
        {
            var holder = Group("Batten", parent);
            holder.position = ceiling;
            Box("Body", holder, new Vector3(0f, -.05f, 0f), new Vector3(1.25f, .1f, .2f), ShowcaseMaterials.Housing, false);
            if (support != null) holder.gameObject.AddComponent<SectionProp>().EditorSetup(support);
            return holder;
        }
    }
}

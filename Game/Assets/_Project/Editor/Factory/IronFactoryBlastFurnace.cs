using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L3: the cold blast furnace in the west bank's south corner: a riveted tower 7 m across that rises almost to
    /// the roof, ringed by its bustle pipe and tuyeres, with an open tap-hole mouth you can walk into. Inside it is
    /// dark, round and close, and one ember at the bottom has never quite gone out. It stands clear of the tier-1
    /// catwalk beside it, the Breaker Hall to the north and the machines (the layout leaves this corner empty).
    /// </summary>
    public static class IronFactoryBlastFurnace
    {
        private static readonly Vector3 Centre = new(5f, 0f, 24f);
        private const float Radius = 3.5f;
        private const float Wall = .5f;
        private const float Height = 18f;
        private const float MouthHeight = 3f;
        private const int Segments = 16;

        /// <summary>Ground the furnace and its bustle pipe cover (metres, plan view): clutter keeps off it.</summary>
        public static Rect Footprint => new(Centre.x - Radius - 1.8f, Centre.z - Radius - 1.8f, (Radius + 1.8f) * 2f, (Radius + 1.8f) * 2f);

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform furnace = Group("BlastFurnace", root);
            furnace.position = Centre;
            float step = 360f / Segments;
            float chord = 2f * Mathf.PI * Radius / Segments + .15f;
            for (int i = 0; i < Segments; i++)
            {
                // Segment i covers the arc centred at angle + half a step (0 = east). The two either side of east
                // are the mouth: open up to MouthHeight, brick above it.
                bool mouth = i == 0 || i == Segments - 1;
                Transform segment = Group($"Wall_{i:00}", furnace);
                segment.localRotation = Quaternion.Euler(0f, 90f - i * step - step * .5f, 0f);
                float from = mouth ? MouthHeight : 0f, tall = Height - from;
                Box("Shell", segment, new Vector3(0f, from + tall * .5f, Radius - Wall * .5f), new Vector3(chord, tall, Wall), art.Rust);
                Box("Lining", segment, new Vector3(0f, from + tall * .5f, Radius - Wall - .04f), new Vector3(chord, tall, .08f), art.Lining, false);
            }
            // Riveted bands, the bustle pipe and the tuyeres that fed it air, the hood and its flue into the roof.
            foreach (float y in new[] { 4f, 9f, 14f })
                Primitive(PrimitiveType.Cylinder, "Band", furnace, Vector3.up * y, new Vector3(Radius * 2f + .2f, .2f, Radius * 2f + .2f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Hood", furnace, Vector3.up * (Height + .3f), new Vector3(Radius * 2f, .6f, Radius * 2f), art.Steel, false);
            Box("Flue", furnace, Vector3.up * (Height + 1.2f), new Vector3(1.2f, 1.8f, 1.2f), art.Steel, false);
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                GameObject bustle = Box("Bustle", furnace, dir * (Radius + .7f) + Vector3.up * 7.5f, new Vector3(.7f, .7f, 2f), art.Steel, false);
                bustle.transform.localRotation = Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up));
                if (Mathf.Abs(a) < .4f || Mathf.Abs(a - Mathf.PI * 2f) < .4f) continue;   // none across the mouth
                GameObject tuyere = Box("Tuyere", furnace, dir * (Radius + .35f) + Vector3.up * 2.2f, new Vector3(.25f, .25f, .7f), art.Steel, false);
                tuyere.transform.localRotation = Quaternion.LookRotation(dir);
            }
            // The cold hearth: a slag crust underfoot and one ember that never went out.
            Box("Slag", furnace, Vector3.up * .05f, new Vector3(Radius * 1.6f, .1f, Radius * 1.6f), art.Bed, false);
            Transform ember = Group("Ember", furnace);
            ember.localPosition = new Vector3(-1.2f, .3f, .4f);
            ember.gameObject.AddComponent<PowerIndependentLight>();
            Light glow = ember.gameObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 4.5f;
            glow.intensity = .7f;
            glow.color = new Color(1f, .3f, .08f);
            glow.shadows = LightShadows.None;
            furnace.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(furnace);
        }
    }
}

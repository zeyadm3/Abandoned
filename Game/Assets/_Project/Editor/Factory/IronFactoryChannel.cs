using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The molten channel down the middle of the plant: a refractory-lined trench 2.6 m deep, a glowing surface
    /// with no collider (whatever goes in sinks onto the bed, out of sight), the host's <see cref="LavaVolume"/>,
    /// and a dented guard rail along the edges with gaps where it has broken away.
    /// </summary>
    public static class IronFactoryChannel
    {
        private const float RailHeight = 1.05f;
        private const float RailBar = .08f;
        private const float PostSpacing = 2f;

        // Rail runs along each edge, as (from, to) in metres along that edge; the spaces between are the
        // broken stretches where nothing stops a careless step.
        private static readonly (float, float)[] WestRuns = { (20f, 32f), (35f, 47f), (52f, 64f), (69f, 80f) };
        private static readonly (float, float)[] EastRuns = { (20f, 27f), (31f, 44f), (48f, 61f), (63f, 80f) };
        private static readonly (float, float)[] SouthRuns = { (52f, 66f), (70f, 72f) };
        private static readonly (float, float)[] NorthRuns = { (52f, 58f), (61f, 72f) };

        public static void Build(Transform root, IronFactoryArt art)
        {
            Rect c = L.ChannelMeters;
            Transform channel = Group("Channel", root);
            // Monsters never path into the trench; agents stop at its edge.
            channel.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            float depth = -L.ChannelFloorY;
            const float lining = .4f;
            GameObject bed = Box("Bed", channel, new Vector3(c.center.x, L.ChannelBedY - .2f, c.center.y), new Vector3(c.width, .4f, c.height), art.Bed);
            bed.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            Box("Lining_W", channel, new Vector3(c.xMin + lining * .5f, -depth * .5f, c.center.y), new Vector3(lining, depth, c.height), art.Lining);
            Box("Lining_E", channel, new Vector3(c.xMax - lining * .5f, -depth * .5f, c.center.y), new Vector3(lining, depth, c.height), art.Lining);
            Box("Lining_S", channel, new Vector3(c.center.x, -depth * .5f, c.yMin + lining * .5f), new Vector3(c.width, depth, lining), art.Lining);
            Box("Lining_N", channel, new Vector3(c.center.x, -depth * .5f, c.yMax - lining * .5f), new Vector3(c.width, depth, lining), art.Lining);
            // Flat steel coping on the lip: reads as an edge in the dark, too low to trip on.
            Box("Coping_W", channel, new Vector3(c.xMin - .15f, .015f, c.center.y), new Vector3(.3f, .03f, c.height + .6f), art.Safety, false);
            Box("Coping_E", channel, new Vector3(c.xMax + .15f, .015f, c.center.y), new Vector3(.3f, .03f, c.height + .6f), art.Safety, false);
            Box("Coping_S", channel, new Vector3(c.center.x, .015f, c.yMin - .15f), new Vector3(c.width, .03f, .3f), art.Safety, false);
            Box("Coping_N", channel, new Vector3(c.center.x, .015f, c.yMax + .15f), new Vector3(c.width, .03f, .3f), art.Safety, false);
            MarkStatic(channel);

            GameObject surface = Box("LavaSurface", channel, new Vector3(c.center.x, L.LavaSurfaceY, c.center.y),
                new Vector3(c.width - 2f * lining, .06f, c.height - 2f * lining), art.Lava, false);
            Renderer surfaceRenderer = surface.GetComponent<Renderer>();
            surfaceRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Transform lava = Group("Lava", channel);
            lava.gameObject.AddComponent<LavaVolume>().EditorSetup(L.LavaKill);
            // Molten iron glows with the power off too.
            lava.gameObject.AddComponent<PowerIndependentLight>();
            lava.gameObject.AddComponent<LavaSurface>().EditorSetup(surfaceRenderer, Glow(lava, c));

            Rails(Group("ChannelRails", root), c, art);
        }

        // Warm point lights along the channel, just above the floor: the lava lights the hall around it, and
        // keeps doing so with the power off. No shadows (ShadowDirector's budget is for the ceiling lights).
        private static Light[] Glow(Transform lava, Rect c)
        {
            var lights = new List<Light>();
            for (float z = c.yMin + 5f; z < c.yMax; z += 10f)
            {
                Transform holder = Group($"Glow_{lights.Count}", lava);
                holder.position = new Vector3(c.center.x, .6f, z);
                Light light = holder.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 17f;
                light.intensity = 9f;
                light.color = new Color(1f, .42f, .1f);
                light.shadows = LightShadows.None;
                lights.Add(light);
            }
            return lights.ToArray();
        }

        private static void Rails(Transform rails, Rect c, IronFactoryArt art)
        {
            const float outside = .35f;
            foreach ((float a, float b) in WestRuns) Run(rails, "W", new Vector3(c.xMin - outside, 0f, a), new Vector3(c.xMin - outside, 0f, b), art);
            foreach ((float a, float b) in EastRuns) Run(rails, "E", new Vector3(c.xMax + outside, 0f, a), new Vector3(c.xMax + outside, 0f, b), art);
            foreach ((float a, float b) in SouthRuns) Run(rails, "S", new Vector3(a, 0f, c.yMin - outside), new Vector3(b, 0f, c.yMin - outside), art);
            foreach ((float a, float b) in NorthRuns) Run(rails, "N", new Vector3(a, 0f, c.yMax + outside), new Vector3(b, 0f, c.yMax + outside), art);
            MarkStatic(rails);
        }

        private static void Run(Transform rails, string side, Vector3 from, Vector3 to, IronFactoryArt art)
        {
            Transform run = Group($"Rail_{side}_{rails.childCount:00}", rails);
            Vector3 along = to - from;
            float length = along.magnitude;
            Quaternion facing = Quaternion.LookRotation(along.normalized, Vector3.up);
            Vector3 mid = (from + to) * .5f;
            foreach (float y in new[] { RailHeight, RailHeight * .5f })
            {
                GameObject bar = Box(y > .9f ? "TopRail" : "MidRail", run, mid + Vector3.up * y, new Vector3(RailBar, RailBar, length), art.Safety);
                bar.transform.rotation = facing;
            }
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / PostSpacing) + 1);
            for (int i = 0; i < posts; i++)
            {
                Vector3 at = Vector3.Lerp(from, to, i / (float)(posts - 1));
                Box($"Post_{i:00}", run, at + Vector3.up * RailHeight * .5f, new Vector3(RailBar, RailHeight, RailBar), art.Steel);
            }
        }
    }
}

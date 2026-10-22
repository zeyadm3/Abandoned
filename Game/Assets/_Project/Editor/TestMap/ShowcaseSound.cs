using System.Collections.Generic;
using Abandoned.Audio;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Where the TestMap site's outdoor sounds come from (TestMapSoundscape plays them): birds and wind round the forest's
    /// edge, a little way out past the fence; crickets in the long grass north of the gallery and by the boiler house;
    /// crows high in the trees on every side.
    /// </summary>
    public static class ShowcaseSound
    {
        public static void Build(Transform root)
        {
            Transform sound = Group("Soundscape", root);
            Transform[] Ring(string name, int count, float outside, float height, float phase)
            {
                var points = new List<Transform>();
                Vector2 centre = Yard.center;
                Vector2 half = Yard.size / 2f + Vector2.one * outside;
                for (int i = 0; i < count; i++)
                {
                    float a = (i + phase) * Mathf.PI * 2f / count;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    // A point on the rectangle round the yard, in the direction a.
                    float t = Mathf.Min(half.x / Mathf.Max(.001f, Mathf.Abs(dir.x)), half.y / Mathf.Max(.001f, Mathf.Abs(dir.y)));
                    Vector2 p = centre + dir * t;
                    Transform point = Group($"{name}_{i}", sound);
                    point.position = new Vector3(p.x, ShowcaseTerrain.HeightAt(p.x, p.y) + height, p.y);
                    points.Add(point);
                }
                return points.ToArray();
            }
            Transform[] birds = Ring("Birds", 10, 24f, 8f, 0f);
            Transform[] wind = Ring("Wind", 8, 30f, 12f, .5f);
            Transform[] crows = Ring("Crow", 9, 45f, 20f, .25f);
            var crickets = new List<Transform>();
            foreach (Vector2 p in new[] { new Vector2(-28f, 44f), new Vector2(0f, 44f), new Vector2(30f, 46f), new Vector2(52f, 44f),
                         new Vector2(-37f, 24f), new Vector2(-37f, -18f) })
            {
                Transform point = Group("Crickets", sound);
                point.position = new Vector3(p.x, Ground + .3f, p.y);
                crickets.Add(point);
            }
            sound.gameObject.AddComponent<TestMapSoundscape>().EditorSetup(birds, wind, crickets.ToArray(), crows);
        }
    }
}

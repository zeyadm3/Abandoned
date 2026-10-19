using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using Flight = Abandoned.EditorTools.IslandPrisonLayout.Flight;
using Landing = Abandoned.EditorTools.IslandPrisonLayout.Landing;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Every stair flight and stair landing on the island: one structural section each (a flight's root at its foot,
    /// its ramp collider beside the Visual), steel treads with risers, stringers and a handrail each side, and a NavMesh
    /// link tying both ends together for monsters. Collapsible flights also carve their footprint and drop their link
    /// when they fall (<see cref="SectionNavLinks"/>), so no monster paths up a stair that isn't there.
    /// </summary>
    public static class IslandFlights
    {
        private const float Tread = .28f;

        public sealed class Built
        {
            public Transform Transform;
            public bool Collapsible;
            public bool IsLanding;
            public Vector2 Size;
        }

        public static Built Flight(Transform parent, Flight f, Material tread, Material rail, bool railLeft = true, bool railRight = true)
        {
            Transform root = Group(f.Name, parent);
            root.position = f.Foot;
            root.rotation = Quaternion.LookRotation(f.Direction, Vector3.up);
            float slope = Mathf.Sqrt(f.Run * f.Run + f.Rise * f.Rise);
            float angle = Mathf.Atan2(f.Rise, f.Run) * Mathf.Rad2Deg;

            Transform ramp = Group("Ramp", root);
            ramp.localPosition = new Vector3(0f, f.Rise * .5f - .1f, f.Run * .5f);
            ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(f.Width, .2f, slope);

            Transform visual = Group("Visual", root);
            int steps = Mathf.Max(4, Mathf.RoundToInt(f.Rise / .19f));
            float stepRun = f.Run / steps, stepRise = f.Rise / steps;
            for (int i = 0; i < steps; i++)
            {
                float z = (i + .5f) * stepRun, y = (i + 1) * stepRise;
                Box($"Tread_{i:00}", visual, new Vector3(0f, y - .03f, z), new Vector3(f.Width, .06f, stepRun * .98f), tread, false);
                Box($"Riser_{i:00}", visual, new Vector3(0f, y - stepRise * .5f, i * stepRun + .01f), new Vector3(f.Width, stepRise, .02f), tread, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject stringer = Box(side < 0 ? "Stringer_L" : "Stringer_R", visual,
                    new Vector3(side * (f.Width * .5f + .04f), f.Rise * .5f - .12f, f.Run * .5f), new Vector3(.08f, .3f, slope), rail, false);
                stringer.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                if (side < 0 ? !railLeft : !railRight) continue;
                // The handrail's collider stops a carry sliding off the side; it sits on the root, not the Visual.
                Transform solid = Group(side < 0 ? "RailSolid_L" : "RailSolid_R", root);
                solid.localPosition = new Vector3(side * (f.Width * .5f + .06f), f.Rise * .5f + .5f, f.Run * .5f);
                solid.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                solid.gameObject.AddComponent<BoxCollider>().size = new Vector3(.06f, 1f, slope);
                GameObject top = Box(side < 0 ? "Rail_L" : "Rail_R", visual,
                    new Vector3(side * (f.Width * .5f + .06f), f.Rise * .5f + 1f, f.Run * .5f), new Vector3(.06f, .06f, slope), rail, false);
                top.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                for (int i = 0; i <= 4; i++)
                {
                    float t = i / 4f;
                    Box("Baluster", visual, new Vector3(side * (f.Width * .5f + .06f), f.Rise * t + .5f, f.Run * t), new Vector3(.04f, 1f, .04f), rail, false);
                }
            }
            var link = root.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.back * .6f;
            link.endPoint = Vector3.forward * (f.Run + .6f) + Vector3.up * f.Rise;
            link.width = Mathf.Max(.6f, f.Width - .4f);
            link.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return new Built { Transform = root, Collapsible = f.Collapsible, Size = new Vector2(f.Width, f.Run) };
        }

        public static Built Landing(Transform parent, Landing l, Material deck, Material rail, params Vector3[] railFromTo)
        {
            Transform root = Group(l.Name, parent);
            root.position = new Vector3(l.Area.center.x, l.Y, l.Area.center.y);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = Vector3.down * .15f;
            solid.size = new Vector3(l.Area.width, .3f, l.Area.height);
            Transform visual = Group("Visual", root);
            Box("Deck", visual, Vector3.down * .15f, new Vector3(l.Area.width, .3f, l.Area.height), deck, false);
            // Rails along the given world edges (pairs of points), on the root so the collider survives in place.
            for (int i = 0; i + 1 < railFromTo.Length; i += 2)
            {
                Vector3 a = railFromTo[i] - root.position, b = railFromTo[i + 1] - root.position;
                IslandKit.Railing(visual, "Rail", a, b, 1.05f, rail, false);
                Transform guard = Group("RailSolid", root);
                guard.localPosition = (a + b) * .5f + Vector3.up * .55f;
                Vector3 d = b - a;
                guard.localRotation = Quaternion.LookRotation(d.normalized);
                guard.gameObject.AddComponent<BoxCollider>().size = new Vector3(.06f, 1.1f, d.magnitude);
            }
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return new Built { Transform = root, Collapsible = l.Collapsible, IsLanding = true, Size = new Vector2(l.Area.width, l.Area.height) };
        }

        /// <summary>Registers flights and landings with the structure. Collapsible ones carve and drop their links.</summary>
        public static void AddSections(StructureSceneSetup setup, IEnumerable<Built> built, float health = 1f, float capacity = 1f)
        {
            foreach (Built b in built)
            {
                setup.AddSection(b.Transform, b.IsLanding ? SectionType.Walkway : SectionType.Stair, b.Collapsible,
                    b.Collapsible ? health : 1f, b.Collapsible ? capacity : 1f, fractured: false);
                if (!b.Collapsible)
                {
                    b.Transform.gameObject.AddComponent<SectionKeepClear>();
                    continue;
                }
                SectionNavCarver.EditorAdd(b.Transform.gameObject, new Vector3(b.Size.x, .3f, b.Size.y));
                if (!b.IsLanding)
                {
                    // The carver sits at the root (the foot); centre it on the flight.
                    var obstacle = b.Transform.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                    obstacle.center = new Vector3(0f, .2f, b.Size.y * .5f);
                    b.Transform.gameObject.AddComponent<SectionNavLinks>();
                }
            }
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.AI;

namespace Abandoned.Tutorial
{
    /// <summary>
    /// The guide on the floor, on this machine only: glowing chevrons that flow along the walkable route from this
    /// player to the lesson's target (round furniture, through doors, down the cellar stairs, up the truck's ramp),
    /// a pulsing ring where the target is, and an outline around a room to enter or a floor to brace or get off.
    /// The route is the level's own NavMesh, worked out again as the player moves; one mesh, rebuilt each frame.
    /// Created by <see cref="TutorialHud"/> in the training house and gone with it.
    /// </summary>
    public sealed class TutorialFloorPath : MonoBehaviour
    {
        public const string MaterialResource = "TutorialGuidePath";

        private const float Spacing = 0.9f, Lead = 0.9f, Reach = 30f, Flow = 1.5f, Lift = 0.04f, Recalculate = 0.2f;
        private static readonly Color Amber = new(1f, 0.74f, 0.32f), Rust = new(1f, 0.34f, 0.24f);

        private readonly List<Vector3> route = new();
        private readonly List<float> along = new();
        private readonly List<Vector3> verts = new();
        private readonly List<Color> colours = new();
        private readonly List<int> tris = new();
        private NavMeshPath navPath;
        private Mesh mesh;
        private MeshRenderer view;
        private float nextRoute;
        private Vector3 routedFrom, routedTo;
        private int ground;

        public static TutorialFloorPath Create()
        {
            var go = new GameObject("TrainingGuidePath");
            return go.AddComponent<TutorialFloorPath>();
        }

        private void Awake()
        {
            navPath = new NavMeshPath();
            ground = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            mesh = new Mesh { name = "TrainingGuidePath" };
            mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            view = gameObject.AddComponent<MeshRenderer>();
            view.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            view.receiveShadows = false;
            view.sharedMaterial = PathMaterial();
            view.enabled = false;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }

        /// <summary>Every frame: draw the guide for this lesson (or nothing).</summary>
        public void Draw(TutorialGuide g, bool show)
        {
            NetworkPlayer me = NetworkPlayer.Local;
            if (!show || me == null || !me.IsSpawned || me.IsDead || !TutorialGuideText.TryTarget(g, out Vector3 target))
            {
                view.enabled = false;
                route.Clear();
                return;
            }
            verts.Clear();
            colours.Clear();
            tris.Clear();
            bool away = g.Kind == TutorialGuideKind.Away;
            Color colour = away ? Rust : Amber;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
            bool walk = g.Kind is TutorialGuideKind.Go or TutorialGuideKind.Take or TutorialGuideKind.Use or TutorialGuideKind.Load
                or TutorialGuideKind.Hide or TutorialGuideKind.Brace;
            Vector3 end = Ground(target);
            if (walk)
            {
                Route(me, g, end);
                Chevrons(colour, me.transform.position);
            }
            if (g.Target == TutorialGuideTarget.Area && TutorialArea.Find(g.Key.ToString()) is TutorialArea area) AreaOutline(area, colour, pulse);
            else if (g.Target == TutorialGuideTarget.Section && TutorialSection.Find(g.Key.ToString()) is Structure.StructuralSection section)
                Outline(section.SurfaceBounds, section.SurfaceBounds.max.y, colour, pulse, away ? 0.12f : 0.08f);
            else if (g.Kind != TutorialGuideKind.Look) Ring(end, colour, pulse);
            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetColors(colours);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            view.enabled = verts.Count > 0;
        }

        // ---- The route ----

        private void Route(NetworkPlayer me, TutorialGuide g, Vector3 end)
        {
            Vector3 from = me.transform.position;
            if (Time.unscaledTime < nextRoute && (from - routedFrom).sqrMagnitude < 0.16f && (end - routedTo).sqrMagnitude < 0.09f && route.Count > 1) return;
            nextRoute = Time.unscaledTime + Recalculate;
            routedFrom = from;
            routedTo = end;
            route.Clear();
            Extraction.TruckCargo truck = Extraction.TruckCargo.Current;
            bool aboard = truck != null && truck.Carries(from);
            if (g.HasVia && !aboard)
            {
                Leg(from, g.Via);
                Straight(Ground(g.Via), end);
            }
            else Leg(from, end);
            along.Clear();
            float total = 0f;
            for (int i = 0; i < route.Count; i++)
            {
                if (i > 0) total += Vector3.Distance(route[i - 1], route[i]);
                along.Add(total);
            }
        }

        // The walkable way between two points. Where the NavMesh stops short of the target (it is on a table, in the
        // truck's bay, or the way is shut), the last stretch is drawn straight only if nothing stands in between, so
        // the arrows never cut through a wall.
        private void Leg(Vector3 from, Vector3 to)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = 0, areaMask = NavMesh.AllAreas };
            if (NavMesh.SamplePosition(from, out NavMeshHit start, 2.5f, filter) && NavMesh.SamplePosition(to, out NavMeshHit finish, 4f, filter) &&
                NavMesh.CalculatePath(start.position, finish.position, filter, navPath) && navPath.status != NavMeshPathStatus.PathInvalid &&
                navPath.corners.Length >= 2)
            {
                Vector3[] corners = navPath.corners;
                Straight(from, corners[0]);
                for (int i = 1; i < corners.Length; i++) Straight(corners[i - 1], corners[i]);
                Vector3 last = corners[corners.Length - 1];
                if ((last - to).sqrMagnitude > 0.16f && InSight(last, to)) Straight(last, to);
                return;
            }
            if (InSight(from, to)) Straight(from, to);
        }

        private bool InSight(Vector3 a, Vector3 b)
        {
            Vector3 up = Vector3.up * 0.6f;
            return !Physics.Linecast(a + up, b + up, ground, QueryTriggerInteraction.Ignore);
        }

        // Points every half metre, each set down on the floor beneath it (stairs, the ramp, a threshold).
        private void Straight(Vector3 a, Vector3 b)
        {
            float length = Vector3.Distance(a, b);
            int steps = Mathf.Clamp(Mathf.CeilToInt(length / 0.5f), 1, 200);
            for (int i = route.Count == 0 ? 0 : 1; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
                route.Add(Ground(p));
            }
        }

        private Vector3 Ground(Vector3 p)
        {
            Vector3 top = p + Vector3.up * 0.9f;
            return Physics.Raycast(top, Vector3.down, out RaycastHit hit, 3.5f, ground, QueryTriggerInteraction.Ignore)
                ? hit.point
                : p;
        }

        // ---- The marks ----

        // Chevrons flow from the player toward the target; they fade in at your feet and out with distance. They are
        // counted from where the player is along the route now, not where the route was last worked out.
        private void Chevrons(Color colour, Vector3 player)
        {
            if (route.Count < 2) return;
            float total = along[along.Count - 1];
            float origin = Along(player);
            float offset = Mathf.Repeat(Time.unscaledTime * Flow, Spacing);
            int segment = 1;
            for (float d = origin + Lead + offset; d < Mathf.Min(total - 0.5f, origin + Reach); d += Spacing)
            {
                while (segment < along.Count - 1 && along[segment] < d) segment++;
                float span = Mathf.Max(0.0001f, along[segment] - along[segment - 1]);
                float t = Mathf.Clamp01((d - along[segment - 1]) / span);
                Vector3 at = Vector3.Lerp(route[segment - 1], route[segment], t);
                Vector3 dir = route[segment] - route[segment - 1];
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) continue;
                float ahead = d - origin;
                float fade = Mathf.Clamp01((ahead - Lead) / 0.8f) * Mathf.Clamp01((Reach - ahead) / 6f);
                Chevron(at + Vector3.up * Lift, dir.normalized, colour, 0.9f * fade);
            }
        }

        // How far along the route the point nearest the player is (searching the first stretch only, so a route that
        // doubles back past the player doesn't jump ahead).
        private float Along(Vector3 player)
        {
            float best = float.MaxValue, at = 0f;
            for (int i = 1; i < route.Count && along[i - 1] < 12f; i++)
            {
                Vector3 a = route[i - 1], b = route[i], ab = b - a;
                float t = ab.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector3.Dot(player - a, ab) / ab.sqrMagnitude) : 0f;
                float d = (a + ab * t - player).sqrMagnitude;
                if (d >= best) continue;
                best = d;
                at = Mathf.Lerp(along[i - 1], along[i], t);
            }
            return at;
        }

        private void Chevron(Vector3 at, Vector3 forward, Color colour, float alpha)
        {
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            const float width = 0.3f, depth = 0.22f, thick = 0.09f;
            Vector3 tip = at + forward * (depth * 0.5f);
            Vector3 leftBack = at - forward * (depth * 0.5f) - right * width;
            Vector3 rightBack = at - forward * (depth * 0.5f) + right * width;
            Quad(leftBack, tip, tip - forward * thick * 1.4f, leftBack - forward * thick, colour, alpha);
            Quad(tip, rightBack, rightBack - forward * thick, tip - forward * thick * 1.4f, colour, alpha);
        }

        private void Ring(Vector3 centre, Color colour, float pulse)
        {
            float radius = 0.45f + 0.12f * pulse;
            Circle(centre + Vector3.up * Lift, radius, 0.06f, colour, 0.55f + 0.4f * pulse);
            Circle(centre + Vector3.up * Lift, radius * 0.45f, 0.04f, colour, 0.35f);
        }

        private void Circle(Vector3 centre, float radius, float width, Color colour, float alpha)
        {
            const int sides = 36;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 d0 = new(Mathf.Cos(a0), 0f, Mathf.Sin(a0)), d1 = new(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                Quad(centre + d0 * radius, centre + d1 * radius, centre + d1 * (radius - width), centre + d0 * (radius - width), colour, alpha);
            }
        }

        private void AreaOutline(TutorialArea area, Color colour, float pulse)
        {
            Vector3 floor = Ground(area.transform.position);
            Vector3 size = area.Size;
            Bounds b = new(new Vector3(area.transform.position.x, floor.y, area.transform.position.z), new Vector3(size.x - 0.3f, 0f, size.z - 0.3f));
            Outline(b, floor.y, colour, pulse, 0.07f);
        }

        // A rectangle on the floor (a room to step into, a floor to brace or keep off).
        private void Outline(Bounds b, float y, Color colour, float pulse, float width)
        {
            float alpha = 0.35f + 0.45f * pulse;
            Vector3 a = new(b.min.x, y + Lift, b.min.z), c = new(b.max.x, y + Lift, b.max.z);
            Vector3 bq = new(c.x, a.y, a.z), d = new(a.x, a.y, c.z);
            Edge(a, bq, Vector3.forward, width, colour, alpha);
            Edge(bq, c, Vector3.left, width, colour, alpha);
            Edge(c, d, Vector3.back, width, colour, alpha);
            Edge(d, a, Vector3.right, width, colour, alpha);
        }

        private void Edge(Vector3 from, Vector3 to, Vector3 inward, float width, Color colour, float alpha) =>
            Quad(from, to, to + inward * width, from + inward * width, colour, alpha);

        private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colour, float alpha)
        {
            if (alpha <= 0.01f) return;
            int i = verts.Count;
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            verts.Add(d);
            Color tint = new(colour.r, colour.g, colour.b, alpha);
            for (int k = 0; k < 4; k++) colours.Add(tint);
            // Both faces: the floor may be seen from either side through a hole.
            tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
            tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
            tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
        }

        // The authored material (built with the training content, so its shader is in every build); a plain one
        // made here if it is missing.
        private static Material PathMaterial()
        {
            Material authored = Resources.Load<Material>(MaterialResource);
            if (authored != null) return authored;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return null;
            var m = new Material(shader) { name = "TrainingGuidePath_Fallback" };
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return m;
        }
    }
}

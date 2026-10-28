using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// One box a map builder drew (a scaled cube or a 24-vertex box mesh), in its own frame and in metres: where it is,
    /// which way its axes point and how big it is along each. The rebuild draws modelled construction inside these
    /// bounds; the box's collider stays as it is.
    /// </summary>
    public sealed class ArchBox
    {
        public enum Role { None, Wall, Slab, Column, Beam, Step, Pane, Roof }

        public MeshRenderer Renderer;
        public Transform Transform;
        public Material Material;
        public Vector3 Centre;   // world
        public Quaternion Rotation; // world, unscaled
        public Vector3 Size;     // metres along the box's local x, y, z
        public Role Kind;
        public ArchStyle.Surface Surface;
        public bool Upright;

        public Vector3 Half => Size * .5f;
        public Vector3 Right => Rotation * Vector3.right;
        public Vector3 Up => Rotation * Vector3.up;
        public Vector3 Forward => Rotation * Vector3.forward;
        public float Bottom => Centre.y - Half.y;
        public float Top => Centre.y + Half.y;

        public Vector3 ToWorld(Vector3 local) => Centre + Rotation * local;
        public Vector3 ToLocal(Vector3 world) => Quaternion.Inverse(Rotation) * (world - Centre);
        public Vector3 DirToWorld(Vector3 local) => Rotation * local;

        /// <summary>True when the point lies inside the box grown by <paramref name="grow"/> on every side.</summary>
        public bool Contains(Vector3 world, float grow = 0f)
        {
            Vector3 p = ToLocal(world), h = Half;
            return Mathf.Abs(p.x) <= h.x + grow && Mathf.Abs(p.y) <= h.y + grow && Mathf.Abs(p.z) <= h.z + grow;
        }

        /// <summary>The part [t0, t1] of the segment from <paramref name="a"/> to <paramref name="b"/> (t in 0..1) inside the
        /// box, or false.</summary>
        public bool Clip(Vector3 a, Vector3 b, out float t0, out float t1)
        {
            Vector3 p = ToLocal(a), d = ToLocal(b) - p, h = Half;
            t0 = 0f;
            t1 = 1f;
            for (int i = 0; i < 3; i++)
            {
                if (Mathf.Abs(d[i]) < 1e-7f)
                {
                    if (Mathf.Abs(p[i]) > h[i]) return false;
                    continue;
                }
                float ta = (-h[i] - p[i]) / d[i], tb = (h[i] - p[i]) / d[i];
                if (ta > tb) (ta, tb) = (tb, ta);
                t0 = Mathf.Max(t0, ta);
                t1 = Mathf.Min(t1, tb);
                if (t0 > t1) return false;
            }
            return true;
        }

        public Bounds WorldBounds
        {
            get
            {
                Vector3 h = Half;
                Vector3 e = new(
                    Mathf.Abs(Right.x) * h.x + Mathf.Abs(Up.x) * h.y + Mathf.Abs(Forward.x) * h.z,
                    Mathf.Abs(Right.y) * h.x + Mathf.Abs(Up.y) * h.y + Mathf.Abs(Forward.y) * h.z,
                    Mathf.Abs(Right.z) * h.x + Mathf.Abs(Up.z) * h.y + Mathf.Abs(Forward.z) * h.z);
                return new Bounds(Centre, e * 2f);
            }
        }

        /// <summary>The box a renderer draws, or null when its mesh is not a plain box.</summary>
        public static ArchBox From(MeshRenderer renderer)
        {
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = filter != null ? filter.sharedMesh : null;
            if (mesh == null || mesh.vertexCount != 24 || mesh.subMeshCount != 1) return null;
            Transform t = renderer.transform;
            Vector3 scale = t.lossyScale;
            Vector3 size = Vector3.Scale(mesh.bounds.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            if (size.x < 1e-4f || size.y < 1e-4f || size.z < 1e-4f) return null;
            Quaternion rotation = t.rotation;
            return new ArchBox
            {
                Renderer = renderer,
                Transform = t,
                Material = renderer.sharedMaterial,
                Centre = t.TransformPoint(mesh.bounds.center),
                Rotation = rotation,
                Size = size,
                Upright = Vector3.Dot(rotation * Vector3.up, Vector3.up) > .999f,
            };
        }
    }
}

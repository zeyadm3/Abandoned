using System.Collections.Generic;
using Abandoned.Audio;
using Abandoned.Equipment;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>
    /// Spray chalk marks for the job (0.17.6), on the run state's object: the host adds a dot when an owner
    /// sprays (oldest go first past the cap); every machine draws them all as one mesh of small quads, in each
    /// crew seat's colour. A new run starts clean.
    /// </summary>
    public class RunChalk : NetworkBehaviour
    {
        private static readonly Color[] SeatColours =
        {
            new(1f, 0.55f, 0.1f), new(0.2f, 0.9f, 0.4f), new(0.3f, 0.7f, 1f), new(1f, 0.3f, 0.8f),
        };

        private readonly NetworkList<ChalkDot> dots = new();
        private Mesh mesh;
        private GameObject drawn;
        // Kept between rebuilds: the mesh is rebuilt on every new dot (about 16 a second while someone
        // sprays), and three fresh lists of up to 800 dots each time was over a megabyte of garbage a second.
        private readonly List<Vector3> verts = new();
        private readonly List<Color> colours = new();
        private readonly List<int> tris = new();
        private bool dirty;
        private float nextHiss;

        public static RunChalk Current { get; private set; }

        public override void OnNetworkSpawn()
        {
            Current = this;
            dots.OnListChanged += OnChanged;
            dirty = true;
        }

        public override void OnNetworkDespawn()
        {
            dots.OnListChanged -= OnChanged;
            if (Current == this) Current = null;
            if (drawn != null) Destroy(drawn);
            // A mesh made in code isn't destroyed with the object that shows it: one was left behind per job.
            if (mesh != null) Destroy(mesh);
        }

        private void OnChanged(NetworkListEvent<ChalkDot> change)
        {
            dirty = true;
            if (change.Type == NetworkListEvent<ChalkDot>.EventType.Add && Time.time >= nextHiss)
            {
                nextHiss = Time.time + 0.25f;
                GameAudio.Play(SoundId.ChalkSpray, change.Value.Position, 1f);
            }
        }

        public void ServerAdd(Vector3 at, Vector3 normal, byte seat)
        {
            if (!IsServer) return;
            if (dots.Count >= ToolConfig.Current.ChalkMaxDots) dots.RemoveAt(0);
            dots.Add(new ChalkDot { Position = at + normal * 0.01f, Normal = normal, Seat = seat });
        }

        private void LateUpdate()
        {
            if (!dirty || !IsSpawned) return;
            dirty = false;
            Rebuild();
        }

        private void Rebuild()
        {
            if (drawn == null)
            {
                drawn = new GameObject("ChalkMarks");
                mesh = new Mesh { name = "ChalkMarks", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                drawn.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = drawn.AddComponent<MeshRenderer>();
                r.sharedMaterial = ToolConfig.Current.ChalkMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            float half = ToolConfig.Current.ChalkDotSize * 0.5f;
            verts.Clear();
            colours.Clear();
            tris.Clear();
            foreach (ChalkDot d in dots)
            {
                Vector3 n = d.Normal.sqrMagnitude > 0.1f ? d.Normal.normalized : Vector3.up;
                Vector3 u = Vector3.Cross(n, Mathf.Abs(n.y) > 0.9f ? Vector3.forward : Vector3.up).normalized * half;
                Vector3 v = Vector3.Cross(n, u).normalized * half;
                int i = verts.Count;
                verts.Add(d.Position - u - v); verts.Add(d.Position - u + v); verts.Add(d.Position + u + v); verts.Add(d.Position + u - v);
                Color c = SeatColours[d.Seat % SeatColours.Length];
                for (int k = 0; k < 4; k++) colours.Add(c);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2); tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);
            }
            mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetColors(colours);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Current = null;
    }
}

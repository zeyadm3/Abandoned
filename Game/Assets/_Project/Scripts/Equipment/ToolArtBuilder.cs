using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Equipment
{
    /// <summary>Build-once assembly: fine mechanical parts become one renderer for each shared finish.</summary>
    public sealed class ToolArtBuilder
    {
        private readonly Dictionary<Material, List<CombineInstance>> surfaces = new();
        public readonly Material Paint, Rubber, Steel, Canvas, Leather, Glass, Screen, Tape, Wood, Dark;

        public ToolArtBuilder(Color colour)
        {
            Paint = CharacterSurfaceLibrary.Get("Paint", colour);
            Rubber = CharacterSurfaceLibrary.Get("Rubber");
            Steel = CharacterSurfaceLibrary.Get("Steel");
            Canvas = CharacterSurfaceLibrary.Get("Canvas", colour);
            Leather = CharacterSurfaceLibrary.Get("Leather");
            Glass = CharacterSurfaceLibrary.Get("Glass");
            Screen = CharacterSurfaceLibrary.Get("Screen");
            Tape = CharacterSurfaceLibrary.Get("Tape");
            Wood = CharacterSurfaceLibrary.Get("Wood");
            Dark = CharacterSurfaceLibrary.Get("CrewTrim");
        }

        public void Box(Vector3 at, Vector3 size, Material material, Vector3 euler = default) =>
            Add(ToolArtMeshes.Box, at, size, Quaternion.Euler(euler), material);

        public void Cylinder(Vector3 at, Vector3 size, Material material, Vector3 euler = default) =>
            Add(ToolArtMeshes.Cylinder, at, size, Quaternion.Euler(euler), material);

        public void Disc(Vector3 at, float diameter, float depth, Material material) =>
            Add(ToolArtMeshes.Cylinder, at, new Vector3(diameter, depth, diameter), Quaternion.Euler(90f, 0f, 0f), material);

        public void Ring(Vector3 at, float diameter, float thickness, Material material, Vector3 euler = default) =>
            Add(ToolArtMeshes.Ring, at, new Vector3(diameter, thickness, diameter), Quaternion.Euler(euler), material);

        public void Rod(Vector3 from, Vector3 to, float diameter, Material material)
        {
            Vector3 along = to - from;
            Add(ToolArtMeshes.Cylinder, (from + to) * .5f, new Vector3(diameter, along.magnitude, diameter),
                Quaternion.FromToRotation(Vector3.up, along.normalized), material);
        }

        public void Beam(Vector3 from, Vector3 to, float width, float depth, Material material)
        {
            Vector3 along = to - from;
            Add(ToolArtMeshes.Box, (from + to) * .5f, new Vector3(width, depth, along.magnitude),
                Quaternion.LookRotation(along.normalized), material);
        }

        public void Screws(Vector3 centre, float width, float length, float y, int rows = 2)
        {
            for (int row = 0; row < rows; row++)
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 at = centre + new Vector3(side * width * .5f, y, rows == 1 ? 0f : (row / (float)(rows - 1) - .5f) * length);
                Cylinder(at, new Vector3(.008f, .003f, .008f), Steel);
                Box(at + Vector3.up * .002f, new Vector3(.005f, .001f, .0015f), Dark);
            }
        }

        public void Grille(Vector3 at, Vector2 size, int count, Material frame)
        {
            Box(at, new Vector3(size.x, .003f, size.y), Dark);
            for (int i = 0; i < count; i++)
                Box(at + new Vector3(0f, .002f, (i / (float)Mathf.Max(1, count - 1) - .5f) * size.y * .86f),
                    new Vector3(size.x * .88f, .0025f, .003f), frame);
        }

        public void Grip(Vector3 from, Vector3 to, float diameter)
        {
            Rod(from, to, diameter, Rubber);
            for (int i = 1; i < 7; i++)
                Add(ToolArtMeshes.Ring, Vector3.Lerp(from, to, i / 7f), new Vector3(diameter * 1.04f, .004f, diameter * 1.04f),
                    Quaternion.FromToRotation(Vector3.up, (to - from).normalized), Dark);
        }

        public void Cable(Vector3 from, Vector3 to, Vector3 sag, float diameter, Material material)
        {
            Vector3 previous = from;
            for (int i = 1; i <= 9; i++)
            {
                float t = i / 9f;
                Vector3 point = Vector3.Lerp(from, to, t) + sag * Mathf.Sin(t * Mathf.PI);
                Rod(previous, point, diameter, material);
                previous = point;
            }
        }

        private void Add(Mesh mesh, Vector3 at, Vector3 scale, Quaternion rotation, Material material)
        {
            if (!surfaces.TryGetValue(material, out var parts)) surfaces[material] = parts = new List<CombineInstance>();
            parts.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(at, rotation, scale) });
        }

        public List<(Mesh mesh, Material material)> Finish(string name)
        {
            var result = new List<(Mesh, Material)>();
            foreach (var surface in surfaces)
            {
                var mesh = new Mesh { name = name + "_" + surface.Key.name, indexFormat = IndexFormat.UInt32, hideFlags = HideFlags.DontSave };
                mesh.CombineMeshes(surface.Value.ToArray(), true, true);
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                result.Add((mesh, surface.Key));
            }
            return result;
        }
    }
}

using UnityEngine;

namespace Abandoned.Structure
{
    /// <summary>Legacy crack slivers and the shared warning dust; authored stage meshes use neither slivers nor their material.</summary>
    public partial class SectionPresentation
    {
        /// <summary>Thin dark slivers on the walking surface, seeded per section so they're the same each time.</summary>
        private void BuildCracks(int count)
        {
            if (cracks == null)
            {
                cracks = new GameObject("Cracks").transform;
                cracks.SetParent(transform, false);
            }
            cracks.localPosition = Vector3.zero;
            if (count == cracks.childCount) return;
            // Detach first: Destroy is deferred, and childCount must be right straight away.
            for (int i = cracks.childCount - 1; i >= 0; i--)
            {
                Transform old = cracks.GetChild(i);
                old.SetParent(null);
                Destroy(old.gameObject);
            }
            if (count == 0 || section.Type == SectionType.Stair) return;

            Bounds surface = section.SurfaceBounds;
            var crackRandom = new System.Random(section.Id * 101 + 7);
            Material material = CrackMaterial();
            for (int i = 0; i < count; i++)
            {
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(crack.GetComponent<Collider>());
                crack.name = "Crack";
                crack.transform.SetParent(cracks, true);
                float length = Mathf.Lerp(visuals.CrackLength.x, visuals.CrackLength.y, (float)crackRandom.NextDouble());
                Vector3 at = new(
                    Mathf.Lerp(surface.min.x + 0.3f, surface.max.x - 0.3f, (float)crackRandom.NextDouble()),
                    surface.max.y + 0.004f,
                    Mathf.Lerp(surface.min.z + 0.3f, surface.max.z - 0.3f, (float)crackRandom.NextDouble()));
                crack.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, crackRandom.Next(180), 0f));
                crack.transform.localScale = new Vector3(visuals.CrackWidth, 0.006f, length);
                crack.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static Material crackMaterial;

        private Material CrackMaterial()
        {
            if (crackMaterial != null) return crackMaterial;
            if (renderers.Length == 0 || renderers[0].sharedMaterial == null) return null;
            // Built from an existing material so it uses the same (always included) URP Lit shader.
            crackMaterial = new Material(renderers[0].sharedMaterial) { name = "Crack" };
            crackMaterial.SetColor(BaseColor, visuals.CrackColor);
            return crackMaterial;
        }

        private void CreateDust()
        {
            var go = new GameObject("Dust");
            go.transform.SetParent(transform, false);
            Bounds b = section.SurfaceBounds;
            // Dust trickles from the underside: what you'd see from the room below.
            go.transform.position = new Vector3(b.center.x, b.min.y - 0.05f, b.center.z);
            dust = go.AddComponent<ParticleSystem>();
            dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = dust.main;
            main.startLifetime = 2.5f;
            main.startSpeed = 0.15f;
            main.startSize = 0.07f;
            main.gravityModifier = 0.08f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.75f, 0.7f, 0.62f, 0.8f);
            ParticleSystem.ShapeModule shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(b.size.x * 0.9f, 0.05f, b.size.z * 0.9f);
            ParticleSystem.EmissionModule emission = dust.emission;
            emission.rateOverTime = 0f;
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = visuals.DustMaterial;
            dust.Play();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.Core
{
    /// <summary>
    /// Grass on a level's own ground (its lawns, verges and dirt, which are meshes, not a terrain): tufts placed by the
    /// editor (MapGrass) and drawn here, instanced, a chunk at a time and only near the camera. Cosmetic and local: no
    /// colliders, never networked, never touches gameplay. Draws in the editor too, so a saved map shows its grass.
    /// </summary>
    [ExecuteAlways]
    public class GrassField : MonoBehaviour
    {
        [Serializable]
        public class Chunk
        {
            public Bounds bounds;
            public Matrix4x4[] tufts;
        }

        public const int MaxPerChunk = 1023;

        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [Tooltip("Chunks further than this from the camera aren't drawn (fog hides the edge).")]
        [SerializeField] private float drawDistance = 70f;
        [SerializeField] private List<Chunk> chunks = new();

        public int TuftCount
        {
            get
            {
                int n = 0;
                foreach (Chunk c in chunks) n += c.tufts.Length;
                return n;
            }
        }

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        // Per camera, as it starts to render: each camera (the player's, a preview, the scene view) draws the chunks
        // near it, and a camera rendered by hand (a capture) gets its grass too.
        private void Draw(ScriptableRenderContext context, Camera cam)
        {
            if (mesh == null || material == null || chunks == null || chunks.Count == 0 || cam.cameraType == CameraType.Preview) return;
            Vector3 eye = cam.transform.position;
            float far = drawDistance * drawDistance;
            var settings = new RenderParams(material)
                { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = gameObject.layer, camera = cam };
            foreach (Chunk chunk in chunks)
            {
                if (chunk.tufts == null || chunk.tufts.Length == 0 || chunk.bounds.SqrDistance(eye) > far) continue;
                settings.worldBounds = chunk.bounds;
                Graphics.RenderMeshInstanced(settings, mesh, 0, chunk.tufts);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Mesh tuft, Material grass, float distance, List<Chunk> placed)
        {
            mesh = tuft;
            material = grass;
            drawDistance = distance;
            chunks = placed;
        }
#endif
    }
}

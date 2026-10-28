using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>Review captures of the open map from given eye points (a temporary camera into an x4 texture: x8 comes back
    /// empty on this Mac). Writes Game/Logs/remake/arch/shots/&lt;name&gt;_&lt;i&gt;.png.</summary>
    public static class ArchShots
    {
        /// <summary>Frames <paramref name="count"/> rebuilt walls of the open map (spread through the record), each from 3.5 m
        /// in front on the side with a roof over it (inside) when it has one.</summary>
        public static string Auto(string name, int count, string mustContain = null)
        {
            GameObject root = GameObject.Find("_Arch");
            var record = root != null ? root.GetComponent<Abandoned.Core.ArchRecord>() : null;
            if (record == null) return "no record";
            var picks = new System.Collections.Generic.List<Vector3>();
            var walls = new System.Collections.Generic.List<Transform>();
            foreach (GameObject d in record.Added)
            {
                if (d == null) continue;
                Mesh m = d.GetComponent<MeshFilter>().sharedMesh;
                if (m == null || !m.name.EndsWith("_Wall") || (mustContain != null && !m.name.Contains(mustContain))) continue;
                Transform box = d.transform.parent;
                Vector3 size = Vector3.Scale(box.GetComponent<MeshFilter>().sharedMesh.bounds.size, Abs(box.lossyScale));
                if (Mathf.Max(size.x, size.z) < 2.5f || size.y < 2.2f) continue;
                walls.Add(box);
            }
            for (int i = 0; i < count && walls.Count > 0; i++)
            {
                Transform box = walls[(int)((i + .5f) / count * walls.Count)];
                Vector3 size = Vector3.Scale(box.GetComponent<MeshFilter>().sharedMesh.bounds.size, Abs(box.lossyScale));
                Vector3 normal = size.x >= size.z ? box.forward : box.right;
                Vector3 centre = box.GetComponent<MeshRenderer>() != null ? box.position : box.GetChild(0).GetComponent<Renderer>().bounds.center;
                float bottom = centre.y - size.y * .5f;
                Vector3 target = new(centre.x, bottom + 1.4f, centre.z);
                Vector3 eye = target + normal * 3.5f + Vector3.up * .3f;
                if (!Physics.Raycast(eye, Vector3.up, 60f) && Physics.Raycast(target - normal * 3.5f, Vector3.up, 60f))
                    eye = target - normal * 3.5f + Vector3.up * .3f;
                picks.Add(eye);
                picks.Add(target + (target - eye).normalized * 0f);
            }
            return Capture(name, picks.ToArray());
        }

        private static Vector3 Abs(Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static string Capture(string name, params Vector3[] eyeThenTarget)
        {
            Directory.CreateDirectory(ArchRebuild.LogFolder + "/shots");
            var go = new GameObject("_ArchShotCam");
            var cam = go.AddComponent<Camera>();
            go.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            cam.fieldOfView = 70f;
            cam.nearClipPlane = .05f;
            cam.farClipPlane = 800f;
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            string written = "";
            for (int i = 0; i + 1 < eyeThenTarget.Length; i += 2)
            {
                go.transform.position = eyeThenTarget[i];
                go.transform.LookAt(eyeThenTarget[i + 1]);
                cam.targetTexture = rt;
                cam.Render();
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                string path = $"{ArchRebuild.LogFolder}/shots/{name}_{i / 2}.png";
                File.WriteAllBytes(path, tex.EncodeToPNG());
                written += path + " ";
            }
            cam.targetTexture = null;
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(tex);
            rt.Release();
            return written;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The bay round Gallows Rock, so the island sits somewhere: the mainland's shore across the water to the north and
    /// east (wooded hills, mountains behind them), the city on it with its windows and aviation lights, a long
    /// suspension bridge to the west, two smaller islands and a freighter at anchor, in the noon haze.
    /// Far scenery uses the mall's trick: an unlit shader with its own distance haze (<see cref="Shader"/>), because
    /// the bay's local fog would wipe out anything this far away. Daylight glass, pale mountain layers and warmer
    /// bridge steel keep the horizon readable without the city becoming a second nighttime lighting setup.
    /// Static, no colliders, out of the NavMesh. Positions are metres from the island's middle.
    /// </summary>
    public static class IslandBackdrop
    {
        public const string GroupName = "Backdrop";
        private const string Folder = IslandPrisonArt.Folder + "/Backdrop";
        private static readonly Vector3 Centre = new(66f, L.SeaY, 69f);
        // The noon sea haze (matches the scene's fog), so distant land fades into the same air.
        private static readonly Color Haze = IslandPrisonBuilder.NoonHaze;

        public static void Build(Transform root)
        {
            Transform g = Group(GroupName, root);
            // Broad value steps keep the bay legible at noon: nearer islands, mainland hills, pale far mountains.
            // Navigation lenses remain coloured fittings in daylight; none bypasses the atmospheric perspective.
            Material hills = Mat("Backdrop_Hills", new Color(.32f, .39f, .33f), 1750f, 0f);
            Material mountains = Mat("Backdrop_Mountains", new Color(.47f, .55f, .62f), 1900f, 0f);
            Material nearLand = Mat("Backdrop_NearLand", new Color(.27f, .34f, .29f), 2100f, 0f);
            Material concrete = Mat("Backdrop_Concrete", new Color(.7f, .7f, .65f), 1600f, 0f);
            Material steel = Mat("Backdrop_Steel", new Color(.56f, .25f, .17f), 1900f, 0f);
            Material windows = Mat("Backdrop_Windows", new Color(.27f, .37f, .42f), 1600f, 0f);
            Material shipHull = Mat("Backdrop_Hull", new Color(.31f, .35f, .36f), 1900f, 0f);
            Material red = Mat("Backdrop_RedLamp", new Color(.49f, .17f, .12f), 2000f, 0f);
            Material white = Mat("Backdrop_WhiteLamp", new Color(.76f, .78f, .72f), 2200f, 0f);

            // The mainland: a near line of wooded hills along the north and east, mountains behind.
            // The hills break where the city comes down to the water.
            Ridge(g, "Mainland_Hills_W", -60f, 12f, 720f, 30f, 110f, 31, hills);
            Ridge(g, "Mainland_Hills_E", 46f, 115f, 720f, 30f, 110f, 37, hills);
            Ridge(g, "Mainland_Mountains", -75f, 125f, 1250f, 120f, 320f, 47, mountains);
            // The west shore the bridge reaches, and the south headland.
            Ridge(g, "WestShore", 160f, 215f, 820f, 25f, 90f, 53, hills);
            Ridge(g, "SouthHeadland", 245f, 290f, 900f, 20f, 70f, 59, hills);
            City(g, concrete, windows, red);
            Bridge(g, steel, white, red);
            SmallIsland(g, "EastIsland", Centre + new Vector3(390f, 0f, -230f), new Vector3(130f, 34f, 80f), nearLand, white);
            SmallIsland(g, "WestRock", Centre + new Vector3(-280f, 0f, 300f), new Vector3(70f, 16f, 45f), nearLand, null);
            Freighter(g, Centre + new Vector3(260f, 0f, 250f), 68f, shipHull, steel, white, red);
            Shadowless(g);
            MarkStatic(g);
        }

        // A strip of hills between two bearings (degrees from north, clockwise) at a distance: a jagged top line made
        // of overlapping noise, faces down into the sea, one mesh.
        private static void Ridge(Transform g, string name, float fromDeg, float toDeg, float distance, float low, float high, int seed, Material m)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            int steps = Mathf.CeilToInt((toDeg - fromDeg) / 1.2f);
            for (int i = 0; i <= steps; i++)
            {
                float deg = Mathf.Lerp(fromDeg, toDeg, i / (float)steps), rad = deg * Mathf.Deg2Rad;
                float n = Mathf.PerlinNoise(i * .09f + seed, seed * .3f) * .7f + Mathf.PerlinNoise(i * .31f, seed) * .3f;
                float r = distance + (Mathf.PerlinNoise(i * .05f, seed * 1.7f) - .5f) * distance * .12f;
                var dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
                Vector3 baseAt = Centre + dir * r + Vector3.down * 2f;
                Vector3 top = Centre + dir * (r + 60f) + Vector3.up * Mathf.Lerp(low, high, n);
                verts.Add(baseAt);
                verts.Add(top);
                if (i == 0) continue;
                int k = verts.Count - 4;
                tris.AddRange(new[] { k, k + 1, k + 3, k, k + 3, k + 2 });
            }
            Place(g, name, verts, tris, m);
        }

        // The city on the mainland to the north-north-east: blocks of towers, their windows, red lamps on the tallest.
        private static void City(Transform g, Material concrete, Material windows, Material red)
        {
            var kit = IslandMeshKit.Get("Backdrop_City", r =>
            {
                for (int i = 0; i < 90; i++)
                {
                    float a = IslandKit.Hash(i, 301), b = IslandKit.Hash(301, i), c = IslandKit.Hash(i, 302);
                    float along = (a - .5f) * 520f, back = b * 160f;
                    float tall = 12f + Mathf.Pow(c, 2.2f) * (Mathf.Abs(along) < 120f ? 140f : 60f);
                    float w = 14f + c * 26f, d = 14f + b * 20f;
                    var at = new Vector3(along, tall * .5f, back);
                    Box("Tower", r, at, new Vector3(w, tall, d), concrete, false);
                    // Blue-grey daylight glazing on the bay face: a few broken bands, never the whole face.
                    int bands = Mathf.Max(1, Mathf.FloorToInt(tall / 7f));
                    for (int k = 0; k < bands; k++)
                    {
                        if (IslandKit.Hash(i * 31 + k, 303) < .45f) continue;
                        float span = w * (.3f + IslandKit.Hash(k, i) * .6f);
                        Box("Windows", r, new Vector3(along + (IslandKit.Hash(i, k) - .5f) * (w - span), 4f + k * 7f, back - d * .5f - .3f),
                            new Vector3(span, 1.6f, .3f), windows, false);
                    }
                    if (tall > 70f) Box("AirLamp", r, new Vector3(along, tall + 1.5f, back), new Vector3(2.5f, 2.5f, 2.5f), red, false);
                }
                // The waterfront: a low quay of warehouses and a string of street lamps.
                for (float x = -280f; x < 280f; x += 22f)
                {
                    Box("Warehouse", r, new Vector3(x, 6f, -30f), new Vector3(18f, 12f, 16f), concrete, false);
                    Box("Lamp", r, new Vector3(x + 11f, 7f, -40f), new Vector3(1.5f, 1.5f, 1.5f), windows, false);
                }
            });
            float bearing = 28f * Mathf.Deg2Rad;
            Vector3 dir = new(Mathf.Sin(bearing), 0f, Mathf.Cos(bearing));
            IslandMeshKit.Place(kit, "City", g, Centre + dir * 700f, Quaternion.LookRotation(dir), false);
        }

        // A long suspension bridge across the western bay: two towers, the deck, the main cables in segments, the
        // suspenders, a lamp every 30 m and red lamps on the tower tops.
        private static void Bridge(Transform g, Material steel, Material white, Material red)
        {
            var kit = IslandMeshKit.Get("Backdrop_Bridge", r =>
            {
                const float deck = 62f, length = 1400f, towerH = 230f;
                float[] towers = { -330f, 330f };
                Box("Deck", r, new Vector3(0f, deck, 0f), new Vector3(28f, 6f, length), steel, false);
                foreach (float z in towers)
                {
                    foreach (float x in new[] { -12f, 12f })
                        Box("TowerLeg", r, new Vector3(x, towerH * .5f - 5f, z), new Vector3(7f, towerH, 9f), steel, false);
                    for (float y = 90f; y < towerH; y += 55f)
                        Box("TowerStrut", r, new Vector3(0f, y, z), new Vector3(24f, 5f, 6f), steel, false);
                    Box("TowerLamp", r, new Vector3(0f, towerH + 2f, z), new Vector3(4f, 4f, 4f), red, false);
                }
                // Cables: the main span sags between the towers, the side spans run down to the anchorages.
                foreach (float x in new[] { -12f, 12f })
                {
                    Cable(r, new Vector3(x, towerH - 8f, towers[0]), new Vector3(x, towerH - 8f, towers[1]), 140f, steel, deck);
                    Cable(r, new Vector3(x, deck + 4f, -length * .5f), new Vector3(x, towerH - 8f, towers[0]), 20f, steel, deck);
                    Cable(r, new Vector3(x, towerH - 8f, towers[1]), new Vector3(x, deck + 4f, length * .5f), 20f, steel, deck);
                }
                for (float z = -length * .5f + 15f; z < length * .5f; z += 30f)
                foreach (float x in new[] { -14f, 14f })
                    Box("DeckLamp", r, new Vector3(x, deck + 6f, z), new Vector3(1.4f, 1.4f, 1.4f), white, false);
                foreach (float z in towers) Box("Pier", r, new Vector3(0f, deck * .5f - 5f, z), new Vector3(34f, deck, 18f), steel, false);
            });
            float bearing = 255f * Mathf.Deg2Rad;
            Vector3 dir = new(Mathf.Sin(bearing), 0f, Mathf.Cos(bearing));
            Vector3 at = Centre + dir * 820f;
            // The bridge runs across the line of sight, so you see its length.
            IslandMeshKit.Place(kit, "Bridge", g, at, Quaternion.LookRotation(Vector3.Cross(Vector3.up, dir)), false);
        }

        // A cable as straight segments along a sagging curve, with suspenders down to the deck.
        private static void Cable(Transform r, Vector3 a, Vector3 b, float sag, Material m, float deck)
        {
            const int segments = 16;
            Vector3 prev = a;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * (sag * 4f * t * (1f - t));
                Vector3 d = p - prev;
                GameObject seg = Box("Cable", r, (p + prev) * .5f, new Vector3(1.2f, 1.2f, d.magnitude), m, false);
                seg.transform.localRotation = Quaternion.LookRotation(d.normalized);
                if (p.y - deck > 8f && i < segments)
                    Box("Suspender", r, new Vector3(p.x, (p.y + deck) * .5f, p.z), new Vector3(.5f, p.y - deck, .5f), m, false);
                prev = p;
            }
        }

        private static void SmallIsland(Transform g, string name, Vector3 at, Vector3 size, Material m, Material lamp)
        {
            Transform island = Group(name, g);
            for (int i = 0; i < 7; i++)
            {
                float h = IslandKit.Hash(i, name.Length * 7);
                Vector3 off = new((h - .5f) * size.x * .7f, 0f, (IslandKit.Hash(name.Length, i) - .5f) * size.z * .6f);
                PolishAssets.Shape("Hill", island, at + off, new Vector3(size.x * (.35f + h * .4f), size.y * (.4f + h * .8f), size.z * (.4f + h * .5f)), m, true);
            }
            if (lamp == null) return;
            Box("LightTower", island, at + new Vector3(size.x * .3f, size.y * .7f, 0f), new Vector3(3f, size.y * .8f, 3f), m, false);
            Box("LightTowerLamp", island, at + new Vector3(size.x * .3f, size.y * 1.12f, 0f), new Vector3(3.2f, 3.2f, 3.2f), lamp, false);
        }

        private static void Freighter(Transform g, Vector3 at, float heading, Material hull, Material rust, Material white, Material red)
        {
            Transform ship = Group("Freighter", g);
            ship.SetPositionAndRotation(at, Quaternion.Euler(0f, heading, 0f));
            Box("Hull", ship, new Vector3(0f, 5f, 0f), new Vector3(22f, 12f, 150f), hull, false);
            Box("Boot", ship, new Vector3(0f, 1f, 0f), new Vector3(22.4f, 3f, 150.4f), rust, false);
            Box("Bridge", ship, new Vector3(0f, 19f, -58f), new Vector3(20f, 16f, 14f), hull, false);
            Box("Funnel", ship, new Vector3(0f, 30f, -62f), new Vector3(5f, 8f, 5f), rust, false);
            for (int i = 0; i < 6; i++) Box("Containers", ship, new Vector3(0f, 13.5f, -30f + i * 16f), new Vector3(18f, 5f, 13f), rust, false);
            Box("Mast", ship, new Vector3(0f, 26f, 60f), new Vector3(.8f, 22f, .8f), hull, false);
            Box("AnchorLight", ship, new Vector3(0f, 37.5f, 60f), new Vector3(1.6f, 1.6f, 1.6f), white, false);
            Box("SternLight", ship, new Vector3(0f, 28f, -58f), new Vector3(1.6f, 1.6f, 1.6f), white, false);
            for (int i = 0; i < 5; i++) Box("DeckLight", ship, new Vector3(-11.2f, 12f, -50f + i * 25f), new Vector3(.8f, .8f, .8f), white, false);
            Box("PortLamp", ship, new Vector3(-10.5f, 22f, -52f), new Vector3(1f, 1f, 1f), red, false);
        }

        private static void Place(Transform g, string name, List<Vector3> verts, List<int> tris, Material m)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            else mesh.Clear();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            var go = new GameObject(name);
            go.transform.SetParent(g, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = m;
        }

        private static void Shadowless(Transform g)
        {
            foreach (Renderer r in g.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        // ---- The island's own backdrop shader: daylight shading and layered sea haze ------------------------------

        private static Material Mat(string name, Color colour, float hazeDistance, float glow)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader()) { name = name, enableInstancing = true };
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = Shader();
            m.SetColor("_BaseColor", colour);
            m.SetColor("_HazeColor", Haze);
            m.SetFloat("_HazeDistance", hazeDistance);
            m.SetFloat("_Glow", glow);
            EditorUtility.SetDirty(m);
            return m;
        }

        public static Shader Shader()
        {
            const string path = Folder + "/IslandBackdrop.shader";
            PolishAssets.EnsureFolder(Folder);
            // Rewritten whenever the source below changes (a stale copy once shipped a compile error: pink shore).
            string source = @"Shader ""Abandoned/IslandBackdrop""
{
    Properties
    {
        _BaseColor (""Colour"", Color) = (.32,.39,.33,1)
        _HazeColor (""Haze"", Color) = (.66,.71,.76,1)
        _HazeDistance (""Haze distance"", Float) = 1400
        _Glow (""Glow (lamps keep their light through the haze)"", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { ""RenderType""=""Opaque"" ""Queue""=""Geometry"" ""RenderPipeline""=""UniversalPipeline"" }
        Pass
        {
            Name ""IslandBackdrop""
            Tags { ""LightMode""=""SRPDefaultUnlit"" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HazeColor;
                float _HazeDistance;
                half _Glow;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half shade : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                half3 normal = TransformObjectToWorldNormal(input.normalOS);
                // The same high sun as the island: a soft daylight floor keeps unlit distant faces open.
                output.shade = .8h + .2h * saturate(dot(normal, normalize(half3(-.19h, .93h, .33h))));
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float bayDistance = length(_WorldSpaceCameraPos.xz - input.positionWS.xz);
                half distanceMist = 1.0h - exp2(-bayDistance * 1.7 / max(_HazeDistance, 1));
                // Dense air stays close to the water; summits retain a little more shape above the horizon band.
                half seaMist = .16h * exp2(-max(input.positionWS.y + 2.5, 0.0) / 90.0);
                half mist = saturate(distanceMist + seaMist) * (1.0h - _Glow * .85h);
                half3 lit = _BaseColor.rgb * lerp(input.shade, (half)1.0, _Glow);
                return half4(lerp(lit, _HazeColor.rgb, mist), 1);
            }
            ENDHLSL
        }
    }
}";
            if (!File.Exists(path) || File.ReadAllText(path) != source)
            {
                File.WriteAllText(path, source);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            return AssetDatabase.LoadAssetAtPath<Shader>(path);
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.MeridianWorldSurroundings;

namespace Abandoned.EditorTools
{
    /// <summary>A visible closed estate boundary keeps distant scenery outside the playable arrival court.</summary>
    public static class MeridianWorldBoundary
    {
        private const float Height = 2.55f;
        private static Material steel, concrete;

        public static void Build(Transform parent)
        {
            steel = Material("EstateBoundarySteel", "FixtureTrim", new Color(.31f, .34f, .31f));
            concrete = Material("EstateFenceFootings", "Concrete", new Color(.43f, .45f, .41f));
            Transform boundary = Group("ClosedEstatePerimeter", parent);
            Run(boundary, new Vector3(-18f, -.46f, -40f), new Vector3(-18f, -.46f, 64f));
            Run(boundary, new Vector3(86f, -.46f, -40f), new Vector3(86f, -.46f, 64f));
            Run(boundary, new Vector3(-18f, -.46f, 64f), new Vector3(86f, -.46f, 64f));
            Run(boundary, new Vector3(-18f, -.46f, -40f), new Vector3(23f, -.46f, -40f));
            Run(boundary, new Vector3(33f, -.46f, -40f), new Vector3(86f, -.46f, -40f));
            Gate(boundary);
        }

        private static void Run(Transform parent, Vector3 from, Vector3 to)
        {
            Vector3 offset = to - from;
            float length = offset.magnitude;
            int panels = Mathf.CeilToInt(length / 4f);
            float width = length / panels;
            Transform run = Group("ChainLinkBoundaryRun", parent);
            run.SetLocalPositionAndRotation(from, Quaternion.LookRotation(Vector3.Cross(offset.normalized, Vector3.up)));
            for (int panel = 0; panel < panels; panel++)
                MeshPanel(run, new Vector3(width * (panel + .5f), 0f, 0f), width, "ClosedChainLinkPanel");
            for (int post = 0; post <= panels; post++)
            {
                float x = post * width;
                Box("BoundarySteelPost", run, new Vector3(x, Height * .5f, 0f), new Vector3(.13f, Height + .16f, .13f), steel);
                Box("PostConcreteFooting", run, new Vector3(x, .13f, 0f), new Vector3(.35f, .26f, .35f), concrete);
            }
            foreach (float y in new[] { .11f, Height - .06f })
                Box("BoundaryHorizontalRail", run, new Vector3(length * .5f, y, 0f), new Vector3(length, .055f, .06f), steel, false);
        }

        private static void Gate(Transform parent)
        {
            Transform gate = Group("LockedArrivalEstateGate", parent);
            gate.localPosition = new Vector3(28f, -.32f, -40f);
            foreach (float side in new[] { -1f, 1f })
            {
                MeshPanel(gate, new Vector3(side * 2.5f, 0f, 0f), 4.94f, "ClosedVehicleGateLeaf");
                Box("GateHingePost", gate, new Vector3(side * 5f, Height * .5f, 0f), new Vector3(.20f, Height + .22f, .20f), steel);
                Box("GateCentreStile", gate, new Vector3(side * .035f, Height * .5f, 0f), new Vector3(.08f, Height, .12f), steel, false);
                foreach (float y in new[] { .10f, Height - .04f })
                    Box("GateLeafFrame", gate, new Vector3(side * 2.5f, y, 0f), new Vector3(4.96f, .09f, .13f), steel, false);
                GameObject brace = Box("GateDiagonalBrace", gate, new Vector3(side * 2.5f, Height * .5f, .03f),
                    new Vector3(5.32f, .065f, .07f), steel, false);
                brace.transform.localRotation = Quaternion.Euler(0f, 0f, side * 25f);
            }
            Box("ClosedGateLatch", gate, new Vector3(0f, 1.22f, -.11f), new Vector3(.46f, .12f, .10f), steel, false);
            Box("GatePadlock", gate, new Vector3(.06f, 1.13f, -.16f), new Vector3(.09f, .14f, .06f), steel, false);
        }

        private static void MeshPanel(Transform parent, Vector3 at, float width, string name)
        {
            Transform panel = Group(name, parent);
            panel.localPosition = at;
            var filter = panel.gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = ChainLink();
            var renderer = panel.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = steel;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            panel.localScale = new Vector3(width, Height, 1f);
            var collider = panel.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.up * .5f;
            collider.size = new Vector3(1f, 1f, .12f);
        }

        private static Mesh ChainLink()
        {
            const string folder = "Assets/_Project/Art/Generated/MeridianWorld";
            const string path = folder + "/EstateChainLink.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
            PolishAssets.EnsureFolder(folder);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            const int columns = 18, rows = 15;
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                float x = column / (float)columns - .5f;
                float y = row / (float)rows;
                Wire(new Vector3(x, y, 0f), new Vector3(x + 1f / columns, y + 1f / rows, 0f));
                Wire(new Vector3(x + 1f / columns, y, .008f), new Vector3(x, y + 1f / rows, .008f));
            }
            var mesh = new Mesh { name = "EstateChainLink", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;

            void Wire(Vector3 start, Vector3 end)
            {
                Vector3 across = Vector3.Cross((end - start).normalized, Vector3.forward) * .0017f;
                Vector3 depth = Vector3.forward * .008f;
                Vector3[] corners = { across, depth, -across, -depth };
                for (int side = 0; side < 4; side++)
                {
                    int first = vertices.Count;
                    Vector3 a = corners[side], b = corners[(side + 1) % 4];
                    vertices.AddRange(new[] { start + a, end + a, end + b, start + b });
                    triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
                }
            }
        }
    }
}

using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.16.3 freight lift: an open goods platform in the atrium's north-west void (tile 5,7), from the
    /// ground floor to the upper gallery, entered from the west at both levels. The planter, bin and bench
    /// standing there are switched off (not deleted), and the one gallery balustrade across its upper
    /// landing is switched off for a lift gate. Gates and the ground cage carve the NavMesh at run time.
    /// </summary>
    public static class Release0163
    {
        public const string GroupName = "_FREIGHT_LIFT_0163";
        private static readonly Vector2Int Tile = new(5, 7);
        private const string Balustrade = "Rail_1_4_7_(1, 0)";
        private const float Rise = 4f, Gate = 1.15f;
        private static readonly Vector2 Deck = new(3.6f, 3.4f);

        [MenuItem(ToolMenu.MallUpdates + "0.16.3 Add Freight Lift", priority = ToolMenu.MallP + 30)]
        public static void Author()
        {
            ReleaseKit.Config<LiftConfig>(ReleaseKit.ExtractionResources, LiftConfig.ResourcePath);
            NetworkContentBuilder.CreateRunStatePrefab();
            SoundBankBuilder.CreateMissing();
            Transform mall = MallSites.OpenMall(out var scene);
            if (mall == null) return;
            Build(mall);
            MallSites.SaveMall(scene, "Lift0163");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        public static void Build(Transform mall)
        {
            Transform root = MallSites.FreshGroup(mall, GroupName);
            Vector3 centre = MallLayout.TileTopCenter(Tile, 0);
            centre.x += 0.2f; // flush to the west landing's edge (x = 20)
            Restore();
            ClearFootprint(mall, centre);
            SwitchOffBalustrade();
            root.position = centre;

            Material steel = SalvageMaterials.Get("Metal"), safety = SalvageMaterials.Get("Safety"), rust = SalvageMaterials.Get("Rust");
            // The deck and its rails ride together.
            Transform deck = Group("Deck", root);
            var platform = deck.gameObject.AddComponent<MovingPlatform>();
            var body = deck.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.None;
            deck.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            Box("Plate", deck, new Vector3(0f, -0.06f, 0f), new Vector3(Deck.x, 0.12f, Deck.y), steel);
            Box("EdgeStripe", deck, new Vector3(-Deck.x * 0.5f + 0.06f, 0.005f, 0f), new Vector3(0.12f, 0.01f, Deck.y), safety, false);
            float hx = Deck.x * 0.5f, hz = Deck.y * 0.5f;
            Box("RailN", deck, new Vector3(0f, 0.55f, hz - 0.03f), new Vector3(Deck.x, 1.1f, 0.06f), rust);
            Box("RailS", deck, new Vector3(0f, 0.55f, -hz + 0.03f), new Vector3(Deck.x, 1.1f, 0.06f), rust);
            Box("RailE", deck, new Vector3(hx - 0.03f, 0.55f, 0f), new Vector3(0.06f, 1.1f, Deck.y), rust);
            GameObject deckButton = Button(deck, "DeckButton", new Vector3(hx - 0.12f, 1.15f, -hz + 0.5f), Vector3.left, -1, safety);

            // Guide posts up the shaft (no colliders above head height), and a cage round the ground level's three closed sides.
            Transform cage = Group("Shaft", root);
            float px = hx + 0.12f, pz = hz + 0.12f;
            foreach (Vector2 corner in new[] { new Vector2(-px, -pz), new Vector2(-px, pz), new Vector2(px, -pz), new Vector2(px, pz) })
                Box("GuidePost", cage, new Vector3(corner.x, Rise + 1.5f, corner.y), new Vector3(0.14f, Rise * 2f + 3f, 0.14f), steel);
            Carved(Box("CageN", cage, new Vector3(0f, 1.2f, pz), new Vector3(Deck.x, 2.4f, 0.05f), rust));
            Carved(Box("CageS", cage, new Vector3(0f, 1.2f, -pz), new Vector3(Deck.x, 2.4f, 0.05f), rust));
            Carved(Box("CageE", cage, new Vector3(px, 1.2f, 0f), new Vector3(0.05f, 2.4f, Deck.y), rust));
            Box("Winch", cage, new Vector3(px - 0.4f, Rise * 2f + 2.6f, 0f), new Vector3(0.8f, 0.6f, 1.2f), steel, false);

            // Gates across the west side at each landing: solid unless the platform is parked there.
            GameObject bottomGate = GateAt(root, "BottomGate", 0f, -px, safety);
            GameObject topGate = GateAt(root, "TopGate", Rise, -px, safety);
            GameObject groundButton = Button(root, "GroundButton", new Vector3(-px - 0.15f, 1.2f, -pz - 0.15f), Vector3.left, 0, safety);
            Box("UpperButtonPost", root, new Vector3(-px - 0.3f, Rise + 0.6f, -pz + 0.15f), new Vector3(0.12f, 1.2f, 0.12f), steel);
            GameObject upperButton = Button(root, "UpperButton", new Vector3(-px - 0.38f, Rise + 1.2f, -pz + 0.15f), Vector3.left, 1, safety);

            var config = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            int sector = config != null ? config.SectorOf(centre + Vector3.up * 1.4f) : 0;
            root.gameObject.AddComponent<FreightLift>().EditorSetup(platform, Rise, Deck, bottomGate, topGate,
                new[] { deckButton.GetComponent<LiftButton>(), groundButton.GetComponent<LiftButton>(), upperButton.GetComponent<LiftButton>() }, sector);
            Debug.Log($"[Lift0163] Freight lift at {centre} (power sector {sector}), ground to upper gallery.");
        }

        private static GameObject GateAt(Transform root, string name, float y, float x, Material material)
        {
            GameObject gate = Box(name, root, new Vector3(x - 0.05f, y + Gate * 0.5f, 0f), new Vector3(0.08f, Gate, Deck.y + 0.2f), material);
            Carved(gate);
            return gate;
        }

        private static void Carved(GameObject solid)
        {
            var carve = solid.AddComponent<NavMeshObstacle>();
            carve.shape = NavMeshObstacleShape.Box;
            carve.size = Vector3.one; // unit cube: the object's scale sizes it
            carve.carving = true;
        }

        private static GameObject Button(Transform parent, string name, Vector3 local, Vector3 facing, int landing, Material material)
        {
            GameObject box = Box(name, parent, local, new Vector3(0.14f, 0.22f, 0.14f), material);
            box.transform.localRotation = Quaternion.LookRotation(facing);
            box.AddComponent<LiftButton>().EditorSetup(landing);
            return box;
        }

        // The ground-floor fittings standing where the shaft goes are switched off (kept in the scene).
        private static void ClearFootprint(Transform mall, Vector3 centre)
        {
            Physics.SyncTransforms();
            var off = new HashSet<GameObject>();
            foreach (Collider c in Physics.OverlapBox(centre + Vector3.up * 1.4f, new Vector3(1.95f, 1.3f, 1.85f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            {
                Transform t = c.transform;
                if (c.GetComponentInParent<Structure.StructuralSection>() != null || t.IsChildOf(mall.Find(GroupName))) continue;
                // Up to the fitting itself: the largest ancestor still no bigger than a lift shaft.
                while (t.parent != null && t.parent != mall && Footprint(t.parent) <= 3.2f) t = t.parent;
                if (t == mall || t.name == "Walls" || t.name == "Flights" || t.name == "Columns" || Footprint(t) > 3.2f) continue;
                off.Add(t.gameObject);
            }
            foreach (GameObject g in off)
            {
                g.SetActive(false);
                Debug.Log($"[Lift0163] Switched off {g.name} (stood in the lift shaft).");
            }
        }

        // The wider of a thing's two flat extents (m), from its renderers.
        private static float Footprint(Transform t)
        {
            Renderer[] rs = t.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) return 0f;
            Bounds b = rs[0].bounds;
            foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return Mathf.Max(b.size.x, b.size.z);
        }

        // A first pass of this step switched off a whole dressing group; bring it back.
        private static void Restore()
        {
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == "E_SolutionsClearanceStaging" && !t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }

        private static void SwitchOffBalustrade()
        {
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == Balustrade)
                {
                    t.gameObject.SetActive(false);
                    Debug.Log($"[Lift0163] Switched off balustrade {t.name} for the upper landing gate.");
                }
        }
    }
}

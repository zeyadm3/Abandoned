using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The island's finishing passes, run on the whole built scene before the NavMesh bake:
    /// - every flat decal (stains, puddles, moss, paper, ash, painted lanes) is lifted to sit clearly above whatever
    ///   surface is under it, each a hair higher than the last, so nothing shares a plane and flickers;
    /// - soft or out-of-reach things are marked walk-through on purpose (leaves, scrub, the far shore, the boat's
    ///   swaying hull art, the turning lamp), and the invisible blockers that stand in for visible ones (window bars,
    ///   rail guards) are marked as intentional;
    /// - the project's solid-objects pass then gives everything else with volume real collision, except where a box
    ///   would block a doorway or the boat's hold (<see cref="RouteConflict"/>);
    /// - outdoor loot and threat points are set onto the ground, which now rolls between the buildings.
    /// </summary>
    public static class IslandSurfaceFixes
    {
        private const float DecalGap = .02f, DecalStep = .004f;

        public static int LiftDecals(Transform root)
        {
            Physics.SyncTransforms();
            int mask = ~LayerMask.GetMask(GameLayers.Player, GameLayers.Loot, GameLayers.Debris, "Ignore Raycast");
            int lifted = 0, n = 0;
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.GetComponent<Collider>() != null || r.GetComponentInParent<Abandoned.Structure.StructuralSection>() != null) continue;
                Bounds b = r.bounds;
                if (b.size.y > .05f || b.size.x < .1f || b.size.z < .1f) continue;
                if (r.name == "Water" || r.name == "Foam") continue;
                // Find the surface under its middle (ignoring itself: it has no collider).
                Vector3 from = new(b.center.x, b.max.y + .4f, b.center.z);
                if (!Physics.Raycast(from, Vector3.down, out RaycastHit hit, 1.2f, mask, QueryTriggerInteraction.Ignore)) continue;
                float want = hit.point.y + DecalGap + (n++ % 6) * DecalStep;
                float gap = b.min.y - hit.point.y;
                if (gap > .06f || gap < -.06f) continue;
                r.transform.position += Vector3.up * (want - b.min.y);
                lifted++;
            }
            return lifted;
        }

        public static void MarkIntentional(Transform root)
        {
            void Walk(string path, string why)
            {
                Transform t = root.Find(path);
                if (t != null && t.GetComponent<WalkThroughObject>() == null) t.gameObject.AddComponent<WalkThroughObject>().EditorSetup(why);
            }
            Walk(IslandVegetation.GroupName + "/Shrubs", "Scrub and ice plant give way underfoot.");
            Walk(IslandVegetation.GroupName + "/Trees", "Tree crowns are overhead; each trunk has its own collider.");
            Walk(IslandBackdrop.GroupName, "The far shore, the city, the bridge: hundreds of metres out in the bay.");
            Walk(IslandBoat.GroupName + "/Truck/HullArt", "The hull sways; the hold's own colliders are the boat.");
            Walk("Lighthouse/LampRoom/LampHead", "The turning lens sits on its solid pedestal.");
            Walk(IslandLighting.GroupName, "Lamps hang overhead; posts carry their own colliders.");
            Walk("Sea", "The water surface and the far islets.");
            Walk("Exterior/Sheds/Shed_North/Door", "The shed door hangs open.");
            Walk("Exterior/Sheds/Shed_East/Door", "The shed door hangs open.");
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
            {
                string n = c.gameObject.name;
                if (n is "BarsSolid" or "RailSolid" or "ParapetSolid" or "GapGuard_L" or "GapGuard_R" || n.StartsWith("Ramp"))
                    if (c.GetComponent<IntentionalInvisibleCollider>() == null)
                        c.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("Stands in for thin bars, rails or the gap beside the gangway.");
            }
        }

        public static int MakeSolid(Transform root) => SolidObjectsPass.Apply(root, L.SceneName, RouteConflict);

        // Doorways (both sides of every opening players walk through) and the boat's hold and gangway.
        private static readonly List<Rect> Keep = BuildKeep();

        private static List<Rect> BuildKeep()
        {
            var k = new List<Rect>();
            void Door(float x, float z, bool wallAlongX, float width)
            {
                float half = width * .5f + .2f;
                k.Add(wallAlongX ? Rect.MinMaxRect(x - half, z - 1.4f, x + half, z + 1.4f) : Rect.MinMaxRect(x - 1.4f, z - half, x + 1.4f, z + half));
            }
            Rect c = L.Cellhouse;
            Door(L.MainDoorX, c.yMin, true, L.MainDoorWidth);
            Door(c.xMin, L.YardDoorZ, false, L.SideDoorWidth);
            Door(L.ServiceDoorX, c.yMax, true, L.SideDoorWidth);
            Door(c.xMax, L.EastDoorZ, false, L.SideDoorWidth);
            Door(36f, L.NorthBand.yMin, true, 2.6f);
            Door(72f, L.NorthBand.yMin, true, 2.6f);
            k.Add(Rect.MinMaxRect(48.6f, L.NorthBand.yMin - 1.5f, 59.4f, L.NorthBand.yMin + 1.5f));
            Door(L.KitchenWest, 83f, false, 2.4f);
            Door(L.Admin.xMax, 42.3f, false, 1.4f);
            Door(36f, L.Admin.yMax, true, 1.6f);
            Door(L.Library.xMin, 42.3f, false, 1.4f);
            Door(72f, L.Library.yMax, true, 1.6f);
            foreach (float x in L.BasementPartitions) Door(x, L.BasementDoorZ, false, 2.4f);
            Door(33.2f, 48.6f, true, 1.3f);
            Door(74.9f, 83.7f, true, 1.3f);
            Door(116f, L.Barracks.yMin, true, 2.4f);
            Door(126f, L.Barracks.yMin, true, 2.4f);
            Door(112f, 6f, false, 1.4f);
            Door(120f, 6f, false, 1.6f);
            Door(116f, 6f, false, 1.2f);
            Door(40f, L.WardenHouse.yMin, true, 2.6f);
            Door(38f, 104f, false, 2.4f);
            Door(44f, 104f, false, 1.6f);
            Door(40f, 106f, false, 1.6f);
            Door(112.7f, 29.9f, false, 1.2f);
            Door(59.5f, L.Lighthouse.yMin, true, 1.8f);
            Door(L.Lighthouse.xMax + 3f, L.Lighthouse.yMin, true, 2.8f);
            Door(L.OfficersClub.center.x, L.OfficersClub.yMin, true, 2.4f);
            Door(L.OfficersClub.xMax - 3f, L.OfficersClub.yMax, true, 2f);
            Door(L.OfficersClub.xMax, L.OfficersClub.center.y, false, 2.4f);
            Rect p = IslandExterior.Powerhouse;
            Door(p.center.x + 2f, p.yMin, true, 3f);
            Door(p.xMax - 3f, p.yMax, true, 2.4f);
            Door(p.xMax, p.center.y, false, 2.8f);
            Door(L.RecYard.xMax - 4f, L.RecYard.yMin, true, 3f);
            Door(84f, 102.6f, true, 2f);
            Door(111.2f, 35.5f, false, 2f);
            // The boat's hold, its gangway, and the quay in front of it.
            k.Add(Rect.MinMaxRect(L.BoatStern.x - 2f, L.BoatStern.z - 6f, L.BoatStern.x + 2f, 3f));
            return k;
        }

        private static string RouteConflict(Bounds box)
        {
            var flat = new Rect(box.min.x, box.min.z, box.size.x, box.size.z);
            foreach (Rect r in Keep)
                if (flat.Overlaps(r)) return "it would stand in a doorway or the boat's hold";
            return null;
        }

        /// <summary>Outdoor points at plateau level go onto the ground (it is 12 m on every pad, so indoor ones stay).</summary>
        public static void SnapToGround(List<LootPointSpec> loot, List<Vector3> threats)
        {
            for (int i = 0; i < loot.Count; i++)
            {
                LootPointSpec p = loot[i];
                if (Mathf.Abs(p.At.y - (L.PlateauY + .05f)) > .2f || IslandLandform.Open(p.At.x, p.At.z)) continue;
                var at = new Vector3(p.At.x, IslandLandform.Height(p.At.x, p.At.z) + .05f, p.At.z);
                loot[i] = new LootPointSpec(p.Name, p.Tag, at, p.Min, p.Max, p.IsJackpot);
            }
            for (int i = 0; i < threats.Count; i++)
            {
                Vector3 t = threats[i];
                if (Mathf.Abs(t.y - L.PlateauY) > .2f || IslandLandform.Open(t.x, t.z)) continue;
                threats[i] = new Vector3(t.x, IslandLandform.Height(t.x, t.z), t.z);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Abandoned.Company;
using Abandoned.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools.Hiring
{
    /// <summary>
    /// 2026 remake: the HQ stations that took over the pause menu's jobs, added to the saved HQ in place (the user's
    /// own HQ edits stay): the crew board in the dispatch office (invites, the crew, house rules), the trophy cabinet
    /// in the crew room (achievements), the notice board beside it (how to play), and the truck garage's paint-shop
    /// desk by the garage door. Each goes to the first of its candidate spots that is clear (checked against the HQ's
    /// colliders); running it again replaces them.
    /// </summary>
    public static class HqStations
    {
        private const string RootName = "_RemakeStations";

        private readonly struct Spot
        {
            public readonly Vector3 At;
            public readonly float Yaw;
            public Spot(float x, float y, float z, float yaw) { At = new Vector3(x, y, z); Yaw = yaw; }
        }

        [MenuItem(ToolMenu.Hq + "Add Remake Stations to HQ", priority = ToolMenu.HqP + 81)]
        public static void AddToSavedHq()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Add the stations in Edit mode.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.OpenScene(HqBuilder.ScenePath, OpenSceneMode.Single);
            Place(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Unity could not save the HQ.");
        }

        public static void Place(Scene scene)
        {
            foreach (GameObject old in scene.GetRootGameObjects())
                if (old.name == RootName) Object.DestroyImmediate(old);
            Physics.SyncTransforms();
            var root = new GameObject(RootName).transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            var placed = new List<string>();

            // Dispatch office, on the back wall (z 10) facing into the office; the service opening is at x 24.6-26.9.
            Station("CrewBoard", root, new Vector3(1.3f, 1f, .06f), new[] { new Spot(28.4f, 1f, 9.86f, 180f), new Spot(33.6f, 1f, 9.86f, 180f) },
                placed, CrewBoardArt).AddComponent<CrewBoard>();
            // Crew room, against the workshop partition (z 18) facing in.
            Station("TrophyCabinet", root, new Vector3(1.2f, 1.9f, .45f), new[] { new Spot(33.3f, 0f, 17.72f, 180f), new Spot(30.2f, 0f, 17.72f, 180f) },
                placed, TrophyArt).AddComponent<TrophyCabinet>();
            // Crew room, on the dispatch office's back wall (z 10) facing into the crew room.
            Station("NoticeBoard", root, new Vector3(1.1f, .8f, .05f), new[] { new Spot(32.6f, 1.15f, 10.16f, 0f), new Spot(30.0f, 1.15f, 10.16f, 0f) },
                placed, NoticeArt).AddComponent<NoticeBoard>();
            // The garage's paint-shop desk, just inside the garage door (x 6, z 0).
            Station("TruckGarageDesk", root, new Vector3(1.1f, 1.05f, .6f), new[] { new Spot(10.2f, 0f, 2.2f, 200f), new Spot(2.2f, 0f, 2.2f, 160f), new Spot(10.2f, 0f, 5.5f, 270f) },
                placed, GarageDeskArt).AddComponent<TruckGarageTerminal>();
            Debug.Log("[Remake] HQ stations: " + string.Join("; ", placed));
        }

        private static GameObject Station(string name, Transform root, Vector3 size, Spot[] spots, List<string> placed, Action<Transform, Vector3> art)
        {
            Spot chosen = spots[0];
            bool clear = false;
            foreach (Spot s in spots)
            {
                if (!Clear(s, size)) continue;
                chosen = s;
                clear = true;
                break;
            }
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.SetPositionAndRotation(chosen.At, Quaternion.Euler(0f, chosen.Yaw, 0f));
            art(go.transform, size);
            var hit = go.AddComponent<BoxCollider>();
            hit.center = new Vector3(0f, size.y / 2f, 0f);
            hit.size = size;
            placed.Add($"{name} at {chosen.At}{(clear ? "" : " (no clear spot; first one used)")}");
            return go;
        }

        // Clear of everything but the floor it stands on and the wall it hangs on (the box is pulled 6 cm off the wall).
        private static bool Clear(Spot s, Vector3 size)
        {
            Quaternion turn = Quaternion.Euler(0f, s.Yaw, 0f);
            Vector3 centre = s.At + Vector3.up * (size.y / 2f + .04f) + turn * Vector3.forward * .08f;
            Vector3 half = new Vector3(size.x / 2f, size.y / 2f - .03f, Mathf.Max(.02f, size.z / 2f - .05f));
            foreach (Collider c in Physics.OverlapBox(centre, half, turn, ~0, QueryTriggerInteraction.Ignore))
                if (c.bounds.size.y > .12f || c.bounds.max.y > s.At.y + .1f) return false;
            return true;
        }

        private static void CrewBoardArt(Transform t, Vector3 size)
        {
            Material cork = PolishAssets.Material("HQ_CrewBoardCork", new Color(.52f, .38f, .24f), .95f);
            Material frame = PolishAssets.Material("HQ_CrewBoardFrame", new Color(.18f, .18f, .19f), .5f);
            Material paper = PolishAssets.Material("HQ_ClipboardPaper", new Color(.86f, .84f, .76f), .9f);
            Material pin = PolishAssets.Material("HQ_CrewBoardPin", new Color(.7f, .1f, .08f), .4f);
            Box("Cork", t, new Vector3(0f, size.y / 2f, 0f), new Vector3(size.x, size.y, .03f), cork, false);
            Box("FrameTop", t, new Vector3(0f, size.y, 0f), new Vector3(size.x + .06f, .04f, .05f), frame, false);
            Box("FrameBottom", t, new Vector3(0f, 0f, 0f), new Vector3(size.x + .06f, .04f, .05f), frame, false);
            Box("FrameLeft", t, new Vector3(-size.x / 2f, size.y / 2f, 0f), new Vector3(.04f, size.y, .05f), frame, false);
            Box("FrameRight", t, new Vector3(size.x / 2f, size.y / 2f, 0f), new Vector3(.04f, size.y, .05f), frame, false);
            // Four crew cards, a rota and a telephone on a shelf: who is on, and the line out (Steam invites).
            for (int i = 0; i < 4; i++)
            {
                float x = -.45f + i * .3f;
                Box("CrewCard", t, new Vector3(x, .55f, .02f), new Vector3(.2f, .27f, .004f), paper, false).transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 2f : -3f));
                Box("Pin", t, new Vector3(x, .67f, .025f), new Vector3(.015f, .015f, .012f), pin, false);
            }
            Box("Rota", t, new Vector3(.35f, .18f, .02f), new Vector3(.42f, .2f, .004f), paper, false);
            Box("Shelf", t, new Vector3(-.35f, .02f, .12f), new Vector3(.4f, .025f, .2f), frame, false);
            Box("Telephone", t, new Vector3(-.35f, .08f, .13f), new Vector3(.2f, .09f, .16f), frame, false);
        }

        private static void TrophyArt(Transform t, Vector3 size)
        {
            Material wood = PolishAssets.Material("HQ_TrophyWood", new Color(.28f, .17f, .1f), .55f);
            // The remake's shared glass (transparent); a plain material would hide the trophies.
            Material glass = AssetDatabase.LoadAssetAtPath<Material>(Remake.RemakePaths.Shared + "/Glass.mat")
                             ?? PolishAssets.Material("HQ_TrophyGlass", new Color(.65f, .75f, .78f), .1f);
            Material gold = PolishAssets.Material("HQ_TrophyGold", new Color(.8f, .6f, .22f), .25f);
            Box("Base", t, new Vector3(0f, .4f, 0f), new Vector3(size.x, .8f, size.z), wood, false);
            Box("Top", t, new Vector3(0f, size.y - .03f, 0f), new Vector3(size.x, .06f, size.z), wood, false);
            Box("Back", t, new Vector3(0f, 1.35f, -size.z / 2f + .02f), new Vector3(size.x, 1.1f, .03f), wood, false);
            foreach (float x in new[] { -size.x / 2f + .02f, size.x / 2f - .02f })
                Box("Side", t, new Vector3(x, 1.35f, 0f), new Vector3(.03f, 1.1f, size.z), wood, false);
            Box("Shelf", t, new Vector3(0f, 1.35f, 0f), new Vector3(size.x - .05f, .02f, size.z - .04f), wood, false);
            Box("Glass", t, new Vector3(0f, 1.35f, size.z / 2f - .02f), new Vector3(size.x - .05f, 1.08f, .01f), glass, false);
            for (int i = 0; i < 3; i++)
            {
                float x = -.38f + i * .38f;
                Primitive(PrimitiveType.Cylinder, "Cup", t, new Vector3(x, 1.0f, 0f), new Vector3(.12f, .08f, .12f), gold, false);
                Primitive(PrimitiveType.Cylinder, "Stem", t, new Vector3(x, .88f, 0f), new Vector3(.04f, .06f, .04f), gold, false);
                Box("Plinth", t, new Vector3(x, .82f, 0f), new Vector3(.14f, .04f, .14f), wood, false);
            }
        }

        private static void NoticeArt(Transform t, Vector3 size)
        {
            Material cork = PolishAssets.Material("HQ_CrewBoardCork", new Color(.52f, .38f, .24f), .95f);
            Material paper = PolishAssets.Material("HQ_ClipboardPaper", new Color(.86f, .84f, .76f), .9f);
            Material ink = PolishAssets.Material("HQ_ClipboardInk", new Color(.12f, .14f, .2f), .9f);
            Box("Cork", t, new Vector3(0f, size.y / 2f, 0f), new Vector3(size.x, size.y, .03f), cork, false);
            // How the job works, drawn not written: a floor plan, a truck, a stick figure carrying a crate.
            Box("SheetPlan", t, new Vector3(-.3f, .45f, .02f), new Vector3(.3f, .4f, .004f), paper, false);
            Box("PlanLine", t, new Vector3(-.3f, .5f, .024f), new Vector3(.2f, .006f, .002f), ink, false);
            Box("PlanLine", t, new Vector3(-.3f, .38f, .024f), new Vector3(.2f, .006f, .002f), ink, false);
            Box("SheetTruck", t, new Vector3(.12f, .5f, .02f), new Vector3(.28f, .2f, .004f), paper, false);
            Box("TruckSketch", t, new Vector3(.12f, .5f, .024f), new Vector3(.16f, .07f, .002f), ink, false);
            Box("SheetRules", t, new Vector3(.38f, .3f, .02f), new Vector3(.22f, .3f, .004f), paper, false);
        }

        private static void GarageDeskArt(Transform t, Vector3 size)
        {
            Material steel = PolishAssets.Material("HQ_GarageDeskSteel", new Color(.3f, .32f, .3f), .5f);
            Material top = PolishAssets.Material("HQ_GarageDeskTop", new Color(.4f, .3f, .2f), .7f);
            Material swatch = PolishAssets.Material("HQ_GarageSwatches", new Color(.65f, .12f, .08f), .5f);
            Material screen = PolishAssets.Material("HR_Screen", new Color(.05f, .09f, .1f), .2f, null, .4f);
            Box("Cabinet", t, new Vector3(0f, .48f, 0f), new Vector3(size.x, .96f, size.z), steel, false);
            Box("Top", t, new Vector3(0f, .985f, 0f), new Vector3(size.x + .04f, .05f, size.z + .04f), top, false);
            Box("PaintChart", t, new Vector3(-.25f, 1.02f, .05f), new Vector3(.3f, .01f, .22f), swatch, false);
            Box("Monitor", t, new Vector3(.25f, 1.2f, -.15f), new Vector3(.4f, .28f, .04f), steel, false);
            Box("Screen", t, new Vector3(.25f, 1.2f, -.128f), new Vector3(.36f, .24f, .005f), screen, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// One button for the open-world places built on 2026-10-23 (user: "just one button to rebuild and do everything"):
    /// each new map's own rebuild, the look/surfaces/lighting/site pass on those maps only, the open world (it reads
    /// every map's finished sky and fog), then the checks. The six older maps are never rebuilt here, so hand edits in
    /// their saved scenes survive; Rebuild Entire Game is the button for everything.
    /// </summary>
    public static class RebuildNewMaps
    {
        private const string Title = "Rebuild New Maps and World";

        [MenuItem(ToolMenu.Root + Title, priority = ToolMenu.RebuildNewMapsP)]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(Title, "Leave Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog(Title,
                    "Rebuilds Saint Brigid Hospital, Last Light Motor Lodge, the Skyscraper Restaurant, the Police Station and " +
                    "the Fire Station from code, gives them their look, surfaces, lighting, sky, trees, grass and sound, " +
                    "rebuilds the open world, then checks everything (level audit, content validation, missing text).\n\n" +
                    "The Mall, School, Factory, Prison, HQ and Tutorial are not touched.\n\n" +
                    "This takes a while and Unity will look frozen between steps.", "Rebuild", "Cancel"))
                return;

            var steps = new List<(string name, Action run)>
            {
                ("Saint Brigid Hospital", Abandoned.Hospital.HospitalBuilder.Build),
                ("Last Light Motor Lodge", Abandoned.Motel.MotelBuilder.Build),
                ("Skyscraper Restaurant", SkyscraperBuilder.Build),
                ("Millbrook 3rd Precinct", PoliceStationBuilder.Build),
                ("Millbrook Fire Station No. 4", FireStationBuilder.Build),
                ("Look, surfaces, lighting, sky and sound (new maps)", AvOverhaul.AfterNewMapsOrThrow),
                RebuildAllMaps.OpenWorldStep,
                ("Checks: level audit of the new maps", AuditNewMaps),
                ("Checks: content validation", Validate),
                ("Checks: missing text", MissingText),
            };
            RebuildAllMaps.RunSteps(Title, steps);
        }

        // The same three audits as Checks > Audit All Levels, on the new maps (that menu lists only the older ones).
        private static void AuditNewMaps()
        {
            int problems = 0;
            foreach (string path in AvOverhaul.NewMapScenes)
            {
                if (!File.Exists(path)) throw new InvalidOperationException($"{path} was not built.");
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                GameObject root = scene.GetRootGameObjects().OrderByDescending(r => r.GetComponentsInChildren<Renderer>(true).Length).First();
                int walk = SolidObjectsPass.Audit(root.transform, scene.name);
                int invisible = InvisibleColliderAudit.Run(root.transform, scene.name);
                int stairs = MallAudit.Stairs(root.transform, scene.name);
                Debug.Log($"[{Title}] {scene.name}: walk-through {walk}, invisible colliders {invisible}, blocked stairs {stairs}.");
                problems += walk + invisible + stairs;
            }
            if (problems > 0) throw new InvalidOperationException($"{problems} audit problems in the new maps (the lines above list them).");
        }

        private static void Validate()
        {
            List<string> errors = ContentValidator.Run();
            ContentValidator.Report(errors);
            if (errors.Count > 0) throw new InvalidOperationException($"{errors.Count} content problems (see the Console).");
        }

        // Tools/Text/check.py is the one source of truth for the nine-language sheets; run it rather than re-implement it.
        private static void MissingText()
        {
            string repo = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            var start = new ProcessStartInfo("/usr/bin/env", "python3 Tools/Text/check.py --missing")
            {
                WorkingDirectory = repo, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
            };
            string output;
            using (Process run = Process.Start(start))
            {
                output = run.StandardOutput.ReadToEnd() + run.StandardError.ReadToEnd();
                run.WaitForExit();
            }
            Match missing = Regex.Match(output, @"Missing from the sheets: (\d+)");
            if (!missing.Success) throw new InvalidOperationException("Tools/Text/check.py did not run:\n" + output);
            if (missing.Groups[1].Value != "0")
            {
                Debug.LogWarning($"[{Title}] Missing text:\n{output}");
                throw new InvalidOperationException($"{missing.Groups[1].Value} lines are missing from the text sheets (see the Console).");
            }
            Debug.Log($"[{Title}] Text sheets: nothing missing.");
        }
    }
}

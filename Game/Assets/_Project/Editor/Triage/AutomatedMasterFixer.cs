using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Batch triage over every build scene: the uGUI fix (saved only where it changed something) and the read-only audit,
    /// one report for the lot in Game/Logs/EmergencyTriage.txt. It does NOT run the horror atmosphere override: each map
    /// has its own tuned lighting and look (lighting rebuild, remodel v2), and a priority-999 volume would flatten them all.
    /// Batch: Tools/unity.sh exec Abandoned.EditorTools.AutomatedMasterFixer.ExecuteAllAndTest (exits 1 on any P0).
    /// </summary>
    public static class AutomatedMasterFixer
    {
        [MenuItem(ToolMenu.Checks + "Emergency Triage: Audit Every Build Scene", priority = ToolMenu.ChecksP + 41)]
        public static void AuditEveryScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Execute();
        }

        public static void ExecuteAllAndTest()
        {
            int p0 = Execute();
            if (Application.isBatchMode) EditorApplication.Exit(p0 > 0 ? 1 : 0);
        }

        private static int Execute()
        {
            var report = new TriageReport();
            var names = new List<string>();
            foreach (string path in BuildScenes.All)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                report.Section(scene.name);
                if (UIEmergencyFixer.FixLoaded() > 0) EditorSceneManager.SaveScene(scene);
                names.AddRange(EmergencyProjectTriage.AuditLoaded(report));
            }
            report.Finish(string.Join(", ", names));
            return report.Count(TriageReport.Level.P0);
        }
    }
}

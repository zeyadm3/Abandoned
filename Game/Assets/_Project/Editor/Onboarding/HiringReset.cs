using Abandoned.Onboarding;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools.Hiring
{
    /// <summary>
    /// Starts this machine's hiring again (the editor's own PlayerPrefs; a player build keeps its own). The editor
    /// auto-hosts on Play, which skips the title screen and so the hiring: to see it, turn off
    /// NetworkConfig.AutoHostInEditor or use a build.
    /// </summary>
    public static class HiringReset
    {
        [MenuItem(ToolMenu.Hq + "Reset Hiring on This Machine", priority = ToolMenu.HqP + 83)]
        public static void Reset()
        {
            OnboardingProgress.Reset();
            Debug.Log("[Onboarding] This machine is a new hire again: the next title screen starts the HR interview.");
        }
    }
}

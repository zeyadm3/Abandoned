using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Compiles native Apple icon appearances, installs them, and restores the bundle signature.</summary>
    public static class MacApplicationIcon
    {
        public static void Install(string appPath)
        {
            if (Application.platform != RuntimePlatform.OSXEditor)
                throw new InvalidOperationException("Appearance-aware Mac icons require Xcode on the Mac build host.");
            ApplicationBranding.WriteMacIconSource();
            string scratch = Path.Combine(Path.GetTempPath(), "AbandonedIcon-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratch);
            try
            {
                string partialPlist = Path.Combine(scratch, "IconInfo.plist");
                Run("/usr/bin/xcrun", "actool", ApplicationBranding.MacIconSource, "--compile", scratch,
                    "--platform", "macosx", "--minimum-deployment-target", "12.0", "--app-icon",
                    ApplicationBranding.MacIconName, "--output-partial-info-plist", partialPlist);

                string resources = Path.Combine(appPath, "Contents", "Resources");
                File.Copy(Path.Combine(scratch, "Assets.car"), Path.Combine(resources, "Assets.car"), true);
                File.Copy(Path.Combine(scratch, ApplicationBranding.MacIconName + ".icns"),
                    Path.Combine(resources, ApplicationBranding.MacIconName + ".icns"), true);
                string infoPlist = Path.Combine(appPath, "Contents", "Info.plist");
                Run("/usr/bin/plutil", "-replace", "CFBundleIconFile", "-string", ApplicationBranding.MacIconName, infoPlist);
                Run("/usr/bin/plutil", "-replace", "CFBundleIconName", "-string", ApplicationBranding.MacIconName, infoPlist);

                // Third-party plugin bundles (Steam Audio's phonon) arrive with seals Unity's copy no longer
                // matches; re-seal each one ad hoc first, or the outer signature can never verify (0.24.12).
                string plugins = Path.Combine(appPath, "Contents", "PlugIns");
                if (Directory.Exists(plugins))
                    foreach (string bundle in Directory.GetDirectories(plugins, "*.bundle"))
                        Run("/usr/bin/codesign", "--force", "--sign", "-", "--timestamp=none", bundle);

                // Unity signs before we add these resources. Re-seal the outer bundle. Preserve the microphone
                // and any other existing signing entitlements.
                Run("/usr/bin/codesign", "--force", "--sign", "-", "--timestamp=none",
                    "--preserve-metadata=entitlements,requirements,flags,runtime", appPath);
                if (MacSignature.Problem(appPath) is string problem)
                    throw new InvalidOperationException("The branded Mac bundle failed signature verification: " + problem);
                UnityEngine.Debug.Log("[Branding] Installed native light/dark Mac icons and verified the bundle signature.");
            }
            finally
            {
                Directory.Delete(scratch, true);
            }
        }

        private static void Run(string executable, params string[] arguments)
        {
            // ProcessStartInfo has no shell. Quote each argument independently so project paths may contain spaces.
            string quoted = string.Join(" ", arguments.Select(value => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""));
            var start = new ProcessStartInfo(executable, quoted)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using Process process = Process.Start(start);
            if (process == null) throw new InvalidOperationException($"Could not start {executable}.");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(60000))
            {
                process.Kill();
                throw new TimeoutException($"Application icon tool timed out: {executable}.");
            }
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Application icon tool failed ({executable}, {process.ExitCode}):\n{error.Result}\n{output.Result}");
        }
    }
}

using System;
using System.Diagnostics;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Checks a built .app's ad-hoc signature. Anything written into the bundle after Unity signs it
    /// breaks the seal, and macOS then refuses the downloaded zip as "damaged" on the tester's Mac.
    /// </summary>
    public static class MacSignature
    {
        /// <summary>Null when the bundle verifies (or codesign isn't available), else codesign's complaint.</summary>
        public static string Problem(string appPath)
        {
            try
            {
                var start = new ProcessStartInfo("codesign", $"--verify --deep --strict \"{appPath}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using Process codesign = Process.Start(start);
                string error = codesign.StandardError.ReadToEnd().Trim();
                codesign.WaitForExit(60000);
                return codesign.ExitCode == 0 ? null : error;
            }
            catch (Exception)
            {
                return null; // not on a Mac with developer tools: nothing to check with
            }
        }
    }
}

using NUnit.Framework.Interfaces;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>
    /// Writes each test's start and result to the log, so a batch run that hangs or crashes shows
    /// which test it was in (the XML results are only written at the end). Called from
    /// <see cref="SteamTestRunGuard"/>: an assembly gets only one TestRunCallback attribute.
    /// </summary>
    public static class TestProgressLog
    {
        public static void Started(ITest test)
        {
            if (!test.IsSuite) Debug.Log($"[Test] start {test.FullName}");
        }

        public static void Finished(ITestResult result)
        {
            if (!result.Test.IsSuite) Debug.Log($"[Test] {result.ResultState} {result.Test.FullName} ({result.Duration:0.0}s)");
        }
    }
}

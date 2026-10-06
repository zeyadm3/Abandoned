using Abandoned.Networking;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(Abandoned.Tests.SteamTestRunGuard))]

namespace Abandoned.Tests
{
    /// <summary>
    /// Keeps the real Steam off for every test run, including runs from the editor's Test Runner window
    /// where Application.isBatchMode is false. Lives in the PlayMode test assembly because that one is
    /// loaded for both EditMode and PlayMode runs (the framework scans every loaded test assembly).
    /// Also the assembly's one run callback, so it drives <see cref="TestProgressLog"/> too.
    /// </summary>
    public sealed class SteamTestRunGuard : ITestRunCallback
    {
        public void RunStarted(ITest testsToRun) => SteamInitPolicy.TestRunActive = true;

        public void RunFinished(ITestResult testResults) => SteamInitPolicy.TestRunActive = false;

        // A PlayMode run reloads the domain on entering Play mode; re-arm before every test in case
        // RunStarted landed in the old domain.
        public void TestStarted(ITest test)
        {
            SteamInitPolicy.TestRunActive = true;
            TestProgressLog.Started(test);
        }

        public void TestFinished(ITestResult result) => TestProgressLog.Finished(result);
    }
}

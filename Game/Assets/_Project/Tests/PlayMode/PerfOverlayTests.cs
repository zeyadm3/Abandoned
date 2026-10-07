using System.Collections;
using Abandoned.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>M7.6: switching the F1 perf readout on and off in the mall runs cleanly (errors fail the test).</summary>
    public class PerfOverlayTests
    {
        [UnityTest]
        public IEnumerator TheF1PerfLineStartsAndStopsCleanly()
        {
            yield return TestBuildingScene.Load("Mall");
            Assert.IsNotNull(Object.FindAnyObjectByType<PerfOverlay>(), "the mall has the perf readout");
            DebugView.SetVisible(true);
            yield return new WaitForSeconds(1.3f);
            DebugView.SetVisible(false);
            yield return null;
            DebugView.SetVisible(true);
            yield return null;
            DebugView.SetVisible(false);
            yield return null;
        }
    }
}

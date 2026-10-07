using Abandoned.Extraction;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class DangerConfigTests
    {
        [Test]
        public void DangerClimbsSteadilyThenJumpsWhenTheWindowCloses()
        {
            var c = ScriptableObject.CreateInstance<DangerConfig>();
            try
            {
                float i = c.LevelInterval, window = 900f;
                Assert.AreEqual(0, c.LevelAt(0f, window));
                Assert.AreEqual(0, c.LevelAt(i - 1f, window));
                Assert.AreEqual(1, c.LevelAt(i + 1f, window));
                int beforeClose = c.LevelAt(window - 1f, window);
                int atClose = c.LevelAt(window + 0.1f, window);
                Assert.GreaterOrEqual(atClose - beforeClose, c.WindowClosedJump, "a sharp increase after the window (GDD 10)");
                Assert.AreEqual(Mathf.Min(c.MaxLevel, atClose + 1), c.LevelAt(window + i / 2f + 0.1f, window), "twice as fast after");
                Assert.AreEqual(c.MaxLevel, c.LevelAt(100000f, window));
            }
            finally
            {
                Object.DestroyImmediate(c);
            }
        }
    }
}

using Abandoned.Core;
using Abandoned.EditorTools;
using NUnit.Framework;

namespace Abandoned.Tests
{
    public class BuildCommitLabelTests
    {
        [Test]
        public void UncommittedShippedChangesMarkTheBuildDirty()
        {
            Assert.AreEqual("abc1234", GitInfo.Label("abc1234\n", ""));
            Assert.AreEqual("abc1234", GitInfo.Label("abc1234", "  \n"));
            Assert.AreEqual("abc1234-dirty", GitInfo.Label("abc1234", " M Game/Assets/_Project/Scripts/Core/VersionInfo.cs"));
            Assert.AreEqual("abc1234-dirty", GitInfo.Label("abc1234", "?? Game/Assets/_Project/Scripts/New.cs"));
            Assert.AreEqual(GitInfo.Unknown, GitInfo.Label("", ""));
            Assert.AreEqual(GitInfo.Unknown, GitInfo.Label(null, null));
        }

        [Test]
        public void TheRealRepositoryGivesAHeadLabel()
        {
            string label = GitInfo.CommitLabel();
            Assert.AreNotEqual(GitInfo.Unknown, label, "git should be reachable from the Unity project in this repo");
            StringAssert.IsMatch("^[0-9a-f]{7,}(-dirty)?$", label);
        }
    }
}

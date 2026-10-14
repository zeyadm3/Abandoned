using Abandoned.Core;
using NUnit.Framework;

namespace Abandoned.Tests
{
    /// <summary>
    /// 0.24.8: text that leaves a machine as English with its values (Core.Loc.Wire, Word, Value) and comes back
    /// worded. The lines here are in no sheet, so they read the same in any language but the stretched dev text.
    /// </summary>
    public class LocWireTests
    {
        [SetUp]
        public void NotStretched() => Assume.That(Languages.Current.Stretched, Is.False, "the stretched dev language pads every line");

        [Test]
        public void ValuesTravelWithTheirLine()
        {
            string wire = Loc.Wire("Test wire {0} and {1}", 3, "x");
            Assert.AreNotEqual("Test wire 3 and x", wire, "the values are not filled in before they arrive");
            Assert.AreEqual("Test wire 3 and x", Loc.FromWire(wire));
        }

        [Test]
        public void PlainTextPassesThrough()
        {
            Assert.AreEqual("Test plain line", Loc.FromWire(Loc.Mark("Test plain line")));
            Assert.AreEqual("", Loc.FromWire(""));
            Assert.IsNull(Loc.FromWire(null));
        }

        [Test]
        public void AWordIsWordedWithItsOwnValues()
        {
            string name = Loc.Word("Test player {0}", 2);
            Assert.AreEqual("Test player 2", Loc.Value(name));
            Assert.AreEqual("Test Test player 2 wins", Loc.FromWire(Loc.Wire("Test {0} wins", name)));
            Assert.AreEqual("Test word", Loc.FromWire(Loc.Word("Test word")));
        }

        [Test]
        public void AChosenNameIsNeverTranslated()
        {
            // "Back" is a sheet line, so in German it would read "Zurück" if a plain value were translated.
            Assert.AreEqual("Back", Loc.Value("Back"), "a player called Back keeps their name");
            Assert.AreEqual("Test Back scores 5.", Loc.FromWire(Loc.Wire("Test {0} scores {1}.", "Back", 5)));
        }

        [Test]
        public void ABrokenPlaceholderNeverThrows()
        {
            string line = null;
            Assert.DoesNotThrow(() => line = Loc.T("Test broken {0} {1}", 1));
            Assert.AreEqual("Test broken {0} {1}", line);
        }
    }
}

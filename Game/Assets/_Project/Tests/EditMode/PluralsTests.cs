using Abandoned.Core;
using NUnit.Framework;

namespace Abandoned.Tests
{
    /// <summary>0.24.8: which word form a count picks (Core.Plurals), at each rule's edges.</summary>
    public class PluralsTests
    {
        [TestCase(0L, 1), TestCase(1L, 0), TestCase(2L, 1), TestCase(21L, 1)]
        public void OneOther(long count, int form) => Assert.AreEqual(form, Plurals.Form(PluralRule.OneOther, count));

        [TestCase(0L, 0), TestCase(1L, 0), TestCase(2L, 1), TestCase(100L, 1)]
        public void ZeroOneOther(long count, int form) => Assert.AreEqual(form, Plurals.Form(PluralRule.ZeroOneOther, count));

        [TestCase(1L, 0), TestCase(21L, 0), TestCase(101L, 0), TestCase(11L, 2), TestCase(111L, 2),
         TestCase(2L, 1), TestCase(4L, 1), TestCase(22L, 1), TestCase(12L, 2), TestCase(14L, 2),
         TestCase(5L, 2), TestCase(0L, 2), TestCase(-3L, 1)]
        public void Russian(long count, int form) => Assert.AreEqual(form, Plurals.Form(PluralRule.Russian, count));

        [TestCase(1L, 0), TestCase(2L, 1), TestCase(4L, 1), TestCase(22L, 1), TestCase(102L, 1),
         TestCase(12L, 2), TestCase(112L, 2), TestCase(5L, 2), TestCase(21L, 2), TestCase(0L, 2)]
        public void Polish(long count, int form) => Assert.AreEqual(form, Plurals.Form(PluralRule.Polish, count));

        [TestCase(0L), TestCase(1L), TestCase(7L)]
        public void NoneHasOneForm(long count) => Assert.AreEqual(0, Plurals.Form(PluralRule.None, count));

        [Test]
        public void PickTakesTheFormsInOrder()
        {
            const string forms = "one | few | many";
            Assert.AreEqual("one", Plurals.Pick(forms, PluralRule.Polish, 1));
            Assert.AreEqual("few", Plurals.Pick(forms, PluralRule.Polish, 3));
            Assert.AreEqual("many", Plurals.Pick(forms, PluralRule.Polish, 5));
        }

        [Test]
        public void ALineWithFewerFormsGivesItsLast()
        {
            Assert.AreEqual("b", Plurals.Pick("a | b", PluralRule.Russian, 5));
            Assert.AreEqual("only", Plurals.Pick("only", PluralRule.Polish, 5));
        }
    }
}

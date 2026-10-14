using System.Collections.Generic;
using Abandoned.Core;
using NUnit.Framework;

namespace Abandoned.Tests
{
    /// <summary>0.24.8: how a text sheet is read (Core.TextTable.Read), independent of the sheets that ship.</summary>
    public class TextTableTests
    {
        private static Dictionary<string, string> Read(string sheet, string language)
        {
            var lines = new Dictionary<string, string>();
            TextTable.Read(sheet, language, lines);
            return lines;
        }

        [Test]
        public void ALineIsKeyedByItsEnglish()
        {
            Dictionary<string, string> de = Read("en: Back\nde: Zurück\nja: 戻る", "de");
            Assert.AreEqual("Zurück", de["Back"]);
            Assert.AreEqual(1, de.Count);
        }

        [Test]
        public void AMissingTranslationStoresNothing()
        {
            // The caller keeps the English it asked with.
            Assert.IsEmpty(Read("en: Back\nde: Zurück", "fr"));
            Assert.IsEmpty(Read("en: Back\nde:", "de"), "an empty translation is no translation");
            Assert.IsEmpty(Read("en: Back\nde: Zurück", "en"), "English never reads its own lines back");
        }

        [Test]
        public void AKeyedBlockFallsBackToTheSheetsEnglish()
        {
            const string sheet = "key: hint.Test\nen: Pick it up\nde: Heb es auf";
            Assert.AreEqual("Heb es auf", Read(sheet, "de")["hint.Test"]);
            Assert.AreEqual("Pick it up", Read(sheet, "fr")["hint.Test"]);
            Assert.AreEqual("Pick it up", Read(sheet, "en")["hint.Test"]);
        }

        [Test]
        public void BlocksSplitOnAnyBlankLine()
        {
            Dictionary<string, string> de = Read("en: A\nde: X\n   \nen: B\nde: Y\n\nen: C\nde: Z", "de");
            Assert.AreEqual("X", de["A"]);
            Assert.AreEqual("Y", de["B"]);
            Assert.AreEqual("Z", de["C"]);
        }

        [Test]
        public void WindowsLineEndingsAndCommentsAreIgnored()
        {
            Dictionary<string, string> de = Read("# a note\r\nen: A\r\nde: X\r\n\r\nen: B\r\n# between\r\nde: Y\r\n", "de");
            Assert.AreEqual("X", de["A"]);
            Assert.AreEqual("Y", de["B"]);
        }

        [Test]
        public void EscapesAreReadInOnePass()
        {
            Dictionary<string, string> de = Read("en: Line one\\nline two\nde: Zeile eins\\nZeile zwei\n\nen: C:\\\\new\nde: C:\\\\neu", "de");
            Assert.AreEqual("Zeile eins\nZeile zwei", de["Line one\nline two"]);
            // An escaped backslash before an n stays a backslash and an n, not a line break.
            Assert.AreEqual("C:\\neu", de["C:\\new"]);
        }

        [Test]
        public void TheFirstColonSplitsTheTag()
        {
            Assert.AreEqual("Achtung: Boden", Read("en: Warning: floor\nde: Achtung: Boden", "de")["Warning: floor"]);
        }
    }
}

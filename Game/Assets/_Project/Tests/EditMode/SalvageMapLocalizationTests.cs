using System.Collections.Generic;
using Abandoned.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>Every new contract location remains readable on all shipped language sheets.</summary>
    public class SalvageMapLocalizationTests
    {
        private const string CompanyTextPath = "Assets/_Project/Data/Text/Resources/Text/company.txt";

        private static readonly string[] NewLocationText =
        {
            "Abandoned Mall (today's daily job)",
            "Abandoned School",
            "Classrooms, a gym and a basement built around failing bleachers.",
            "Abandoned School (today's daily job)",
            "Abandoned Factory",
            "Machine halls, loading docks and high catwalks over a live production floor.",
            "Abandoned Factory (today's daily job)",
            "Gallows Rock Penitentiary",
            "An island prison in a foggy bay: three-tier cell blocks, a cliff road down to the boat and a dead lighthouse. The sea keeps whatever falls.",
            "Gallows Rock Penitentiary (today's daily job)",
        };

        [Test]
        public void NewLocationNamesAndBriefsHaveEveryShippedTranslation()
        {
            TextAsset sheet = AssetDatabase.LoadAssetAtPath<TextAsset>(CompanyTextPath);
            Assert.IsNotNull(sheet, $"missing localization sheet {CompanyTextPath}");

            foreach (LanguageInfo language in Languages.All)
            {
                if (language.IsEnglish) continue;
                var translations = new Dictionary<string, string>();
                TextTable.Read(sheet.text, language.Code, translations);
                foreach (string english in NewLocationText)
                {
                    Assert.IsTrue(translations.TryGetValue(english, out string localized) && !string.IsNullOrWhiteSpace(localized),
                        $"{language.Code}: missing translation for '{english}'");
                }
            }
        }
    }
}

#!/usr/bin/env python3
"""Writes Tools/Steam/achievements_languages.csv: every achievement's name and description in the game's nine
languages, read from the text sheets, for Steam's achievement localization (0.24.8). Reads files only.

    python3 Tools/Text/steam_achievements.py

Run it again after changing an achievement's wording in data_achievements.txt. The "steam" column is Steam's
own name for the language (Spanish is listed twice: Spain and Latin America share one translation).
"""
import csv, glob, os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHEETS = os.path.join(ROOT, "Game", "Assets", "_Project", "Data", "Text", "Resources", "Text")
STEAM = os.path.join(ROOT, "Tools", "Steam")
LANGUAGES = [("zh-CN", "schinese"), ("ja", "japanese"), ("ko", "koreana"), ("es", "spanish"), ("es", "latam"),
             ("pt-BR", "brazilian"), ("de", "german"), ("fr", "french"), ("ru", "russian"), ("pl", "polish")]


def sheets():
    lines = {}
    for path in sorted(glob.glob(os.path.join(SHEETS, "*.txt"))):
        for block in open(path, encoding="utf-8").read().split("\n\n"):
            entry = {}
            for line in block.split("\n"):
                if line.startswith("#") or ":" not in line: continue
                tag, text = line.split(":", 1)
                entry[tag.strip()] = text.strip()
            if "en" in entry and "key" not in entry: lines[entry["en"]] = entry
    return lines


def main():
    lines, missing = sheets(), 0
    with open(os.path.join(STEAM, "achievements.csv"), encoding="utf-8", newline="") as source:
        achievements = list(csv.DictReader(source))
    with open(os.path.join(STEAM, "achievements_languages.csv"), "w", encoding="utf-8", newline="") as out:
        writer = csv.writer(out)
        writer.writerow(["api_name", "steam", "display_name", "description"])
        for a in achievements:
            for tag, steam in LANGUAGES:
                name = lines.get(a["display_name"], {}).get(tag)
                description = lines.get(a["description"], {}).get(tag)
                if name is None or description is None: missing += 1
                writer.writerow([a["api_name"], steam, name or a["display_name"], description or a["description"]])
    print(f"{len(achievements)} achievements x {len(LANGUAGES)} Steam languages -> Tools/Steam/achievements_languages.csv"
          + (f"   ({missing} rows fell back to English: no sheet line)" if missing else ""))


main()

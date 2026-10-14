#!/usr/bin/env python3
"""Checks the game's text sheets against the code and the content assets (0.24.8). Reads files only.

    python3 Tools/Text/check.py              summary and every problem
    python3 Tools/Text/check.py --missing    also: English lines with no sheet block, ready to paste and translate
    python3 Tools/Text/check.py --unused     also: sheet blocks nothing asks for

A sheet (Game/Assets/_Project/Data/Text/Resources/Text/*.txt) is a list of blocks split by a blank line, one
"tag: text" line per language; the "en:" line is what code asks for (Loc.T), or the block starts with "key: id"
(Loc.K). Format and wording rules: Docs/LOCALIZATION_0.24.8.md.
"""
import glob, os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PROJECT = os.path.join(ROOT, "Game", "Assets", "_Project")
SHEETS = os.path.join(PROJECT, "Data", "Text", "Resources", "Text")
LANGUAGES = ["zh-CN", "ja", "ko", "es", "pt-BR", "de", "fr", "ru", "pl"]
PLURAL_FORMS = {"en": 2, "zh-CN": 1, "ja": 1, "ko": 1, "es": 2, "pt-BR": 2, "de": 2, "fr": 2, "ru": 3, "pl": 3}
# Text fields of content assets (ScriptableObjects under Data/) that screens show through Loc.
ASSET_FIELDS = ["DisplayName", "Description", "CrushedLine"]
# ...and lists of lines (the boss's voicemails and board quips in CompanyMessages).
ASSET_LIST_FIELDS = ["Welcome", "QuotaMet", "BigHaul", "QuotaMissed", "LastWarning", "Bankrupt", "LevelUp", "BoardQuips"]
SKIP_DIRS = ("/NetTest/",)
PLACEHOLDER = re.compile(r"\{(?:\d+|key:\w+)(?:[,:][^{}]*)?\}")
CJK = re.compile(r"[぀-ヿ㐀-鿿가-힯]")
CYRILLIC = re.compile(r"[Ѐ-ӿ]")


class Block:
    def __init__(self, sheet, line):
        self.sheet, self.line, self.key, self.lines = sheet, line, None, {}

    @property
    def id(self): return self.key if self.key is not None else self.lines.get("en")


# One pass, as TextTable.Unescape: an escaped backslash before an n ("\\n" in the sheet) stays a backslash and an n.
def unescape_sheet(text): return re.sub(r"\\([n\\])", lambda m: "\n" if m.group(1) == "n" else "\\", text)


def read_sheets(problems):
    blocks = []
    for path in sorted(glob.glob(os.path.join(SHEETS, "*.txt"))):
        name, block = os.path.basename(path), None
        for number, raw in enumerate(open(path, encoding="utf-8").read().split("\n"), 1):
            line = raw.strip()
            if raw != raw.rstrip(" \t\r") and line: problems.append(f"{name}:{number}: the line ends with a space")
            if not line:
                block = None
                continue
            if line.startswith("#"): continue
            colon = line.find(":")
            tag = line[:colon] if colon > 0 else ""
            if tag != "key" and tag != "en" and tag not in LANGUAGES:
                problems.append(f"{name}:{number}: not a 'tag: text' line (a block may have been split by a stray line break): {line[:60]}")
                continue
            if block is None:
                block = Block(name, number)
                blocks.append(block)
            text = unescape_sheet(line[colon + 1:].strip())
            if tag == "key": block.key = text
            elif tag in block.lines: problems.append(f"{name}:{number}: '{tag}' twice in one block")
            elif text: block.lines[tag] = text
    return blocks


# ---------------------------------------------------------------- C# side

def read_literal(src, i):
    """A string literal starting at src[i] (after optional @). Returns (text, end) or None."""
    verbatim = src[i] == "@"
    if verbatim: i += 1
    if i >= len(src) or src[i] != '"': return None
    i += 1
    out = []
    while i < len(src):
        c = src[i]
        if verbatim:
            if c == '"':
                if src[i + 1:i + 2] == '"': out.append('"'); i += 2; continue
                return "".join(out), i + 1
            out.append(c); i += 1; continue
        if c == "\\":
            n = src[i + 1]
            if n == "u": out.append(chr(int(src[i + 2:i + 6], 16))); i += 6; continue
            out.append({"n": "\n", "t": "\t", "r": "\r", "0": "\0"}.get(n, n)); i += 2; continue
        if c == '"': return "".join(out), i + 1
        out.append(c); i += 1
    return None


def skip_space(src, i):
    while i < len(src) and src[i] in " \t\r\n": i += 1
    return i


def read_concat(src, i):
    """One or more literals joined by +. Returns (text, end, open) or None; open = more is added at run time."""
    i = skip_space(src, i)
    first = read_literal(src, i)
    if first is None: return None
    text, i = first
    while True:
        j = skip_space(src, i)
        if src[j:j + 1] != "+": return text, i, False
        nxt = read_literal(src, skip_space(src, j + 1))
        if nxt is None: return text, i, True
        text, i = text + nxt[0], nxt[1]


def skip_argument(src, i):
    """Past one argument expression: returns the index of the , or ) that ends it."""
    depth = 0
    while i < len(src):
        c = src[i]
        if c in '"@$' and (c == '"' or src[i + 1:i + 2] in ('"', "@", "$")):
            j = i
            while src[j] in "@$": j += 1
            lit = read_literal(src, j if src[i] != "@" or "$" in src[i:j] else i)
            if lit is None: return i
            i = lit[1]; continue
        if c in "([{": depth += 1
        elif c in ")]}":
            if depth == 0: return i
            depth -= 1
        elif c == "," and depth == 0: return i
        i += 1
    return i


def scan_code(asked, dynamic, problems, prefixes):
    call = re.compile(r"\bLoc\.(T|N|K|C|Mark|Wire|Word)\(")
    for path in sorted(glob.glob(os.path.join(PROJECT, "Scripts", "**", "*.cs"), recursive=True)):
        if any(d in path for d in SKIP_DIRS): continue
        short = os.path.relpath(path, os.path.join(PROJECT, "Scripts"))
        if short.startswith("Core/Text/"): continue
        src = open(path, encoding="utf-8").read()
        def where(i): return f"{short}:{src.count(chr(10), 0, i) + 1}"
        def add(key, i, plural=False, by_id=False, english=None):
            if key != key.strip(): problems.append(f"{where(i)}: the text starts or ends with a space, which a sheet line cannot hold: {key!r}")
            asked.setdefault(key, {"where": [], "plural": plural, "by_id": by_id, "english": english})["where"].append(where(i))
        for m in call.finditer(src):
            kind, i = m.group(1), m.end()
            if kind == "N":
                i = skip_argument(src, i)
                one = read_concat(src, i + 1) if src[i:i + 1] == "," else None
                j = skip_space(src, one[1]) if one else 0
                other = read_concat(src, j + 1) if one and src[j:j + 1] == "," else None
                if one and other and not one[2] and not other[2]: add(one[0] + " | " + other[0], m.start(), plural=True)
                else: dynamic.append(where(m.start()) + "  Loc.N without two literal forms")
                continue
            first = read_concat(src, i)
            if first is not None and first[2] and kind == "K":
                prefixes.append(first[0])  # Loc.K("hint." + id, ...): every sheet key under the prefix is asked for
                continue
            if first is None or first[2]:
                dynamic.append(where(m.start()) + "  " + src[m.start():src.find("\n", m.start())].strip()[:110])
                continue
            if kind in ("K", "C"):
                j = skip_space(src, first[1])
                english = read_concat(src, j + 1) if src[j:j + 1] == "," else None
                if kind == "K": add(first[0], m.start(), by_id=True, english=english[0] if english else None)
                elif english and not english[2]: add(first[0] + "." + english[0], m.start(), by_id=True, english=english[0])
                else: dynamic.append(where(m.start()) + "  " + src[m.start():src.find("\n", m.start())].strip()[:110])
            else: add(first[0], m.start())
        source_lines = src.split("\n")
        for number, line in enumerate(source_lines, 1):
            # A string split over lines with + is marked once, after its last line: join the lines first.
            if re.search(r"// @loc(?::[\w.]+)?\s*$", line) and re.match(r"\s*\"", line):
                back = number - 2
                while back >= 0 and source_lines[back].rstrip().endswith("+"):
                    line = source_lines[back].rstrip() + " " + line.lstrip(); back -= 1
                joined = read_concat(line, line.find('"'))
                mark = re.search(r"// @loc(?::([\w.]+))?\s*$", line)
                if joined and back < number - 2:
                    key = (mark.group(1) + "." if mark.group(1) else "") + joined[0]
                    asked.setdefault(key, {"where": [], "plural": False, "by_id": bool(mark.group(1)), "english": joined[0] if mark.group(1) else None})["where"].append(f"{short}:{number}")
                    continue
            # "...", // @loc[:context]  marks the strings before it; a comment holding  @loc[:context] "a", "b"
            # declares text that reaches Loc from outside the code (an input system name, a quality level).
            mark = re.search(r"// @loc(?::([\w.]+))?\s*$", line)
            declared = re.search(r"//.*@loc(?::([\w.]+))?\s+(?=\")", line) if not mark else None
            if not mark and not declared: continue
            context = (mark or declared).group(1)
            i, stop, found = (0, mark.start(), 0) if mark else (declared.end(), len(line), 0)
            while i < stop:
                if line[i] == '"' or line[i:i + 2] == '@"':
                    lit = read_literal(line, i)
                    if lit is None: break
                    if lit[0].strip():
                        key = context + "." + lit[0] if context else lit[0]
                        asked.setdefault(key, {"where": [], "plural": False, "by_id": bool(context), "english": lit[0] if context else None})["where"].append(f"{short}:{number}")
                    i, found = lit[1], found + 1
                    continue
                i += 1
            if not found: problems.append(f"{short}:{number}: '// @loc' on a line without a string")


def yaml_scalar(lines, index, value, indent):
    """The value of a Unity YAML string field that starts on lines[index] after 'name: '."""
    value = value.strip()
    if value[:1] in ("'", '"'):
        quote, text, i = value[0], value[1:], index
        while True:
            end, k = None, 0
            while k < len(text):
                if quote == "'" and text[k] == "'":
                    if text[k + 1:k + 2] == "'": k += 2; continue
                    end = k; break
                if quote == '"' and text[k] == "\\": k += 2; continue
                if quote == '"' and text[k] == '"': end = k; break
                k += 1
            if end is not None: text = text[:end]; break
            i += 1
            if i >= len(lines): break
            nxt = lines[i].strip()
            text += ("\n" if not nxt else (" " if not text.endswith("\n") else "") + nxt)
        if quote == "'": return text.replace("''", "'")
        text = re.sub(r"\\u([0-9a-fA-F]{4})", lambda m: chr(int(m.group(1), 16)), text)
        text = re.sub(r"\\x([0-9a-fA-F]{2})", lambda m: chr(int(m.group(1), 16)), text)
        return text.replace("\\n", "\n").replace('\\"', '"').replace("\\\\", "\\")
    text, i = value, index + 1
    while i < len(lines) and lines[i].strip() and len(lines[i]) - len(lines[i].lstrip()) > indent and not re.match(r"\s*[-<\w][^:]*:(\s|$)", lines[i]):
        text += " " + lines[i].strip(); i += 1
    return text


def scan_assets(asked):
    field = re.compile(r"^(\s*)<(" + "|".join(ASSET_FIELDS + ASSET_LIST_FIELDS) + r")>k__BackingField:(.*)$")
    for path in sorted(glob.glob(os.path.join(PROJECT, "Data", "**", "*.asset"), recursive=True)):
        short = "Data/" + os.path.relpath(path, os.path.join(PROJECT, "Data"))
        lines = open(path, encoding="utf-8", errors="replace").read().split("\n")
        for index, line in enumerate(lines):
            m = field.match(line)
            if not m: continue
            indent, texts = len(m.group(1)), []
            if m.group(2) in ASSET_LIST_FIELDS:
                item = index + 1
                while item < len(lines) and lines[item].startswith(" " * indent + "- "):
                    texts.append(yaml_scalar(lines, item, lines[item][indent + 2:], indent))
                    item += 1
                    while item < len(lines) and lines[item].strip() and len(lines[item]) - len(lines[item].lstrip()) > indent: item += 1
            else: texts.append(yaml_scalar(lines, index, m.group(3), indent))
            for text in texts:
                text = text.strip()
                if text and re.search(r"[A-Za-z]{2}", text):
                    asked.setdefault(text, {"where": [], "plural": False, "by_id": False, "english": None})["where"].append(short)


# ---------------------------------------------------------------- checks

def main():
    show_missing, show_unused = "--missing" in sys.argv, "--unused" in sys.argv
    problems, warnings, dynamic, asked, prefixes = [], [], [], {}, []
    blocks = read_sheets(problems)
    scan_code(asked, dynamic, problems, prefixes)
    scan_assets(asked)

    by_id = {}
    for b in blocks:
        at = f"{b.sheet}:{b.line}"
        if "en" not in b.lines and b.key is None:
            problems.append(f"{at}: a block without an 'en:' line"); continue
        if b.id in by_id: problems.append(f"{at}: the same line is already in {by_id[b.id].sheet}:{by_id[b.id].line}: {b.id[:60]!r}")
        by_id[b.id] = b
        english = b.lines.get("en", "")
        # English reads by-id lines from the sheet too, so the sheet must say what the code says.
        coded = asked.get(b.id, {}).get("english") if b.key else None
        if coded is not None and coded != english: problems.append(f"{at}: the sheet's English differs from the code's ({coded[:50]!r})")
        want = sorted(set(PLACEHOLDER.findall(english)))
        plural = asked.get(b.id, {}).get("plural", False)
        letters = re.sub(PLACEHOLDER, "", english)
        wordy = bool(re.search(r"[a-z]{3}", letters)) or bool(re.search(r"\b[A-Z]{4,}\b", letters))
        for tag in LANGUAGES:
            text = b.lines.get(tag)
            if text is None: continue
            if sorted(set(PLACEHOLDER.findall(text))) != want:
                problems.append(f"{at}: {tag} has other placeholders than the English ({' '.join(want) or 'none'}): {text[:70]}")
            if text.count("\n") != english.count("\n"): warnings.append(f"{at}: {tag} has {text.count(chr(10))} line breaks, the English {english.count(chr(10))}")
            if plural:
                forms = len(text.split(" | "))
                if forms != PLURAL_FORMS[tag]: problems.append(f"{at}: {tag} needs {PLURAL_FORMS[tag]} form(s) split by ' | ', has {forms}")
            # Left exactly as the English on purpose (a brand, "MSAA 2x") is fine; Latin words that differ are not.
            latin = text != english and re.search(r"[A-Za-z]{3}", re.sub(PLACEHOLDER, "", text))
            if wordy and latin and tag in ("zh-CN", "ja", "ko") and not CJK.search(text): warnings.append(f"{at}: {tag} has no {tag} characters: {text[:60]}")
            if wordy and latin and tag == "ru" and not CYRILLIC.search(text): warnings.append(f"{at}: ru has no Cyrillic: {text[:60]}")
            if tag not in ("zh-CN", "ja", "ko") and CJK.search(text): problems.append(f"{at}: {tag} holds CJK characters (lines swapped?): {text[:60]}")
            if tag != "ru" and CYRILLIC.search(text): problems.append(f"{at}: {tag} holds Cyrillic (lines swapped?): {text[:60]}")
        if plural and len(english.split(" | ")) != 2: problems.append(f"{at}: a counted English line needs two forms split by ' | '")

    missing = {key: info for key, info in asked.items() if key not in by_id}
    unused = [b for b in blocks if b.id is not None and b.id not in asked and not (b.key and any(b.key.startswith(p) for p in prefixes))]
    total = len(by_id)
    print(f"Sheets: {total} lines in {len(set(b.sheet for b in blocks))} files. Asked for by code and data: {len(asked)}.")
    for tag in LANGUAGES:
        have = sum(1 for b in by_id.values() if tag in b.lines)
        print(f"  {tag:6} {have:5} / {total}" + ("" if have == total else f"   ({total - have} lines stay English)"))
    print(f"Missing from the sheets: {len(missing)}.   In the sheets but never asked for: {len(unused)}.   Loc calls with text built at run time: {len(dynamic)}.")
    for label, items in (("PROBLEMS", problems), ("WARNINGS", warnings)):
        if items:
            print(f"\n{label} ({len(items)})")
            for item in items: print("  " + item)
    if show_missing and missing:
        print("\nMISSING (paste into a sheet and translate)")
        groups = {}
        for key, info in missing.items(): groups.setdefault(info["where"][0].rsplit(":", 1)[0], []).append((key, info))
        for source in sorted(groups):
            print(f"\n# {source}")
            for key, info in groups[source]:
                if info["by_id"]: print(f"key: {key}\nen: {(info['english'] or '').replace(chr(10), chr(92) + 'n')}\n")
                else: print("en: " + key.replace("\\", "\\\\").replace("\n", "\\n") + "\n")
    if show_unused and unused:
        print("\nNEVER ASKED FOR")
        for b in unused: print(f"  {b.sheet}:{b.line}: {str(b.id)[:90]}")
    if "--dynamic" in sys.argv and dynamic:
        print("\nBUILT AT RUN TIME (make sure the text they pass is marked with // @loc or Loc.Mark)")
        for d in dynamic: print("  " + d)
    sys.exit(1 if problems else 0)


if __name__ == "__main__":
    main()

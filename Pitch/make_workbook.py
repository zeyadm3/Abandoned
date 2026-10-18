"""Builds 'Sarab Studio - Budget.xlsx' from budget_model.py.
Every formula is written with its value, so previews in mail and chat apps show numbers."""
import sys
import xlsxwriter
import budget_model as m

problems = m.check()
if problems:
    sys.exit("Budget does not add up:\n" + "\n".join(problems))

OUT = "Sarab Studio - Budget.xlsx"
wb = xlsxwriter.Workbook(OUT)
INK, YELLOW, SOFT, BAND, RULE, MUTED, BLUE = "#0E1116", "#F2C230", "#FCF3D2", "#F4F6F8", "#D9DEE4", "#5B6572", "#1D4ED8"
AED = '#,##0;-#,##0;"–"'
PCT = '0%;-0%;"–"'
_cache = {}

def fmt(**k):
    base = {"font_name": "Arial", "font_size": 10, "valign": "vcenter", "font_color": INK}
    base.update(k)
    key = tuple(sorted(base.items()))
    if key not in _cache:
        _cache[key] = wb.add_format(base)
    return _cache[key]

def title_bar(ws, last_col, title, subtitle):
    ws.set_row(0, 36); ws.set_row(1, 20); ws.set_row(2, 10)
    ws.merge_range(0, 0, 0, last_col, title, fmt(bold=True, font_size=18, font_color="#FFFFFF", bg_color=INK, indent=1))
    ws.merge_range(1, 0, 1, last_col, subtitle, fmt(font_size=10, font_color=YELLOW, bg_color=INK, indent=1))

def header(ws, row, labels, height=22):
    ws.set_row(row, height)
    for c, text in enumerate(labels):
        ws.write(row, c, text, fmt(bold=True, font_color="#FFFFFF", bg_color=INK, text_wrap=True,
                                   align="left" if c == 0 else "center", indent=1 if c == 0 else 0, border=1, border_color=INK))

def section(ws, row, text):
    ws.set_row(row, 24)
    ws.write(row, 0, text, fmt(bold=True, font_size=12))

def body(band, **k):
    k.setdefault("bottom", 1); k.setdefault("bottom_color", RULE)
    if band:
        k["bg_color"] = BAND
    return fmt(**k)

def finish(ws, cols, tab=None):
    ws.hide_gridlines(2)
    ws.set_landscape(); ws.set_paper(9); ws.fit_to_pages(1, 0); ws.center_horizontally()
    ws.set_margins(left=0.4, right=0.4, top=0.5, bottom=0.5)
    for c, w in enumerate(cols):
        ws.set_column(c, c, w)
    if tab:
        ws.set_tab_color(tab)

# ---------- Assumptions ----------
A = {}
# Sheets are created in the order they appear; Assumptions is filled first so the others know its cells
SHEETS = {n: wb.add_worksheet(n) for n in ["Summary", *m.TIERS, "Assumptions"]}
asm = SHEETS["Assumptions"]
title_bar(asm, 3, "Assumptions and checks", "Blue figures are inputs. Everything else in the workbook is worked out from them.")

section(asm, 3, "What we keep from one sale")
header(asm, 4, ["Step", "Rate", "AED left", ""])
steps = [("List price ($9.99 tier)", None, m.PRICE),
         ("Average sale after regional prices and discounts", m.AVG_SALE, None),
         ("Less VAT and sales tax inside the price", m.VAT, None),
         ("Less refunds", m.REFUNDS, None),
         ("Less Steam's share", m.STEAM, None),
         ("Less US withholding: 30% tax on the 30% of sales from US buyers", m.US_SHARE * m.US_WITHHOLD, None)]
left = m.PRICE
for i, (label, rate, value) in enumerate(steps):
    r = 5 + i; band = i % 2 == 1
    asm.write(r, 0, label, body(band, indent=1))
    if rate is None:
        asm.write(r, 1, "", body(band))
        asm.write(r, 2, value, body(band, num_format="0.00", font_color=BLUE))
    else:
        left = left * rate if i == 1 else left * (1 - rate)
        if i == 5:
            asm.write_formula(r, 1, "=0.3*0.3", body(band, num_format="0%", font_color=BLUE, align="center"), rate)
        else:
            asm.write(r, 1, rate, body(band, num_format="0%", font_color=BLUE, align="center"))
        f = f"=C{r}*B{r+1}" if i == 1 else f"=C{r}*(1-B{r+1})"
        asm.write_formula(r, 2, f, body(band, num_format="0.00"), left)
    asm.write(r, 3, "", body(band))
r = 5 + len(steps)
asm.write(r, 0, "Studio keeps per copy", fmt(bold=True, bg_color=SOFT, indent=1))
asm.write(r, 1, "", fmt(bg_color=SOFT))
asm.write_formula(r, 2, f"=C{r}", fmt(bold=True, bg_color=SOFT, num_format="0.00"), m.NET)
asm.write(r, 3, "", fmt(bg_color=SOFT))
A["net"] = f"Assumptions!$C${r+1}"

r += 2; section(asm, r, "Other inputs"); r += 1
header(asm, r, ["Input", "Value", "", ""]); r += 1
inputs = [("peg", "AED per US dollar (fixed peg)", m.PEG, "0.0000"),
          ("unity", "Unity Personal limit, USD of funding plus revenue over 12 months", m.UNITY_LIMIT_USD, AED),
          ("zeyad", "Founder pay a month, Zeyad", m.FOUNDERS[0][1], AED),
          ("yousef", "Founder pay a month, Yousef", m.FOUNDERS[1][1], AED),
          ("jihad", "Founder pay a month, Jihad", m.FOUNDERS[2][1], AED),
          ("setups", "Work setups", m.SETUPS, AED),
          ("setup_price", "Price per setup", m.SETUP_PRICE, AED),
          ("langs", "Languages checked by native speakers", m.LANGUAGES, AED),
          ("lang_price", "Price per language", m.LANGUAGE_PRICE, AED),
          ("interns", "Interns (Max plan only)", m.INTERNS, AED),
          ("intern_pay", "Intern pay per month", m.INTERN_PAY, AED),
          ("intern_months", "Intern months", m.INTERN_MONTHS, AED)]
for i, (key, label, value, nf) in enumerate(inputs):
    band = i % 2 == 1
    asm.write(r, 0, label, body(band, indent=1))
    asm.write(r, 1, value, body(band, num_format=nf, font_color=BLUE, align="right"))
    asm.write(r, 2, "", body(band)); asm.write(r, 3, "", body(band))
    A[key] = f"Assumptions!$B${r+1}"
    r += 1

r += 1; section(asm, r, "Software, 14 months"); r += 1
header(asm, r, ["Item", "AED a month", "Count", "14 months"]); r += 1
sw_first = r
for i, (label, price, count) in enumerate(m.SOFTWARE):
    band = i % 2 == 1
    asm.write(r, 0, label, body(band, indent=1))
    asm.write(r, 1, price, body(band, num_format=AED, font_color=BLUE))
    asm.write(r, 2, count, body(band, num_format=AED, font_color=BLUE))
    asm.write_formula(r, 3, f"=B{r+1}*C{r+1}*14", body(band, num_format=AED), price * count * 14)
    r += 1
band = len(m.SOFTWARE) % 2 == 1
asm.write(r, 0, f"Domain (AED {m.DOMAIN_PER_YEAR} a year)", body(band, indent=1))
asm.write(r, 1, m.DOMAIN_PER_YEAR, body(band, num_format=AED, font_color=BLUE))
asm.write(r, 2, "per year", body(band, align="center", font_color=MUTED))
asm.write_formula(r, 3, f"=B{r+1}*14/12", body(band, num_format=AED), m.DOMAIN_PER_YEAR * 14 / 12)
r += 1
asm.write(r, 0, "Itemised total", fmt(bold=True, top=1, indent=1))
asm.write(r, 1, "", fmt(top=1)); asm.write(r, 2, "", fmt(top=1))
asm.write_formula(r, 3, f"=SUM(D{sw_first+1}:D{r})", fmt(bold=True, top=1, num_format=AED), m.SOFTWARE_ITEMISED)
A["sw_itemised"] = f"Assumptions!$D${r+1}"; r += 1
asm.write(r, 0, "Budgeted, rounded up", fmt(bold=True, bg_color=SOFT, indent=1))
asm.write(r, 1, "", fmt(bg_color=SOFT)); asm.write(r, 2, "", fmt(bg_color=SOFT))
asm.write(r, 3, m.SOFTWARE_BUDGET, fmt(bold=True, bg_color=SOFT, num_format=AED, font_color=BLUE))
A["sw_budget"] = f"Assumptions!$D${r+1}"; r += 1
asm.write(r, 0, "Prices are the plans' list prices in dirhams. Check them before the meeting.", fmt(italic=True, font_color=MUTED, indent=1))
checks_row = r + 2
finish(asm, [62, 14, 14, 14], INK)

# ---------- Plan sheets ----------
LINE_ROW = {k: 4 + i for i, k in enumerate(m.LINES)}          # 0-based rows 4..13
RES_ROW, TOT_ROW, SHARE_ROW = 14, 15, 16
plans = {t: m.plan(t) for t in m.TIERS}
for t in m.TIERS:
    p = plans[t]
    ws = SHEETS[t]
    title_bar(ws, 6, f"{t} plan", "What each tranche pays for. All amounts in AED.")
    header(ws, 3, ["Line", "Tranche 1\nat start\nNov 2026", "Tranche 2\nMar 2027\n10k copies, 75% reviews",
                   "Tranche 3\nSep 2027\n20k copies", "Reserve\nheld back", "Line total", "How it is split"], height=50)
    for i, (k, (label, note, _)) in enumerate(m.LINES.items()):
        r = LINE_ROW[k]; band = i % 2 == 1; vals = p["rows"][k]
        ws.set_row(r, 20)
        ws.write(r, 0, label, body(band, indent=1))
        for j, v in enumerate(vals):
            ws.write(r, 1 + j, v, body(band, num_format=AED, font_color=BLUE))
        ws.write(r, 4, 0, body(band, num_format=AED, font_color=MUTED))
        ws.write_formula(r, 5, f"=SUM(B{r+1}:E{r+1})", body(band, num_format=AED, bold=True), sum(vals))
        funded = sum(vals) > 0
        ws.write(r, 6, note if funded else m.NOT_FUNDED, body(band, font_color=MUTED, indent=1))
    first, last = LINE_ROW["founders"] + 1, LINE_ROW["interns"] + 1
    r = RES_ROW; ws.set_row(r, 20)
    ws.write(r, 0, "Reserve, held back for overruns", fmt(italic=True, indent=1, bottom=1, bottom_color=RULE))
    for j in range(3):
        ws.write(r, 1 + j, 0, fmt(num_format=AED, font_color=MUTED, bottom=1, bottom_color=RULE))
    ws.write_formula(r, 4, f"=F{TOT_ROW+1}-SUM(B{first}:D{last})", fmt(num_format=AED, bottom=1, bottom_color=RULE), p["reserve"])
    ws.write_formula(r, 5, f"=SUM(B{r+1}:E{r+1})", fmt(num_format=AED, bold=True, bottom=1, bottom_color=RULE), p["reserve"])
    ws.write(r, 6, "Whatever is left between the spend and the total ask, about 10%",
             fmt(font_color=MUTED, indent=1, bottom=1, bottom_color=RULE))
    r = TOT_ROW; ws.set_row(r, 26)
    tot = dict(bold=True, bg_color=YELLOW, font_size=11)
    ws.write(r, 0, "Total we receive", fmt(indent=1, **tot))
    for j in range(4):
        col = "BCDE"[j]
        v = p["tranches"][j] if j < 3 else p["reserve"]
        ws.write_formula(r, 1 + j, f"=SUM({col}{first}:{col}{RES_ROW+1})", fmt(num_format=AED, **tot), v)
    ws.write(r, 5, p["total"], fmt(num_format=AED, font_color=BLUE, **tot))
    ws.write(r, 6, "Total ask (blue: change it to move the reserve)", fmt(indent=1, **{**tot, "bold": False}))
    r = SHARE_ROW
    ws.write(r, 0, "Share of the total ask", fmt(font_color=MUTED, indent=1))
    for j in range(5):
        col = "BCDEF"[j]
        v = (p["tranches"] + [p["reserve"], p["total"]])[j] / p["total"]
        ws.write_formula(r, 1 + j, f"={col}{TOT_ROW+1}/$F${TOT_ROW+1}", fmt(num_format=PCT, font_color=MUTED, align="right"), v)
    notes = ["Blue figures are inputs. Change a tranche amount and the line total, reserve and summary update.",
             "Tranche 2 is released at the end of February 2027 if at least 10,000 copies have sold and reviews are at least 75% positive.",
             "Tranche 3 is released in September 2027 if at least 20,000 copies have sold."]
    for i, n in enumerate(notes):
        ws.write(18 + i, 0, n, fmt(italic=True, font_color=MUTED, indent=1))
    ws.freeze_panes(4, 1)
    finish(ws, [38, 14, 15, 14, 12, 13, 62], INK)

# ---------- Checks on the Assumptions sheet ----------
r = checks_row
section(asm, r, "Checks"); r += 1
header(asm, r, ["Check", "Lean", "Standard", "Max"]); r += 1
def ref(t, col, row0):
    return f"'{t}'!${col}${row0+1}"
def total(t, k):
    return sum(plans[t]["rows"][k])
# Each check: label, Excel condition, and the same condition in Python for the stored value
checks = [
    ("Founder pay equals the three monthly amounts x 14 months",
     lambda t: f"{ref(t,'F',LINE_ROW['founders'])}=({A['zeyad']}+{A['yousef']}+{A['jihad']})*14",
     lambda t: total(t, "founders") == sum(v for _, v in m.FOUNDERS) * 14),
    ("Software equals the software budget",
     lambda t: f"{ref(t,'F',LINE_ROW['software'])}={A['sw_budget']}",
     lambda t: total(t, "software") == m.SOFTWARE_BUDGET),
    ("Software budget covers the itemised prices",
     lambda t: f"{A['sw_budget']}>={A['sw_itemised']}",
     lambda t: m.SOFTWARE_BUDGET >= m.SOFTWARE_ITEMISED),
    ("Setups equal count x price",
     lambda t: f"{ref(t,'F',LINE_ROW['setups'])}={A['setups']}*{A['setup_price']}",
     lambda t: total(t, "setups") == m.SETUPS * m.SETUP_PRICE),
    ("Language checks equal languages x price",
     lambda t: f"{ref(t,'F',LINE_ROW['languages'])}={A['langs']}*{A['lang_price']}",
     lambda t: total(t, "languages") == m.LANGUAGES * m.LANGUAGE_PRICE),
    ("Interns: 3 x 3,000 x 6 months in Max, none in Lean and Standard",
     lambda t: (f"{ref(t,'F',LINE_ROW['interns'])}={A['interns']}*{A['intern_pay']}*{A['intern_months']}" if t == "Max"
                else f"{ref(t,'F',LINE_ROW['interns'])}=0"),
     lambda t: total(t, "interns") == (m.INTERNS * m.INTERN_PAY * m.INTERN_MONTHS if t == "Max" else 0)),
    ("Reserve is between 9% and 11% of spend",
     lambda t: f"AND({ref(t,'E',TOT_ROW)}/SUM({ref(t,'B',TOT_ROW)}:{ref(t,'D',TOT_ROW)})>=0.09,"
               f"{ref(t,'E',TOT_ROW)}/SUM({ref(t,'B',TOT_ROW)}:{ref(t,'D',TOT_ROW)})<=0.11)",
     lambda t: 0.09 <= plans[t]["reserve"] / plans[t]["spend"] <= 0.11),
    ("Tranches plus reserve equal the total ask",
     lambda t: f"SUM({ref(t,'B',TOT_ROW)}:{ref(t,'E',TOT_ROW)})={ref(t,'F',TOT_ROW)}",
     lambda t: sum(plans[t]["tranches"]) + plans[t]["reserve"] == plans[t]["total"]),
    ("Under Unity's USD 200,000 limit before game sales (if not, Unity Pro is needed)",
     lambda t: f"{ref(t,'F',TOT_ROW)}/{A['peg']}<{A['unity']}",
     lambda t: plans[t]["total"] / m.PEG < m.UNITY_LIMIT_USD),
]
check_first = r
for i, (label, f, ok) in enumerate(checks):
    band = i % 2 == 1
    asm.write(r, 0, label, body(band, indent=1))
    for j, t in enumerate(m.TIERS):
        asm.write_formula(r, 1 + j, f'=IF({f(t)},"OK","CHECK")', body(band, align="center", bold=True), "OK" if ok(t) else "CHECK")
    r += 1
asm.conditional_format(check_first, 1, r - 1, 3, {"type": "cell", "criteria": "==", "value": '"OK"',
                                                    "format": wb.add_format({"font_color": "#17803D"})})
asm.conditional_format(check_first, 1, r - 1, 3, {"type": "cell", "criteria": "==", "value": '"CHECK"',
                                                    "format": wb.add_format({"font_color": "#B42318", "bg_color": "#FDE8E6"})})

# ---------- Summary ----------
s = SHEETS["Summary"]
title_bar(s, 3, "ABANDONED funding request", "Sarab Studio. Three options over 14 months, November 2026 to December 2027. All amounts in AED.")
def tier_row(row, label, src, values, style="body", band=False, nf=AED, note=None):
    st = {"body": dict(), "total": dict(bold=True, bg_color=YELLOW, font_size=11), "sub": dict(bold=True, top=1),
          "muted": dict(font_color=MUTED)}[style]
    lf = fmt(indent=1, **st) if style != "body" else body(band, indent=1)
    s.write(row, 0, label, lf)
    for j, t in enumerate(m.TIERS):
        nfmt = fmt(num_format=nf, align="right", **st) if style != "body" else body(band, num_format=nf)
        s.write_formula(row, 1 + j, src(t, "BCD"[j]), nfmt, values[j])

r = 3; header(s, r, ["What we receive", "Lean", "Standard", "Max"]); r += 1
P = [plans[t] for t in m.TIERS]
tier_row(r, "Total ask", lambda t, c: f"={ref(t,'F',TOT_ROW)}", [p["total"] for p in P], "total"); r += 1
for j, label in enumerate(["Tranche 1, at the start (November 2026)",
                           "Tranche 2, at 10,000 copies and 75% positive reviews (March 2027)",
                           "Tranche 3, at 20,000 copies (September 2027)"]):
    tier_row(r, label, lambda t, c, j=j: f"={ref(t,'BCD'[j],TOT_ROW)}", [p["tranches"][j] for p in P], band=j % 2 == 1); r += 1
tier_row(r, "Reserve, held back for overruns", lambda t, c: f"={ref(t,'E',TOT_ROW)}", [p["reserve"] for p in P], band=True); r += 1
tot_r = 5
tier_row(r, "Total ask in US dollars", lambda t, c: f"={c}{tot_r}/{A['peg']}", [p["total"] / m.PEG for p in P], "muted"); r += 1

r += 1; header(s, r, ["Where the money goes", "Lean", "Standard", "Max"]); r += 1
line_first = r
for i, (k, (label, _, _)) in enumerate(m.LINES.items()):
    tier_row(r, label, lambda t, c, k=k: f"={ref(t,'F',LINE_ROW[k])}", [sum(p["rows"][k]) for p in P], band=i % 2 == 1); r += 1
spend_r = r
tier_row(r, "Spend", lambda t, c: f"=SUM({c}{line_first+1}:{c}{r})", [p["spend"] for p in P], "sub"); r += 1
tier_row(r, "Reserve", lambda t, c: f"={ref(t,'E',TOT_ROW)}", [p["reserve"] for p in P]); r += 1
tier_row(r, "Total ask", lambda t, c: f"={c}{spend_r+1}+{c}{r}", [p["total"] for p in P], "total"); r += 1

r += 1; header(s, r, ["Key figures", "Lean", "Standard", "Max"]); r += 1
mk_r = line_first + list(m.LINES).index("marketing") + 1
tier_row(r, "Tranche 1 as a share of the total ask", lambda t, c: f"={c}6/{c}{tot_r}", [p["tranches"][0] / p["total"] for p in P], nf="0%"); r += 1
tier_row(r, "Marketing as a share of spend", lambda t, c: f"={c}{mk_r}/{c}{spend_r+1}", [sum(p["rows"]["marketing"]) / p["spend"] for p in P], nf="0%", band=True); r += 1
tier_row(r, "Copies to sell to earn back the total ask", lambda t, c: f"={c}{tot_r}/{A['net']}", [p["total"] / m.NET for p in P]); r += 1
tier_row(r, "Room under Unity's USD 200,000 limit, before game sales (USD)", lambda t, c: f"={A['unity']}-{c}{tot_r}/{A['peg']}",
         [m.UNITY_LIMIT_USD - p["total"] / m.PEG for p in P], band=True); r += 1
r += 1
for n in [f"Each copy earns the studio about AED {m.NET:.2f} after regional prices, VAT, refunds, Steam's share and US withholding (see Assumptions).",
          "Tranche 2 is released at the end of February 2027 if at least 10,000 copies have sold and reviews are at least 75% positive.",
          "Tranche 3 is released in September 2027 if at least 20,000 copies have sold. The reserve is only drawn if costs run over.",
          "Unity's limit counts funding plus game revenue over 12 months. A plan above it, or one that sells past it, needs Unity Pro."]:
    s.write(r, 0, n, fmt(italic=True, font_color=MUTED, indent=1)); r += 1
s.freeze_panes(3, 0)
finish(s, [66, 15, 15, 15], YELLOW)

s.activate(); s.set_first_sheet()
wb.close()
print("Wrote", OUT)

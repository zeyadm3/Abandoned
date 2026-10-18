"""Sarab Studio / ABANDONED: three funding options in AED, Nov 2026 - Dec 2027 (14 months).
Every line is funded in AED amounts across three tranches; the reserve is held back.
Run it to print the budget and to check that every split adds up."""
TIERS = ("Lean", "Standard", "Max")
PEG = 3.6725           # AED per USD
UNITY_LIMIT_USD = 200000

# What the studio keeps from one AED 36.70 sale ($9.99 tier)
PRICE = 36.70
AVG_SALE = 0.78        # regional pricing and discounts
VAT = 0.06             # sales tax and VAT inside the price
REFUNDS = 0.04
STEAM = 0.30
US_SHARE = 0.30        # share of revenue from US buyers
US_WITHHOLD = 0.30     # no US-UAE tax treaty
NET = PRICE * AVG_SALE * (1 - VAT) * (1 - REFUNDS) * (1 - STEAM) * (1 - US_SHARE * US_WITHHOLD)

# Software, AED a month (the user's prices; check before the pitch)
SOFTWARE = [("Claude Max (Zeyad, Yousef)", 370, 2), ("ChatGPT Pro (Zeyad, Yousef)", 375, 2), ("Claude Pro (Jihad)", 75, 1),
            ("Google Workspace mailboxes on our domain", 26, 3), ("Website hosting and builder", 75, 1)]
DOMAIN_PER_YEAR = 100
MONTHS = 14
SOFTWARE_ITEMISED = sum(p * n * MONTHS for _, p, n in SOFTWARE) + DOMAIN_PER_YEAR * MONTHS / 12
SOFTWARE_BUDGET = 25000   # itemised total rounded up

FOUNDERS = (("Zeyad", 5000), ("Yousef", 5000), ("Jihad", 5000))   # AED a month each, for all 14 months
SETUPS, SETUP_PRICE = 3, 20000
LANGUAGES, LANGUAGE_PRICE = 11, 1000
INTERNS, INTERN_PAY, INTERN_MONTHS = 3, 3000, 6

TOTAL_ASK = (550000, 620000, 730000)   # Max stays under Unity's USD 200,000 (AED 734,500)

# key: (label, note, {tier: (tranche 1, tranche 2, tranche 3)}); one tuple for all tiers when they match
LINES = {
    "founders": ("Founder pay (3 x 5,000 a month)", "Zeyad, Yousef and Jihad at 5,000 a month each, for the months each tranche covers (4, 6 and 4)",
                 (60000, 90000, 60000)),
    "marketing": ("Marketing and community", "Full budget at the start",
                  {"Lean": (160000, 0, 0), "Standard": (210000, 0, 0), "Max": (240000, 0, 0)}),
    "software": ("Software, website and domain", "Monthly costs for the months each tranche covers (4, 6 and 4)",
                 (7000, 11000, 7000)),
    "setups": ("Work setups (3 x 20,000)", "Two setups now, the third with tranche 2",
               (40000, 20000, 0)),
    "deck": ("Steam Deck for testing", "Bought before launch",
             (2400, 0, 0)),
    "legal": ("Legal, accounting and bookkeeping", "Company setup first, then running costs",
              (10000, 6000, 4000)),
    "languages": ("Language checks (11 x 1,000)", "Launch text before December, DLC text in 2027",
                  (8000, 3000, 0)),
    "events": ("Events", "Spread across the year",
               (3000, 4000, 3000)),
    "freelancers": ("UAE freelancers (art, sound)", "Spread across the year",
                    {"Lean": (0, 0, 0), "Standard": (4500, 6000, 4500), "Max": (9000, 12000, 9000)}),
    "interns": ("UAE interns (3 x 3,000 x 6 months)", "November to February from tranche 1, March and April from tranche 2",
                {"Lean": (0, 0, 0), "Standard": (0, 0, 0), "Max": (36000, 18000, 0)}),
}
NOT_FUNDED = "Not in this plan"

# What each line must add up to, worked out from the inputs above
EXPECTED = {
    "founders": lambda t: sum(v for _, v in FOUNDERS) * MONTHS,
    "software": lambda t: SOFTWARE_BUDGET,
    "setups": lambda t: SETUPS * SETUP_PRICE,
    "deck": lambda t: 2400,
    "legal": lambda t: 20000,
    "languages": lambda t: LANGUAGES * LANGUAGE_PRICE,
    "events": lambda t: 10000,
    "interns": lambda t: INTERNS * INTERN_PAY * INTERN_MONTHS if t == "Max" else 0,
}

def split(key, tier):
    s = LINES[key][2]
    return s[tier] if isinstance(s, dict) else s

def plan(tier):
    rows = {k: split(k, tier) for k in LINES}
    tranches = [sum(r[j] for r in rows.values()) for j in range(3)]
    spend = sum(tranches)
    total = TOTAL_ASK[TIERS.index(tier)]
    return {"rows": rows, "tranches": tranches, "spend": spend, "reserve": total - spend, "total": total}

def check():
    problems = []
    for t in TIERS:
        p = plan(t)
        for k, f in EXPECTED.items():
            if sum(p["rows"][k]) != f(t):
                problems.append(f"{t}: {k} adds up to {sum(p['rows'][k]):,} but should be {f(t):,}")
        if sum(p["tranches"]) + p["reserve"] != p["total"]:
            problems.append(f"{t}: tranches and reserve do not add up to the total ask")
        if not 0.09 <= p["reserve"] / p["spend"] <= 0.11:
            problems.append(f"{t}: reserve is {p['reserve'] / p['spend']:.1%} of spend")
    if SOFTWARE_ITEMISED > SOFTWARE_BUDGET:
        problems.append("software budget is below the itemised total")
    return problems

if __name__ == "__main__":
    for t in TIERS:
        p = plan(t)
        print(f"\n== {t}: total ask AED {p['total']:,} (USD {p['total'] / PEG:,.0f})")
        for k, (label, _, _) in LINES.items():
            r = p["rows"][k]
            print(f"  {label:40s}" + "".join(f"{v:>10,}" for v in r) + f"{sum(r):>11,}")
        print(f"  {'Tranches':40s}" + "".join(f"{v:>10,}" for v in p["tranches"]) + f"{p['spend']:>11,}")
        print(f"  Reserve {p['reserve']:,} ({p['reserve'] / p['spend']:.1%}), break-even {p['total'] / NET:,.0f} copies")
    print(f"\nNet per copy AED {NET:.2f}; software itemised {SOFTWARE_ITEMISED:,.0f}")
    print("\n".join(check()) or "All checks pass")

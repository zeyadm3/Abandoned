# ABANDONED — Balance sheet (M10.9)

First-pass numbers for Early Access, set without playtest data (the user brief for M10 was "no testing").
Everything here is data: change the asset, not the code. Retune after real sessions (PLAYBOOK playtest
checklist, GDD 27).

## The money loop

| Lever | Where | Value | Why |
|---|---|---|---|
| Quota at level 1 | Data/Contracts/ContractConfig `BaseQuota` | $30,000 | A full crew clears it in one careful run. |
| Quota growth | ContractConfig `QuotaGrowth` | +15 % per level | L5 $48k, L10 $70k, L15 $93k (full crew). |
| Quota spread | ContractConfig `QuotaSpread` | ±15 % | Boards differ. |
| Crew size | ContractConfig `CrewQuotaScale` | 1: 55 %, 2: 75 %, 3: 90 %, 4: 100 % | GDD 3: solo playable, co-op the focus. Applied when the van leaves (the crew that drives owes it). |
| Payout | CompanyLedger | the whole haul, × (1 + bonus) when the quota is met | Unchanged from M6. |
| Running costs (new) | Data/Company/CompanyConfig `RunningCostBase` / `PerLevel` | $6,000 + $1,200 per level above 1 | Money was meaningless (one run bought the whole shop). Every job now costs fuel and rent, so a bad run hurts even before the penalty. |
| Missed quota | CompanyConfig `ShortfallPenalty` | 50 % of the shortfall | Unchanged. |
| Bankruptcy | CompanyConfig `MissesToBankruptcy` | 3 in a row | GDD 13. |

What's in the building: 144 loot points × 60 % fill ≈ 86 items, ≈ $300k ordinary loot (average item ≈ $3.5k;
pocket $2.6k, one-hand $1.2k, two-hand $5.5k, heavy $4.7k) plus 1–2 jackpots ($35k–$100k). A crew that gets
a quarter of it out makes ≈ $75k; minus $6k–$23k running costs. Locked stores (M10.4) add 50 % to what's inside
them.

## Experience and levels

XP = haul / $1,000 + 10 for meeting the quota (CompanyConfig). A $50k run ≈ 60 XP: level 3 after the first
job, level 8 around job 6, level 10 around job 9, level 15 around job 20, level 20 around job 35.

## Unlock table (GDD 13)

| Level | Gear (shop) | Truck | Contracts |
|---|---|---|---|
| 1 | flashlight $400, radio $600, hand trolley $2,500, medkit $1,500 (single use), crowbar $2,000 | – | power off, unstable, rush job, fragile collection, heavy jackpot, picked over |
| 2 | planks $800 (single use), noise maker $1,200 (single use), backpack $4,000, bolt cutters $3,500 | Bigger Bay $12,000 (cargo ×1.35) | storm, sealed |
| 3 | stress scanner $8,000, flatbed trolley $9,000 ("better trolley") | Floodlights $8,000 | |
| 4 | motion detector $9,000 | Tuned Engine $10,000 (honk 7 s) | |
| 5 | | Box Truck $30,000 (cargo ×1.75; GDD "truck, more cargo") | |
| 6 | night vision $14,000 | | |
| 7 | | V8 $25,000 (honk 4 s) | |
| 8 | rope & pulley $2,000 (single use), support jack $2,500 (single use) | | hot property ("harder modifiers") |
| 9 | | Armored Bay $40,000 | |
| 15 | | | night job, condemned ("night contracts, elite modifiers") |

The new company's starter kit (4 flashlights, 4 radios, a medkit, a hand trolley) is free. Bankruptcy takes
everything but cosmetics and achievements.

## Danger and time (unchanged from M5/M9)

Extraction window 10–18 min (rush job ×0.6, hot property ×0.55). Danger rises over the window (DangerConfig):
faster decay, sharper hearing, faster monsters, more of them; after the window it climbs sharply. Storm: hearing
×0.6, decay ×1.4.

## What to watch in playtests

- Do crews make quota on the first job without knowing the game? If not: BaseQuota down to $25k.
- Does money pile up by level 5? Then raise RunningCostPerLevel or the truck prices.
- Solo: do players reach level 3 without going bankrupt? Tune CrewQuotaScale[0].
- Is anyone buying night vision / the motion detector? If not, they're too expensive or too late.

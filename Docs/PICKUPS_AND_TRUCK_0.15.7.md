# 0.15.7.EEA — more pickable stock and commercial collection truck

**Truck model superseded:** the user rejected this release's vehicle appearance. The replacement
is documented in `Docs/TRUCK_REDO_0.15.7b.md`, including its corrected raised deck and refitted
loading geometry. The pickup additions and spawn settings documented below remain applicable.

The user requested more objects they can recover and a realistic-looking truck for depositing loot.
This release promotes 16 of the saved 0.15.6 original prop families to actual networked loot and adds
fitted pickup pockets across the saved mall. The truck receives original detailed commercial art
while keeping its established extraction mechanics and collision envelope.

## New recoverable items

| Item | Category | Gameplay kg | Value range | Carry class |
| --- | --- | ---: | ---: | --- |
| Receipt printer | Tech | 2.6 | $200–480 | One hand |
| Card payment terminal | Tech | 0.55 | $120–320 | Pocket |
| Boxed headphones | Tech | 0.65 | $180–480 | Pocket |
| Rack network switch | Tech | 3.2 | $650–1,500 | One hand |
| Showroom sofa | Furniture | 72 | $900–2,400 | Heavy, two carriers |
| Showroom floor lamp | Furniture | 5.5 | $180–520 | Two hands |
| Commercial office printer | Tech | 58 | $1,200–2,800 | Heavy, two carriers |
| Portable air compressor | Hardware | 52 | $1,300–2,800 | Heavy, two carriers |
| Welder and gas trolley | Hardware | 65 | $1,800–3,900 | Heavy, two carriers |
| Workshop dust vacuum | Hardware | 12 | $400–1,000 | Two hands |
| Footwear stock bundle | Material | 8 | $240–650 | Two hands |
| Cash register | Tech | 8 | $600–1,500 | Two hands |
| Extension cable reel | Hardware | 6 | $190–540 | One hand |
| Mobile planning board | Furniture | 16 | $250–650 | Two hands |
| Removed ventilation ducts | Material | 28 | $350–850 | Two hands |
| Recovery cart | Hardware | 22 | $450–1,100 | Two hands |

Each variant has an ordinary `LootDefinition`, registered NGO prefab, dynamic Rigidbody, existing
authoritative value/damage, owner-authoritative carrying and the existing focus outline. Logical
load remains separate from clamped physics mass. Heavy items use existing shared carrying. Sofas,
lamps, boards and carts use fitted compound box/capsule proxies; small solid stock uses simple boxes.
Carry and highlight sockets are generated in the loot-centred frame. No per-item runtime scripts
are added. Saved original art, materials and three LODs are reused; model budgets, PBR maps, wear
and item-specific concept prompts are in `Docs/ASSET_ADDITIONS_0.15.6.md`.

## More actual loot per run

The additional stock pass requests up to 110 ordinary spawn pockets across electronics, both furniture
floors, hardware, apparel, concourse staging, basement and remaining tenants. Construction fits
item-sized bounds against earlier solid fittings, supporting slabs, earlier loot pockets, stairs,
hauling lanes and all monster spawn reservations. Accepted pockets use explicit item IDs and facing.
Heavy additions are capped at ten overall and two per type. Existing markers remain.

Unity accepted all 110 requested pockets: 18 electronics, 12 lower and 14 upper furniture,
14 hardware, 12 apparel, 8 staging, 12 basement, 6 offices, 6 food court, 4 cinema and 4 toys.
The saved scene now has 200 total loot markers. These are eligible locations; the seeded fill
roll and contract modifiers determine the actual spawned count on each run.

Ordinary fill chance increases from 60% to 82%. The existing host seed planner decides occupancy and
value, and contract fill multipliers still apply. Featured/jackpot selection remains 1–2; no prize
count or quota change is introduced. F1's existing loot view reports actual item count and value.

## Truck and authoring

The worn off-white cab-over body uses rounded tyres, dual rear wheels, visible suspension/axles,
fuel and exhaust hardware, windshield seals, pillars, mirrors, wipers, seats, dashboard, grille and
front/rear lenses. The box has 25 pressed ribs on each side, interior tie-down rails, grip deck,
company markings and an overhead collision proxy. Body and each moving rear door have three saved
material-combined LODs; hinges remain nonstatic. The main body is approximately 7.9 m long, 2.83 m
wide and 2.84 m high, with tyres extending to about 3.24 m width. The cargo interior remains 5 × 2.6 m
on its established 0.4 m deck, retaining the saved haul ramp.

The original cargo trigger, cargo volume, loading ramp, haul screen, ignition, floodlight upgrade,
ride zone and front extraction route remain. New art is fitted around those systems. Real rear-door
hinges close over the final two departure seconds and reopen when departure is cancelled. Headlight
spots originate at the visible cab-front lenses and point forward. Original greybox fallback doors
remain available for older truck scenes without authored hinges.

**Tools > Abandoned > Mall > More Pickups and Collection Truck (0.15.7)** writes the content over the
saved scene, preserving its current mall density and stronger 0.15.6 threats. Batch entry:
`Abandoned.EditorTools.SalvageRelease0157.AuthorAndBuildPlayersAndExit`. The full mall builder retains
the new pickup pass and commercial truck art on future regeneration. Added markers do not alter
interior navigation; truck geometry remains excluded from the mall navigation bake.

Both players use `0.15.7.EEA` and one shared build stamp. The user's running game stays intact; changes
take effect on the next launch of the new build. No tests, gameplay, QA screenshots or verification
are run in this pass. The user reviews after construction.

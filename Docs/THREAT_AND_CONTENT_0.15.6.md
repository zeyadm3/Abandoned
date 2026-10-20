# 0.15.6.EEA — threat pressure and continued salvage construction

The user reported runs that felt like uncontested collecting: too few monsters and little danger
from those that appeared. This release increases early and sustained population and repairs
navigation and attack behavior that could leave creatures ineffective. The saved five-zone mall,
two-floor Nordic Habitat, extraction truck and prior density remain the base for additive content.

## Threat behavior

| Setting | LOW | MEDIUM | HIGH |
| --- | --- | --- | --- |
| Opening population | 2 | 3 | 4 |
| Normal opening delay | 14.4 s | 12 s | 9 s |
| Later arrival interval | 32.2–48.3 s | 28–42 s | 21–31.5 s |
| Solo alive cap | 4 | 5 | 6 |
| Four-player alive cap | 7 | 8 | 9 |

The company's first job opens at 20 seconds. Opening reinforcements stagger four seconds apart,
and the first creature is always harmful. Failed spawn placement retries in three seconds. Each new
arrival has 15 seconds of attack protection while it approaches. The roster limits Collector, Weight
and lethal Hunter to one each; Stalker and Thing to two each; Blind One and Crawlers to three each.
The final hunt remains separate. Crawlers are eligible from danger zero, Weight from one, and Thing
and Hunter from two; timed danger rises every 120 seconds. Thirty additional marker candidates extend
the original ten points across protected ground/upper lanes and basement service routes.

Host-authoritative patrols choose walkable destinations on their intended floor with complete
routes. Roaming creatures reuse throttled destination updates instead of asking for a fresh path
every frame. The Blind One does not mistake a failed route for arrival at a noise. Its hearing and
shared attack visibility ignore its own body collider, while walls still obstruct contact damage.

The Stalker follows its prey through doorways until it has sight, then holds its stalking distance.
Watching it and keeping the crew together remain useful. Hunter patrols retry invalid destinations;
Crawlers follow the authored bite timing. New arrivals have an authored warning before contact
damage or the Weight's structural strike. Creature songs, hierarchy and individual rules remain.

Spawns must be hidden, at least 14 metres from living players, unobstructed at the creature's feet,
and connected to a living player's navigation island. Candidate points are sampled on the intended
floor. No fair point means a short retry rather than a visible fallback or a whole arrival interval
of silence. Monsters begin arriving once the crew can be reached inside the mall.

## Added content

Eighteen original three-LOD prop families add recognisable silhouettes: receipt printers, card
terminals, headphone cartons, network switches, sofas, floor lamps, office printers, locker banks,
compressors, welding trolleys, workshop vacuums, shoe cartons, registers, barricades, extension reels,
mobile planning boards, salvaged ducts and recovery carts. These are fixed scene dressing rather
than new networked loot definitions. Their fitted placements use the saved routes, earlier collision
and new spawn reservations. Simple collision stays on bulk items; packing fragments remain visual.
See `Docs/ASSET_ADDITIONS_0.15.6.md` for budgets, wear, prefab conventions and concept prompts.

Four sawtooth factories, four vacant office buildings, four utility/silo works and two closed portable
clearance compounds extend the outer district. They are additive scenery outside the playable mall
and do not add extraction routes. Interior render meshes are saved in their own material/LOD batches
per supporting structural section so the existing collapse bindings remain meaningful. The earlier
0.15.5b batches keep their original asset paths. F1 shows the added content and threat pacing counters.
Unity saved 134 additional stock/equipment placements in 62 independently supported render batches
and all 14 outer buildings/compounds. The earlier 366 props and 1,165 packing details remain.

## Authoring and delivery

**Tools > Abandoned > Maps > Mall > Updates > 0.15.6 Finish Threat and Content Update** authors the update over the
saved scene. It writes threat tuning and coverage, adds the new content layer, authors navigation and
culling, and saves the scene. It replaces only its own additions on another invocation. The full
mall builder also retains the 0.15.6 additions on future regeneration.

Batch entry: `Abandoned.EditorTools.SalvageRelease0156.AuthorAndBuildPlayersAndExit`.
Both shareable players use version `0.15.6.EEA` and one build stamp. The update takes effect on the
next launch of the new player; a running game is not hot-patched or interrupted.

The user's standing instruction is to build first and leave all tests and verification to them.
No gameplay, EditMode/PlayMode, multiplayer tests, QA screenshots or optional artifact inspection
are part of this pass. Compile and content bakes are construction steps.

# Compact field HUD and interactions — 2026-10-18

The approved compact salvage/VHS reference is integrated through the existing runtime HUD components.
No scene or player-prefab rebuild is required. The final `FieldHud.uss` theme layer applies across locations.

## Layout

- Upper left: decorative REC, tape number and elapsed session/job time. These do not record anything.
- Upper centre: objective, extraction timer and directions-only compass. Departure replaces the timer with its countdown and aboard status.
- Upper right: the actual flashlight battery, labelled POWER.
- Left edge: four numbered pockets and the active selection.
- Lower left: small health and stamina bars with percentages.
- Lower centre: the physically carried object's name and shared carrier count, or a brief tool-switch label.
- Lower right: appraised truck cargo value and the player's current handling weight. The weight gauge uses the existing weight slowdown scale, not a new carrying limit.
- Small prompts follow the targeted object. Scan tags show name, weight, fragility and an estimate such as `150~`.

There is no threat meter or structural stress readout. Cracking floors, creaks and the world communicate structural danger. Dispatch and accessibility captions remain available and stack above the carried-item label.

## Controls and behaviour

Default bindings remain rebindable in Controls:

| Action | Default |
| --- | --- |
| Pick up / doors / drawers / ignition | E |
| Personal built-in scan | Q |
| Select pocket | Mouse wheel or 1–4 |
| Toggle inventory | Tab |
| Discard selected pocket while inventory is open | Right mouse |
| Gently set held loot onto a clear support | Right mouse |
| Deliberate throw | Hold left mouse, then release |
| Rotate a held object | Hold middle mouse / Left Alt and move the mouse |
| Peek while hiding | Hold middle mouse / Left Alt and move the mouse |
| Flashlight | F |
| Development overlay | F1 |

Tools and pocket-sized loot share four stable slots. Equipped pocket loot stays reserved in its slot while physically held. Switching pockets stows that loot and presents the next selection. Full pockets refuse another pocket item until the player discards something; no automatic swap occurs. Discarded tools have host-simulated world models that crew members can retrieve.

Scanning is personal and optional for pickup. Scanned items are remembered locally for inventory inspection. Estimates are derived from the loot definition and a stable item bias; true value becomes visible after the host appraises it aboard the truck. Appraisal state is replicated. Cargo includes the pockets of living players aboard without counting presented pocket loot twice.

Large loot retains the physical spring, sway and weight. Any player may drag Heavy loot alone; Huge loot needs a hand trolley in a pocket or a crew, and one person alone without one only nudges it (user, 2026-10-19; the hand trolley took over from the removed flatbed on 2026-10-22). Shared handling weight is divided across current carriers and transfers when a carrier leaves; insufficient carriers drag rather than lift. Structural load continues to count supported weight separately from handling effort.

Normal drops lower loot onto checked support. Throws require a minimum hold. Outside the building, lower-fragility loot uses a fresh 50-condition-point threshold for each impact; only the excess reduces value. High and Extreme fragility bypass that protection. Protection is never a cumulative pool. These settings live in CarryConfig and LootDamageConfig.

Bulky held objects must be set down before hiding. Small held objects and pocket contents may remain. Enclosed lockers/cabinets conceal players unless witnessed; open hiding places and peeking use sight checks. The Hunter remembers a player it sees peeking. Vents keep their existing concealment. DoorConfig contains peek reach and return speed.

One ignition press starts the default six-second departure countdown. Only the player who started it can cancel by pressing the ignition again, at any point before departure. Existing crew permissions still apply. Only living players inside the truck at departure extract.

## Verification and review

- Runtime, Editor and EditMode-test sources compile outside Unity with zero warnings/errors.
- Unity's compiler and Netcode post-processing completed successfully; the new stylesheet and text sheet imported.
- `python Tools/Text/check.py`: zero missing text; all new lines have nine translations.
- Regression cases cover four independent equipment slots, preserved pocket identity, per-impact protection, appraisal state and persistent extra-slot binding overrides. They were compiled, not executed.
- Gameplay, host/client play, visual layout and gamepad feel have not been tested. No player builds were made.

Review in Unity:

1. Let imports finish, then enter Play mode and host a game normally.
2. At the gear rack, fill all four pockets; start a job and inspect the compact HUD, scanning, pocket switching, gentle placement and deliberate throws.
3. With a second player, check shared carrying, peeking exposure, and starting/cancelling departure from the ignition. Confirm a player outside the truck is left behind.

Suggested commit message: `Integrate compact salvage HUD and four-pocket interactions`.

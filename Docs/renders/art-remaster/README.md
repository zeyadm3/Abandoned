# Art remaster review — 2026-10-22

Display-space sRGB saved-map captures with production fog, grading, exposure, selected shadows and local bounce.
Art review initializes fixture state, simulates authored ambient particles, and temporarily renders nearby reflection
probes. Flashlight/detail views use the gameplay flashlight's calibrated range, intensity, cone and cookie.
These are diagnostic art views; the runtime reflection queue and networked gameplay require an interactive session.

| Map | Arrival / spawn | Interior | Flashlight | Surface / prop detail |
| --- | --- | --- | --- | --- |
| HQ | [Office](HQ-outside.png) | — | — | — |
| Mall | [Arrival](Mall-outside.png) | [Shop](Mall-room-0.png), [second view](Mall-room-1.png) | [Flashlight](Mall-flashlight.png) | [Surface](Mall-surface-detail.png), [prop](Mall-prop-detail.png) |
| School | [Arrival](School-outside.png) | [Auditorium](School-room-0.png), [second view](School-room-1.png) | [Flashlight](School-flashlight.png) | [Surface](School-surface-detail.png), [upholstery](School-prop-detail.png) |
| Factory | [Arrival](Factory-outside.png) | [Works](Factory-room-0.png), [second view](Factory-room-1.png) | [Flashlight](Factory-flashlight.png) | [Surface](Factory-surface-detail.png), [prop](Factory-prop-detail.png) |
| Prison | [Arrival](Prison-outside.png) | [First view](Prison-room-0.png), [courtyard](Prison-room-1.png) | [Flashlight](Prison-flashlight.png) | [Surface](Prison-surface-detail.png) |
| Tutorial | [Arrival](Tutorial-outside.png) | [House](Tutorial-room-0.png), [second view](Tutorial-room-1.png) | [Flashlight](Tutorial-flashlight.png) | [Surface](Tutorial-surface-detail.png), [prop](Tutorial-prop-detail.png) |

[Authoring report](AuthoringReport.txt) · [Changes, commands and validation](../../ART_REMASTER.md).
The [earlier lighting captures](../lighting-after/README.md) predate this art pass and use startup reflection fallbacks,
so they provide a baseline rather than a strictly isolated material comparison.

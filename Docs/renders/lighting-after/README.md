# Lighting review — 2026-10-21

Saved-map review renders after the lighting rebuild. Internal shading is HDR; PNG output is sRGB.
These are diagnostic views with the saved lights, camera-based shadow selection and localized bounce. Reflection
probes retain their startup fallback in batch captures; their native realtime refresh needs an interactive run.

| Map | Spawn / arrival | View 1 | View 2 |
| --- | --- | --- | --- |
| HQ | [Office](HQ-outside.png) | — | — |
| Mall | [Arrival](Mall-outside.png) | [Interior](Mall-room-0.png) | [Interior](Mall-room-1.png) |
| School | [Arrival](School-outside.png) | [Auditorium](School-room-0.png) | [Auditorium](School-room-1.png) |
| Factory | [Arrival](Factory-outside.png) | [Works](Factory-room-0.png) | [Works](Factory-room-1.png) |
| Prison | [Arrival](Prison-outside.png) | [Island](Prison-room-0.png) | [Cellhouse](Prison-room-1.png) |
| Tutorial | [Arrival](Tutorial-outside.png) | [House](Tutorial-room-0.png) | [House](Tutorial-room-1.png) |

Implementation, commands and verification: [Lighting rebuild](../../LIGHTING_REBUILD.md).

# 0.15.7b.EEA — collection truck replacement

The user rejected the appearance of the 0.15.7 truck and requested a replacement. This pass rebuilds
the vehicle's silhouette and panel geometry around a coherent commercial cab-over truck. The scene
has been authored and saved through Unity, and Mac/Windows players have been built. No tests,
gameplay runs, screenshots or verification were performed; the user reviews the finished build.

## Vehicle model and materials

The cab uses thin formed sheets, a continuous curved nose, a crowned roof and real wheel-arch
openings. The sloped panoramic windscreen and inset side windows expose a hollow cab with seats,
dashboard and steering wheel. Door shut lines, rubber window gaskets, mirror arms, rounded bumper
and recessed lamp assemblies support the main shape instead of concealing it beneath stacked boxes.

New tyres and dished rims use continuous radial meshes with sensible sidewalls, wheel clearance and
commercial rear duals. The chassis, suspension, access steps and running gear fit the vehicle frame.
The cargo box uses continuous outer panels, restrained edge extrusions and interior loading hardware.
Body and moving-door meshes retain distance LODs and saved material batches.

Paint, glass, rubber, upholstery and metal finishes use restrained PBR values. Ordinary surface
colors remain within the normal base-color range; lens brightness belongs to emission and actual
lights. Off-white fleet paint has subtle grain and mild wear, with separate rubber, metal and plywood
maps. All vehicle geometry is original; no purchased models or packages are added.

## Corrected deck and loading geometry

The original cargo floor was at **0.40 m**, below the **0.96 m** tyre top. That made the truck's body
and chassis physically implausible. The replacement raises the cargo body by **0.65 m**, placing the
deck at **1.05 m** and the box roof at **3.40 m**. The cab retains its own lower roof and front-axle
frame, as on a commercial cab-over vehicle with a tall box behind it.

Cargo dimensions and capacity remain the same. The loading ramp is refitted for a **4.4 m run and
1.05 m rise**, and the cargo trigger, loading surfaces, haul display, ignition, lights, collision
proxies and rear-door hinges are relocated to match the new deck. Departure still uses the existing
authoritative cargo and payout systems; the rear hinges close on departure and reopen on cancellation.

## Rear view and retained content

A small rear-view monitor is mounted on the upper-right cargo bulkhead. Its camera sits above
the cargo box, with the view framed behind the vehicle so the taller box does not block it.

The additional 0.15.7 pickups, 200 total eligible loot locations, ordinary 82% fill setting, mall
density and 0.15.6 threat pressure remain. The replacement changes the truck and its fitted loading
geometry. It does not regenerate the retail layout or alter loot values, carrying rules or quotas.

The truck portion of `Docs/PICKUPS_AND_TRUCK_0.15.7.md` describes the superseded model. Its pickup
breakdown remains applicable. Both shareable players are saved as **0.15.7b.EEA** with one build stamp:
Mac **421 MB** and Windows **400 MB** before compression. This document records authored changes
and is not a gameplay or QA report.

Reauthoring menu: **Tools > Abandoned > Maps > Mall > Updates > 0.15.7b Replace Collection Truck**.
Batch entry: `Abandoned.EditorTools.CollectionTruckRedoRelease.AuthorAndBuildPlayersAndExit`.
The scene's occlusion data was authored again; interior navigation was retained because the truck
is excluded from the mall's navigation geometry.

# School revamp — 2026-10-20

Implementation only. The user explicitly requested **no testing, running, or verification**. No compiler,
Unity rebuild, navigation/occlusion bake, audit, render, play session or player build was invoked for this pass.
The changes are in the school authoring source; the saved scene still requires the rebuild below.

## Apply in Unity

1. Open the `Game` Unity project and allow the source imports to finish.
2. Use **Tools > Abandoned > Maps > School > Rebuild Jackson P Burley Middle School**.

That existing menu regenerates `Assets/_Project/Scenes/School.unity`, school materials and meshes,
the School navigation asset and the occlusion data. It also retains the existing truck, collection bay,
run plumbing, power controller, structural simulation and network scene stamping. Testing belongs to the user.

## Building and routes

- The front entry uses an open frame and propped door leaves instead of a solid frame block across the opening.
- The gym shell and floor now agree on the east boundary at x=100. The north library envelope is closed.
- Room partitions divide the entrance spine from the classrooms, clinic, administration, art and staff areas.
  Side portals make those rooms reachable from the spine as well as the main hall.
- The original academic stair keeps continuous ground underneath it. The upper opening has guards.
- A second academic stair rises from `(66, 0, 42)` to `(66, 4, 50)`, meeting a dedicated upper landing.
  Its upper opening, landing and adjacent route are protected structural surfaces.
- The boiler room has lower retaining walls, a ground-level service lobby/ceiling, a proper service stair
  opening and continuous basement floor. The terrain is cut out around the service basement.
- The central north/south route and hallway floors remain protected; side rooms retain weak floor sections.

## Rooms and halls

- Classrooms have supported desks and chairs, teaching details and different arrangements. The science lab
  has open work surfaces and a fume hood; the previously sparse upper north rooms become an art studio and staff records room.
- The library stacks leave a central carry route. Shelves have individual book groups and empty salvage bays.
- The cafeteria and commercial kitchen use furniture with open supports and recognisable fixed fittings.
- The clinic and administration have fitted furniture and usable salvage surfaces.
- The gym has split bleachers, trusses, wall padding, banners, circular court markings and strung hoops.
- The auditorium has bolted seating around aisles, folded curtains, fly bars, stage scenery and stage access.
  The static grand piano is removed so the piano salvage point can supply the actual item.
- Hall locker banks fit between classroom portals. Door jambs, open leaves, wainscot, baseboards, clocks,
  fountains, fire fittings, notice boards and corrected directions give the halls a consistent school identity.
- Four working hiding lockers use the existing hiding system on protected hall floors.
- The trophy salvage display has an open front and supported spawn positions; it no longer blocks the entrance spine.

## Wear, light and materials

- Acoustic ceilings and metal grids finish the academic wings. Ceiling panels and hanging lights beneath weak slabs follow those slabs' collapse state.
- Floor cracks and damp patches identify weak upper slabs. Window leaks, stage/kitchen water marks and exposed
  heating/steam services add abandonment without filling the carry routes with clutter.
- Floor-mounted upper furniture binds to its supporting collapsible section through the existing `SectionProp` component.
- Textured school cubes and cylinders receive metre-scale UVs. Brick, tile, timber, snow and glazing repeat at
  consistent sizes instead of stretching once across large walls and slabs.
- Corridor and room lighting has different intensities and fixture details; the auditorium has warm house/stage
  lighting, stairs have local pools of light, and gym fittings hang from visible supports.
- A small number of local lights cast shadows. Small one-time reflection probes cover the main interior zones.
- Snow falls downward in exterior strips with roof collision; steam rises. Both use a soft particle material.
- Interior fog density is reduced to retain room depth and make navigation landmarks clearer.

## Campus

- Parking and the outdoor court move east of the gym instead of overlapping it. Parking has an aisle;
  athletics uses actual running-track curves, basketball arcs, wire fences, supported bleachers and base paths.
- Continuous campus ground supports the approach. The snow island and flagpoles sit west of the direct truck-to-entry walk.
  The front bus and parking bus move out of the circulation route and gym respectively.
- Original terrain meshes replace the giant rectangular mountain blocks. Layered conifers and pitched house roofs
  replace the box-shaped foliage and roof stacks.
- The frozen pond is exposed through a cutout in the park snow. A low arched footbridge has supported decking and rails;
  the park has slatted benches and shore lamps. Buses and sedans gain round wheels, window pillars, cabin interiors,
  grilles, bumpers, mirrors, lights and weathering.

## Neighbourhood and campus (2026-10-24)

User: "rework the surroundings and level design of the school"; asked how far into the building, they chose
outside and campus (the building's inside is unchanged). Written, not compiled or rebuilt: run **Rebuild Jackson P
Burley Middle School**, then **Art > Map Sites: Sky, Trees, Grass and Sound** (it lays the land and woods round the
new, bigger ground) and **Art > Rebuild Lighting and Shadows**; Rebuild All Maps does all of it.

- **Layout fix**: the running track's oval used to run under Rose Hill Drive (its south curve reached z -39, the road
  is at z -18..-10). The track, the field and their terraces now stand 38 m further north
  (`BurleySchoolLayout.TrackShift`), clear of the road and its sidewalk; the baseball diamond and the park are untouched.
- **Streets** (`BurleySchoolTown`, station town kit): Rose Hill Drive across the front (x -70..270), Concord Avenue south
  off it, Henry Avenue up the west side to the north edge, Madison Avenue down the east side, all with 4 m sidewalks
  and kerbs, zebras at every junction, signals at Madison and stop signs at Henry and Concord. The drop-off loop meets
  Rose Hill across its whole frontage, so the truck, the crew spawns and the collection bay are exactly where they were.
- **Blocks** round the campus: houses in fenced yards, row houses, corner shops, a walk-up and churches facing every
  street (south of Rose Hill, west of Henry, east of Madison, Henry's east side north of the school), each block with a
  second row behind facing out. They replace the twelve box houses.
- **Street life** (`BurleySchoolTownStreet`): sodium lamps that still light, a power line down Rose Hill, a bus shelter,
  hydrants, bins and meters, cars left at the kerb, broken plow banks along every kerb (walk-over), and the city's
  snowplow stalled on Rose Hill with its blade down.
- **Two outer rings** (Docs/POLICE_STATION_PLAN.md "The outer rings"): a ring road, a block built back to back, a second
  ring road and an edge of houses, with Rose Hill, Concord, Henry and Madison running through to the new edge and a spur
  on the east (z 75..85) where the open world's road comes in. Residential mix (`StationRingPools.Suburb`); winter street
  trees (evergreens and snags). **Whole ground now x -192..392, z -192..352** (was x -70..270, z -70..230).
- **Snow**: the kit's materials are swapped as it builds (`StationRingStock.Remap`, `StationRingStock.Repaint`): flat and
  gabled roofs, lots and yards become the school's snow, roads its snowy asphalt, sidewalks its cleared concrete.
- **Campus level design** (`BurleySchoolCampus`): every outside door now has a cleared, lit walk back to the front
  loop: the auditorium's south door (x 12) by a walk to the loop; a covered walk along the west wing past a **bus lane**
  off Henry with three buses in their bays; a path north past **two portable classrooms** to a fenced **boiler service
  yard** that the boiler room's north door opens into (dumpsters, oil tank, transformer, pallets, a delivery van, a snow
  heap); from the gym's north door round its east side and along its front to the loop. A **service drive** runs from
  Henry through the yard to the staff lot, which had no way in. The old east service walk and the bus stop that stood
  in the new sidewalk are gone; pines that stood in the new walks and lane were moved.
- Off the NavMesh (all under `CampusSurroundings`), not occluders or occludees (`BurleySchoolRendering`); the town's
  building and car copies are not static-batched (`StationRings.Settle`).

### What to check first (this pass)
1. Rebuild, then from the truck look down Rose Hill both ways: houses on both sides, lamps lit, plow banks, no car or
   lamp in the loop's mouth, the truck and the collection bay unchanged.
2. Walk out of each outside door (auditorium south, gym north, boiler north) and follow the cleared walk back to the
   truck; nothing solid across it.
3. The track and field clear of Rose Hill, the baseball diamond and staff lot as they were, the service drive reaching
   the staff lot.
4. Walk or look out to the new edge along Henry and Madison: both ring roads, buildings both sides, snow on the roofs.
5. Watch the rebuild time, the scene's size and the frame rate looking out over the town (the School keeps a 5 km far
   clip and never culls its surroundings by occlusion).

## Source entry points

- `Editor/School/BurleySchoolBuilder.cs` — rebuild orchestration.
- `BurleySchoolLayout.cs`, `BurleySchoolStructure.cs`, `BurleySchoolLootAndThreats.cs` — boundaries, routes and salvage.
- `BurleySchoolBuilding.cs`, `BurleySchoolArchitecture.cs` — school shell and architectural geometry.
- `BurleySchoolInteriors.cs`, `BurleySchoolFurniture.cs`, `BurleySchoolRoomDetails.cs` — room authoring.
- `BurleySchoolCorridors.cs`, `BurleySchoolHallFittings.cs`, `BurleySchoolHallHiding.cs` — circulation and hiding.
- `BurleySchoolWear.cs`, `BurleySchoolInteriorSupport.cs`, `BurleySchoolSurfaces.cs` — wear, collapse attachment and UVs.
- `BurleySchoolLighting.cs`, `BurleySchoolWinterWeather.cs`, `BurleySchoolArt.cs` — light, weather and materials.
- `BurleySchoolAthletics.cs`, `BurleySchoolAthleticsGeometry.cs`, `BurleySchoolSurroundings.cs`,
  `BurleySchoolLandscape.cs`, `BurleySchoolCampusProps.cs` — campus and scenery.
- `BurleySchoolTown.cs`, `BurleySchoolTownStreet.cs`, `BurleySchoolCampus.cs` — the neighbourhood, its outer rings and the
  campus grounds (2026-10-24), on the station kit in `Editor/PoliceStation/`.

All builder files listed above live in `Game/Assets/_Project/Editor/School/`. No new packages are required.

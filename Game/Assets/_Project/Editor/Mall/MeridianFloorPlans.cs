using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    // Read the generated section/loot authoring so review plans cannot drift from the actual rebuild.
    public static class MeridianFloorPlans
    {
        private const float Scale = 9f;
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        private static float X(float x) => (x + 12) * Scale;
        private static float Y(float z) => 70 + (62 - z) * Scale;
        private static string N(float value) => value.ToString("0.##", Invariant);
        private static string Escape(string text) => System.Security.SecurityElement.Escape(text) ?? "";

        public static void Write()
        {
            Transform mall = GameObject.Find("Mall")?.transform;
            if (mall == null || mall.Find("Tiles") == null)
                throw new InvalidOperationException("Open the generated Mall scene before exporting its floor plans.");
            string directory = Path.Combine(MeridianArtRenders.Folder, "floorplans");
            Directory.CreateDirectory(directory);
            for (int floor = -1; floor < Floors; floor++)
            {
                string name = floor < 0 ? "basement" : floor == 0 ? "ground" : $"level-{floor}";
                WriteLevel(mall, floor, Path.Combine(directory, name));
            }
            Debug.Log($"[Meridian] Wrote four actual-layout SVG and ASCII floor plans to {directory}.");
        }

        private static void WriteLevel(Transform mall, int floor, string path)
        {
            var zones = new List<Zone>();
            for (int z = 0; z < TilesZ; z++) for (int x = 0; x < TilesX; x++)
            {
                Zone? zone = ZoneAt(new Vector2Int(x, z), floor);
                if (zone.HasValue && zones.All(a => a.Name != zone.Value.Name)) zones.Add(zone.Value);
            }
            var svg = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1240\" height=\"900\" viewBox=\"0 0 1240 900\">\n");
            svg.Append("<rect width=\"1240\" height=\"900\" fill=\"#101718\"/><defs><pattern id=\"wound\" width=\"8\" height=\"8\" patternUnits=\"userSpaceOnUse\"><path d=\"M0 8L8 0\" stroke=\"#c67854\" stroke-width=\"2\"/></pattern></defs>\n");
            Text(svg, 25, 30, $"MERIDIAN MALL / {(floor < 0 ? "SERVICE BASEMENT" : floor == 0 ? "GROUND" : $"LEVEL {floor}")} / y = {N(FloorY(floor))} m", "#ffffff", 20);
            Text(svg, 25, 53, "Generated from owned cells and authored scene components. North / +Z is up. Grid: 4 m.", "#bcc5c0", 12);
            var ascii = new StringBuilder($"MERIDIAN MALL / floor {floor} / y={N(FloorY(floor))} m\nNorth (+Z) is up. Cells are 4 x 4 m.\n\n");
            ascii.Append(" z\\x ");
            for (int x = 0; x < TilesX; x++) ascii.Append($"{x,3}");
            ascii.AppendLine();
            for (int z = TilesZ - 1; z >= 0; z--)
            {
                ascii.Append($"{z,4} ");
                for (int x = 0; x < TilesX; x++)
                {
                    var cell = new Vector2Int(x, z);
                    Zone? zone = ZoneAt(cell, floor);
                    bool wound = !zone.HasValue && floor == 2 && ZonesByFloor[2].Any(a => a.Tag == "gallery" && a.Contains(cell)) && InFootprint(cell);
                    StructuralSection section = mall.Find($"Tiles/Floor_{floor}/{MallBuilder.TileName(cell, floor)}")?.GetComponent<StructuralSection>();
                    bool permanent = section != null && !section.CanCollapse;
                    bool weak = IsWeak(section);
                    string code = zone.HasValue ? ((char)('A' + zones.FindIndex(a => a.Name == zone.Value.Name))).ToString() : "-";
                    string state = !zone.HasValue ? wound ? "X" : "-" : weak ? "!" : permanent ? "P" : "~";
                    ascii.Append($" {code}{state}");
                    float px = X(x * Tile), py = Y((z + 1) * Tile), size = Tile * Scale;
                    string fill = zone.HasValue ? Colour(zone.Value.Tag) : wound ? "url(#wound)" : "#080c0d";
                    svg.Append($"<rect x=\"{N(px)}\" y=\"{N(py)}\" width=\"{N(size)}\" height=\"{N(size)}\" fill=\"{fill}\" stroke=\"{(weak ? "#ffbb59" : permanent ? "#83d8a1" : "#495250")}\" stroke-width=\"{(weak || permanent ? "2" : "1")}\"/>\n");
                    if (zone.HasValue) Text(svg, px + 11, py + 23, code + (weak ? "!" : permanent ? "P" : ""), "#ffffff", 10);
                }
                ascii.AppendLine();
            }
            ascii.AppendLine("\nCell key: A.. = owned zone; P = permanent slab; ! = authored weak slab; ~ = collapsible slab; -- = void/outside; -X = pre-collapsed gallery wound.");
            Text(svg, 900, 90, "OWNED ZONES / LOOT TAGS", "#ffffff", 15);
            int line = 117;
            for (int i = 0; i < zones.Count; i++)
            {
                Zone zone = zones[i];
                string label = $"{(char)('A' + i)}  {zone.DisplayName}";
                Text(svg, 900, line, label, Colour(zone.Tag), 13);
                Text(svg, 922, line + 16, $"{zone.Tag} / {zone.Kind}", "#bdc7c1", 11);
                line += 39;
                ascii.AppendLine($"{(char)('A' + i)} = {zone.DisplayName} [{zone.Name}; {zone.Tag}; {zone.Kind}]");
            }
            DrawRoutes(svg, ascii, floor);
            DrawPrizes(svg, ascii, mall, floor);
            DrawExits(svg, ascii, floor);
            if (floor == 0) DrawArrival(svg, ascii, mall);
            Text(svg, 900, line + 15, "P  permanent walking slab", "#83d8a1", 12);
            Text(svg, 900, line + 36, "!  weak slab / amber outline", "#ffbb59", 12);
            Text(svg, 900, line + 57, "Hatched: gallery wound / void", "#c67854", 12);
            Text(svg, 900, line + 78, "E escalator / S stair / R haul ramp", "#a9d1ef", 12);
            Text(svg, 900, line + 99, "J true Jackpot / H Heavy sofa site", "#ffdf70", 12);
            Text(svg, 900, line + 120, "One or two featured sites per run;", "#bdc7c1", 12);
            Text(svg, 900, line + 138, "at least one selected site is J.", "#bdc7c1", 12);
            Text(svg, 900, line + 164, "RW permanent one-way rope window", "#7be8e8", 12);
            Text(svg, 25, 852, "Protected corridors and the service core form the permanent evacuation / haul route. Markers describe source authoring, before run pre-damage.", "#bdc7c1", 12);
            Text(svg, 25, 875, "Decorative geometry, runtime shutters and loose scatter are omitted for readability; permanent doors are shown as white ticks.", "#bdc7c1", 12);
            svg.AppendLine("</svg>");
            File.WriteAllText(path + ".svg", svg.ToString());
            File.WriteAllText(path + ".txt", ascii.ToString());
        }

        private static bool IsWeak(StructuralSection section)
        {
            if (section == null) return false;
            var serialized = new SerializedObject(section);
            return serialized.FindProperty("initialHealth").floatValue < .999f || serialized.FindProperty("capacityMultiplier").floatValue < .999f;
        }

        private static void DrawRoutes(StringBuilder svg, StringBuilder ascii, int floor)
        {
            ascii.AppendLine("\nFLIGHTS (world x,y,z; rise; clear width; collapse flag):");
            foreach (Flight flight in AllFlights)
            {
                Vector3 head = flight.Foot + new Vector3(flight.Dir.x * flight.Length, flight.Rise, flight.Dir.y * flight.Length);
                string code = flight.Kind == FlightKind.Escalator ? "E" : flight.Kind == FlightKind.Stair ? "S" : "R";
                if (flight.Floor == floor)
                {
                    string colour = flight.CanCollapse ? "#ffbb59" : "#9bd3ff";
                    svg.Append($"<line x1=\"{N(X(flight.Foot.x))}\" y1=\"{N(Y(flight.Foot.z))}\" x2=\"{N(X(head.x))}\" y2=\"{N(Y(head.z))}\" stroke=\"{colour}\" stroke-width=\"4\"/>\n");
                    Marker(svg, (flight.Foot + head) * .5f, code + "↑", colour);
                    Marker(svg, head, "▲", colour);
                    ascii.AppendLine($"{code} {flight.Name}: {Position(flight.Foot)} -> {Position(head)}; rise {N(flight.Rise)}m; width {N(flight.Width)}m; collapsible={flight.CanCollapse}");
                }
                else if (Mathf.Abs(head.y - FloorY(floor)) < .05f)
                {
                    Marker(svg, head, code + "↓", "#a9d1ef");
                    ascii.AppendLine($"{code} {flight.Name}: descending head at {Position(head)} to floor {flight.Floor}.");
                }
            }
            foreach (Door door in Doors.Where(d => d.Floor == floor))
            {
                Vector3 a = TileTopCenter(door.A, floor), b = TileTopCenter(door.B, floor), midpoint = (a + b) * .5f;
                Vector3 cross = Vector3.Cross((b - a).normalized, Vector3.up) * (door.Wide ? 1.3f : .6f);
                Line(svg, midpoint - cross, midpoint + cross, "#ffffff", 3);
            }
        }

        private static void DrawPrizes(StringBuilder svg, StringBuilder ascii, Transform mall, int floor)
        {
            ascii.AppendLine("\nFEATURED CANDIDATES (not all appear in one run):");
            foreach (LootSpawnPoint point in mall.GetComponentsInChildren<LootSpawnPoint>(true).Where(p => p.IsFeatured || p.IsJackpot))
            {
                if (Mathf.RoundToInt(point.transform.position.y / StoryHeight) != floor) continue;
                string code = point.IsJackpot ? "J" : "H";
                Marker(svg, point.transform.position, code, "#ffdf70");
                var ids = new SerializedObject(point).FindProperty("allowedDefinitionIds");
                string items = string.Join(",", Enumerable.Range(0, ids.arraySize).Select(i => ids.GetArrayElementAtIndex(i).stringValue));
                ascii.AppendLine($"{code}: {items}; tag={point.Tag}; class={point.MinClass}..{point.MaxClass}; position={Position(point.transform.position)}; Jackpot={point.IsJackpot}.");
            }
        }

        private static void DrawExits(StringBuilder svg, StringBuilder ascii, int floor)
        {
            ascii.AppendLine("\nEXTERIOR OPENINGS:");
            foreach (var exit in Exterior.Where(e => e.floor == floor))
            {
                Vector3 at = ExteriorPosition(exit);
                Marker(svg, at, exit.window ? "RW" : exit.side == 'E' ? "DOCK" : "EXIT", "#7be8e8");
                ascii.AppendLine($"{exit.side}{exit.column}: {Position(at)}; {(exit.window ? "permanent one-way rope descent to ground" : "exterior door")}; wide={exit.wide}.");
            }
        }

        private static void DrawArrival(StringBuilder svg, StringBuilder ascii, Transform mall)
        {
            Line(svg, LoadingDoor, TruckRear, "#7be8e8", 3);
            Marker(svg, TruckRear, "TRUCK REAR", "#ffffff");
            Marker(svg, LoadingDoor, "LOADING", "#7be8e8");
            Marker(svg, ArrivalVehicle, "CREW VAN", "#e2f8ba");
            ascii.AppendLine($"\nTRUCK rear={Position(TruckRear)}; loading door={Position(LoadingDoor)}; dock route is +X.");
            ascii.AppendLine($"CREW ARRIVAL VAN={Position(ArrivalVehicle)}; parking aisle connects to the east collection drive.");
            Transform spawns = mall.Find("Spawns");
            if (spawns == null) return;
            foreach (Transform spawn in spawns)
            {
                Marker(svg, spawn.position, "P", "#e2f8ba");
                Line(svg, spawn.position, spawn.position + spawn.forward * 2, "#e2f8ba", 2);
                ascii.AppendLine($"{spawn.name}: {Position(spawn.position)}; faces {Position(spawn.forward)}.");
            }
        }

        private static void Line(StringBuilder svg, Vector3 a, Vector3 b, string colour, int width) =>
            svg.Append($"<line x1=\"{N(X(a.x))}\" y1=\"{N(Y(a.z))}\" x2=\"{N(X(b.x))}\" y2=\"{N(Y(b.z))}\" stroke=\"{colour}\" stroke-width=\"{width}\"/>\n");

        private static void Marker(StringBuilder svg, Vector3 position, string label, string colour)
        {
            svg.Append($"<circle cx=\"{N(X(position.x))}\" cy=\"{N(Y(position.z))}\" r=\"8\" fill=\"#101718\" stroke=\"{colour}\" stroke-width=\"2\"/>\n");
            Text(svg, X(position.x) + 10, Y(position.z) - 7, label, colour, 11);
        }

        private static void Text(StringBuilder svg, float x, float y, string text, string colour, int size) =>
            svg.Append($"<text x=\"{N(x)}\" y=\"{N(y)}\" fill=\"{colour}\" font-family=\"Arial,sans-serif\" font-size=\"{size}\">{Escape(text)}</text>\n");

        private static string Position(Vector3 p) => $"({N(p.x)},{N(p.y)},{N(p.z)})";
        private static string Colour(string tag) => tag switch
        {
            "stock" => "#426452", "concourse" => "#566770", "walkway" => "#596a77", "electronics" => "#4d637d",
            "clothing" => "#785867", "food" => "#80714a", "jewelry" => "#7d7554", "office" => "#5b705f",
            "furniture" => "#805f48", "toys" => "#67608a", "cinema" => "#74596f", "gallery" => "#88755e", _ => "#536966"
        };
    }
}

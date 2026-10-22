using System.Collections.Generic;
using Abandoned.Interaction;
using Abandoned.Structure;
using UnityEngine;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Everything a station's dressing code works with while the scene is being built: the plan and art, the props and
    /// hiding-place builders (which keep doorways and flights clear), the slabs' sections, the structure setup (for
    /// extra sections such as a tower's platforms), and the loot and monster points it collects for the plumbing.
    /// </summary>
    public sealed class StationContext
    {
        public readonly Transform Root;
        public readonly StationPlan Plan;
        public readonly StationArt Art;
        public readonly StationProps Props;
        public readonly StationHiding Hiding;
        public readonly StructureSceneSetup Setup;
        public readonly Dictionary<Vector3Int, StructuralSection> Sections;
        public readonly List<LootPointSpec> Loot = new();
        public readonly List<Vector3> Threats = new();
        public readonly List<string> Problems = new();
        public int Breakers;
        private readonly string lootPrefix;

        public StationContext(Transform root, StationPlan plan, StationArt art, StructureSceneSetup setup,
            Dictionary<Vector3Int, StructuralSection> sections, string lootPrefix)
        {
            Root = root;
            Plan = plan;
            Art = art;
            Setup = setup;
            Sections = sections;
            this.lootPrefix = lootPrefix;
            Props = new StationProps(root, plan, art);
            Hiding = new StationHiding(Props, SectionAt);
        }

        /// <summary>The slab under a point (its storey from the height), or null.</summary>
        public StructuralSection SectionAt(Vector3 p)
        {
            int floor = Mathf.FloorToInt((p.y + .5f) / P.Story);
            var key = new Vector3Int(Mathf.FloorToInt(p.x / P.Tile), floor, Mathf.FloorToInt(p.z / P.Tile));
            return Sections.TryGetValue(key, out StructuralSection s) ? s : null;
        }

        public void AddLoot(string tag, Vector3 at, CarryClass min, CarryClass max, bool jackpot = false) =>
            Loot.Add(new LootPointSpec($"{lootPrefix}{Loot.Count:000}", tag, at, min, max, jackpot));

        /// <summary>Loot points on a loose grid over a room (kept 1.3 m off its walls), on the room's own tag.</summary>
        public int RoomLoot(P.Room room, CarryClass max, float spacing = 4f, CarryClass min = CarryClass.Pocket, string tag = null)
        {
            Rect r = room.Meters;
            int n = 0;
            for (float x = r.xMin + 1.3f + ((r.width - 2.6f) % spacing) * .5f; x <= r.xMax - 1.3f + .01f; x += spacing)
            for (float z = r.yMin + 1.3f + ((r.height - 2.6f) % spacing) * .5f; z <= r.yMax - 1.3f + .01f; z += spacing)
            {
                AddLoot(tag ?? room.LootTag, new Vector3(x, room.Y, z), min, max);
                n++;
            }
            return n;
        }

        public void Threat(Vector3 at) => Threats.Add(at);
        public void Threat(P.Room room, float u = .5f, float v = .5f) => Threats.Add(StationProps.In(room, u, v));
    }
}

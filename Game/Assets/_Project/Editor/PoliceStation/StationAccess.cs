using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using P = Abandoned.EditorTools.StationPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Fixed ladders (<see cref="Abandoned.Core.Ladder"/>: the factory's rules, players only, hands empty, never on the NavMesh) for a
    /// station: the roof hatch's ladder up from the top floor's hall, and any other ladder a recipe needs.
    /// </summary>
    public static class StationAccess
    {
        public const string GroupName = "Ladders";

        /// <summary>
        /// A ladder whose foot is at <paramref name="foot"/> on the edge of what it reaches; <paramref name="facing"/>
        /// points out toward the climber (who climbs facing the other way and steps off that way at the top).
        /// </summary>
        public static Transform Fixed(Transform parent, string name, Vector3 foot, Vector3 facing, float rise, StationArt art)
        {
            Transform ladder = Group(name, parent);
            ladder.SetPositionAndRotation(foot, Quaternion.LookRotation(facing, Vector3.up));
            float stile = rise + 1.1f;
            foreach (float x in new[] { -.25f, .25f })
                Box(x < 0 ? "Stile_L" : "Stile_R", ladder, new Vector3(x, stile * .5f, .06f), new Vector3(.06f, stile, .06f), art.Safety, false);
            for (float y = .3f; y < rise; y += .3f)
                Box("Rung", ladder, new Vector3(0f, y, .06f), new Vector3(.5f, .04f, .04f), art.Steel, false);
            ladder.gameObject.AddComponent<Abandoned.Core.Ladder>().EditorSetup(rise);
            return ladder;
        }

        /// <summary>
        /// The ladder up through a roof hatch (<see cref="StationPlan.RoofHatches"/>) from the top floor: it stands at the
        /// hatch's south edge facing north, so you climb facing south and step out onto the roof south of the hatch.
        /// </summary>
        public static Transform RoofLadder(StationContext ctx, Vector2Int cell, int fromFloor)
        {
            Transform group = ctx.Root.Find(GroupName);
            if (group == null) group = Group(GroupName, ctx.Root);
            Vector3 c = P.CellCentre(cell, fromFloor);
            float rise = ctx.Plan.RoofY + P.Slab - c.y;
            Transform ladder = Fixed(group, $"RoofLadder_{cell.x}_{cell.y}", new Vector3(c.x, c.y, c.z - P.HatchSize * .5f), Vector3.forward, rise, ctx.Art);
            // Nothing stands under the hatch.
            ctx.Props.KeepOut(new Bounds(new Vector3(c.x, c.y + 1.2f, c.z), new Vector3(P.HatchSize + 1.4f, 2.4f, P.HatchSize + 1.8f)));
            return ladder;
        }
    }
}

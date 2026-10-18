using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Plan = Abandoned.EditorTools.IronFactoryCatwalkPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The ways between tiers: steel stairs (haul-friendly, monsters use them through a NavMesh link) and fixed
    /// ladders (fast, hands empty, players only). Stairs are structural sections that never fall, like the
    /// mall's flights; ladders are plain scene objects read by the player motor.
    /// </summary>
    public static class IronFactoryAccess
    {
        private const float FlightLength = 8f;
        private const float FlightWidth = 3f;
        private const int Steps = 18;
        private const float RungSpacing = .3f;

        public static List<Transform> BuildStairs(Transform root, IronFactoryArt art)
        {
            Transform flights = Group("Flights", root);
            var built = new List<Transform>();
            foreach (Plan.Stair stair in Plan.Stairs) built.Add(Stair(flights, stair, art));
            return built;
        }

        private static Transform Stair(Transform parent, Plan.Stair stair, IronFactoryArt art)
        {
            Vector3 dir = new(stair.Direction.x, 0f, stair.Direction.y);
            float low = Plan.Height(stair.FromLevel), rise = Plan.Height(stair.FromLevel + 1) - low;
            Transform root = Group(stair.Name, parent);
            root.position = L.CellCentre(stair.Foot, low) + dir * (L.Tile * .5f);
            root.rotation = Quaternion.LookRotation(dir, Vector3.up);

            float angle = Mathf.Atan2(rise, FlightLength) * Mathf.Rad2Deg;
            float slope = Mathf.Sqrt(FlightLength * FlightLength + rise * rise);
            Transform ramp = Group("Ramp", root);
            ramp.localPosition = new Vector3(0f, rise * .5f, FlightLength * .5f);
            ramp.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            ramp.gameObject.AddComponent<BoxCollider>().size = new Vector3(FlightWidth, .25f, slope);

            Transform visual = Group("Visual", root);
            for (int i = 0; i < Steps; i++)
            {
                float t = (i + .5f) / Steps;
                Box($"Tread_{i:00}", visual, new Vector3(0f, rise * t - .06f, FlightLength * t),
                    new Vector3(FlightWidth, .06f, FlightLength / Steps * .9f), art.Grating, false);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                GameObject stringer = Box(side < 0 ? "Stringer_L" : "Stringer_R", visual,
                    new Vector3(side * (FlightWidth * .5f + .05f), rise * .5f - .15f, FlightLength * .5f),
                    new Vector3(.1f, .3f, slope), art.Safety, false);
                stringer.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
                GameObject rail = Box(side < 0 ? "Rail_L" : "Rail_R", visual,
                    new Vector3(side * (FlightWidth * .5f + .08f), rise * .5f + .55f, FlightLength * .5f),
                    new Vector3(.12f, 1.1f, slope), art.Steel);
                rail.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            }
            // Ties both landings together for monsters; the bake can leave a hairline gap at a flight's ends.
            var link = root.gameObject.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.back * .75f;
            link.endPoint = Vector3.forward * (FlightLength + .75f) + Vector3.up * rise;
            link.width = FlightWidth - .35f;
            link.bidirectional = true;
            root.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            return root;
        }

        public static void AddSections(StructureSceneSetup setup, IEnumerable<Transform> stairs)
        {
            foreach (Transform stair in stairs) setup.AddSection(stair, SectionType.Stair, false, fractured: false);
        }

        public static void BuildLadders(Transform root, IronFactoryArt art)
        {
            Transform ladders = Group("Ladders", root);
            foreach (Plan.LadderSpot spot in Plan.Ladders)
            {
                Vector3 side = new(spot.Side.x, 0f, spot.Side.y);
                float low = Plan.Height(spot.Level - 1), rise = Plan.Height(spot.Level) - low;
                Transform ladder = Group($"Ladder_{spot.Level}_{spot.Cell.x}_{spot.Cell.y}", ladders);
                Vector3 edge = L.CellCentre(spot.Cell, low) + side * (L.Tile * .5f);
                ladder.SetPositionAndRotation(edge, Quaternion.LookRotation(side, Vector3.up));
                // Stiles run on past the top as handholds for stepping over onto the walkway.
                float stile = rise + 1.1f;
                foreach (float x in new[] { -.25f, .25f })
                    Box(x < 0 ? "Stile_L" : "Stile_R", ladder, new Vector3(x, stile * .5f, .06f), new Vector3(.06f, stile, .06f), art.Safety, false);
                for (float y = RungSpacing; y < rise; y += RungSpacing)
                    Box($"Rung_{y:0.0}", ladder, new Vector3(0f, y, .06f), new Vector3(.5f, .04f, .04f), art.Steel, false);
                ladder.gameObject.AddComponent<Ladder>().EditorSetup(rise);
            }
            MarkStatic(ladders);
        }
    }
}

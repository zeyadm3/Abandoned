using Abandoned.EditorTools;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalShell
    {
        public static void Build(Transform root, HospitalKit kit, HospitalStructure floors)
        {
            StationContext context = floors.Context;
            // The station shell merges adjacent walls and marks only permanent architecture as occluders.
            StationWalls.BuildInterior(root, context.Plan, context.Art);
            StationFacade.Build(root, context.Plan, context.Art);
            Transform bay = HospitalKit.Group("AmbulanceCanopy", root);
            kit.Box("Canopy", bay, new Vector3(44f, 3.8f, - 4f), new Vector3(14f, .24f, 8f), kit.Accent);
            foreach (float x in new []
            {
                37.3f, 50.7f
            })
            {
                kit.Box("Column", bay, new Vector3(x, 1.85f, - 7.5f), new Vector3(.25f, 3.7f, .25f), kit.Metal);
            }
            // A cross is readable at a distance without another notice board in the carry lane.
            kit.Box("CrossVertical", bay, new Vector3(44f, 3.35f, - 7.96f), new Vector3(.2f, .8f, .04f), kit.Glow, false);
            kit.Box("CrossHorizontal", bay, new Vector3(44f, 3.35f, - 7.99f), new Vector3(.7f, .2f, .04f), kit.Glow, false);
            GreyboxFactory.MarkStatic(bay);
        }
    }
}

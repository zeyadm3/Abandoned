using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEngine;
namespace Abandoned.Hospital
{
    public sealed class HospitalStructure
    {
        private readonly StructureSceneSetup setup;
        public StationContext Context
        {
            get;
            private set;
        }
        public HospitalStructure(HospitalKit kit, StructureSceneSetup sections)
        {
            setup = sections;
        }
        public void Build(Transform root)
        {
            StationPlan plan = HospitalLayout.Create();
            var art = new StationArt(HospitalKit.Art, "Hospital", new Color(.72f, .7f, .61f), new Color(.16f, .34f, .29f), new Color(.8f, .94f, .87f));
            StationFloors.Result floors = StationFloors.Build(root, plan, art);
            var sections = StationFloors.AddSections(setup, plan, floors);
            Context = new StationContext(root, plan, art, setup, sections, "hospital_loot_");
            foreach (Transform flight in floors.Stairs)
            {
                flight.gameObject.AddComponent<SectionKeepClear>();
            }
        }
        public StructuralSection At(Vector3 point) => Context.SectionAt(point);
        public void BindDressing()
        {
            foreach (Transform room in Context.Props.Root)
            {
                foreach (Transform piece in room)
                {
                    if (piece.GetComponentInChildren<Abandoned.Extraction.HidingLocker>() != null)
                    {
                        continue;
                    }
                    StructuralSection section = At(piece.position);
                    if (section == null || ! section.CanCollapse)
                    {
                        continue;
                    }
                    var prop = piece.GetComponent<SectionProp>() ?? piece.gameObject.AddComponent<SectionProp>();
                    prop.EditorSetup(section);
                }
            }
        }
    }
}

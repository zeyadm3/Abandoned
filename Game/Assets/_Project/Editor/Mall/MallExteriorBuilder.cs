using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    public static class MallExteriorBuilder
    {
        public static void Build(Transform mall)
        {
            Transform exterior = Group("Exterior", mall);
            Transform slabs = Group("GroundSlabs", exterior);
            Transform parking = Group("Parking", exterior);
            Material stone = MeridianRetailMaterials.Stone;
            MeridianArrivalCourt.Build(parking);
            // These closed perimeter walks are scenery and circulation outside the solid mall envelope.
            Box("WestPerimeterWalk", slabs, new Vector3(-7f, -.2f, 24f), new Vector3(14f, .4f, 48f), stone);
            Box("EastPerimeterWalk", slabs, new Vector3(69f, -.2f, 24f), new Vector3(26f, .4f, 48f), stone);
            Box("RearPerimeterWalk", slabs, new Vector3(34f, -.2f, 55f), new Vector3(96f, .4f, 14f), stone);
            Box("DockApron", slabs, new Vector3(MallLayout.TruckRear.x, -.032f, MallLayout.TruckRear.z + 2.5f),
                new Vector3(MeridianCollectionBayBuilder.ApproachWidth + .6f, .08f, 3f), stone);
            foreach (Transform item in exterior.GetComponentsInChildren<Transform>())
                if (item.GetComponent<Collider>() != null && item.GetComponent<SurfaceTag>() == null)
                    item.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
            MarkStatic(exterior);
        }

        // Keep the hierarchy API, with no ledges, ropes, open windows or extra escape routes.
        public static void Exits(Transform mall) => Group("Exits", mall);
    }
}

using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The yard around the plant and the collection truck at the rail dock: the same truck, ramp and cargo
    /// bay as every other location, backed up to the plant's one opening. The rail yard, slag heaps and
    /// smokestacks come in step 7; until then the yard is plain ground so no view looks into the void.
    /// </summary>
    public static class IronFactoryExterior
    {
        private const float Margin = 24f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform exterior = Group("Exterior", root);
            Transform ground = Group("GroundSlabs", exterior);
            Vector3 size = L.SizeMeters;
            const float y = -.22f, thick = .4f;
            // Four slabs around the footprint, never under it: one under the building would roof the channel.
            Box("DockLot", ground, new Vector3(size.x * .5f, y, -Margin * .5f), new Vector3(size.x + 2f * Margin, thick, Margin), art.Asphalt);
            Box("NorthYard", ground, new Vector3(size.x * .5f, y, size.z + Margin * .5f), new Vector3(size.x + 2f * Margin, thick, Margin), art.Dirt);
            Box("WestYard", ground, new Vector3(-Margin * .5f, y, size.z * .5f), new Vector3(Margin, thick, size.z), art.Dirt);
            Box("EastYard", ground, new Vector3(size.x + Margin * .5f, y, size.z * .5f), new Vector3(Margin, thick, size.z), art.Dirt);
            // The truck's cargo trigger is its own volume; the apron makes the shared approach readable.
            GameObject apron = Box("DockApron", ground, new Vector3(L.DockX, -.16f, -4.5f),
                new Vector3(MeridianCollectionBayBuilder.ApproachWidth + .6f, .12f, 4.4f), art.Concrete);
            apron.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            ground.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);

            TruckCargo cargo = TruckBuilder.Build(Group("Truck", root), L.TruckRear, Vector3.back);
            MeridianCollectionBayBuilder.Build(root, cargo);
            MarkStatic(exterior);
        }
    }
}

using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.BurleySchoolLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Readable structural wear and fixed services, with ceiling panels owned by their slabs.</summary>
    public static class BurleySchoolWear
    {
        public static void Build(Transform root, BurleySchoolArt art, BurleySchoolStructure.Result structure)
        {
            Transform wear = Group("SchoolWear", root);
            Transform ceilings = Group("SuspendedCeilings", wear);
            Transform damage = Group("WaterDamage", wear);
            foreach (BurleySchoolStructure.TilePiece tile in structure.Pieces)
            {
                if (tile.Floor != 1) continue;
                Vector3 at = tile.Transform.position;
                if (tile.Transform.name == "EastStairLanding") continue;
                Transform panel = Group($"Ceiling_{tile.Cell.x}_{tile.Cell.y}", ceilings);
                panel.position = at - Vector3.up * .29f;
                Box("AcousticPanel", panel, Vector3.zero, new Vector3(3.94f, .045f, 3.94f), art.AcousticCeiling, false);
                Box("Grid_NS", panel, new Vector3(-1.98f, -.025f, 0f), new Vector3(.025f, .025f, 4f), art.MetalSteel, false);
                Box("Grid_EW", panel, new Vector3(0f, -.025f, -1.98f), new Vector3(4f, .025f, .025f), art.MetalSteel, false);
                if (tile.Collapsible)
                {
                    StructuralSection section = tile.Transform.GetComponent<StructuralSection>();
                    panel.gameObject.AddComponent<SectionProp>().EditorSetup(section);
                    Transform cracks = Group($"WeakSlab_{tile.Cell.x}_{tile.Cell.y}", damage);
                    cracks.position = at + Vector3.up * .008f;
                    cracks.gameObject.AddComponent<SectionProp>().EditorSetup(section);
                    Stain(cracks, "DampPatch", new Vector3(-.7f, -.002f, .4f), new Vector2(2.3f, 1.8f), Quaternion.Euler(90f, 0f, 0f), art);
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject crack = Box("Crack_" + i, cracks, new Vector3(-1.2f + i * .58f, 0f, -.45f + i * .27f),
                            new Vector3(.78f, .008f, .018f), art.BrickDark, false);
                        crack.transform.localRotation = Quaternion.Euler(0f, i % 2 == 0 ? -27f : 17f, 0f);
                    }
                }
            }

            // Upper ceilings stop at the external envelope; nothing seals the open gym or auditorium halfway up.
            for (float x = 26f; x < 68f; x += 4f)
            for (float z = 22f; z < 80f; z += 4f)
            {
                Box($"UpperPanel_{x}_{z}", ceilings, new Vector3(x, 7.78f, z), new Vector3(3.95f, .06f, 3.95f), art.AcousticCeiling, false);
                Box($"UpperGridX_{x}_{z}", ceilings, new Vector3(x, 7.735f, z - 1.98f), new Vector3(4f, .035f, .025f), art.MetalSteel, false);
                Box($"UpperGridZ_{x}_{z}", ceilings, new Vector3(x - 1.98f, 7.735f, z), new Vector3(.025f, .035f, 4f), art.MetalSteel, false);
            }

            // Water tracks cluster below fittings and window sills, away from the main entrance and carry aisles.
            for (int i = 0; i < 7; i++)
            {
                float x = 27f + i * 5.6f;
                Stain(damage, "FrontSillLeak_" + i, new Vector3(x, .7f + (i % 3) * .12f, 20.14f),
                    new Vector2(1.1f + i % 2 * .3f, 1.3f), Quaternion.identity, art);
                Stain(damage, "UpperSillLeak_" + i, new Vector3(x, 4.7f, 20.14f),
                    new Vector2(.8f, 1.25f), Quaternion.identity, art);
            }
            Stain(damage, "StageRoofLeak", new Vector3(3f, 1.108f, 53f), new Vector2(3.7f, 2.5f), Quaternion.Euler(90f, 0f, 0f), art);
            Stain(damage, "KitchenDamp", new Vector3(65f, .012f, 77.5f), new Vector2(3f, 2.8f), Quaternion.Euler(90f, 0f, 0f), art);

            Transform services = Group("ExposedServices", wear);
            foreach (float x in new[] { 25.1f, 25.35f })
            {
                Rod(services, "ScienceHeatingRiser", new Vector3(x, 4.15f, 21.1f), new Vector3(x, 7.35f, 21.1f), .075f, art.Rust);
                Rod(services, "ScienceHeatingReturn", new Vector3(x, 7.35f, 21.1f), new Vector3(x, 7.35f, 30.8f), .075f, art.Rust);
            }
            for (int i = 0; i < 6; i++)
                Box("PipeWallClip_" + i, services, new Vector3(25.22f, 4.5f + i * .46f, 21.1f), new Vector3(.45f, .045f, .16f), art.MetalSteel, false);

            for (int i = 0; i < 3; i++)
            {
                float x = 58f + i * 1.7f;
                Rod(services, "BasementSteamMain_" + i, new Vector3(x, -.68f, 80.5f), new Vector3(x, -.68f, 91.5f), .16f, art.Rust);
                Rod(services, "BasementValveDrop_" + i, new Vector3(x, -.68f, 90.5f), new Vector3(x, -2.2f, 90.5f), .12f, art.MetalSteel);
                Box("PipeAnchor_" + i, services, new Vector3(x, -.37f, 84f), new Vector3(.07f, .6f, .07f), art.MetalSteel, false);
            }
            MarkStatic(wear);
        }

        private static void Stain(Transform parent, string name, Vector3 at, Vector2 size, Quaternion rotation, BurleySchoolArt art)
        {
            GameObject stain = Primitive(PrimitiveType.Quad, name, parent, at, new Vector3(size.x, size.y, 1f), art.DampStain, false);
            stain.transform.localRotation = rotation;
            stain.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Rod(Transform parent, string name, Vector3 a, Vector3 b, float diameter, Material material)
        {
            GameObject pipe = Primitive(PrimitiveType.Cylinder, name, parent, (a + b) * .5f,
                new Vector3(diameter, Vector3.Distance(a, b) * .5f, diameter), material, false);
            pipe.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }
    }
}

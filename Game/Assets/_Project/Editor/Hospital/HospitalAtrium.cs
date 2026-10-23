using Abandoned.EditorTools;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
namespace Abandoned.Hospital
{
    public static class HospitalAtrium
    {
        public static void Dress(StationContext c)
        {
            // Waiting pockets occupy the wide ground gallery, leaving both four-metre side lanes open.
            foreach (float z in new []
            {
                6f, 16f, 28f, 42f
            })
            {
                foreach (float x in new []
                {
                    25f, 31f
                })
                {
                    F.Bench(c.Props, "Galleria", new Vector3(x, 0, z), x <28 ? 90f : 270f, 2.1f);
                }
            }
            foreach (float z in new []
            {
                10f, 34f
            })
            {
                Transform island = c.Props.Place("Galleria", "DryOasisBasin", new Vector3(28, 0, z), 0f, new Vector3(2.4f, .5f, 2.6f));
                if (island == null)
                {
                    continue;
                }
                StationProps.B(island, "Basin", new Vector3(0, .24f, 0), new Vector3(2.4f, .48f, 2.6f), c.Art.Trim);
                StationProps.B(island, "SandBed", new Vector3(0, .49f, 0), new Vector3(2.1f, .03f, 2.3f), c.Art.Gravel);
            }
            for (int z = 4; z <48; z += 8)
            {
                foreach (float x in new []
                {
                    23.8f, 32.2f
                })
                {
                    Transform column = c.Props.Hang("Galleria", "FlutedColumn", new Vector3(x, 0, z), 0f);
                    StationProps.B(column, "StoneShaft", new Vector3(0, 3.8f, 0), new Vector3(.24f, 7.6f, .3f), c.Art.Trim);
                    for (int i = 0; i <3; i++)
                    {
                        StationProps.B(column, "BronzeFlute", new Vector3(- .07f + i * .07f, 3.8f, - .17f), new Vector3(.025f, 7.3f, .04f), c.Art.Brass);
                    }
                }
            }
            // Repeated pointed frames make the public gallery legible from either clinical wing.
            foreach (float z in new []
            {
                8f, 24f, 40f
            })
            {
                Transform crown = c.Props.Hang("Galleria", "PatternedPortal", new Vector3(28, 6.4f, z), 0f);
                foreach (float sign in new []
                {
                    - 1f, 1f
                })
                {
                    GameObject arm = StationProps.B(crown, "ArchShoulder", new Vector3(sign * 2f, .35f, 0), new Vector3(4.3f, .16f, .22f), c.Art.Brass);
                    arm.transform.localRotation = Quaternion.Euler(0, 0, - sign * 12f);
                }
            }
            // Floor inlays communicate the public/clinical routes without more text signage.
            foreach (float x in new []
            {
                21f, 35f
            })
            {
                Transform band = c.Props.Hang("Galleria", "ClinicalGuideInlay", new Vector3(x, .014f, 24), 0f);
                StationProps.B(band, "Strip", Vector3.zero, new Vector3(.12f, .015f, 46f), c.Art.Accent);
            }
        }
    }
}

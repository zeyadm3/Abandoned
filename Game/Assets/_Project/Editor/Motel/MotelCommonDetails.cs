using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelCommonDetails
    {
        public static void Build(Transform root, MotelKit k, MotelStructure floors)
        {
            Transform g = MotelKit.Group("FittedServiceDetails", root);
            Transform office = MotelKit.Group("OfficeMailSlots", g);
            office.position = new Vector3(35.8f, 1.65f, 18.8f);
            k.Box("Backing", office, Vector3.zero, new Vector3(.08f, 1.1f, 1.5f), k.Wood, false);
            for (int i = 0; i <5; i++)
            {
                k.Box("FixedReceiptSlot", office, new Vector3(- .12f, - .45f + i * .22f, 0), new Vector3(.24f, .035f, 1.45f), k.Metal, false);
            }
            k.Attach(office, floors.At(new Vector3(35.8f, 0, 18.8f)));
            Transform pipe = MotelKit.Group("LaundryCeilingServices", g);
            pipe.position = new Vector3(30, 2.85f, 25);
            foreach (float x in new []
            {
                - .2f, .2f
            })
            {
                k.Box("LaundryPipe", pipe, new Vector3(x, 0, 0), new Vector3(.06f, .06f, 5.3f), k.Metal, false);
            }
            for (int z = - 2; z <= 2; z++)
            {
                k.Box("PipeClip", pipe, new Vector3(0, .08f, z), new Vector3(.65f, .035f, .08f), k.Accent, false);
            }
            k.Attach(pipe, floors.At(new Vector3(30, 4, 25)));
            k.Box("DinerExtractorHood", g, new Vector3(102, 2.65f, 16), new Vector3(1.5f, .5f, 5.2f), k.Metal, false);
            k.Box("DinerExtractorDuct", g, new Vector3(103.2f, 3.2f, 16), new Vector3(.35f, 1.3f, .35f), k.Metal, false);
            for (int i = 0; i <16; i++)
            {
                Transform joist = MotelKit.Group("GalleryEdgeJoist", g);
                joist.position = new Vector3(i * 4f + 2f, 3.55f, 13);
                k.Box("WeatheredJoist", joist, Vector3.zero, new Vector3(3.8f, .12f, 1.75f), k.Metal, false);
                foreach (float x in new []
                {
                    - 1.7f, 1.7f
                })
                {
                    k.Box("EndPlate", joist, new Vector3(x, - .04f, 0), new Vector3(.15f, .2f, 1.8f), k.Accent, false);
                }
                k.Attach(joist, floors.At(new Vector3(i * 4f + 2f, 4, 13)));
            }
            k.Box("ProtectedGalleryInlay", g, new Vector3(32, 4.012f, 14.18f), new Vector3(63.8f, .015f, .08f), k.Accent, false);
        }
    }
}

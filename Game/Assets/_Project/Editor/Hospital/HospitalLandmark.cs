using Abandoned.EditorTools;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalLandmark
    {
        public static void Build(Transform parent, HospitalKit k, HospitalPalette p)
        {
            Transform g = HospitalKit.Group("MedicalCityArchitecture", parent);
            // The projecting crescent changes the approach silhouette while leaving the loading apron alone.
            for (int i = 0; i <18; i++)
            {
                float angle =(- 170f + i * 160f / 17f) * Mathf.Deg2Rad;
                Vector3 at = new(28f + Mathf.Cos(angle) * 10f, 6.4f, - 1f + Mathf.Sin(angle) * 5f);
                GameObject panel = k.Box("CrescentPortico", g, at, new Vector3(1.9f, .3f, 4f), p.White);
                panel.transform.localRotation = Quaternion.Euler(0f, - angle * Mathf.Rad2Deg, 0f);
                k.Box("BronzeSoffit", panel.transform, new Vector3(0f, - .6f, 0f), new Vector3(.96f, .08f, .95f), p.Bronze, false);
            }
            foreach (float x in new []
            {
                19f, 37f
            })
            {
                k.Box("EntryColumn", g, new Vector3(x, 3.1f, - 2f), new Vector3(.55f, 6.2f, .55f), p.Stone);
            }
            Transform lantern = HospitalKit.Group("GalleryLantern", parent.Find("Roof"));
            k.Box("AtriumSkylight", lantern, new Vector3(28f, 7.95f, 18f), new Vector3(8f, .12f, 36f), p.Glass);
            for (int z = 0; z <= 36; z += 4)
            {
                k.Box("LanternRib", lantern, new Vector3(28f, 8.1f, z), new Vector3(8.4f, .25f, .12f), p.White, false);
            }
            foreach (float x in new []
            {
                24f, 32f
            })
            {
                k.Box("LanternEdge", lantern, new Vector3(x, 8.08f, 18f), new Vector3(.2f, .25f, 36f), p.Bronze, false);
            }
            k.Box("UpperEntryGlazing", g, new Vector3(28f, 5.8f, - .04f), new Vector3(8f, 3.7f, .08f), p.Glass);
            foreach (float x in new []
            {
                24f, 28f, 32f
            })
            {
                k.Box("EntryGlassFin", g, new Vector3(x, 5.8f, - .12f), new Vector3(.08f, 3.8f, .3f), p.Bronze, false);
            }
            // A dry reflecting basin is part of the hospital courtyard, not an expanded world surround.
            k.Box("CourtPaving", g, new Vector3(46f, - .2f, 22f), new Vector3(20f, .4f, 12f), p.Stone);
            k.Box("PublicForecourtInset", g, new Vector3(10f, - .2f, 4f), new Vector3(20f, .4f, 8f), p.Stone);
            foreach (float x in new []
            {
                40f, 52f
            })
            {
                k.Box("DryBasin", g, new Vector3(x, .16f, 22f), new Vector3(3f, .32f, 6f), p.Dark);
                k.Box("BasinBed", g, new Vector3(x, .33f, 22f), new Vector3(2.6f, .025f, 5.6f), k.Asphalt, false);
            }
            Transform canopy = HospitalKit.Group("AmbulanceCanopy", g);
            k.Box("WingCanopy", canopy, new Vector3(46f, 3.8f, - 3f), new Vector3(20f, .25f, 6f), p.White);
            foreach (float x in new []
            {
                36.5f, 55.5f
            })
            {
                k.Box("BayColumn", canopy, new Vector3(x, 1.85f, - 5.5f), new Vector3(.25f, 3.7f, .25f), p.Bronze);
            }
            k.Box("CrossVertical", canopy, new Vector3(46f, 3.1f, - 6.04f), new Vector3(.2f, .9f, .05f), k.Glow, false);
            k.Box("CrossHorizontal", canopy, new Vector3(46f, 3.1f, - 6.08f), new Vector3(.7f, .2f, .05f), k.Glow, false);
            GreyboxFactory.MarkStatic(g);
        }
    }
}

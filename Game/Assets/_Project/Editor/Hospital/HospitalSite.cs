using Abandoned.Core;
using Abandoned.EditorTools;
using UnityEngine;
namespace Abandoned.Hospital
{
    public static class HospitalSite
    {
        public static void Build(Transform root, HospitalKit kit)
        {
            Transform site = HospitalKit.Group("Forecourt", root);
            // Ground surrounds the footprint; it must never seal the morgue or its stair opening.
            kit.Box("SouthApron", site, new Vector3(28f, - .2f, - 14f), new Vector3(80f, .4f, 28f), kit.Asphalt);
            kit.Box("RearServiceYard", site, new Vector3(28f, - .2f, 54f), new Vector3(80f, .4f, 12f), kit.Asphalt);
            foreach (float x in new []
            {
                - 6f, 62f
            })
            {
                kit.Box("SideAccess", site, new Vector3(x, - .2f, 24f), new Vector3(12f, .4f, 48f), kit.Asphalt);
            }
            Transform dock = HospitalKit.Group("VanDock", root);
            dock.SetPositionAndRotation(new Vector3(44f, 0f, - 14f), Quaternion.Euler(0f, 180f, 0f));
            foreach (float x in new []
            {
                40f, 48f
            })
            {
                kit.Box("AmbulanceLane", site, new Vector3(x, .012f, - 7f), new Vector3(.12f, .02f, 14f), kit.Glow, false);
            }
            for (int i = 0; i <7; i++)
            {
                float x = - 2f + i * 4f;
                kit.Box("ParkingStripe", site, new Vector3(x, .01f, - 20f), new Vector3(.1f, .02f, 5f), kit.Glow, false);
                kit.Box("WheelStop", site, new Vector3(x + 1.8f, .1f, - 22f), new Vector3(2.5f, .2f, .25f), kit.Wall);
            }
            for (int i = 0; i <5; i++)
            {
                Transform rack = HospitalKit.Group("OutdoorGasRack", site);
                rack.position = new Vector3(59f, 0f, 38f + i * 2f);
                kit.Box("CylinderBody", rack, new Vector3(0f, .8f, 0f), new Vector3(.4f, 1.6f, .4f), kit.Metal);
                kit.Box("Valve", rack, new Vector3(0f, 1.7f, 0f), new Vector3(.12f, .2f, .12f), kit.Accent, false);
            }
            foreach (float x in new []
            {
                - 9f, 65f
            })
            {
                foreach (float z in new []
                {
                    - 22f, 55f
                })
                {
                    kit.Box("YardPost", site, new Vector3(x, 3f, z), new Vector3(.18f, 6f, .18f), kit.Metal);
                    Transform head = HospitalKit.Group("IndependentYardLight", site);
                    head.position = new Vector3(x, 5.8f, z);
                    head.gameObject.AddComponent<PowerIndependentLight>();
                    Light lamp = head.gameObject.AddComponent<Light>();
                    lamp.type = LightType.Point;
                    lamp.range = 15f;
                    lamp.intensity = 2f;
                    lamp.color = new Color(1f, .76f, .48f);
                    lamp.shadows = LightShadows.None;
                }
            }
            GreyboxFactory.MarkStatic(site);
            // The district round the hospital's own ground (streets, blocks, street furniture; off the NavMesh).
            HospitalTown.Build(root);
        }
    }
}

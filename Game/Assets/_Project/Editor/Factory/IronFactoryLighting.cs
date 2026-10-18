using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Light for the heights (step 7): the roof trusses the catwalks hang from, high-bay lamps under them that
    /// reach the upper tiers (the pendant lights from step 1 only light the floor), and sodium floodlight masts
    /// over the dock yard. The high bays are ordinary <see cref="LightFixture"/>s, so the power, the danger's
    /// blackouts and the breakers treat them like every other ceiling light.
    /// </summary>
    public static class IronFactoryLighting
    {
        private const float TrussSpacing = 12f;
        private const float TrussY = 18.8f;

        public static int Build(Transform root, IronFactoryArt art)
        {
            Trusses(Group("Trusses", root), art);
            int lamps = HighBays(root.Find("Lights") ?? Group("Lights", root), art);
            YardMasts(Group("YardLights", root), art);
            return lamps;
        }

        // North-south trusses every 12 m: a bottom and a top chord with uprights, all well above tier 3.
        private static void Trusses(Transform g, IronFactoryArt art)
        {
            float depth = L.Depth * L.Tile;
            for (float x = TrussSpacing; x < L.Width * L.Tile; x += TrussSpacing)
            {
                Transform truss = Group($"Truss_{x:0}", g);
                Box("BottomChord", truss, new Vector3(x, TrussY, depth * .5f), new Vector3(.3f, .35f, depth), art.Steel, false);
                Box("TopChord", truss, new Vector3(x, L.WallHeight - .25f, depth * .5f), new Vector3(.3f, .3f, depth), art.Steel, false);
                for (float z = 4f; z < depth; z += 6f)
                    Box("Upright", truss, new Vector3(x, (TrussY + L.WallHeight - .25f) * .5f, z), new Vector3(.12f, L.WallHeight - .25f - TrussY, .12f), art.Steel, false);
            }
            MarkStatic(g);
        }

        private static int HighBays(Transform lights, IronFactoryArt art)
        {
            int index = 0;
            for (float x = TrussSpacing; x < L.Width * L.Tile; x += TrussSpacing * 2f)
            for (float z = 14f; z < L.Depth * L.Tile; z += 24f)
            {
                Transform holder = Group("HighBay_" + index, lights);
                holder.position = new Vector3(x, TrussY - .45f, z);
                Box("Housing", holder, new Vector3(0f, .2f, 0f), new Vector3(.7f, .3f, .7f), art.Steel, false);
                GameObject glow = Box("Lens", holder, Vector3.zero, new Vector3(.6f, .06f, .6f), art.Lamp, false);
                Transform lampRoot = Group("Light", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 24f;
                lamp.spotAngle = 95f;
                lamp.intensity = 9f;
                lamp.color = new Color(1f, .82f, .58f);
                lamp.shadows = LightShadows.None;
                // L2: the heights stay mostly dark: four in ten dead, a quarter failing.
                int roll = Mathf.Abs((index * 6151 + 911) % 100);
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(),
                    roll >= 40 && roll < 65, roll < 40, 2400 + index);
                index++;
            }
            return index;
        }

        // Two tall masts either side of the dock, sodium orange, aimed at the truck and the siding.
        private static void YardMasts(Transform g, IronFactoryArt art)
        {
            foreach (float x in new[] { L.DockX - 32f, L.DockX + 32f })
            {
                Transform mast = Group("FloodMast", g);
                mast.position = new Vector3(x, 0f, -16f);
                Box("Pole", mast, new Vector3(0f, 6f, 0f), new Vector3(.3f, 12f, .3f), art.Steel);
                Box("Head", mast, new Vector3(0f, 12.1f, 0f), new Vector3(1.6f, .4f, .6f), art.Lamp, false);
                Transform aim = Group("Light", mast);
                aim.localPosition = new Vector3(0f, 11.8f, 0f);
                aim.rotation = Quaternion.LookRotation(new Vector3(L.DockX, 0f, -4f) - (mast.position + Vector3.up * 11.8f));
                Light flood = aim.gameObject.AddComponent<Light>();
                flood.type = LightType.Spot;
                flood.range = 45f;
                flood.spotAngle = 70f;
                flood.intensity = 14f;
                flood.color = new Color(1f, .62f, .28f);
                flood.shadows = LightShadows.None;
            }
        }
    }
}

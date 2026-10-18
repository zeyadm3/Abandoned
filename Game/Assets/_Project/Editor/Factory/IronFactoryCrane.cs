using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L3: the overhead crane, parked over the lava. Twin box girders span the channel on end trucks that ride the
    /// tier-3 runways; from the trolley, four cables lower a hook block and a crucible that hangs a few metres above
    /// the molten iron, swaying slowly. Scenery only (no colliders): it sits above head height on the runways and
    /// clear of every walkway and bridge below. The loot crucible is a different, smaller one.
    /// </summary>
    public static class IronFactoryCrane
    {
        // Over the channel at row 17: clear of tier 1's north bridge (row 18), tier 2's bridges (rows 10 and 14)
        // and tier 3's narrow bridge (row 19).
        private const float Z = 70f;
        private const float GirderY = 17.6f;
        private const float HookY = 10.4f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Rect c = L.ChannelMeters;
            float x0 = c.xMin - 6f, x1 = c.xMax + 6f, mid = c.center.x, span = x1 - x0;
            Transform crane = Group("Crane", root);

            foreach (float side in new[] { -.7f, .7f })
                Box("Girder", crane, new Vector3(mid, GirderY, Z + side), new Vector3(span, .9f, .5f), art.Safety, false);
            foreach (float x in new[] { x0 + 1.2f, x1 - 1.2f })
            {
                Box("EndTruck", crane, new Vector3(x, GirderY - .35f, Z), new Vector3(2.4f, .7f, 3.2f), art.Steel, false);
                foreach (float z in new[] { -1.2f, 1.2f })
                    Primitive(PrimitiveType.Cylinder, "Wheel", crane, new Vector3(x, GirderY - .75f, Z + z), new Vector3(.5f, .12f, .5f), art.Steel, false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            Box("Trolley", crane, new Vector3(mid, GirderY + .55f, Z), new Vector3(2.6f, 1.1f, 2.4f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "HoistDrum", crane, new Vector3(mid, GirderY + .2f, Z), new Vector3(1f, .9f, 1f), art.Rust, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Box("Cab", crane, new Vector3(x0 + 3.5f, GirderY - 1.4f, Z + 1.6f), new Vector3(1.8f, 1.8f, 1.6f), art.Steel, false);
            MarkStatic(crane);

            // Everything below the trolley swings together from its underside.
            Transform hoist = Group("Hoist", root);
            hoist.position = new Vector3(mid, GirderY - .5f, Z);
            float cable = hoist.position.y - HookY;
            foreach (Vector2 o in new[] { new Vector2(-.25f, -.25f), new Vector2(.25f, -.25f), new Vector2(-.25f, .25f), new Vector2(.25f, .25f) })
                Box("Cable", hoist, new Vector3(o.x, -cable * .5f, o.y), new Vector3(.04f, cable, .04f), art.Steel, false);
            Box("HookBlock", hoist, new Vector3(0f, -cable, 0f), new Vector3(1f, .8f, .6f), art.Safety, false);
            Box("Hook", hoist, new Vector3(0f, -cable - .6f, 0f), new Vector3(.15f, .5f, .15f), art.Steel, false);
            Crucible(hoist, -cable - 2.4f, art);
            hoist.gameObject.AddComponent<IdleSway>().EditorSetup(2.5f, 9f);
        }

        // A ladle-crucible on its bail: a rusted pot, a rim, trunnions, a crust of iron that never quite cooled.
        private static void Crucible(Transform hoist, float centreY, IronFactoryArt art)
        {
            Transform pot = Group("Crucible", hoist);
            pot.localPosition = Vector3.up * centreY;
            Primitive(PrimitiveType.Cylinder, "Pot", pot, Vector3.zero, new Vector3(2.4f, .95f, 2.4f), art.Rust, false);
            Primitive(PrimitiveType.Cylinder, "Rim", pot, Vector3.up * .95f, new Vector3(2.6f, .08f, 2.6f), art.Steel, false);
            Material crust = GetMaterial("IronFactory_CoolingIron", new Color(.25f, .08f, .03f));
            crust.SetColor("_EmissionColor", new Color(1.2f, .3f, .05f));
            crust.EnableKeyword("_EMISSION");
            Primitive(PrimitiveType.Cylinder, "Crust", pot, Vector3.up * .9f, new Vector3(2.2f, .03f, 2.2f), crust, false);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Trunnion", pot, new Vector3(side * 1.35f, .3f, 0f), new Vector3(.3f, .3f, .3f), art.Steel, false);
                Box("Bail", pot, new Vector3(side * 1.4f, 1.2f, 0f), new Vector3(.12f, 1.8f, .12f), art.Steel, false);
            }
            Box("BailTop", pot, new Vector3(0f, 2.1f, 0f), new Vector3(2.9f, .14f, .14f), art.Steel, false);
        }
    }
}

using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Two more kinds of hiding place: a canvas laundry cart on castors (<see cref="HideSpotKind.Cart"/>, open on top,
    /// a flap at the front) and a steel-mesh gear cage of the kind turnout gear and SCBA sets are locked in
    /// (<see cref="HideSpotKind.Cabinet"/>). Same rules and wiring as the others.
    /// </summary>
    public sealed partial class StationHiding
    {
        public Transform Cart(StationPlan.Room room, StationPlan.Side wall, float along) =>
            Cart(room.Name, StationProps.Against(room, wall, along, 1f + Front, out float yaw), yaw);
        public Transform Cage(StationPlan.Room room, StationPlan.Side wall, float along) =>
            Cage(room.Name, StationProps.Against(room, wall, along, .8f + Front, out float yaw), yaw);

        public Transform Cart(string room, Vector3 at, float yaw)
        {
            const float w = 1.25f, h = 1.8f, d = 1f;
            Transform spot = Begin(room, "Cart", at, yaw, new Vector3(w, h, d));
            if (spot == null) return null;
            StationArt a = props.Art;
            Material canvas = a.Plain("CartCanvas", new Color(.55f, .52f, .44f), .1f);
            Box("Back", spot, new Vector3(0f, h * .5f, .02f), new Vector3(w, h - .2f, .04f), canvas, false);
            foreach (float x in new[] { -w * .5f, w * .5f })
            {
                Box("Side", spot, new Vector3(x, h * .5f, d * .5f), new Vector3(.04f, h - .2f, d), canvas, false);
                Box("RimSide", spot, new Vector3(x, h - .08f, d * .5f), new Vector3(.045f, .05f, d + .04f), a.Steel, false);
            }
            foreach (float z in new[] { 0f, d })
                Box("RimEnd", spot, new Vector3(0f, h - .08f, z), new Vector3(w, .05f, .045f), a.Steel, false);
            Box("Base", spot, new Vector3(0f, .15f, d * .5f), new Vector3(w, .06f, d), a.Steel, false);
            foreach (float x in new[] { -w * .45f, w * .45f })
            foreach (float z in new[] { .1f, d - .1f })
                Primitive(PrimitiveType.Cylinder, "Castor", spot, new Vector3(x, .07f, z), new Vector3(.12f, .02f, .12f), a.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            for (int i = 0; i < 3; i++)
                Box("Linen", spot, new Vector3(-.3f + i * .3f, h * .45f, d * .4f), new Vector3(.4f, .2f, .5f), a.Mattress, false);
            // The flap: hinged on its left edge at the front, like a locker door.
            Transform flap = Group("Door", spot);
            flap.localPosition = new Vector3(-w * .5f, 0f, d);
            Box("Canvas", flap, new Vector3(w * .5f, h * .5f + .05f, 0f), new Vector3(w, h - .3f, .03f), canvas, false);
            Box("Strap", flap, new Vector3(w - .1f, 1.1f, .02f), new Vector3(.06f, .3f, .02f), a.Leather, false);
            Wire(spot, flap, new Vector3(w, h, d), HideSpotKind.Cart, at);
            Carts++;
            return spot;
        }

        public Transform Cage(string room, Vector3 at, float yaw)
        {
            const float w = 1.2f, h = 2.1f, d = .8f;
            Transform spot = Begin(room, "Cage", at, yaw, new Vector3(w, h, d));
            if (spot == null) return null;
            StationArt a = props.Art;
            Box("Back", spot, new Vector3(0f, h * .5f, .02f), new Vector3(w, h, .03f), a.Grating, false);
            foreach (float x in new[] { -w * .5f, w * .5f })
            {
                Box("Side", spot, new Vector3(x, h * .5f, d * .5f), new Vector3(.03f, h, d), a.Grating, false);
                Box("Post", spot, new Vector3(x, h * .5f, d), new Vector3(.05f, h, .05f), a.DarkSteel, false);
            }
            Box("Top", spot, new Vector3(0f, h - .02f, d * .5f), new Vector3(w, .03f, d), a.Grating, false);
            // Gear hung inside: a coat, a helmet on the shelf, boots on the floor.
            Material coat = a.Plain("TurnoutCoat", new Color(.55f, .48f, .3f), .1f);
            Box("Shelf", spot, new Vector3(0f, 1.7f, d * .45f), new Vector3(w - .06f, .03f, d - .1f), a.Steel, false);
            Box("Coat", spot, new Vector3(-.25f, 1.15f, .25f), new Vector3(.5f, .8f, .25f), coat, false);
            Box("Helmet", spot, new Vector3(.25f, 1.82f, .35f), new Vector3(.32f, .2f, .36f), a.Rubber, false);
            Box("Boots", spot, new Vector3(.25f, .15f, .3f), new Vector3(.36f, .3f, .28f), a.Rubber, false);
            Transform door = Group("Door", spot);
            door.localPosition = new Vector3(-w * .5f, 0f, d);
            Box("Mesh", door, new Vector3(w * .5f, h * .5f, 0f), new Vector3(w - .04f, h - .04f, .02f), a.Grating, false);
            Box("Frame", door, new Vector3(w * .5f, h * .5f, 0f), new Vector3(w - .02f, .05f, .04f), a.DarkSteel, false);
            Box("Padlock", door, new Vector3(w - .1f, 1.05f, .04f), new Vector3(.06f, .09f, .03f), a.Brass, false);
            Wire(spot, door, new Vector3(w, h, d), HideSpotKind.Cabinet, at);
            Cages++;
            return spot;
        }

        // Places the footprint (front clearance included) and returns the spot's own frame: origin at the back, +z out.
        private Transform Begin(string room, string kind, Vector3 at, float yaw, Vector3 size)
        {
            if (Count >= Max) return null;
            Transform root = props.Place(room, $"{kind}_{Count:00}", at, yaw, size + new Vector3(0f, 0f, Front), solid: false);
            if (root == null) return null;
            Transform spot = Group("Spot", root);
            spot.localPosition = new Vector3(0f, 0f, -(size.z + Front) * .5f);
            return spot;
        }
    }
}

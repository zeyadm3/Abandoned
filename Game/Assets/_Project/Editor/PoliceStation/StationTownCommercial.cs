using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using F = Abandoned.EditorTools.StationTownFacade;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood's commercial buildings (see StationTownBuildings.cs): a glass office block (a medical office
    /// building beside the hospital: a red or green cross on its crown), an open parking deck with its split ramps,
    /// stair tower and the cars left on it, and a roadside strip mall under one long canopy.
    /// </summary>
    public static partial class StationTownBuildings
    {
        private static void Office(Transform b, StationArt a, StationTownArt t, float w, float d, int storeys, int seed)
        {
            const float ground = 4.5f, storey = 3.6f;
            float h = ground + (storeys - 1) * storey + .4f;
            Mass(b, w, d, h, t.Window);
            // Spandrel bands at every floor and mullions every 1.5 m on all four sides.
            for (int i = 0; i < storeys; i++)
            {
                float y = ground + i * storey - .45f;
                Box("SpandrelF", b, new Vector3(0f, y, 0f), new Vector3(w + .12f, .9f, .12f), t.Stone, false);
                Box("SpandrelB", b, new Vector3(0f, y, -d), new Vector3(w + .12f, .9f, .12f), t.Stone, false);
                Box("SpandrelL", b, new Vector3(-w * .5f, y, -d * .5f), new Vector3(.12f, .9f, d + .12f), t.Stone, false);
                Box("SpandrelR", b, new Vector3(w * .5f, y, -d * .5f), new Vector3(.12f, .9f, d + .12f), t.Stone, false);
            }
            float top = h - ground;
            for (float x = -w * .5f + 1.5f; x < w * .5f - .5f; x += 1.5f)
            {
                Box("MullionF", b, new Vector3(x, ground + top * .5f, .04f), new Vector3(.07f, top, .1f), t.FrameDark, false);
                Box("MullionB", b, new Vector3(x, ground + top * .5f, -d - .04f), new Vector3(.07f, top, .1f), t.FrameDark, false);
            }
            for (float z = -d + 1.5f; z < -.5f; z += 1.5f)
            {
                Box("MullionL", b, new Vector3(-w * .5f - .04f, ground + top * .5f, z), new Vector3(.1f, top, .07f), t.FrameDark, false);
                Box("MullionR", b, new Vector3(w * .5f + .04f, ground + top * .5f, z), new Vector3(.1f, top, .07f), t.FrameDark, false);
            }
            // Broken panes: dark holes and the odd board in the curtain wall.
            for (int i = 0; i < 10; i++)
            {
                int roll = Mathf.Abs(seed * 13 + i * 29);
                float x = -w * .5f + 2.25f + (roll % Mathf.Max(1, Mathf.FloorToInt((w - 3f) / 1.5f))) * 1.5f;
                float y = ground + (roll % Mathf.Max(1, storeys - 1)) * storey + 1.6f;
                Box("BrokenPane", b, new Vector3(x, y, .015f), new Vector3(1.38f, 1.9f, .02f), roll % 3 == 0 ? a.Board : t.Void, false);
            }
            // The podium: stone piers, a glazed lobby, a canopy over the doors.
            Transform front = F.Face(b, F.Side.Front, w, d);
            for (float x = -w * .5f + .3f; x <= w * .5f; x += 6f)
                Box("Pier", front, new Vector3(x, ground * .5f, .15f), new Vector3(.6f, ground, .3f), t.Stone, false);
            Box("Lobby", front, new Vector3(0f, 1.5f, .05f), new Vector3(5f, 3f, .02f), t.Void, false);
            foreach (float x in new[] { -.8f, .8f }) Box("GlassDoor", front, new Vector3(x, 1.2f, .07f), new Vector3(1.4f, 2.4f, .02f), t.Window, false);
            Box("Canopy", front, new Vector3(0f, 3.7f, 1.6f), new Vector3(7f, .3f, 3.2f), t.Stone, false);
            foreach (float x in new[] { -3.2f, 3.2f }) Box("CanopyColumn", front, new Vector3(x, 1.85f, 3f), new Vector3(.3f, 3.7f, .3f), a.Steel);
            if (seed % 2 == 0)
            {
                // A medical cross on the crown.
                Material cross = seed % 4 == 0 ? t.Signs[0] : t.Signs[3];
                Box("CrossPanel", front, new Vector3(w * .3f, h - 1.6f, .12f), new Vector3(2.4f, 2.4f, .08f), a.White, false);
                Box("CrossV", front, new Vector3(w * .3f, h - 1.6f, .18f), new Vector3(.6f, 1.8f, .04f), cross, false);
                Box("CrossH", front, new Vector3(w * .3f, h - 1.6f, .18f), new Vector3(1.8f, .6f, .04f), cross, false);
            }
            F.FlatRoof(b, a, t, t.Stone, w, d, h, .9f);
            Box("Penthouse", b, new Vector3(0f, h + 1.5f, -d * .5f), new Vector3(w * .4f, 3f, d * .4f), t.OldConcrete, false);
            for (int i = 0; i < 8; i++)
                Box("Louvre", b, new Vector3(0f, h + .5f + i * .3f, -d * .5f + d * .2f + .02f), new Vector3(w * .36f, .05f, .08f), a.DarkSteel, false);
            Scan(b, "exterior_aircon_unit", new Vector3(-w * .3f, h, -d * .25f), 0f, new Vector3(1.2f, 1.3f, 1.2f));
            Scan(b, "exterior_aircon_unit", new Vector3(w * .3f, h, -d * .75f), 180f, new Vector3(1.2f, 1.3f, 1.2f));
        }

        // An open parking deck: concrete decks on a column grid, parapets round each, two ramp strips along the east
        // side (odd levels climb one, even the other, so no ramp runs under another), a stair tower, the cars left in it.
        private static void ParkingDeck(Transform b, StationArt a, StationTownArt t, float w, float d, int levels, int seed)
        {
            const float rise = 3f, slab = .3f, strip = 3.5f;
            Material concrete = t.OldConcrete;
            float deckW = w - strip * 2f, deckX = -strip;
            for (int i = 1; i <= levels; i++)
            {
                float y = i * rise;
                Box("Deck", b, new Vector3(deckX, y - slab * .5f, -d * .5f), new Vector3(deckW, slab, d), concrete);
                Box("DeckSoffit", b, new Vector3(deckX, y - slab - .005f, -d * .5f), new Vector3(deckW - .2f, .01f, d - .2f), a.Grime, false);
                // Parapets round the deck (the ramp side has its own along the outer edge).
                Box("ParapetF", b, new Vector3(0f, y + .5f, -.15f), new Vector3(w, 1f, .3f), concrete);
                Box("ParapetB", b, new Vector3(0f, y + .5f, -d + .15f), new Vector3(w, 1f, .3f), concrete);
                Box("ParapetW", b, new Vector3(-w * .5f + .15f, y + .5f, -d * .5f), new Vector3(.3f, 1f, d - .6f), concrete);
                Box("ParapetE", b, new Vector3(w * .5f - .15f, y + .5f, -d * .5f), new Vector3(.3f, 1f, d - .6f), concrete);
                Box("Stripe", b, new Vector3(0f, y + .25f, .01f), new Vector3(w, .12f, .01f), a.Safety, false);
                // The climb to this level: strip A (inner) for odd levels, strip B (outer) for even, alternating ends.
                bool odd = i % 2 == 1;
                float x = w * .5f - (odd ? strip * 1.5f : strip * .5f);
                float zLow = odd ? -1f : -d + 1f, zHigh = odd ? -d + 1f : -1f;
                var low = new Vector3(x, (i - 1) * rise, zLow);
                var high = new Vector3(x, i * rise, zHigh);
                Vector3 dir = (high - low).normalized;
                GameObject ramp = Box("Ramp", b, (low + high) * .5f - Vector3.up * slab * .5f, new Vector3(strip - .1f, slab, Vector3.Distance(low, high)), concrete);
                ramp.transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
                Box("RampArrow", b, (low + high) * .5f + Vector3.up * .01f, new Vector3(.3f, .01f, 2f), a.Paint, false).transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
            }
            // Landings at the ramps' far ends join the two strips to the deck.
            for (int i = 1; i <= levels; i++)
            {
                float z = i % 2 == 1 ? -d + .5f : -.5f;
                Box("Landing", b, new Vector3(w * .5f - strip, i * rise - slab * .5f, z), new Vector3(strip * 2f, slab, 1f), concrete);
            }
            for (float x = -w * .5f + .4f; x < w * .5f - strip * 2f; x += 8f)
            for (float z = -.4f; z > -d; z -= 8f)
                Box("Column", b, new Vector3(x, levels * rise * .5f, z), new Vector3(.6f, levels * rise, .6f), concrete);
            // The stair tower at the front west corner, glazed down its front.
            Box("StairTower", b, new Vector3(-w * .5f - 2f, (levels * rise + 2.5f) * .5f, -2.5f), new Vector3(4f, levels * rise + 2.5f, 5f), concrete);
            Box("StairGlass", b, new Vector3(-w * .5f - 2f, (levels * rise + 2.5f) * .5f, .01f), new Vector3(2.4f, levels * rise + 1.5f, .02f), t.Window, false);
            Box("StairDoor", b, new Vector3(-w * .5f - 2f, 1.1f, .02f), new Vector3(1f, 2.2f, .02f), t.DoorSteel, false);
            // The P sign on the tower, the barrier arm at the entrance.
            Box("ParkSign", b, new Vector3(-w * .5f - 2f, levels * rise + 1.2f, .05f), new Vector3(1.6f, 1.6f, .06f), t.Signs[2], false);
            Box("ParkMark", b, new Vector3(-w * .5f - 2f, levels * rise + 1.2f, .09f), new Vector3(.9f, .9f, .02f), a.White, false);
            Box("BarrierPost", b, new Vector3(w * .5f - strip * 2f - .6f, .6f, 1f), new Vector3(.3f, 1.2f, .3f), a.Safety);
            Box("BarrierArm", b, new Vector3(w * .5f - strip * 1.25f, 1.05f, 1f), new Vector3(2.8f, .08f, .08f), a.Red, false);
            // The cars nobody came back for, on every level.
            StationVehicleArt v = StationVehicleArt.For(a);
            for (int i = 0; i <= levels; i++)
            for (int k = 0; k < 4; k++)
            {
                int roll = Mathf.Abs(seed * 17 + i * 31 + k * 7);
                if (roll % 3 == 0) continue;
                // Two cars to a bay, between the columns (which stand every 8 m from 0.4 m in).
                var local = new Vector3(-w * .5f + 4.4f + (k / 2) * 8f + (k % 2 == 0 ? -1.5f : 1.5f), i * rise, -d * .3f - (roll % 2) * d * .35f);
                if (local.x > w * .5f - strip * 2f - 1.5f) continue;
                var kind = (StationVehicleKind)(new[] { 0, 2, 3, 4 }[roll % 4]);
                StationVehicles.Car(b, "DeckCar", b.TransformPoint(local), b.eulerAngles.y + (roll % 2 == 0 ? 0f : 180f), kind, v.Paint(roll), a,
                    StationVehicleWear.Dusty | (roll % 5 == 1 ? StationVehicleWear.FlatRear : StationVehicleWear.None), roll);
            }
            for (float x = -w * .5f + 4f; x < w * .5f - strip * 2f; x += 10f)
            {
                Box("LampPole", b, new Vector3(x, levels * rise + 2.5f, -d * .5f), new Vector3(.15f, 5f, .15f), a.DarkSteel, false);
                Box("LampHead", b, new Vector3(x, levels * rise + 5f, -d * .5f), new Vector3(.9f, .2f, .4f), a.DarkSteel, false);
            }
        }

        // A strip of shop units under one canopy, each with its own front and sign above.
        private static void StripMall(Transform b, StationArt a, StationTownArt t, float w, float d, int seed)
        {
            const float h = 5.2f;
            Material wall = seed % 2 == 0 ? t.Stucco : t.Brick(seed);
            Mass(b, w, d, h, wall);
            Transform front = F.Face(b, F.Side.Front, w, d);
            int units = Mathf.Max(2, Mathf.RoundToInt(w / 8f));
            float unit = w / units;
            for (int i = 0; i < units; i++)
            {
                Transform u = Group("Unit", front);
                float x = -w * .5f + unit * (i + .5f);
                u.localPosition = new Vector3(x, 0f, 0f);
                F.Storefront(u, a, t, unit - .4f, 3.7f, seed + i * 5, awning: false);
                Box("UnitSign", front, new Vector3(x, h + .4f, .1f), new Vector3(unit * .7f, .9f, .1f), t.Sign(seed + i), false);
                Box("UnitMark", front, new Vector3(x - unit * .2f, h + .4f, .16f), new Vector3(.6f, .6f, .02f), t.SignLight, false);
                Box("UnitBar", front, new Vector3(x + unit * .08f, h + .4f, .16f), new Vector3(unit * .3f, .14f, .02f), t.SignLight, false);
            }
            Box("Canopy", b, new Vector3(0f, 3.95f, 1.5f), new Vector3(w + .4f, .3f, 3f), t.Frame, false);
            Box("CanopyFascia", b, new Vector3(0f, 3.95f, 3.01f), new Vector3(w + .4f, .5f, .04f), t.Paint(seed), false);
            for (int i = 0; i <= units; i++)
                Box("CanopyPost", b, new Vector3(-w * .5f + unit * i, 1.9f, 2.8f), new Vector3(.22f, 3.8f, .22f), t.FrameDark);
            Box("Walk", b, new Vector3(0f, .06f, 1.5f), new Vector3(w, .12f, 3f), t.OldConcrete);
            F.FlatRoof(b, a, t, wall, w, d, h, 1.5f);
            for (int i = 0; i < units; i += 2) Scan(b, "exterior_aircon_unit", new Vector3(-w * .5f + unit * (i + .5f), h, -d * .5f), 90f, new Vector3(1.2f, 1.3f, 1.2f));
            Transform back = F.Face(b, F.Side.Back, w, d);
            for (int i = 0; i < units; i++) F.Door(back, a, t, -w * .5f + unit * (i + .5f), 1f, 2.2f, t.DoorSteel);
        }
    }
}

using Abandoned.Interaction;
using UnityEngine;
using F = Abandoned.EditorTools.StationFurniture;
using L = Abandoned.EditorTools.PoliceStationLayout;
using P = Abandoned.EditorTools.StationPlan;
using Props = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The east end of the precinct's basement (y -4): the evidence vault behind its round steel door (walls of
    /// deposit drawers; the basement's jackpot), basement records, and the evidence intake under the sally port, where
    /// seized property was logged, photographed and bagged before it went into the vault.
    /// </summary>
    public static class PoliceStationVault
    {
        private const float Y = -P.Story;

        public static void Dress(StationContext ctx)
        {
            Vault(ctx);
            Records(ctx);
            Intake(ctx);
        }

        // Evidence vault: x 32-44, z 28-40; wide door south at x 38, its round door swung back inside against the west side.
        private static void Vault(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Evidence_Vault");
            Transform door = p.Place(r.Name, "VaultDoor", new Vector3(35.9f, Y, 29.9f), 0f, new Vector3(.5f, 3.2f, 3.2f));
            if (door != null)
            {
                GameObject disc = Props.C(door, "Disc", new Vector3(0f, 1.6f, 0f), new Vector3(3.1f, .25f, 3.1f), a.Steel);
                disc.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                for (int i = 0; i < 8; i++)
                {
                    float ang = i * Mathf.PI / 4f;
                    Props.C(door, "Bolt", new Vector3(.3f, 1.6f + Mathf.Sin(ang) * 1.3f, Mathf.Cos(ang) * 1.3f), new Vector3(.12f, .1f, .12f), a.Chrome)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                Props.B(door, "WheelSpokeA", new Vector3(.32f, 1.6f, 0f), new Vector3(.05f, 1f, .06f), a.Chrome);
                Props.B(door, "WheelSpokeB", new Vector3(.32f, 1.6f, 0f), new Vector3(.05f, .06f, 1f), a.Chrome);
            }
            Transform frame = p.Hang(r.Name, "VaultFrame", new Vector3(38f, Y, 27.8f), 0f);
            foreach (float x in new[] { -1.85f, 1.85f }) Props.B(frame, "Jamb", new Vector3(x, 1.7f, 0f), new Vector3(.5f, 3.4f, .4f), a.DarkSteel);
            Props.B(frame, "Lintel", new Vector3(0f, 3.35f, 0f), new Vector3(4.2f, .5f, .4f), a.DarkSteel);
            DepositWall(p, a, r, P.Side.North, 6f, 6f);
            DepositWall(p, a, r, P.Side.East, 6f, 5f);
            F.Table(p, r.Name, new Vector3(37.4f, Y, 34.6f), 0f, 1.8f, .8f, a.Steel);
            F.Crate(p, r.Name, new Vector3(33.4f, Y, 37.6f), 0f, new Vector3(.9f, .7f, .7f));
            F.Crate(p, r.Name, new Vector3(33.4f, Y, 36.6f), 0f, new Vector3(.8f, .5f, .6f));
            ctx.AddLoot(L.Evidence, new Vector3(40.5f, Y, 32.6f), CarryClass.Pocket, CarryClass.Huge, true);
            ctx.AddLoot(L.Evidence, new Vector3(35.6f, Y, 33.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Evidence, new Vector3(39.6f, Y, 37.4f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.AddLoot(L.Evidence, new Vector3(41.4f, Y, 29.4f), CarryClass.Pocket, CarryClass.Heavy);
            ctx.Threat(new Vector3(36.4f, Y, 38f));
        }

        // A wall of safe-deposit drawers, floor to head height.
        private static void DepositWall(Props p, StationArt a, P.Room r, P.Side wall, float along, float width)
        {
            Transform t = p.Place(r.Name, "DepositDrawers", Props.Against(r, wall, along, .5f, out float yaw), yaw, new Vector3(width, 2.4f, .5f));
            if (t == null) return;
            Props.B(t, "Body", new Vector3(0f, 1.2f, 0f), new Vector3(width, 2.4f, .5f), a.DarkSteel);
            int columns = Mathf.FloorToInt(width / .5f);
            for (int c = 0; c < columns; c++)
            for (int row = 0; row < 5; row++)
            {
                float x = -width * .5f + .25f + c * .5f;
                bool open = (c * 7 + row * 3) % 9 == 0;
                Props.B(t, "Drawer", new Vector3(x, .3f + row * .45f, open ? .4f : .255f), new Vector3(.44f, .38f, open ? .3f : .01f), open ? a.Steel : a.Chrome);
            }
        }

        // Basement records: x 44-52, z 28-40; door south at x 46.
        private static void Records(StationContext ctx)
        {
            Props p = ctx.Props;
            P.Room r = L.Room("Basement_Records");
            F.Shelving(p, r.Name, new Vector3(49f, Y, 33f), 90f, 2.6f, 150, ctx.Art.Paper);
            F.Shelving(p, r.Name, new Vector3(49f, Y, 35.8f), 90f, 2.6f, 151);
            F.Shelving(p, r.Name, Props.Against(r, P.Side.East, 9.5f, .5f, out float yaw), yaw, 2f, 152);
            for (int i = 0; i < 3; i++) F.FilingCabinet(p, r.Name, Props.Against(r, P.Side.West, 8f + i * .55f, .64f, out yaw), yaw);
            F.Desk(p, r.Name, Props.Against(r, P.Side.North, 2.6f, .8f, out yaw), yaw, true, 1.4f);
            F.FloorLitter(p, r.Name, new Vector3(46.6f, Y, 33.6f), 17);
            ctx.AddLoot(L.Office, new Vector3(46.4f, Y, 31.6f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Office, new Vector3(51f, Y, 31f), CarryClass.Pocket, CarryClass.OneHand);
            ctx.Threat(new Vector3(46.6f, Y, 36f));
        }

        // Evidence intake: x 32-52, z 16-24, under the sally port; wide door north at x 42.
        private static void Intake(StationContext ctx)
        {
            Props p = ctx.Props;
            StationArt a = ctx.Art;
            P.Room r = L.Room("Evidence_Intake");
            F.Counter(p, r.Name, new Vector3(37f, Y, 20.4f), 0f, 5f, 1.05f, a.DarkSteel);
            F.Chair(p, r.Name, new Vector3(36f, Y, 19.3f), 0f);
            F.Lockers(p, r.Name, Props.Against(r, P.Side.East, 4f, .52f, out float yaw), yaw, 6);
            Transform photo = p.Place(r.Name, "PhotoStand", new Vector3(45.6f, Y, 17.6f), 0f, new Vector3(1.4f, 1.6f, .9f));
            if (photo != null)
            {
                Props.B(photo, "Table", new Vector3(0f, .8f, 0f), new Vector3(1.4f, .05f, .9f), a.White);
                foreach (float x in new[] { -.65f, .65f }) Props.B(photo, "Leg", new Vector3(x, .4f, 0f), new Vector3(.05f, .8f, .8f), a.DarkSteel);
                Props.B(photo, "Column", new Vector3(0f, 1.2f, -.4f), new Vector3(.05f, .8f, .05f), a.DarkSteel);
                Props.B(photo, "Camera", new Vector3(0f, 1.55f, -.25f), new Vector3(.2f, .15f, .25f), a.Rubber);
            }
            foreach (float x in new[] { 33.4f, 34.6f, 41f }) F.Crate(p, r.Name, new Vector3(x, Y, 16.8f), 0f, new Vector3(.9f, .6f, .7f));
            ctx.Hiding.Cabinet(r, P.Side.South, 16f);
            ctx.AddLoot(L.Evidence, new Vector3(36f, Y, 22.4f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Evidence, new Vector3(44.6f, Y, 21.2f), CarryClass.Pocket, CarryClass.TwoHand);
            ctx.AddLoot(L.Evidence, new Vector3(38.6f, Y, 17.6f), CarryClass.Pocket, CarryClass.OneHand);
        }
    }
}

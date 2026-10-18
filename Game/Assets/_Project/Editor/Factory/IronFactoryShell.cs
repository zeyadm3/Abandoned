using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The plant's envelope: 20 m brick walls on a steel portal frame, the roof, the one dock opening in the
    /// south wall, the enclosed ground-floor rooms (5 m tall, walkable roofs that join catwalk tier 1) and
    /// the pendant work lights that follow the building's power.
    /// </summary>
    public static class IronFactoryShell
    {
        private const float DoorWidth = 2.6f;
        private const float DoorHeight = 2.8f;
        private const float RoomWall = .22f;
        private const float ColumnSpacing = 12f;

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform shell = Group("Shell", root);
            Vector3 size = L.SizeMeters;
            float h = L.WallHeight, t = L.WallThickness;
            Wall(shell, "South", new Vector3(size.x * .5f, 0f, 0f), size.x, h, false, L.DockX, L.DockWidth, L.DockHeight, t, art.Wall);
            Box("North", shell, new Vector3(size.x * .5f, h * .5f, size.z), new Vector3(size.x, h, t), art.Wall);
            Box("West", shell, new Vector3(0f, h * .5f, size.z * .5f), new Vector3(t, h, size.z), art.Wall);
            Box("East", shell, new Vector3(size.x, h * .5f, size.z * .5f), new Vector3(t, h, size.z), art.Wall);
            Box("Roof", shell, new Vector3(size.x * .5f, h + L.Slab * .5f, size.z * .5f), new Vector3(size.x, L.Slab, size.z), art.Steel);
            Columns(Group("Frame", shell), art);
            MarkStatic(shell);

            Transform rooms = Group("Rooms", root);
            foreach (L.Room room in L.Rooms) Room(rooms, room, art);
            MarkStatic(rooms);

            Lights(Group("Lights", root), art);
        }

        // Portal-frame columns just inside the walls: they give the long walls scale and take the roof.
        private static void Columns(Transform frame, IronFactoryArt art)
        {
            Vector3 size = L.SizeMeters;
            const float inset = .7f, side = .6f;
            for (float x = ColumnSpacing; x < size.x; x += ColumnSpacing)
            {
                if (Mathf.Abs(x - L.DockX) > L.DockWidth * .5f + 1f)
                    Box($"Column_S_{x:0}", frame, new Vector3(x, size.y * .5f, inset), new Vector3(side, size.y, side), art.Steel);
                Box($"Column_N_{x:0}", frame, new Vector3(x, size.y * .5f, size.z - inset), new Vector3(side, size.y, side), art.Steel);
            }
            for (float z = ColumnSpacing; z < size.z; z += ColumnSpacing)
            {
                Box($"Column_W_{z:0}", frame, new Vector3(inset, size.y * .5f, z), new Vector3(side, size.y, side), art.Steel);
                Box($"Column_E_{z:0}", frame, new Vector3(size.x - inset, size.y * .5f, z), new Vector3(side, size.y, side), art.Steel);
            }
        }

        private static void Room(Transform parent, L.Room room, IronFactoryArt art)
        {
            Transform holder = Group(room.Name, parent);
            Rect r = room.Meters;
            float wallHeight = L.RoomHeight - L.Slab;
            // Walls sit just inside the room's cells, so two rooms side by side get two walls, never one
            // doubled face; a side on the building's outline uses the outer wall instead.
            float half = RoomWall * .5f;
            if (room.Cells.y > 0)
                Side(holder, "South", new Vector3(r.center.x, 0f, r.yMin + half), r.width, false, room.Doors.HasFlag(L.Door.South), wallHeight, art);
            if (room.Cells.yMax < L.Depth)
                Side(holder, "North", new Vector3(r.center.x, 0f, r.yMax - half), r.width, false, room.Doors.HasFlag(L.Door.North), wallHeight, art);
            if (room.Cells.x > 0)
                Side(holder, "West", new Vector3(r.xMin + half, 0f, r.center.y), r.height, true, room.Doors.HasFlag(L.Door.West), wallHeight, art);
            if (room.Cells.xMax < L.Width)
                Side(holder, "East", new Vector3(r.xMax - half, 0f, r.center.y), r.height, true, room.Doors.HasFlag(L.Door.East), wallHeight, art);

            GameObject roof = Box("Roof", holder, new Vector3(r.center.x, L.RoomHeight - L.Slab * .5f, r.center.y),
                new Vector3(r.width, L.Slab, r.height), art.Steel);
            roof.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
        }

        private static void Side(Transform holder, string name, Vector3 baseCentre, float length, bool alongZ, bool door,
            float height, IronFactoryArt art)
        {
            if (door) Wall(holder, name, baseCentre, length, height, alongZ, alongZ ? baseCentre.z : baseCentre.x, DoorWidth, DoorHeight, RoomWall, art.Wall);
            else Box(name, holder, baseCentre + Vector3.up * height * .5f,
                alongZ ? new Vector3(RoomWall, height, length) : new Vector3(length, height, RoomWall), art.Wall);
        }

        /// <summary>A straight wall with one opening (a door, or the dock) and the lintel over it.</summary>
        private static void Wall(Transform parent, string name, Vector3 baseCentre, float length, float height, bool alongZ,
            float openingCentre, float openingWidth, float openingHeight, float thickness, Material material)
        {
            float start = (alongZ ? baseCentre.z : baseCentre.x) - length * .5f;
            float relative = Mathf.Clamp(openingCentre - start, openingWidth * .5f, length - openingWidth * .5f);
            float left = relative - openingWidth * .5f;
            float right = length - relative - openingWidth * .5f;
            Vector3 axis = alongZ ? Vector3.forward : Vector3.right;
            Vector3 Size(float run, float tall) => alongZ ? new Vector3(thickness, tall, run) : new Vector3(run, tall, thickness);
            if (left > .01f)
                Box(name + "_L", parent, baseCentre - axis * (length * .5f - left * .5f) + Vector3.up * height * .5f, Size(left, height), material);
            if (right > .01f)
                Box(name + "_R", parent, baseCentre + axis * (length * .5f - right * .5f) + Vector3.up * height * .5f, Size(right, height), material);
            float lintel = height - openingHeight;
            if (lintel > .01f)
                Box(name + "_Lintel", parent, baseCentre + axis * (relative - length * .5f) + Vector3.up * (openingHeight + lintel * .5f),
                    Size(openingWidth, lintel), material);
        }

        // Pendant work lights on a 12 m grid, none over the channel (its glow lights it). They hang just
        // under catwalk tier 1, so step 2's walkways can carry them.
        private static void Lights(Transform lights, IronFactoryArt art)
        {
            Rect channel = L.ChannelMeters;
            const float hang = 4.55f;
            int index = 0;
            for (float z = 6f; z < L.Depth * L.Tile; z += 12f)
            for (float x = 6f; x < L.Width * L.Tile; x += 12f)
            {
                if (x > channel.xMin - 2f && x < channel.xMax + 2f && z > channel.yMin - 2f && z < channel.yMax + 2f) continue;
                Transform holder = Group("Fixture_" + index, lights);
                holder.position = new Vector3(x, hang, z);
                if (!InsideRoom(x, z))
                    Box("Drop", holder, Vector3.up * (L.WallHeight - hang) * .5f, new Vector3(.04f, L.WallHeight - hang, .04f), art.Steel, false);
                GameObject glow = Box("Diffuser", holder, Vector3.zero, new Vector3(1.25f, .12f, .48f), art.Lamp, false);
                Transform lampRoot = Group("Light", holder);
                lampRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Light lamp = lampRoot.gameObject.AddComponent<Light>();
                lamp.type = LightType.Spot;
                lamp.range = 12f;
                lamp.spotAngle = 108f;
                lamp.intensity = 7f;
                lamp.color = new Color(1f, .86f, .64f);
                lamp.shadows = LightShadows.None;
                holder.gameObject.AddComponent<LightFixture>().EditorSetup(lamp, glow.GetComponent<Renderer>(),
                    index % 7 == 0, index % 17 == 0, 2200 + index);
                index++;
            }
        }

        private static bool InsideRoom(float x, float z)
        {
            foreach (L.Room room in L.Rooms)
                if (room.Meters.Contains(new Vector2(x, z))) return true;
            return false;
        }
    }
}

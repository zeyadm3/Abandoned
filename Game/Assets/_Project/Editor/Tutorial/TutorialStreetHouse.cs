using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.TutorialStreetHouseKit;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// One copy of the training house on a street lot: same plan, its own paint, sometimes mirrored, with or without
    /// a garage, a hood over the stoop, a chimney, a wreath, shrubs. Shut up for the winter: nothing inside, and near
    /// the crew a solid block per wing so nobody walks in (the walls themselves are one render mesh with no collider).
    /// </summary>
    public static class TutorialStreetHouse
    {
        /// <summary>What makes one copy its own. Chimney is an index into the chimney spots, or -1 for none.</summary>
        public readonly struct Style
        {
            public readonly bool Mirrored, Garage, Hood, Wreath, ShrubSouth, ShrubNorth;
            public readonly int Chimney;
            public readonly Material Siding, Shadow, Shingles, Door, Trim;

            public Style(bool mirrored, bool garage, bool hood, bool wreath, bool shrubSouth, bool shrubNorth, int chimney,
                Material siding, Material shadow, Material shingles, Material door, Material trim)
            {
                Mirrored = mirrored; Garage = garage; Hood = hood; Wreath = wreath; ShrubSouth = shrubSouth; ShrubNorth = shrubNorth;
                Chimney = chimney; Siding = siding; Shadow = shadow; Shingles = shingles; Door = door; Trim = trim;
            }
        }

        /// <summary>Where a copy stands: its position and yaw, and the ground it covers (world, roof and stoop included).</summary>
        public readonly struct Placement
        {
            public readonly Vector3 Position;
            public readonly float Yaw, Setback;
            public readonly Rect Footprint;
            public Placement(Vector3 position, float yaw, float setback, Rect footprint) { Position = position; Yaw = yaw; Setback = setback; Footprint = footprint; }
        }

        // The training house's chimney on the band's north slope, one on the west wing's back slope, one over the kitchen.
        private static readonly Vector3[] ChimneySpots =
        {
            TutorialHouseRoof.ChimneyAt,
            new(1.6f, TutorialHouseRoof.Eave, 9f),
            new(L.Kitchen.center.x, TutorialHouseRoof.Eave, L.BandSouth + 1.1f),
        };

        public static Style Roll(System.Random random, TutorialHouseArt art)
        {
            int siding = random.Next(art.StreetSidings.Length);
            return new Style(
                mirrored: random.Next(2) == 0,
                garage: random.NextDouble() < .55,
                hood: random.NextDouble() < .45,
                wreath: random.NextDouble() < .35,
                shrubSouth: random.NextDouble() < .6,
                shrubNorth: random.NextDouble() < .6,
                chimney: random.Next(ChimneySpots.Length + 1) - 1,
                siding: art.StreetSidings[siding],
                shadow: art.StreetSidingShadows[siding],
                shingles: art.StreetShingles[random.Next(art.StreetShingles.Length)],
                door: art.StreetDoors[random.Next(art.StreetDoors.Length)],
                trim: random.NextDouble() < .7 ? art.StreetTrims[0] : art.StreetTrims[1 + random.Next(art.StreetTrims.Length - 1)]);
        }

        /// <summary>
        /// A copy facing a street: its stoop <paramref name="setback"/> metres back from the road's edge
        /// (<paramref name="edge"/>, the kerb on the house's side), centred on <paramref name="along"/>.
        /// </summary>
        public static Placement Place(Style style, float edge, float along, bool northSouth, float side, float setback)
        {
            // The house's street side (local +X) faces the road: back across it from the side the house is on.
            Vector3 facing = northSouth ? new Vector3(-side, 0f, 0f) : new Vector3(0f, 0f, -side);
            float yaw = Mathf.Atan2(-facing.z, facing.x) * Mathf.Rad2Deg;
            Quaternion turn = Quaternion.Euler(0f, yaw, 0f);
            float mirror = style.Mirrored ? -1f : 1f, z0 = mirror * K.SideMin, z1 = mirror * (style.Garage ? K.GarageSideMax : K.SideMax);
            float lo = Mathf.Min(z0, z1), hi = Mathf.Max(z0, z1);
            float line = edge + side * setback;
            Vector3 front = northSouth ? new Vector3(line, 0f, along) : new Vector3(along, 0f, line);
            Vector3 position = front - turn * new Vector3(K.FrontX, 0f, (lo + hi) * .5f);
            Vector3 a = position + turn * new Vector3(K.BackX, 0f, lo), b = position + turn * new Vector3(K.FrontX, 0f, hi);
            var footprint = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.z, b.z), Mathf.Max(a.x, b.x), Mathf.Max(a.z, b.z));
            return new Placement(position, yaw, setback, footprint);
        }

        public static void Build(Transform parent, K.Kit kit, Style style, Placement place, bool solid, TutorialHouseArt art)
        {
            Transform house = Group("StreetHouse", parent);
            house.SetPositionAndRotation(place.Position, Quaternion.Euler(0f, place.Yaw, 0f));
            // Mirrored houses are drawn through a flipped child; colliders never sit under it (a box can't be negative).
            Transform look = Group("Look", house);
            float mirror = style.Mirrored ? -1f : 1f;
            look.localScale = new Vector3(1f, 1f, mirror);

            var paint = new Dictionary<Material, Material>
            {
                { art.Siding, style.Siding }, { art.SidingDark, style.Shadow }, { art.Shingles, style.Shingles },
                { art.FrontDoor, style.Door }, { art.Trim, style.Trim },
            };
            var near = new List<Renderer> { Draw("House", look, kit.House, Vector3.zero, paint) };
            var far = new List<Renderer> { Draw("HouseFar", look, kit.HouseFar, Vector3.zero, paint) };
            if (style.Garage)
            {
                near.Add(Draw("Garage", look, kit.Garage, Vector3.zero, paint));
                far.Add(Draw("GarageFar", look, kit.GarageFar, Vector3.zero, paint));
            }
            void Both(Renderer r) { near.Add(r); far.Add(r); }
            if (style.Hood) Both(Draw("PorchHood", look, kit.Canopy, Vector3.zero, paint));
            if (style.Chimney >= 0) Both(Draw("Chimney", look, kit.Chimney, ChimneySpots[style.Chimney], paint));
            if (style.Wreath)
                near.Add(Box("Wreath", look, new Vector3(L.HouseEast + .07f, 1.65f, L.FrontDoorZ + L.FrontDoorWidth * .25f), new Vector3(.05f, .42f, .42f), art.TreeGreen, false).GetComponent<Renderer>());
            if (style.ShrubSouth) Shrub(look, new Vector3(L.HouseEast + .8f, L.Grade, L.Porch.yMin - 1.3f), art, near);
            if (style.ShrubNorth) Shrub(look, new Vector3(L.HouseEast + .8f, L.Grade, L.Porch.yMax + 1.3f), art, near);

            // Up close everything; from down the road the plain shell (the fog has the rest).
            var lod = house.gameObject.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(.25f, near.ToArray()), new LOD(.004f, far.ToArray()) });
            lod.RecalculateBounds();

            Ground(look, style, place.Setback, art);
            if (solid) Solid(house, style, mirror);
        }

        private static Renderer Draw(string name, Transform parent, TutorialMeshMerge.Merged piece, Vector3 at, Dictionary<Material, Material> paint)
        {
            Transform t = Group(name, parent);
            t.localPosition = at;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = piece.Mesh;
            var renderer = t.gameObject.AddComponent<MeshRenderer>();
            var materials = new Material[piece.Materials.Length];
            for (int i = 0; i < materials.Length; i++)
                materials[i] = paint.TryGetValue(piece.Materials[i], out Material repainted) ? repainted : piece.Materials[i];
            renderer.sharedMaterials = materials;
            return renderer;
        }

        private static void Shrub(Transform parent, Vector3 at, TutorialHouseArt art, List<Renderer> near)
        {
            Transform bush = Group("Shrub", parent);
            bush.localPosition = at;
            near.Add(Primitive(PrimitiveType.Sphere, "Crown", bush, new Vector3(0f, .45f, 0f), new Vector3(1.1f, .8f, 1.1f), art.Pine, false).GetComponent<Renderer>());
            near.Add(Primitive(PrimitiveType.Sphere, "SnowCap", bush, new Vector3(0f, .72f, 0f), new Vector3(1f, .4f, 1f), art.Snow, false).GetComponent<Renderer>());
        }

        // The drive (to the garage door, or a parking pad beside the band) and the front walk, both to the road,
        // flat on the snow: the town's land is what you stand on.
        private static void Ground(Transform look, Style style, float setback, TutorialHouseArt art)
        {
            float road = K.FrontX + setback, y = L.Grade + .007f;
            float driveFrom = style.Garage ? K.Garage.xMax : 20.5f, driveZ = style.Garage ? K.Garage.center.y : 16.2f, driveWidth = style.Garage ? 5.4f : 3.4f;
            Box("Drive", look, new Vector3((driveFrom + road) * .5f, y, driveZ), new Vector3(road - driveFrom, .012f, driveWidth), art.Asphalt, false);
            Box("FrontWalk", look, new Vector3((K.FrontX + road) * .5f, y + .001f, L.FrontDoorZ), new Vector3(setback, .012f, 1.2f), art.Sidewalk, false);
        }

        // Only near the crew: a block for each wing, the garage and the hood's posts, wall face to wall face and up to
        // the fascia, and the stoop to stand on. Mirrored by hand, as this group is never flipped.
        private static void Solid(Transform house, Style style, float mirror)
        {
            Transform solid = Group("Solid", house);
            const float wall = L.ExteriorWall * .5f, top = L.Ceiling + .6f;
            var blocks = new List<(Rect area, float top)>
            {
                (Grow(L.WestWing, wall), top), (Grow(L.Band, wall), top), (Grow(L.Suite, wall), top),
            };
            if (style.Garage) blocks.Add((Grow(K.Garage, wall), K.GarageWallTop + .5f));
            if (style.Hood)
                foreach (float z in K.HoodPostsZ)
                    blocks.Add((Rect.MinMaxRect(K.HoodPostX - .07f, z - .07f, K.HoodPostX + .07f, z + .07f), 2.75f));
            foreach ((Rect area, float height) in blocks)
            {
                var box = solid.gameObject.AddComponent<BoxCollider>();
                box.center = new Vector3(area.center.x, (L.Grade + height) * .5f, mirror * area.center.y);
                box.size = new Vector3(area.width, height - L.Grade, area.height);
            }
            solid.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup(
                "A house on the street, shut up for the winter: one block per wing so nobody walks in (its walls are one merged render mesh).");

            Transform stoop = Group("Stoop", house);
            Rect s = Rect.MinMaxRect(L.Porch.xMin + wall, L.Porch.yMin, L.Porch.xMax, L.Porch.yMax);
            var step = stoop.gameObject.AddComponent<BoxCollider>();
            step.center = new Vector3(s.center.x, L.Floor - .2f, mirror * s.center.y);
            step.size = new Vector3(s.width, .4f, s.height);
            stoop.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            stoop.gameObject.AddComponent<IntentionalInvisibleCollider>().EditorSetup("The stoop of a house on the street (drawn by its merged mesh).");
        }

        private static Rect Grow(Rect r, float by) => Rect.MinMaxRect(r.xMin - by, r.yMin - by, r.xMax + by, r.yMax + by);
    }
}

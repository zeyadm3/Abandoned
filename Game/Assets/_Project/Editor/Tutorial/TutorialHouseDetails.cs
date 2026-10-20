using System.Linq;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.TutorialHouseModels;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted visual detail stays on furniture and walls, keeping every established hauling route clear.</summary>
    public static class TutorialHouseDetails
    {
        public static void Rooms(Transform rooms, TutorialHouseArt art)
        {
            foreach (Transform room in rooms)
            foreach (Transform piece in room.Cast<Transform>().ToArray())
            {
                string name = piece.name;
                if (name == "Sofa") M.Replace(piece, "Sofa", art);
                else if (name.StartsWith("Armchair") || name == "ReadingChair") M.Replace(piece, "Armchair", art);
                else if (name.Contains("Bed") && !name.Contains("Boxes") && !name.Contains("Rug"))
                    M.Replace(piece, "Bed", art, new Vector3(name == "TwinBed" ? 2f / 3f : 1, 1, 1), room.name == "BedroomB" ? art.BeddingB : art.Bedding);
                else if (name == "Toilet") M.Replace(piece, "Toilet", art);
                else if (name == "Tub") M.Replace(piece, "Tub", art);
                else if (name.StartsWith("Nightstand")) M.Replace(piece, "Nightstand", art);
                else if (name is "Dresser" or "Sideboard")
                {
                    float width = piece.GetComponent<BoxCollider>().size.x;
                    M.Replace(piece, "Dresser", art, new Vector3(width, 1, 1));
                }
                else if (name == "DiningSet")
                    foreach (Transform chair in piece.Cast<Transform>().Where(t => t.name == "DiningChair").ToArray()) M.Replace(chair, "Chair", art);
                else if (name == "Desk")
                {
                    Transform chair = piece.Find("DeskChair");
                    if (chair != null) M.Replace(chair, "Chair", art);
                }
                else if (name.StartsWith("Counter_") || name == "Island") Counter(piece, art);
                else if (name == "Vanity") Vanity(piece, art);
                else if (name == "ChristmasTree")
                {
                    Remove(piece, "Boughs", "Trunk");
                    M.Place("TreeFoliage", piece, Vector3.zero, 0, art);
                }
                else if (name.Contains("Rug") || name == "Runner")
                {
                    // Rugs are scaled unit cubes: these child strips form an inset woven border in that space.
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box("WovenBorder", piece, new Vector3(side*.46f, .60f, 0), new Vector3(.015f, .1f, .93f), art.Curtain, false);
                        Box("WovenBorder", piece, new Vector3(0, .60f, side*.46f), new Vector3(.93f, .1f, .015f), art.Curtain, false);
                    }
                }
            }
            // Small objects occupy only existing support surfaces, away from the lesson's salvage landing points.
            On(rooms, "Living/CoffeeTable", "Books", new Vector3(-.30f, .42f, .05f), -8, art);
            On(rooms, "Dining/Sideboard", "Crockery", new Vector3(-.45f, .85f, 0), 0, art);
            On(rooms, "BedroomA/Desk", "Books", new Vector3(.30f, .76f, -.06f), 8, art);
            On(rooms, "Master/Dresser", "Books", new Vector3(-.42f, .85f, 0), -5, art);
            On(rooms, "Kitchen/Counter_North", "Crockery", new Vector3(1.25f, .925f, 0), 12, art);
            On(rooms, "Kitchen/Island", "Crockery", new Vector3(.6f, .925f, 0), 5, art);
            On(rooms, "HallBath/Tub", "Curtain", new Vector3(0, .70f, .39f), 0, art, new Vector3(.35f, .27f, .7f));
            On(rooms, "Hallway", "Picture", new Vector3(5.71f, 1.8f, 7.3f), 90, art);
            On(rooms, "Living", "Picture", new Vector3(25.2f, 1.8f, 6.60f), 0, art, new Vector3(1.5f, 1.5f, 1));
            On(rooms, "BedroomA", "Picture", new Vector3(4.6f, 1.8f, 17.36f), 180, art);
            On(rooms, "Master", "Picture", new Vector3(19f, 1.8f, .14f), 0, art, new Vector3(1.3f, 1.3f, 1));
        }

        private static void Counter(Transform piece, TutorialHouseArt art)
        {
            float width = piece.GetComponent<BoxCollider>().size.x;
            bool sink = piece.Find("SinkBasin") != null;
            bool upper = piece.Find("UpperCabinets") != null;
            Remove(piece, "DoorLine", "UpperDoorLine", "Pull", "UpperCabinets");
            int count = Mathf.Max(1, Mathf.RoundToInt(width / .6f));
            float doorWidth = width / count;
            for (int i = 0; i < count; i++)
            {
                float x = -width * .5f + (i + .5f) * doorWidth;
                M.Place("CabinetDoor", piece, new Vector3(x, .48f, .307f), 0, art, new Vector3((doorWidth-.012f)/.55f, 1, 1));
                // Leave the north sink window clear rather than running the upper cabinets across its glass.
                float worldX = piece.TransformPoint(new Vector3(x, 0, 0)).x;
                if (upper && (!sink || worldX + doorWidth*.5f < 9.50f || worldX - doorWidth*.5f > 11.10f))
                {
                    Box("UpperCarcass", piece, new Vector3(x, 1.85f, -.14f), new Vector3(doorWidth-.012f, .72f, .34f), art.Cabinet, false);
                    M.Place("CabinetDoor", piece, new Vector3(x, 1.85f, .042f), 0, art, new Vector3((doorWidth-.025f)/.55f, 1.06f, 1));
                    Box("Cornice", piece, new Vector3(x, 2.225f, -.13f), new Vector3(doorWidth, .035f, .38f), art.OakDark, false);
                }
            }
            if (piece.name == "Island") Remove(piece, "Backsplash");
            if (!sink) return;
            Remove(piece, "SinkBasin", "Faucet", "Spout", "Countertop", "BaseCabinets");
            Carcass(piece, width, .58f, .86f, art);
            CutTop(piece, width+.02f, .64f, .9f, .72f, .42f, art);
            M.Place("Sink", piece, new Vector3(0, .92f, 0), 0, art);
        }

        private static void Vanity(Transform piece, TutorialHouseArt art)
        {
            float width = piece.GetComponent<BoxCollider>().size.x;
            Remove(piece, "Basin", "Tap", "Top", "Cabinet");
            Carcass(piece, width-.02f, .5f, .8f, art);
            CutTop(piece, width, .54f, .83f, .45f, .34f, art);
            M.Place("VanityBasin", piece, new Vector3(0, .855f, 0), 0, art);
            foreach (float x in new[] { -width*.24f, width*.24f })
                M.Place("CabinetDoor", piece, new Vector3(x, .4f, .267f), 0, art, new Vector3(width*.46f/.55f, .95f, 1));
            foreach (float x in new[] { -width*.45f, width*.45f })
                Box("MirrorFrame", piece, new Vector3(x, 1.55f, -.228f), new Vector3(.025f, .82f, .035f), art.Chrome, false);
            foreach (float y in new[] { 1.15f, 1.95f })
                Box("MirrorFrame", piece, new Vector3(0, y, -.228f), new Vector3(width*.9f, .025f, .035f), art.Chrome, false);
        }

        private static void Carcass(Transform piece, float width, float depth, float height, TutorialHouseArt art)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Box("CabinetSide", piece, new Vector3(side*(width*.5f-.015f), height*.5f, 0), new Vector3(.03f, height, depth), art.Cabinet, false);
                Box("CabinetFace", piece, new Vector3(0, height*.5f, side*(depth*.5f-.015f)), new Vector3(width, height, .03f), art.Cabinet, false);
            }
            Box("CabinetBottom", piece, new Vector3(0, .12f, 0), new Vector3(width, .03f, depth), art.Cabinet, false);
        }

        private static void CutTop(Transform parent, float width, float depth, float y, float holeWidth, float holeDepth, TutorialHouseArt art)
        {
            float side = (width-holeWidth)*.5f, end = (depth-holeDepth)*.5f;
            foreach (float sign in new[] { -1f, 1f })
            {
                Box("CounterSide", parent, new Vector3(sign*(holeWidth+side)*.5f, y, .02f), new Vector3(side, .04f, depth), art.Laminate, false);
                Box("CounterEnd", parent, new Vector3(0, y, .02f+sign*(holeDepth+end)*.5f), new Vector3(holeWidth, .04f, end), art.Laminate, false);
            }
        }

        public static void Architecture(Transform root, TutorialHouseArt art)
        {
            Transform details = Group("HouseDetails", root.Find("Walls"));
            foreach (var wall in TutorialHouseWalls.OuterWalls())
            {
                float inside = wall.At-wall.Outward*(L.ExteriorWall*.5f+.014f);
                TutorialHouseWalls.Run(details, "Skirting", wall.Along, inside, wall.From, wall.To, 0, .13f, .028f, art.Trim, wall.Holes, false);
                TutorialHouseWalls.Run(details, "SkirtingCap", wall.Along, inside-.009f*wall.Outward, wall.From, wall.To, .12f, .145f, .032f, art.Trim, wall.Holes, false);
                TutorialHouseWalls.Run(details, "CeilingCove", wall.Along, inside, wall.From, wall.To, 2.94f, 3f, .065f, art.Trim, null, false);
                foreach (var hole in wall.Holes)
                {
                    if (!hole.Window || hole.Bottom > 1.2f || wall.Name == "Band_North" && hole.From < 19) continue;
                    float yaw = wall.Along ? (wall.Outward > 0 ? 180 : 0) : (wall.Outward > 0 ? -90 : 90);
                    float width = hole.To-hole.From;
                    Vector3 center = wall.Along ? new Vector3((hole.From+hole.To)*.5f, hole.Top+.12f, inside-wall.Outward*.12f)
                        : new Vector3(inside-wall.Outward*.12f, hole.Top+.12f, (hole.From+hole.To)*.5f);
                    Transform fitting = Group("CurtainPair", details);
                    fitting.SetPositionAndRotation(center, Quaternion.Euler(0, yaw, 0));
                    Box("Rod", fitting, Vector3.zero, new Vector3(width+.5f, .025f, .025f), art.Brass, false);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        M.Place("Curtain", fitting, new Vector3(side*(width*.5f+.05f), -.73f, .025f), 0, art, new Vector3(.35f, .95f, 1));
                        Primitive(PrimitiveType.Sphere, "Finial", fitting, new Vector3(side*(width*.5f+.27f), 0, 0), Vector3.one*.06f, art.Brass, false);
                    }
                }
            }
            Wall(details, "Vent", new Vector3(22.4f, .24f, 6.59f), 0, art);
            Wall(details, "Vent", new Vector3(5.69f, .24f, 7.4f), 90, art);
            Wall(details, "Outlet", new Vector3(24.3f, .32f, 13.85f), 180, art);
            Wall(details, "Outlet", new Vector3(18.8f, .32f, .145f), 0, art);
            Wall(details, "Outlet", new Vector3(.145f, .32f, 16.2f), 90, art);
            Wall(details, "Outlet", new Vector3(12f, 1.13f, 13.85f), 180, art);
            Wall(details, "Radiator", new Vector3(4.55f, .02f, 17.32f), 180, art);
            Wall(details, "Radiator", new Vector3(22.1f, .02f, .18f), 0, art);
        }

        private static void Wall(Transform parent, string key, Vector3 at, float yaw, TutorialHouseArt art)
            => M.Place(key, parent, at, yaw, art);

        private static void On(Transform root, string path, string key, Vector3 at, float yaw, TutorialHouseArt art, Vector3? scale = null)
        {
            Transform parent = root.Find(path);
            if (parent != null) M.Place(key, parent, at, yaw, art, scale);
        }

        private static void Remove(Transform parent, params string[] names)
        {
            foreach (Transform child in parent.Cast<Transform>().Where(t => names.Contains(t.name)).ToArray()) Object.DestroyImmediate(child.gameObject);
        }
    }
}

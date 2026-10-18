using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// L4: the factory's clutter, built from original low-poly parts: oil drums (standing and toppled), scrap heaps
    /// bristling with bar ends, crates, tool carts, gas bottles, broken pallets and slag lumps on the floor, chains
    /// hanging from the steel overhead and cables sagging along the walls. Solid props get one plain collider
    /// (monsters path round them, players can hop the low ones); floor litter and anything overhead has none.
    /// </summary>
    public sealed class IronFactoryProps
    {
        public enum Kind { Drums, ToppledDrum, ScrapHeap, Crates, ToolCart, GasBottles, BrokenPallet, SlagLump }

        private readonly IronFactoryArt art;
        private readonly Material red, blue, black, wood, scrap;

        public IronFactoryProps(IronFactoryArt art)
        {
            this.art = art;
            red = GetMaterial("IronFactory_DrumRed", new Color(.36f, .09f, .06f));
            blue = GetMaterial("IronFactory_DrumBlue", new Color(.1f, .16f, .26f));
            black = GetMaterial("IronFactory_DrumBlack", new Color(.06f, .06f, .065f));
            wood = IronFactoryMaterialKit.FromMall("IronFactory_Wood", "Wood", new Color(.7f, .62f, .5f));
            scrap = GetMaterial("IronFactory_Scrap", new Color(.13f, .1f, .08f));
        }

        /// <summary>Footprint (x, z) and height of each kind, for the placement checks.</summary>
        public static Vector3 Size(Kind kind) => kind switch
        {
            Kind.Drums => new Vector3(1.4f, .95f, 1.4f),
            Kind.ToppledDrum => new Vector3(1f, .65f, .7f),
            Kind.ScrapHeap => new Vector3(2.4f, .5f, 1.8f),
            Kind.Crates => new Vector3(1.8f, 1.7f, 1f),
            Kind.ToolCart => new Vector3(1f, .95f, .65f),
            Kind.GasBottles => new Vector3(.9f, 1.5f, .4f),
            Kind.BrokenPallet => new Vector3(1.2f, .14f, 1f),
            _ => new Vector3(.6f, .3f, .5f),
        };

        /// <summary>Floor litter: no collider, walked over.</summary>
        public static bool Solid(Kind kind) => kind is not (Kind.BrokenPallet or Kind.SlagLump);

        public Transform Build(Transform parent, Kind kind, Vector3 at, float yaw, System.Random rng)
        {
            Transform root = Group(kind.ToString(), parent);
            root.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
            Material paint = rng.Next(3) switch { 0 => red, 1 => blue, _ => black };
            switch (kind)
            {
                case Kind.Drums:
                    int count = 2 + rng.Next(3);
                    for (int i = 0; i < count; i++)
                    {
                        var p = new Vector3((i % 2) * .62f - .31f, 0f, (i / 2) * .62f - .31f);
                        Drum(root, p, i % 2 == 0 ? paint : art.Rust);
                    }
                    break;
                case Kind.ToppledDrum:
                    GameObject lying = Primitive(PrimitiveType.Cylinder, "Drum", root, new Vector3(0f, .3f, 0f), new Vector3(.6f, .45f, .6f), paint, false);
                    lying.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Primitive(PrimitiveType.Cylinder, "Spill", root, new Vector3(.75f, .006f, 0f), new Vector3(1.1f, .005f, .8f), art.Oil, false);
                    break;
                case Kind.ScrapHeap:
                    PolishAssets.Shape("Heap", root, new Vector3(0f, .12f, 0f), new Vector3(2.2f, .5f, 1.6f), scrap, true);
                    for (int i = 0; i < 6; i++)
                    {
                        GameObject bar = Box("Bar", root, new Vector3(R(-.8f, .8f), R(.2f, .45f), R(-.6f, .6f)), new Vector3(.05f, .05f, R(.8f, 1.6f)), i % 2 == 0 ? art.Rust : art.Steel, false);
                        bar.transform.localRotation = Quaternion.Euler(R(-35f, 35f), R(0f, 360f), R(-20f, 20f));
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject plate = Box("Plate", root, new Vector3(R(-.6f, .6f), R(.3f, .4f), R(-.4f, .4f)), new Vector3(R(.4f, .7f), .02f, R(.3f, .6f)), art.Rust, false);
                        plate.transform.localRotation = Quaternion.Euler(R(-25f, 25f), R(0f, 360f), R(-25f, 25f));
                    }
                    break;
                case Kind.Crates:
                    Box("Crate", root, new Vector3(-.45f, .4f, 0f), new Vector3(.8f, .8f, .8f), wood, false);
                    Box("Crate", root, new Vector3(.45f, .35f, 0f), new Vector3(.75f, .7f, .75f), wood, false);
                    GameObject top = Box("Crate", root, new Vector3(-.4f, 1.2f, .05f), new Vector3(.7f, .7f, .7f), wood, false);
                    top.transform.localRotation = Quaternion.Euler(0f, R(-15f, 15f), 0f);
                    break;
                case Kind.ToolCart:
                    Box("Body", root, new Vector3(0f, .55f, 0f), new Vector3(.9f, .5f, .55f), paint, false);
                    Box("Tray", root, new Vector3(0f, .86f, 0f), new Vector3(.95f, .06f, .6f), art.Steel, false);
                    Box("Handle", root, new Vector3(.5f, .95f, 0f), new Vector3(.04f, .04f, .5f), art.Steel, false);
                    foreach (float x in new[] { -.35f, .35f })
                    foreach (float z in new[] { -.22f, .22f })
                        Primitive(PrimitiveType.Cylinder, "Wheel", root, new Vector3(x, .1f, z), new Vector3(.18f, .03f, .18f), art.Bed, false)
                            .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                case Kind.GasBottles:
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject bottle = Primitive(PrimitiveType.Cylinder, "Bottle", root, new Vector3(-.3f + i * .3f, .72f, 0f), new Vector3(.24f, .7f, .24f), i == 1 ? black : paint, false);
                        bottle.transform.localRotation = Quaternion.Euler(R(-4f, 4f), 0f, R(-6f, 6f));
                    }
                    Box("Strap", root, new Vector3(0f, 1.05f, -.14f), new Vector3(.95f, .05f, .03f), art.Steel, false);
                    break;
                case Kind.BrokenPallet:
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject board = Box("Board", root, new Vector3(R(-.3f, .3f), .04f + i * .025f, -.4f + i * .27f), new Vector3(R(.7f, 1.2f), .025f, .12f), wood, false);
                        board.transform.localRotation = Quaternion.Euler(0f, R(-25f, 25f), R(-6f, 6f));
                    }
                    break;
                default:
                    PolishAssets.Shape("Slag", root, new Vector3(0f, .12f, 0f), new Vector3(R(.4f, .6f), R(.2f, .3f), R(.35f, .5f)), scrap, true);
                    break;
            }
            if (Solid(kind))
            {
                Vector3 s = Size(kind);
                var solid = root.gameObject.AddComponent<BoxCollider>();
                solid.center = Vector3.up * s.y * .5f;
                solid.size = new Vector3(s.x * .85f, s.y, s.z * .85f);
                root.gameObject.AddComponent<SurfaceTag>().EditorSet(kind == Kind.Crates ? SurfaceMaterial.Wood : SurfaceMaterial.Metal);
            }
            return root;
        }

        private void Drum(Transform parent, Vector3 at, Material paint)
        {
            Primitive(PrimitiveType.Cylinder, "Drum", parent, at + Vector3.up * .45f, new Vector3(.58f, .45f, .58f), paint, false);
            Primitive(PrimitiveType.Cylinder, "Rib", parent, at + Vector3.up * .3f, new Vector3(.6f, .015f, .6f), art.Rust, false);
            Primitive(PrimitiveType.Cylinder, "Rib", parent, at + Vector3.up * .6f, new Vector3(.6f, .015f, .6f), art.Rust, false);
        }

        /// <summary>A chain hanging straight down from <paramref name="top"/>, ending in a hook.</summary>
        public void Chain(Transform parent, Vector3 top, float length)
        {
            Transform chain = Group("HangingChain", parent);
            chain.position = top;
            Box("Links", chain, Vector3.down * length * .5f, new Vector3(.05f, length, .05f), art.Steel, false);
            Box("Hook", chain, Vector3.down * (length + .15f), new Vector3(.08f, .3f, .14f), art.Rust, false);
        }

        /// <summary>A cable sagging between two points: a few straight runs along a catenary-ish curve.</summary>
        public void Cable(Transform parent, Vector3 a, Vector3 b, float sag)
        {
            Transform cable = Group("Cable", parent);
            const int runs = 8;
            Vector3 Point(int i)
            {
                float t = i / (float)runs;
                return Vector3.Lerp(a, b, t) + Vector3.down * sag * 4f * t * (1f - t);
            }
            for (int i = 0; i < runs; i++)
            {
                Vector3 p = Point(i), q = Point(i + 1);
                GameObject run = Box("Run", cable, (p + q) * .5f, new Vector3(.05f, .05f, Vector3.Distance(p, q) + .02f), art.Bed, false);
                run.transform.rotation = Quaternion.LookRotation(q - p);
            }
        }
    }
}

using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Simple dynamic compounds leave the gaps around legs, lamp stems and cart frames open.</summary>
    internal static class SalvagePickupGeometry0157
    {
        public static void AddColliders(Transform root, string id, Bounds bounds, PhysicsMaterial friction)
        {
            if (id is not ("showroom_sofa" or "floor_lamp" or "mobile_planning_board"
                or "welding_trolley" or "recovery_cart"))
            {
                SalvagePrefabGeometry.AddColliders(root, id, bounds, dynamic: true, friction: friction);
                return;
            }
            var holder = new GameObject("_CollisionProxies");
            holder.transform.SetParent(root, false);
            Vector3 c = bounds.center, s = bounds.size;
            float floor = bounds.min.y;
            switch (id)
            {
                case "showroom_sofa":
                    Box(holder.transform, "SeatAndBase", new(c.x, floor + s.y * .295f, c.z),
                        new(s.x, s.y * .415f, s.z), friction);
                    Box(holder.transform, "Backrest", new(c.x, floor + s.y * .645f, c.z + s.z * .382f),
                        new(s.x * .98f, s.y * .71f, s.z * .237f), friction);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box(holder.transform, "Arm", new(c.x + side * s.x * .451f, floor + s.y * .492f, c.z),
                            new(s.x * .109f, s.y * .503f, s.z * .96f), friction);
                        foreach (float end in new[] { -1f, 1f })
                            Box(holder.transform, "Foot", new(c.x + side * s.x * .4f,
                                floor + s.y * .0765f, c.z + end * s.z * .342f),
                                new(s.x * .051f, s.y * .153f, s.z * .118f), friction);
                    }
                    break;
                case "floor_lamp":
                    Box(holder.transform, "WeightedBase", new(c.x, floor + .035f, c.z),
                        new(s.x, .07f, s.z), friction);
                    Box(holder.transform, "Upright", new(c.x, floor + s.y * .478f, c.z),
                        new(.038f, s.y * .895f, .038f), friction);
                    Box(holder.transform, "Shade", new(c.x, floor + s.y * .925f, c.z),
                        new(s.x * .95f, s.y * .15f, s.z * .95f), friction);
                    break;
                case "mobile_planning_board":
                    Box(holder.transform, "Board", new(c.x, floor + s.y * .725f, c.z),
                        new(s.x, s.y * .55f, .045f), friction);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Box(holder.transform, "Support", new(c.x + side * s.x * .43f, floor + s.y * .443f, c.z),
                            new(.04f, s.y * .887f, .04f), friction);
                        Box(holder.transform, "Stabiliser", new(c.x + side * s.x * .43f, floor + .065f, c.z),
                            new(.08f, .08f, s.z), friction);
                    }
                    break;
                case "welding_trolley":
                case "recovery_cart":
                    Cart(holder.transform, c, s, floor, friction);
                    if (id == "welding_trolley")
                    {
                        Box(holder.transform, "Welder", new(c.x - s.x * .156f, floor + s.y * .488f, c.z - .05f),
                            new(s.x * .594f, s.y * .268f, s.z * .84f), friction);
                        var cylinder = new GameObject("GasBottle");
                        cylinder.transform.SetParent(holder.transform, false);
                        cylinder.transform.localPosition = new(c.x + s.x * .266f, floor + s.y * .607f, c.z + .06f);
                        var capsule = cylinder.AddComponent<CapsuleCollider>();
                        capsule.direction = 1;
                        capsule.height = s.y * .79f;
                        capsule.radius = .10f;
                        capsule.sharedMaterial = friction;
                    }
                    else
                        Box(holder.transform, "LoadedTray", new(c.x, floor + s.y * .817f, c.z),
                            new(s.x * .94f, s.y * .22f, s.z * .93f), friction);
                    break;
            }
        }

        private static void Cart(Transform parent, Vector3 c, Vector3 s, float floor, PhysicsMaterial friction)
        {
            Box(parent, "LowerTray", new(c.x, floor + .22f, c.z), new(s.x, .08f, s.z), friction);
            foreach (float side in new[] { -1f, 1f })
            {
                Box(parent, "FrameUpright", new(c.x + side * (s.x * .5f - .04f), floor + .60f, c.z + s.z * .41f),
                    new(.035f, 1f, .035f), friction);
                foreach (float end in new[] { -1f, 1f })
                    Box(parent, "Caster", new(c.x + side * s.x * .37f, floor + .08f, c.z + end * s.z * .35f),
                        new(.16f, .16f, .045f), friction);
            }
            Box(parent, "PushBar", new(c.x, floor + 1.04f, c.z + s.z * .41f),
                new(s.x, .035f, .04f), friction);
        }

        private static void Box(Transform parent, string name, Vector3 centre, Vector3 size, PhysicsMaterial friction)
        {
            var proxy = new GameObject(name);
            proxy.transform.SetParent(parent, false);
            proxy.transform.localPosition = centre;
            var collider = proxy.AddComponent<BoxCollider>();
            collider.size = size;
            collider.sharedMaterial = friction;
        }
    }
}

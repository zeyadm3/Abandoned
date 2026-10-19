using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the worker's 9-part ragdoll (pelvis, chest, head, arms, thighs, shins). Each part is an unscaled root with its own collider and a
    /// scaled visual child, so joint anchors are in metres. Positions are relative to the feet.
    /// </summary>
    public static class PlayerRagdollBuilder
    {
        public readonly struct Result
        {
            public readonly Transform Root;
            public readonly Rigidbody Pelvis;
            public readonly Transform Head;

            public Result(Transform root, Rigidbody pelvis, Transform head)
            {
                Root = root;
                Pelvis = pelvis;
                Head = head;
            }
        }

        public static Result Build(Transform player, float totalMass, Material material)
        {
            Transform root = GreyboxFactory.Group("Ragdoll", player);

            Rigidbody pelvis = BoxPart(root, "Pelvis", new(0f, 0.95f, 0f), new(0.34f, 0.22f, 0.22f), 0.20f, totalMass, material);
            Rigidbody chest = BoxPart(root, "Chest", new(0f, 1.3f, 0f), new(0.4f, 0.45f, 0.24f), 0.25f, totalMass, material);
            Rigidbody head = SpherePart(root, "Head", new(0f, 1.6f, 0f), 0.13f, 0.08f, totalMass, WorkerVisualBuilder.Skin);
            Rigidbody armL = CapsulePart(root, "ArmL", new(-0.29f, 1.22f, 0f), 0.06f, 0.6f, 0.05f, totalMass, material);
            Rigidbody armR = CapsulePart(root, "ArmR", new(0.29f, 1.22f, 0f), 0.06f, 0.6f, 0.05f, totalMass, material);
            Rigidbody thighL = CapsulePart(root, "ThighL", new(-0.1f, 0.63f, 0f), 0.08f, 0.44f, 0.10f, totalMass, material);
            Rigidbody thighR = CapsulePart(root, "ThighR", new(0.1f, 0.63f, 0f), 0.08f, 0.44f, 0.10f, totalMass, material);
            Rigidbody shinL = CapsulePart(root, "ShinL", new(-0.1f, 0.21f, 0f), 0.07f, 0.42f, 0.06f, totalMass, material);
            Rigidbody shinR = CapsulePart(root, "ShinR", new(0.1f, 0.21f, 0f), 0.07f, 0.42f, 0.06f, totalMass, material);

            Dress(player, pelvis, chest, head, armL, armR, thighL, thighR, shinL, shinR);

            // Anchors sit at the top of each child part, where it hangs from its parent.
            Joint(chest, pelvis, new(0f, -0.22f, 0f), 15f, 25f);
            Joint(head, chest, new(0f, -0.12f, 0f), 30f, 40f);
            Joint(armL, chest, new(0f, 0.28f, 0f), 40f, 80f);
            Joint(armR, chest, new(0f, 0.28f, 0f), 40f, 80f);
            Joint(thighL, pelvis, new(0f, 0.21f, 0f), 20f, 60f);
            Joint(thighR, pelvis, new(0f, 0.21f, 0f), 20f, 60f);
            Joint(shinL, thighL, new(0f, 0.2f, 0f), 5f, 70f);
            Joint(shinR, thighR, new(0f, 0.2f, 0f), 5f, 70f);

            return new Result(root, pelvis, head.transform);
        }

        private static void Dress(Transform player, params Rigidbody[] bodies)
        {
            // The physics remains the same nine parts. Reuse the authored outfit so going down never changes
            // the worker into primitive capsules or loses the reinforced clothing, boots and equipment harness.
            GameObject source = WorkerVisualBuilder.Build(player);
            string[] paths =
            {
                "HipsVisual", "Torso/TorsoVisual", "Torso/Head/HeadVisual", "Torso/ArmL/ArmVisual", "Torso/ArmR/ArmVisual",
                "LegL/ThighVisual", "LegR/ThighVisual", "LegL/Knee/ShinVisual", "LegR/Knee/ShinVisual",
            };
            try
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    Transform visual = source.transform.Find(paths[i]);
                    if (visual == null) continue;
                    GameObject copy = Object.Instantiate(visual.gameObject, bodies[i].transform, true);
                    copy.name = "Visual";
                }
            }
            finally { Object.DestroyImmediate(source); }
        }

        private static Rigidbody BoxPart(Transform root, string name, Vector3 pos, Vector3 size, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            body.gameObject.AddComponent<BoxCollider>().size = size;
            return body;
        }

        private static Rigidbody SpherePart(Transform root, string name, Vector3 pos, float radius, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            body.gameObject.AddComponent<SphereCollider>().radius = radius;
            return body;
        }

        private static Rigidbody CapsulePart(Transform root, string name, Vector3 pos, float radius, float height, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            var capsule = body.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.height = height;
            capsule.direction = 1;
            return body;
        }

        private static Rigidbody Part(Transform root, string name, Vector3 pos, float massShare, float total)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            var body = go.AddComponent<Rigidbody>();
            body.mass = massShare * total;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            return body;
        }

        private static void Joint(Rigidbody child, Rigidbody parent, Vector3 anchor, float twist, float swing)
        {
            var joint = child.gameObject.AddComponent<CharacterJoint>();
            joint.connectedBody = parent;
            joint.anchor = anchor;
            joint.axis = Vector3.up;
            joint.swingAxis = Vector3.forward;
            joint.lowTwistLimit = new SoftJointLimit { limit = -twist };
            joint.highTwistLimit = new SoftJointLimit { limit = twist };
            joint.swing1Limit = new SoftJointLimit { limit = swing };
            joint.swing2Limit = new SoftJointLimit { limit = swing };
            joint.enableProjection = true;
        }
    }
}

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

            Material dark = WorkerVisualBuilder.Dark, tape = WorkerVisualBuilder.Tape;
            GreyboxFactory.Box("Zip", chest.transform, new Vector3(0f, 0f, 0.13f), new Vector3(0.015f, 0.35f, 0.015f), dark, false);
            foreach (float side in new[] { -1f, 1f })
                GreyboxFactory.Box("ReflectiveTape", chest.transform, new Vector3(side * 0.14f, 0.08f, 0.13f), new Vector3(0.055f, 0.22f, 0.02f), tape, false);
            GreyboxFactory.Box("Brow", head.transform, new Vector3(0f, 0.035f, 0.122f), new Vector3(0.18f, 0.022f, 0.015f), dark, false);
            PolishAssets.Shape("Hair", head.transform, new Vector3(0f, 0.11f, -0.015f), new Vector3(0.28f, 0.08f, 0.26f), dark, true);
            foreach (Rigidbody arm in new[] { armL, armR })
                PolishAssets.Shape("Glove", arm.transform, new Vector3(0f, -0.27f, 0.015f), new Vector3(0.14f, 0.14f, 0.17f), dark);
            foreach (Rigidbody shin in new[] { shinL, shinR })
            {
                GreyboxFactory.Box("Boot", shin.transform, new Vector3(0f, -0.13f, 0.04f), new Vector3(0.2f, 0.16f, 0.31f), dark, false);
                PolishAssets.Shape("AnkleTape", shin.transform, new Vector3(0f, -0.045f, 0f), new Vector3(0.16f, 0.035f, 0.17f), tape);
            }
            WorkerVisualBuilder.NumberBadge(chest.transform, new Vector3(0.09f, -0.045f, 0.145f));

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

        private static Rigidbody BoxPart(Transform root, string name, Vector3 pos, Vector3 size, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            body.gameObject.AddComponent<BoxCollider>().size = size;
            PolishAssets.Shape("Visual", body.transform, Vector3.zero, size, m);
            return body;
        }

        private static Rigidbody SpherePart(Transform root, string name, Vector3 pos, float radius, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            body.gameObject.AddComponent<SphereCollider>().radius = radius;
            PolishAssets.Shape("Visual", body.transform, Vector3.zero, Vector3.one * radius * 2f, m, true);
            return body;
        }

        private static Rigidbody CapsulePart(Transform root, string name, Vector3 pos, float radius, float height, float massShare, float total, Material m)
        {
            Rigidbody body = Part(root, name, pos, massShare, total);
            var capsule = body.gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.height = height;
            capsule.direction = 1;
            PolishAssets.Shape("Visual", body.transform, Vector3.zero, new Vector3(radius * 2f, height, radius * 2f), m);
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

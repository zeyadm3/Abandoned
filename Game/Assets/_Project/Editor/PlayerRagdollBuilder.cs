using System.Linq;
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

            // Sized to the 2026-10-22 crew worker: eyes at 1.65, shoulders at +-0.19, hips at +-0.102.
            Rigidbody pelvis = BoxPart(root, "Pelvis", new(0f, 0.93f, 0f), new(0.38f, 0.2f, 0.28f), 0.20f, totalMass, material);
            Rigidbody chest = BoxPart(root, "Chest", new(0f, 1.27f, 0f), new(0.4f, 0.44f, 0.32f), 0.25f, totalMass, material);
            Rigidbody head = SpherePart(root, "Head", new(0f, 1.65f, 0f), 0.11f, 0.08f, totalMass, WorkerVisualBuilder.Skin);
            Rigidbody armL = CapsulePart(root, "ArmL", new(-0.2f, 1.07f, 0f), 0.065f, 0.7f, 0.05f, totalMass, material);
            Rigidbody armR = CapsulePart(root, "ArmR", new(0.2f, 1.07f, 0f), 0.065f, 0.7f, 0.05f, totalMass, material);
            Rigidbody thighL = CapsulePart(root, "ThighL", new(-0.102f, 0.7f, 0f), 0.09f, 0.42f, 0.10f, totalMass, material);
            Rigidbody thighR = CapsulePart(root, "ThighR", new(0.102f, 0.7f, 0f), 0.09f, 0.42f, 0.10f, totalMass, material);
            Rigidbody shinL = CapsulePart(root, "ShinL", new(-0.102f, 0.26f, 0f), 0.075f, 0.5f, 0.06f, totalMass, material);
            Rigidbody shinR = CapsulePart(root, "ShinR", new(0.102f, 0.26f, 0f), 0.075f, 0.5f, 0.06f, totalMass, material);

            Dress(player, pelvis, chest, head, armL, armR, thighL, thighR, shinL, shinR);

            // Anchors sit at the top of each child part, where it hangs from its parent.
            Joint(chest, pelvis, new(0f, -0.2f, 0f), 15f, 25f);
            Joint(head, chest, new(0f, -0.13f, 0f), 30f, 40f);
            Joint(armL, chest, new(0f, 0.345f, 0f), 40f, 80f);
            Joint(armR, chest, new(0f, 0.345f, 0f), 40f, 80f);
            Joint(thighL, pelvis, new(0f, 0.2f, 0f), 20f, 60f);
            Joint(thighR, pelvis, new(0f, 0.2f, 0f), 20f, 60f);
            Joint(shinL, thighL, new(0f, 0.24f, 0f), 5f, 70f);
            Joint(shinR, thighR, new(0f, 0.24f, 0f), 5f, 70f);

            return new Result(root, pelvis, head.transform);
        }

        private static void Dress(Transform player, params Rigidbody[] bodies)
        {
            if (Remake.CrewLookBuilder.Available && WorkerVisualBuilder.HasLooks())
            {
                DressSkinned(player, bodies);
                return;
            }
            // The physics remains the same nine parts. Reuse the authored outfit so going down never changes
            // the worker into primitive capsules or loses the reinforced clothing, boots and equipment harness.
            GameObject source = WorkerVisualBuilder.Build(player);
            // Each arm body carries the upper arm and its forearm (the standing worker bends at the elbow).
            string[][] paths =
            {
                new[] { "HipsVisual" }, new[] { "Torso/TorsoVisual" }, new[] { "Torso/Head/HeadVisual" },
                new[] { "Torso/ArmL/ArmVisual", "Torso/ArmL/Elbow/ForearmVisual" }, new[] { "Torso/ArmR/ArmVisual", "Torso/ArmR/Elbow/ForearmVisual" },
                new[] { "LegL/ThighVisual" }, new[] { "LegR/ThighVisual" }, new[] { "LegL/Knee/ShinVisual" }, new[] { "LegR/Knee/ShinVisual" },
            };
            try
            {
                for (int i = 0; i < bodies.Length; i++)
                    foreach (string path in paths[i])
                    {
                        Transform visual = source.transform.Find(path);
                        if (visual == null) continue;
                        GameObject copy = Object.Instantiate(visual.gameObject, bodies[i].transform, true);
                        copy.name = "Visual";
                    }
            }
            finally { Object.DestroyImmediate(source); }
        }

        /// <summary>
        /// 2026 remake: the same skinned look as the standing worker, bound to bone helpers that sit inside the nine
        /// bodies at the standing joints' rest positions (the forearm rides on its arm body, the shin's bone on the shin),
        /// so one set of bind poses serves both.
        /// </summary>
        private static void DressSkinned(Transform player, Rigidbody[] b)
        {
            // pelvis, chest, head, armL, armR, thighL, thighR, shinL, shinR -> crew bone order.
            Rigidbody[] carriers = { b[0], b[1], b[2], b[3], b[3], b[4], b[4], b[5], b[7], b[6], b[8] };
            Transform[] owner = carriers.Select(r => r.transform).ToArray();
            var bones = new Transform[Remake.CrewLookBuilder.Bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                var helper = new GameObject("Bone_" + Remake.CrewLookBuilder.Bones[i]).transform;
                helper.SetParent(owner[i], false);
                helper.position = player.TransformPoint(Remake.CrewLookBuilder.Rest[i]);
                helper.rotation = player.rotation;
                bones[i] = helper;
            }
            Transform ragdoll = b[0].transform.parent;
            Remake.CrewLookBuilder.AddLook(ragdoll.gameObject, ragdoll, ragdoll.InverseTransformPoint(player.position), bones);
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

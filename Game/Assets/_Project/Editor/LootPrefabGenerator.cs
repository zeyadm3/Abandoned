using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds a loot prefab from a LootDefinition: placeholder visual + collider, Rigidbody with
    /// the clamped physics mass, Grabbable, LootItem and LootFeedback, and the network parts
    /// (NetworkObject, owner-authoritative NetworkTransform, NetworkLoot). No per-item code.
    /// </summary>
    public static class LootPrefabGenerator
    {
        public const string Folder = "Assets/_Project/Prefabs/Loot";
        public const string HeavyFrictionPath = LootCatalogBuilder.Folder + "/Loot_HeavyFriction.asset";
        public const string SharedCarryConfigPath = "Assets/_Project/Data/Interaction/SharedCarryConfig.asset";

        public static string PrefabPathFor(LootDefinition definition) => $"{Folder}/Loot_{definition.Id}.prefab";

        [MenuItem("Tools/Abandoned/Generate Loot Prefabs")]
        public static void GenerateAll()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:LootDefinition", new[] { LootCatalogBuilder.Folder }))
                Generate(AssetDatabase.LoadAssetAtPath<LootDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("CONTEXT/LootDefinition/Generate Prefab")]
        private static void GenerateFromContext(MenuCommand command)
        {
            Generate((LootDefinition)command.context);
            AssetDatabase.SaveAssets();
        }

        public static GameObject Generate(LootDefinition definition)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Loot");
            var damageConfig = LoadOrCreateAsset<LootDamageConfig>(LootCatalogBuilder.DamageConfigPath);
            var netConfig = LoadOrCreateAsset<LootNetConfig>(NetworkContentBuilder.LootNetConfigPath);

            var root = new GameObject($"Loot_{definition.Id}");
            // NetworkObject first: NGO expects it on the root before any NetworkBehaviour.
            var networkObject = root.AddComponent<NetworkObject>();
            // A carrier leaving the session must not take the loot with them; it returns to the host.
            networkObject.DontDestroyWithOwner = true;
            // Loot is never parented under network objects; in-scene items sit under plain group objects.
            networkObject.AutoObjectParentSync = false;
            Material material = GreyboxFactory.GetMaterial($"Loot_{definition.Id}", definition.Color);
            GameObject visual = GreyboxFactory.Primitive(ToPrimitive(definition.Shape), "Visual", root.transform, Vector3.zero,
                VisualScale(definition.Shape, definition.Size), material, withCollider: true);
            // Physics mass is clamped for stability, so Heavy/Huge items get grippy friction instead:
            // a carried laptop can't shove a 2-ton statue across the floor.
            if (definition.CarryClass >= CarryClass.Heavy)
                visual.GetComponent<Collider>().sharedMaterial = HeavyFriction();

            var body = root.AddComponent<Rigidbody>();
            body.mass = definition.PhysicsMass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            // Small, light things get thrown fast; continuous detection stops them tunnelling through floors.
            body.collisionDetectionMode = definition.CarryClass <= CarryClass.TwoHand
                ? CollisionDetectionMode.ContinuousDynamic
                : CollisionDetectionMode.Discrete;

            root.AddComponent<Grabbable>();
            // Heavy/Huge items are carried together through carry points generated from the definition.
            bool shared = definition.CarryClass >= CarryClass.Heavy;
            if (shared) Set(root.AddComponent<SharedCarryable>(), "config", LoadOrCreateAsset<SharedCarryConfig>(SharedCarryConfigPath));
            var item = root.AddComponent<LootItem>();
            Set(item, "definition", definition);
            Set(item, "damageConfig", damageConfig);
            var feedback = root.AddComponent<LootFeedback>();
            Set(feedback, "damageConfig", damageConfig);
            NetworkTransform networkTransform = AddNetworkTransform(root);
            var networkLoot = root.AddComponent<NetworkLoot>();
            Set(networkLoot, "config", netConfig);
            Set(networkLoot, "networkTransform", networkTransform);
            if (shared) root.AddComponent<NetworkSharedCarry>();

            SetLayerRecursively(root, GameLayers.LootLayer);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPathFor(definition));
            Object.DestroyImmediate(root);
            NetworkObjectIds.StampPrefab(prefab);
            return prefab;
        }

        private static NetworkTransform AddNetworkTransform(GameObject root)
        {
            var nt = root.AddComponent<NetworkTransform>();
            // Owner-authoritative: the carrier simulates while carrying, the host owns it the rest of the time.
            nt.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            nt.SyncPositionX = nt.SyncPositionY = nt.SyncPositionZ = true;
            nt.SyncRotAngleX = nt.SyncRotAngleY = nt.SyncRotAngleZ = true;
            nt.SyncScaleX = nt.SyncScaleY = nt.SyncScaleZ = false;
            nt.InLocalSpace = false;
            nt.Interpolate = true;
            return nt;
        }

        public static PhysicsMaterial HeavyFriction()
        {
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(HeavyFrictionPath);
            if (material != null) return material;
            material = new PhysicsMaterial("Loot_HeavyFriction")
            {
                staticFriction = 1.5f,
                dynamicFriction = 1.1f,
                frictionCombine = PhysicsMaterialCombine.Maximum
            };
            AssetDatabase.CreateAsset(material, HeavyFrictionPath);
            return material;
        }

        /// <summary>Errors if a generated prefab no longer matches its definition (regenerate it).</summary>
        public static void ValidatePrefab(LootDefinition definition, GameObject prefab, System.Collections.Generic.List<string> errors)
        {
            string n = $"Loot prefab {definition.Id}";
            var body = prefab.GetComponent<Rigidbody>();
            if (body == null || !Mathf.Approximately(body.mass, definition.PhysicsMass))
                errors.Add($"{n}: Rigidbody mass differs from PhysicsMass; run Generate Loot Prefabs.");
            Transform visual = prefab.transform.Find("Visual");
            if (visual == null || (visual.localScale - VisualScale(definition.Shape, definition.Size)).sqrMagnitude > 1e-6f)
                errors.Add($"{n}: visual size differs from Size; run Generate Loot Prefabs.");
            var item = prefab.GetComponent<LootItem>();
            if (item == null || item.Definition != definition)
                errors.Add($"{n}: LootItem doesn't point at its definition.");
            bool shouldShare = definition.CarryClass >= CarryClass.Heavy;
            var shared = prefab.GetComponent<SharedCarryable>();
            if (shouldShare != (shared != null) || shouldShare != (prefab.GetComponent<NetworkSharedCarry>() != null))
                errors.Add($"{n}: Heavy/Huge loot needs SharedCarryable + NetworkSharedCarry (and nothing lighter has them); run Generate Loot Prefabs.");
            else if (shared != null && Get(shared, "config") == null)
                errors.Add($"{n}: SharedCarryable has no SharedCarryConfig.");
        }

        /// <summary>Places a loot prefab instance resting on a surface at the given height.</summary>
        public static GameObject PlaceInScene(string id, Vector3 position, float yaw, float surfaceY, int seed, Transform parent)
        {
            LootDefinition definition = LootCatalogBuilder.Load(id);
            var prefab = definition != null ? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathFor(definition)) : null;
            if (prefab == null)
            {
                Debug.LogError($"No loot prefab for '{id}'; run Generate Loot Prefabs.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
            float halfHeight = definition.Size.y / 2f;
            instance.transform.SetPositionAndRotation(new Vector3(position.x, surfaceY + halfHeight + 0.02f, position.z), rotation);
            SetInt(instance.GetComponent<LootItem>(), "seed", seed);
            return instance;
        }

        private static PrimitiveType ToPrimitive(PlaceholderShape shape) => shape switch
        {
            PlaceholderShape.Cylinder => PrimitiveType.Cylinder,
            PlaceholderShape.Sphere => PrimitiveType.Sphere,
            PlaceholderShape.Capsule => PrimitiveType.Capsule,
            _ => PrimitiveType.Cube,
        };

        // Unity's cylinder and capsule are 2 units tall at scale 1; cube and sphere are 1.
        private static Vector3 VisualScale(PlaceholderShape shape, Vector3 size) =>
            shape is PlaceholderShape.Cylinder or PlaceholderShape.Capsule
                ? new Vector3(size.x, size.y / 2f, size.z)
                : size;
    }
}

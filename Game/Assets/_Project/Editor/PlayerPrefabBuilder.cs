using Abandoned.Interaction;
using Abandoned.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds Prefabs/Player.prefab: CharacterController, input reader, motor, stamina, look,
    /// debug overlay, a capsule placeholder body and a Cinemachine camera locked to the eyes.
    /// </summary>
    public static class PlayerPrefabBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Player.prefab";
        public const string ConfigPath = "Assets/_Project/Data/Player/PlayerMovementConfig.asset";
        public const string CarryConfigPath = "Assets/_Project/Data/Interaction/CarryConfig.asset";

        private const float FieldOfView = 75f;
        private const float NearClip = 0.05f;

        [MenuItem("Tools/Abandoned/Create Player Prefab")]
        public static void Create()
        {
            var config = LoadOrCreateAsset<PlayerMovementConfig>(ConfigPath);
            var carryConfig = LoadOrCreateAsset<CarryConfig>(CarryConfigPath);

            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.height = config.StandingHeight;
            controller.center = Vector3.up * (config.StandingHeight / 2f);
            controller.radius = config.Radius;
            controller.slopeLimit = config.SlopeLimit;
            controller.stepOffset = config.StepOffset;
            controller.minMoveDistance = 0f;

            Transform cameraRoot = new GameObject("CameraRoot").transform;
            cameraRoot.SetParent(root.transform, false);
            cameraRoot.localPosition = Vector3.up * config.StandingEyeHeight;

            BuildBody(root.transform, config);
            BuildCamera(root.transform, cameraRoot);

            var reader = root.AddComponent<PlayerInputReader>();
            var stamina = root.AddComponent<PlayerStamina>();
            var look = root.AddComponent<PlayerLook>();
            var motor = root.AddComponent<PlayerMotor>();
            var debug = root.AddComponent<PlayerMovementDebug>();
            var inventory = root.AddComponent<PlayerInventory>();
            var carrier = root.AddComponent<PlayerCarrier>();
            var interactor = root.AddComponent<PlayerInteractor>();
            var hud = root.AddComponent<InteractionHud>();

            Set(stamina, "config", config);
            Set(look, "config", config);
            Set(look, "inputReader", reader);
            Set(look, "cameraRoot", cameraRoot);
            Set(motor, "config", config);
            Set(motor, "inputReader", reader);
            Set(motor, "stamina", stamina);
            Set(motor, "cameraRoot", cameraRoot);
            Set(debug, "motor", motor);
            Set(debug, "stamina", stamina);
            Set(inventory, "config", carryConfig);
            Set(carrier, "config", carryConfig);
            Set(carrier, "motor", motor);
            Set(carrier, "cameraRoot", cameraRoot);
            Set(interactor, "inputReader", reader);
            Set(interactor, "carrier", carrier);
            Set(hud, "interactor", interactor);
            Set(hud, "carrier", carrier);
            Set(hud, "inputReader", reader);

            SetLayerRecursively(root, Abandoned.Core.GameLayers.PlayerLayer);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"Player prefab saved to {PrefabPath}.");
        }

        /// <summary>Places a Player prefab instance at the pose; returns null if the prefab is missing.</summary>
        public static GameObject PlaceInScene(Vector3 position, Quaternion rotation)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"{PrefabPath} not found; run Tools/Abandoned/Create Player Prefab first.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private static void BuildBody(Transform root, PlayerMovementConfig config)
        {
            // Placeholder until a humanoid model arrives; the camera sits inside it, and backface
            // culling keeps it invisible to its own player while others (and shadows) still see it.
            // A Unity capsule is 2 units tall at scale 1, so Y scale is half the height.
            GreyboxFactory.Primitive(PrimitiveType.Capsule, "Body", root,
                Vector3.up * (config.StandingHeight / 2f),
                new Vector3(config.Radius * 2f, config.StandingHeight / 2f, config.Radius * 2f),
                GreyboxFactory.GetMaterial("Greybox_Player", new Color(0.95f, 0.45f, 0.1f)), withCollider: false);
        }

        private static void BuildCamera(Transform root, Transform cameraRoot)
        {
            var go = new GameObject("PlayerCamera");
            go.transform.SetParent(root, false);
            go.transform.localPosition = cameraRoot.localPosition;

            var cam = go.AddComponent<CinemachineCamera>();
            cam.Target.TrackingTarget = cameraRoot;
            LensSettings lens = cam.Lens;
            lens.FieldOfView = FieldOfView;
            lens.NearClipPlane = NearClip;
            cam.Lens = lens;

            // Zero damping: first-person look must not lag behind the mouse.
            go.AddComponent<CinemachineHardLockToTarget>().Damping = 0f;
            go.AddComponent<CinemachineRotateWithFollowTarget>().Damping = 0f;
        }
    }
}

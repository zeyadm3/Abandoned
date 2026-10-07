using System.Collections.Generic;
using Abandoned.Player;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Abandoned.Tests
{
    /// <summary>
    /// Spawns the real Player prefab with its input and Update loops disabled, so tests drive
    /// PlayerMotor.Simulate directly with exact inputs and fixed timesteps.
    /// </summary>
    public class PlayerTestRig
    {
        public const float Dt = 1f / 60f;
        public const string PrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        private readonly List<GameObject> spawned = new();

        public GameObject Player { get; private set; }
        public PlayerMotor Motor { get; private set; }
        public PlayerStamina Stamina { get; private set; }
        public PlayerLook Look { get; private set; }
        public PlayerMovementConfig Config => Motor.Config;

        public static PlayerTestRig OnFlatGround(Vector3 position = default)
        {
            var rig = new PlayerTestRig();
            rig.AddBox("Ground", new Vector3(0f, -0.5f, 0f), new Vector3(200f, 1f, 200f));
            rig.AddLight();
            rig.SpawnPlayer(position, Quaternion.identity);
            return rig;
        }

        /// <summary>Takes control of an existing player (e.g. the one in TestMap).</summary>
        public static PlayerTestRig ForExisting(GameObject player)
        {
            var rig = new PlayerTestRig();
            rig.Bind(player);
            return rig;
        }

        public GameObject AddBox(string name, Vector3 center, Vector3 size)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = center;
            box.transform.localScale = size;
            spawned.Add(box);
            Physics.SyncTransforms();
            return box;
        }

        /// <summary>Destroys the object with the rig.</summary>
        public void Track(GameObject go) => spawned.Add(go);

        /// <summary>A sun, so screenshots taken in test scenes are readable.</summary>
        public void AddLight()
        {
            var go = new GameObject("TestSun");
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            go.AddComponent<Light>().type = LightType.Directional;
            spawned.Add(go);
        }

        public void SpawnPlayer(Vector3 position, Quaternion rotation)
        {
            GameObject prefab = null;
#if UNITY_EDITOR
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
#endif
            if (prefab == null) throw new System.InvalidOperationException($"{PrefabPath} missing; run RebuildContent.");
            GameObject player = Object.Instantiate(prefab, position, rotation);
            spawned.Add(player);
            Bind(player);
        }

        private void Bind(GameObject player)
        {
            Player = player;
            Motor = player.GetComponent<PlayerMotor>();
            Stamina = player.GetComponent<PlayerStamina>();
            Look = player.GetComponent<PlayerLook>();
            player.GetComponent<PlayerInputReader>().enabled = false;
            Motor.enabled = false;
            Look.enabled = false;
            Physics.SyncTransforms();
        }

        public void Teleport(Vector3 position, float yaw = 0f)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            Physics.SyncTransforms();
        }

        public static PlayerInputFrame Frame(Vector2 move = default, bool sprint = false, bool crouchHeld = false,
            bool crouchPressed = false, bool jump = false) =>
            new(move, Vector2.zero, sprint, crouchHeld, crouchPressed, jump, false, false, false, false, false, false);

        /// <summary>Simulates for a duration; the first frame may carry one-shot presses.</summary>
        public void Run(PlayerInputFrame frame, float seconds, PlayerInputFrame? firstFrame = null)
        {
            int steps = Mathf.Max(1, Mathf.RoundToInt(seconds / Dt));
            for (int i = 0; i < steps; i++)
                Motor.Simulate(i == 0 && firstFrame.HasValue ? firstFrame.Value : frame, Dt);
        }

        /// <summary>Simulates until the condition is true or the time runs out; returns elapsed seconds or -1.</summary>
        public float RunUntil(PlayerInputFrame frame, System.Func<bool> condition, float maxSeconds)
        {
            for (float t = 0f; t <= maxSeconds; t += Dt)
            {
                if (condition()) return t;
                Motor.Simulate(frame, Dt);
            }
            return condition() ? maxSeconds : -1f;
        }

        public void Settle() => Run(Frame(), 0.25f);

        public void Destroy()
        {
            // Immediate, so a test can swap environments within one frame without the old one lingering.
            foreach (GameObject go in spawned)
                if (go != null) Object.DestroyImmediate(go);
            spawned.Clear();
        }
    }
}

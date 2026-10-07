using Abandoned.Player;
using Abandoned.Voice;
using UnityEngine;
using static Abandoned.EditorTools.SerializedWiring;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Adds proximity voice to the Player prefab: NetworkVoice (send/relay/receive), a "Voice" child at
    /// eye height with the AudioSource + low-pass + VoicePlayback (remote copies speak from their head,
    /// muffled by walls), a "Radio" child (2D, band-passed) for walkie-talkie transmissions, and the
    /// owner-only VoiceTransmitter + VoiceHud.
    /// </summary>
    public static class PlayerVoiceBuilder
    {
        public const string ConfigPath = "Assets/_Project/Data/Voice/VoiceConfig.asset";

        public readonly struct Result
        {
            public readonly Behaviour Transmitter, Hud;

            public Result(Behaviour transmitter, Behaviour hud)
            {
                Transmitter = transmitter;
                Hud = hud;
            }
        }

        public static Result Add(GameObject root, Transform cameraRoot, PlayerInputReader reader)
        {
            var config = LoadOrCreateAsset<VoiceConfig>(ConfigPath);

            var mouth = new GameObject("Voice");
            mouth.transform.SetParent(cameraRoot, false);
            mouth.AddComponent<AudioSource>();
            mouth.AddComponent<AudioLowPassFilter>().cutoffFrequency = VoiceMath.OpenCutoff;
            var playback = mouth.AddComponent<VoicePlayback>();
            Set(playback, "config", config);

            // The radio is in the listener's hand, not at the speaker: 2D, so where it hangs doesn't matter.
            var handset = new GameObject("Radio");
            handset.transform.SetParent(root.transform, false);
            handset.AddComponent<AudioSource>();
            handset.AddComponent<AudioHighPassFilter>();
            handset.AddComponent<AudioLowPassFilter>();
            var radio = handset.AddComponent<RadioPlayback>();
            Set(radio, "config", config);

            var voice = root.AddComponent<NetworkVoice>();
            Set(voice, "config", config);
            Set(voice, "playback", playback);
            Set(voice, "radio", radio);

            var transmitter = root.AddComponent<VoiceTransmitter>();
            Set(transmitter, "config", config);
            Set(transmitter, "inputReader", reader);
            Set(transmitter, "voice", voice);

            var hud = root.AddComponent<VoiceHud>();
            Set(hud, "transmitter", transmitter);

            // Gear (M6.4): hand slots, and a flashlight beam from the eyes that everyone sees.
            var lamp = new GameObject("Flashlight");
            lamp.transform.SetParent(cameraRoot, false);
            lamp.transform.localPosition = new Vector3(0.2f, -0.15f, 0.1f);
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 22f;
            light.spotAngle = 55f;
            light.intensity = 8f;
            // Soft shadows make the beam read in a dark store; carried items don't cast any (Grabbable).
            light.shadows = LightShadows.Soft;
            light.shadowNearPlane = 0.3f;
            light.enabled = false;
            var equipment = root.AddComponent<Abandoned.Equipment.PlayerEquipment>();
            Set(equipment, "catalog", UnityEditor.AssetDatabase.LoadAssetAtPath<Abandoned.Equipment.EquipmentCatalog>(EquipmentContentBuilder.CatalogPath));
            Set(equipment, "inputReader", reader);
            Set(equipment, "carrier", root.GetComponent<Abandoned.Interaction.PlayerCarrier>());
            Set(equipment, "flashlight", light);
            var plank = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"{LootPrefabGenerator.Folder}/Loot_plank.prefab");
            if (plank != null) Set(equipment, "plankPrefab", plank.GetComponent<Unity.Netcode.NetworkObject>());
            else Debug.LogError("Loot_plank prefab missing; run Rebuild Content.");
            Set(equipment, "noiseMakerPrefab", EquipmentContentBuilder.CreateNoiseMaker().GetComponent<Unity.Netcode.NetworkObject>());
            Set(equipment, "supportJackPrefab", EquipmentContentBuilder.CreateSupportJack().GetComponent<Unity.Netcode.NetworkObject>());
            Set(equipment, "pulleyPrefab", EquipmentContentBuilder.CreatePulley().GetComponent<Unity.Netcode.NetworkObject>());
            // Death (M6.7): a ghost camera that orbits living teammates; inactive until needed.
            var ghostObject = new GameObject("GhostCamera");
            ghostObject.transform.SetParent(root.transform, false);
            var ghostCam = ghostObject.AddComponent<Unity.Cinemachine.CinemachineCamera>();
            ghostCam.Lens.FieldOfView = 70f;
            Set(ghostObject.AddComponent<Abandoned.Player.PlayerFieldOfView>(), "playerCamera", ghostCam); // the player's FOV setting too
            ghostObject.SetActive(false);
            var ghost = root.AddComponent<Abandoned.Networking.GhostSpectator>();
            Set(ghost, "player", root.GetComponent<Abandoned.Networking.NetworkPlayer>());
            Set(ghost, "inputReader", reader);
            Set(ghost, "ghostCamera", ghostCam);
            var scanner = root.AddComponent<Abandoned.Equipment.StressScannerHud>();
            Set(scanner, "equipment", equipment);
            Set(scanner, "eye", cameraRoot);
            // Detection gear (M10.5): owner-only readouts and effects.
            var motion = root.AddComponent<Abandoned.Equipment.MotionDetectorHud>();
            Set(motion, "equipment", equipment);
            Set(motion, "eye", cameraRoot);
            var goggles = root.AddComponent<Abandoned.Equipment.NightVisionGoggles>();
            Set(goggles, "equipment", equipment);
            Set(goggles, "inputReader", reader);
            Set(goggles, "carrier", root.GetComponent<Abandoned.Interaction.PlayerCarrier>());
            Set(goggles, "eye", cameraRoot);
            // UI overhaul step 2: the player's own HUD and the loot value tags / scan (owner only).
            root.AddComponent<Abandoned.UI.PlayerHud>().EditorSetup(root.GetComponent<Abandoned.Networking.NetworkPlayer>(),
                root.GetComponent<Abandoned.Player.PlayerStamina>(), root.GetComponent<Abandoned.Interaction.PlayerCarrier>(), equipment, reader);
            root.AddComponent<Abandoned.UI.LootTags>().EditorSetup(root.GetComponent<Abandoned.Networking.NetworkPlayer>(),
                root.GetComponent<Abandoned.Interaction.PlayerInteractor>(), root.GetComponent<Abandoned.Interaction.PlayerCarrier>(), reader);
            return new Result(transmitter, hud);
        }
    }
}

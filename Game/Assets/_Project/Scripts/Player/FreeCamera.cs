using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Player
{
    /// <summary>
    /// A free-flying camera for trailer shots (M10.11, F9; pair with F10 clip mode; see <see cref="Allowed"/>): it takes over the view
    /// from where you are, your player stands still, and the game carries on around you. Local only:
    /// nobody else sees anything. Mouse looks, WASD flies, Q/E down/up, Shift fast, the wheel sets speed.
    /// </summary>
    public class FreeCamera : MonoBehaviour
    {
        private static FreeCamera instance;

        private CinemachineCamera cam;
        private float yaw, pitch, speed = 3f;

        public static bool Active => instance != null;

        /// <summary>
        /// Dev builds anywhere; in player builds only where it can't scout for the living (QA B-02): at the
        /// HQ between jobs, or as a ghost.
        /// </summary>
        public static bool Allowed
        {
            get
            {
                if (Core.DevTools.Enabled) return true;
                var run = Extraction.RunState.Current;
                bool onJob = run != null && run.IsSpawned && run.State.Phase != Extraction.RunPhase.Departed;
                var me = Networking.NetworkPlayer.Local;
                return !onJob || (me != null && me.IsDead);
            }
        }

        public static void Toggle()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                return;
            }
            Camera main = Camera.main;
            if (main == null) return;
            var go = new GameObject("FreeCamera");
            go.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
            instance = go.AddComponent<FreeCamera>();
            instance.cam.Lens.FieldOfView = main.fieldOfView;
        }

        private void Awake()
        {
            cam = gameObject.AddComponent<CinemachineCamera>();
            cam.Priority = 1000;
            Vector3 e = transform.eulerAngles;
            yaw = e.y;
            pitch = e.x > 180f ? e.x - 360f : e.x;
            Freeze(true);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            Freeze(false);
            UI.HudCallout.Clear(this);
            // Leaving (F9) in the frame a photo was being taken ends its coroutine: the HUD comes back here.
            RestoreHud();
        }

        // ---- 0.23.6 photo mode ----

        /// <summary>Where photos go: Pictures/Abandoned (or the game's data folder where there's no Pictures).</summary>
        public static string PhotoFolder
        {
            get
            {
                string pictures = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
                return string.IsNullOrEmpty(pictures) ? System.IO.Path.Combine(Application.persistentDataPath, "Photos") : System.IO.Path.Combine(pictures, "Abandoned");
            }
        }

        // The HUD a photo in progress has hidden, and what to set it back to (null = none hidden).
        private UnityEngine.UIElements.VisualElement photoHud;
        private UnityEngine.UIElements.DisplayStyle photoHudShown;
        private bool takingPhoto;

        private void RestoreHud()
        {
            if (photoHud != null) photoHud.style.display = photoHudShown;
            photoHud = null;
            takingPhoto = false;
        }

        private System.Collections.IEnumerator TakePhoto()
        {
            // One at a time: a second photo started while the first had the HUD hidden remembered "hidden"
            // as the state to restore, and the HUD never came back.
            if (takingPhoto) yield break;
            takingPhoto = true;
            UnityEngine.UIElements.VisualElement hud = UI.HudLayer.Root;
            if (hud != null)
            {
                photoHudShown = hud.resolvedStyle.display;
                photoHud = hud;
                hud.style.display = UnityEngine.UIElements.DisplayStyle.None;
            }
            yield return new WaitForEndOfFrame();
            // The Pictures folder can refuse us (macOS privacy, a read-only or redirected folder). That used
            // to throw here, which ended this coroutine with the HUD still hidden; now the photo goes to the
            // game's own folder instead, and the HUD is restored whatever happens.
            string folder = PhotoFolder, file = null;
            for (int attempt = 0; attempt < 2 && file == null; attempt++)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(folder);
                    file = System.IO.Path.Combine(folder, $"Abandoned_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
                    ScreenCapture.CaptureScreenshot(file);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Photo] Couldn't save to {folder} ({e.Message}).");
                    file = null;
                    folder = System.IO.Path.Combine(Application.persistentDataPath, "Photos");
                }
            }
            yield return null;
            RestoreHud();
            if (file == null)
            {
                Audio.GameAudio.PlayUi(Audio.SoundId.UiError, 0.8f);
                UI.ToastFeed.Show("PHOTO", "Couldn't save the photo: no folder to write to.", null, UI.ToastFeed.Kind.Warn);
                yield break;
            }
            Audio.GameAudio.PlayUi(Audio.SoundId.UiConfirm, 0.8f);
            UI.ToastFeed.Show("PHOTO", $"Saved to {file}", null, UI.ToastFeed.Kind.Good);
        }

        // Every player on this machine stands still while we fly (only ours reads devices anyway).
        private static void Freeze(bool frozen)
        {
            foreach (PlayerInputReader reader in FindObjectsByType<PlayerInputReader>(FindObjectsSortMode.None))
                if (reader != null) reader.Override = frozen ? default(PlayerInputFrame) : null;
        }

        private void Update()
        {
            Keyboard k = Keyboard.current;
            Mouse m = Mouse.current;
            if (m != null)
            {
                Vector2 look = m.delta.ReadValue() * 0.08f * Core.GameSettings.Sensitivity;
                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, -89f, 89f);
                float wheel = m.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f) speed = Mathf.Clamp(speed * (wheel > 0f ? 1.15f : 1f / 1.15f), 0.2f, 40f);
            }
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            if (k == null) return;
            UI.HudCallout.Set(this, "PHOTO MODE   WASD / Q E fly   wheel speed   Z / X zoom   Enter take a photo   F9 leave");
            if (k.zKey.isPressed) cam.Lens.FieldOfView = Mathf.Max(15f, cam.Lens.FieldOfView - 30f * Time.unscaledDeltaTime);
            if (k.xKey.isPressed) cam.Lens.FieldOfView = Mathf.Min(110f, cam.Lens.FieldOfView + 30f * Time.unscaledDeltaTime);
            if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame) StartCoroutine(TakePhoto());
            Vector3 move = Vector3.zero;
            if (k.wKey.isPressed) move += Vector3.forward;
            if (k.sKey.isPressed) move += Vector3.back;
            if (k.dKey.isPressed) move += Vector3.right;
            if (k.aKey.isPressed) move += Vector3.left;
            Vector3 world = transform.rotation * move;
            if (k.eKey.isPressed) world += Vector3.up;
            if (k.qKey.isPressed) world += Vector3.down;
            float boost = k.leftShiftKey.isPressed ? 4f : 1f;
            transform.position += world * (speed * boost * Time.unscaledDeltaTime);
        }
    }
}

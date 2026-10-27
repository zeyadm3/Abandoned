using Abandoned.Player;
using UnityEngine;

namespace Abandoned.Onboarding
{
    /// <summary>
    /// The HR manager at her desk: the crew's worker figure (Resources/WorkerVisual) dressed in her own look, seated,
    /// her head turned to whoever she is talking to and nodding with her voice. Local, offline, never networked.
    /// </summary>
    public class HrManagerActor : MonoBehaviour
    {
        [SerializeField] private CosmeticDefinition person;
        [SerializeField] private CosmeticDefinition outfit;
        [SerializeField] private Color tint = new(.16f, .18f, .24f);
        [SerializeField] private AudioSource voice;
        [Tooltip("Where she looks while she talks (the player's seat camera).")]
        [SerializeField] private Transform lookAt;
        [Tooltip("Height of her hips on the chair, from this object.")]
        [SerializeField] private float hipHeight = .55f;

        private WorkerRig rig;
        private Quaternion headRest;
        private readonly float[] samples = new float[256];
        private float talk, nod;

        public AudioSource Voice => voice;

        private void Start()
        {
            GameObject prefab = Resources.Load<GameObject>("WorkerVisual");
            if (prefab == null) return;
            GameObject figure = Instantiate(prefab, transform, false);
            figure.transform.localPosition = Vector3.up * hipHeight;
            foreach (TextMesh mark in figure.GetComponentsInChildren<TextMesh>()) mark.text = "";
            foreach (Collider c in figure.GetComponentsInChildren<Collider>()) Destroy(c);
            rig = figure.GetComponent<WorkerRig>();
            if (rig == null) return;
            rig.Tint(tint);
            if (rig.TryGetComponent(out WorkerLook look)) look.Apply(person, outfit, tint);
            headRest = rig.HeadAnchor.localRotation;
        }

        private void LateUpdate()
        {
            if (rig == null) return;
            // Seated: thighs level, shins down, forearms on the desk.
            rig.LeftLeg.localRotation = Quaternion.Euler(-86f, 0f, -4f);
            rig.RightLeg.localRotation = Quaternion.Euler(-86f, 0f, 4f);
            rig.LeftKnee.localRotation = Quaternion.Euler(84f, 0f, 0f);
            rig.RightKnee.localRotation = Quaternion.Euler(84f, 0f, 0f);
            rig.LeftArm.localRotation = Quaternion.Euler(-42f, 0f, -12f);
            rig.RightArm.localRotation = Quaternion.Euler(-42f, 0f, 12f);
            rig.LeftElbow.localRotation = Quaternion.Euler(-58f, 0f, 0f);
            rig.RightElbow.localRotation = Quaternion.Euler(-58f, 0f, 0f);

            // Speech drives a small nod; between lines she breathes and glances.
            if (voice != null && voice.isPlaying)
            {
                voice.GetOutputData(samples, 0);
                float sum = 0f;
                foreach (float s in samples) sum += s * s;
                talk = Mathf.Lerp(talk, Mathf.Clamp01(Mathf.Sqrt(sum / samples.Length) * 9f), 18f * Time.deltaTime);
            }
            else talk = Mathf.Lerp(talk, 0f, 6f * Time.deltaTime);
            nod = Mathf.Sin(Time.time * 7.3f) * talk * 4f + Mathf.Sin(Time.time * .9f) * 1.2f;
            rig.Torso.localRotation = Quaternion.Euler(6f + Mathf.Sin(Time.time * 1.7f) * .8f, 0f, 0f);

            Transform head = rig.HeadAnchor;
            Quaternion aim = headRest;
            if (lookAt != null && head.parent != null)
            {
                Vector3 to = head.parent.InverseTransformDirection(lookAt.position - head.position);
                float yaw = Mathf.Clamp(Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg, -55f, 55f);
                float pitch = Mathf.Clamp(-Mathf.Atan2(to.y, new Vector2(to.x, to.z).magnitude) * Mathf.Rad2Deg, -20f, 25f);
                aim = Quaternion.Euler(pitch + nod, yaw, 0f) * headRest;
            }
            head.localRotation = Quaternion.Slerp(head.localRotation, aim, 5f * Time.deltaTime);
        }

#if UNITY_EDITOR
        public void EditorSetup(CosmeticDefinition who, CosmeticDefinition wearing, AudioSource speaker, Transform target)
        {
            person = who;
            outfit = wearing;
            voice = speaker;
            lookAt = target;
        }
#endif
    }
}

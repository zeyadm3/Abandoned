using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Equipment;
using Abandoned.Interaction;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>
    /// First-person hands (0.21.0), on the local player only: two coverall sleeves with gloves hung from the
    /// camera. Empty, they ride low at the bottom of the view and swing with the walk; carrying, they reach out
    /// to the sides of what you hold and grip it, and they shake under its weight; with gear in hand the right
    /// one holds it up (0.21.1, HeldToolView). Purely visual and local.
    /// </summary>
    public class FirstPersonHands : MonoBehaviour
    {
        private static readonly Vector3 Shoulder = new(0.21f, -0.3f, 0.02f);

        private Transform eye;
        private NetworkPlayer me;
        private PlayerCarrier carrier;
        private Transform[] arms = new Transform[2], hands = new Transform[2];
        private Vector3[] shown = new Vector3[2];
        private float walk;
        private Vector3 lastPos;
        private GameObject tool;
        private EquipmentDefinition shownTool;
        private readonly List<Renderer> coverall = new();
        private readonly MaterialPropertyBlock suitBlock = new();
        private Color shownSuit = new(-1f, -1f, -1f, -1f);
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public Transform RightHand => hands[1];

        private void Update()
        {
            if (NetworkPlayer.Local != me) Rebuild();
            if (me == null || eye == null) return;
            ApplySuit();
            bool visible = !me.IsDead && !me.IsHiding && !me.Ragdoll.IsRagdolled && Camera.main != null && Camera.main.enabled;
            for (int i = 0; i < 2; i++)
            {
                if (arms[i] != null) arms[i].gameObject.SetActive(visible);
                if (hands[i] != null) hands[i].gameObject.SetActive(visible);
            }
            if (!visible) return;
            float speed = Time.deltaTime > 0f ? Vector3.ProjectOnPlane(me.transform.position - lastPos, Vector3.up).magnitude / Time.deltaTime : 0f;
            lastPos = me.transform.position;
            walk += Mathf.Min(speed, 6f) * Time.deltaTime * 4f;
            Grabbable held = carrier != null ? carrier.Held : null;
            float strain = held != null ? Mathf.Clamp01(carrier.CarriedWeight / 70f) : 0f;
            for (int side = 0; side < 2; side++)
            {
                float s = side == 0 ? -1f : 1f;
                Vector3 target;
                if (held != null && held.TryGetComponent(out Collider c))
                {
                    // Grip the item's sides, seen from the eye: its box's left and right, at its middle height.
                    Bounds b = c.bounds;
                    Vector3 grip = b.center + eye.right * s * Mathf.Min(b.extents.x + b.extents.z, 0.45f) * 0.8f;
                    target = eye.InverseTransformPoint(grip);
                    target.z = Mathf.Clamp(target.z, 0.25f, 0.75f);
                    target += Random.insideUnitSphere * strain * 0.006f;
                }
                else
                {
                    float sway = Mathf.Sin(walk + side * Mathf.PI) * Mathf.Clamp01(speed / 4f) * 0.03f;
                    target = new Vector3(s * 0.24f, -0.62f + sway, 0.32f);
                    PlayerEquipment gear = me.GetComponent<PlayerEquipment>();
                    if (side == 1 && gear != null && gear.InHand != null) target = new Vector3(0.22f, -0.38f + sway * 0.5f, 0.45f);
                }
                shown[side] = Vector3.Lerp(shown[side], target, 1f - Mathf.Exp(-14f * Time.deltaTime));
                Pose(side, shown[side]);
            }
            ShowTool(held == null ? me.GetComponent<PlayerEquipment>()?.InHand : null);
        }

        // 0.21.1: the gear in hand, in the right glove.
        private void ShowTool(EquipmentDefinition inHand)
        {
            if (inHand == shownTool) return;
            shownTool = inHand;
            if (tool != null) Destroy(tool);
            if (inHand == null || hands[1] == null) return;
            tool = ToolModels.Build(inHand, hands[1].parent);
            tool.name = "FirstPersonHands_Tool";
        }

        private void LateUpdate()
        {
            if (tool != null && hands[1] != null)
            {
                tool.transform.localPosition = hands[1].localPosition + new Vector3(0f, 0.03f, 0.02f);
                tool.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
                tool.SetActive(hands[1].gameObject.activeInHierarchy && arms[1].gameObject.activeSelf);
            }
        }

        // An arm is a sleeve from the shoulder to the hand, with a glove on the end.
        private void Pose(int side, Vector3 hand)
        {
            Vector3 shoulder = new(Shoulder.x * (side == 0 ? -1f : 1f), Shoulder.y, Shoulder.z);
            Vector3 along = hand - shoulder;
            arms[side].localPosition = shoulder + along * 0.5f;
            arms[side].localRotation = Quaternion.FromToRotation(Vector3.up, along.normalized);
            arms[side].localScale = new Vector3(1f, along.magnitude, 1f);
            hands[side].localPosition = hand;
            hands[side].localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }

        private void Rebuild()
        {
            for (int i = 0; i < 2; i++)
            {
                if (arms[i] != null) Destroy(arms[i].gameObject);
                if (hands[i] != null) Destroy(hands[i].gameObject);
            }
            if (tool != null) Destroy(tool);
            tool = null;
            shownTool = null;
            coverall.Clear();
            shownSuit = new Color(-1f, -1f, -1f, -1f);
            me = NetworkPlayer.Local;
            if (me == null) return;
            var look = me.GetComponent<PlayerLook>();
            eye = look != null ? look.EyeRoot : null;
            carrier = me.Carrier;
            if (eye == null) return;
            lastPos = me.transform.position;
            for (int i = 0; i < 2; i++)
            {
                arms[i] = WorkerHandArt.Sleeve(eye, i);
                hands[i] = WorkerHandArt.Glove(eye, i);
                foreach (Renderer renderer in arms[i].GetComponentsInChildren<Renderer>())
                    if (renderer.sharedMaterial == CharacterSurfaceLibrary.Get("CrewFabric")) coverall.Add(renderer);
                shown[i] = new Vector3(i == 0 ? -0.24f : 0.24f, -0.62f, 0.32f);
            }
            ApplySuit();
        }

        private void ApplySuit()
        {
            PlayerCosmetics cosmetics = me != null ? me.GetComponent<PlayerCosmetics>() : null;
            CosmeticDefinition outfit = cosmetics != null && cosmetics.Catalog != null ? cosmetics.Catalog.Coverall(cosmetics.Choice.Coverall) : null;
            Color colour = outfit != null ? outfit.Color : new Color(.88f, .39f, .085f);
            if (colour == shownSuit) return;
            shownSuit = colour;
            foreach (Renderer renderer in coverall)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(suitBlock);
                suitBlock.SetColor(BaseColor, colour);
                renderer.SetPropertyBlock(suitBlock);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < 2; i++)
            {
                if (arms[i] != null) Destroy(arms[i].gameObject);
                if (hands[i] != null) Destroy(hands[i].gameObject);
            }
            if (tool != null) Destroy(tool);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>
    /// Held tools in view (0.21.1): a small code-built model of each piece of gear, the same one in your own
    /// first-person hand and in your body's hand as crewmates see it. Shapes by kind; anything without its own
    /// shape is a box in the gear's colour. Grip at the origin, pointing along +Z.
    /// </summary>
    public static class ToolModels
    {
        public static GameObject Build(EquipmentDefinition d, Transform parent, float scale = 1f)
        {
            var root = new GameObject($"Tool_{d.Id}");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * scale;
            Color steel = new(0.5f, 0.52f, 0.55f), dark = new(0.1f, 0.1f, 0.11f);
            switch (d.Kind)
            {
                case EquipmentKind.Flashlight:
                    Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.08f), new Vector3(0.04f, 0.1f, 0.04f), dark, 90f);
                    Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0f, 0.19f), new Vector3(0.055f, 0.02f, 0.055f), new Color(1f, 0.95f, 0.75f), 90f);
                    break;
                case EquipmentKind.Crowbar:
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.25f), new Vector3(0.025f, 0.025f, 0.6f), d.Color);
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.56f), new Vector3(0.025f, 0.1f, 0.025f), d.Color);
                    break;
                case EquipmentKind.Sledgehammer:
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.3f), new Vector3(0.03f, 0.03f, 0.7f), new Color(0.5f, 0.35f, 0.2f));
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.66f), new Vector3(0.09f, 0.09f, 0.18f), steel);
                    break;
                case EquipmentKind.Medkit:
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.1f), new Vector3(0.2f, 0.12f, 0.14f), d.Color);
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0.061f, 0.1f), new Vector3(0.1f, 0.002f, 0.03f), Color.white);
                    break;
                case EquipmentKind.Radio:
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0.04f, 0.06f), new Vector3(0.06f, 0.16f, 0.04f), dark);
                    Part(root, PrimitiveType.Cylinder, new Vector3(0.02f, 0.17f, 0.06f), new Vector3(0.008f, 0.06f, 0.008f), dark);
                    break;
                case EquipmentKind.BoltCutters:
                    Part(root, PrimitiveType.Cube, new Vector3(-0.02f, 0f, 0.25f), new Vector3(0.02f, 0.02f, 0.5f), d.Color);
                    Part(root, PrimitiveType.Cube, new Vector3(0.02f, 0f, 0.25f), new Vector3(0.02f, 0.02f, 0.5f), d.Color);
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.52f), new Vector3(0.05f, 0.03f, 0.08f), steel);
                    break;
                case EquipmentKind.SprayChalk:
                case EquipmentKind.Flares:
                case EquipmentKind.GlowSticks:
                    Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0.05f), new Vector3(0.05f, 0.08f, 0.05f), d.Color);
                    break;
                default:
                    Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.08f), new Vector3(0.12f, 0.1f, 0.16f), d.Color);
                    break;
            }
            return root;
        }

        private static void Part(GameObject root, PrimitiveType type, Vector3 at, Vector3 size, Color colour, float tilt = 0f)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            // Off at once (Destroy waits for the end of the frame): a solid shape in a hand, in front of
            // the eyes, is in the way of that frame's interaction ray and of whatever is carried.
            Collider solid = go.GetComponent<Collider>();
            if (solid != null)
            {
                solid.enabled = false;
                Object.Destroy(solid);
            }
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = at;
            go.transform.localScale = size;
            if (tilt != 0f) go.transform.localRotation = Quaternion.Euler(tilt, 0f, 0f);
            var r = go.GetComponent<Renderer>();
            Material shared = MaterialFor(r, colour);
            if (shared != null) r.sharedMaterial = shared;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // One material per colour, shared by every tool model. A model is rebuilt each time the gear in a
        // hand changes (for every player, on every machine), and renderer.material made a new material
        // for every part every time; destroying the model does not destroy those, so they piled up all session.
        private static readonly Dictionary<int, Material> Materials = new();

        private static Material MaterialFor(Renderer part, Color colour)
        {
            Color32 c = colour;
            int key = (c.r << 24) | (c.g << 16) | (c.b << 8) | c.a;
            if (Materials.TryGetValue(key, out Material material) && material != null) return material;
            if (part.sharedMaterial == null) return null;
            material = new Material(part.sharedMaterial) { name = "ToolPart", color = colour };
            Materials[key] = material;
            return material;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Materials.Clear();
    }
}

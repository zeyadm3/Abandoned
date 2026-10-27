using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>One finished salvage-tool design in first person, on crewmates and on the floor. Grip is zero, +Z forward.</summary>
    public static class ToolModels
    {
        private static readonly Dictionary<string, List<(Mesh mesh, Material material)>> Models = new();

        public static GameObject Build(EquipmentDefinition definition, Transform parent, float scale = 1f)
        {
            // 2026 remake: the remodelled gear when there is one.
            GameObject remade = GearModels.For(definition.Kind);
            if (remade != null)
            {
                var holder = new GameObject($"Tool_{definition.Id}");
                holder.transform.SetParent(parent, false);
                holder.transform.localScale = Vector3.one * scale;
                GameObject model = Object.Instantiate(remade, holder.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                return holder;
            }
            string key = $"{(int)definition.Kind}_{ColorUtility.ToHtmlStringRGBA(definition.Color)}";
            if (!Models.TryGetValue(key, out var parts))
            {
                var art = new ToolArtBuilder(definition.Color);
                switch (definition.Kind)
                {
                    case EquipmentKind.Flashlight: ToolElectronicModels.Flashlight(art); break;
                    case EquipmentKind.Radio: ToolElectronicModels.Radio(art); break;
                    case EquipmentKind.StressScanner: ToolElectronicModels.Detector(art, 0); break;
                    case EquipmentKind.MotionDetector: ToolElectronicModels.Detector(art, 1); break;
                    case EquipmentKind.ThermalCamera: ToolElectronicModels.Detector(art, 2); break;
                    case EquipmentKind.NightVision: ToolElectronicModels.NightVision(art); break;
                    case EquipmentKind.Battery: ToolElectronicModels.Battery(art); break;
                    case EquipmentKind.NoiseMaker: ToolElectronicModels.NoiseMaker(art); break;
                    case EquipmentKind.Drone: ToolElectronicModels.Drone(art); break;
                    case EquipmentKind.Floodlight: ToolElectronicModels.Floodlight(art); break;
                    case EquipmentKind.Crowbar: ToolHandModels.Crowbar(art); break;
                    case EquipmentKind.Sledgehammer: ToolHandModels.Sledgehammer(art); break;
                    case EquipmentKind.BoltCutters: ToolHandModels.BoltCutters(art); break;
                    case EquipmentKind.Lockpick: ToolHandModels.Lockpick(art); break;
                    case EquipmentKind.SprayChalk: ToolHandModels.Canister(art, 0); break;
                    case EquipmentKind.GlowSticks: ToolHandModels.Canister(art, 1); break;
                    case EquipmentKind.Flares: ToolHandModels.Canister(art, 2); break;
                    case EquipmentKind.Planks: ToolHandModels.Planks(art); break;
                    case EquipmentKind.Medkit: ToolUtilityModels.Medkit(art); break;
                    case EquipmentKind.Backpack: ToolUtilityModels.Backpack(art); break;
                    case EquipmentKind.CrashMat: ToolUtilityModels.Fabric(art, true); break;
                    case EquipmentKind.MovingBlanket: ToolUtilityModels.Fabric(art, false); break;
                    case EquipmentKind.SupportJack: ToolUtilityModels.SupportJack(art); break;
                    case EquipmentKind.RopePulley: ToolUtilityModels.RopePulley(art); break;
                    case EquipmentKind.HandTrolley: ToolUtilityModels.Trolley(art, false); break;
                    case EquipmentKind.Flatbed: ToolUtilityModels.Trolley(art, true); break;
                    default: ToolUtilityModels.TransitCase(art); break;
                }
                parts = art.Finish("Equipment_" + definition.Kind);
                Models[key] = parts;
            }
            var root = new GameObject($"Tool_{definition.Id}");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one * scale;
            foreach (var part in parts)
            {
                var item = new GameObject(part.material.name);
                item.transform.SetParent(root.transform, false);
                item.AddComponent<MeshFilter>().sharedMesh = part.mesh;
                var renderer = item.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = part.material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return root;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            foreach (var model in Models.Values)
            foreach (var part in model)
                if (part.mesh != null)
                {
                    if (Application.isPlaying) Object.Destroy(part.mesh);
                    else Object.DestroyImmediate(part.mesh);
                }
            Models.Clear();
            ToolArtMeshes.Reset();
        }
    }
}

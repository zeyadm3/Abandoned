using System.Collections.Generic;
using System.IO;
using Abandoned.Core;
using Abandoned.Extraction;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IronFactoryMachinePlan.MachineKind;
using L = Abandoned.EditorTools.IronFactoryLayout;
using Plan = Abandoned.EditorTools.IronFactoryMachinePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Power in the Iron Factory (step 4): the Breaker Hall's bank of breakers, one per power sector, and the
    /// machines that come back to life when their sector's breaker is thrown. A breaker lights its sector's
    /// fixtures for a few minutes (the existing 0.16.1 rule) and here also starts every press, mill stand,
    /// boiler and belt in that sector: light, at the price of noise and moving iron.
    /// </summary>
    public static class IronFactoryPower
    {
        private const string ConfigPath = "Assets/_Project/Data/Extraction/Resources/MachineHazardConfig.asset";
        private static readonly Vector3 PanelSize = new(.7f, .9f, .28f);
        private const float PanelLift = 1f;

        /// <summary>One panel per sector along the Breaker Hall's south wall, facing into the room.</summary>
        public static int BuildBreakers(Transform root, IronFactoryArt art)
        {
            HorrorConfig horror = AssetDatabase.LoadAssetAtPath<HorrorConfig>(HorrorContentBuilder.Path);
            int sectors = Mathf.Clamp(horror != null ? horror.Sectors : 6, 1, RunBreakers.MaxSectors);
            Rect hall = System.Array.Find(L.Rooms, r => r.Name == "Breaker_Hall").Meters;
            Transform bank = Group("BreakerBank", root);
            float wallFace = hall.yMin + .22f;
            float spacing = Mathf.Min(2.2f, (hall.width - 3f) / sectors);
            for (int s = 0; s < sectors; s++)
                Panel(bank, s, new Vector3(hall.xMin + 2f + s * spacing, 0f, wallFace), art);
            return sectors;
        }

        private static void Panel(Transform parent, int sector, Vector3 at, IronFactoryArt art)
        {
            Transform root = Group($"Breaker_Sector{sector}", parent);
            root.SetPositionAndRotation(at, Quaternion.LookRotation(Vector3.forward));
            float z = PanelSize.z * .5f + .02f, y = PanelLift + PanelSize.y * .5f;
            Box("Body", root, new Vector3(0f, y, z), PanelSize, art.Steel, false);
            Box("Door", root, new Vector3(0f, y, PanelSize.z + .03f), new Vector3(PanelSize.x - .06f, PanelSize.y - .06f, .02f), art.Bed, false);
            Box("Stripe", root, new Vector3(0f, PanelLift + PanelSize.y - .06f, PanelSize.z + .045f), new Vector3(PanelSize.x - .1f, .06f, .01f), art.Safety, false);
            Box("Conduit", root, new Vector3(.2f, (PanelLift + PanelSize.y + L.RoomHeight) * .5f, .06f),
                new Vector3(.06f, L.RoomHeight - PanelLift - PanelSize.y, .06f), art.Steel, false);
            // The handle pivots at the cabinet's side; down = off.
            Transform pivot = Group("Handle", root);
            pivot.localPosition = new Vector3(PanelSize.x * .5f + .06f, y, PanelSize.z * .5f);
            Box("Arm", pivot, new Vector3(0f, .16f, 0f), new Vector3(.05f, .32f, .05f), art.Steel, false);
            Box("Grip", pivot, new Vector3(0f, .34f, .04f), new Vector3(.12f, .06f, .1f), art.Safety, false);
            pivot.localRotation = Quaternion.Euler(40f, 0f, 0f);
            GameObject lamp = Primitive(PrimitiveType.Sphere, "Lamp", root, new Vector3(-.22f, PanelLift + PanelSize.y - .15f, PanelSize.z + .06f),
                Vector3.one * .07f, GetMaterial("Greybox_BreakerLamp", new Color(.9f, .15f, .1f)), false);
            var solid = root.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, y, z);
            solid.size = PanelSize;
            root.gameObject.AddComponent<BreakerPanel>().EditorSetup(sector, pivot, lamp.GetComponent<Renderer>(), true);
        }

        /// <summary>Fits the hazard to each machine that has one; returns how many.</summary>
        public static int AddHazards(IEnumerable<(Plan.Machine plan, Transform root)> machines, IronFactoryArt art)
        {
            EnsureConfig();
            Material steam = GetMaterial("IronFactory_Steam", new Color(.82f, .82f, .80f));
            Material lampGlass = GetMaterial("IronFactory_WarningLamp", new Color(.15f, .1f, .05f));
            int count = 0, n = 0;
            foreach ((Plan.Machine plan, Transform root) in machines)
            {
                n++;
                Vector3 s = Plan.Size(plan.Kind);
                if (plan.Length > 0f) s.z = plan.Length;
                // Golden-ratio phases: neighbours never strike together.
                float phase = Mathf.Repeat(n * .618f, 1f);
                switch (plan.Kind)
                {
                    case K.Press:
                    {
                        Transform ram = root.Find("Visual/Ram");
                        Movable(ram);
                        var zone = new Bounds(new Vector3(0f, 1.25f, s.z * .5f + .8f), new Vector3(s.x, 2.5f, 1.6f));
                        Stripes(root, zone, art);
                        root.gameObject.AddComponent<MachineHazard>().EditorSetup(MachineHazardKind.Press, zone, phase, ram, null,
                            Lamp(root, new Vector3(0f, s.y + .12f, 0f), lampGlass), null);
                        count++;
                        break;
                    }
                    case K.MillStand:
                    {
                        // Over the roller tables either side of the stand, from the table top up: the ground beside is safe.
                        var zone = new Bounds(new Vector3(0f, 1.7f, 0f), new Vector3(s.x - .6f, 1.8f, s.z + 2.4f));
                        root.gameObject.AddComponent<MachineHazard>().EditorSetup(MachineHazardKind.Rollers, zone, phase, null, null,
                            Lamp(root, new Vector3(0f, s.y + .12f, 0f), lampGlass), null);
                        count++;
                        break;
                    }
                    case K.Boiler:
                    {
                        var zone = new Bounds(new Vector3(0f, 1.25f, 0f), new Vector3(s.x + 3f, 2.5f, s.z + 3f));
                        Stripes(root, zone, art);
                        root.gameObject.AddComponent<MachineHazard>().EditorSetup(MachineHazardKind.Steam, zone, phase, null,
                            Jets(root, s, steam), Lamp(root, new Vector3(s.x * .5f + .05f, 2f, 0f), lampGlass), null);
                        count++;
                        break;
                    }
                    case K.RollerTable:
                    case K.Conveyor:
                    {
                        MovingPlatform belt = root.gameObject.AddComponent<MovingPlatform>();
                        root.gameObject.AddComponent<MachineHazard>().EditorSetup(MachineHazardKind.Belt, new Bounds(), 0f, null, null, null, belt);
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        // Moving parts must not be batched into the static mesh.
        private static void Movable(Transform t)
        {
            if (t != null) GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
        }

        private static Renderer Lamp(Transform root, Vector3 at, Material glass)
        {
            GameObject lamp = Primitive(PrimitiveType.Sphere, "WarningLamp", root, at, Vector3.one * .22f, glass, false);
            Movable(lamp.transform);
            return lamp.GetComponent<Renderer>();
        }

        // Yellow lines on the floor around the danger zone: what the warning lamp is warning about.
        private static void Stripes(Transform root, Bounds zone, IronFactoryArt art)
        {
            Transform lines = Group("HazardStripes", root);
            const float w = .14f, y = .008f;
            Vector3 c = zone.center, e = zone.extents;
            Box("N", lines, new Vector3(c.x, y, c.z + e.z), new Vector3(e.x * 2f, .012f, w), art.Safety, false);
            Box("S", lines, new Vector3(c.x, y, c.z - e.z), new Vector3(e.x * 2f, .012f, w), art.Safety, false);
            Box("E", lines, new Vector3(c.x + e.x, y, c.z), new Vector3(w, .012f, e.z * 2f), art.Safety, false);
            Box("W", lines, new Vector3(c.x - e.x, y, c.z), new Vector3(w, .012f, e.z * 2f), art.Safety, false);
        }

        // Four jets from the boiler's sides, shown only while it vents.
        private static GameObject Jets(Transform root, Vector3 s, Material steam)
        {
            Transform jets = Group("Steam", root);
            Movable(jets);
            float r = s.x * .5f + .55f;
            foreach (Vector3 d in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
            {
                GameObject jet = Box("Jet", jets, d * r + Vector3.up * 1.2f, new Vector3(.6f, 2.4f, .6f), steam, false);
                jet.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Movable(jet.transform);
            }
            return jets.gameObject;
        }

        private static void EnsureConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<MachineHazardConfig>(ConfigPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MachineHazardConfig>(), ConfigPath);
        }
    }
}

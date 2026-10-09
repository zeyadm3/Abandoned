using Abandoned.Core;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>A permanently closed steel staff entrance, fitted to the saved office portal.</summary>
    public static class HqStaffEntryDoor
    {
        private const string PortalName = "Portal_OPERATIONS / STAFF ENTRY";
        private const string DoorName = "StaffEntryDoor";
        private const float Width = 2.4f, Height = 2.75f, Thickness = .16f;

        public static void Build(Transform parent)
        {
            Transform portal = null;
            Transform[] descendants = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in descendants)
                if (child.name == PortalName) { portal = child; break; }

            foreach (Transform child in descendants)
                if (child != null && child != parent && child.name == DoorName)
                    Object.DestroyImmediate(child.gameObject);

            // Staying beneath the actual portal preserves hand-authored placement and future moves.
            Transform door = Group(DoorName, portal != null ? portal : parent);
            if (portal == null) door.localPosition = new Vector3(HqLayout.Dispatch.x, 0f, -.1f);
            door.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);

            Material steel = DoorMaterial("HQ_StaffDoorSteel", new Color(.20f, .26f, .25f), .69f);
            Material frame = DoorMaterial("HQ_StaffDoorFrame", new Color(.14f, .19f, .19f), .72f);
            Material hardware = DoorMaterial("HQ_StaffDoorHardware", new Color(.42f, .45f, .43f), .46f);
            Material rubber = DoorMaterial("HQ_StaffDoorWeatherSeal", new Color(.035f, .04f, .037f), .94f);
            Material glazing = DoorMaterial("HQ_StaffDoorVision", new Color(.075f, .13f, .14f), .16f);

            foreach (float side in new[] { -1f, 1f })
            {
                Box("SteelFrameJamb", door, new Vector3(side * (Width * .5f + .045f), Height * .5f, 0f),
                    new Vector3(.12f, Height, .24f), frame);
                Leaf(door, side, steel, frame, hardware, rubber, glazing);
            }
            Box("SteelFrameHeader", door, new Vector3(0f, Height + .045f, 0f),
                new Vector3(Width + .21f, .12f, .24f), frame);
            Box("StaffDoorThreshold", door, new Vector3(0f, .014f, 0f),
                new Vector3(Width, .028f, .30f), hardware);
            Box("CentreWeatherStrip", door, new Vector3(0f, Height * .5f, -.088f),
                new Vector3(.022f, Height, .012f), rubber, false);
            Box("InteriorCentreWeatherStrip", door, new Vector3(0f, Height * .5f, .088f),
                new Vector3(.022f, Height, .012f), rubber, false);

            // A visible secured centre bolt reinforces that this entrance has no opening interaction.
            Box("DeadboltBridge", door, new Vector3(0f, 1.04f, -.12f), new Vector3(.34f, .065f, .045f), hardware, false);
            Box("LockBody", door, new Vector3(.03f, .965f, -.148f), new Vector3(.095f, .11f, .058f), frame, false);
            HqSalvageProps.Rod("LockShackle", door, new Vector3(-.005f, 1.016f, -.15f),
                new Vector3(-.005f, 1.057f, -.15f), .017f, hardware);
            HqSalvageProps.Rod("LockShackle", door, new Vector3(.064f, 1.016f, -.15f),
                new Vector3(.064f, 1.057f, -.15f), .017f, hardware);
            HqSalvageProps.Rod("LockShackleTop", door, new Vector3(-.005f, 1.057f, -.15f),
                new Vector3(.064f, 1.057f, -.15f), .017f, hardware);
            MarkStatic(door);
        }

        private static Material DoorMaterial(string name, Color color, float roughness)
        {
            Material saved = AssetDatabase.LoadAssetAtPath<Material>($"{PolishAssets.Folder}/{name}.mat");
            return saved != null ? saved : PolishAssets.Material(name, color, roughness);
        }

        private static void Leaf(Transform parent, float side, Material steel, Material frame,
            Material hardware, Material rubber, Material glazing)
        {
            float leafWidth = Width * .5f;
            Transform leaf = Group(side < 0f ? "LockedLeftLeaf" : "LockedRightLeaf", parent);
            leaf.localPosition = new Vector3(side * leafWidth * .5f, 0f, 0f);
            // One immutable collider spans the leaf and its glazing, with no window or bottom gaps.
            var collider = leaf.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.up * Height * .5f;
            collider.size = new Vector3(leafWidth, Height, Thickness);

            const float windowBottom = 1.42f, windowHeight = .85f, windowWidth = .24f;
            float windowTop = windowBottom + windowHeight;
            float sideWidth = (leafWidth - windowWidth) * .5f;
            Box("LowerSteelPanel", leaf, Vector3.up * windowBottom * .5f,
                new Vector3(leafWidth, windowBottom, Thickness), steel, false);
            Box("UpperSteelPanel", leaf, Vector3.up * (windowTop + (Height - windowTop) * .5f),
                new Vector3(leafWidth, Height - windowTop, Thickness), steel, false);
            foreach (float edge in new[] { -1f, 1f })
                Box("SteelVisionStile", leaf,
                    new Vector3(edge * (windowWidth + sideWidth) * .5f, windowBottom + windowHeight * .5f, 0f),
                    new Vector3(sideWidth, windowHeight, Thickness), steel, false);
            Box("SolidVisionPane", leaf, new Vector3(0f, windowBottom + windowHeight * .5f, 0f),
                new Vector3(windowWidth, windowHeight, .045f), glazing, false);

            foreach (float face in new[] { -1f, 1f })
            {
                float z = face * .087f;
                foreach (float edge in new[] { -1f, 1f })
                    Box("VisionPaneFrame", leaf, new Vector3(edge * .135f, windowBottom + windowHeight * .5f, z),
                        new Vector3(.04f, windowHeight + .06f, .025f), frame, false);
                foreach (float y in new[] { windowBottom - .012f, windowTop + .012f })
                    Box("VisionPaneFrame", leaf, new Vector3(0f, y, z), new Vector3(.31f, .04f, .025f), frame, false);
                Box("SteelKickPlate", leaf, new Vector3(0f, .25f, z), new Vector3(1.07f, .34f, .016f), hardware, false);
                float handleX = -side * .43f;
                Box("HandlePlate", leaf, new Vector3(handleX, 1.14f, z), new Vector3(.115f, .45f, .02f), frame, false);
                foreach (float y in new[] { .99f, 1.30f })
                    HqSalvageProps.Rod("HandleMount", leaf, new Vector3(handleX, y, z),
                        new Vector3(handleX, y, face * .155f), .026f, hardware);
                HqSalvageProps.Rod("SteelPullHandle", leaf, new Vector3(handleX, .99f, face * .155f),
                    new Vector3(handleX, 1.30f, face * .155f), .031f, hardware);
                Primitive(PrimitiveType.Cylinder, "KeyCylinder", leaf, new Vector3(handleX, .87f, face * .102f),
                    new Vector3(.042f, .010f, .042f), rubber, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            foreach (float y in new[] { .44f, 1.37f, 2.30f })
            {
                Primitive(PrimitiveType.Cylinder, "HeavyHinge", leaf, new Vector3(side * .55f, y, -.095f),
                    new Vector3(.052f, .086f, .052f), hardware, false);
                Box("HingePlate", leaf, new Vector3(side * .52f, y, -.089f), new Vector3(.12f, .17f, .024f), frame, false);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>A restrained set of mounted identification and exit signs replaces the scattered notices.</summary>
    public static class MeridianDressingSigns
    {
        private const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public static void Place(Transform mall, Transform group)
        {
            var serviceFloors = new System.Collections.Generic.HashSet<int>();
            foreach (MallLayout.Door door in MallLayout.Doors)
            {
                MallLayout.Zone? a = MallLayout.ZoneAt(door.A, door.Floor), b = MallLayout.ZoneAt(door.B, door.Floor);
                bool serviceA = a.HasValue && a.Value.Tag == "stock", serviceB = b.HasValue && b.Value.Tag == "stock";
                if (serviceA == serviceB || !serviceFloors.Add(door.Floor)) continue;
                Vector2Int direction = serviceA ? door.B - door.A : door.A - door.B;
                Vector3 front = new(direction.x, 0f, direction.y);
                Vector3 at = (MallLayout.TileTopCenter(door.A, door.Floor) + MallLayout.TileTopCenter(door.B, door.Floor)) * 0.5f;
                Sign(group, at + front * 0.17f + Vector3.up * 3.55f, front, "SERVICE", 1.2f, 0.20f);
            }
            var entrance = System.Array.Find(MallLayout.Exterior, e => e.floor == 0 && e.side == 'S' && !e.window);
            float entrancePlane = MallLayout.ExteriorPosition(entrance).z;
            // The anchor fins project half a metre beyond the wall; the fascia sits in front of them.
            Sign(group, new Vector3(MallLayout.EntranceTarget.x, 7.1f, entrancePlane - 0.62f), Vector3.back, "MERIDIAN MALL", 10f, 0.7f);
            foreach (var exit in MallLayout.Exterior)
            {
                Vector3 front = exit.side switch
                {
                    'S' => Vector3.forward, 'N' => Vector3.back, 'E' => Vector3.left, _ => Vector3.right,
                };
                Vector3 at = MallLayout.ExteriorPosition(exit) + front * 0.18f + Vector3.up * (exit.window ? 2.7f : 2.75f);
                Sign(group, at, front, "EXIT", 0.9f, 0.20f);
            }
        }

        public static void Sign(Transform group, Vector3 at, Vector3 front, string text, float width, float height,
            float lineSpacing = 1f)
        {
            Transform holder = Group("Sign_" + text.Replace(' ', '_').Replace('/', '_'), group);
            holder.SetLocalPositionAndRotation(at, Quaternion.LookRotation(front));
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "Fascia";
            face.transform.SetParent(holder, false);
            face.transform.localScale = new Vector3(width, height, 0.035f);
            Object.DestroyImmediate(face.GetComponent<Collider>());
            face.GetComponent<Renderer>().sharedMaterial = MeridianArt.Material("StorefrontTrim");
            var label = new GameObject("Letters").AddComponent<TextMesh>();
            label.transform.SetParent(holder, false);
            label.transform.SetLocalPositionAndRotation(Vector3.forward * 0.023f, Quaternion.Euler(0f, 180f, 0f));
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 64;
            label.characterSize = 1f;
            label.lineSpacing = lineSpacing;
            label.color = new Color(0.40f, 0.41f, 0.34f);
            WorldTextMaterial.Apply(label, AssetDatabase.LoadAssetAtPath<Font>(FontPath));
            Fit(label, width * 0.90f, height * 0.76f);
            face.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Fit(TextMesh label, float width, float height)
        {
            Font font = label.font;
            if (font == null) throw new System.InvalidOperationException("Meridian signs require their authored font.");
            font.RequestCharactersInTexture(label.text, label.fontSize, label.fontStyle);
            WorldTextMaterial.Apply(label, font);
            // Force native TextMesh generation after assigning the actual font, before measuring its geometry.
            string text = label.text;
            label.text = string.Empty;
            label.text = text;
            var renderer = label.GetComponent<MeshRenderer>();
            Bounds bounds = renderer.localBounds;
            if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f)
                throw new System.InvalidOperationException($"Meridian sign has no generated glyph bounds: {text}");
            float scale = Mathf.Min(width / bounds.size.x, height / bounds.size.y);
            label.transform.localScale = Vector3.one * scale;
            label.transform.localPosition = Vector3.forward * 0.023f
                - label.transform.localRotation * (bounds.center * scale);
        }

    }
}

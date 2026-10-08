using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Landmarks and original mall identity share the same authoritative zone labels.</summary>
    public static class MeridianDressingSigns
    {
        private const string FontPath = "Assets/_Project/Art/ThirdParty/Fonts/BarlowCondensed/BarlowCondensed-SemiBold.ttf";

        public static void Place(Transform mall, Transform group)
        {
            foreach (MallLayout.Door door in MallLayout.Doors)
            {
                MallLayout.Zone? a = MallLayout.ZoneAt(door.A, door.Floor), b = MallLayout.ZoneAt(door.B, door.Floor);
                MallLayout.Zone? store = a.HasValue && a.Value.Kind == MallLayout.Kind.Store ? a
                    : b.HasValue && b.Value.Kind == MallLayout.Kind.Store ? b : null;
                if (!store.HasValue) continue;
                Vector2Int direction = store.Value.Contains(door.A) ? door.B - door.A : door.A - door.B;
                Vector3 front = new(direction.x, 0f, direction.y);
                Vector3 at = (MallLayout.TileTopCenter(door.A, door.Floor) + MallLayout.TileTopCenter(door.B, door.Floor)) * 0.5f;
                string label = string.IsNullOrEmpty(store.Value.DisplayName) ? store.Value.Name : store.Value.DisplayName;
                Sign(group, at + front * 0.17f + Vector3.up * 3.55f, front, label.ToUpperInvariant(), 3.4f, 0.32f);
                if (store.Value.Tag == "stock")
                    Sign(group, at + front * 0.19f + Vector3.up * 2.72f, front, "SERVICE / SAFE STAIRS", 2.6f, 0.26f);
            }
            var entrance = System.Array.Find(MallLayout.Exterior, e => e.floor == 0 && e.side == 'S' && !e.window);
            float entrancePlane = MallLayout.ExteriorPosition(entrance).z;
            // The anchor fins project half a metre beyond the wall; the fascia sits in front of them.
            Sign(group, new Vector3(MallLayout.EntranceTarget.x, 7.1f, entrancePlane - 0.62f), Vector3.back, "MERIDIAN MALL", 10f, 0.7f);
            Sign(group, new Vector3(MallLayout.EntranceTarget.x, 3.74f, entrancePlane - 4.12f), Vector3.back,
                "WE WILL REOPEN SHORTLY / EST. 2009", 6.4f, 0.22f);
            Sign(group, new Vector3(28f, 2.55f, 47.76f), Vector3.back, "FOOD COURT / FOOD SUBJECT TO AVAILABILITY", 8.4f, 0.42f);
            Sign(group, new Vector3(40.24f, 2.2f, 30f), Vector3.right, "STAFF ONLY / EVERYONE LEFT", 2.8f, 0.35f);
            Sign(group, new Vector3(55.7f, 2.6f, 34f), Vector3.left, "LOADING / TRUCK", 2.6f, 0.32f);
            Sign(group, new Vector3(40.24f, -1.8f, 42f), Vector3.right, "LEVEL -1 / NO REFUNDS BELOW THIS LINE", 3.6f, 0.34f);
            foreach (var exit in MallLayout.Exterior)
            {
                Vector3 front = exit.side switch
                {
                    'S' => Vector3.forward, 'N' => Vector3.back, 'E' => Vector3.left, _ => Vector3.right,
                };
                Vector3 at = MallLayout.ExteriorPosition(exit) + front * 0.18f + Vector3.up * (exit.window ? 2.7f : 2.75f);
                Sign(group, at, front, exit.window ? "ROPE EXIT / OPEN WINDOW" : "EXIT", exit.window ? 2.5f : 1.2f, 0.28f);
            }
            Directory(group);
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

        private static void Directory(Transform group)
        {
            const string map = "MERIDIAN\n02  FINAL REEL / GALLERY\n01  TOYS / HOME / TERRACE\n00  SHOPS / FOOD COURT\nSERVICE & LOADING  >";
            Quaternion rotation = Quaternion.Euler(0f, 20f, 0f);
            Vector3 front = rotation * Vector3.forward;
            Sign(group, new Vector3(18f, 1.5f, 9.4f) + front * 0.34f, front, map, 1.14f, 1.5f, 1.35f);
        }
    }
}

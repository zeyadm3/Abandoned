using System;
using System.Collections.Generic;
using System.IO;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.3 achievement icons: a 256 px pair (achieved / not yet) for each achievement, drawn in code in the
    /// company style: a symbol in a hazard-yellow stamped frame on slate, or grey when locked. Written to
    /// Tools/Steam/AchievementIcons for the partner site, and as in-game icons (Art/Generated/AchievementIcons,
    /// "achievement/&lt;id&gt;" in UiIcons) shown on the achievements screen.
    /// </summary>
    public static class Release0243
    {
        private const int Size = 256;
        public const string Folder = "Assets/_Project/Art/Generated/AchievementIcons";

        [MenuItem("Tools/Abandoned/Content/Achievement Icons (0.24.3)")]
        public static void Author()
        {
            PolishAssets.EnsureFolder(Folder);
            string steam = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Tools", "Steam", "AchievementIcons");
            Directory.CreateDirectory(steam);
            AchievementCatalog catalog = Achievements.Catalog;
            int n = 0;
            foreach (AchievementDefinition a in catalog.Items)
            {
                if (a == null) continue;
                List<Func<Vector2, bool>> symbol = Symbol(a.Stat);
                byte[] on = Draw(symbol, true), off = Draw(symbol, false);
                File.WriteAllBytes(Path.Combine(steam, $"{a.SteamName}.png"), on);
                File.WriteAllBytes(Path.Combine(steam, $"{a.SteamName}_locked.png"), off);
                string path = $"{Folder}/{a.Id}.png";
                File.WriteAllBytes(path, on);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                n++;
            }
            IconPacks();
            BuildScript.ApplyPlayerSettings();
            Debug.Log($"[Icons0243] {n} achievement icon pairs written to {steam}.");
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);

        // In-game: UiContentBuilder's icon packs include the folder ("achievement/<id>").
        private static void IconPacks() => UiContentBuilder.BuildIcons();

        private static byte[] Draw(List<Func<Vector2, bool>> symbol, bool achieved)
        {
            Color bg = achieved ? new Color(0.07f, 0.08f, 0.1f) : new Color(0.1f, 0.1f, 0.1f);
            Color ink = achieved ? new Color(0.95f, 0.75f, 0.16f) : new Color(0.38f, 0.39f, 0.4f);
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var px = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                // A stamped frame, with hazard stripes along the bottom when achieved.
                bool frame = p.x < 0.06f || p.x > 0.94f || p.y < 0.06f || p.y > 0.94f;
                bool stripe = achieved && p.y < 0.13f && p.y > 0.06f && Mathf.Repeat((p.x + p.y) * 10f, 1f) < 0.5f;
                bool mark = false;
                foreach (Func<Vector2, bool> s in symbol) if (s(p)) { mark = true; break; }
                px[y * Size + x] = frame || stripe || mark ? ink : bg;
            }
            tex.SetPixels(px);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            return png;
        }

        // A symbol per stat the achievement counts.
        private static List<Func<Vector2, bool>> Symbol(string stat)
        {
            var s = new List<Func<Vector2, bool>>();
            void Box(float x, float y, float w, float h) => s.Add(p => p.x >= x && p.x <= x + w && p.y >= y && p.y <= y + h);
            void Disc(float x, float y, float r) => s.Add(p => Vector2.Distance(p, new Vector2(x, y)) <= r);
            void Ring(float x, float y, float r, float t) => s.Add(p => Mathf.Abs(Vector2.Distance(p, new Vector2(x, y)) - r) <= t);
            void Line(float ax, float ay, float bx, float by, float t)
            {
                Vector2 a = new(ax, ay), b = new(bx, by), d = b - a;
                s.Add(p => Vector2.Distance(p, a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude)) <= t);
            }
            switch (stat)
            {
                case Achievements.StatRuns: Box(0.3f, 0.3f, 0.4f, 0.32f); Box(0.42f, 0.62f, 0.16f, 0.08f); break; // a toolbox
                case Achievements.StatEscapes: Line(0.25f, 0.5f, 0.7f, 0.5f, 0.05f); Line(0.55f, 0.66f, 0.72f, 0.5f, 0.05f); Line(0.55f, 0.34f, 0.72f, 0.5f, 0.05f); break; // out
                case Achievements.StatHaul: Ring(0.5f, 0.5f, 0.22f, 0.04f); Box(0.47f, 0.3f, 0.06f, 0.4f); break; // a coin
                case Achievements.StatQuotaMet: Line(0.28f, 0.5f, 0.44f, 0.34f, 0.05f); Line(0.44f, 0.34f, 0.74f, 0.68f, 0.05f); break; // a tick
                case Achievements.StatJackpots: Line(0.5f, 0.75f, 0.28f, 0.5f, 0.04f); Line(0.28f, 0.5f, 0.5f, 0.25f, 0.04f); Line(0.5f, 0.25f, 0.72f, 0.5f, 0.04f); Line(0.72f, 0.5f, 0.5f, 0.75f, 0.04f); break; // a gem
                case Achievements.StatCollapses: Box(0.22f, 0.6f, 0.56f, 0.07f); Line(0.3f, 0.5f, 0.45f, 0.3f, 0.04f); Line(0.55f, 0.48f, 0.68f, 0.28f, 0.04f); break; // a falling slab
                case Achievements.StatDeaths: Disc(0.5f, 0.55f, 0.18f); Box(0.4f, 0.28f, 0.2f, 0.12f); break; // a skull
                case Achievements.StatRevives: Box(0.44f, 0.28f, 0.12f, 0.44f); Box(0.28f, 0.44f, 0.44f, 0.12f); break; // a cross
                case Achievements.StatHunterFalls: Box(0.3f, 0.55f, 0.4f, 0.2f); Line(0.5f, 0.5f, 0.5f, 0.2f, 0.05f); break;
                case Achievements.StatDarkEscapes: Ring(0.5f, 0.5f, 0.2f, 0.05f); Disc(0.58f, 0.58f, 0.16f); break; // a moon
                case Achievements.StatBreakers: Box(0.38f, 0.25f, 0.24f, 0.5f); Line(0.62f, 0.55f, 0.78f, 0.72f, 0.04f); break;
                case Achievements.StatVaults: Ring(0.5f, 0.5f, 0.24f, 0.04f); Disc(0.5f, 0.5f, 0.07f); break;
                case Achievements.StatLiftRides: Box(0.3f, 0.3f, 0.4f, 0.05f); Line(0.5f, 0.38f, 0.5f, 0.72f, 0.04f); Line(0.4f, 0.62f, 0.5f, 0.72f, 0.04f); Line(0.6f, 0.62f, 0.5f, 0.72f, 0.04f); break;
                case Achievements.StatVents: for (int i = 0; i < 4; i++) Box(0.28f, 0.3f + i * 0.12f, 0.44f, 0.05f); break;
                case Achievements.StatSets: Box(0.25f, 0.4f, 0.14f, 0.2f); Box(0.43f, 0.4f, 0.14f, 0.2f); Box(0.61f, 0.4f, 0.14f, 0.2f); break;
                case Achievements.StatBossRequests: Box(0.32f, 0.25f, 0.36f, 0.46f); Line(0.38f, 0.6f, 0.62f, 0.6f, 0.02f); Line(0.38f, 0.5f, 0.62f, 0.5f, 0.02f); break;
                case Achievements.StatRushOrders: Ring(0.5f, 0.5f, 0.22f, 0.04f); Line(0.5f, 0.5f, 0.5f, 0.66f, 0.03f); Line(0.5f, 0.5f, 0.62f, 0.5f, 0.03f); break;
                case Achievements.StatHires: Disc(0.5f, 0.62f, 0.1f); Box(0.36f, 0.28f, 0.28f, 0.2f); break;
                case Achievements.StatPings: Line(0.5f, 0.75f, 0.3f, 0.5f, 0.05f); Line(0.3f, 0.5f, 0.5f, 0.25f, 0.05f); Line(0.5f, 0.25f, 0.7f, 0.5f, 0.05f); Line(0.7f, 0.5f, 0.5f, 0.75f, 0.05f); break;
                case Achievements.StatOvertime: Ring(0.5f, 0.5f, 0.22f, 0.04f); Line(0.5f, 0.5f, 0.5f, 0.68f, 0.03f); Line(0.5f, 0.5f, 0.36f, 0.42f, 0.03f); break;
                default: Disc(0.5f, 0.5f, 0.18f); break;
            }
            return s;
        }
    }
}

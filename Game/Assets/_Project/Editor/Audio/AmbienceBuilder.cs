using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Adds a level's ambience (M7.3): wind, light hum and distant settling.</summary>
    public static class AmbienceBuilder
    {
        /// <summary>The mall: windy and humming, with things settling now and then.</summary>
        public static void Mall(Transform root) => Add(root, wind: 0.22f, hum: 0.1f, plainHum: 0f, settleEvery: new Vector2(20f, 50f));

        /// <summary>The HQ: quieter wind, a steady hum from its lamps, nothing settling (it's safe).</summary>
        public static void Hq(Transform root) => Add(root, wind: 0.08f, hum: 0.06f, plainHum: 0.06f, settleEvery: Vector2.zero);

        private static void Add(Transform root, float wind, float hum, float plainHum, Vector2 settleEvery)
        {
            var go = new GameObject("Ambience");
            go.transform.SetParent(root, false);
            go.AddComponent<LevelAmbience>().EditorSetup(wind, hum, plainHum, settleEvery);
        }
    }
}

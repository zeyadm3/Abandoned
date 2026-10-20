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

        /// <summary>The Iron Factory: draughts through a big hall, a low hum, iron ticking and settling often.</summary>
        public static void Factory(Transform root) => Add(root, wind: 0.26f, hum: 0.08f, plainHum: 0.04f, settleEvery: new Vector2(15f, 40f));

        /// <summary>The island prison: sea wind through every gap, no hum (the power plant is long dead), the cellhouse settling.</summary>
        public static void Island(Transform root) => Add(root, wind: 0.34f, hum: 0f, plainHum: 0.02f, settleEvery: new Vector2(18f, 45f));

        /// <summary>Burley Middle School: wind down empty school halls, distant locker rattles, flickering light hum.</summary>
        public static void School(Transform root) => Add(root, wind: 0.22f, hum: 0.12f, plainHum: 0.04f, settleEvery: new Vector2(16f, 44f));

        /// <summary>The training house: winter wind at the windows, no hum (the power is off), the old frame settling now and then.</summary>
        public static void House(Transform root) => Add(root, wind: 0.18f, hum: 0f, plainHum: 0f, settleEvery: new Vector2(25f, 60f));

        private static void Add(Transform root, float wind, float hum, float plainHum, Vector2 settleEvery)
        {
            var go = new GameObject("Ambience");
            go.transform.SetParent(root, false);
            go.AddComponent<LevelAmbience>().EditorSetup(wind, hum, plainHum, settleEvery);
        }
    }
}

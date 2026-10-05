using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>
    /// Physics layer names (created in TagManager by the Editor layer setup) and the collision
    /// rules between them, applied at startup so they can't drift in a settings file.
    /// </summary>
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Loot = "Loot";
        public const string Debris = "Debris";
        public const string Structure = "Structure";

        public static readonly string[] All = { Player, Loot, Debris, Structure };

        public static int PlayerLayer => LayerMask.NameToLayer(Player);
        public static int LootLayer => LayerMask.NameToLayer(Loot);
        public static int DebrisLayer => LayerMask.NameToLayer(Debris);
        public static int StructureLayer => LayerMask.NameToLayer(Structure);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplyCollisionRules()
        {
            int debris = DebrisLayer;
            if (debris < 0) return;
            // Debris is cosmetic: it must never block players or knock loot around.
            if (PlayerLayer >= 0) Physics.IgnoreLayerCollision(debris, PlayerLayer, true);
            if (LootLayer >= 0) Physics.IgnoreLayerCollision(debris, LootLayer, true);
        }
    }
}

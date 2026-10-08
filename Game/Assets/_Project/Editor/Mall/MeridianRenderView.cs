using UnityEngine;

namespace Abandoned.EditorTools
{
    public readonly struct MeridianRenderView
    {
        public readonly string Name;
        public readonly Vector3 Eye, Target;
        public MeridianRenderView(string name, Vector3 eye, Vector3 target)
        { Name = name; Eye = eye; Target = target; }

        // Coordinates follow the authored fountain, retail rooms, service spine and parking spawn.
        public static readonly MeridianRenderView[] Authored =
        {
            new("atrium", new Vector3(31f, 1.7f, 13.5f), new Vector3(28f, 3.5f, 26f)),
            new("food-court", new Vector3(18f, 1.7f, 41.5f), new Vector3(24f, 1.8f, 47.5f)),
            new("cinema", new Vector3(19f, 9.7f, 13.6f), new Vector3(15.95f, 10.6f, 14f)),
            new("dead-circuit", new Vector3(13f, 1.7f, 14f), new Vector3(8.5f, 1.2f, 10f)),
            new("service-corridor", new Vector3(42f, 1.7f, 25.5f), new Vector3(42f, 1.7f, 35f)),
            new("basement", new Vector3(42f, -2.3f, 29f), new Vector3(49f, -2.5f, 34f)),
            new("parking-night", MallLayout.SpawnCentre + Vector3.up * 1.65f, new Vector3(28f, 4f, 6f)),
            new("gallery-wound", new Vector3(26f, 9.7f, 35.5f), new Vector3(34f, 8.6f, 43f)),
        };
    }
}

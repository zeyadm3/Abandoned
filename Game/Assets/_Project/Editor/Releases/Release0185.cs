using Abandoned.Equipment;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>0.18.5 moving blanket: the catalog item, its icon and the quilt's material.</summary>
    public static class Release0185
    {
        [MenuItem(ToolMenu.Features + "0.18.5 Moving Blanket", priority = ToolMenu.FeaturesP + 10)]
        public static void Author()
        {
            var config = ReleaseKit.Config<ToolConfig>(ReleaseKit.EquipmentResources, ToolConfig.ResourcePath);
            config.EditorSetBlanketMaterial(GreyboxFactory.GetMaterial("Greybox_MovingBlanket", new Color(0.2f, 0.28f, 0.45f)));
            EditorUtility.SetDirty(config);
            ReleaseKit.Gear();
        }

        public static void AuthorAndExit() => ReleaseKit.RunAndExit(Author);
    }
}

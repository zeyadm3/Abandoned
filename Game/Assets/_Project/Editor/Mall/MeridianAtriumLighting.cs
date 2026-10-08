using Abandoned.Structure;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Four bounded hanging lights reveal the atrium without introducing a shadow or floating lamp.</summary>
    public static class MeridianAtriumLighting
    {
        public static int Place(Transform group, MeridianPresentationConfig config, int budget)
        {
            var cells = new[] {
                new Vector2Int(MallLayout.Atrium.xMin + 1, MallLayout.Atrium.yMin + 1),
                new Vector2Int(MallLayout.Atrium.xMin + 2, MallLayout.Atrium.yMin + 1),
                new Vector2Int(MallLayout.Atrium.xMin + 1, MallLayout.Atrium.yMax - 1),
                new Vector2Int(MallLayout.Atrium.xMin + 2, MallLayout.Atrium.yMax - 1),
            };
            int count = Mathf.Clamp(Mathf.Min(config.AtriumFixtures, budget), 0, cells.Length);
            float height = Mathf.Clamp(config.AtriumFixtureHeight, MallLayout.StoryHeight + 2.2f,
                MallLayout.Floors * MallLayout.StoryHeight - 1f);
            float top = height + MeridianLightingCeilingBounds.MaximumY("FluorescentFixture");
            for (int index = 0; index < count; index++)
            {
                Vector2Int cell = cells[index];
                for (int floor = 1; floor < MallLayout.Floors; floor++)
                    if (!MallLayout.IsVoid(cell, floor))
                        throw new System.InvalidOperationException($"Atrium suspension would pass through a walking slab at {cell}/{floor}.");
                Vector3 at = MallLayout.TileTopCenter(cell, 0); at.y = height;
                Transform holder = MeridianLighting.Fixture(group, cell, 1, null, config, false, at);
                float roofBase = MallLayout.Floors * MallLayout.StoryHeight + .18f;
                float ridgeHeight = 1.25f, roofSlope = .15625f;
                float anchorY = roofBase + ridgeHeight - Mathf.Abs(at.z - MallLayout.Atrium.center.y * MallLayout.Tile) * roofSlope - .12f;
                // This roof never breaks; the pendant remains power-controlled while suspended through the void.
                GameObject mount = GreyboxFactory.Primitive(PrimitiveType.Cube, "RafterMount", holder,
                    new Vector3(0f, anchorY - height, 0f), new Vector3(1.1f, .08f, .12f),
                    MeridianArt.Material("EscalatorSteel"), false);
                mount.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                foreach (float side in new[] { -1f, 1f })
                {
                    GameObject suspension = GreyboxFactory.Primitive(PrimitiveType.Cylinder, "SkylightSuspension", holder,
                        new Vector3(side * .46f, (top + anchorY) * .5f - height, 0f),
                        new Vector3(.024f, (anchorY - top) * .5f, .024f), MeridianArt.Material("EscalatorSteel"), false);
                    suspension.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            return count;
        }
    }
}

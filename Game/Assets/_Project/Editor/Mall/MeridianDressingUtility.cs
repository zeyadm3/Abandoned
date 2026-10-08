using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    public static class MeridianDressingUtility
    {
        public static Transform Group(string name, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        public static StructuralSection Support(Transform mall, Vector2Int cell, int floor)
        {
            Transform tile = mall.Find($"{MallBuilder.TilesGroup}/Floor_{floor}/{MallBuilder.TileName(cell, floor)}");
            if (tile != null) return tile.GetComponent<StructuralSection>();
            Vector3 at = MallLayout.TileTopCenter(cell, floor);
            foreach (StructuralSection section in mall.GetComponentsInChildren<StructuralSection>(true))
                if ((section.transform.position - at).sqrMagnitude < 0.04f) return section;
            return null;
        }

        public static GameObject Prop(string key, Transform group, Vector3 position, Quaternion rotation,
            StructuralSection support = null, Vector3? scale = null)
        {
            GameObject model = MeridianArt.Place(key, group, position, rotation, scale);
            if (model == null) return null;
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
            if (support != null) model.AddComponent<SectionProp>().EditorSetup(support);
            return model;
        }

        public static bool Occupied(Vector2Int cell, int floor) =>
            MallLayout.InFootprint(cell) && !MallLayout.IsVoid(cell, floor) && MallLayout.ZoneAt(cell, floor).HasValue;

        public static void Patch(string name, Transform parent, Vector3 at, Quaternion rotation, Vector2 size,
            Material material, StructuralSection support = null)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.SetLocalPositionAndRotation(at, rotation);
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            var renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            if (support != null) quad.AddComponent<SectionProp>().EditorSetup(support);
        }

        public static int Hash(Vector2Int cell, int floor) =>
            (int)((uint)(cell.x * 73856093 ^ cell.y * 19349663 ^ floor * 83492791) & 0x7fffffff);
    }
}

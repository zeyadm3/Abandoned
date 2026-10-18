using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Places fixed scene dressing on its actual floor; recoverable inventory uses loot spawns.</summary>
    internal sealed class SalvageDressingPlacement
    {
        private readonly Transform mall;

        public SalvageDressingPlacement(Transform mall) => this.mall = mall;

        public Transform Assembly(Transform parent, string name, int floor, float x, float z, float yaw = 0,
            bool ceiling = false)
        {
            Transform item = Group(name, parent);
            item.localPosition = new Vector3(x, MallLayout.FloorY(floor), z);
            item.localRotation = Quaternion.Euler(0, yaw, 0);
            Bind(item.gameObject, floor + (ceiling ? 1 : 0), x, z);
            return item;
        }

        public GameObject Model(string id, Transform parent, int floor, float x, float z, float yaw = 0, float lift = 0)
        {
            GameObject item = SalvageAssetBuilder.Place(id, parent,
                new Vector3(x, MallLayout.FloorY(floor) + lift, z), yaw);
            // 0.24.9: a loot model never stands as scenery; its place goes to a fixture or a dust sheet.
            if (item != null && LootLookalikes.Is(id)) item = LootLookalikeStandIns.Swap(item, id, false);
            if (item != null) Bind(item, floor, x, z);
            return item;
        }

        public GameObject Inside(string id, Transform parent, Vector3 at, float yaw = 0)
        {
            GameObject item = SalvageAssetBuilder.Place(id, parent, at, yaw);
            return LootLookalikes.Is(id) ? LootLookalikeStandIns.Swap(item, id, true) : item;
        }

        public void Sign(Transform parent, Vector3 at, float yaw, string text, float width, float height)
        {
            // Plain operation captions duplicated the actual salvage HUD and overwhelmed the printed retail artwork.
        }

        public void Tag(Transform parent, int floor, float x, float z, string text, float yaw = 0, float y = 1.45f)
        {
            // Retain this authoring API for old layouts, without rebuilding empty plates or world-text status tags.
        }

        public Transform DisplayCase(Transform parent, int floor, float x, float z, int damage)
        {
            Transform item = Assembly(parent, "InterruptedTechDisplay_" + damage, floor, x, z);
            Box(item, "display plinth", new Vector3(0, .16f, 0), new Vector3(2.2f, .32f, .82f), "Metal");
            Box(item, "empty case tray", new Vector3(0, .335f, 0), new Vector3(2.1f, .025f, .74f), "Plastic");
            for (int xSide = -1; xSide <= 1; xSide += 2)
                for (int zSide = -1; zSide <= 1; zSide += 2)
                    Box(item, "display frame", new Vector3(xSide * 1.055f, .72f, zSide * .37f), new Vector3(.026f, .78f, .026f), "Metal");
            Box(item, "top frame rear", new Vector3(0, 1.10f, .37f), new Vector3(2.14f, .026f, .026f), "Metal");
            if (damage < 2)
            {
                Box(item, "surviving top glass", new Vector3(damage == 0 ? 0 : -.56f, 1.1f, 0),
                    new Vector3(damage == 0 ? 2.08f : .96f, .012f, .71f), "Glass");
                Box(item, "rear glass", new Vector3(0, .72f, .369f), new Vector3(2.08f, .72f, .009f), "Glass");
                Box(item, "front glass remnant", new Vector3(damage == 0 ? 0 : .78f, .72f, -.375f),
                    new Vector3(damage == 0 ? 2.08f : .48f, .72f, .009f), "Glass");
            }
            if (damage == 1) Inside("broken_glass", item, new Vector3(.4f, .352f, -.05f));
            return item;
        }

        public Transform Workbench(Transform parent, int floor, float x, float z, float yaw = 0)
        {
            Transform item = Assembly(parent, "PartlyClearedWorkbench", floor, x, z, yaw);
            Box(item, "scarred work surface", new Vector3(0, .91f, 0), new Vector3(2.20f, .075f, .8f), "Metal");
            Box(item, "lower timber shelf", new Vector3(0, .22f, 0), new Vector3(2.01f, .036f, .68f), "Wood");
            for (int xSide = -1; xSide <= 1; xSide += 2)
                for (int zSide = -1; zSide <= 1; zSide += 2)
                    Box(item, "steel bench leg", new Vector3(xSide * .98f, .43f, zSide * .30f), new Vector3(.055f, .86f, .055f), "Metal");
            return item;
        }

        public void Tracks(Transform parent, int floor, float x, float z, float yaw = 0)
        {
            Transform item = Assembly(parent, "ForkliftDustTracks", floor, x, z, yaw);
            Inside("tire_track", item, new Vector3(-.72f, .005f, 0));
            Inside("tire_track", item, new Vector3(.72f, .005f, 0));
        }

        private void Bind(GameObject item, int floor, float x, float z)
        {
            var cell = new Vector2Int(Mathf.FloorToInt(x / MallLayout.Tile), Mathf.FloorToInt(z / MallLayout.Tile));
            StructuralSection support = MeridianDressingUtility.Support(mall, cell, floor);
            SectionProp binding = item.GetComponent<SectionProp>();
            if (binding == null) binding = item.AddComponent<SectionProp>();
            binding.EditorSetup(support);
            foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>(true))
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
        }
    }
}

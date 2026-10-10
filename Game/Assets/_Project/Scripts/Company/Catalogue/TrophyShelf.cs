using System.Linq;
using Abandoned.Loot;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The HQ trophy shelf (0.18.7): a small copy of every jackpot the company has ever sold, best first.
    /// Rebuilt on every machine whenever the catalogue changes. A long-term goal: fill the shelf.
    /// </summary>
    public class TrophyShelf : MonoBehaviour
    {
        [SerializeField] private LootCatalog catalog;
        [SerializeField] private Transform[] slots;
        [Tooltip("Largest side (m) a trophy is shrunk to.")]
        [SerializeField, Min(0.1f)] private float fit = 0.42f;

        private int shownCount = -1;

        private void OnEnable() => CompanyService.CatalogueChanged += Refresh;
        private void OnDisable() => CompanyService.CatalogueChanged -= Refresh;

        private void Update()
        {
            CompanyService company = CompanyService.Current;
            int count = company != null && company.IsSpawned ? company.CatalogueCount : 0;
            if (count != shownCount) Refresh();
        }

        private void Refresh()
        {
            CompanyService company = CompanyService.Current;
            shownCount = company != null && company.IsSpawned ? company.CatalogueCount : 0;
            if (slots == null || catalog == null) return;
            foreach (Transform slot in slots)
                if (slot != null) for (int i = slot.childCount - 1; i >= 0; i--) Destroy(slot.GetChild(i).gameObject);
            if (company == null) return;
            var won = catalog.Entries.Select(e => e.definition).Where(d => d != null && d.Jackpot && company.Catalogued(d.Id))
                .OrderByDescending(d => d.ValueMax).ToList();
            for (int i = 0; i < slots.Length && i < won.Count; i++) Place(won[i], slots[i]);
        }

        private void Place(LootDefinition d, Transform slot)
        {
            GameObject copy = d.Model != null ? Instantiate(d.Model, slot) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            copy.transform.SetParent(slot, false);
            foreach (Collider c in copy.GetComponentsInChildren<Collider>()) Destroy(c);
            copy.transform.localRotation = Quaternion.Euler(0f, d.ModelYaw, 0f);
            Renderer[] renderers = copy.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            float largest = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (largest > 0.001f) copy.transform.localScale *= fit / largest;
            // Sit it on the shelf.
            b = renderers[0].bounds;
            foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            copy.transform.position += Vector3.up * (slot.position.y - b.min.y);
        }

#if UNITY_EDITOR
        public void EditorSetup(LootCatalog loot, Transform[] places)
        {
            catalog = loot;
            slots = places;
        }
#endif
    }
}

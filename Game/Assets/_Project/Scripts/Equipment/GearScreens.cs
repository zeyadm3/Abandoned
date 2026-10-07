using Abandoned.Company;
using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Abandoned.Equipment
{
    /// <summary>Placeholder HQ screens (OnGUI): the shop and the gear rack. Esc or Close shuts them.</summary>
    public class GearScreens : MonoBehaviour
    {
        [SerializeField] private int fontSize = 15;

        private GUIStyle box, label, title;

        private void Update()
        {
            bool open = ShopTerminal.Open || GearRack.Open;
            if (open && (CompanyService.Current == null || Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
                ShopTerminal.Open = GearRack.Open = false;
            CursorOwner.Set(this, ShopTerminal.Open || GearRack.Open);
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ShopTerminal.Open = GearRack.Open = false;
        }

        private void OnGUI()
        {
            CompanyService company = CompanyService.Current;
            if (company == null || (!ShopTerminal.Open && !GearRack.Open)) return;
            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize };
            label ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true, wordWrap = true };
            title ??= new GUIStyle(label) { fontSize = fontSize + 8, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            float w = Mathf.Min(760f, Screen.width - 40f), h = Mathf.Min(560f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), box);
            if (ShopTerminal.Open) Shop(company);
            else Rack(company);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Height(28f))) ShopTerminal.Open = GearRack.Open = false;
            GUILayout.EndArea();
        }

        private void Shop(CompanyService company)
        {
            EquipmentCatalog catalog = company.Equipment;
            int money = company.State.Money;
            GUILayout.Label("SHOP", title);
            GUILayout.Label($"Company money: {(money < 0 ? $"<color=#ff7766>DEBT ${-money:N0}</color>" : $"${money:N0}")}   Level {company.State.Level}", label);
            for (int i = 0; i < catalog.Items.Count; i++)
            {
                EquipmentDefinition d = catalog.Items[i];
                GUILayout.BeginHorizontal(box);
                GUILayout.Label($"<b>{d.DisplayName}</b>{(d.Consumable ? " (single use)" : "")}\n{d.Description}", label, GUILayout.Width(w(0.55f)));
                GUILayout.Label($"${d.Price:N0}\nowned {company.OwnedCount(i)}", label, GUILayout.Width(w(0.18f)));
                bool locked = d.UnlockLevel > company.State.Level;
                GUI.enabled = !locked && money >= d.Price;
                if (GUILayout.Button(locked ? $"Level {d.UnlockLevel}" : "Buy", GUILayout.Height(36f))) company.RequestBuy(i);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            static float w(float share) => Mathf.Min(760f, Screen.width - 40f) * share;
        }

        private void Rack(CompanyService company)
        {
            PlayerEquipment mine = null;
            foreach (PlayerEquipment e in PlayerEquipment.All) if (e != null && e.IsOwner) mine = e;
            GUILayout.Label("GEAR RACK", title);
            if (mine == null) return;
            EquipmentCatalog catalog = mine.Catalog;
            for (int slot = 0; slot < 2; slot++)
            {
                EquipmentDefinition held = mine.InSlot(slot);
                GUILayout.Label($"<b>Hand slot {slot + 1}</b> (key {slot + 1}): {(held != null ? held.DisplayName : "empty")}", label);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Empty", GUILayout.Width(90f))) mine.RequestEquip(slot, -1);
                for (int i = 0; i < catalog.Items.Count; i++)
                {
                    int free = mine.Available(i, mine) - (mine.State[1 - slot] == i ? 1 : 0);
                    if (company.OwnedCount(i) <= 0) continue;
                    GUI.enabled = free > 0 || mine.State[slot] == i;
                    if (GUILayout.Button($"{catalog.Items[i].DisplayName} ({Mathf.Max(0, free)})")) mine.RequestEquip(slot, i);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("F: flashlight. Hold R: radio. Single-use gear is used with the left mouse button when your hands are empty.", label);
        }
    }
}

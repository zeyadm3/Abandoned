using Abandoned.Contracts;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// Placeholder contract board UI (OnGUI): the three offers with everything GDD 14 lists. The host
    /// picks one ("Take this job"); everyone else reads along. Close with the button or Esc.
    /// </summary>
    public class ContractBoardScreen : MonoBehaviour
    {
        [SerializeField] private int fontSize = 15;

        private GUIStyle box, label, title;

        private void Update()
        {
            if (ContractBoard.Open && CompanyService.Current == null) ContractBoard.Open = false;
            if (ContractBoard.Open && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                ContractBoard.Open = false;
            CursorOwner.Set(this, ContractBoard.Open);
        }

        private void OnDisable()
        {
            CursorOwner.Set(this, false);
            ContractBoard.Open = false;
        }

        private void OnGUI()
        {
            CompanyService company = CompanyService.Current;
            if (!ContractBoard.Open || company == null) return;
            box ??= new GUIStyle(GUI.skin.box) { fontSize = fontSize };
            label ??= new GUIStyle(GUI.skin.label) { fontSize = fontSize, richText = true, wordWrap = true };
            title ??= new GUIStyle(label) { fontSize = fontSize + 8, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };

            float w = Mathf.Min(1000f, Screen.width - 40f), h = Mathf.Min(520f, Screen.height - 40f);
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), box);
            GUILayout.Label("CONTRACT BOARD", title);
            GUILayout.BeginHorizontal();
            var board = company.Board;
            for (int i = 0; i < board.Count; i++)
            {
                Contract c = board[i];
                GUILayout.BeginVertical(box, GUILayout.Width(w / board.Count - 12f));
                GUILayout.Label($"<b>{c.Location.ToUpperInvariant()}</b>{(company.Selected == i ? "  <color=#7dff7d>(TAKEN)</color>" : "")}", label);
                GUILayout.Label($"Estimated loot: ${c.LootMin:N0} - ${c.LootMax:N0}", label);
                GUILayout.Label($"Quota: <b>${c.Quota:N0}</b>", label);
                GUILayout.Label($"Threat level: {Contract.ThreatName(c.ThreatLevel)}", label);
                GUILayout.Label("Known threats: ???", label);
                GUILayout.Label($"Structural stability: {c.Stability:P0}", label);
                GUILayout.Label($"Power: {(c.PowerOff ? "<color=#ff7766>OFF</color>" : "ON")}", label);
                GUILayout.Label($"Extraction window: {c.WindowSeconds / 60f:0} min", label);
                GUILayout.Label($"Modifier: <b>{c.ModifierName}</b>", label);
                GUILayout.Label($"Payout bonus: +{c.PayoutBonus:P0}", label);
                GUILayout.FlexibleSpace();
                if (company.IsServer)
                {
                    if (GUILayout.Button(company.Selected == i ? "Taken - drive the van" : "Take this job", GUILayout.Height(32f))) company.Select(i);
                }
                else GUILayout.Label("<i>The host picks the job.</i>", label);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Close", GUILayout.Height(28f))) ContractBoard.Open = false;
            GUILayout.EndArea();
        }
    }
}

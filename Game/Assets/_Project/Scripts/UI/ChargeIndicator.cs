using Abandoned.Equipment;
using Abandoned.Extraction;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Flashlight recharge at the truck (0.20.2): the truck bay has topped batteries up since QA D-01, but
    /// nothing said so. While the local player's flashlight charges there, a small line under the crosshair
    /// shows it, with the battery level; a soft tick plays as it fills.
    /// </summary>
    public class ChargeIndicator : MonoBehaviour
    {
        private GUIStyle style;
        private float shownLevel = -1f, nextTick;

        private static bool Charging(out float level)
        {
            level = 0f;
            NetworkPlayer me = NetworkPlayer.Local;
            RunState run = RunState.Current;
            if (me == null || me.IsDead || run == null || !run.IsSpawned || run.State.Phase == RunPhase.Departed) return false;
            var gear = me.GetComponent<PlayerEquipment>();
            if (gear == null || !gear.Has(EquipmentKind.Flashlight) || TruckCargo.Current == null || !TruckCargo.Current.Carries(me.transform.position)) return false;
            level = gear.Battery01;
            return level < 0.999f && gear.FlashlightConfig.TruckRechargePerSecond > 0f;
        }

        private void Update()
        {
            if (!Charging(out float level))
            {
                shownLevel = -1f;
                return;
            }
            if (shownLevel >= 0f && level > shownLevel + 0.001f && Time.time >= nextTick)
            {
                nextTick = Time.time + 1.2f;
                Audio.GameAudio.PlayUi(Audio.SoundId.ChargeTick, 0.4f);
            }
            shownLevel = level;
        }

        private void OnGUI()
        {
            if (!Charging(out float level) || HudLayer.Root == null) return;
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            style.fontSize = Mathf.RoundToInt(Screen.height * 0.02f);
            GUI.color = new Color(0.55f, 0.9f, 1f, 0.9f);
            GUI.Label(new Rect(0, Screen.height * 0.58f, Screen.width, Screen.height * 0.04f), $"CHARGING FLASHLIGHT  {level * 100f:0}%", style);
            GUI.color = Color.white;
        }
    }
}

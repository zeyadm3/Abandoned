using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Skyscraper
{
    /// <summary>
    /// Interactive button for the Skyscraper Restaurant elevator (calling to a landing or sending from the cab).
    /// </summary>
    public class SkyscraperElevatorButton : MonoBehaviour, IUsable, IHintSource
    {
        [SerializeField] private SkyscraperElevator elevator;
        [Tooltip("Target floor: 0 = Street Lobby, 1 = Mechanical Deck, 2 = Apex Restaurant.")]
        [SerializeField, Range(0, 2)] private int targetFloor;
        [SerializeField] private string label = "Call Elevator";

        public HintId Hint => HintId.Lift;
        public float UseHoldDuration => 0f;

        public bool CanUse(NetworkPlayer player)
        {
            if (elevator == null || player == null || player.IsDead) return false;
            return elevator.CanAcceptRequest(targetFloor);
        }

        public string UsePrompt(GameObject user)
        {
            if (elevator == null) return string.Empty;
            if (!elevator.IsPowered) return Loc.T("Lift (No Power)");
            if (elevator.IsBroken) return Loc.T("Lift (Out of Order)");
            if (elevator.IsMoving) return Loc.T("Lift (In Transit...)");
            if (elevator.CurrentFloor == targetFloor && !elevator.IsMoving)
                return Loc.T("Lift (Already Here)");

            string floorName = targetFloor switch
            {
                0 => "Lobby (Street)",
                1 => "Mechanical (Mid)",
                2 => "Apex Restaurant",
                _ => $"Floor {targetFloor}"
            };

            return $"{Loc.T(label)}: {Loc.T(floorName)}";
        }

        public void Use(GameObject user)
        {
            if (elevator != null && user != null)
                elevator.RequestFloor(targetFloor);
        }

#if UNITY_EDITOR
        public void EditorSetup(SkyscraperElevator lift, int floor, string promptLabel)
        {
            elevator = lift;
            targetFloor = floor;
            label = promptLabel;
        }
#endif
    }
}

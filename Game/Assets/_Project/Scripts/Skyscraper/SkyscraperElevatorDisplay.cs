using UnityEngine;
namespace Abandoned.Skyscraper
{
    public class SkyscraperElevatorDisplay : MonoBehaviour
    {
        [SerializeField] private SkyscraperElevator elevator;
        [SerializeField] private TextMesh display;
        private void Update()
        {
            if(elevator==null || display==null)return;
            string text=elevator.IsBroken?"OUT OF SERVICE":!elevator.IsPowered?"POWER OFF":
                elevator.IsMoving?$"{elevator.DeckPosition.y:000} m":elevator.CurrentFloor==0?"L / LOBBY":elevator.CurrentFloor==1?"M / SERVICES":"A / APEX";
            if(display.text!=text)display.text=text;
        }
#if UNITY_EDITOR
        public void EditorSetup(SkyscraperElevator lift,TextMesh text){elevator=lift;display=text;}
#endif
    }
}

using Abandoned.Core;
using UnityEngine;

namespace Abandoned.World
{
    /// <summary>F1: the van's numbers on screen (speed, fuel, damage, engine, seats, wheels on the ground) and gizmos.</summary>
    public class VanDebugView : MonoBehaviour
    {
        [SerializeField] private WorldVan van;

        private GUIStyle style;

        private void Awake()
        {
            if (van == null) van = GetComponent<WorldVan>();
        }

        private void OnGUI()
        {
            if (!DebugView.Visible || van == null || !van.IsSpawned) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(1f, .85f, .45f) } };
            VanNetState s = van.State;
            VanSeating seating = van.Seats != null ? van.Seats.Seating : VanSeating.None;
            string seats = "";
            for (int i = 0; i < VanSeating.Count; i++) seats += seating[i] == VanSeating.Empty ? "- " : $"{seating[i]} ";
            string text = $"VAN {(van.IsServer ? "host-simulated" : "following")}  {van.Drive.SpeedKmh:0} km/h  grounded {van.Drive.Grounded}\n" +
                          $"fuel {s.Fuel:0.0} L  damage {s.Damage:0}%  engine {s.EngineOn}  broken {s.BrokenDown}  ramp {(s.RampDown ? "down" : "up")}  " +
                          $"repair {(s.Repairing ? $"{s.RepairProgress:P0}" : "-")}\nseats [{seats}]  steer {s.Steer:+0.00;-0.00}  pos {van.transform.position:F0}";
            GUI.Label(new Rect(12f, Screen.height - 90f, 900f, 80f), text, style);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || van == null) return;
            Rigidbody body = van.Drive != null ? van.Drive.Body : null;
            if (body == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(body.worldCenterOfMass, .15f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(body.worldCenterOfMass, body.worldCenterOfMass + body.linearVelocity * .5f);
            if (van.Seats == null) return;
            Gizmos.color = Color.green;
            for (int i = 0; i < VanSeating.Count; i++)
                if (van.Seats.SeatPoint(i) != null) Gizmos.DrawWireSphere(van.Seats.SeatPoint(i).position, .12f);
        }
    }
}

using UnityEngine;

namespace Abandoned.UI
{
    /// <summary>
    /// Where the title screen's camera stands in a level (eye height, looking down something long and
    /// dark). Levels without one keep their scene camera's own view behind the menu.
    /// </summary>
    public class MenuVantage : MonoBehaviour
    {
        [SerializeField, Range(30f, 90f)] private float fieldOfView = 58f;

        public float FieldOfView => fieldOfView;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.9f, 0.6f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }
    }
}

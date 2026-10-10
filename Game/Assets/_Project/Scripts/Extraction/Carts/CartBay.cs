using UnityEngine;

namespace Abandoned.Extraction
{
    /// <summary>Where a shopping cart stands at the start of a job (0.18.4). A scene marker; RunCarts spawns the cart.</summary>
    public class CartBay : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.8f, 1f, 0.6f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * 0.5f, new Vector3(0.6f, 1f, 0.95f));
        }
    }
}

using Abandoned.Loot;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The break room's dartboard (0.19.6): host, a dart striking it scores by how near the bull it lands, and sticks a moment.</summary>
    public class DartBoard : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float radius = 0.23f;

        private void OnCollisionEnter(Collision collision)
        {
            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || collision.rigidbody == null) return;
            LootItem dart = collision.rigidbody.GetComponent<LootItem>();
            if (dart == null || dart.Definition == null || dart.Definition.Id != "dart") return;
            Vector3 local = transform.InverseTransformPoint(collision.GetContact(0).point);
            float r = new Vector2(local.x * transform.lossyScale.x, local.y * transform.lossyScale.y).magnitude / radius;
            int score = r < 0.08f ? 50 : r < 0.2f ? 25 : r < 0.5f ? 15 : r < 0.8f ? 10 : r <= 1.05f ? 5 : 0;
            if (score == 0) return;
            // Stuck in the board for a few seconds, then it drops for the next throw.
            Rigidbody body = collision.rigidbody;
            body.isKinematic = true;
            StartCoroutine(Release(body));
            string who = (HqToys.Current != null ? HqToys.Current.Thrower(dart) : null) ?? Core.Loc.Word("Someone");
            // English and its values: every machine words the toast itself (CompanyService.Toys).
            CompanyService.Current?.ServerToyScore(score == 50 ? Core.Loc.Wire("BULLSEYE! {0}", who) : Core.Loc.Wire("{0} scores {1}.", who, score));
        }

        private static System.Collections.IEnumerator Release(Rigidbody body)
        {
            yield return new WaitForSeconds(3f);
            // Pulled out by hand meanwhile: the hold decides its physics now (a client carrier owns it).
            if (body != null && (!body.TryGetComponent(out Networking.NetworkLoot loot) || loot.Hold.Mode == Networking.LootHoldMode.Free))
                body.isKinematic = false;
        }
    }
}

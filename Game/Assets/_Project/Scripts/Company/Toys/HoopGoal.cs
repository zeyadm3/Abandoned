using System.Collections.Generic;
using Abandoned.Loot;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>The hoop's net (0.19.6): host, a basketball dropping down through it scores for whoever threw it.</summary>
    public class HoopGoal : MonoBehaviour
    {
        private readonly Dictionary<LootItem, float> scoredAt = new();

        private void OnTriggerEnter(Collider other)
        {
            NetworkManager net = NetworkManager.Singleton;
            if (net == null || !net.IsServer) return;
            LootItem ball = other.GetComponentInParent<LootItem>();
            if (ball == null || ball.Definition == null || ball.Definition.Id != "basketball") return;
            if (!other.attachedRigidbody || other.attachedRigidbody.linearVelocity.y > -0.3f) return; // through it, downwards
            if (scoredAt.TryGetValue(ball, out float t) && Time.time - t < 1.5f) return;
            scoredAt[ball] = Time.time;
            string who = (HqToys.Current != null ? HqToys.Current.Thrower(ball) : null) ?? Core.Loc.Word("Someone");
            CompanyService.Current?.ServerToyScore(Core.Loc.Wire("{0} sinks a basket!", who));
        }
    }
}

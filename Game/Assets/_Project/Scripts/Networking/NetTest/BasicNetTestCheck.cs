using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// The 'basic' scenario's verdict, kept free of networking so EditMode tests can feed it good and
    /// bad data: every machine sees every player, each player is where its owner says it is on every
    /// machine, and every client really moved.
    /// </summary>
    public static class BasicNetTestCheck
    {
        public static List<string> Verify(IReadOnlyList<NetTestView> views, IReadOnlyList<NetTestMove> moves,
            int expectedPlayers, int expectedMovers, float tolerance, float minDistance)
        {
            var errors = new List<string>();
            if (views.Count != expectedPlayers)
                errors.Add($"expected a view from each of {expectedPlayers} machines, got {views.Count}");

            foreach (NetTestView view in views)
            {
                int distinct = view.players.Select(p => p.owner).Distinct().Count();
                if (view.players.Count != expectedPlayers || distinct != expectedPlayers)
                    errors.Add($"machine {view.observer} sees {view.players.Count} players ({distinct} distinct), expected {expectedPlayers}");
            }

            if (moves.Count != expectedMovers)
                errors.Add($"expected {expectedMovers} scripted moves, got {moves.Count}");
            foreach (NetTestMove move in moves)
                if (move.HorizontalDistance < minDistance)
                    errors.Add($"player {move.owner} moved only {move.HorizontalDistance:0.00} m (need {minDistance:0.00})");

            foreach (ulong owner in views.SelectMany(v => v.players).Select(p => p.owner).Distinct().OrderBy(o => o))
            {
                if (!TryReference(owner, views, moves, out Vector3 reference, out string source))
                {
                    errors.Add($"no reference position for player {owner} (its own machine didn't report it)");
                    continue;
                }
                foreach (NetTestView view in views)
                {
                    NetTestPlayerSnapshot seen = view.players.FirstOrDefault(p => p.owner == owner);
                    if (seen == null) continue; // already reported as a missing player above
                    float off = Vector3.Distance(seen.position, reference);
                    if (off > tolerance)
                        errors.Add($"machine {view.observer} sees player {owner} at {seen.position} - {off:0.00} m from {source} {reference} (tolerance {tolerance:0.00})");
                }
            }
            return errors;
        }

        // A mover's own end position is the truth; anyone else is where its owner sees itself.
        private static bool TryReference(ulong owner, IReadOnlyList<NetTestView> views, IReadOnlyList<NetTestMove> moves,
            out Vector3 reference, out string source)
        {
            NetTestMove move = moves.FirstOrDefault(m => m.owner == owner);
            if (move != null)
            {
                reference = move.end;
                source = "where its owner stopped";
                return true;
            }
            NetTestPlayerSnapshot self = views.FirstOrDefault(v => v.observer == owner)?.players.FirstOrDefault(p => p.owner == owner);
            reference = self?.position ?? default;
            source = "where its owner sees it";
            return self != null;
        }
    }
}

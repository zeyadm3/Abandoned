using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Networked structure end to end: every client stands on TestBuilding's rotten tile (Tile_U_3_3,
    /// ~220 kg capacity) and the host drops a server rack in the middle of it. The host computes the
    /// load from the clients' replicated positions plus the rack, drains the tile, runs its Failing
    /// window and collapses it. Every machine then reports which sections it shows collapsed (id, seed,
    /// colliders) and how far its own player fell; the host checks they all agree (<see cref="CollapseNetTestCheck"/>).
    /// </summary>
    public sealed class CollapseNetTestScenario : INetTestScenario
    {
        public const string ScenarioName = "collapse";
        public const string TargetName = "Tile_U_3_3";
        public const string HeavyItemId = "server_rack";
        /// <summary>From the upper floor (4 m) to the ground floor a player drops ~3.5 m; well short of that means they never fell.</summary>
        public const float MinFall = 2f;
        private const float ConnectTimeout = 40f, StepTimeout = 20f, CollapseTimeout = 30f;
        private const float CornerOffset = 1.2f, StandHeight = 0.05f, DropHeight = 1.5f;
        // Teleports reach the host one interpolation delay later; after the collapse players need time to land.
        private const float ArriveTime = 0.75f, FallTime = 3f;
        private const string Stand = "stand", Standing = "standing", Report = "collapsereport", Done = "done";
        private static readonly Vector2[] Corners = { new(-1f, -1f), new(1f, -1f), new(-1f, 1f), new(1f, 1f) };

        public string Name => ScenarioName;

        public IEnumerator RunHost(NetTestContext ctx)
        {
            int players = ctx.ExpectedPlayers;
            StructureNetSync sync = null;
            yield return ctx.WaitFor(() => ctx.Manager.ConnectedClientsIds.Count == players && ctx.Players.Count() == players
                                           && (sync = StructureNetSync.For(ctx.Manager)) != null && sync.Simulation != null,
                $"{players} connected players and the structure sync", ConnectTimeout);
            if (ctx.Aborted) yield break;
            StructuralSection target = sync.Simulation.Sections.FirstOrDefault(s => s.name == TargetName);
            NetworkObject rack = LootNetTestScenario.FindPrefab(ctx.Manager, HeavyItemId);
            if (target == null || rack == null)
            {
                ctx.Abort(target == null ? $"no section named {TargetName} here" : $"no network prefab for '{HeavyItemId}'");
                yield break;
            }
            ctx.Result.collapseTarget = target.Id;
            ctx.Note($"target {TargetName} (#{target.Id}): {target.Stage}, hp {target.HealthFraction:P0}, capacity {target.Capacity:0} kg");

            Bounds surface = target.SurfaceBounds;
            int corner = 0;
            foreach (NetworkPlayer p in ctx.Players.Where(p => p.OwnerClientId != ctx.Manager.LocalClientId))
            {
                Vector2 c = Corners[corner++ % Corners.Length] * CornerOffset;
                Vector3 at = new(surface.center.x + c.x, surface.max.y + StandHeight, surface.center.z + c.y);
                ctx.Channel.Send(p.OwnerClientId, Stand, Format(at));
            }
            var standing = new List<NetTestMessage>();
            yield return ctx.Collect(Standing, ctx.ExpectedClients, standing, StepTimeout);
            if (ctx.Aborted) yield break;
            yield return NetTestContext.Seconds(ArriveTime);

            ctx.Note($"load on {TargetName} from the clients' replicated players: {target.Load:0} kg");
            Vector3 drop = new(surface.center.x, surface.max.y + DropHeight, surface.center.z);
            ctx.Manager.SpawnManager.InstantiateAndSpawn(rack, position: drop, rotation: Quaternion.identity);
            float started = Time.realtimeSinceStartup;
            yield return ctx.WaitFor(() => target.IsCollapsed, $"{TargetName} to collapse", CollapseTimeout);
            if (ctx.Aborted)
            {
                ctx.Note($"{TargetName} at timeout: {target.Stage}, hp {target.HealthFraction:P0}, load {target.Load:0}/{target.Capacity:0} kg");
                yield break;
            }
            ctx.Note($"{TargetName} collapsed {Time.realtimeSinceStartup - started:0.0} s after the drop, seed {target.CollapseSeed}");
            yield return NetTestContext.Seconds(FallTime);

            ctx.Channel.SendToClients(Report);
            NetTestCollapseView own = NetTestCollapseView.Take(ctx.Manager.LocalClientId, sync.Simulation);
            ctx.Result.collapseViews.Add(own);
            var reports = new List<NetTestMessage>();
            yield return ctx.Collect(Report, ctx.ExpectedClients, reports, StepTimeout);
            if (ctx.Aborted) yield break;
            foreach (NetTestMessage m in reports) ctx.Result.collapseViews.Add(JsonUtility.FromJson<NetTestCollapseView>(m.Payload));

            foreach (string error in CollapseNetTestCheck.Verify(ctx.Result.collapseViews, players, target.Id, MinFall))
                ctx.Fail(error);
            ctx.Channel.SendToClients(Done, ctx.Result.errors.Count == 0 ? "pass" : "fail");
            yield return NetTestContext.Seconds(0.5f);
        }

        public IEnumerator RunClient(NetTestContext ctx)
        {
            StructureNetSync sync = null;
            yield return ctx.WaitFor(() => ctx.Manager.IsConnectedClient && ctx.OwnPlayer != null
                                           && (sync = StructureNetSync.For(ctx.Manager)) != null && sync.Simulation != null,
                "own player and the structure sync", ConnectTimeout);
            if (ctx.Aborted) yield break;
            Vector3 at = default;
            bool parsed = false;
            yield return ctx.Receive(Stand, ConnectTimeout, m => parsed = TryParse(m.Payload, out at));
            if (ctx.Aborted) yield break;
            if (!parsed)
            {
                ctx.Abort("couldn't read where to stand");
                yield break;
            }

            NetworkPlayer player = ctx.OwnPlayer;
            player.OwnerTeleport(at);
            yield return NetTestContext.Seconds(ArriveTime);
            var view = new NetTestCollapseView { stoodOnTarget = true, startHeight = player.transform.position.y };
            view.lowestHeight = view.startHeight;
            ctx.Note($"standing at {player.transform.position}; structure mirror {sync.Simulation.IsMirror}");
            ctx.Channel.SendToHost(Standing, Format(player.transform.position));

            // Track the lowest point of this machine's own player (its ragdoll while down) until asked to report.
            float end = Time.realtimeSinceStartup + CollapseTimeout + FallTime + StepTimeout;
            while (!ctx.Channel.TryTake(Report, out _))
            {
                float y = player.Ragdoll.IsRagdolled ? player.Ragdoll.BodyPosition.y : player.transform.position.y;
                view.lowestHeight = Mathf.Min(view.lowestHeight, y);
                if (!ctx.Manager.IsListening || Time.realtimeSinceStartup > end)
                {
                    ctx.Abort("never asked to report (session ended or the host timed out)");
                    yield break;
                }
                yield return null;
            }

            NetTestCollapseView seen = NetTestCollapseView.Take(ctx.Manager.LocalClientId, sync.Simulation);
            seen.stoodOnTarget = view.stoodOnTarget;
            seen.startHeight = view.startHeight;
            seen.lowestHeight = view.lowestHeight;
            ctx.Result.collapseViews.Add(seen);
            ctx.Note($"fell {seen.Fall:0.00} m; collapsed here: {string.Join(", ", seen.collapsed.Select(c => $"{c.name} seed {c.seed}"))}");
            ctx.Channel.SendToHost(Report, JsonUtility.ToJson(seen));

            yield return ctx.Receive(Done, StepTimeout, m =>
            {
                if (m.Payload != "pass") ctx.Fail("the host's cross-machine check failed (see the host's result)");
            });
        }

        private static string Format(Vector3 v) =>
            string.Join(";", v.x.ToString("R", CultureInfo.InvariantCulture), v.y.ToString("R", CultureInfo.InvariantCulture),
                v.z.ToString("R", CultureInfo.InvariantCulture));

        public static bool TryParse(string text, out Vector3 v)
        {
            v = default;
            string[] parts = (text ?? "").Split(';');
            if (parts.Length != 3) return false;
            bool ok = float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out v.x);
            ok &= float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out v.y);
            ok &= float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out v.z);
            return ok;
        }
    }
}

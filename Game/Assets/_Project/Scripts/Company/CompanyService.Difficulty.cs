using Abandoned.Core;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Company
{
    /// <summary>
    /// The host's difficulty preset (0.19.4): kept in the save, shown to the crew, changed only by the host
    /// at the HQ (the difficulty switch on the dispatch wall).
    /// </summary>
    public partial class CompanyService
    {
        private readonly NetworkVariable<byte> difficulty = new((byte)Difficulty.Standard);

        public Difficulty Difficulty => (Difficulty)difficulty.Value;

        // Host, from Update: the save is the truth; the preset everything host-side reads follows it.
        private void SyncDifficulty()
        {
            if (!IsServer || save == null) return;
            byte want = (byte)Mathf.Clamp(save.difficulty, 0, 2);
            if (difficulty.Value != want) difficulty.Value = want;
            DifficultyConfig.Active = (Difficulty)want;
        }

        public void RequestCycleDifficulty() => CycleDifficultyRpc();

        [Rpc(SendTo.Server)]
        private void CycleDifficultyRpc(RpcParams rpcParams = default)
        {
            // The host's call, and only at the HQ: decay and quota are read from it during a job.
            if (save == null || rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || !AtHome) return;
            save.difficulty = (save.difficulty + 1) % 3;
            Saved();
            SyncDifficulty();
            Debug.Log($"[Company] Difficulty is now {(Difficulty)save.difficulty}.");
        }
    }
}

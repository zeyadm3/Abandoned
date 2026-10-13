using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.UI;
using Unity.Netcode;

namespace Abandoned.Company
{
    /// <summary>The HQ toys' scores (0.19.6), told to the whole crew.</summary>
    public partial class CompanyService
    {
        public void ServerToyScore(string text)
        {
            if (IsServer && IsSpawned) ToyScoreRpc(text);
        }

        [Rpc(SendTo.Everyone)]
        private void ToyScoreRpc(string text)
        {
            ToastFeed.Show(Loc.T("BREAK ROOM"), Loc.FromWire(text), null, ToastFeed.Kind.Good);
            GameAudio.PlayUi(SoundId.ToyScore, 0.8f);
        }
    }
}

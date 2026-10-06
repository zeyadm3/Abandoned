using System;
using Unity.Netcode;

namespace Abandoned.Networking
{
    /// <summary>
    /// Shuts Steam down once NGO has finished shutting down its transport. A plain object rather than
    /// part of SteamBootstrap because the bootstrap may already be destroyed (quit, scene teardown)
    /// by the time NGO stops.
    /// </summary>
    internal sealed class DeferredSteamShutdown
    {
        private readonly NetworkManager network;
        private readonly ISteamClient client;
        private bool done;

        private DeferredSteamShutdown(NetworkManager network, ISteamClient client)
        {
            this.network = network;
            this.client = client;
        }

        public static void After(NetworkManager network, ISteamClient client)
        {
            var pending = new DeferredSteamShutdown(network, client);
            network.OnServerStopped += pending.OnStopped;
            network.OnClientStopped += pending.OnStopped;
            // Fallback: if NGO's shutdown throws part-way, the stop events never fire, but its OnDestroy
            // still raises OnDestroying afterwards, so Steam isn't left initialised.
            NetworkManager.OnDestroying += pending.OnDestroying;
        }

        private void OnStopped(bool _) => Finish();

        private void OnDestroying(NetworkManager destroyed)
        {
            if (destroyed == network) Finish();
        }

        private void Finish()
        {
            if (done) return;
            done = true;
            if (network != null)
            {
                network.OnServerStopped -= OnStopped;
                network.OnClientStopped -= OnStopped;
            }
            NetworkManager.OnDestroying -= OnDestroying;
            SteamBootstrap.ShutdownClient(client);
        }
    }
}

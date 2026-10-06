using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>
    /// Keeps in-process "machines" physically apart: whenever any machine spawns something, its
    /// colliders are told to ignore every other machine's before the next physics step. NGO spawns
    /// copies in EarlyUpdate and physics steps before coroutines run, so a coroutine would leave one
    /// step in which a host copy and a client copy overlap and fling each other (and players).
    /// </summary>
    public class MachineSeparator : MonoBehaviour
    {
        private NetTestHarness net;
        private int lastCount = -1;

        public static MachineSeparator Create(NetTestHarness net)
        {
            var go = new GameObject("MachineSeparator");
            var separator = go.AddComponent<MachineSeparator>();
            separator.net = net;
            net.Track(go);
            return separator;
        }

        private void FixedUpdate()
        {
            int count = 0;
            foreach (var machine in net.Machines)
                if (machine != null && machine.Manager != null && machine.Manager.SpawnManager != null)
                    count += machine.Manager.SpawnManager.SpawnedObjectsList.Count;
            if (count == lastCount) return;
            lastCount = count;
            NetLootKit.KeepMachinesApart(net);
        }
    }
}

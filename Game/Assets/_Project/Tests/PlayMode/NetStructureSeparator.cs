using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>
    /// <see cref="MachineSeparator"/> for structure tests: whenever any machine spawns something, before
    /// the next physics step, its colliders stop touching other machines' copies of the building (and
    /// of each other), and client copies stop counting as load (see <see cref="NetStructureKit.KeepApart"/>).
    /// </summary>
    public class NetStructureSeparator : MonoBehaviour
    {
        private NetTestHarness net;
        private NetStructureKit kit;
        private int lastCount = -1;

        public static NetStructureSeparator Create(NetTestHarness net, NetStructureKit kit)
        {
            var go = new GameObject("NetStructureSeparator");
            var separator = go.AddComponent<NetStructureSeparator>();
            separator.net = net;
            separator.kit = kit;
            net.Track(go);
            separator.Separate();
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
            Separate();
        }

        private void Separate()
        {
            NetLootKit.KeepMachinesApart(net);
            kit.KeepApart();
        }
    }
}

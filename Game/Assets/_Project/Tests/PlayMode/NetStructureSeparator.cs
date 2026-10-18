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
            SoloMounds();
            int count = 0;
            foreach (var machine in net.Machines)
                if (machine != null && machine.Manager != null && machine.Manager.SpawnManager != null)
                    count += machine.Manager.SpawnManager.SpawnedObjectsList.Count;
            if (count == lastCount) return;
            lastCount = count;
            Separate();
        }

        // Every machine builds the same rubble mound for a fallen section (2026-10-08). On real machines each sees one;
        // here they share one physics world and would stack, so only the host's copy stays solid.
        private void SoloMounds()
        {
            foreach (Abandoned.Structure.RubbleMound mound in FindObjectsByType<Abandoned.Structure.RubbleMound>(FindObjectsSortMode.None))
            {
                if (mound.Fallen == null || mound.Fallen == kit.Section(net.Host, mound.Fallen.name)) continue;
                foreach (Collider c in mound.GetComponentsInChildren<Collider>()) c.enabled = false;
            }
        }

        private void Separate()
        {
            NetLootKit.KeepMachinesApart(net);
            kit.KeepApart();
        }
    }
}

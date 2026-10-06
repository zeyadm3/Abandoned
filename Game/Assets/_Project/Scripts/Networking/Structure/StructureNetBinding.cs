using Abandoned.Structure;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// Ties a structure to one NetworkManager. Only needed when one process runs several sessions
    /// (in-process network tests: every "machine" has its own copy of the building). Without any
    /// binding the scene's single StructureSimulation is used.
    /// </summary>
    [RequireComponent(typeof(StructureSimulation))]
    public class StructureNetBinding : MonoBehaviour
    {
        [SerializeField] private NetworkManager manager;

        public NetworkManager Manager => manager;

        public void Bind(NetworkManager networkManager) => manager = networkManager;

        /// <summary>The structure a session on <paramref name="networkManager"/> runs or mirrors.</summary>
        public static StructureSimulation Resolve(NetworkManager networkManager)
        {
            StructureSimulation unbound = null;
            foreach (StructureSimulation s in FindObjectsByType<StructureSimulation>(FindObjectsSortMode.None))
            {
                if (s.TryGetComponent(out StructureNetBinding binding) && binding.manager != null)
                {
                    if (binding.manager == networkManager) return s;
                    continue;
                }
                unbound ??= s;
            }
            return unbound;
        }
    }
}

using Abandoned.Equipment;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Networking
{
    /// <summary>
    /// A moving blanket round the item (0.18.5): the host wraps it, every machine shows the blanket, and its
    /// knocks hurt it much less (<see cref="Loot.LootItem.Cushion"/>) for the rest of the job.
    /// </summary>
    public partial class NetworkLoot
    {
        private readonly NetworkVariable<bool> wrapped = new(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private GameObject blanket;

        public bool Wrapped => wrapped.Value;

        /// <summary>Host: wrap it. False when it already is (the blanket isn't spent).</summary>
        public bool ServerWrap()
        {
            if (!IsServer || wrapped.Value || item.IsShattered) return false;
            wrapped.Value = true;
            ShowWrapped(true);
            return true;
        }

        private void SpawnBlanket()
        {
            wrapped.OnValueChanged += OnWrappedChanged;
            if (wrapped.Value) ShowWrapped(true);
        }

        private void DespawnBlanket() => wrapped.OnValueChanged -= OnWrappedChanged;

        private void OnWrappedChanged(bool was, bool now) => ShowWrapped(now);

        private void ShowWrapped(bool on)
        {
            item.Cushion = on ? ToolConfig.Current.BlanketCushion : 1f;
            if (!on || blanket != null || item.Definition == null) return;
            // A padded quilt draped over the whole item, a little larger than its box.
            blanket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blanket.name = "MovingBlanket";
            // Destroy only takes effect at the end of the frame: switched off first, or the primitive's box
            // (on the Default layer, bigger than the item) is part of the item's body for one physics step.
            Collider box = blanket.GetComponent<Collider>();
            if (box != null)
            {
                box.enabled = false;
                Destroy(box);
            }
            blanket.transform.SetParent(transform, false);
            blanket.transform.localScale = item.Definition.Size * 1.06f + Vector3.one * 0.02f;
            Material material = ToolConfig.Current.BlanketMaterial;
            if (material != null) blanket.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}

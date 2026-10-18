using Abandoned.Core;
using Abandoned.Networking;
using UnityEngine;

namespace Abandoned.Equipment
{
    /// <summary>Host-simulated gear on the ground; its originating player's replicated list owns its lifetime.</summary>
    public class DroppedTool : MonoBehaviour, IUsable
    {
        public PlayerEquipment Source { get; private set; }
        public int Id { get; private set; }
        public Rigidbody Body { get; private set; }
        private EquipmentDefinition definition;

        public void Setup(PlayerEquipment source, DroppedToolState state, bool host)
        {
            Source = source;
            Id = state.Id;
            definition = source.Catalog.At(state.CatalogIndex);
            transform.SetPositionAndRotation(state.Position, state.Rotation);
            ToolModels.Build(definition, transform);
            gameObject.layer = Mathf.Max(0, LayerMask.NameToLayer(GameLayers.Loot));
            Bounds shape = new(Vector3.zero, Vector3.one * .05f);
            foreach (MeshFilter part in GetComponentsInChildren<MeshFilter>())
            {
                if (part.sharedMesh == null) continue;
                Bounds mesh = part.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = mesh.center + Vector3.Scale(mesh.extents, new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    shape.Encapsulate(transform.InverseTransformPoint(part.transform.TransformPoint(point)));
                }
            }
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = shape.center;
            collider.size = shape.size;
            Body = gameObject.AddComponent<Rigidbody>();
            Body.mass = 1f;
            Body.isKinematic = !host;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
        }
        public string UsePrompt(GameObject user)
        {
            PlayerEquipment gear = user != null ? user.GetComponentInParent<PlayerEquipment>() : null;
            if (gear == null || definition == null) return null;
            return gear.FreePocket < 0 ? Loc.T("Pockets full") : Loc.T("Pick up {0}", definition.LocalName);
        }
        public void Use(GameObject user) => Source?.RequestRecoverTool(Id);
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !DebugView.Visible || Body == null) return;
            Gizmos.color = Body.isKinematic ? Color.cyan : Color.yellow;
            if (TryGetComponent(out Collider solid)) Gizmos.DrawWireCube(solid.bounds.center, solid.bounds.size);
        }
    }
}

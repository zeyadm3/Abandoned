using UnityEngine;

namespace Abandoned.Core
{
    /// <summary>Marks what a collider is made of, for footsteps and impact sounds. Untagged = Concrete.</summary>
    public class SurfaceTag : MonoBehaviour
    {
        [SerializeField] private SurfaceMaterial material = SurfaceMaterial.Concrete;

        public SurfaceMaterial Material => material;

        public static SurfaceMaterial Of(Collider collider) =>
            collider != null && collider.GetComponentInParent<SurfaceTag>() is { } tag ? tag.material : SurfaceMaterial.Concrete;

#if UNITY_EDITOR
        public void EditorSet(SurfaceMaterial value) => material = value;
#endif
    }
}

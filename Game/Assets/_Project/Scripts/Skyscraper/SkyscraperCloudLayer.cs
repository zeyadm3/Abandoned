using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Skyscraper
{
    /// <summary>Local presentation only. A moving cloud bank clears the tower interior and follows its world placement.</summary>
    [ExecuteAlways, RequireComponent(typeof(Renderer))]
    public sealed class SkyscraperCloudLayer : MonoBehaviour
    {
        [SerializeField] private Transform tower;
        private Renderer cloudRenderer;
        private MaterialPropertyBlock properties;
        private void OnEnable() => UpdateBounds();
        private void LateUpdate() => UpdateBounds();
        private void UpdateBounds()
        {
            if(cloudRenderer==null)cloudRenderer=GetComponent<Renderer>();
            properties ??= new MaterialPropertyBlock();
            cloudRenderer.GetPropertyBlock(properties);
            properties.SetVector("_CloudCenter",transform.position);
            properties.SetVector("_CloudHalfSize",transform.lossyScale*.5f);
            properties.SetVector("_BuildingCenter",tower!=null?tower.TransformPoint(new Vector3(0,205,6)):new Vector3(0,205,6));
            cloudRenderer.SetPropertyBlock(properties);
        }
#if UNITY_EDITOR
        public void EditorSetup(Transform root) {tower=root;UpdateBounds();}
        private void OnDrawGizmosSelected()
        {
            Gizmos.color=new Color(.7f,.85f,1,.4f);
            Gizmos.DrawWireCube(transform.position,transform.lossyScale);
        }
#endif
    }
}

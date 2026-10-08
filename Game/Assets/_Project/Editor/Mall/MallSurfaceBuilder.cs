using Abandoned.Core;
using UnityEngine;

namespace Abandoned.EditorTools
{
    public static class MallSurfaceBuilder
    {
        public static void Assign(Transform mall)
        {
            foreach(Collider collider in mall.GetComponentsInChildren<Collider>(true))
            {
                if(collider.isTrigger||collider.GetComponentInParent<SurfaceTag>()!=null)continue;
                string name=collider.name;
                SurfaceMaterial surface=name.Contains("Rail")||name.Contains("Rope")?SurfaceMaterial.Metal:SurfaceMaterial.Concrete;
                collider.gameObject.AddComponent<SurfaceTag>().EditorSet(surface);
            }
        }
    }
}

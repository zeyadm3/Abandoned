using UnityEngine;
using Abandoned.Structure;

namespace Abandoned.EditorTools
{
    public static class BridgeProbe
    {
        public static void RunAndExit() => ReleaseKit.RunAndExit(() =>
        {
            Transform mall = MallSites.OpenMall(out _);
            foreach (StructuralSection s in mall.GetComponentsInChildren<StructuralSection>(true))
            {
                Collider c = s.GetComponentInChildren<Collider>(true);
                Vector3 at = c != null ? c.bounds.center : s.transform.position;
                if (at.y > 2f && at.y < 6f && at.z > 20f && at.z < 32f && at.x > 16f && at.x < 40f)
                    Debug.Log($"[Probe] {s.name} type={s.Type} collapse={s.CanCollapse} at={at} kind={s.FloorKind}");
            }
        });
    }
}

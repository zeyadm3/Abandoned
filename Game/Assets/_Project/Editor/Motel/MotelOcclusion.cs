using Abandoned.EditorTools;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelOcclusion
    {
        public static void Prepare(Transform root)
        {
            foreach (string name in new []
            {
                "Shell", "Roof", "AnnexShell"
            })
            {
                Transform permanent = root.Find(name);
                if (permanent != null)
                {
                    GreyboxFactory.MarkStatic(permanent);
                }
            }
            foreach (Transform part in root.GetComponentsInChildren<Transform>(true))
            {
                if (part.name == "OfficeBackRoomWall" || part.name == "BackRoomSide" || part.name == "BedroomPartition")
                {
                    GreyboxFactory.MarkStatic(part);
                }
            }
            foreach (TextMesh label in root.GetComponentsInChildren<TextMesh>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(label.gameObject, 0);
            }
            // Shell ceiling panels follow failing upper slabs. They must not keep hiding the room after falling.
            foreach (SectionProp prop in root.GetComponentsInChildren<SectionProp>(true))
            {
                Clear(prop.transform);
            }
            // Permanent floor sections also stay out: the project never uses floor slabs as baked occluders.
            foreach (StructuralSection section in root.GetComponentsInChildren<StructuralSection>(true))
            {
                Clear(section.transform);
            }
        }
        private static void Clear(Transform root)
        {
            foreach (Transform part in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(part.gameObject, 0);
            }
        }
    }
}

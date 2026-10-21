using UnityEngine;

namespace Abandoned.Player
{
    /// <summary>The authored worker's visual joints and suit renderers, also reused by the wardrobe mannequin.</summary>
    public class WorkerRig : MonoBehaviour
    {
        [SerializeField] private Transform leftArm, rightArm, leftLeg, rightLeg, leftKnee, rightKnee, torso, headAnchor;
        [SerializeField] private Renderer[] coverall;
        // 2026-10-22 crew redesign: forearms bend at the elbow (older saved workers have none).
        [Abandoned.Core.OptionalReference, SerializeField] private Transform leftElbow, rightElbow;
        private MaterialPropertyBlock block;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public Transform LeftArm => leftArm;
        public Transform RightArm => rightArm;
        public Transform LeftLeg => leftLeg;
        public Transform RightLeg => rightLeg;
        public Transform LeftKnee => leftKnee;
        public Transform RightKnee => rightKnee;
        public Transform Torso => torso;
        public Transform HeadAnchor => headAnchor;
        public Transform LeftElbow => leftElbow;
        public Transform RightElbow => rightElbow;
        public Renderer[] Coverall => coverall;

        public void Tint(Color color)
        {
            block ??= new MaterialPropertyBlock();
            if (coverall == null) return;
            foreach (Renderer renderer in coverall)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(block);
                block.SetColor(BaseColor, color);
                renderer.SetPropertyBlock(block);
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(Transform armL, Transform armR, Transform legL, Transform legR, Transform kneeL, Transform kneeR, Transform chest, Transform crown, Renderer[] suit,
            Transform elbowL = null, Transform elbowR = null)
        {
            leftArm = armL; rightArm = armR; leftLeg = legL; rightLeg = legR; leftKnee = kneeL; rightKnee = kneeR;
            torso = chest; headAnchor = crown; coverall = suit; leftElbow = elbowL; rightElbow = elbowR;
        }
#endif
    }
}

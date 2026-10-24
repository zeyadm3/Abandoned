using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Skyscraper
{
    [DefaultExecutionOrder(-55)]
    public class SkyscraperElevatorDoor : MonoBehaviour
    {
        [SerializeField] private Transform left;
        [SerializeField] private Transform right;
        [SerializeField,Min(.1f)] private float seconds=.7f;
        [SerializeField,Min(.5f)] private float slide=1.12f;
        private bool opening;
        private float fraction;
        public bool IsClosed => fraction<=.001f;
        public bool IsOpen => fraction>=.999f;
        public void SetOpen(bool open)=>opening=open;
        private void Update()
        {
            fraction=Mathf.MoveTowards(fraction,opening?1:0,Time.deltaTime/seconds);
            Place();
        }
        private void Place()
        {
            if(left!=null)left.localPosition=new Vector3(-.55f-slide*fraction,1.3f,0);
            if(right!=null)right.localPosition=new Vector3(.55f+slide*fraction,1.3f,0);
        }
#if UNITY_EDITOR
        public void EditorSetup(Transform l,Transform r,bool open)
        {
            left=l;right=r;opening=open;fraction=open?1:0;Place();
        }
#endif
    }
}

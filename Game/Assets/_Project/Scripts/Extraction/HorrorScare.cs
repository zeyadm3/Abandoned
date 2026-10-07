using UnityEngine;

namespace Abandoned.Extraction
{
    public class HorrorScare : MonoBehaviour
    {
        private Vector3 start;
        private float born;
        private int kind;
        public void Setup(int eventKind){kind=eventKind;start=transform.position;born=Time.time;}
        private void Update()
        {
            float age=Time.time-born;
            if(kind==0)transform.position=start+transform.right*age*4;
            else if(kind==1)transform.position=start-Vector3.up*Mathf.Min(age*3,2.8f);
            else transform.localRotation=Quaternion.Euler(0,Mathf.Sin(age*0.6f)*3,Mathf.Sin(age*17)*2);
            if(age>(kind==0?1.4f:kind==1?2.5f:5f))Destroy(gameObject);
        }
    }
}

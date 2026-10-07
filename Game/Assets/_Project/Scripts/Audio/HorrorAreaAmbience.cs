using Abandoned.Extraction;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    public class HorrorAreaAmbience : MonoBehaviour
    {
        [SerializeField] private Vector3 size=new(12,4,12);
        [SerializeField] private int character;
        private AudioSource source;
        private float nextSound;
        private void Start()
        {
            source=gameObject.AddComponent<AudioSource>();
            source.clip=HorrorAudio.Clip(character==2?HorrorAudio.Cue.Water:HorrorAudio.Cue.Vent);
            source.playOnAwake=false; source.loop=true; source.volume=0; source.spatialBlend=0; source.Play();
            nextSound=Time.time+17+character*4;
        }
        private void Update()
        {
            Camera ear=Camera.main;
            if(ear==null)return;
            var bounds=new Bounds(transform.position,size);
            float distance=Vector3.Distance(ear.transform.position,bounds.ClosestPoint(ear.transform.position));
            float nearby=Mathf.Clamp01(1-distance/5);
            source.volume=0.16f*nearby*AudioLevels.Ambience*AudioLevels.Sfx*AudioLevels.BackgroundDuck;
            if(nearby<=0 || Time.time<nextSound)return;
            int danger=RunState.Current!=null?RunState.Current.State.Danger:0;
            nextSound=Time.time+Mathf.Max(8,28-danger*3);
            if(danger>0) HorrorAudio.Play(character==2?HorrorAudio.Cue.Water:HorrorAudio.Cue.Ceiling,transform.position+Vector3.up*2,0.2f+danger*0.025f);
        }
        private void OnDrawGizmos(){if(!DebugView.Visible)return;Gizmos.color=Color.cyan;Gizmos.DrawWireCube(transform.position,size);}
#if UNITY_EDITOR
        public void EditorSetup(Vector3 area,int soundCharacter){size=area;character=soundCharacter;}
#endif
    }
}

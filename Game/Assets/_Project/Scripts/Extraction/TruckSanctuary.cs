using Abandoned.Audio;
using UnityEngine;

namespace Abandoned.Extraction
{
    public class TruckSanctuary : MonoBehaviour
    {
        private AudioSource idle;
        private TruckCargo truck;
        private Camera mirrorCamera;
        private RenderTexture reflection;
        private Transform doors;
        private Vector3 cameraHome;
        private bool leaving;
        private void Awake()
        {
            truck=GetComponent<TruckCargo>();
            idle=gameObject.AddComponent<AudioSource>();idle.clip=HorrorAudio.Clip(HorrorAudio.Cue.EngineIdle);
            idle.loop=true;idle.playOnAwake=false;idle.spatialBlend=1;idle.minDistance=2;idle.maxDistance=22;idle.volume=0;
            AddLight(new Vector3(0,2.5f,0),new Color(1,0.65f,0.29f),1.7f,8);
            foreach(float side in new[]{-0.9f,0.9f})
            {
                Light head=AddLight(new Vector3(side,1.25f,3.7f),new Color(1,0.78f,0.43f),4,17);
                head.type=LightType.Spot;head.spotAngle=55;
            }
            doors=new GameObject("DepartureDoors").transform;doors.SetParent(transform,false);
            doors.localPosition=new Vector3(0,1.55f,-2.56f);
            var panel=GameObject.CreatePrimitive(PrimitiveType.Cube);panel.transform.SetParent(doors,false);panel.transform.localScale=new Vector3(2.6f,2.2f,0.07f);
            Destroy(panel.GetComponent<Collider>());
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=new Color(0.18f,0.2f,0.18f);panel.GetComponent<Renderer>().material=mat;
            doors.gameObject.SetActive(false);
            reflection=new RenderTexture(256,128,16);reflection.Create();
            var cameraObject=new GameObject("RearMirrorCamera");cameraObject.transform.SetParent(transform,false);
            cameraHome=new Vector3(0,2.8f,-3.0f);cameraObject.transform.localPosition=cameraHome;cameraObject.transform.localRotation=Quaternion.Euler(0,180,0);
            mirrorCamera=cameraObject.AddComponent<Camera>();mirrorCamera.targetTexture=reflection;mirrorCamera.fieldOfView=70;mirrorCamera.farClipPlane=80;mirrorCamera.nearClipPlane=0.2f;mirrorCamera.depth=-5;
            mirrorCamera.enabled=false;
            var mirror=GameObject.CreatePrimitive(PrimitiveType.Quad);mirror.name="RearViewMirror";mirror.transform.SetParent(transform,false);
            mirror.transform.localPosition=new Vector3(0,2.6f,2.38f);mirror.transform.localScale=new Vector3(1.1f,0.55f,1);
            Destroy(mirror.GetComponent<Collider>());
            var mirrorMat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mirrorMat.mainTexture=reflection;mirror.GetComponent<Renderer>().material=mirrorMat;
        }
        private Light AddLight(Vector3 at,Color color,float intensity,float range)
        {
            var go=new GameObject("TruckWarmLamp");go.transform.SetParent(transform,false);go.transform.localPosition=at;
            var light=go.AddComponent<Light>();light.type=LightType.Point;light.color=color;light.intensity=intensity;light.range=range;light.shadows=LightShadows.None;return light;
        }
        private void Update()
        {
            RunState run=RunState.Current;Camera eye=Camera.main;
            bool near=eye!=null&&Vector3.Distance(eye.transform.position,transform.position)<14;
            if(run!=null&&run.IsSpawned&&run.Elapsed<7)near=false;
            if(near&&!idle.isPlaying)idle.Play();
            idle.volume=near?0.15f*AudioLevels.Sfx:0;
            mirrorCamera.enabled=near&&truck!=null&&eye!=null&&truck.Carries(eye.transform.position-Vector3.up);
            if(run==null){leaving=false;doors.gameObject.SetActive(false);mirrorCamera.transform.localPosition=cameraHome;return;}
            if(run.State.Phase==RunPhase.Honking && run.HonkRemaining<2)
            {
                if(!leaving){leaving=true;HorrorAudio.Play(HorrorAudio.Cue.Door,transform.position,0.9f);}
                doors.gameObject.SetActive(true);
                float progress=1-run.HonkRemaining/2;
                doors.localScale=new Vector3(Mathf.Max(0.02f,progress),1,1);
                mirrorCamera.transform.localPosition=cameraHome+Vector3.forward*progress*8;
                idle.pitch=1+progress;
            }
        }
        private void OnDestroy(){if(reflection!=null){reflection.Release();Destroy(reflection);}}
    }
}

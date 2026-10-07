using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.Threats;
using Unity.Netcode;
using UnityEngine;

namespace Abandoned.Extraction
{
    public class RunHorrorDirector : NetworkBehaviour
    {
        [SerializeField] private HorrorConfig config;
        private readonly NetworkVariable<double> enteredAt=new(-1);
        private readonly NetworkVariable<bool> final=new();
        private RunState run;
        private int arrivalBeat,shownDanger=-1;
        private float nextPressure,nextScare,nextSiren;
        private bool announced,arrivalPlayed,departurePlayed;
        private System.Random random;
        private LightFixture[] fixtures;
        private Light[] alarms;
        public static RunHorrorDirector Current {get;private set;}
        public static string RadioLine {get;private set;}
        public static float RadioUntil {get;private set;}
        public bool FinalPhase=>final.Value;
        public bool Inside(Vector3 at)=>config!=null&&config.Building.Contains(at)&&(TruckCargo.Current==null||!TruckCargo.Current.Carries(at));
        public override void OnNetworkSpawn()
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Mall"){enabled=false;return;}
            Current=this;run=GetComponent<RunState>();random=new System.Random(run.State.Seed^713);
            fixtures=FindObjectsByType<LightFixture>(FindObjectsSortMode.None);
            alarms=GameObject.FindGameObjectsWithTag("Untagged").Where(g=>g.name.StartsWith("HorrorAlarm")).Select(g=>g.GetComponent<Light>()).Where(l=>l!=null).ToArray();
            nextPressure=Time.time+25;nextScare=Time.time+100;
        }
        public override void OnNetworkDespawn()
        {
            if(Current==this)Current=null;
            if(fixtures!=null)foreach(LightFixture f in fixtures)if(f!=null){f.SetHorrorPower(true);f.SetAlarm(false);}
            if(alarms!=null)foreach(Light l in alarms)if(l!=null)l.enabled=false;
            RadioLine=null;
        }
        private void Update()
        {
            if(!IsSpawned||run==null||config==null)return;
            if(!arrivalPlayed && run.State.Phase==RunPhase.Running)
            {
                arrivalPlayed=true;
                HorrorAudio.Play(HorrorAudio.Cue.EngineStop,TruckCargo.Current!=null?TruckCargo.Current.transform.position:Vector3.zero,0.7f);
                Say("DISPATCH: Don't stay past dark. Last crew didn't.",8);
            }
            if(run.State.Phase!=RunPhase.Running&&run.State.Phase!=RunPhase.Honking)return;
            if(IsServer)HostPressure();
            Lights();
            if(final.Value&&!announced)
            {
                announced=true;Say("LOCKDOWN / IT IS HERE / GET TO THE TRUCK",12);
                HorrorAudio.Play(HorrorAudio.Cue.Roar,new Vector3(28,8,24),1,false);
                nextSiren=0;LightFixture.Disturb(8);
            }
            if(final.Value&&Time.time>=nextSiren){nextSiren=Time.time+9;HorrorAudio.Play(HorrorAudio.Cue.Siren,Vector3.zero,0.7f,false);}
            if(run.State.Phase==RunPhase.Honking&&!departurePlayed)
            {
                departurePlayed=true;HorrorAudio.Play(HorrorAudio.Cue.Door,TruckCargo.Current.transform.position,1);
                HorrorAudio.Play(HorrorAudio.Cue.EngineEscape,TruckCargo.Current.transform.position,0.9f);
                Say("DOORS SHUT. HOLD ON.",4);
            }
        }
        private void HostPressure()
        {
            NetworkPlayer[] inside=NetworkPlayer.All.Where(p=>p!=null&&!p.IsDead&&Inside(p.transform.position)).ToArray();
            if(enteredAt.Value<0 && inside.Length>0) enteredAt.Value=run.Now;
            if(enteredAt.Value>=0)
            {
                double age=run.Now-enteredAt.Value;
                float[] times={2,9,17,30,46};
                if(arrivalBeat<times.Length&&age>=times[arrivalBeat])
                {
                    Vector3 at=inside.Length>0?inside[0].transform.position:new Vector3(28,0,10);
                    EventRpc(arrivalBeat,at,(int)(run.State.Seed+arrivalBeat));arrivalBeat++;
                }
            }
            if(!final.Value&&run.Now>=run.State.WindowEnd+config.FinalDelay)
            {
                final.Value=true;
                ThreatDirector.Current?.SpawnFinalHunter();
            }
            if(inside.Length==0)return;
            int danger=run.State.Danger;
            if(Time.time>=nextPressure)
            {
                nextPressure=Time.time+Mathf.Max(6,config.PressureInterval-danger*2.5f);
                var p=inside[random.Next(inside.Length)];
                float distance=Mathf.Lerp(18,3,danger/6f);
                Vector3 at=p.transform.position+new Vector3((float)random.NextDouble()*2-1,0,(float)random.NextDouble()*2-1).normalized*distance;
                EventRpc(5+random.Next(3),at+Vector3.up*(2+random.Next(2)),random.Next());
            }
            if(Time.time>=nextScare)
            {
                nextScare=Time.time+config.ScareInterval+random.Next(45);
                var p=inside[random.Next(inside.Length)];
                Vector3 at=p.transform.position+p.transform.forward*9+Vector3.up;
                at.x=Mathf.Clamp(at.x,2,54);at.z=Mathf.Clamp(at.z,2,46);
                EventRpc(8+random.Next(3),at,random.Next());
                if(danger>=2&&RunShutters.Current!=null)
                {
                    RollerShutter[] stores=RollerShutter.All.Where(s=>s!=null&&!s.Entrance&&!s.IsDown).ToArray();
                    if(stores.Length>0)RunShutters.Current.ServerSlam(stores[random.Next(stores.Length)].Index);
                }
            }
        }
        private void Lights()
        {
            int danger=run.State.Danger;
            double arrival=enteredAt.Value>=0?run.Now-enteredAt.Value:-1;
            foreach(LightFixture f in fixtures)
            {
                if(f==null)continue;
                Vector3 at=f.transform.position;
                int sector=Mathf.Abs(Mathf.FloorToInt(at.x/12)+3*Mathf.FloorToInt(at.z/12)+Mathf.FloorToInt(at.y/4))%6;
                bool power=danger<=sector;
                if(arrival>=5&&arrival<16)power &= Mathf.Abs((float)arrival-5-at.z/5)<1.2f;
                f.SetHorrorPower(power);f.SetAlarm(final.Value);
            }
            foreach(Light l in alarms)if(l!=null)l.enabled=final.Value;
            if(shownDanger!=danger){shownDanger=danger;if(danger>0)Say(danger>=5?"SIGNAL FAILING / LEAVE":"BUILDING STATUS "+new string('|',danger)+" / deteriorating",4);}
        }
        [Rpc(SendTo.Everyone)]
        private void EventRpc(int kind,Vector3 at,int seed)
        {
            switch(kind)
            {
                case 0:GameAudio.Play(SoundId.Groan,at+Vector3.up*6,1);LightFixture.Disturb(3);break;
                case 1:GameAudio.Play(SoundId.RadioStatic,at,0.35f);break;
                case 2:HorrorAudio.Play(HorrorAudio.Cue.Scream,at+Vector3.forward*22+Vector3.up*5,0.7f);break;
                case 3:Scare(0,new Vector3(26,4,29),seed);HorrorAudio.Play(HorrorAudio.Cue.Door,new Vector3(40,4,30),0.8f);break;
                case 4:HorrorAudio.Play(HorrorAudio.Cue.Ceiling,at+Vector3.up*4,0.7f);break;
                case 5:HorrorAudio.Play(HorrorAudio.Cue.Whisper,at,0.45f);break;
                case 6:HorrorAudio.Play(HorrorAudio.Cue.Knock,at,0.6f);break;
                case 7:HorrorAudio.Play(HorrorAudio.Cue.Ceiling,at,0.65f);break;
                default:Scare(kind-8,at+(kind==9?Vector3.up*2:Vector3.zero),seed);HorrorAudio.Play(HorrorAudio.Cue.Door,at,0.6f);break;
            }
        }
        private void Scare(int kind,Vector3 at,int seed)
        {
            if(config.ScareModel==null)return;
            GameObject shape=Instantiate(config.ScareModel,at,Quaternion.Euler(0,seed%360,kind==1?180:0));
            foreach(Collider c in shape.GetComponentsInChildren<Collider>())Destroy(c);
            shape.AddComponent<HorrorScare>().Setup(kind);
        }
        private static void Say(string text,float seconds){RadioLine=text;RadioUntil=Time.time+seconds;}
        private void OnGUI(){if(DebugView.Visible)GUI.Label(new Rect(15,190,650,25),$"HORROR arrival {arrivalBeat}/5 final {final.Value} next scare {nextScare-Time.time:0}s");}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){Current=null;RadioLine=null;RadioUntil=0;}
    }
}

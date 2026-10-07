using System.Collections.Generic;
using UnityEngine;

namespace Abandoned.Audio
{
    // Original procedural sound design; cached short clips keep run events allocation-free.
    public static class HorrorAudio
    {
        public enum Cue { EngineStop, EngineIdle, EngineEscape, Scream, Knock, Whisper, Ceiling, Siren, Roar, Door, Water, Vent, Breathing }
        private static readonly Dictionary<Cue,AudioClip> clips=new();
        public static AudioClip Clip(Cue cue)
        {
            if(clips.TryGetValue(cue,out AudioClip clip)) return clip;
            const int rate=22050;
            float duration=cue==Cue.EngineIdle?4:cue==Cue.Siren?5:cue==Cue.Roar?4:cue==Cue.Whisper?3:2;
            var data=new float[(int)(duration*rate)];
            var random=new System.Random(731+(int)cue*911);
            float smooth=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,u=t/duration;
                float noise=(float)(random.NextDouble()*2-1);
                smooth=Mathf.Lerp(smooth,noise,0.075f);
                float sine=Mathf.Sin(2*Mathf.PI*(45+9*Mathf.Sin(t*3))*t);
                float env=Mathf.Sin(Mathf.PI*u);
                data[i]=cue switch
                {
                    Cue.EngineIdle=>0.32f*sine+0.12f*smooth,
                    Cue.EngineStop=>(1-u)*(0.5f*Mathf.Sin(2*Mathf.PI*(85-60*u)*t)+0.2f*smooth),
                    Cue.EngineEscape=>env*(0.55f*Mathf.Sin(2*Mathf.PI*(60+100*u)*t)+0.3f*smooth),
                    Cue.Siren=>env*0.5f*Mathf.Sin(2*Mathf.PI*(420+190*Mathf.Sin(t*3))*t),
                    Cue.Roar=>env*(0.65f*smooth+0.4f*Mathf.Sin(2*Mathf.PI*(37-15*u)*t)),
                    Cue.Scream=>env*(0.45f*Mathf.Sin(2*Mathf.PI*(690-180*u+30*Mathf.Sin(t*21))*t)+0.14f*noise),
                    Cue.Knock or Cue.Door=>Mathf.Exp(-20*(t%0.48f))*(0.6f*smooth+0.5f*sine)*(t<1.5f?1:0),
                    Cue.Whisper=>env*(0.26f*noise)*(0.5f+0.5f*Mathf.Sin(t*19)),
                    Cue.Ceiling=>Mathf.Exp(-16*(t%0.37f))*(0.38f*smooth+0.18f*sine),
                    Cue.Water=>0.2f*smooth+0.06f*noise*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*5)),18),
                    Cue.Vent=>0.2f*smooth+0.05f*Mathf.Sin(t*2*Mathf.PI*93),
                    _=>env*0.10f*noise*(0.5f+0.5f*Mathf.Sin(t*4)),
                };
            }
            clip=AudioClip.Create("ABANDONED_"+cue,data.Length,1,rate,false);
            clip.SetData(data,0); clips[cue]=clip; return clip;
        }
        public static void Play(Cue cue,Vector3 at,float volume=1,bool spatial=true,float range=50) =>
            AudioPool.Play(Clip(cue),at,volume*AudioLevels.Sfx,1,2,range,spatial,48);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()=>clips.Clear();
    }
}

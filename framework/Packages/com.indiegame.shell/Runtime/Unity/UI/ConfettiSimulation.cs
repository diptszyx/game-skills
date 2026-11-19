using System;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public readonly struct ConfettiConfig
    {
        public readonly int Count;
        public readonly float Lifetime, FadeSeconds, Gravity, Drag, SpeedMin, SpeedMax, SpreadDegrees, SpinMax, SizeMin, SizeMax;
        public readonly Color[] Palette;
        public ConfettiConfig(int count,float lifetime,float fadeSeconds,float gravity,float drag,float speedMin,float speedMax,float spreadDegrees,float spinMax,float sizeMin,float sizeMax,Color[] palette)
        {
            if(count<1||lifetime<=0||fadeSeconds<0||fadeSeconds>lifetime||speedMin<0||speedMax<speedMin||sizeMin<=0||sizeMax<sizeMin||palette==null||palette.Length==0)
                throw new ArgumentException("Invalid confetti configuration");
            Count=count; Lifetime=lifetime; FadeSeconds=fadeSeconds; Gravity=gravity; Drag=drag; SpeedMin=speedMin; SpeedMax=speedMax;
            SpreadDegrees=spreadDegrees; SpinMax=spinMax; SizeMin=sizeMin; SizeMax=sizeMax; Palette=palette;
        }
        public static readonly ConfettiConfig Default=new ConfettiConfig(60,2.2f,.6f,1400,1.2f,700,1500,55,540,14,26,
            new[]{ new Color(.96f,.30f,.35f), new Color(.98f,.78f,.20f), new Color(.30f,.75f,.45f), new Color(.30f,.60f,.95f), new Color(.75f,.45f,.90f) });
    }
    // Pure particle state: no UnityEngine objects, no allocation after construction.
    public sealed class ConfettiSimulation
    {
        readonly ConfettiConfig config;
        readonly Vector2[] position, velocity; readonly float[] rotation, spin, size; readonly int[] colorIndex;
        readonly System.Random random;
        float age; bool running;
        public ConfettiSimulation(ConfettiConfig config,int seed)
        {
            this.config=config; random=new System.Random(seed);
            position=new Vector2[config.Count]; velocity=new Vector2[config.Count]; rotation=new float[config.Count];
            spin=new float[config.Count]; size=new float[config.Count]; colorIndex=new int[config.Count];
        }
        public int Count=>config.Count;
        public bool Running=>running;
        public float Age=>age;
        public Vector2 Position(int i)=>position[i];
        public float Rotation(int i)=>rotation[i];
        public float Size(int i)=>size[i];
        public Color Color(int i)=>config.Palette[colorIndex[i]];
        public float Alpha(int i)=>!running||age>=config.Lifetime?0:config.FadeSeconds<=0?1:Mathf.Clamp01((config.Lifetime-age)/config.FadeSeconds);
        float Next(float min,float max)=>min+(float)random.NextDouble()*(max-min);
        public void Start(Vector2 origin)
        {
            age=0; running=true;
            for(int i=0;i<config.Count;i++)
            {
                float angle=(90+Next(-config.SpreadDegrees,config.SpreadDegrees))*Mathf.Deg2Rad, speed=Next(config.SpeedMin,config.SpeedMax);
                position[i]=origin; velocity[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed;
                rotation[i]=Next(0,360); spin[i]=Next(-config.SpinMax,config.SpinMax); size[i]=Next(config.SizeMin,config.SizeMax);
                colorIndex[i]=random.Next(config.Palette.Length);
            }
        }
        public void Stop()=>running=false;
        public void Step(float dt)
        {
            if(!running||dt<=0) return;
            age+=dt;
            if(age>=config.Lifetime) { running=false; return; }
            float keep=Mathf.Max(0,1-config.Drag*dt);
            for(int i=0;i<config.Count;i++)
            {
                var v=velocity[i]; v.y-=config.Gravity*dt; v*=keep; velocity[i]=v;
                position[i]+=v*dt; rotation[i]+=spin[i]*dt;
            }
        }
    }
}

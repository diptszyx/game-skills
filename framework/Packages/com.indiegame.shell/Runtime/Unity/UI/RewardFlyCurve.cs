using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public static class RewardFlyCurve
    {
        // LaunchDelay lets a freshly opened popup finish its layout before icons read the source position.
        public const float LandingScale=.6f, FadeStart=.85f, LaunchDelay=.1f;
        public static float Duration(int count,float stagger,float seconds)=>LaunchDelay+Mathf.Max(0,count-1)*stagger+seconds;
        public static float Progress(float elapsed,float delay,float seconds)
        { return seconds<=0?1:Mathf.Clamp01((elapsed-delay)/seconds); }
        public static float Ease(float t)=>t*t*(3-2*t);
        // Straight eased path with an upward bow; the bow scales with the distance so it reads the same at any canvas scale.
        public static Vector2 Position(Vector2 from,Vector2 to,float arc,float t)
        {
            var bow=4*t*(1-t)*arc*(to-from).magnitude;
            return Vector2.Lerp(from,to,Ease(t))+Vector2.up*bow;
        }
        public static float Scale(float t)=>Mathf.Lerp(1,LandingScale,Ease(t));
        public static float Alpha(float t)=>t<=0?0:t<FadeStart?1:Mathf.Clamp01((1-t)/(1-FadeStart));
    }
}

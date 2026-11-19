using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Reward icons that fly from where a reward was claimed to its counter in the top bar.
    public sealed class RewardFlyLayer : MonoBehaviour
    {
        sealed class Flight
        {
            public readonly Image Icon; public readonly RectTransform Source, Target; public readonly float Delay;
            public Vector3 Origin; public float Elapsed;
            public Flight(Image icon,RectTransform source,RectTransform target,float delay)
            { Icon=icon; Source=source; Target=target; Delay=delay; Origin=source.position; }
        }
        readonly Dictionary<string,RectTransform> targets=new Dictionary<string,RectTransform>();
        readonly List<Flight> flights=new List<Flight>();
        ShellUiFactory factory; RectTransform root;
        public int ActiveFlights=>flights.Count;
        public static RewardFlyLayer Create(ShellUiFactory factory,Transform parent)
        {
            var node=ShellUiFactory.Node("Reward fly",parent); ShellUiFactory.Stretch(node);
            var layer=node.gameObject.AddComponent<RewardFlyLayer>(); layer.factory=factory; layer.root=node;
            node.gameObject.SetActive(true); return layer;
        }
        public void SetTarget(string icon,RectTransform target) { targets[icon]=target; }
        public void Fly(RectTransform source,IReadOnlyList<RewardPart> parts)
        {
            if(source==null) return;
            var style=factory.Style;
            foreach(var part in parts)
            {
                if(part.Icon==null || !targets.TryGetValue(part.Icon,out var target) || target==null || !target.gameObject.activeInHierarchy) continue;
                for(int i=0;i<style.RewardFlyCount;i++)
                {
                    var icon=factory.AppearanceImage(root,"Fly "+part.Icon,part.Icon);
                    icon.rectTransform.sizeDelta=Vector2.one*style.RewardFlyIconSize; icon.canvasRenderer.SetAlpha(0);
                    flights.Add(new Flight(icon,source,target,RewardFlyCurve.LaunchDelay+i*style.RewardFlyStagger));
                }
            }
        }
        void Update()
        {
            var style=factory.Style; float dt=Time.unscaledDeltaTime;
            for(int i=flights.Count-1;i>=0;i--)
            {
                var flight=flights[i]; flight.Elapsed+=dt;
                float t=RewardFlyCurve.Progress(flight.Elapsed,flight.Delay,style.RewardFlySeconds);
                if(flight.Target==null || t>=1) { Destroy(flight.Icon.gameObject); flights.RemoveAt(i); continue; }
                if(t<=0 && flight.Source!=null && flight.Source.gameObject.activeInHierarchy) flight.Origin=flight.Source.position;
                var target=flight.Target.position;
                var position=RewardFlyCurve.Position(flight.Origin,target,style.RewardFlyArc,t);
                flight.Icon.rectTransform.position=new Vector3(position.x,position.y,target.z);
                flight.Icon.rectTransform.localScale=Vector3.one*RewardFlyCurve.Scale(t);
                flight.Icon.canvasRenderer.SetAlpha(RewardFlyCurve.Alpha(t));
            }
        }
    }
}

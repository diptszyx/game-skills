using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Draws a ConfettiSimulation with pooled Images; the pool is built once and only toggled afterwards.
    public sealed class ConfettiView : MonoBehaviour
    {
        readonly List<Image> pieces=new List<Image>();
        ConfettiSimulation simulation; RectTransform root; float originHeightFraction;
        public bool Running=>simulation!=null&&simulation.Running;
        public int PieceCount=>pieces.Count;
        public static ConfettiView Create(RectTransform parent,ConfettiConfig config,float originHeightFraction,int seed)
        {
            var node=new GameObject("Confetti",typeof(RectTransform)); node.transform.SetParent(parent,false);
            var rect=(RectTransform)node.transform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var view=node.AddComponent<ConfettiView>(); view.root=rect; view.originHeightFraction=originHeightFraction;
            view.simulation=new ConfettiSimulation(config,seed);
            for(int i=0;i<config.Count;i++)
            {
                var piece=new GameObject("Piece",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<Image>();
                piece.transform.SetParent(rect,false); piece.raycastTarget=false; piece.gameObject.SetActive(false); view.pieces.Add(piece);
            }
            return view;
        }
        public void Play()
        {
            var area=root.rect;
            simulation.Start(new Vector2(0,(originHeightFraction-.5f)*area.height));
            for(int i=0;i<pieces.Count;i++)
            {
                pieces[i].rectTransform.sizeDelta=new Vector2(simulation.Size(i),simulation.Size(i)*.6f);
                pieces[i].gameObject.SetActive(true);
            }
            Draw();
        }
        public void Stop()
        {
            simulation.Stop();
            foreach(var piece in pieces) piece.gameObject.SetActive(false);
        }
        void Update()
        {
            if(!simulation.Running) return;
            simulation.Step(Time.unscaledDeltaTime);
            if(!simulation.Running) { Stop(); return; }
            Draw();
        }
        void Draw()
        {
            for(int i=0;i<pieces.Count;i++)
            {
                var piece=pieces[i]; var color=simulation.Color(i); color.a=simulation.Alpha(i);
                piece.color=color;
                piece.rectTransform.anchoredPosition=simulation.Position(i);
                piece.rectTransform.localRotation=Quaternion.Euler(0,0,simulation.Rotation(i));
            }
        }
    }
}

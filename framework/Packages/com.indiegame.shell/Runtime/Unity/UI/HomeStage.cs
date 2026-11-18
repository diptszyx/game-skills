using System;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    // Scales a design-sized stage onto its parent rect with the same stretch the screen background uses,
    // so layers placed in design pixels stay registered to the picture at every aspect ratio.
    public sealed class HomeStageFit : MonoBehaviour
    {
        public RectTransform Stage;
        public void Apply()
        {
            if(Stage==null) return;
            var area=((RectTransform)transform).rect; var design=Stage.sizeDelta;
            if(area.width<=0 || area.height<=0 || design.x<=0 || design.y<=0) return;
            Stage.localScale=new Vector3(area.width/design.x,area.height/design.y,1);
        }
        void OnEnable()=>Apply();
        void OnRectTransformDimensionsChange()=>Apply();
    }
    // Contract: the game factory returns a stage RectTransform whose sizeDelta is the design size of the screen
    // background picture (for example its pixel size). Children are positioned in that design space, centred
    // pivot. The stage lives inside Main, above the background and below Main's content, the chrome and popups,
    // never takes raycasts, and is built once.
    public static class HomeStageBuilder
    {
        public const string HolderName="Home stage";
        public static RectTransform Create(Transform mainRoot,ShellUiFactory factory,Func<HomeStageContext,RectTransform> build)
        {
            if(mainRoot==null) throw new ArgumentNullException(nameof(mainRoot));
            if(build==null) throw new ArgumentNullException(nameof(build));
            var holder=ShellUiFactory.Node(HolderName,mainRoot); ShellUiFactory.Stretch(holder); holder.SetAsFirstSibling();
            var group=holder.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts=false; group.interactable=false;
            RectTransform stage;
            try
            {
                stage=build(new HomeStageContext { Factory=factory,Style=factory.Style,Parent=holder });
                if(stage==null) throw new InvalidOperationException("HomeStageFactory returned null");
                if(stage.sizeDelta.x<=0 || stage.sizeDelta.y<=0) throw new InvalidOperationException("HomeStageFactory stage needs a positive sizeDelta (design size)");
            }
            catch(Exception) { Discard(holder.gameObject); throw; }
            if(stage.parent!=holder) stage.SetParent(holder,false);
            stage.anchorMin=stage.anchorMax=stage.pivot=new Vector2(.5f,.5f); stage.anchoredPosition=Vector2.zero;
            foreach(var graphic in stage.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget=false;
            holder.gameObject.AddComponent<HomeStageFit>().Stage=stage;
            holder.gameObject.SetActive(true); holder.GetComponent<HomeStageFit>().Apply();
            return stage;
        }
        static void Discard(GameObject value) { if(Application.isPlaying) UnityEngine.Object.Destroy(value); else UnityEngine.Object.DestroyImmediate(value); }
    }
}

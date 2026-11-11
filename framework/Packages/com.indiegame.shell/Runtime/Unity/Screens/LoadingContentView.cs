using IndieGame.Shell.Unity.UI;
using IndieGame.Shell.Unity.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class LoadingContentView : MonoBehaviour
    {
        LoadingScreenView loading; RectTransform fill; TMP_Text tip;
        public float RenderedProgress { get; private set; }
        public void Initialize(ShellUiFactory factory, LoadingScreenView loading, ScreenText text, string tipKey)
        {
            this.loading=loading;
            var track=ShellUiFactory.Node("Loading Progress",loading.transform); track.sizeDelta=new Vector2(500,24);
            track.gameObject.AddComponent<Image>().color=factory.Style.ProgressTrackColor;
            fill=ShellUiFactory.Node("Fill",track); fill.gameObject.AddComponent<Image>().color=factory.Style.ProgressFillColor;
            fill.anchorMin=Vector2.zero; fill.offsetMin=fill.offsetMax=Vector2.zero; fill.gameObject.SetActive(true); track.gameObject.SetActive(true);
            tip=factory.Label(loading.transform,text.Get(tipKey)); LocalizedLayout.LoadingTip(tip.rectTransform);
            // A raycast surface is required even when loading was created without a ScreenHost.
            if(loading.GetComponent<Graphic>()==null) loading.gameObject.AddComponent<Image>().color=Color.clear;
            Render();
        }
        void LateUpdate() { if(loading!=null) Render(); }
        public void Render() { RenderedProgress=loading.Controller.Progress; fill.anchorMax=new Vector2(RenderedProgress,1); }
    }
}

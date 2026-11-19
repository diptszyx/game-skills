using IndieGame.Shell.Unity.UI;
using IndieGame.Shell.Unity.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class LoadingContentView : MonoBehaviour
    {
        LoadingScreenView loading; ProgressBarView bar; TMP_Text tip;
        public float RenderedProgress { get; private set; }
        public void Initialize(ShellUiFactory factory, LoadingScreenView loading, ScreenText text, string tipKey)
        {
            this.loading=loading;
            bar=factory.ProgressBar(loading.transform,"Loading Progress",ProgressBarView.FlatColors); bar.Root.sizeDelta=new Vector2(500,24);
            tip=factory.Label(loading.transform,text.Get(tipKey)); LocalizedLayout.LoadingTip(tip.rectTransform);
            // A raycast surface is required even when loading was created without a ScreenHost.
            if(loading.GetComponent<Graphic>()==null) loading.gameObject.AddComponent<Image>().color=Color.clear;
            Render();
        }
        void LateUpdate() { if(loading!=null) Render(); }
        public void Render() { RenderedProgress=loading.Controller.Progress; bar.SetRatio(RenderedProgress); }
    }
}

using System;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class GameHudView : IDisposable
    {
        const float FrameWidth=640, FrameHeight=720, ViewportInset=36;
        GameViewportView viewport;
        public RectTransform Frame { get; }
        public GameViewportView Viewport=>viewport??=new GameViewportView(Frame,new Vector2(ViewportInset,ViewportInset));
        public GameHudView(ShellUiFactory factory, Transform parent, bool drawFrame)
        {
            Frame=drawFrame ? factory.AppearanceImage(parent,"Board frame","boardFrame",true).rectTransform : ShellUiFactory.Node("Board frame",parent);
            Frame.anchorMin=Frame.anchorMax=new Vector2(.5f,.5f); Frame.sizeDelta=new Vector2(FrameWidth,FrameHeight); Frame.anchoredPosition=Vector2.zero;
            Frame.gameObject.SetActive(true); Frame.SetAsFirstSibling();
        }
        public void Dispose() { viewport?.Dispose(); viewport=null; }
    }
}

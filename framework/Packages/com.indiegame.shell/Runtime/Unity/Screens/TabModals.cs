using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity.Screens
{
    // Bottom-tab screens shown as popups over Home. Uses the shared PopupHost; at most one tab popup is open.
    public sealed class TabModals
    {
        const float PanelMarginX=.06f, PanelMarginY=.02f;
        readonly PopupHost host; readonly ShellUiFactory factory;
        readonly Dictionary<ShellScreen,PopupView> frames=new Dictionary<ShellScreen,PopupView>();
        public event Action<PopupView> FrameCreated;
        public event System.Action Opened;
        public TabModals(PopupHost host,ShellUiFactory factory) { this.host=host; this.factory=factory; }
        public bool HasFrame(ShellScreen screen)=>frames.ContainsKey(screen);
        public PopupView Frame(ShellScreen screen)=>frames[screen];
        public ShellScreen? Active
        {
            get { foreach(var pair in frames) if(pair.Value.Host==host && !pair.Value.Closing) return pair.Key; return null; }
        }
        public PopupView CreateFrame(ShellScreen screen,string closeLabel)
        {
            var frame=factory.PopupFrame(host.transform,"",true,closeLabel); frame.ManagedContent=true; frame.name="Tab "+screen;
            var root=(RectTransform)frame.transform; root.offsetMin=new Vector2(0,ScreenWidgets.TabBarHeight);
            var panel=frame.Panel; panel.anchorMin=new Vector2(PanelMarginX,PanelMarginY); panel.anchorMax=new Vector2(1-PanelMarginX,1-PanelMarginY);
            panel.offsetMin=panel.offsetMax=Vector2.zero;
            frames.Add(screen,frame); FrameCreated?.Invoke(frame); return frame;
        }
        public void Open(ShellScreen screen)
        {
            var frame=frames[screen];
            if(Active==screen) return;
            CloseActive();
            host.Open(frame);
            Opened?.Invoke();
        }
        public void CloseActive()
        {
            var active=Active;
            if(active!=null && host.Top==frames[active.Value]) host.Close(frames[active.Value]);
        }
        public bool IsTop(PopupView frame)=>host.Top==frame;
        public void SetCloseLabel(string label)
        { foreach(var frame in frames.Values) frame.CloseButton.GetComponent<ActionAccessibility>().Set("close",label); }
    }
}

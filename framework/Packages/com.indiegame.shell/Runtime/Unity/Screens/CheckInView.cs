using System;
using UnityEngine;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class CheckInView
    {
        readonly ListSurface surface;
        public RectTransform Root => surface.Root;
        public ScreenLifetime Lifetime => surface.Lifetime;
        public ListDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action ClaimRequested;
        public CheckInView(ShellUiFactory factory, Transform parent) { surface=new ListSurface(factory,parent,"CheckIn"); }
        public void Render(ListDisplay display) { Snapshot=display; surface.Render(display,_=>RequestClaim()); }
        public void RequestClaim() { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Enabled) { ClaimRequested?.Invoke(); break; } }
        public void ShowError(string error) { Error=error; surface.Error(error); }
    }
}

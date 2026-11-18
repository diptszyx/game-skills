using System;
using UnityEngine;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class QuestsView
    {
        readonly ListSurface surface;
        public RectTransform Root => surface.Root;
        public ScreenLifetime Lifetime => surface.Lifetime;
        public ListDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action<string> ClaimRequested;
        public QuestsView(ShellUiFactory factory, Transform parent, PopupView modal=null) { surface=new ListSurface(factory,parent,"Quests",modal); }
        public void Render(ListDisplay display) { Snapshot=display; surface.Render(display,RequestClaim); }
        public void RequestClaim(string id) { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Id==id && r.Enabled) { ClaimRequested?.Invoke(id); break; } }
        public void ShowError(string error) { Error=error; surface.Error(error); }
    }
}

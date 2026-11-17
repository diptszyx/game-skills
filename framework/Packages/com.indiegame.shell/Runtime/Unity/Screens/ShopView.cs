using System;
using TMPro;
using UnityEngine;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class ShopView
    {
        readonly ListSurface surface;
        readonly PopupHost popups;
        readonly PopupView popup;
        readonly TMP_Text popupTitle, popupBody;
        public RectTransform Root => surface.Root;
        public ScreenLifetime Lifetime => surface.Lifetime;
        public ListDisplay Snapshot { get; private set; }
        public string Error { get; private set; }
        public event Action<string> BuyRequested;
        public ShopView(ShellUiFactory factory, Transform parent, PopupHost popups)
        {
            this.popups=popups; surface=new ListSurface(factory,parent,"Shop");
            popup=factory.PopupFrame(popups.transform,""); popupTitle=popup.Panel.GetComponentInChildren<TMP_Text>(true); popupBody=factory.Label(popup.Content,"");
        }
        public void SetCloseName(string label)=>popup.CloseButton.GetComponent<ActionAccessibility>().Set("close",label);
        public void Render(ListDisplay display) { Snapshot=display; surface.Render(display,RequestBuy); }
        public void RequestBuy(string id) { if(Snapshot!=null) foreach(var r in Snapshot.Rows) if(r.Id==id && r.Enabled) { BuyRequested?.Invoke(id); break; } }
        public void ShowError(string title,string error) { Error=error; surface.Error(error); popupTitle.text=title; popupBody.text=error; popups.Open(popup); }
    }
}

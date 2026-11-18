using System;
using System.Collections.Generic;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class ShopPresenter : IDisposable
    {
        readonly ShellSession session; readonly ShopService shop; readonly ShopView view; readonly ScreenText text;
        bool busy; string language;
        public ShopPresenter(ShellSession session, ShopService shop, ShopView view, LocalizationService localization)
        {
            this.session=session; this.shop=shop; this.view=view; text=new ScreenText(session,localization);
            view.BuyRequested+=Buy; view.Lifetime.Entered+=Enter; view.Lifetime.Resumed+=Resume; view.Lifetime.Ticked+=Tick; Enter();
        }
        public void Enter() => Refresh();
        public void Resume() => Refresh();
        public void Tick() { if(language!=session.Settings.Language) Refresh(); }
        public void Refresh()
        {
            language=session.Settings.Language; view.SetCloseName(text.Get("shell.close")); var rows=new List<RowDisplay>();
            foreach(var p in shop.Catalog.Products) rows.Add(Row(p));
            view.Render(new ListDisplay(text.Get("shell.shop.title"),rows,text.Get("shell.shop.empty")));
        }
        RowDisplay Row(Product p)
        {
            bool gold=p.PriceKind==PriceKind.Gold;
            return new RowDisplay(p.Id,text.Get(p.TitleKey),text.Get("shell.buy"),!busy,icon:gold?"coin":"",amount:gold?p.Price.ToString(Invariant):text.Get("shell.price.iap",(p.Price/100m).ToString("0.00",Invariant)),detail:text.RewardShort(p.Reward));
        }
        static readonly System.Globalization.CultureInfo Invariant=System.Globalization.CultureInfo.InvariantCulture;
        void Buy(string id)
        {
            if(busy) return; busy=true;
            try { var result=shop.Buy(id,Guid.NewGuid().ToString("N")); if(result.Success) RetrySave(); else Error(text.Get("shell.error."+result.Error)); }
            finally { busy=false; Refresh(); }
        }
        void Error(string message) => view.ShowError(text.Get("shell.error.title"),message);
        public void RetrySave() => text.Commit(Error);
        public void Dispose() { view.BuyRequested-=Buy; view.Lifetime.Entered-=Enter; view.Lifetime.Resumed-=Resume; view.Lifetime.Ticked-=Tick; }
    }
}

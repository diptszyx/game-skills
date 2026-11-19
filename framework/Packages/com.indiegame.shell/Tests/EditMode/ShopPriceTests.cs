using System;
using System.Linq;
using IndieGame.Shell.Unity.Localization;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ShopPriceTests
    {
        DemoStyles demos; GameObject root; ShopView view; ShopPresenter presenter;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); var f=new ShellUiFactory(demos.Make(false));
            root=new GameObject("UI",typeof(RectTransform),typeof(Canvas)); var screen=(RectTransform)new GameObject("Screen",typeof(RectTransform)).transform; screen.SetParent(root.transform,false);
            var session=ScreenTestData.Session(new ScreenClock(),new ScreenFiles()); session.LoadAll();
            view=new ShopView(f,screen,root.AddComponent<PopupHost>());
            presenter=new ShopPresenter(session,new ShopService(ShopPack.Standard(50,30,5),session,new FakePurchaseService()),view,ScreenTestData.Text());
        }
        [TearDown] public void Cleanup() { presenter.Dispose(); UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        Button ButtonOf(string id) => view.Root.GetComponentsInChildren<Button>(true).Where(b=>b.GetComponent<ActionAccessibility>()!=null).ElementAt(Array.FindIndex(view.Snapshot.Rows.ToArray(),r=>r.Id==id));
        static string AmountText(Button b) => b.GetComponentsInChildren<TMP_Text>(true).Single().text;
        static bool HasIcon(Button b) => b.GetComponentsInChildren<ShellAppearance>(true).Any(a=>a.Role=="actionIcon");
        [Test] public void IapButtonShowsTheFormattedPriceWithoutTheCurrencyIcon()
        {
            foreach(var (id,price) in new[]{(ShopPack.GoldSmall,"$0.99"),(ShopPack.GoldMedium,"$4.99"),(ShopPack.GoldLarge,"$9.99")})
            {
                var b=ButtonOf(id); Assert.That(AmountText(b),Is.EqualTo(price),id); Assert.That(HasIcon(b),Is.False,id);
            }
        }
        [Test] public void GoldButtonKeepsTheCurrencyIconAndAmount()
        {
            var b=ButtonOf(ShopPack.Hearts); Assert.That(AmountText(b),Is.EqualTo("30")); Assert.That(HasIcon(b),Is.True);
        }
        [Test] public void IapDetailLineDropsThePurchaseFiller()
        {
            var row=view.Snapshot.Rows.First(r=>r.Id==ShopPack.GoldSmall);
            Assert.That(row.Detail,Is.Null); Assert.That(row.Rewards.Select(r=>r.Text),Is.EqualTo(new[]{"50"}));
        }
        [Test] public void PriceFormatKeyExistsInBothLanguagesWithAPlaceholder()
        {
            var text=SampleLocalization.Load();
            foreach(var lang in new[]{"en","vi"}) Assert.That(text.Lookup(lang,"shell.price.iap").Value,Does.Contain("{0}"),lang);
        }
    }
}

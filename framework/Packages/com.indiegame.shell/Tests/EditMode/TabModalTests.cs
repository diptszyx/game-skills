using System.Linq;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class TabModalTests
    {
        PopupFixture f; TabModals modals;
        [SetUp] public void Setup() { f=new PopupFixture(); modals=new TabModals(f.Host,f.Factory); }
        [TearDown] public void Cleanup() { f.Dispose(); }
        [Test] public void ModalTabsAreOnByDefaultAndNeedHomeChrome()
        {
            var style=ScriptableObject.CreateInstance<ShellStyle>();
            Assert.That(style.TabsAsModals,Is.True); Assert.That(style.ModalTabs,Is.False,"no chrome means no tab bar");
            style.EnableHomeChrome=true; Assert.That(style.ModalTabs,Is.True);
            style.TabsAsModals=false; Assert.That(style.ModalTabs,Is.False);
            Object.DestroyImmediate(style);
        }
        [Test] public void FrameLeavesTheTabBarStripOutsideTheScrim()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close");
            Assert.That(((RectTransform)frame.transform).offsetMin.y,Is.EqualTo(160f)); Assert.That(frame.Policy,Is.EqualTo(DismissalPolicy.Dismissible));
            Assert.That(frame.Panel.anchorMin.x,Is.GreaterThan(0f)); Assert.That(frame.Panel.anchorMax.x,Is.LessThan(1f));
        }
        [Test] public void ListInFrameUsesThePopupTitleAndHasNoBackButtonOrOwnHeading()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); var view=new QuestsView(f.Factory,f.Root.transform,frame);
            view.Render(new ListDisplay("Quests",new[]{new RowDisplay("q","Quest","Claim",true)}));
            Assert.That(view.Root.parent,Is.SameAs(frame.Content)); Assert.That(frame.Panel.Find("Title").GetComponent<TMP_Text>().text,Is.EqualTo("Quests"));
            Assert.That(view.Root.GetComponentsInChildren<ShellAppearance>(true).Any(a=>a.Role=="screenTitle"),Is.False);
            Assert.That(view.Root.GetComponentsInChildren<ActionAccessibility>(true).Any(a=>a.Key=="back"),Is.False);
        }
        [Test] public void RowsInsideAFrameStillRaiseClaims()
        {
            var frame=modals.CreateFrame(ShellScreen.Quests,"Close"); var view=new QuestsView(f.Factory,f.Root.transform,frame); string claimed=null;
            view.ClaimRequested+=id=>claimed=id;
            view.Render(new ListDisplay("Quests",new[]{new RowDisplay("q","Quest","Claim",true)}));
            var button=view.Root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.GetComponent<ActionAccessibility>()!=null);
            button.onClick.Invoke(); Assert.That(claimed,Is.EqualTo("q"));
        }
        [Test] public void ShopRowsInsideAFrameStillRaiseBuys()
        {
            var frame=modals.CreateFrame(ShellScreen.Shop,"Close"); var view=new ShopView(f.Factory,f.Root.transform,f.Host,frame); string bought=null;
            view.BuyRequested+=id=>bought=id;
            view.Render(new ListDisplay("Shop",new[]{new RowDisplay("p","Product","Buy",true,icon:"coin")}));
            var button=view.Root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.GetComponent<ActionAccessibility>()!=null);
            button.onClick.Invoke(); Assert.That(bought,Is.EqualTo("p"));
        }
        [Test] public void OpeningASecondTabReplacesTheFirstAndBackClosesIt()
        {
            modals.CreateFrame(ShellScreen.CheckIn,"Close"); modals.CreateFrame(ShellScreen.Quests,"Close");
            modals.Open(ShellScreen.CheckIn); Assert.That(modals.Active,Is.EqualTo(ShellScreen.CheckIn));
            modals.Open(ShellScreen.Quests); Assert.That(modals.Active,Is.EqualTo(ShellScreen.Quests)); Assert.That(f.Host.Top,Is.SameAs(modals.Frame(ShellScreen.Quests)));
            Assert.That(f.Host.Back(),Is.True); Assert.That(modals.Active,Is.Null);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class IconStructureTests
    {
        static readonly string[] Keys={"back","home","sound_on","sound_off","music_on","music_off","haptics_on","haptics_off","video","gold","play","next","retry","check","close","gift","heart","coin","refresh","trophy","save","globe"};
        [Test] public void EveryKeyHasCachedProceduralFallback()
        {
            using(var f=new PopupFixture()) {
                var method=typeof(ShellStyle).GetMethod("Icon");
                Assert.That(method,Is.Not.Null,"Style must supply action icons without art");
                foreach(var key in Keys) {
                    var sprite=method.Invoke(f.Factory.Style,new object[]{key}) as Sprite;
                    Assert.That(sprite,Is.Not.Null,key);
                    Assert.That(method.Invoke(f.Factory.Style,new object[]{key}),Is.SameAs(sprite),key);
                    Assert.That(sprite.texture.GetPixels32().Any(p=>p.a>0),Is.True,key);
                }
            }
        }
        [TestCase("en")] [TestCase("vi")]
        public void ModalActionsAreLocalizedIconsWithFixedGeometry(string language)
        {
            using(var f=new PopupFixture()) {
                f.Session.Settings.SetLanguage(language);
                f.Presenter.ShowPause();
                var pause=f.Presenter.Popups[SessionPopupKind.Pause];
                Assert.That(pause.Buttons["leave"].name,Is.EqualTo(language=="en"?"Home":"Trang chu"));
                foreach(var key in new[]{"sound","music","haptics"}) {
                    var b=pause.Buttons[key];
                    Assert.That(b.GetComponent<LayoutCategory>().Category.ToString(),Is.EqualTo("IconButton"));
                    Assert.That(((RectTransform)b.transform).rect.size,Is.EqualTo(new Vector2(64,64)));
                    Assert.That(b.transform.parent,Is.SameAs(pause.Buttons["sound"].transform.parent));
                }
                f.Presenter.ShowLeave(); f.Presenter.ShowWin("icons"); f.Presenter.ShowLose(); f.Presenter.ShowRevive();
                foreach(var popup in f.Presenter.Popups.Values) { popup.View.Reflow(); foreach(var b in popup.Buttons.Values) {
                    Assert.That(b.name,Is.Not.Null.And.Not.Empty);
                    Assert.That(b.GetComponentsInChildren<ShellAppearance>(true).Any(a=>a.Role=="actionIcon"),Is.True,b.name);
                    Assert.That(b.GetComponentsInChildren<TMP_Text>(true).All(t=>t.text=="x2"||t.text=="20"),Is.True,b.name);
                    var category=b.GetComponent<LayoutCategory>().Category.ToString();
                    var expected=category=="IconButton"?new Vector2(64,64):new Vector2(popup.Buttons.Values.Where(action=>action.gameObject.activeSelf && action.GetComponent<LayoutCategory>().Category!=ShellLayoutCategory.IconButton).Max(action=>ShellUiFactory.IconSize(action.GetComponent<LayoutCategory>().Category).x),72);
                    if(!b.gameObject.activeSelf) expected=ShellUiFactory.IconSize(b.GetComponent<LayoutCategory>().Category);
                    Assert.That(((RectTransform)b.transform).rect.size,Is.EqualTo(expected),b.name);
                } }
                foreach(var kind in new[]{SessionPopupKind.Win,SessionPopupKind.Lose,SessionPopupKind.Revive}) {
                    var b=f.Presenter.Popups[kind].Buttons[kind==SessionPopupKind.Win?"double":kind==SessionPopupKind.Lose?"revive":"ad"];
                    Assert.That(b.GetComponentsInChildren<ShellAppearance>().Any(a=>a.IconKey=="video"),Is.True);
                }
            }
        }
        [Test] public void IconModalsMeasureContentWithoutLegacyHeightOverride()
        {
            using(var f=new PopupFixture()) {
                f.Presenter.ShowPause(); var view=f.Presenter.Popups[SessionPopupKind.Pause].View; view.Reflow();
                Assert.That(view.Panel.rect.height,Is.LessThan(800).And.GreaterThanOrEqualTo(240));
                f.Factory.Style.PopupLayout.PanelMinHeight=600; f.Factory.Style.PopupLayout.PanelMaxHeight=650; view.Reflow();
                Assert.That(view.Panel.rect.height,Is.InRange(600,650));
            }
        }
        [Test] public void SpriteOverridesAndRoundHitAreaSurviveStyleSwitch()
        {
            using(var f=new PopupFixture()) {
                var b=f.Factory.IconButton(f.Root.transform,"back","Back");
                var image=b.GetComponent<Image>(); var padding=image.raycastPadding;
                var rect=((RectTransform)b.transform).rect;
                Assert.That(rect.width-padding.x-padding.z,Is.GreaterThanOrEqualTo(96));
                Assert.That(rect.height-padding.y-padding.w,Is.GreaterThanOrEqualTo(96));
                var glyph=f.Factory.Style.Icon("gift");
                f.Factory.Style.Icons.Add(new ShellStyle.TabIconEntry { Key="back",Sprite=glyph });
                f.Factory.SwitchStyle(f.Root.transform,f.Factory.Style);
                Assert.That(b.GetComponentsInChildren<ShellAppearance>().Single(a=>a.Role=="actionIcon").GetComponent<Image>().sprite,Is.SameAs(glyph));
                Assert.That(((RectTransform)b.transform).rect.size,Is.EqualTo(new Vector2(64,64)));
            }
        }
        [TestCase(false,"en")] [TestCase(false,"vi")] [TestCase(true,"en")] [TestCase(true,"vi")]
        public void EverySubscreenHasLocalizedIconsAndSafeTopLeftBack(bool chrome,string language)
        {
            using(var f=new PopupFixture()) {
                f.Factory.Style.EnableHomeChrome=chrome; f.Session.Settings.SetLanguage(language);
                var localization=IndieGame.Shell.Unity.Localization.SampleLocalization.Load();
                var text=new ScreenText(f.Session,localization);
                var app=f.Root.AddComponent<ShellApp>(); var screens=f.Root.AddComponent<ScreenHost>(); screens.Initialize(f.Factory);
                void Property(string name,object value)=>typeof(ShellApp).GetProperty(name).SetValue(app,value);
                void Field(string name,object value)=>typeof(ShellApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app,value);
                Property("Ready",true); Property("Screens",screens); Property("Popups",f.Host); Property("Session",f.Session); Property("Text",localization);
                Field("factory",f.Factory); Field("screenText",text);
                Field("config",new ShellAppConfig { Style=f.Factory.Style,Clock=new ScreenClock(),BoardId="board",ShopCatalog=ScreenTestData.Catalog(),Purchases=new FakePurchaseService(),
                    Leaderboard=new LeaderboardRequests(new FakeLeaderboardService("me",new FakeLeaderboardState())) });
                foreach(ShellScreen screen in Enum.GetValues(typeof(ShellScreen))) {
                    app.Navigate(screen);
                    var back=screens.Current.GetComponentsInChildren<ActionAccessibility>().Single(a=>a.Key=="back");
                    Assert.That(back.Label,Is.EqualTo(text.Get("shell.back")));
                    var rect=(RectTransform)back.transform;
                    Assert.That(rect.anchorMin,Is.EqualTo(new Vector2(0,1))); Assert.That(rect.anchorMax,Is.EqualTo(new Vector2(0,1)));
                    Assert.That(rect.anchoredPosition,Is.EqualTo(new Vector2(16,-16)));
                    foreach(var b in screens.Current.GetComponentsInChildren<Button>()) {
                        var accessible=b.GetComponent<ActionAccessibility>();
                        Assert.That(accessible,Is.Not.Null,b.name); Assert.That(accessible.Label,Is.Not.Empty.And.Not.Contain("["));
                        Assert.That(b.name,Is.EqualTo(accessible.Label));
                        Assert.That(b.GetComponentsInChildren<ShellAppearance>().Any(a=>a.Role=="actionIcon"),Is.True,b.name);
                        Assert.That(b.GetComponentsInChildren<TMP_Text>().All(t=>long.TryParse(t.text,out _)),Is.True,b.name);
                        Assert.That(((RectTransform)b.transform).rect.size,Is.EqualTo(ShellUiFactory.IconSize(b.GetComponent<LayoutCategory>().Category)),b.name);
                    }
                }
            }
        }
        [Test] public void BakedCloseUsesOneVisibleGlyphAndAlignedHitTarget()
        {
            using(var f=new PopupFixture()) {
                f.Factory.Style.PanelHasBakedCloseIcon=true;
                var popup=f.Factory.PopupFrame(f.Root.transform,"Popup");
                var close=popup.CloseButton;
                var glyph=close.GetComponentsInChildren<ShellAppearance>().Single(a=>a.Role=="actionIcon").GetComponent<Image>();
                Assert.That(close.GetComponent<Image>().color.a,Is.Zero,"Panel already draws its close circle");
                Assert.That(glyph.enabled,Is.False,"Panel already draws its X");
                Assert.That(((RectTransform)close.transform).anchoredPosition,Is.EqualTo(new Vector2(-36,-40)));
                Assert.That(close.GetComponent<Image>().raycastTarget,Is.True);
                f.Factory.Style.PanelHasBakedCloseIcon=false;
                f.Factory.SwitchStyle(f.Root.transform,f.Factory.Style);
                Assert.That(glyph.enabled,Is.True); Assert.That(close.GetComponent<Image>().color.a,Is.GreaterThan(0));
            }
        }
        [TestCase("en")] [TestCase("vi")]
        public void MainPlayKeepsLocalizedAccessibilityName(string language)
        {
            using(var f=new PopupFixture()) {
                f.Session.Settings.SetLanguage(language); var localization=IndieGame.Shell.Unity.Localization.SampleLocalization.Load();
                var view=new MainView(f.Factory,f.Root.transform);
                using(var presenter=new MainPresenter(f.Session,view,localization,new ScreenClock(),f.Game,_=>{})) {
                    var b=view.Root.GetComponentsInChildren<Button>().Single(button=>button.GetComponentInChildren<TMP_Text>()?.text==view.Snapshot.Play);
                    Assert.That(b.GetComponent<ActionAccessibility>(),Is.Not.Null);
                    Assert.That(b.GetComponent<ActionAccessibility>().Label,Is.EqualTo(view.Snapshot.Play));
                    Assert.That(b.name,Is.EqualTo(view.Snapshot.Play));
                }
            }
        }
        [TestCase("en")] [TestCase("vi")]
        public void FiveTabsKeepLocalizedAccessibilityNames(string language)
        {
            using(var f=new PopupFixture()) {
                var localization=IndieGame.Shell.Unity.Localization.SampleLocalization.Load();
                var keys=new[]{"home","checkin","quests","shop","leaderboard"};
                var tabs=new TabBarView(f.Factory,f.Root.transform,keys.Select(key=>new TabDefinition(key,localization.Lookup(language,"shell.tab."+key).Value,key)).ToArray());
                var buttons=tabs.Root.GetComponentsInChildren<Button>(); Assert.That(buttons.Length,Is.EqualTo(5));
                foreach(var b in buttons) {
                    var accessible=b.GetComponent<ActionAccessibility>(); Assert.That(accessible,Is.Not.Null);
                    Assert.That(accessible.Label,Is.EqualTo(localization.Lookup(language,"shell.tab."+accessible.Key).Value));
                    Assert.That(b.name,Is.EqualTo(accessible.Label));
                }
                tabs.SetLabel("home","Updated localized home");
                Assert.That(buttons.Single(b=>b.GetComponent<ActionAccessibility>().Key=="home").name,Is.EqualTo("Updated localized home"));
            }
        }
        [Test] public void ToggleChangesGlyphAndKeepsPersistence()
        {
            using(var f=new PopupFixture()) {
                f.Presenter.ShowPause(); var b=f.Presenter.Popups[SessionPopupKind.Pause].Buttons["sound"];
                var icon=b.GetComponentsInChildren<ShellAppearance>().FirstOrDefault(a=>a.Role=="actionIcon");
                Assert.That(icon,Is.Not.Null); var before=icon.GetComponent<Image>().sprite;
                f.Presenter.Act(SessionPopupKind.Pause,"sound");
                Assert.That(icon.IconKey,Is.EqualTo("sound_off"));
                Assert.That(icon.GetComponent<Image>().sprite,Is.Not.SameAs(before));
                Assert.That(f.Files.Writes,Is.GreaterThan(0));
            }
        }
    }
}

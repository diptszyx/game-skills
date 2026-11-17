using System;
using System.Linq;
using IndieGame.Shell.Unity.UI;
using IndieGame.Shell.Unity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class HomeChromeTests
    {
        DemoStyles demos; GameObject root; ShellStyle style; ShellUiFactory factory;
        static readonly string[] Keys={"home","checkin","quests","shop","leaderboard"};
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); style=Chrome(demos.Make(false)); factory=new ShellUiFactory(style);
            root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas));
        }
        ShellStyle Chrome(ShellStyle s)
        {
            s.EnableHomeChrome=true;
            s.TabBarPlainBackground=s.DayCellCollected=s.DayCellToday=s.DayCellLocked=s.BadgeSprite=s.PlainPillBackground=s.TabCellBackground=s.TabCellSelectedBackground=s.GearButton=s.BoardFrame=s.CurrencyIcon;
            foreach(var key in Keys) s.TabIcons.Add(new ShellStyle.TabIconEntry{Key=key,Sprite=s.CurrencyIcon});
            return s;
        }
        TabBarView Tabs(Transform parent=null) => new TabBarView(factory,parent??root.transform,Keys.Select(k=>new TabDefinition(k,k,k)).ToArray());
        [TearDown] public void Cleanup() { UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        [Test] public void ChunkyPaletteTextColoursMeetMinimumContrast()
        {
            var palette=ShellKitPalette.Chunky;
            Assert.That(ShellStyle.Contrast(palette.Outline,palette.Cream),Is.GreaterThanOrEqualTo(ShellStyle.MinTextContrast));
            Assert.That(ShellStyle.Contrast(palette.Outline,palette.Backdrop),Is.GreaterThanOrEqualTo(ShellStyle.MinTextContrast));
        }
        [Test] public void RealChunkySpriteColoursMeetMinimumContrast()
        { using(var fixture=new ChunkyFixture()) Assert.DoesNotThrow(()=>fixture.Style.Validate()); }
        // Reuse DemoStyles' owned assets so this exercises the same imported atlas as production.
        sealed class ChunkyFixture : IDisposable
        {
            readonly object demos; readonly Type type; public ShellStyle Style;
            public ChunkyFixture()
            {
                type=Type.GetType("IndieGame.Shell.Tests.DemoStyles, IndieGame.Shell.Tests.PlayMode",true);
                demos=Activator.CreateInstance(type,true); Style=(ShellStyle)type.GetMethod("MakeChunky").Invoke(demos,null);
            }
            public void Dispose()=>type.GetMethod("Dispose").Invoke(demos,null);
        }
        [Test] public void SpriteContrastRejectsGoodDeclaredButBadRenderedColours()
        {
            style.PanelColor=Color.black; style.PanelTextColor=Color.white;
            style.SurfaceColor=Color.white; // The actual white sprite contradicts declared black.
            Assert.That(Assert.Throws<InvalidOperationException>(()=>style.Validate()).Message,Does.Contain("popupText"));
        }
        [Test] public void EveryRenderedTextRoleRejectsLowContrast()
        {
            foreach(var role in new[]{"popupText","backdropText","title","body","buttonText"})
            {
                using(var fixture=new ChunkyFixture())
                {
                    var s=fixture.Style;
                    if(role=="popupText") s.PanelTextColor=s.PanelColor;
                    else if(role=="backdropText") s.BackdropTextColor=s.BackdropColor;
                    else if(role=="title") s.TitleColor=Color.red;
                    else if(role=="body") s.Theme.BodyColor=s.PanelColor;
                    else s.Theme.Primary.LabelColor=new Color(1,.4f,.3f);
                    Assert.Throws<InvalidOperationException>(()=>s.Validate(),role);
                }
            }
        }
        [Test] public void LowContrastPanelTextFailsValidation()
        {
            style.PanelColor=ShellKitPalette.Chunky.Cream; style.PanelTextColor=ShellKitPalette.Chunky.Cream;
            Assert.That(Assert.Throws<InvalidOperationException>(()=>style.Validate()).Message,Does.Contain("PanelTextColor"));
        }
        [Test] public void HalfDeclaredContrastPairFailsValidation()
        {
            style.BackdropTextColor=ShellKitPalette.Chunky.Outline; style.BackdropColor=Color.clear;
            Assert.That(Assert.Throws<InvalidOperationException>(()=>style.Validate()).Message,Does.Contain("BackdropTextColor"));
        }
        [Test] public void ContrastOfBlackOnWhiteIs21() { Assert.That(ShellStyle.Contrast(Color.black,Color.white),Is.EqualTo(21f).Within(.01f)); }
        [Test] public void HomeChromeStyleValidates() { Assert.DoesNotThrow(()=>style.Validate()); }
        [Test] public void EveryMissingChromeSlotThrowsOnlyWhenEnabled()
        {
            foreach(var name in new[]{"TabBarPlainBackground","DayCellCollected","DayCellToday","DayCellLocked","BadgeSprite","PlainPillBackground","TabCellBackground","TabCellSelectedBackground","GearButton","BoardFrame"})
            {
                var field=typeof(ShellStyle).GetField(name); var saved=field.GetValue(style); field.SetValue(style,null);
                Assert.That(Assert.Throws<InvalidOperationException>(()=>style.Validate()).Message,Does.Contain(name));
                style.EnableHomeChrome=false; Assert.DoesNotThrow(()=>style.Validate()); style.EnableHomeChrome=true; field.SetValue(style,saved);
            }
            foreach(var entry in style.TabIcons)
            {
                var saved=entry.Sprite; entry.Sprite=null;
                Assert.That(Assert.Throws<InvalidOperationException>(()=>style.Validate()).Message,Does.Contain(entry.Key)); entry.Sprite=saved;
            }
        }
        [Test] public void SetBadgeTogglesOnlyRequestedTab()
        {
            var tabs=Tabs(); var badge=tabs.Root.Find("Tabs/Tab shop/Icon/Badge").gameObject;
            Assert.That(badge.activeSelf,Is.False); tabs.SetBadge("shop",true); Assert.That(badge.activeSelf,Is.True);
            Assert.That(tabs.Root.Find("Tabs/Tab home/Icon/Badge").gameObject.activeSelf,Is.False);
            tabs.SetBadge("shop",false); Assert.That(badge.activeSelf,Is.False);
        }
        [Test] public void SelectingTabFiresOneEventWithKey()
        {
            var tabs=Tabs(); int count=0; string received=null; tabs.TabSelected+=key=>{count++; received=key;};
            tabs.SetSelected("home"); Assert.That(count,Is.Zero);
            Assert.That(tabs.Root.Find("Tabs/Tab shop/Cell").GetComponent<Image>().raycastTarget,Is.True);
            tabs.Root.Find("Tabs/Tab shop").GetComponent<Button>().onClick.Invoke();
            Assert.That(count,Is.EqualTo(1)); Assert.That(received,Is.EqualTo("shop"));
            var selected=tabs.Root.Find("Tabs/Tab shop/Icon"); Assert.That(selected.localScale.x,Is.EqualTo(1.15f));
            Assert.That(selected.GetComponent<Image>().color,Is.EqualTo(style.TabSelectedTint));
        }
        [Test] public void ShowPreservesChromeAndTabVisibility()
        {
            var host=root.AddComponent<ScreenHost>(); host.Initialize(factory); var tabs=Tabs(); host.SetTabBar(tabs);
            Assert.That(tabs.Root.gameObject.activeSelf,Is.False); host.ShowTabBar(true);
            var first=factory.ScreenRoot(root.transform); host.Show(first); host.Show(factory.ScreenRoot(root.transform));
            Assert.That(first.gameObject.activeSelf,Is.False); Assert.That(host.ChromeRoot.gameObject.activeInHierarchy,Is.True);
            Assert.That(tabs.Root.gameObject.activeInHierarchy,Is.True); Assert.That(tabs.Root.parent,Is.SameAs(host.ChromeRoot));
            Assert.That(root.GetComponentsInChildren<SafeAreaFitter>(true).Length,Is.EqualTo(1));
            Assert.That(host.ChromeRoot.GetSiblingIndex(),Is.GreaterThan(host.Current.GetSiblingIndex()));
            Assert.That(host.Loading.transform.GetSiblingIndex(),Is.EqualTo(root.transform.childCount-1));
            host.ShowTabBar(false); host.Show(first); Assert.That(tabs.Root.gameObject.activeSelf,Is.False);
        }
        [Test] public void StyleSwitchUpdatesEveryChromeSpriteAndKeepsState()
        {
            var top=new TopBarView(factory,root.transform); top.Render(new TopBarDisplay(123,4,"02:30"));
            var tabs=Tabs(); tabs.SetSelected("quests"); tabs.SetBadge("shop",true);
            var day=new DayCellView(factory,root.transform); day.Render("Day 2","50",DayState.Today);
            var replacement=Chrome(demos.Make(true));
            factory.SwitchStyle(root.transform,replacement);
            foreach(var binding in root.GetComponentsInChildren<ShellAppearance>(true).Where(b=>b.GetComponent<Image>()!=null))
            {
                var expected=binding.Role=="heartIcon"?replacement.HeartIcon:replacement.CurrencyIcon;
                Assert.That(binding.GetComponent<Image>().sprite,Is.SameAs(expected),binding.Role);
            }
            Assert.That(tabs.Root.Find("Tabs/Tab shop/Icon/Badge").gameObject.activeSelf,Is.True);
            Assert.That(tabs.Root.Find("Tabs/Tab quests/Icon").localScale.x,Is.EqualTo(1.15f));
            Assert.That(tabs.Root.Find("Tabs/Tab quests/Icon").GetComponent<Image>().color,Is.EqualTo(replacement.TabSelectedTint));
            day.Render("Day 3","100",DayState.Locked);
            Assert.That(day.Root.Find("State").GetComponent<Image>().sprite,Is.SameAs(replacement.DayCellLocked));
        }
        [Test] public void DayCellsUseMatchingStateSprite()
        {
            style.DayCellToday=style.HeartIcon; style.DayCellLocked=style.LockedButtonSprite;
            var day=new DayCellView(factory,root.transform);
            foreach(DayState state in Enum.GetValues(typeof(DayState)))
            {
                day.Render("Day","Reward",state);
                Assert.That(day.Root.Find("State").GetComponent<Image>().sprite,Is.SameAs(state==DayState.Collected?style.DayCellCollected:state==DayState.Today?style.DayCellToday:style.DayCellLocked));
            }
        }
        [Test] public void NewSlicedSpritesHaveFourPositiveBorders()
        {
            foreach(var name in new[]{"tab_bar_plain","pill_plain","tab_cell","tab_cell_selected","day_cell_1","day_cell_2","day_cell_3"})
            {
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/KitChunky/"+name+".png"); Assert.That(sprite,Is.Not.Null,name);
                var b=sprite.border; Assert.That(new[]{b.x,b.y,b.z,b.w},Is.All.GreaterThan(0),name);
                Assert.That(b.x+b.z,Is.LessThan(sprite.rect.width)); Assert.That(b.y+b.w,Is.LessThan(sprite.rect.height));
            }
        }
        [Test] public void PillMeshPreservesAspectAndRoundCapsAt331By80()
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/KitChunky/pill_plain.png");
            var node=new GameObject("Pill mesh",typeof(RectTransform),typeof(Image));
            try
            {
                var image=node.GetComponent<Image>(); image.sprite=sprite; image.type=Image.Type.Sliced;
                image.rectTransform.sizeDelta=new Vector2(331,80);
                using(var helper=new VertexHelper())
                {
                    typeof(Image).GetMethod("OnPopulateMesh",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic,
                        null,new[]{typeof(VertexHelper)},null).Invoke(image,new object[]{helper});
                    var vertices=new System.Collections.Generic.List<UIVertex>(); helper.GetUIVertexStream(vertices);
                    var xs=vertices.Select(v=>v.position.x).Distinct().OrderBy(x=>x).ToArray();
                    var ys=vertices.Select(v=>v.position.y).Distinct().OrderBy(y=>y).ToArray();
                    float width=xs.Last()-xs.First(),height=ys.Last()-ys.First();
                    Assert.That(height/width,Is.EqualTo(80f/331).Within((80f/331)*.03f));
                    float cap=xs[1]-xs[0]; Assert.That(cap,Is.GreaterThanOrEqualTo(35));
                    Assert.That(cap/(ys[1]-ys[0]),Is.EqualTo(1).Within(.03f),"Caps must keep source aspect");
                    Assert.That(height,Is.EqualTo(sprite.rect.height/(sprite.pixelsPerUnit/100)).Within(2.4f));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(node); }
        }
        static float Luminance(Color c)
        {
            Func<float,float> linear=v=>v<=.04045f?v/12.92f:Mathf.Pow((v+.055f)/1.055f,2.4f);
            return .2126f*linear(c.r)+.7152f*linear(c.g)+.0722f*linear(c.b);
        }
        [Test] public void TopTextContrastAgainstSpriteInteriorExceedsFourPointFive()
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/KitChunky/pill_plain.png");
            var top=new TopBarView(factory,root.transform); top.Render(new TopBarDisplay(12345,5,"04:32"));
            float background=Luminance(texture.GetPixel(texture.width/2,texture.height/2));
            foreach(var label in top.Root.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                float foreground=Luminance(label.color);
                Assert.That((Mathf.Max(foreground,background)+.05f)/(Mathf.Min(foreground,background)+.05f),Is.GreaterThanOrEqualTo(4.5f));
                Assert.That(label.color,Is.EqualTo(Color.white));
            }
        }
        [Test] public void ScreenTitleRoleUsesOverrideAndFallsBackToTitleColor()
        {
            var screenTitle=factory.Label(root.transform,"x",true,"screenTitle"); var popupTitle=factory.Label(root.transform,"y",true);
            Assert.That(screenTitle.color,Is.EqualTo(style.TitleColor));
            style.ScreenTitleColorOverride=new Color(.2f,.05f,.2f,1);
            screenTitle.GetComponent<ShellAppearance>().Apply(style); popupTitle.GetComponent<ShellAppearance>().Apply(style);
            Assert.That(screenTitle.color,Is.EqualTo(style.ScreenTitleColorOverride));
            Assert.That(popupTitle.color,Is.EqualTo(style.TitleColor));
        }
        [Test] public void TabSelectionSwapsCellsAndRetiredSlotsAndAssetsAreAbsent()
        {
            style.TabCellBackground=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/KitChunky/tab_cell.png");
            style.TabCellSelectedBackground=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/KitChunky/tab_cell_selected.png");
            var tabs=Tabs();
            foreach(var selected in Keys)
            {
                tabs.SetSelected(selected);
                foreach(var key in Keys)
                    Assert.That(tabs.Root.Find("Tabs/Tab "+key+"/Cell").GetComponent<Image>().sprite,
                        Is.SameAs(key==selected?style.TabCellSelectedBackground:style.TabCellBackground));
            }
            foreach(var field in new[]{"CurrencyPillBackground","LivesPillBackground","TabBarBackground"})
                Assert.That(typeof(ShellStyle).GetField(field),Is.Null,field);
            var atlas=AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>("Assets/Art/KitChunky/KitChunkyUi.spriteatlas");
            foreach(var name in new[]{"pill_currency","pill_lives","tab_bar"})
            {
                Assert.That(System.IO.File.Exists("Assets/Art/KitChunky/"+name+".png"),Is.False);
                Assert.That(atlas.GetSprite(name),Is.Null,name);
            }
        }
    }
}

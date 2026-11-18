using System;
using System.Linq;
using System.Reflection;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ShellLayoutTests
    {
        DemoStyles demos; ShellStyle style; ShellUiFactory factory; GameObject root; float priorScale;
        [SetUp] public void Setup()
        {
            priorScale=Time.timeScale; demos=new DemoStyles(); style=demos.Make(false); factory=new ShellUiFactory(style);
            root=new GameObject("Layout tests",typeof(RectTransform),typeof(Canvas));
            ((RectTransform)root.transform).sizeDelta=new Vector2(720,1600);
        }
        [TearDown] public void Cleanup() { UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); Time.timeScale=priorScale; }
        static void Size(Component node, Vector2 expected) => Assert.That(((RectTransform)node.transform).rect.size,Is.EqualTo(expected),node.name);
        static void Category(Component node,string expected)
        {
            var tag=node.GetComponents<Component>().FirstOrDefault(c=>c.GetType().Name=="LayoutCategory");
            Assert.That(tag,Is.Not.Null,node.name+" must record a layout category");
            Assert.That(tag.GetType().GetProperty("Category").GetValue(tag).ToString(),Is.EqualTo(expected));
        }
        void AbsurdThemeSizes()
        {
            style.Theme.Primary.DefaultSize=new Vector2(7,999);
            style.Theme.Secondary.DefaultSize=new Vector2(9999,3);
            style.Theme.PanelSize=new Vector2(777,888);
        }
        [TestCase(ButtonRole.Primary)] [TestCase(ButtonRole.Secondary)] [TestCase(ButtonRole.Locked)]
        public void FactoryRoleUsesStandardLayout(ButtonRole role)
        { var button=factory.Button(root.transform,"Action",role); Size(button,new Vector2(280,80)); Category(button,"Standard"); }
        [Test] public void ThemeSizesDoNotChangeButtonsOrRows()
        {
            AbsurdThemeSizes();
            foreach(ButtonRole role in Enum.GetValues(typeof(ButtonRole))) Size(factory.Button(root.transform,"Action",role),new Vector2(280,80));
            var row=factory.ListRow(root.transform,"Row"); Size(row,new Vector2(600,120));
            Assert.That(row.GetComponent<LayoutElement>().preferredHeight,Is.EqualTo(120));
            var other=demos.Make(true); other.Theme.Primary.DefaultSize=Vector2.one;
            factory.SwitchStyle(root.transform,other);
            foreach(var button in root.GetComponentsInChildren<Button>()) Size(button,new Vector2(280,80));
            Size(row,new Vector2(600,120));
            Assert.That(style.Theme.Primary.DefaultSize,Is.EqualTo(new Vector2(7,999)),"No asset mutation");
        }
        [Test] public void PopupActionAndCloseHitUseShellLayout()
        {
            AbsurdThemeSizes(); var popup=factory.PopupFrame(root.transform,"Popup");
            var button=factory.Button(popup.Content,"Action",ButtonRole.Secondary);
            Size(button,new Vector2(400,76)); Category(button,"PopupAction");
            var close=(RectTransform)popup.CloseButton.transform; Size(close,new Vector2(64,64));
            Assert.That(close.anchoredPosition,Is.EqualTo(new Vector2(-36,-40)));
            Category(close,"IconButton");
        }
        [Test] public void PopupReflowCapsActionWidthAtAvailableSpace()
        {
            style.PopupLayout.PanelMinHeight=240; style.PopupLayout.PanelMaxHeight=1200;
            style.Theme.PanelSize=new Vector2(420,400);
            var popup=factory.PopupFrame(root.transform,"Popup"); var button=factory.Button(popup.Content,"Action");
            popup.gameObject.SetActive(true); popup.Reflow();
            Size(button,new Vector2(324,76)); Category(button,"PopupAction");
            popup.Panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,700); popup.Reflow();
            Size(button,new Vector2(400,76));
        }
        [TestCase(false,68)] [TestCase(true,200)]
        public void BackUsesStandardLayoutRegardlessOfTheme(bool chrome,float offset)
        {
            AbsurdThemeSizes(); style.EnableHomeChrome=chrome;
            var files=new ScreenFiles(); var session=ScreenTestData.Session(new ScreenClock(),files); session.LoadAll();
            var app=root.AddComponent<ShellApp>(); var host=root.AddComponent<ScreenHost>(); host.Initialize(factory);
            // Exercise actual Navigate construction without starting the PlayMode boot coroutine.
            typeof(ShellApp).GetProperty("Ready").SetValue(app,true);
            typeof(ShellApp).GetProperty("Screens").SetValue(app,host);
            typeof(ShellApp).GetProperty("Session").SetValue(app,session);
            typeof(ShellApp).GetProperty("Text").SetValue(app,ScreenTestData.Text());
            void Field(string name,object value)=>typeof(ShellApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app,value);
            Field("factory",factory); Field("config",new ShellAppConfig { Style=style });
            Field("screenText",new ScreenText(session,app.Text));
            app.Navigate(ShellScreen.CheckIn);
            var back=host.Current.GetComponentsInChildren<Button>().Single(b=>b.GetComponent<ActionAccessibility>()?.Key=="back");
            Size(back,new Vector2(64,64)); Category(back,"IconButton");
            Assert.That(back.GetComponentInChildren<TMPro.TMP_Text>(),Is.Null);
            Assert.That(((RectTransform)back.transform).anchorMax,Is.EqualTo(new Vector2(0,1)));
            Assert.That(((RectTransform)back.transform).anchoredPosition,Is.EqualTo(new Vector2(16,-16)));
        }
        [TestCase(ButtonRole.Primary)] [TestCase(ButtonRole.Secondary)] [TestCase(ButtonRole.Locked)]
        public void OversizedButtonBordersFailValidation(ButtonRole role)
        {
            var texture=style.Theme.Primary.Enabled.texture;
            foreach(bool horizontal in new[]{true,false})
            {
                // Vertical borders total 80 UI units: Standard fits, PopupAction (76) must fail.
                var sprite=Sprite.Create(texture,new Rect(0,0,16,16),Vector2.one*.5f,horizontal?.01f:.1f,0,SpriteMeshType.FullRect,
                    horizontal?new Vector4(4,0,4,0):new Vector4(0,4,0,4));
                try
                {
                    if(role==ButtonRole.Primary) style.Theme.Primary.Enabled=sprite;
                    else if(role==ButtonRole.Secondary) style.Theme.Secondary.Enabled=sprite;
                    else style.LockedButtonSprite=sprite;
                    var error=Assert.Throws<InvalidOperationException>(()=>style.Validate());
                    Assert.That(error.Message,Does.Contain("border").And.Contain(role.ToString()));
                    if(!horizontal) Assert.That(error.Message,Does.Contain("PopupAction"));
                }
                finally { UnityEngine.Object.DestroyImmediate(sprite); }
            }
        }
        [Test] public void BorderValidationConvertsPixelsToUiUnits()
        {
            var texture=new Texture2D(1024,256); texture.SetPixels(Enumerable.Repeat(Color.white,1024*256).ToArray()); texture.Apply();
            var sprite=Sprite.Create(texture,new Rect(0,0,1024,256),Vector2.one*.5f,700,0,SpriteMeshType.FullRect,new Vector4(310,115,310,115));
            try
            {
                style.Theme.Primary.Enabled=style.Theme.Secondary.Enabled=style.LockedButtonSprite=sprite;
                Assert.DoesNotThrow(()=>style.Validate(),"620x230 source-pixel borders fit after /700 PPU conversion");
            }
            finally { UnityEngine.Object.DestroyImmediate(sprite); UnityEngine.Object.DestroyImmediate(texture); }
        }
        [TestCase(0)] [TestCase(2)] [TestCase(80)]
        public void PopupHeadingUsesRenderedTopBorderOrLegacyGeometry(float top)
        {
            var texture=new Texture2D(200,200);
            var sprite=Sprite.Create(texture,new Rect(0,0,200,200),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,top));
            try {
                style.Theme.PanelSprite=sprite;
                var popup=factory.PopupFrame(root.transform,"Title");
                popup.gameObject.SetActive(true); popup.Reflow();
                var title=popup.Panel.Find("Title").GetComponent<TMPro.TMP_Text>();
                Assert.That(title.rectTransform.rect.height,Is.EqualTo(top>=24?top:104).Within(.01));
                Assert.That(title.alignment,Is.EqualTo(top>=24?TMPro.TextAlignmentOptions.Center:TMPro.TextAlignmentOptions.Top));
                Assert.That(title.margin.y,Is.EqualTo(top>=24?0:8));
                Assert.That(popup.Content.offsetMax.y,Is.EqualTo(-(top>=24?top+28:132)).Within(.01));
                popup.Panel.GetComponent<Image>().pixelsPerUnitMultiplier=2;
                popup.Reflow();
                Assert.That(title.rectTransform.rect.height,Is.EqualTo(top>=48?top/2:104).Within(.01));
            }
            finally { UnityEngine.Object.DestroyImmediate(sprite); UnityEngine.Object.DestroyImmediate(texture); }
        }
        [Test] public void LabelledTextButtonsStayStackedSoLongCaptionsKeepFullWidth()
        {
            style.PopupLayout.PanelMinHeight=240; style.PopupLayout.PanelMaxHeight=1200;
            var popup=factory.PopupFrame(root.transform,"Actions");
            var buttons=new[]{factory.Button(popup.Content,"A long caption that needs the whole width"),factory.Button(popup.Content,"Another long caption")};
            popup.gameObject.SetActive(true); popup.Reflow(); Canvas.ForceUpdateCanvases();
            foreach(var button in buttons) Assert.That(button.transform.parent,Is.SameAs(popup.Content));
            Assert.That(buttons.Select(b=>b.transform.position.y).Distinct().Count(),Is.EqualTo(2));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void PopupGroupsUpToThreeActionsAndStacksFour(int count)
        {
            style.PopupLayout.PanelMinHeight=240; style.PopupLayout.PanelMaxHeight=1200;
            var popup=factory.PopupFrame(root.transform,"Actions");
            var buttons=Enumerable.Range(0,count).Select(i=>factory.IconButton(popup.Content,"retry","Action "+i,category:ShellLayoutCategory.IconAction)).ToArray();
            popup.gameObject.SetActive(true); popup.Reflow(); Canvas.ForceUpdateCanvases();
            foreach(var button in buttons) {
                var rect=(RectTransform)button.transform;
                Assert.That(rect.rect.width,Is.GreaterThanOrEqualTo(96));
                Assert.That(rect.rect.height,Is.GreaterThanOrEqualTo(72));
                if(count<=3) Assert.That(button.transform.parent.GetComponent<HorizontalLayoutGroup>(),Is.Not.Null);
                else Assert.That(button.transform.parent,Is.SameAs(popup.Content));
                Assert.That(rect.rect.size,Is.EqualTo(((RectTransform)buttons[0].transform).rect.size));
            }
            if(count<=3) Assert.That(buttons.Select(b=>b.transform.position.y).Distinct().Count(),Is.EqualTo(1));
            else Assert.That(buttons.Select(b=>b.transform.position.y).Distinct().Count(),Is.EqualTo(4));
        }
        [Test] public void CustomIconShapesReplaceFallbacksAndSurviveStyleSwitch()
        {
            var round=factory.IconButton(root.transform,"back","Back");
            var pill=factory.IconButton(root.transform,"retry","Retry",category:ShellLayoutCategory.IconAction);
            var defaultRound=round.GetComponent<Image>().sprite; var defaultPill=pill.GetComponent<Image>().sprite;
            style.RoundButtonSprite=style.PillButtonSprite=style.Theme.Primary.Enabled;
            Assert.DoesNotThrow(()=>style.Validate());
            factory.SwitchStyle(root.transform,style);
            foreach(var button in new[]{round,pill}) {
                Assert.That(button.GetComponent<Image>().sprite,Is.SameAs(style.Theme.Primary.Enabled));
                Assert.That(button.GetComponent<Image>().color,Is.EqualTo(Color.white));
            }
            Assert.That(round.GetComponent<Image>().type,Is.EqualTo(Image.Type.Simple));
            Assert.That(pill.GetComponent<Image>().type,Is.EqualTo(Image.Type.Sliced));
            style.RoundButtonSprite=style.PillButtonSprite=null; factory.SwitchStyle(root.transform,style);
            Assert.That(round.GetComponent<Image>().sprite,Is.SameAs(defaultRound));
            Assert.That(pill.GetComponent<Image>().sprite,Is.SameAs(defaultPill));
            Assert.That(pill.GetComponent<Image>().color,Is.EqualTo(style.ActionSurfaceColor));
        }
        [Test] public void StandardLayoutIsImmutable()
        {
            var type=typeof(ShellUiFactory).Assembly.GetType("IndieGame.Shell.Unity.UI.ShellLayout");
            Assert.That(type,Is.Not.Null,"ShellLayout must exist");
            Assert.That(type.IsSealed||type.IsValueType,Is.True);
            Assert.That(type.GetConstructors(),Is.Empty,"Only Standard is constructible");
            foreach(var field in type.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static))
                Assert.That(field.IsInitOnly||field.IsLiteral,Is.True,field.Name);
            foreach(var property in type.GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static))
                Assert.That(property.GetSetMethod(true),Is.Null,property.Name);
            var standard=type.GetProperty("Standard").GetValue(null);
            Assert.That(type.GetProperty("ButtonStandard").GetValue(standard),Is.EqualTo(new Vector2(280,80)));
            Assert.That(type.GetProperty("Row").GetValue(standard),Is.EqualTo(new Vector2(600,120)));
        }
    }
}

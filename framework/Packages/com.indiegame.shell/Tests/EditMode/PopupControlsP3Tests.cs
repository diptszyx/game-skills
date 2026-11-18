using System.Linq;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class PopupControlsP3Tests
    {
        const float RimRowsTop=35, HeaderRowsBottom=119, SpriteWidth=640, SpriteHeight=480;
        DemoStyles demos; ShellStyle style; ShellUiFactory factory; GameObject root; Texture2D texture; Sprite panelSprite;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); style=demos.Make(true);
            texture=new Texture2D((int)SpriteWidth,(int)SpriteHeight);
            panelSprite=Sprite.Create(texture,new Rect(0,0,SpriteWidth,SpriteHeight),new Vector2(.5f,.5f),200,0,SpriteMeshType.FullRect,new Vector4(130,130,130,134));
            style.Theme.PanelSprite=style.MandatoryPanelSprite=panelSprite;
            style.Theme.PanelPixelsPerUnitMultiplier=.8f; style.Theme.PanelSize=new Vector2(620,520);
            style.PanelHeaderRows=style.MandatoryPanelHeaderRows=new Vector2(RimRowsTop,HeaderRowsBottom);
            style.CloseVisibleSize=72;
            factory=new ShellUiFactory(style);
            root=new GameObject("Popup P3",typeof(RectTransform),typeof(Canvas));
            ((RectTransform)root.transform).sizeDelta=new Vector2(720,1600);
        }
        [TearDown] public void Cleanup()
        {
            Object.DestroyImmediate(root); Object.DestroyImmediate(panelSprite); Object.DestroyImmediate(texture); demos.Dispose();
        }
        static Rect InPanel(RectTransform panel,RectTransform node)
        {
            var corners=new Vector3[4]; node.GetWorldCorners(corners);
            var low=panel.InverseTransformPoint(corners[0]); var high=panel.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
        float RimUnits(PopupView popup)
        {
            var image=popup.Panel.GetComponent<Image>();
            return RimRowsTop/(image.pixelsPerUnit*image.pixelsPerUnitMultiplier);
        }
        float HeaderBottomUnits(PopupView popup)
        {
            var image=popup.Panel.GetComponent<Image>();
            return HeaderRowsBottom/(image.pixelsPerUnit*image.pixelsPerUnitMultiplier);
        }
        [TestCase(DismissalPolicy.Dismissible)] [TestCase(DismissalPolicy.Mandatory)]
        public void TitleSitsInsideVisibleHeaderBandAndClearsTheRim(DismissalPolicy policy)
        {
            var popup=factory.PopupFrame(root.transform,"Paused"); popup.Policy=policy;
            var title=InPanel(popup.Panel,(RectTransform)popup.Panel.Find("Title"));
            float top=popup.Panel.rect.yMax;
            Assert.That(top-title.yMax,Is.GreaterThanOrEqualTo(RimUnits(popup)),"title top must start below the rim");
            Assert.That(top-title.yMin,Is.LessThanOrEqualTo(HeaderBottomUnits(popup)),"title bottom must stay above the cream body");
        }
        [Test] public void TitleLeavesRoomForTheDrawnCloseButton()
        {
            var popup=factory.PopupFrame(root.transform,"Paused"); popup.Policy=DismissalPolicy.Dismissible;
            var title=InPanel(popup.Panel,(RectTransform)popup.Panel.Find("Title"));
            var close=InPanel(popup.Panel,(RectTransform)popup.CloseButton.transform);
            Assert.That(title.Overlaps(close),Is.False);
        }
        [Test] public void LegacyStyleKeepsOldHeadingAndCloseGeometry()
        {
            style.PanelHeaderRows=style.MandatoryPanelHeaderRows=Vector2.zero; style.CloseVisibleSize=0;
            var popup=factory.PopupFrame(root.transform,"Popup");
            var title=(RectTransform)popup.Panel.Find("Title");
            Assert.That(title.anchoredPosition,Is.EqualTo(Vector2.zero));
            var close=(RectTransform)popup.CloseButton.transform;
            Assert.That(close.rect.size,Is.EqualTo(new Vector2(64,64)));
            Assert.That(close.anchoredPosition,Is.EqualTo(new Vector2(-36,-40)));
        }
        [Test] public void CloseIsDrawnLargeWithBigHitAreaAndNotClipped()
        {
            var popup=factory.PopupFrame(root.transform,"Paused"); popup.Policy=DismissalPolicy.Dismissible;
            var close=(RectTransform)popup.CloseButton.transform;
            Assert.That(close.rect.width,Is.GreaterThanOrEqualTo(56)); Assert.That(close.rect.height,Is.GreaterThanOrEqualTo(56));
            var padding=close.GetComponent<Image>().raycastPadding;
            Assert.That(close.rect.width-padding.x-padding.z,Is.GreaterThanOrEqualTo(96));
            Assert.That(close.rect.height-padding.y-padding.w,Is.GreaterThanOrEqualTo(96));
            var box=InPanel(popup.Panel,close);
            var panel=popup.Panel.rect;
            Assert.That(box.xMin>=panel.xMin-.01f&&box.xMax<=panel.xMax+.01f&&box.yMin>=panel.yMin-.01f&&box.yMax<=panel.yMax+.01f,Is.True,"close must lie inside the panel, not clipped");
            Assert.That(popup.Panel.rect.yMax-box.yMax,Is.LessThanOrEqualTo(RimUnits(popup)+20),"close sits on the rim");
            var glyph=close.GetComponentsInChildren<ShellAppearance>().Single(a=>a.Role=="actionIcon");
            Assert.That(glyph.GetComponent<Image>().enabled,Is.True);
        }
        [Test] public void MandatoryPopupHasNoCloseButton()
        {
            var popup=factory.PopupFrame(root.transform,"Paused"); popup.Policy=DismissalPolicy.Mandatory;
            Assert.That(popup.CloseButton.gameObject.activeSelf,Is.False);
        }
        [TestCase("music_on")] [TestCase("sound_on")] [TestCase("haptics_on")] [TestCase("close")] [TestCase("play")]
        public void RoundButtonGlyphHasAtLeastEighteenPercentPaddingOnEverySide(string key)
        {
            var button=factory.IconButton(root.transform,key,key);
            var surface=(RectTransform)button.transform;
            var glyph=button.GetComponentsInChildren<ShellAppearance>().Single(a=>a.Role=="actionIcon").GetComponent<RectTransform>();
            var box=InPanel(surface,glyph);
            float size=Mathf.Min(surface.rect.width,surface.rect.height);
            Assert.That(box.xMin-surface.rect.xMin,Is.GreaterThanOrEqualTo(size*.18f-.01f));
            Assert.That(surface.rect.xMax-box.xMax,Is.GreaterThanOrEqualTo(size*.18f-.01f));
            Assert.That(box.yMin-surface.rect.yMin,Is.GreaterThanOrEqualTo(size*.18f-.01f));
            Assert.That(surface.rect.yMax-box.yMax,Is.GreaterThanOrEqualTo(size*.18f-.01f));
        }
    }
}

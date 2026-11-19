using System;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ProgressBarSkinTests
    {
        const float BarWidth=200,BarHeight=40,CapUnits=.5f;
        DemoStyles demos; GameObject root; ShellStyle style; Texture2D texture; Sprite frame,fill;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); style=demos.Make(false); root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas));
            texture=new Texture2D(16,16); frame=SlicedSprite(); fill=SlicedSprite();
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(frame); UnityEngine.Object.DestroyImmediate(fill);
            UnityEngine.Object.DestroyImmediate(texture); demos.Dispose();
        }
        Sprite SlicedSprite() => Sprite.Create(texture,new Rect(0,0,16,16),new Vector2(.5f,.5f),16,0,SpriteMeshType.FullRect,new Vector4(4,4,4,4));
        void Skin() { style.ProgressFrameSprite=frame; style.ProgressFillSprite=fill; style.ProgressChannelInset=new Vector4(.25f,.25f,.25f,.25f); }
        ProgressBarView Bar(float ratio)
        {
            var bar=new ShellUiFactory(style).ProgressBar(root.transform,"Bar",ProgressBarView.FlatColors);
            bar.Root.sizeDelta=new Vector2(BarWidth,BarHeight); bar.SetRatio(ratio); return bar;
        }
        static RectTransform FillRect(ProgressBarView bar) => (RectTransform)bar.Root.GetChild(0);
        [Test] public void DefaultLookIsFlatColoursWithoutSprites()
        {
            var bar=Bar(.5f); var track=bar.Root.GetComponent<Image>();
            Assert.That(track.sprite,Is.Null); Assert.That(bar.FillImage.sprite,Is.Null);
            Assert.That(track.color.r,Is.EqualTo(style.ProgressTrackColor.r).Within(.001f)); Assert.That(bar.FillImage.color.g,Is.EqualTo(style.ProgressFillColor.g).Within(.001f));
            Assert.That(FillRect(bar).anchorMax,Is.EqualTo(new Vector2(.5f,1)));
        }
        [Test] public void SkinIsAppliedWhenBothSpritesAreProvided()
        {
            Skin(); var bar=Bar(.5f); var track=bar.Root.GetComponent<Image>();
            Assert.That(track.sprite,Is.SameAs(frame)); Assert.That(track.type,Is.EqualTo(Image.Type.Sliced));
            Assert.That(bar.FillImage.sprite,Is.SameAs(fill)); Assert.That(bar.FillImage.type,Is.EqualTo(Image.Type.Sliced));
            Assert.That(track.color,Is.EqualTo(Color.white));
        }
        [Test] public void SwitchingStyleSwapsBetweenFlatAndSkinnedWithoutRebuilding()
        {
            var factory=new ShellUiFactory(style); var bar=factory.ProgressBar(root.transform,"Bar",ProgressBarView.FlatColors); var fillObject=bar.FillImage.gameObject;
            var skinned=demos.Make(false); skinned.ProgressFrameSprite=frame; skinned.ProgressFillSprite=fill;
            factory.SwitchStyle(root.transform,skinned);
            Assert.That(bar.Root.GetComponent<Image>().sprite,Is.SameAs(frame)); Assert.That(bar.FillImage.gameObject,Is.SameAs(fillObject));
            factory.SwitchStyle(root.transform,style); Assert.That(bar.Root.GetComponent<Image>().sprite,Is.Null);
        }
        [TestCase(0f,0f),TestCase(.02f,CapUnits+.02f*(BarWidth-.5f-CapUnits)),TestCase(.5f,CapUnits+.5f*(BarWidth-.5f-CapUnits)),TestCase(1f,BarWidth-.5f)]
        public void SkinnedFillWidthFollowsRatioAndNeverBelowItsCaps(float ratio,float expected)
        {
            Skin(); var bar=Bar(ratio); var rect=FillRect(bar);
            Assert.That(rect.rect.width,Is.EqualTo(expected).Within(.01f));
            Assert.That(bar.FillImage.enabled,Is.EqualTo(ratio>0));
            if(ratio>0) Assert.That(rect.rect.width,Is.GreaterThanOrEqualTo(CapUnits-.001f),"rounded cap is never squashed");
        }
        [TestCase(0f),TestCase(.02f),TestCase(.5f),TestCase(1f)]
        public void FlatFillAnchorEqualsRatio(float ratio) => Assert.That(FillRect(Bar(ratio)).anchorMax.x,Is.EqualTo(ratio).Within(.0001f));
        [Test] public void RatioIsClamped() { var bar=Bar(2f); Assert.That(bar.Ratio,Is.EqualTo(1f)); bar.SetRatio(-1); Assert.That(bar.Ratio,Is.EqualTo(0f)); }
        [Test] public void ValidateRejectsNegativeOrOversizedInsetAndHalfSkin()
        {
            Skin(); Assert.DoesNotThrow(()=>style.Validate());
            style.ProgressChannelInset=new Vector4(-1,0,0,0); Assert.Throws<InvalidOperationException>(()=>style.Validate());
            style.ProgressChannelInset=new Vector4(0,0,float.NaN,0); Assert.Throws<InvalidOperationException>(()=>style.Validate());
            style.ProgressChannelInset=new Vector4(.6f,0,.6f,0); Assert.Throws<InvalidOperationException>(()=>style.Validate(),"leaves no channel");
            Skin(); style.ProgressFillSprite=null; Assert.Throws<InvalidOperationException>(()=>style.Validate());
            Skin(); style.ProgressRowBarHeight=0; Assert.Throws<InvalidOperationException>(()=>style.Validate());
        }
    }
}

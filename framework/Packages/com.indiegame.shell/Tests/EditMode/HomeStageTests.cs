using System;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class HomeStageTests
    {
        static readonly Vector2 Design=new Vector2(841,1870);
        PopupFixture f; RectTransform main; int calls; Image layer;
        [SetUp] public void Setup()
        {
            f=new PopupFixture(); calls=0; layer=null;
            main=ShellUiFactory.Node("Main",f.Root.transform); main.anchorMin=main.anchorMax=main.pivot=new Vector2(.5f,.5f); main.sizeDelta=new Vector2(720,1600);
            var content=ShellUiFactory.Node("Home content",main); ShellUiFactory.Stretch(content);
        }
        [TearDown] public void Cleanup()=>f.Dispose();
        RectTransform Build(Func<HomeStageContext,RectTransform> factory=null)=>HomeStageBuilder.Create(main,f.Factory,factory??Fake);
        RectTransform Fake(HomeStageContext context)
        {
            calls++; Assert.That(context.Style,Is.SameAs(f.Factory.Style)); Assert.That(context.Factory,Is.SameAs(f.Factory));
            var stage=ShellUiFactory.Node("Fake stage",context.Parent); stage.sizeDelta=Design;
            var art=ShellUiFactory.Node("Layer",stage); art.sizeDelta=new Vector2(100,100); layer=art.gameObject.AddComponent<Image>(); layer.raycastTarget=true;
            return stage;
        }
        [Test] public void StageIsBuiltOnceFirstUnderMainAndNeverBlocksRaycasts()
        {
            var stage=Build();
            Assert.That(calls,Is.EqualTo(1));
            var holder=stage.parent; Assert.That(holder.name,Is.EqualTo(HomeStageBuilder.HolderName)); Assert.That(holder.parent,Is.SameAs(main));
            Assert.That(holder.GetSiblingIndex(),Is.EqualTo(0),"below Main content");
            Assert.That(holder.gameObject.activeSelf,Is.True);
            Assert.That(holder.GetComponent<CanvasGroup>().blocksRaycasts,Is.False);
            Assert.That(layer.raycastTarget,Is.False);
            Assert.That(stage.GetComponentsInChildren<Graphic>(true),Has.All.Matches<Graphic>(g=>!g.raycastTarget));
        }
        [Test] public void StageFollowsTheBackgroundStretchAtEveryAspect()
        {
            var stage=Build(); var fit=stage.parent.GetComponent<HomeStageFit>();
            foreach(var size in new[]{new Vector2(720,1600),new Vector2(720,1280),new Vector2(540,1600)})
            {
                main.sizeDelta=size; fit.Apply();
                Assert.That(stage.localScale.x,Is.EqualTo(size.x/Design.x).Within(1e-5f)); Assert.That(stage.localScale.y,Is.EqualTo(size.y/Design.y).Within(1e-5f));
                var corner=main.InverseTransformPoint(stage.TransformPoint(new Vector3(Design.x/2,Design.y/2,0)));
                Assert.That(corner.x,Is.EqualTo(size.x/2).Within(1e-3f)); Assert.That(corner.y,Is.EqualTo(size.y/2).Within(1e-3f));
            }
        }
        [Test] public void NullStageOrBadDesignSizeFailsAndLeavesNoHolder()
        {
            Assert.Throws<InvalidOperationException>(()=>Build(_=>null));
            Assert.Throws<InvalidOperationException>(()=>Build(c=>{ var zero=ShellUiFactory.Node("Zero",c.Parent); zero.sizeDelta=Vector2.zero; return zero; }));
            Assert.That(main.Find(HomeStageBuilder.HolderName),Is.Null);
        }
        [Test] public void ConfigDefaultsToNoStageFactory()
        {
            Assert.That(new ShellAppConfig().HomeStageFactory,Is.Null);
        }
    }
}

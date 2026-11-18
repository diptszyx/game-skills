using System;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class PressFeedbackTests
    {
        const float Seconds = .12f, Scale = .94f, Darken = .88f;
        [Test] public void CurveStartsAtRestAndReachesFullPressOnDown()
        {
            Assert.That(PressFeedbackCurve.Amount(0,true,0,Seconds),Is.EqualTo(0).Within(1e-6f));
            var amount=PressFeedbackCurve.Amount(0,true,Seconds,Seconds);
            Assert.That(amount,Is.EqualTo(1).Within(1e-6f));
            Assert.That(PressFeedbackCurve.Scale(amount,Scale),Is.EqualTo(Scale).Within(1e-6f));
            Assert.That(PressFeedbackCurve.Tint(amount,Darken),Is.EqualTo(Darken).Within(1e-6f));
        }
        [Test] public void ReleaseReturnsToRestWithBoundedOvershoot()
        {
            float min=0;
            for(float t=0;t<=Seconds;t+=Seconds/60) min=Mathf.Min(min,PressFeedbackCurve.Amount(1,false,t,Seconds));
            Assert.That(min,Is.LessThan(0),"overshoots past rest");
            Assert.That(min,Is.GreaterThan(-.2f));
            Assert.That(PressFeedbackCurve.Amount(1,false,Seconds,Seconds),Is.EqualTo(0).Within(1e-6f));
            Assert.That(PressFeedbackCurve.Scale(PressFeedbackCurve.Amount(1,false,Seconds*5,Seconds),Scale),Is.EqualTo(1).Within(1e-6f));
        }
        [Test] public void TintNeverBrightensDuringOvershoot()
        { Assert.That(PressFeedbackCurve.Tint(-.1f,Darken),Is.EqualTo(1)); }
        [Test] public void CurveIsDeterministic()
        { Assert.That(PressFeedbackCurve.Amount(.4f,false,.05f,Seconds),Is.EqualTo(PressFeedbackCurve.Amount(.4f,false,.05f,Seconds))); }
        [Test] public void StyleRejectsInvalidPressValues()
        {
            var demos=new DemoStyles();
            try
            {
                foreach(var apply in new Action<ShellStyle>[]{s=>s.PressScale=0,s=>s.PressScale=1.1f,s=>s.PressDarken=0,s=>s.PressSeconds=0})
                {
                    var style=demos.Make(false); apply(style);
                    Assert.Throws<InvalidOperationException>(()=>style.Validate());
                }
                var valid=demos.Make(false); Assert.That(valid.PressScale,Is.EqualTo(.94f)); Assert.DoesNotThrow(()=>valid.Validate());
            }
            finally { demos.Dispose(); }
        }
        [Test] public void FactoryButtonsHaveFeedbackAndScrimsDoNot()
        {
            var demos=new DemoStyles(); var root=new GameObject("Press",typeof(RectTransform),typeof(Canvas));
            try
            {
                var factory=new ShellUiFactory(demos.Make(false));
                var popup=factory.PopupFrame(root.transform,"Title");
                foreach(var button in popup.GetComponentsInChildren<Button>(true))
                {
                    var tag=button.GetComponent<LayoutCategory>(); var category=tag!=null?tag.Category:ShellLayoutCategory.Standard;
                    bool blocker=category==ShellLayoutCategory.PopupScrim||category==ShellLayoutCategory.PopupPanel;
                    Assert.That(button.GetComponent<PressFeedback>()!=null,Is.EqualTo(!blocker),button.name);
                }
                Assert.That(factory.Button(root.transform,"Go").GetComponent<PressFeedback>(),Is.Not.Null);
                Assert.That(factory.IconButton(root.transform,"close","Close").GetComponent<PressFeedback>(),Is.Not.Null);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        }
    }
}

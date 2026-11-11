using System.Collections;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class PopupHostTests
    {
        DemoStyles demos; GameObject root,events; ShellUiFactory factory; PopupHost host;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); factory=new ShellUiFactory(demos.Make(false));
            root=new GameObject("UI",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)); root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            events=new GameObject("Events",typeof(EventSystem)); host=root.AddComponent<PopupHost>();
        }
        [TearDown] public void Cleanup() { Time.timeScale=1; Object.DestroyImmediate(root); Object.DestroyImmediate(events); demos.Dispose(); }
        PopupView Popup(string title, DismissalPolicy policy=DismissalPolicy.Dismissible)
        {
            var p=factory.PopupFrame(root.transform,title); p.Policy=policy;
            p.InitialFocus=factory.Button(p.Content,"Action"); return p;
        }
        [UnityTest] public IEnumerator OnlyTopPopupReceivesInput()
        {
            var a=Popup("A"); var b=Popup("B"); host.Open(a); host.Open(b); yield return null;
            Assert.That(a.Group.interactable,Is.False); Assert.That(a.Group.blocksRaycasts,Is.False);
            Assert.That(a.InitialFocus.IsInteractable(),Is.False); Assert.That(b.InitialFocus.IsInteractable(),Is.True);
            Assert.That(b.Group.blocksRaycasts,Is.True); Assert.That(a.RequestDismiss(),Is.False);
            Assert.That(host.Back(),Is.True); Assert.That(b.Group.blocksRaycasts,Is.True);
            Assert.That(host.Back(),Is.False); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(host.Top,Is.SameAs(a));
        }
        [UnityTest] public IEnumerator ReopenLowerMovesToTop()
        {
            var a=Popup("A"); var b=Popup("B"); host.Open(a); host.Open(b); host.Open(a);
            Assert.That(host.Stack.Count,Is.EqualTo(2)); Assert.That(host.Top,Is.SameAs(a));
            Assert.That(a.transform.GetSiblingIndex(),Is.GreaterThan(b.transform.GetSiblingIndex()));
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.SameAs(a.InitialFocus.gameObject));
            Assert.That(host.Back(),Is.True); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(host.Top,Is.SameAs(b)); Assert.That(a.gameObject.activeSelf,Is.False);
            // Reopening during close cancels the close and retains a single entry.
            host.Back(); host.Open(b); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(host.Top,Is.SameAs(b)); Assert.That(host.Stack.Count,Is.EqualTo(1)); Assert.That(b.Group.alpha,Is.EqualTo(1).Within(.01));
        }
        [UnityTest] public IEnumerator MandatoryPopupRejectsDismissal()
        {
            var p=Popup("Mandatory",DismissalPolicy.Mandatory); host.Open(p);
            Assert.That(host.Back(),Is.False); Assert.That(p.RequestDismiss(),Is.False);
            p.GetComponent<Button>().onClick.Invoke(); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(host.Top,Is.SameAs(p)); Assert.That(p.gameObject.activeSelf,Is.True);
            Assert.That(host.Close(p),Is.True); yield return new WaitForSecondsRealtime(.12f); Assert.That(host.Top,Is.Null);
        }
        [UnityTest] public IEnumerator TransitionsWorkWhilePaused()
        {
            Time.timeScale=0; var p=Popup("Paused"); host.Open(p); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(p.Group.alpha,Is.EqualTo(1).Within(.01)); Assert.That(p.Panel.localScale,Is.EqualTo(Vector3.one));
            host.Back(); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(host.Top,Is.Null); Assert.That(p.gameObject.activeSelf,Is.False); Assert.That(Time.timeScale,Is.Zero);
        }
        [UnityTest] public IEnumerator PopRestoresFocus()
        {
            var outside=factory.Button(root.transform,"Outside"); EventSystem.current.SetSelectedGameObject(outside.gameObject);
            var a=Popup("A"); var b=Popup("B"); host.Open(a); host.Open(b); host.Back(); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.SameAs(a.InitialFocus.gameObject));
            host.Back(); yield return new WaitForSecondsRealtime(.12f);
            Assert.That(EventSystem.current.currentSelectedGameObject,Is.SameAs(outside.gameObject));
        }
    }
}

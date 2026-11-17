using System.Collections;
using System.Linq;
using TMPro;
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
        DemoStyles demos; GameObject root,events,fitCamera; RenderTexture fitTarget; ShellUiFactory factory; PopupHost host;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); factory=new ShellUiFactory(demos.Make(false));
            root=new GameObject("UI",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster)); root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            events=new GameObject("Events",typeof(EventSystem)); host=root.AddComponent<PopupHost>();
        }
        [TearDown] public void Cleanup() { if(fitCamera!=null) Object.DestroyImmediate(fitCamera); if(fitTarget!=null) { fitTarget.Release(); Object.DestroyImmediate(fitTarget); } Time.timeScale=1; Object.DestroyImmediate(root); Object.DestroyImmediate(events); demos.Dispose(); }
        PopupView Popup(string title, DismissalPolicy policy=DismissalPolicy.Dismissible)
        {
            var p=factory.PopupFrame(root.transform,title); p.Policy=policy;
            p.InitialFocus=factory.Button(p.Content,"Action"); return p;
        }
        [Test] public void MandatoryPopupUsesNoCloseBadgePanel()
        {
            var style=factory.Style; var normal=style.Theme.PanelSprite;
            var noClose=Sprite.Create(normal.texture,normal.rect,new Vector2(.5f,.5f),normal.pixelsPerUnit,0,SpriteMeshType.FullRect,normal.border);
            try
            {
                style.MandatoryPanelSprite=noClose; style.Validate();
                var p=Popup("Mandatory",DismissalPolicy.Mandatory);
                var frame=p.Panel.GetComponent<Image>();
                Assert.That(frame.sprite,Is.SameAs(noClose));
                Assert.That(frame.type,Is.EqualTo(Image.Type.Sliced));
                p.Policy=DismissalPolicy.Dismissible;
                Assert.That(frame.sprite,Is.SameAs(normal));
                p.Policy=DismissalPolicy.Mandatory;
                Assert.That(frame.sprite,Is.SameAs(noClose));
                var replacement=demos.Make(true); replacement.MandatoryPanelSprite=noClose;
                factory.SwitchStyle(root.transform,replacement);
                Assert.That(frame.sprite,Is.SameAs(noClose));
                p.Policy=DismissalPolicy.Dismissible;
                Assert.That(frame.sprite,Is.SameAs(replacement.Theme.PanelSprite));
            }
            finally { Object.DestroyImmediate(noClose); }
        }
        [Test] public void MandatoryPanelFallsBackToNormalPanelWhenSlotEmpty()
        {
            Assert.That(factory.Style.MandatoryPanelSprite,Is.Null);
            Assert.DoesNotThrow(()=>factory.Style.Validate());
            var p=Popup("Mandatory",DismissalPolicy.Mandatory);
            Assert.That(p.Panel.GetComponent<Image>().sprite,Is.SameAs(factory.Style.Theme.PanelSprite));
            var close=p.Panel.Find("CloseHit");
            Assert.That(close.GetComponent<Button>().interactable,Is.False);
            Assert.That(close.GetComponent<Image>().raycastTarget,Is.False);
            p.Policy=DismissalPolicy.Dismissible;
            Assert.That(p.Panel.GetComponent<Image>().sprite,Is.SameAs(factory.Style.Theme.PanelSprite));
            Assert.That(close.GetComponent<Button>().interactable,Is.True);
        }
        [UnityTest] public IEnumerator CloseAreaIsInertOnlyForMandatoryPopups()
        {
            foreach(var policy in new[]{DismissalPolicy.Mandatory,DismissalPolicy.Dismissible})
            {
                var p=Popup("Close",policy); host.Open(p); yield return null;
                var close=p.Panel.Find("CloseHit").GetComponent<Button>();
                Assert.That(close.interactable,Is.EqualTo(policy==DismissalPolicy.Dismissible));
                Assert.That(close.GetComponent<Image>().raycastTarget,Is.EqualTo(policy==DismissalPolicy.Dismissible));
                Canvas.ForceUpdateCanvases();
                var position=RectTransformUtility.WorldToScreenPoint(null,close.transform.position);
                var data=new PointerEventData(EventSystem.current){position=position,button=PointerEventData.InputButton.Left};
                var hits=new System.Collections.Generic.List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
                Assert.That(hits.Count,Is.GreaterThan(0));
                ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler);
                if(policy==DismissalPolicy.Dismissible)
                {
                    Assert.That(hits[0].gameObject.GetComponentInParent<Button>(),Is.SameAs(close));
                    float deadline=Time.realtimeSinceStartup+5;
                    while(p.gameObject.activeSelf&&Time.realtimeSinceStartup<deadline) yield return null;
                }
                Assert.That(p.gameObject.activeSelf,Is.EqualTo(policy==DismissalPolicy.Mandatory));
                if(policy==DismissalPolicy.Mandatory)
                {
                    Assert.That(host.Close(p),Is.True); float deadline=Time.realtimeSinceStartup+5;
                    while(p.gameObject.activeSelf&&Time.realtimeSinceStartup<deadline) yield return null;
                    Assert.That(p.gameObject.activeSelf,Is.False);
                }
            }
        }
        [UnityTest] public IEnumerator BalooVietnameseFitsAtNineBySixteen() { yield return CheckFit(new Vector2(1080,1920)); }
        [UnityTest] public IEnumerator BalooVietnameseFitsAtNineByTwenty() { yield return CheckFit(new Vector2(1080,2400)); }
        IEnumerator CheckFit(Vector2 resolution)
        {
            factory=new ShellUiFactory(demos.MakeChunky());
            fitCamera=new GameObject("Fit camera",typeof(Camera)); var camera=fitCamera.GetComponent<Camera>();
            camera.orthographic=true; camera.transform.position=new Vector3(0,0,-10);
            camera.nearClipPlane=.1f; camera.farClipPlane=100;
            fitTarget=new RenderTexture((int)resolution.x,(int)resolution.y,24); fitTarget.Create(); camera.targetTexture=fitTarget;
            var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=10;
            var scaler=root.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=resolution; scaler.matchWidthOrHeight=0;
            var p=factory.PopupFrame(root.transform,"Chúc mừng bạn đã hoàn thành thử thách tuyệt vời hôm nay");
            factory.Label(p.Content,"Bạn đã vượt qua một hành trình đầy thử thách. Hãy nhận phần thưởng của mình và tiếp tục khám phá những màn chơi thú vị tiếp theo!",role:"popupText");
            factory.Button(p.Content,"Nhận phần thưởng và tiếp tục"); factory.Button(p.Content,"Quay lại trang chủ",ButtonRole.Secondary);
            host.Open(p); yield return null; yield return null;
            Assert.That(((RectTransform)root.transform).rect.size.x,Is.EqualTo(resolution.x).Within(1));
            Assert.That(((RectTransform)root.transform).rect.size.y,Is.EqualTo(resolution.y).Within(1));
            CheckPanel(p);
        }
        [UnityTest] public IEnumerator OneButtonPanelHasNoExcessBottomSpace() { yield return CheckSize(1,false); }
        [UnityTest] public IEnumerator TwoButtonPanelHasNoExcessBottomSpace() { yield return CheckSize(2,false); }
        [UnityTest] public IEnumerator LongBodyExpandsPanel() { yield return CheckSize(2,true); }
        IEnumerator CheckSize(int buttons,bool longBody)
        {
            factory=new ShellUiFactory(demos.MakeChunky());
            var p=factory.PopupFrame(root.transform,"Phần thưởng");
            var body=factory.Label(p.Content,"Bạn đã hoàn thành!",role:"popupText");
            for(int i=0;i<buttons;i++) factory.Button(p.Content,"Tiếp tục");
            host.Open(p); yield return null; yield return null;
            float shortHeight=p.Panel.rect.height;
            if(longBody) { body.text=string.Join(" ",Enumerable.Repeat("Hãy tiếp tục khám phá hành trình tuyệt vời và nhận thêm phần thưởng mỗi ngày.",8)); yield return null; yield return null; Assert.That(p.Panel.rect.height,Is.GreaterThan(shortHeight)); }
            CheckPanel(p);
            var last=p.Content.GetComponentsInChildren<Button>().Last(); var corners=new Vector3[4]; ((RectTransform)last.transform).GetWorldCorners(corners);
            float bottom=p.Panel.InverseTransformPoint(corners[0]).y-p.Panel.rect.yMin;
            Assert.That(bottom,Is.InRange(0f,72f),"Empty space below last button (padding=48)");
            Assert.That(p.Panel.Find("CloseHit").GetComponent<RectTransform>().anchorMax,Is.EqualTo(Vector2.one));
            Assert.That(p.Panel.GetComponentInChildren<TMP_Text>().rectTransform.anchorMax.y,Is.EqualTo(1));
        }
        static void CheckPanel(PopupView p)
        {
            Canvas.ForceUpdateCanvases(); var corners=new Vector3[4];
            foreach(var rect in p.Panel.GetComponentsInChildren<RectTransform>().Where(r=>r.GetComponent<TMP_Text>()!=null||r.GetComponent<ShellAppearance>()?.Role=="button"))
            {
                rect.GetWorldCorners(corners);
                foreach(var corner in corners) { var point=p.Panel.InverseTransformPoint(corner); Assert.That(point.x,Is.InRange(p.Panel.rect.xMin-1,p.Panel.rect.xMax+1),rect.name); Assert.That(point.y,Is.InRange(p.Panel.rect.yMin-1,p.Panel.rect.yMax+1),rect.name); }
                if(rect.GetComponent<ShellAppearance>()?.Role=="button") Assert.That(rect.rect.width,Is.LessThanOrEqualTo(p.Panel.rect.width-96));
            }
            foreach(var label in p.Panel.GetComponentsInChildren<TMP_Text>())
            {
                label.ForceMeshUpdate(); bool auto=label.enableAutoSizing; label.enableAutoSizing=false;
                var preferred=label.GetPreferredValues(label.text,label.rectTransform.rect.width,float.PositiveInfinity); label.enableAutoSizing=auto;
                Assert.That(preferred.x,Is.LessThanOrEqualTo(label.rectTransform.rect.width+1),label.text);
                Assert.That(preferred.y,Is.LessThanOrEqualTo(label.rectTransform.rect.height+1),label.text);
                Assert.That(label.isTextOverflowing,Is.False,label.text);
            }
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

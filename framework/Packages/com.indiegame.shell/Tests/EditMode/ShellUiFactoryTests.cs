using System;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class ShellUiFactoryTests
    {
        DemoStyles demos; GameObject root; ShellStyle a,b; ShellUiFactory f;
        [SetUp] public void Setup() { demos=new DemoStyles(); a=demos.Make(false); b=demos.Make(true); f=new ShellUiFactory(a); root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas)); }
        [TearDown] public void Cleanup() { UnityEngine.Object.DestroyImmediate(root); demos.Dispose(); }
        [Test] public void BuildsAllPrimitives()
        {
            var screen=f.ScreenRoot(root.transform); Assert.That(screen.GetComponent<Image>().sprite,Is.SameAs(a.ScreenBackground));
            Assert.That(f.Label(screen,"Title",true).text,Is.EqualTo("Title"));
            foreach(ButtonRole role in Enum.GetValues(typeof(ButtonRole))) Assert.That(f.Button(screen,role.ToString(),role),Is.Not.Null);
            var scroll=f.ScrollList(screen); Assert.That(scroll.viewport.GetComponent<RectMask2D>(),Is.Not.Null);
            Assert.That(f.ListRow(scroll.content,"Row").GetComponent<Image>().sprite,Is.SameAs(a.ListRowSprite));
            var popup=f.PopupFrame(root.transform,"Popup"); Assert.That(popup.Content,Is.Not.Null); Assert.That(popup.gameObject.activeSelf,Is.False);
            Assert.That(f.PopupFrame(root.transform,"Without content",false).Content,Is.Null);
            var host=root.AddComponent<ScreenHost>(); host.Initialize(f); host.Show(screen);
            var second=f.ScreenRoot(root.transform,"Second"); host.Show(second);
            Assert.That(screen.gameObject.activeSelf,Is.False); Assert.That(second.parent,Is.SameAs(host.SafeRoot));
            Assert.That(host.SafeRoot.GetComponent<SafeAreaFitter>(),Is.Not.Null);
            Assert.That(host.Loading.gameObject.activeSelf,Is.True); Assert.That(host.Loading.transform.GetSiblingIndex(),Is.EqualTo(root.transform.childCount-1));
        }
        [Test] public void MissingRoleFailsByName()
        {
            a.CurrencyIcon=null; Assert.That(Assert.Throws<InvalidOperationException>(()=>new ShellUiFactory(a)).Message,Does.Contain("CurrencyIcon"));
            a.CurrencyIcon=b.CurrencyIcon; a.Theme.Font=null;
            Assert.That(Assert.Throws<InvalidOperationException>(()=>new ShellUiFactory(a)).Message,Does.Contain("Theme.Font"));
        }
        [Test] public void LockedButtonUsesLockedSprite()
        {
            var button=f.Button(root.transform,"Locked",ButtonRole.Locked);
            Assert.That(button.GetComponent<Image>().sprite,Is.SameAs(a.LockedButtonSprite)); Assert.That(button.interactable,Is.False);
        }
        [Test] public void StyleSwitchKeepsTextAndLayout()
        {
            var screen=f.ScreenRoot(root.transform); f.Label(screen,"Long title",true); f.Button(screen,"Play"); f.ListRow(f.ScrollList(screen).content,"Reward");
            var all=screen.GetComponentsInChildren<RectTransform>(true); var texts=screen.GetComponentsInChildren<TMP_Text>().Select(t=>t.text).ToArray();
            var layout=all.Select(t=>(t.anchorMin,t.anchorMax,t.sizeDelta,t.anchoredPosition)).ToArray();
            f.SwitchStyle(screen,b);
            Assert.That(screen.GetComponentsInChildren<RectTransform>(true).Length,Is.EqualTo(all.Length));
            Assert.That(screen.GetComponentsInChildren<TMP_Text>().Select(t=>t.text).ToArray(),Is.EqualTo(texts));
            Assert.That(all.Select(t=>(t.anchorMin,t.anchorMax,t.sizeDelta,t.anchoredPosition)).ToArray(),Is.EqualTo(layout));
            Assert.That(screen.GetComponent<Image>().color,Is.EqualTo(b.ScreenColor));
            Assert.That(screen.GetComponentInChildren<TMP_Text>().color,Is.EqualTo(b.TitleColor));
            Assert.That(a.name,Is.EqualTo("dark flat")); Assert.That(b.name,Is.EqualTo("bright rounded"));
        }
    }
    public sealed class ShellUiDependencyTests
    {
        [Test] public void RuntimeAssemblyHasNoEditorReferences()
        {
            var assembly=typeof(ShellUiFactory).Assembly;
            Assert.That(assembly.GetReferencedAssemblies().Any(n=>n.Name.StartsWith("UnityEditor")),Is.False);
            var path=System.IO.Path.Combine(Application.dataPath,"../../Packages/com.indiegame.shell/Runtime/Unity/IndieGame.Shell.Unity.asmdef");
            Assert.That(System.IO.File.ReadAllText(path),Does.Not.Contain("UnityEditor"));
        }
    }
}

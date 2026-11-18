using System.Linq;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace IndieGame.Shell.Tests
{
    public sealed class TabNavigationP3Tests
    {
        static readonly string[] Keys={"home","checkin","quests","shop","leaderboard"};
        const float Width=720;
        DemoStyles demos; GameObject root; ShellStyle style; ShellUiFactory factory; ScreenHost host; TabBarView tabs;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); style=demos.Make(false); style.EnableHomeChrome=true;
            style.TabBarPlainBackground=style.DayCellCollected=style.DayCellToday=style.DayCellLocked=style.BadgeSprite=style.PlainPillBackground=style.TabCellBackground=style.TabCellSelectedBackground=style.GearButton=style.BoardFrame=style.CurrencyIcon;
            foreach(var key in Keys) style.TabIcons.Add(new ShellStyle.TabIconEntry{Key=key,Sprite=style.CurrencyIcon});
            factory=new ShellUiFactory(style);
            root=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas));
            ((RectTransform)root.transform).sizeDelta=new Vector2(Width,1600);
            host=root.AddComponent<ScreenHost>(); host.Initialize(factory);
            ((RectTransform)host.SafeRoot).sizeDelta=new Vector2(Width,1600);
            tabs=new TabBarView(factory,root.transform,Keys.Select(k=>new TabDefinition(k,k,k)).ToArray()); host.SetTabBar(tabs); host.ShowTabBar(true);
            tabs.SetSelected("home");
        }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(root); demos.Dispose(); }
        static PointerEventData Pointer(Vector2 at) => new PointerEventData(null){position=at,pressPosition=at};
        void Drag(TabSwipeHandler h,Vector2 from,Vector2 to,float seconds)
        {
            float now=0; h.Clock=()=>now; h.Width=()=>Width;
            h.OnBeginDrag(Pointer(from)); now=seconds; var end=Pointer(to); end.pressPosition=from; h.OnEndDrag(end);
        }
        [Test] public void SwipeIsOnByDefaultAndCanBeDisabled()
        {
            Assert.That(ScriptableObject.CreateInstance<ShellStyle>().EnableTabSwipe,Is.True);
            Assert.That(TabSwipeHandler.Attach(style,host,tabs),Is.Not.Null);
            style.EnableTabSwipe=false; Assert.That(TabSwipeHandler.Attach(style,host,tabs),Is.Null);
        }
        [TestCase(-150f,0f,.6f,1)] [TestCase(150f,0f,.6f,-1)] [TestCase(-100f,0f,.6f,0)]
        [TestCase(-60f,0f,.05f,1)] [TestCase(-60f,0f,1f,0)] [TestCase(-200f,-180f,.4f,0)] [TestCase(-300f,40f,.4f,1)]
        public void RuleDecidesByDistanceFlickAndDominantAxis(float dx,float dy,float seconds,int expected)
        { Assert.That(TabSwipeRule.Decide(new Vector2(dx,dy),seconds,Width),Is.EqualTo(expected)); }
        [Test] public void LeftSwipeSelectsNextTabThroughTheSameEventAsATap()
        {
            var h=TabSwipeHandler.Attach(style,host,tabs); int count=0; string got=null; tabs.TabSelected+=k=>{count++; got=k;};
            Drag(h,new Vector2(500,800),new Vector2(300,800),.5f);
            Assert.That(count,Is.EqualTo(1)); Assert.That(got,Is.EqualTo("checkin")); Assert.That(tabs.Selected,Is.EqualTo("checkin"));
            Assert.That(tabs.ButtonFor("checkin").transform.Find("Icon").localScale.x,Is.EqualTo(1.15f));
            Assert.That(tabs.ButtonFor("home").transform.Find("Icon").localScale.x,Is.EqualTo(1f));
        }
        [Test] public void SwipeStopsAtBothEnds()
        {
            var h=TabSwipeHandler.Attach(style,host,tabs); int count=0; tabs.TabSelected+=_=>count++;
            Drag(h,new Vector2(300,800),new Vector2(500,800),.5f); Assert.That(count,Is.Zero); Assert.That(tabs.Selected,Is.EqualTo("home"));
            tabs.SetSelected("leaderboard"); Drag(h,new Vector2(500,800),new Vector2(300,800),.5f); Assert.That(count,Is.Zero);
        }
        [Test] public void VerticalDragDoesNotChangeTab()
        {
            var h=TabSwipeHandler.Attach(style,host,tabs); int count=0; tabs.TabSelected+=_=>count++;
            Drag(h,new Vector2(400,300),new Vector2(330,1100),.3f); Assert.That(count,Is.Zero);
        }
        [Test] public void SwipeIsIgnoredWhileTabBarHiddenOrPopupOpen()
        {
            var h=TabSwipeHandler.Attach(style,host,tabs); int count=0; tabs.TabSelected+=_=>count++;
            host.ShowTabBar(false); Drag(h,new Vector2(500,800),new Vector2(300,800),.5f); Assert.That(count,Is.Zero);
            host.ShowTabBar(true); var popup=ShellUiFactory.Node("Popup",host.SafeRoot); popup.gameObject.AddComponent<PopupView>(); popup.gameObject.SetActive(true);
            Drag(h,new Vector2(500,800),new Vector2(300,800),.5f); Assert.That(count,Is.Zero);
        }
        [Test] public void TabLabelsAreAtLeast22PxAtReferenceResolution()
        {
            foreach(var key in Keys)
            {
                var label=tabs.ButtonFor(key).GetComponentInChildren<TMP_Text>();
                Assert.That(label.fontSize,Is.GreaterThanOrEqualTo(22f),key); Assert.That(label.fontSizeMin,Is.GreaterThanOrEqualTo(22f),key);
            }
        }
    }
}

using System;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class ScreenHost : MonoBehaviour
    {
        public RectTransform SafeRoot { get; private set; }
        public RectTransform ChromeRoot { get; private set; }
        TabBarView tabBar;
        bool tabBarVisible;
        public LoadingScreenView Loading { get; private set; }
        public RectTransform Current { get; private set; }
        public void Initialize(ShellUiFactory factory)
        {
            if (SafeRoot != null) throw new InvalidOperationException("ScreenHost already initialized");
            SafeRoot = ShellUiFactory.Node("Safe Screens", transform); ShellUiFactory.Stretch(SafeRoot);
            SafeRoot.gameObject.AddComponent<SafeAreaFitter>(); SafeRoot.gameObject.SetActive(true);
            ChromeRoot = ShellUiFactory.Node("ChromeRoot", SafeRoot); ShellUiFactory.Stretch(ChromeRoot);
            ChromeRoot.gameObject.SetActive(true);
            var overlay = factory.ScreenRoot(transform, "Loading Overlay"); overlay.gameObject.SetActive(false);
            Loading = overlay.gameObject.AddComponent<LoadingScreenView>(); overlay.gameObject.SetActive(true);
            overlay.SetAsLastSibling();
        }
        public void SetTabBar(TabBarView view)
        {
            if (ChromeRoot == null) throw new InvalidOperationException("Initialize ScreenHost first");
            if (tabBar != null && tabBar != view) tabBar.Root.gameObject.SetActive(false);
            tabBar = view;
            if (view != null) { view.Root.SetParent(ChromeRoot, false); view.Root.gameObject.SetActive(tabBarVisible); }
        }
        public void ShowTabBar(bool visible)
        { tabBarVisible = visible; if (tabBar != null) tabBar.Root.gameObject.SetActive(visible); }
        public void Show(RectTransform screen)
        {
            if (SafeRoot == null) throw new InvalidOperationException("Initialize ScreenHost first");
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            foreach (Transform child in SafeRoot) if (child != ChromeRoot && child.GetComponent<PopupView>() == null) child.gameObject.SetActive(false);
            screen.SetParent(SafeRoot, false); ShellUiFactory.Stretch(screen); Current = screen; screen.gameObject.SetActive(true);
            screen.SetAsFirstSibling();
            Loading.transform.SetAsLastSibling();
        }
    }
}

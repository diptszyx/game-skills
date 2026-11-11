using System;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public sealed class ScreenHost : MonoBehaviour
    {
        public RectTransform SafeRoot { get; private set; }
        public LoadingScreenView Loading { get; private set; }
        public RectTransform Current { get; private set; }
        public void Initialize(ShellUiFactory factory)
        {
            if (SafeRoot != null) throw new InvalidOperationException("ScreenHost already initialized");
            SafeRoot = ShellUiFactory.Node("Safe Screens", transform); ShellUiFactory.Stretch(SafeRoot);
            SafeRoot.gameObject.AddComponent<SafeAreaFitter>(); SafeRoot.gameObject.SetActive(true);
            var overlay = factory.ScreenRoot(transform, "Loading Overlay"); overlay.gameObject.SetActive(false);
            Loading = overlay.gameObject.AddComponent<LoadingScreenView>(); overlay.gameObject.SetActive(true);
            overlay.SetAsLastSibling();
        }
        public void Show(RectTransform screen)
        {
            if (SafeRoot == null) throw new InvalidOperationException("Initialize ScreenHost first");
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            foreach (Transform child in SafeRoot) child.gameObject.SetActive(false);
            screen.SetParent(SafeRoot, false); ShellUiFactory.Stretch(screen); Current = screen; screen.gameObject.SetActive(true);
            Loading.transform.SetAsLastSibling();
        }
    }
}

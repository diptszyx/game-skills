using UnityEngine;

namespace IndieGame.Shell.Unity
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField, Min(0)] float minimumSeconds = 0.5f;
        [SerializeField, Min(0)] float fadeSeconds = 0.2f;
        CanvasGroup group;
        public LoadingScreenController Controller { get; private set; }
        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            Controller = new LoadingScreenController(new SystemClock(), minimumSeconds, fadeSeconds);
            Render();
        }
        public void Show() { Controller.Show(); Render(); }
        public void SetProgress(float value) { Controller.SetProgress(value); }
        public void Hide() { Controller.Hide(); Render(); }
        void Update() { Controller.Tick(); Render(); }
        void Render()
        {
            group.alpha = Controller.Alpha;
            group.blocksRaycasts = group.interactable = Controller.State != LoadingScreenState.Hidden;
        }
    }
}

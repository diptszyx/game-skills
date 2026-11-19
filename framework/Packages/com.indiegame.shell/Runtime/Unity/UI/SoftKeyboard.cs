using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Wraps the platform difference: TouchScreenKeyboard.area is a zero Rect on Android (Unity 6000.3 docs).
    public static class SoftKeyboard
    {
        public static float Height()
        {
            if(!TouchScreenKeyboard.visible) return 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHeight();
#else
            return TouchScreenKeyboard.area.height;
#endif
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        static float AndroidHeight()
        {
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var window=activity.Call<AndroidJavaObject>("getWindow"))
            using(var decor=window.Call<AndroidJavaObject>("getDecorView"))
            using(var visible=new AndroidJavaObject("android.graphics.Rect"))
            {
                decor.Call("getWindowVisibleDisplayFrame",visible);
                return Mathf.Max(0,Screen.height-visible.Get<int>("bottom"));
            }
        }
#endif
    }
}

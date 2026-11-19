using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Screen pixels, origin at the bottom edge. The keyboard always sits flush with the bottom, so its height is enough.
    public static class KeyboardAvoidance
    {
        public static float Shift(float panelBottom,float panelTop,float focusBottom,float keyboardHeight,float screenHeight,float margin)
        {
            if(keyboardHeight<=0) return 0;
            float clearLine=keyboardHeight+margin;
            float panelOverlap=Mathf.Max(0,clearLine-panelBottom);
            float roomAbove=Mathf.Max(0,screenHeight-panelTop);
            float focusOverlap=Mathf.Max(0,clearLine-focusBottom);
            return Mathf.Max(Mathf.Min(panelOverlap,roomAbove),focusOverlap);
        }
    }
}

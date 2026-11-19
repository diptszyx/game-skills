using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace IndieGame.Shell.Unity.UI
{
    // Lifts the top popup's panel while a TMP input inside it is focused, so the soft keyboard never covers the field.
    internal sealed class PopupKeyboardShift
    {
        const float MarginPixels=24;
        PopupView shifted; float appliedPixels;
        internal void Tick(PopupView top)
        {
            var input=FocusedInput(top);
            if(shifted!=null&&(shifted!=top||input==null)) Restore();
            if(input==null) return;
            var panel=ScreenRect(top.Panel); var field=ScreenRect((RectTransform)input.transform);
            float target=KeyboardAvoidance.Shift(panel.yMin-appliedPixels,panel.yMax-appliedPixels,field.yMin-appliedPixels,SoftKeyboard.Height(),Screen.height,MarginPixels);
            Move(top,target); shifted=top;
        }
        internal void Restore()
        {
            if(shifted!=null&&shifted.Panel!=null) Move(shifted,0);
            shifted=null; appliedPixels=0;
        }
        void Move(PopupView view,float targetPixels)
        {
            float delta=targetPixels-appliedPixels; if(Mathf.Approximately(delta,0)) return;
            view.Panel.anchoredPosition+=new Vector2(0,delta/view.Panel.GetComponentInParent<Canvas>().rootCanvas.scaleFactor);
            appliedPixels=targetPixels;
        }
        static TMP_InputField FocusedInput(PopupView top)
        {
            if(top==null||top.Closing||EventSystem.current==null) return null;
            var selected=EventSystem.current.currentSelectedGameObject; if(selected==null||!selected.transform.IsChildOf(top.transform)) return null;
            var input=selected.GetComponent<TMP_InputField>();
            return input!=null&&input.isFocused?input:null;
        }
        static Rect ScreenRect(RectTransform rect)
        {
            var canvas=rect.GetComponentInParent<Canvas>().rootCanvas; var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            var corners=new Vector3[4]; rect.GetWorldCorners(corners);
            Vector2 low=RectTransformUtility.WorldToScreenPoint(camera,corners[0]), high=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
    }
}

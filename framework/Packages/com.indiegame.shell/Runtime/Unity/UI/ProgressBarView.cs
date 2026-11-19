using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.UI
{
    public delegate void BarColorSource(ShellStyle style,out Color track,out Color fill);
    // Flat colour bar by default; a style with frame and fill sprites switches to a skinned bar without changing the hierarchy.
    public sealed class ProgressBarView : MonoBehaviour
    {
        Image track,fill; RectTransform fillRect; BarColorSource colors; ShellStyle style;
        public float Ratio { get; private set; }
        public RectTransform Root => (RectTransform)transform;
        public Image FillImage => fill;
        public static void FlatColors(ShellStyle style,out Color track,out Color fill) { track=style.ProgressTrackColor; fill=style.ProgressFillColor; }
        internal void Build(BarColorSource colors)
        {
            this.colors=colors; track=GetComponent<Image>();
            fillRect=ShellUiFactory.Node("Fill",Root);
            fill=fillRect.gameObject.AddComponent<Image>(); fillRect.gameObject.SetActive(true);
        }
        public void Apply(ShellStyle style)
        {
            this.style=style; bool skinned=style.HasProgressSkin;
            if(skinned)
            {
                track.sprite=style.ProgressFrameSprite; track.type=Image.Type.Sliced; track.color=Color.white;
                fill.sprite=style.ProgressFillSprite; fill.type=Image.Type.Sliced; fill.color=Color.white;
            }
            else
            {
                colors(style,out var trackColor,out var fillColor);
                trackColor.a=1; fillColor.a=1;
                track.sprite=null; track.type=Image.Type.Simple; track.color=trackColor;
                fill.sprite=null; fill.type=Image.Type.Simple; fill.color=fillColor;
            }
            Layout();
        }
        public void SetRatio(float value) { Ratio=Mathf.Clamp01(value); Layout(); }
        void OnRectTransformDimensionsChange() { if(fillRect!=null&&style!=null&&style.HasProgressSkin) Layout(); }
        // Flat fill scales with the ratio; a skinned fill keeps its sprite borders unstretched by never shrinking below its caps.
        void Layout()
        {
            if(style==null) return;
            if(!style.HasProgressSkin)
            { fillRect.anchorMin=Vector2.zero; fillRect.anchorMax=new Vector2(Ratio,1); fillRect.offsetMin=fillRect.offsetMax=Vector2.zero; return; }
            var inset=style.ProgressChannelInset;
            float channelWidth=Root.rect.width-inset.x-inset.z;
            fillRect.anchorMin=Vector2.zero; fillRect.anchorMax=Vector2.up; fillRect.pivot=new Vector2(0,.5f);
            fillRect.offsetMin=new Vector2(inset.x,inset.y); fillRect.offsetMax=new Vector2(inset.x+SkinnedFillWidth(Ratio,channelWidth,CapWidth(style.ProgressFillSprite)),-inset.w);
            fill.enabled=Ratio>0;
        }
        static float CapWidth(Sprite sprite) { var b=sprite.border; return (b.x+b.z)/sprite.pixelsPerUnit; }
        internal static float SkinnedFillWidth(float ratio,float channelWidth,float capWidth)
        {
            if(ratio<=0) return 0;
            float min=Mathf.Min(capWidth,channelWidth);
            return min+ratio*(channelWidth-min);
        }
    }
}

using GameKit.UI;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.LowLevel;
namespace IndieGame.Shell.Tests
{
    // Runtime-only demo assets: no imported art or TMP Essentials dependency.
    internal sealed class DemoStyles
    {
        readonly List<Object> owned = new List<Object>();
        T Own<T>(T o) where T : Object { owned.Add(o); return o; }
        public ShellStyle Make(bool bright)
        {
            var s = Own(ScriptableObject.CreateInstance<ShellStyle>()); s.name = bright ? "bright rounded" : "dark flat";
            s.Theme = Own(ScriptableObject.CreateInstance<UiTheme>());
            s.Theme.Font = Own(TMP_FontAsset.CreateFontAsset(Font.GetPathsToOSFonts().First(path => path.EndsWith("Arial.ttf")), 0, 32, 4, GlyphRenderMode.SDFAA, 512, 512));
            var tex = Own(new Texture2D(16,16)); var pixels = new Color[256];
            for (int y=0;y<16;y++) for(int x=0;x<16;x++)
                pixels[y*16+x] = bright && ((x<3 && y<3) || (x>12 && y<3) || (x<3 && y>12) || (x>12 && y>12)) ? Color.clear : Color.white;
            tex.SetPixels(pixels); tex.Apply();
            var sprite = Own(Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(.5f,.5f), 16,0,SpriteMeshType.FullRect,new Vector4(4,4,4,4)));
            var locked = Own(Sprite.Create(tex, new Rect(0,0,8,8),new Vector2(.5f,.5f)));
            s.ScreenBackground=s.CurrencyIcon=s.HeartIcon=s.BoosterIcon=s.ListRowSprite=sprite;
            s.LockedButtonSprite=locked; s.Theme.PanelSprite=s.Theme.Primary.Enabled=s.Theme.Secondary.Enabled=sprite;
            s.ScreenColor = bright ? new Color(.95f,.93f,.8f) : new Color(.04f,.04f,.08f);
            s.SurfaceColor = bright ? new Color(.3f,.8f,1) : new Color(.2f,.2f,.25f);
            s.TitleColor=s.ListRowTextColor=s.Theme.BodyColor = bright ? Color.black : Color.white;
            s.ListRowColor = bright ? Color.white : Color.gray;
            s.ProgressTrackColor = bright ? Color.gray : Color.black; s.ProgressFillColor = bright ? Color.blue : Color.green;
            s.Theme.Primary.LabelColor=s.Theme.Secondary.LabelColor=s.TitleColor;
            s.Theme.Motion.OpenSeconds=.04f; s.Theme.Motion.CloseSeconds=.04f; s.Theme.Motion.ScrimFadeSeconds=.03f;
            return s;
        }
        public void Dispose() { for(int i=owned.Count-1;i>=0;i--) if(owned[i]!=null) Object.DestroyImmediate(owned[i]); }
    }
}

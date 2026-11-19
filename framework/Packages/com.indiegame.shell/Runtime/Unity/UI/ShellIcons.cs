using System;
using System.Collections.Generic;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    // Runtime-only raster glyphs. Shared across styles, generated lazily, never written to disk.
    public static class ShellIcons
    {
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        public static Sprite Get(string key)
        {
            if(key=="gold") key="coin";
            if(cache.TryGetValue(key,out var cached)&&cached!=null) return cached;
            const int n=64; var pixels=new Color32[n*n];
            void Dot(int x,int y,int radius=2) { for(int j=-radius;j<=radius;j++) for(int i=-radius;i<=radius;i++)
                if(i*i+j*j<=radius*radius&&x+i>=0&&x+i<n&&y+j>=0&&y+j<n) pixels[(y+j)*n+x+i]=new Color32(255,255,255,255); }
            void Line(float x,float y,float xx,float yy) { int steps=Mathf.CeilToInt(Vector2.Distance(new Vector2(x,y),new Vector2(xx,yy))*2);
                for(int i=0;i<=steps;i++) { float t=steps==0?0:(float)i/steps; Dot(Mathf.RoundToInt(Mathf.Lerp(x,xx,t)),Mathf.RoundToInt(Mathf.Lerp(y,yy,t))); } }
            void Path(params int[] xy) { for(int i=2;i<xy.Length;i+=2) Line(xy[i-2],xy[i-1],xy[i],xy[i+1]); }
            void Circle(float x,float y,float radius,float from=0,float to=360) { for(float a=from;a<to;a+=2) {
                float b=Mathf.Min(a+2,to); Line(x+radius*Mathf.Cos(a*Mathf.Deg2Rad),y+radius*Mathf.Sin(a*Mathf.Deg2Rad),x+radius*Mathf.Cos(b*Mathf.Deg2Rad),y+radius*Mathf.Sin(b*Mathf.Deg2Rad)); } }
            switch(key) {
                case "trophy": Path(20,51,44,51,44,35,39,27,25,27,20,35,20,51);
                    Path(20,47,11,47,11,37,17,31,23,31); Path(44,47,53,47,53,37,47,31,41,31);
                    Line(32,27,32,14); Line(21,13,43,13); break;
                case "save": Path(13,12,51,12,51,44,43,52,13,52,13,12);
                    Path(22,52,22,36,42,36,42,52); Path(22,12,22,27,42,27,42,12); Line(35,42,35,48); break;
                case "globe": Circle(32,32,23); Line(9,32,55,32); Line(14,44,50,44); Line(14,20,50,20);
                    Path(32,55,24,44,21,32,24,20,32,9,40,20,43,32,40,44,32,55); break;
                case "edit": Path(14,14,18,30,44,56,56,44,30,18,14,14); Line(38,50,50,38); break;
                case "back": Path(33,49,16,32,33,15); Line(17,32,50,32); break;
                case "next": Path(30,49,47,32,30,15); Line(14,32,46,32); break;
                case "home": Path(10,31,32,51,54,31); Path(17,33,17,13,27,13,27,28,37,28,37,13,47,13,47,33); break;
                case "pause": Line(24,16,24,48); Line(40,16,40,48); break;
                case "play": Path(23,49,47,32,23,15,23,49); break;
                case "close": Line(17,17,47,47); Line(17,47,47,17); break;
                case "check": Path(13,31,26,18,51,46); break;
                case "retry": case "refresh": Circle(32,32,20,40,325); Path(38,54,48,47,51,59); break;
                case "video": Path(11,18,39,18,39,46,11,46,11,18); Path(39,37,53,45,53,19,39,27); break;
                case "coin": Circle(32,32,22); Circle(32,32,16); Line(32,22,32,42); break;
                case "gift": Path(12,33,52,33,52,16,12,16,12,33,9,33,9,42,55,42,55,33,12,33);
                    Line(32,16,32,43); Path(32,43,18,47,18,54,25,54,32,43,39,54,46,54,46,47,32,43); break;
                case "heart": Path(32,13,12,33,12,43,20,50,28,48,32,43,36,48,44,50,52,43,52,33,32,13); break;
                case "sound_on": case "sound_off": Path(10,25,21,25,34,14,34,50,21,39,10,39,10,25);
                    if(key.EndsWith("_on")) { Circle(32,32,15,-50,50); Circle(32,32,23,-50,50); } else { Line(43,25,55,39); Line(43,39,55,25); } break;
                case "music_on": case "music_off": Path(24,19,24,48,48,53,48,25); Circle(18,19,6); Circle(42,25,6);
                    if(key.EndsWith("_off")) Line(10,54,54,10); break;
                case "haptics_on": case "haptics_off": Path(23,12,41,12,41,52,23,52,23,12); Line(29,18,35,18);
                    if(key.EndsWith("_on")) { Path(14,19,10,25,14,32,10,39,14,46); Path(50,19,54,25,50,32,54,39,50,46); }
                    else Line(10,54,54,10); break;
                case "_round": case "_pill":
                    for(int y=0;y<n;y++) for(int x=0;x<n;x++) {
                        float dx=key=="_round"?x-31.5f:Mathf.Max(0,Mathf.Abs((x-31.5f)*2)-32);
                        if(dx*dx+(y-31.5f)*(y-31.5f)<31*31) pixels[y*n+x]=new Color32(255,255,255,255);
                    } break;
                default: throw new ArgumentException("Unknown shell icon: "+key);
            }
            var texture=new Texture2D(n,n,TextureFormat.RGBA32,false) { name="Shell glyph "+key,hideFlags=HideFlags.HideAndDontSave };
            texture.SetPixels32(pixels); texture.Apply();
            var sprite=Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),100);
            sprite.name=texture.name; sprite.hideFlags=HideFlags.HideAndDontSave; cache[key]=sprite; return sprite;
        }
    }
    public sealed class ActionAccessibility : MonoBehaviour
    {
        public string Key { get; private set; }
        public string Label { get; private set; }
        public void Set(string key,string label) { if(string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Action needs a localized name"); Key=key; Label=label; gameObject.name=label; }
    }
}

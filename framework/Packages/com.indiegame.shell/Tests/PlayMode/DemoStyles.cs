using GameKit.UI;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.LowLevel;
namespace IndieGame.Shell.Tests
{
    // Procedural baseline styles plus the imported T27 chunky slice trial.
    internal sealed class DemoStyles
    {
        UnityEngine.U2D.SpriteAtlas chunkyAtlas;
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
        public ShellStyle MakeChunky()
        {
            var s=Make(false); s.name="chunky";
#if UNITY_EDITOR
            chunkyAtlas=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>("Assets/Art/KitChunky/KitChunkyUi.spriteatlas");
            if(chunkyAtlas==null) throw new System.InvalidOperationException("Missing chunky atlas");
#endif
            s.EnableHomeChrome=true;
            s.TabBarPlainBackground=ChunkySprite("tab_bar_plain"); s.DayCellCollected=ChunkySprite("day_cell_1");
            s.DayCellToday=ChunkySprite("day_cell_2"); s.DayCellLocked=ChunkySprite("day_cell_3");
            s.BadgeSprite=ChunkySprite("badge_1"); s.PlainPillBackground=ChunkySprite("pill_plain");
            s.TabCellBackground=ChunkySprite("tab_cell"); s.TabCellSelectedBackground=ChunkySprite("tab_cell_selected");
            foreach(var key in new[]{"home","checkin","quests","shop","leaderboard"})
                s.TabIcons.Add(new ShellStyle.TabIconEntry{Key=key,Sprite=ChunkySprite(key=="leaderboard"?"tab_trophy":"tab_"+key)});
            s.Theme.PanelSprite=ChunkySprite("panel_popup");
            s.Theme.Primary.Enabled=ChunkySprite("btn_primary");
            s.Theme.Secondary.Enabled=ChunkySprite("btn_secondary");
            s.Theme.Primary.Locked=s.Theme.Secondary.Locked=s.LockedButtonSprite=ChunkySprite("btn_locked");
            s.ListRowSprite=ChunkySprite("list_row"); s.ScreenBackground=ChunkySprite("bg_play");
            s.CurrencyIcon=ChunkySprite("icons_1"); s.HeartIcon=ChunkySprite("icons_2"); s.BoosterIcon=ChunkySprite("icons_5");
            s.Theme.CloseSprite=ChunkySprite("btn_close"); s.Theme.ToggleOn=ChunkySprite("toggle_1"); s.Theme.ToggleOff=ChunkySprite("toggle_2");
            var plum=new Color32(0x30,0x0c,0x31,255); var cream=new Color32(0xfe,0xe9,0xd1,255);
            s.TabLabelColor=plum; s.TabUnselectedTint=new Color(.92f,.92f,.95f); s.SurfaceColor=s.ScreenColor=Color.white; s.TitleColor=Color.white; s.Theme.PanelSize=new Vector2(620f,400f);
            s.ListRowColor=cream; s.ListRowTextColor=plum; s.Theme.BodyColor=cream;
            s.ProgressTrackColor=new Color32(0xd9,0xd6,0xf0,255); s.ProgressFillColor=new Color32(0xd7,0x44,0x49,255);
            s.Theme.Primary.LabelColor=s.Theme.Secondary.LabelColor=Color.white;
            var outlined=Own(new Material(s.Theme.Font.material));
            outlined.SetColor("_OutlineColor",plum); outlined.SetFloat("_OutlineWidth",.045f); outlined.EnableKeyword("OUTLINE_ON");
            s.Theme.Primary.LabelMaterial=s.Theme.Secondary.LabelMaterial=outlined;
            s.Theme.TitleMaterial=Own(new Material(s.Theme.Font.material));
            s.Theme.TitleMaterial.SetColor("_OutlineColor",plum); s.Theme.TitleMaterial.SetFloat("_OutlineWidth",.06f); s.Theme.TitleMaterial.EnableKeyword("OUTLINE_ON");
            s.Validate(); return s;
        }
        Sprite ChunkySprite(string name)
        {
#if UNITY_EDITOR
            var single=name=="bg_home"||name=="bg_play"||name=="logo";
            var sprite=single?UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/KitChunky/"+name+".png"):Own(chunkyAtlas.GetSprite(name));
            if(sprite!=null&&!single) sprite.name=name;
            if(sprite==null) throw new System.InvalidOperationException("Missing chunky source: "+name);
            return sprite;
#else
            throw new System.InvalidOperationException("Chunky demo is an Editor screenshot fixture.");
#endif
        }
        public static void FitChunky(Transform root)
        {
            var plum=new Color32(0x30,0x0c,0x31,255);
            foreach(var binding in root.GetComponentsInChildren<ShellAppearance>(true))
            {
                var text=binding.GetComponent<TMP_Text>(); if(text==null) continue;
                if(binding.Role=="body"&&binding.GetComponentInParent<PopupView>()!=null) text.color=plum;
                if(binding.Role=="buttonText")
                {
                    // Leave a safe inset inside the source rim, including wrapped stress labels.
                    text.rectTransform.offsetMin=new Vector2(30,14); text.rectTransform.offsetMax=new Vector2(-30,-14);
                    text.fontStyle=FontStyles.Bold; text.fontSizeMin=20;
                }
            }
        }
        public void Dispose() { for(int i=owned.Count-1;i>=0;i--) if(owned[i]!=null) Object.DestroyImmediate(owned[i]); }
    }
}

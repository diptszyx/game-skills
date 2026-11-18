using System;
using System.Collections.Generic;
using GameKit.UI;
using UnityEngine;
namespace IndieGame.Shell.Unity.UI
{
    public enum ButtonRole { Primary, Secondary, Locked }
    [CreateAssetMenu(menuName = "IndieGame/Shell Style")]
    public sealed class ShellStyle : ScriptableObject
    {
        public bool EnableHomeChrome;
        public bool ShowScreenTitleWithChrome=true;
        // Bottom-tab screens open as popups over Home (default); false keeps flat full-screen pages with a Back button.
        public bool TabsAsModals=true;
        public bool ModalTabs=>EnableHomeChrome&&TabsAsModals;
        public bool EnableTabSwipe = true;
        // Mandatory popups disable close input even when the optional no-badge art is absent.
        public bool PanelHasBakedCloseIcon, ShowCloseHotspot=true;
        [Serializable] public sealed class PopupLayoutTheme
        {
            // Zero retains the original Theme.PanelSize height for existing themes.
            public float PanelMinHeight, PanelMaxHeight;
        }
        public PopupLayoutTheme PopupLayout=new PopupLayoutTheme();
        // Tab modals fit their rows; fractions are of the screen area above the tab bar.
        [Serializable] public sealed class ModalLayoutTheme
        {
            public float WidthFraction=.894f, MinHeightFraction=.30f, MaxHeightFraction=.84f;
        }
        public ModalLayoutTheme Modal=new ModalLayoutTheme();
        // Button press feedback: scale and colour multiplier while held, seconds to return.
        public float PressScale=.94f, PressDarken=.88f, PressSeconds=.12f;
        // Sprite pixel rows [top,bottom) of the visible header band; zero keeps the full top border as the title area.
        public Vector2 PanelHeaderRows, MandatoryPanelHeaderRows;
        public Vector2 PopupHeaderRows(bool mandatory)=>mandatory&&MandatoryPanelSprite!=null?MandatoryPanelHeaderRows:PanelHeaderRows;
        // Zero keeps the 64 unit close button at the legacy position; above zero draws a larger close button on the panel corner.
        public float CloseVisibleSize;
        public Rect PanelTextRegion=new Rect(.25f,.25f,.5f,.3f);
        public Rect TitleTextRegion=new Rect(.25f,.25f,.5f,.5f);
        public Rect ButtonTextRegion=new Rect(.25f,.35f,.5f,.3f);
        public Rect BackdropTextRegion=new Rect(.1f,.1f,.8f,.8f);
        public Sprite Logo, Mascot;
        public Sprite GearButton, BoardFrame, TabBarPlainBackground, DayCellCollected, DayCellToday, DayCellLocked, BadgeSprite, PlainPillBackground, TabCellBackground, TabCellSelectedBackground;
        [Serializable] public sealed class TabIconEntry { public string Key; public Sprite Sprite; }
        public Sprite RoundButtonSprite, PillButtonSprite;
        // ActionSurfaceColor tints fallback shapes; custom button sprites render white.
        public Color ActionSurfaceColor=new Color(.10f,.28f,.36f,1), ActionIconColor=Color.white;
        public List<TabIconEntry> Icons = new List<TabIconEntry>();
        public Sprite Icon(string key) { var entry=Icons.Find(e=>e!=null&&e.Key==key&&e.Sprite!=null); return entry!=null?entry.Sprite:ShellIcons.Get(key); }
        public List<TabIconEntry> TabIcons = new List<TabIconEntry>();
        public Color TabSelectedTint = Color.white, TabUnselectedTint = new Color(.7f,.7f,.8f); public Color TabLabelColor = Color.white;
        public Color ScreenTitleColorOverride = Color.clear;
        public Sprite TabIcon(string key)
        {
            var entry = TabIcons.Find(e => e != null && e.Key == key);
            return entry == null ? null : entry.Sprite;
        }
        // Optional override; PanelSprite belongs to the shared GameKit theme.
        public Sprite MandatoryPanelSprite;
        public UiTheme Theme;
        public Sprite ScreenBackground, CurrencyIcon, HeartIcon, BoosterIcon, ListRowSprite, LockedButtonSprite;
        public Color ScreenColor, TitleColor, ListRowColor, ListRowTextColor, ProgressTrackColor, ProgressFillColor;
        public Color SurfaceColor = Color.white;
        public const float MinTextContrast = 4.5f;
        public const float LargeTextContrast=3f;
        public Color PanelColor = Color.clear, PanelTextColor = Color.clear, BackdropColor = Color.clear, BackdropTextColor = Color.clear;
        public static float Contrast(Color a, Color b)
        {
            var la = Luminance(a); var lb = Luminance(b);
            return (Mathf.Max(la, lb) + .05f) / (Mathf.Min(la, lb) + .05f);
        }
        static float Luminance(Color c) => .2126f * Mathf.GammaToLinearSpace(c.r) + .7152f * Mathf.GammaToLinearSpace(c.g) + .0722f * Mathf.GammaToLinearSpace(c.b);
        void RequireContrast(Color text, Color surface, string pair)
        {
            if (text.a <= 0 && surface.a <= 0) return;
            if (text.a <= 0 || surface.a <= 0) throw new InvalidOperationException("Style pair needs both colours: " + pair);
            var ratio = Contrast(text, surface);
            if (ratio < MinTextContrast) throw new InvalidOperationException("Style text contrast " + ratio.ToString("0.0") + " < " + MinTextContrast + ": " + pair);
        }
        static Color Over(Color foreground,Color background)
        { return new Color(foreground.r*foreground.a+background.r*(1-foreground.a),foreground.g*foreground.a+background.g*(1-foreground.a),foreground.b*foreground.a+background.b*(1-foreground.a),1); }
        void RequireSpriteContrast(Color text,Sprite sprite,Color tint,Rect region,string role)
        {
            float minimum=MinimumSpriteContrast(text,sprite,tint,region);
            float required=role=="title"?LargeTextContrast:MinTextContrast;
            if(minimum<required) throw new InvalidOperationException("Style rendered text contrast "+minimum.ToString("0.00")+" < "+required+": "+role);
        }
        public Color BestTextColor(Color preferred,Color alternative,Sprite sprite,Color tint,Rect region) =>
            MinimumSpriteContrast(alternative,sprite,tint,region)>MinimumSpriteContrast(preferred,sprite,tint,region)?alternative:preferred;
        float MinimumSpriteContrast(Color text,Sprite sprite,Color tint,Rect region)
        {
            if(sprite==null) return float.PositiveInfinity;
            // UV bounds belong to this sprite, including atlas packing (not the whole atlas).
            var vertices=sprite.vertices; var uv=sprite.uv;
            var bounds=sprite.bounds;
            Vector2 Map(Vector2 point)
            {
                for(int i=0;i<vertices.Length-2;i++) for(int j=i+1;j<vertices.Length-1;j++) for(int k=j+1;k<vertices.Length;k++)
                {
                    Vector2 a=vertices[j]-vertices[i],b=vertices[k]-vertices[i]; float det=a.x*b.y-a.y*b.x;
                    if(Mathf.Abs(det)<.000001f) continue;
                    Vector2 local=new Vector2(bounds.min.x+point.x*bounds.size.x,bounds.min.y+point.y*bounds.size.y)-vertices[i];
                    float u=(local.x*b.y-local.y*b.x)/det,v=(a.x*local.y-a.y*local.x)/det;
                    return uv[i]+u*(uv[j]-uv[i])+v*(uv[k]-uv[i]);
                }
                throw new InvalidOperationException("Cannot sample sprite: "+sprite.name);
            }
            // GPU copy works for non-readable imported textures and packed sprites too.
            var previous=RenderTexture.active;
            var target=RenderTexture.GetTemporary(9,5,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var pixels=new Texture2D(9,5,TextureFormat.RGBA32,false);
            float minimum=float.PositiveInfinity;
            try
            {
                var a=Map(region.min); var b=Map(region.max);
                var min=Vector2.Min(a,b); var max=Vector2.Max(a,b);
                Graphics.Blit(sprite.texture,target,max-min,min); RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,9,5),0,0,false);
                foreach(var pixel in pixels.GetPixels())
                {
                    var surface=Over(pixel*tint,ScreenColor);
                    minimum=Mathf.Min(minimum,Contrast(Over(text,surface),surface));
                }
            }
            finally
            {
                RenderTexture.active=previous; RenderTexture.ReleaseTemporary(target);
                if(Application.isPlaying) Destroy(pixels); else DestroyImmediate(pixels);
            }
            return minimum;
        }
        void ValidateRenderedContrast()
        {
            RequireSpriteContrast(PanelTextColor.a>0?PanelTextColor:Theme.BodyColor,Theme.PanelSprite,SurfaceColor,PanelTextRegion,"popupText");
            RequireSpriteContrast(BackdropTextColor.a>0?BackdropTextColor:Theme.BodyColor,ScreenBackground,ScreenColor,BackdropTextRegion,"backdropText");
            RequireSpriteContrast(TitleColor,Theme.PanelSprite,SurfaceColor,TitleTextRegion,"title");
            RequireSpriteContrast(Theme.BodyColor,Theme.PanelSprite,SurfaceColor,PanelTextRegion,"body");
            RequireSpriteContrast(Theme.Primary.LabelColor,Theme.Primary.Enabled,SurfaceColor,ButtonTextRegion,"buttonText primary");
            RequireSpriteContrast(Theme.Secondary.LabelColor,Theme.Secondary.Enabled,SurfaceColor,ButtonTextRegion,"buttonText secondary");
        }
        public float Spacing = 12;
        public T Require<T>(T value, string role) where T : UnityEngine.Object
        { if (value == null) throw new InvalidOperationException("Missing style role: " + role); return value; }
        static void RequireButtonBorderFit(Sprite sprite, Vector2 size, string role)
        {
            // Sprite borders are source pixels; shell dimensions are UI units.
            var border=sprite.border / sprite.pixelsPerUnit;
            if(size.x < border.x+border.z || size.y < border.y+border.w)
                throw new InvalidOperationException(role+" border does not fit shell layout "+size+" in UI units");
        }
        void ValidateButtonBorders(Sprite sprite, string role)
        {
            RequireButtonBorderFit(sprite,ShellLayout.Standard.ButtonStandard,role+" Standard");
            RequireButtonBorderFit(sprite,ShellLayout.Standard.PopupAction,role+" PopupAction");
        }
        public void Validate()
        {
            RequireContrast(PanelTextColor, PanelColor, "PanelTextColor on PanelColor");
            RequireContrast(BackdropTextColor, BackdropColor, "BackdropTextColor on BackdropColor");
            RequireContrast(ListRowTextColor, ListRowColor, "ListRowTextColor on ListRowColor");
            if (EnableHomeChrome)
            {
                Require(TabBarPlainBackground, nameof(TabBarPlainBackground)); Require(DayCellCollected, nameof(DayCellCollected));
                Require(DayCellToday, nameof(DayCellToday)); Require(DayCellLocked, nameof(DayCellLocked));
                Require(BadgeSprite, nameof(BadgeSprite)); Require(GearButton, nameof(GearButton)); Require(BoardFrame, nameof(BoardFrame)); Require(PlainPillBackground, nameof(PlainPillBackground));
                Require(TabCellBackground, nameof(TabCellBackground)); Require(TabCellSelectedBackground, nameof(TabCellSelectedBackground));
                foreach (var key in new[] { "home", "checkin", "quests", "shop", "leaderboard" }) Require(TabIcon(key), "TabIcons." + key);
            }
            if(Modal.WidthFraction<=0||Modal.WidthFraction>1||Modal.MinHeightFraction<=0||Modal.MaxHeightFraction>1||Modal.MinHeightFraction>Modal.MaxHeightFraction)
                throw new InvalidOperationException("Invalid Modal layout: width (0,1], 0 < min <= max <= 1");
            if(PressScale<=0||PressScale>1||PressDarken<=0||PressDarken>1||PressSeconds<=0)
                throw new InvalidOperationException("Invalid press feedback: PressScale and PressDarken in (0,1], PressSeconds > 0");
            Require(Theme, "Theme"); Require(Theme.Font, "Theme.Font");
            Require(ScreenBackground, "ScreenBackground"); Require(CurrencyIcon, "CurrencyIcon");
            Require(HeartIcon, "HeartIcon"); Require(BoosterIcon, "BoosterIcon");
            Require(ListRowSprite, "ListRowSprite"); Require(LockedButtonSprite, "LockedButtonSprite");
            Require(Theme.PanelSprite, "Theme.PanelSprite");
            if (MandatoryPanelSprite != null && MandatoryPanelSprite.rect.size != Theme.PanelSprite.rect.size)
                throw new InvalidOperationException("MandatoryPanelSprite pixel size must equal Theme.PanelSprite pixel size");
            if (Theme.Primary == null) throw new InvalidOperationException("Missing style role: Theme.Primary");
            if (Theme.Secondary == null) throw new InvalidOperationException("Missing style role: Theme.Secondary");
            Require(Theme.Primary.Enabled, "Theme.Primary.Enabled"); Require(Theme.Secondary.Enabled, "Theme.Secondary.Enabled");
            if (Theme.Motion == null) throw new InvalidOperationException("Missing style role: Theme.Motion");
            ValidateButtonBorders(Theme.Primary.Enabled,"Primary");
            ValidateButtonBorders(Theme.Secondary.Enabled,"Secondary");
            ValidateButtonBorders(LockedButtonSprite,"Locked");
            foreach(var sprite in new[]{RoundButtonSprite,PillButtonSprite})
                if(sprite!=null && (sprite.texture==null || sprite.rect.width<=0 || sprite.rect.height<=0 || sprite.vertices.Length<3))
                    throw new InvalidOperationException("Invalid icon button sprite: "+sprite.name);
            if(PillButtonSprite!=null) RequireButtonBorderFit(PillButtonSprite,ShellLayout.Standard.IconAction,"PillButtonSprite");
            ValidateRenderedContrast();
        }
    }
}

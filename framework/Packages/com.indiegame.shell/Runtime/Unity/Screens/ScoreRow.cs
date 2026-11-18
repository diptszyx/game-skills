using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class ScoreColumns
    {
        public string Rank { get; }
        public string Name { get; }
        public string Score { get; }
        public int Place { get; }
        public bool Own { get; }
        public bool Divider { get; }
        public ScoreColumns(string rank,string name,string score,int place,bool own=false,bool divider=false)
        { Rank=rank; Name=name; Score=score; Place=place; Own=own; Divider=divider; }
    }
    internal static class ScoreRow
    {
        internal const float Inset=16, RankWidth=72, ColumnGap=8, ScoreWidth=200, IconSize=36, IconGap=8, FontSize=32, DividerHeight=2, DividerAlpha=.25f;
        internal const string ScoreIconKey="scoreIcon";
        internal static void Fill(ShellUiFactory f,RectTransform row,TMP_Text name,ScoreColumns c)
        {
            var style=f.Style;
            if(c.Own) { var binding=row.GetComponent<ShellAppearance>(); binding.Role="ownRow"; binding.Apply(style); }
            ScreenWidgets.OpaqueCard(row.GetComponent<Image>());
            var ink=c.Own?style.LeaderboardOwnRowTextColor:style.ListRowTextColor;
            bool podium=!c.Own&&c.Place>=1&&c.Place<=3;
            var rank=f.Label(row,c.Rank,role:"rowText"); rank.gameObject.name="Rank";
            Style(rank,TextAlignmentOptions.Center,podium?PodiumColor(style,c.Place):ink,podium||c.Own);
            PinColumn(rank.rectTransform,0,Inset,Inset+RankWidth);
            name.gameObject.name="Name"; name.text=c.Name; Style(name,TextAlignmentOptions.MidlineLeft,ink,c.Own);
            var nameRect=name.rectTransform; nameRect.anchorMin=Vector2.zero; nameRect.anchorMax=Vector2.one;
            nameRect.offsetMin=new Vector2(Inset+RankWidth+ColumnGap,0); nameRect.offsetMax=new Vector2(-(Inset+ScoreWidth+ColumnGap),0);
            AddScore(f,row,c,ink);
            if(c.Divider&&!c.Own) AddDivider(row,ink);
        }
        static Color PodiumColor(ShellStyle style,int place) =>
            place==1?style.LeaderboardGoldColor:place==2?style.LeaderboardSilverColor:style.LeaderboardBronzeColor;
        static void Style(TMP_Text label,TextAlignmentOptions alignment,Color color,bool bold)
        {
            label.alignment=alignment; label.color=color; label.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
            label.enableAutoSizing=false; label.fontSize=FontSize;
            label.textWrappingMode=TextWrappingModes.NoWrap; label.overflowMode=TextOverflowModes.Ellipsis;
        }
        static void PinColumn(RectTransform rect,float anchorX,float near,float far)
        {
            rect.anchorMin=new Vector2(anchorX,0); rect.anchorMax=new Vector2(anchorX,1);
            rect.offsetMin=new Vector2(near,0); rect.offsetMax=new Vector2(far,0);
        }
        static void AddScore(ShellUiFactory f,RectTransform row,ScoreColumns c,Color ink)
        {
            bool hasIcon=f.Style.StyleIcon(ScoreIconKey)!=null;
            float numberWidth=hasIcon?ScoreWidth-IconSize-IconGap:ScoreWidth;
            var score=f.Label(row,c.Score,role:"rowText"); score.gameObject.name="Score";
            Style(score,TextAlignmentOptions.MidlineRight,ink,true);
            PinColumn(score.rectTransform,1,-(Inset+numberWidth),-Inset);
            if(!hasIcon) return;
            var icon=f.AppearanceImage(row,"Score icon","scoreIcon",iconKey:ScoreIconKey);
            float textWidth=Mathf.Min(score.GetPreferredValues(c.Score).x,numberWidth);
            var rect=icon.rectTransform; rect.anchorMin=rect.anchorMax=new Vector2(1,.5f); rect.pivot=new Vector2(1,.5f);
            rect.sizeDelta=new Vector2(IconSize,IconSize); rect.anchoredPosition=new Vector2(-(Inset+textWidth+IconGap),0);
        }
        static void AddDivider(RectTransform row,Color ink)
        {
            var line=ShellUiFactory.Node("Divider",row); line.anchorMin=Vector2.zero; line.anchorMax=new Vector2(1,0); line.pivot=new Vector2(.5f,0);
            line.offsetMin=new Vector2(Inset,0); line.offsetMax=new Vector2(-Inset,DividerHeight);
            var image=line.gameObject.AddComponent<Image>(); image.color=new Color(ink.r,ink.g,ink.b,DividerAlpha); image.raycastTarget=false;
            line.gameObject.SetActive(true);
        }
    }
}

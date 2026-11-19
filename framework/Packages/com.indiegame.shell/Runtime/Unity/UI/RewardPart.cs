using System.Globalization;
namespace IndieGame.Shell.Unity.UI
{
    // One piece of a reward: an icon role plus an amount, or plain text when the piece has no icon.
    public sealed class RewardPart
    {
        public const string GoldIcon="currencyIcon", HeartIcon="heartIcon";
        public string Icon { get; }
        public string Text { get; }
        RewardPart(string icon,string text) { Icon=icon; Text=text; }
        public static RewardPart Gold(long amount)=>new RewardPart(GoldIcon,amount.ToString(CultureInfo.InvariantCulture));
        public static RewardPart Hearts(int amount)=>new RewardPart(HeartIcon,amount.ToString(CultureInfo.InvariantCulture));
        public static RewardPart Plain(string text)=>new RewardPart(null,text);
    }
}

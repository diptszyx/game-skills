using System.Globalization;
namespace IndieGame.Shell.Unity.UI
{
    // One piece of a reward: an icon role plus an amount, or plain text when the piece has no icon.
    public sealed class RewardPart
    {
        public const string GoldIcon="currencyIcon", HeartIcon="heartIcon";
        public string Icon { get; }
        public string Text { get; }
        public string BoosterId { get; }
        public string AmountText { get; }
        public long Amount { get; }
        RewardPart(string icon,string text,string boosterId=null,string amountText=null,long amount=0) { Icon=icon; Text=text; BoosterId=boosterId; AmountText=amountText; Amount=amount; }
        public static RewardPart Gold(long amount)=>new RewardPart(GoldIcon,amount.ToString(CultureInfo.InvariantCulture),amount:amount);
        public static RewardPart Hearts(int amount)=>new RewardPart(HeartIcon,amount.ToString(CultureInfo.InvariantCulture),amount:amount);
        // Text is the plain fallback shown when the factory has no icon for the booster.
        public static RewardPart Booster(string id,int amount,string text)=>new RewardPart(null,text,id,amount.ToString(CultureInfo.InvariantCulture));
        public static RewardPart Plain(string text)=>new RewardPart(null,text);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace IndieGame.Shell
{
    public static class ShopPack
    {
        public const string Hearts = "hearts";
        public const string GoldSmall = "gold_s";
        public const string GoldMedium = "gold_m";
        public const string GoldLarge = "gold_l";

        public const long GoldSmallPrice = 99;
        public const long GoldMediumPrice = 499;
        public const long GoldLargePrice = 999;
        public const int GoldMediumMultiplier = 5;
        public const int GoldLargeMultiplier = 12;

        public const string BoosterPrefix = "booster_";

        public static ProductCatalog Standard(int shopGold, long heartsPrice, int maxHearts, IReadOnlyList<string> boosterIds = null, long boosterPrice = 0)
        {
            if (shopGold < 1) throw new ArgumentOutOfRangeException(nameof(shopGold));
            if (heartsPrice < 1) throw new ArgumentOutOfRangeException(nameof(heartsPrice));
            if (maxHearts < 1) throw new ArgumentOutOfRangeException(nameof(maxHearts));
            var products = new List<Product>
            {
                new Product(Hearts, PriceKind.Gold, heartsPrice, new Reward(hearts: maxHearts)),
                new Product(GoldSmall, PriceKind.Iap, GoldSmallPrice, new Reward(shopGold)),
                new Product(GoldMedium, PriceKind.Iap, GoldMediumPrice, new Reward((long)shopGold * GoldMediumMultiplier)),
                new Product(GoldLarge, PriceKind.Iap, GoldLargePrice, new Reward((long)shopGold * GoldLargeMultiplier)),
            };
            if (boosterIds != null)
            {
                if (boosterIds.Count > 0 && boosterPrice < 1) throw new ArgumentOutOfRangeException(nameof(boosterPrice));
                foreach (var id in boosterIds) products.Add(new Product(BoosterPrefix + id, PriceKind.Gold, boosterPrice, RewardBoosters.One(id)));
            }
            return new ProductCatalog(products.OrderBy(p => p.PriceKind).ThenBy(p => p.Price));
        }
    }
}

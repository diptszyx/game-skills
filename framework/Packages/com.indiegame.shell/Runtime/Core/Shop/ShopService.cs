using System;
using System.Collections.Generic;

namespace IndieGame.Shell
{
    public enum PriceKind { Gold, Iap }
    public sealed class Product
    {
        public string Id { get; }
        public string TitleKey { get; }
        public PriceKind PriceKind { get; }
        public long Price { get; }
        public Reward Reward { get; }
        public Product(string id, PriceKind priceKind, long price, Reward reward, string? titleKey = null)
        {
            if (string.IsNullOrWhiteSpace(id) || !Enum.IsDefined(typeof(PriceKind), priceKind) || price < 0 ||
                (priceKind == PriceKind.Gold && price == 0) || DailyRules.Copy(reward) == null)
                throw new ArgumentException("Invalid product configuration.");
            Id = id; PriceKind = priceKind; Price = price; Reward = DailyRules.Copy(reward)!;
            TitleKey = titleKey ?? "shell.product." + id;
        }
    }
    public sealed class ProductCatalog
    {
        readonly Dictionary<string, Product> byId = new Dictionary<string, Product>(StringComparer.Ordinal);
        public IReadOnlyList<Product> Products { get; }
        public ProductCatalog(IEnumerable<Product> products)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));
            var list = new List<Product>();
            foreach (var p in products)
            { if (p == null || byId.ContainsKey(p.Id)) throw new ArgumentException("Duplicate or null product."); byId.Add(p.Id, p); list.Add(p); }
            Products = list.AsReadOnly();
        }
        public Product? Find(string id) => id != null && byId.TryGetValue(id, out var p) ? p : null;
    }
    public enum ShopError { None, NotFound, InvalidArgument, InsufficientGold, GrantRejected, PurchaseFailed, Cancelled }
    public sealed class ShopResult
    {
        public ShopError Error { get; }
        public bool Changed { get; }
        public bool Success => Error == ShopError.None;
        public ShopResult(ShopError error = ShopError.None, bool changed = false) { Error = error; Changed = changed; }
    }
    // Single-writer, synchronous boundary matching the existing service sockets.
    public sealed class ShopService
    {
        public ProductCatalog Catalog { get; }
        readonly EconomyService economy;
        readonly LivesService lives;
        readonly IPurchaseService purchases;
        readonly IRewardGrantor grantor;
        public ShopService(ProductCatalog catalog, ShellSession session, IPurchaseService purchases)
            : this(catalog, session.Economy, session.Lives, purchases, session.Economy) { }
        public ShopService(ProductCatalog catalog, EconomyService economy, LivesService lives, IPurchaseService purchases, IRewardGrantor grantor)
        { Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); this.economy = economy ?? throw new ArgumentNullException(nameof(economy));
          this.lives = lives ?? throw new ArgumentNullException(nameof(lives)); this.purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
          this.grantor = grantor ?? throw new ArgumentNullException(nameof(grantor)); }
        public ShopResult Buy(string productId, string transactionId)
        {
            var product = Catalog.Find(productId);
            if (product == null) return new ShopResult(ShopError.NotFound);
            if (string.IsNullOrWhiteSpace(transactionId)) return new ShopResult(ShopError.InvalidArgument);
            if (product.PriceKind == PriceKind.Iap)
            {
                if (economy.Claims.Contains("iap-attempt:" + transactionId)) return new ShopResult();
                var receipt = purchases.Purchase(product.Id);
                if (!receipt.Success) return new ShopResult(receipt.Outcome == PurchaseOutcome.Cancelled ? ShopError.Cancelled : ShopError.PurchaseFailed);
                if (string.IsNullOrWhiteSpace(receipt.TransactionId) || receipt.ProductId != product.Id) return new ShopResult(ShopError.PurchaseFailed);
                // The provider's verified receipt is authoritative, even if the UI attempt id differs.
                var outcome = grantor.Grant("iap:" + receipt.TransactionId, product.Reward);
                if (outcome != GrantOutcome.Rejected) economy.GrantReward("iap-attempt:" + transactionId, new Reward(0));
                return new ShopResult(outcome == GrantOutcome.Rejected ? ShopError.GrantRejected : ShopError.None, outcome == GrantOutcome.Granted);
            }
            var claim = "shop:" + transactionId;
            if (economy.Claims.Contains(claim)) return new ShopResult();
            var before = new SaveDocument(); economy.Save(before); lives.Save(before);
            if (!economy.SpendGold(product.Price).Success) return new ShopResult(ShopError.InsufficientGold);
            var granted = economy.GrantReward(claim, product.Reward);
            if (!granted.Success)
            { economy.Load(before); lives.Load(before); return new ShopResult(ShopError.GrantRejected); }
            return new ShopResult(changed: true);
        }
    }
}

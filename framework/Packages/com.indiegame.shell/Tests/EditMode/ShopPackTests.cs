using System.Linq;
using NUnit.Framework;

namespace IndieGame.Shell.Tests
{
    public sealed class ShopPackTests
    {
        static ProductCatalog Pack() => ShopPack.Standard(50, 30, 5);

        [Test] public void StandardPackHasUniqueIdsAndPositivePrices()
        {
            var products = Pack().Products;
            Assert.That(products.Select(p => p.Id).Distinct().Count(), Is.EqualTo(products.Count));
            Assert.That(products.All(p => p.Price > 0), Is.True);
        }

        [Test] public void StandardPackRewardsAreValidAndNeverBuyGoldWithGold()
        {
            var products = Pack().Products;
            Assert.That(products.All(p => p.Reward.Gold > 0 || p.Reward.Hearts > 0), Is.True);
            Assert.That(products.Where(p => p.PriceKind == PriceKind.Gold).All(p => p.Reward.Gold == 0), Is.True);
        }

        [Test] public void StandardPackIsSortedByPriceKindThenPrice()
        {
            var products = Pack().Products;
            var sorted = products.OrderBy(p => p.PriceKind).ThenBy(p => p.Price).Select(p => p.Id);
            Assert.That(products.Select(p => p.Id), Is.EqualTo(sorted));
        }

        [Test] public void StandardPackScalesIapGoldWithPrice()
        {
            var iap = Pack().Products.Where(p => p.PriceKind == PriceKind.Iap).ToList();
            for (int i = 1; i < iap.Count; i++) Assert.That(iap[i].Reward.Gold, Is.GreaterThan(iap[i - 1].Reward.Gold));
        }

        [Test] public void StandardPackIsAcceptedByTheServiceAndRefillsHearts()
        {
            var session = ScreenTestData.Session(new ScreenClock(), new ScreenFiles());
            session.LoadAll();
            var fake = new FakePurchaseService();
            var service = new ShopService(Pack(), session, fake);
            Assert.That(service.Buy(ShopPack.Hearts, "t1").Success, Is.True);
            Assert.That(session.Economy.Hearts, Is.EqualTo(5));
            Assert.That(session.Economy.Gold, Is.EqualTo(70));
        }

        [Test] public void SaveFromAnOlderCatalogStillBootsWithTheStandardPack()
        {
            var clock = new ScreenClock(); var files = new ScreenFiles();
            var old = ScreenTestData.Session(clock, files); old.LoadAll();
            var oldCatalog = new ProductCatalog(new[] { new Product("p", PriceKind.Gold, 10, new Reward(5)) });
            Assert.That(new ShopService(oldCatalog, old, new FakePurchaseService()).Buy("p", "old").Success, Is.True);
            Assert.That(old.Commit().Success, Is.True);
            var current = ScreenTestData.Session(clock, files);
            Assert.That(current.LoadAll().Success, Is.True);
            Assert.That(new ShopService(Pack(), current, new FakePurchaseService()).Buy(ShopPack.Hearts, "new").Success, Is.True);
        }

        [Test] public void StandardPackRejectsNonPositiveInputs()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ShopPack.Standard(0, 30, 5));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ShopPack.Standard(50, 0, 5));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ShopPack.Standard(50, 30, 0));
        }
        [Test] public void EachBoosterIdBecomesAGoldProductAtTheBoosterPrice()
        {
            var catalog = ShopPack.Standard(50, 30, 5, new[] { "a", "b" }, 100);
            var product = catalog.Products.Single(p => p.Id == "booster_a");
            Assert.That(product.PriceKind, Is.EqualTo(PriceKind.Gold)); Assert.That(product.Price, Is.EqualTo(100));
            Assert.That(product.Reward.Boosters["a"], Is.EqualTo(1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ShopPack.Standard(50, 30, 5, new[] { "a" }, 0));
        }
    }
}

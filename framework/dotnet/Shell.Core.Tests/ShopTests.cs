using System;
using System.Collections.Generic;
using IndieGame.Shell;
using Xunit;
namespace IndieGame.Shell.Tests;
public sealed class ShopTests
{
    readonly DailyFakeClock clock = new();
    readonly FakePurchaseService purchases = new();
    readonly LivesService lives;
    readonly EconomyService economy;
    public ShopTests() { lives=new LivesService(new LivesConfig(5,1,TimeSpan.FromMinutes(30)),clock); economy=new EconomyService(new EconomyConfig(100),lives); }
    ShopService Shop(Reward? reward=null, long price=20) => new(new ProductCatalog(new[] {
        new Product("gold",PriceKind.Gold,price,reward??new Reward(0,1)),new Product("iap",PriceKind.Iap,0,new Reward(50,1)) }),economy,lives,purchases,economy);
    [Fact] public void GoldPurchaseAtomic()
    {
        var s=Shop(); Assert.True(s.Buy("gold","a").Success); Assert.Equal(80,economy.Gold); Assert.Equal(2,lives.Hearts);
        Assert.True(s.Buy("gold","a").Success); Assert.Equal(80,economy.Gold);
        economy.GrantReward("seed",new Reward(0,0,new Dictionary<string,int>{{"boost",int.MaxValue}}));
        var rejected=Shop(new Reward(0,1,new Dictionary<string,int>{{"boost",1}})).Buy("gold","b");
        Assert.Equal(ShopError.GrantRejected,rejected.Error); Assert.Equal(80,economy.Gold); Assert.Equal(2,lives.Hearts); Assert.False(economy.Claims.Contains("shop:b"));
    }
    [Fact] public void InsufficientGoldRejected() { Assert.Equal(ShopError.InsufficientGold,Shop(price:101).Buy("gold","a").Error); Assert.Equal(100,economy.Gold); Assert.Equal(1,lives.Hearts); }
    [Fact] public void IapSuccessGrantsOnce()
    {
        purchases.Enqueue(PurchaseOutcome.Success,"receipt"); purchases.Enqueue(PurchaseOutcome.Success,"different-receipt"); var s=Shop();
        Assert.True(s.Buy("iap","receipt").Changed); Assert.False(s.Buy("iap","receipt").Changed);
        Assert.Equal(150,economy.Gold); Assert.Equal(2,lives.Hearts); Assert.True(economy.Claims.Contains("iap:receipt"));
    }
    [Fact] public void RepeatedReceiptGrantsOnce()
    {
        purchases.Enqueue(PurchaseOutcome.Success,"receipt"); purchases.Enqueue(PurchaseOutcome.Success,"receipt");
        Assert.True(Shop().Buy("iap","attempt1").Success);
        var saved=new SaveDocument(); economy.Save(saved); lives.Save(saved);
        var restoredLives=new LivesService(new LivesConfig(5,1,TimeSpan.FromMinutes(30)),clock); restoredLives.Load(saved);
        var restoredEconomy=new EconomyService(new EconomyConfig(100),restoredLives); restoredEconomy.Load(saved);
        var s=new ShopService(Shop().Catalog,restoredEconomy,restoredLives,purchases,restoredEconomy);
        Assert.False(s.Buy("iap","attempt2").Changed); Assert.Equal(150,restoredEconomy.Gold); Assert.Equal(2,restoredLives.Hearts);
    }
    [Fact] public void FailedPurchaseLeavesBalanceUnchanged() { purchases.Enqueue(PurchaseOutcome.Failed); Assert.Equal(ShopError.PurchaseFailed,Shop().Buy("iap","a").Error); Assert.Equal(100,economy.Gold); Assert.Equal(1,lives.Hearts); }
    [Fact] public void CancelledPurchaseLeavesBalanceUnchanged() { purchases.Enqueue(PurchaseOutcome.Cancelled); Assert.Equal(ShopError.Cancelled,Shop().Buy("iap","a").Error); Assert.Equal(100,economy.Gold); Assert.Equal(1,lives.Hearts); }
    [Fact] public void UnknownProductNotFound() { Assert.Equal(ShopError.NotFound,Shop().Buy("missing","a").Error); Assert.Equal(100,economy.Gold); }
}

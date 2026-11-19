using System.Linq;
using NUnit.Framework;

namespace IndieGame.Shell.Tests
{
    public sealed class CheckInPackTests
    {
        sealed class CountingGrantor : IRewardGrantor
        { public GrantOutcome Grant(string claimId, Reward reward) => GrantOutcome.Granted; }

        [Test] public void StandardPackHasSevenPositiveEscalatingRewardsWithTheBiggestOnDaySeven()
        {
            foreach (var row in CheckInPack.Standard(10).RewardTables)
            {
                Assert.That(row.Count, Is.EqualTo(CheckInPack.Days));
                Assert.That(row.All(r => r.Gold > 0), Is.True);
                for (int i = 1; i < row.Count; i++) Assert.That(row[i].Gold, Is.GreaterThan(row[i - 1].Gold));
                Assert.That(row.Last().Gold, Is.EqualTo(row.Max(r => r.Gold)));
                Assert.That(row.Last().Hearts, Is.GreaterThan(0));
                Assert.That(row.Take(CheckInPack.Days - 1).All(r => r.Hearts == 0), Is.True);
            }
        }

        [Test] public void RewardAtSlotReportsEverySlotAndNullOutsideTheCycle()
        {
            var service = new DailyCheckInService(CheckInPack.Standard(10), new ScreenClock(), new CountingGrantor());
            for (int slot = 0; slot < CheckInPack.Days; slot++) Assert.That(service.RewardAtSlot(slot).Gold, Is.GreaterThan(0), "slot " + slot);
            Assert.That(service.RewardAtSlot(-1), Is.Null); Assert.That(service.RewardAtSlot(CheckInPack.Days), Is.Null);
        }
        [Test] public void StandardPackRejectsNegativeGold()
        { Assert.Throws<System.ArgumentOutOfRangeException>(() => CheckInPack.Standard(-1)); }

        [Test] public void StandardPackIsAcceptedByTheServiceAndWalksSevenDays()
        {
            var clock = new ScreenClock();
            var service = new DailyCheckInService(CheckInPack.Standard(10), clock, new CountingGrantor());
            for (int day = 0; day < CheckInPack.Days; day++)
            {
                Assert.That(service.Preview().NextSlot, Is.EqualTo(day));
                Assert.That(service.Claim().Success, Is.True);
                clock.UtcNow = clock.UtcNow.AddDays(1);
            }
            Assert.That(service.Preview().NextSlot, Is.EqualTo(0), "cycle wraps after day 7");
        }

        [Test] public void ServiceRejectsAFiveDayConfig()
        {
            var five = new DailyCheckInConfig(new[] { Enumerable.Repeat(new Reward(1), 5).ToArray(), Enumerable.Repeat(new Reward(1), 5).ToArray() });
            Assert.That(new DailyCheckInService(five, new ScreenClock(), new CountingGrantor()).Preview().Error, Is.EqualTo(DailyError.InvalidConfig));
        }

        static SaveDocument DocumentWithSlot(int slot)
        {
            var document = new SaveDocument();
            int yesterday = (int)(new ScreenClock().UtcNow.Date.Ticks / System.TimeSpan.TicksPerDay) - 1;
            document.WriteSection("dailyCheckIn", new DailyCheckInService.State { Version = 1, Day = yesterday, ClaimedDay = yesterday, NextSlot = slot });
            return document;
        }

        [Test] public void SaveFromTheOldFiveDayCycleStillLoadsAndContinues()
        {
            var service = new DailyCheckInService(CheckInPack.Standard(10), new ScreenClock(), new CountingGrantor());
            Assert.That(service.Load(DocumentWithSlot(3)).Success, Is.True);
            Assert.That(service.NextSlot, Is.EqualTo(3)); Assert.That(service.Preview().CanClaimNow, Is.True);
        }

        [Test] public void SaveWithASlotBeyondTheCycleResetsInsteadOfFailingBoot()
        {
            var service = new DailyCheckInService(CheckInPack.Standard(10), new ScreenClock(), new CountingGrantor());
            Assert.That(service.Load(DocumentWithSlot(CheckInPack.Days + 2)).Success, Is.True);
            Assert.That(service.NextSlot, Is.EqualTo(0)); Assert.That(service.Preview().CanClaimNow, Is.True);
        }
        [Test] public void BoosterIdsAddBoostersOnDaysThreeFiveAndSeven()
        {
            var row = CheckInPack.Standard(10, new[] { "a", "b" }).RewardTables[0];
            Assert.That(row[2].Boosters["a"], Is.EqualTo(1)); Assert.That(row[4].Boosters["b"], Is.EqualTo(1));
            Assert.That(row[6].Boosters.Keys, Is.EquivalentTo(new[] { "a", "b" }));
            Assert.That(row[0].Boosters, Is.Empty); Assert.That(row[2].Gold, Is.EqualTo(30));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace IndieGame.Shell.Tests
{
    public sealed class QuestPackTests
    {
        sealed class NullGrantor : IRewardGrantor { public GrantOutcome Grant(string claimId, Reward reward) => default; }

        [Test] public void StandardPackHasUniqueIdsAndPicksFewerThanPool()
        {
            var pack = QuestPack.Standard(10);
            Assert.That(pack.Daily.Select(q => q.Id).Distinct().Count(), Is.EqualTo(pack.Daily.Count));
            Assert.That(pack.DailyCount, Is.LessThan(pack.Daily.Count));
        }

        [Test] public void StandardPackUsesOnlySharedEventKeys()
        {
            var allowed = new HashSet<string> { QuestEvents.Win, QuestEvents.Play };
            Assert.That(QuestPack.Standard(10).Daily.All(q => allowed.Contains(q.EventKey)), Is.True);
        }

        [Test] public void StandardPackIsAcceptedByTheService()
        {
            var service = new DailyQuestsService(QuestPack.Standard(10), new ScreenClock(), new NullGrantor());
            Assert.That(service.Refresh().Success, Is.True);
            Assert.That(service.Quests.Count, Is.EqualTo(QuestPack.DailyPicked));
        }

        [Test] public void StandardPackScalesRewardWithTarget()
        {
            var pack = QuestPack.Standard(10);
            Assert.That(pack.Daily.Single(q => q.Id == "win5").Reward.Gold, Is.GreaterThan(pack.Daily.Single(q => q.Id == "win1").Reward.Gold));
        }

        [Test] public void SaveFromAnOlderQuestSetResetsInsteadOfFailingBoot()
        {
            var oldConfig = new DailyQuestsConfig(new[] { new QuestTemplate("q", "win", 1, new Reward(5)) }, 1);
            var document = new SaveDocument();
            var oldService = new DailyQuestsService(oldConfig, new ScreenClock(), new NullGrantor());
            Assert.That(oldService.Refresh().Success, Is.True);
            Assert.That(oldService.Save(document).Success, Is.True);
            var current = new DailyQuestsService(QuestPack.Standard(10), new ScreenClock(), new NullGrantor());
            Assert.That(current.Load(document).Success, Is.True);
            Assert.That(current.Refresh().Success, Is.True);
            Assert.That(current.Quests.Count, Is.EqualTo(QuestPack.DailyPicked));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using IndieGame.Shell.Unity;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
namespace IndieGame.Shell.Tests
{
    public sealed class BoosterInventoryTests
    {
        DemoStyles demos; GameObject root; ShellUiFactory factory; ScreenClock clock; ScreenFiles files; LocalizationService text;
        Texture2D texture; Sprite sprite;
        [SetUp] public void Setup()
        {
            demos=new DemoStyles(); factory=new ShellUiFactory(demos.Make(false)); root=new GameObject("UI",typeof(RectTransform),typeof(Canvas));
            clock=new ScreenClock(); files=new ScreenFiles(); text=ScreenTestData.Text();
            texture=new Texture2D(4,4); sprite=Sprite.Create(texture,new Rect(0,0,4,4),Vector2.one*.5f);
        }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); Object.DestroyImmediate(root); demos.Dispose(); }

        ShellSession NewSession(params (string id,int count)[] initial)
        {
            var boosters=initial.ToDictionary(b=>b.id,b=>b.count);
            var row=Enumerable.Repeat(new Reward(25,1),CheckInPack.Days).ToArray();
            var session=new ShellSession(new SaveStore(fileSystem:files),"booster.json",clock,new LivesConfig(5,1,System.TimeSpan.FromSeconds(30)),new EconomyConfig(100,boosters),
                new DailyCheckInConfig(new[]{row,row}),new DailyQuestsConfig(new[]{new QuestTemplate("q","event",2,new Reward(7))},1),
                new SettingsConfig(true,true,true,"en",new[]{"en","vi"}),new RemoteConfigRules(new RemoteConfig(1,60),1000,1,3600));
            session.LoadAll(); return session;
        }
        static Reward BoosterReward(string id,int count)=>new Reward(boosters:new Dictionary<string,int>{{id,count}});

        [Test] public void InventoryCountReflectsInitialBoosters()
        {
            IBoosterInventory inventory=NewSession(("hint",3)).Economy;
            Assert.That(inventory.Count("hint"),Is.EqualTo(3));
        }
        [Test] public void InventoryUnknownIdCountsZeroAndCannotBeUsed()
        {
            var economy=NewSession(("hint",3)).Economy; int changed=0; economy.Changed+=()=>changed++;
            Assert.That(economy.Count("nope"),Is.EqualTo(0)); Assert.That(economy.Use("nope"),Is.False); Assert.That(changed,Is.EqualTo(0));
        }
        [Test] public void InventoryRewardGrantRaisesChangedOnceAndAddsCount()
        {
            var economy=NewSession(("hint",1)).Economy; int changed=0; economy.Changed+=()=>changed++;
            Assert.That(economy.GrantReward("c1",BoosterReward("hint",2)).Success,Is.True);
            Assert.That(changed,Is.EqualTo(1)); Assert.That(economy.Count("hint"),Is.EqualTo(3));
        }
        [Test] public void InventoryUseDecrementsRaisesChangedAndSurvivesSaveRestore()
        {
            var session=NewSession(("hint",2)); int changed=0; session.Economy.Changed+=()=>changed++;
            Assert.That(session.Economy.Use("hint"),Is.True);
            Assert.That(changed,Is.EqualTo(1)); Assert.That(session.Economy.Count("hint"),Is.EqualTo(1));
            Assert.That(session.Commit().Success,Is.True);
            var restored=NewSession(("hint",2)); Assert.That(restored.Economy.Count("hint"),Is.EqualTo(1));
        }
        [Test] public void InventoryUseAtZeroReturnsFalseWithoutChanged()
        {
            var economy=NewSession(("hint",0)).Economy; int changed=0; economy.Changed+=()=>changed++;
            Assert.That(economy.Use("hint"),Is.False); Assert.That(changed,Is.EqualTo(0)); Assert.That(economy.Count("hint"),Is.EqualTo(0));
        }

        [Test] public void RewardStripShowsAnImageForABoosterWithAnIcon()
        {
            factory.BoosterIcons=new Dictionary<string,Sprite>{{"hint",sprite}};
            var parts=new ScreenText(NewSession(),text).RewardParts(BoosterReward("hint",2));
            var strip=factory.RewardStrip(root.transform,parts,"rowText");
            Assert.That(strip.GetComponentsInChildren<UnityEngine.UI.Image>(true).Any(i=>i.sprite==sprite),Is.True);
            Assert.That(strip.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text),Is.EqualTo(new[]{"2"}));
        }
        [Test] public void RewardStripKeepsPlainTextForABoosterWithoutAnIcon()
        {
            var session=NewSession(); var screenText=new ScreenText(session,text);
            var parts=screenText.RewardParts(BoosterReward("hint",2));
            var strip=factory.RewardStrip(root.transform,parts,"rowText");
            Assert.That(strip.GetComponentsInChildren<UnityEngine.UI.Image>(true),Is.Empty);
            Assert.That(strip.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.text),Is.EqualTo(new[]{parts.Single().Text}));
        }

        [Test] public void GameConfigParsesInitialBoosters()
        {
            var game=ShellGameConfig.Parse("{\"gameId\":\"t\",\"initialBoosters\":[{\"id\":\"hint\",\"count\":3},{\"id\":\"undo\",\"count\":1}]}");
            Assert.That(game.initialBoosters.Select(b=>(b.id,b.count)),Is.EqualTo(new[]{("hint",3),("undo",1)}));
        }
        [Test] public void GameConfigWithoutInitialBoostersHasNoneAndRejectsBadEntries()
        {
            Assert.That(ShellGameConfig.Parse("{\"gameId\":\"t\"}").initialBoosters,Is.Empty);
            Assert.That(Assert.Throws<System.InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"t\",\"initialBoosters\":[{\"id\":\"\",\"count\":1}]}")).Message,Does.Contain("initialBoosters"));
            Assert.That(Assert.Throws<System.InvalidOperationException>(()=>ShellGameConfig.Parse("{\"gameId\":\"t\",\"initialBoosters\":[{\"id\":\"a\",\"count\":-1}]}")).Message,Does.Contain("initialBoosters"));
        }
    }
}

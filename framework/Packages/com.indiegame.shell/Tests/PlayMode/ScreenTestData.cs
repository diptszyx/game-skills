using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace IndieGame.Shell.Tests
{
    public sealed class ScreenClock : IClock { public DateTime UtcNow { get; set; } = new DateTime(2026,11,10,23,59,59,DateTimeKind.Utc); }
    public sealed class ScreenFiles : ISaveFileSystem
    {
        public readonly Dictionary<string,string> Data=new Dictionary<string,string>(); public bool Fail; public int Writes;
        public bool Exists(string path)=>Data.ContainsKey(path);
        public string ReadAllText(string path)=>Data[path];
        public void WriteAllText(string path,string text) { Writes++; if(Fail) throw new IOException("injected"); Data[path]=text; }
        public void Replace(string source,string destination,string backup) { if(backup!=null) Data[backup]=Data[destination]; Data[destination]=Data[source]; Data.Remove(source); }
        public void Move(string source,string destination) { Data[destination]=Data[source]; Data.Remove(source); }
    }
    public static class ScreenTestData
    {
        public static ShellSession Session(ScreenClock clock,ScreenFiles files,bool empty=false)
        {
            var row=Enumerable.Repeat(new Reward(25,1),5).ToArray();
            return new ShellSession(new SaveStore(fileSystem:files),"screen.json",clock,new LivesConfig(5,1,TimeSpan.FromSeconds(30)),new EconomyConfig(100),
                new DailyCheckInConfig(new[]{row,row}),new DailyQuestsConfig(empty?Array.Empty<QuestTemplate>():new[]{new QuestTemplate("q","event",2,new Reward(7))},empty?0:1),
                new SettingsConfig(true,true,true,"en",new[]{"en","vi"}),new RemoteConfigRules(new RemoteConfig(1,60),1000,1,3600));
        }
        public static ProductCatalog Catalog()=>new ProductCatalog(new[]{new Product("p",PriceKind.Iap,0,new Reward(10))});
        public static LocalizationService Text()
        {
            var table=new Dictionary<string,string> {
                {"shell.checkin.title","Check in"},{"shell.checkin.slot","Slot {0}: {1}"},{"shell.checkin.other","Upcoming"},
                {"shell.claim","Claim"},{"shell.gold","Gold {0}"},{"shell.hearts","Hearts {0}"},{"shell.reward","{0}"},{"shell.separator",", "},
                {"shell.reward.unavailable","Unavailable"},{"shell.quests.title","Quests"},{"shell.quests.empty","No quests"},{"shell.quest.q","Quest"},{"shell.quest.row","{0} {1}/{2} {3}"},
                {"shell.main.title","Main"},{"shell.play","Play"},{"shell.lives.full","Full"},{"shell.lives.countdown","Full in {0}s"},
                {"shell.shop.title","Shop"},{"shell.shop.empty","No products"},{"shell.product.p","Product"},{"shell.shop.row","{0} {1} {2}"},{"shell.price.iap","Purchase"},{"shell.buy","Buy"},
                {"shell.error.title","Error"},{"shell.error.commit","Save failed: {0} {1}"},{"shell.error.None",""},{"shell.error.Io","Storage error"},
                {"shell.error.PurchaseFailed","Purchase failed"},{"shell.error.Cancelled","Cancelled"},{"shell.error.Incomplete","Incomplete"} };
            var l=new LocalizationService(new LocalizationConfig("en")); l.LoadTable("en","{"+string.Join(",",table.Select(p=>"\""+p.Key+"\":\""+p.Value+"\""))+"}"); return l;
        }
    }
}

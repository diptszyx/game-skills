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
            var row=Enumerable.Repeat(new Reward(25,1),CheckInPack.Days).ToArray();
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
                {"shell.reward.unavailable","Unavailable"},{"shell.quests.title","Quests"},{"shell.quests.empty","No quests"},{"shell.quest.q","Quest"},{"shell.quest.win1","Win 1"},{"shell.quest.win3","Win 3"},{"shell.quest.win5","Win 5"},{"shell.quest.play3","Play 3"},{"shell.quest.play5","Play 5"},{"shell.quest.row","{0} {1}/{2} {3}"},
                {"shell.main.title","Main"},{"shell.play","Play"},{"shell.lives.countdown","+1 {0}"},
                {"shell.shop.title","Shop"},{"shell.shop.empty","No products"},{"shell.product.p","Product"},{"shell.product.hearts","Hearts"},{"shell.product.gold_s","Small"},{"shell.product.gold_m","Medium"},{"shell.product.gold_l","Large"},{"shell.shop.row","{0} {1} {2}"},{"shell.price.iap","${0}"},{"shell.buy","Buy"},
                {"shell.error.title","Error"},{"shell.error.commit","Save failed: {0} {1}"},{"shell.error.None",""},{"shell.error.Io","Storage error"},
                {"shell.error.PurchaseFailed","Purchase failed"},{"shell.error.Cancelled","Cancelled"},{"shell.error.Incomplete","Incomplete"} };
            foreach(var kind in new[]{"win","lose","revive","pause","leave"}) { table["popup."+kind+".title"]=kind; table["popup."+kind+".body"]=kind; }
            foreach(var action in new[]{"next","double","retry","revive","ad","gold","resume","sound","music","haptics","leave","confirm","cancel"}) table["popup.action."+action]=action;
            foreach(var setting in new[]{"sound","music","haptics"}) table["popup.setting."+setting]=setting+" {0}";
            foreach(var error in new[]{"Failed","Unavailable","Skipped","NotReady","Insufficient"}) table["shell.error."+error]=error;
            table["shell.retrySave"]="Retry save"; table["shell.on"]="On"; table["shell.off"]="Off"; table["popup.revive.price"]="Gold {0}";
            table["shell.loading.tip"]="Tip"; table["shell.refresh"]="Refresh"; table["shell.leaderboard.title"]="Leaderboard";
            table["shell.leaderboard.loading"]="Loading"; table["shell.leaderboard.you"]="You"; table["shell.leaderboard.offline"]="Offline"; table["shell.leaderboard.empty"]="Empty";
            table["shell.home"]="Home"; table["shell.back"]="Back";
            var l=new LocalizationService(new LocalizationConfig("en")); l.LoadLayeredTable("vi",UnityEngine.Resources.Load<UnityEngine.TextAsset>("ShellLocalization/vi").text,"{}"); l.LoadLayeredTable("en",UnityEngine.Resources.Load<UnityEngine.TextAsset>("ShellLocalization/en").text,"{"+string.Join(",",table.Select(p=>"\""+p.Key+"\":\""+p.Value+"\""))+"}"); return l;
        }
    }
}

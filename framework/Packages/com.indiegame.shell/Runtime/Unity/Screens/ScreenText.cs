using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public sealed class ScreenText
    {
        readonly ShellSession session;
        readonly LocalizationService localization;
        public ScreenText(ShellSession session, LocalizationService localization) { this.session=session; this.localization=localization; }
        public string Get(string key, params object[] args) => localization.Format(session.Settings.Language,key,args).Value;
        public string KeyOrText(string value) => value.Contains(' ')||!value.Contains('.') ? value : Get(value);
        public string Reward(Reward reward)
        {
            if(reward==null) return Get("shell.reward.unavailable");
            var items=new List<string>();
            if(reward.Gold>0) items.Add(Get("shell.gold",reward.Gold));
            if(reward.Hearts>0) items.Add(Get("shell.hearts",reward.Hearts));
            foreach(var item in reward.Boosters) items.Add(Get("shell.booster",Get("shell.booster."+item.Key),item.Value));
            return Get("shell.reward",string.Join(Get("shell.separator"),items));
        }
        public IReadOnlyList<RewardPart> RewardParts(Reward reward)
        {
            if(reward==null) return new[]{RewardPart.Plain(Get("shell.reward.unavailable"))};
            var parts=new List<RewardPart>();
            if(reward.Gold>0) parts.Add(RewardPart.Gold(reward.Gold));
            if(reward.Hearts>0) parts.Add(RewardPart.Hearts(reward.Hearts));
            foreach(var item in reward.Boosters) parts.Add(RewardPart.Booster(item.Key,item.Value,Get("shell.booster",Get("shell.booster."+item.Key),item.Value)));
            return parts;
        }
        internal string CommitError(SessionResult result)
        {
            var codes=new List<string>(); foreach(var item in result.ModuleErrors) codes.Add(Get("shell.error."+item.Value));
            return Get("shell.error.commit",Get("shell.error."+result.Error),string.Join(Get("shell.separator"),codes));
        }
        public bool Commit(Action<string> error)
        {
            var result=session.Commit();
            if(!result.Success)
            {
                error(CommitError(result));
            }
            return result.Success;
        }
    }
}

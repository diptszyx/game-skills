using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public enum SessionPopupKind { Win, Lose, Revive, Pause, Leave, Settings }
    public sealed class SessionPopupConfig
    {
        public Reward WinReward { get; }
        public long ReviveGold { get; }
        public string RevivePlacement { get; }
        public string DoublePlacement { get; }
        public SessionPopupConfig(Reward winReward,long reviveGold,string revivePlacement,string doublePlacement)
        {
            if(winReward==null||reviveGold<=0||string.IsNullOrWhiteSpace(revivePlacement)||string.IsNullOrWhiteSpace(doublePlacement)) throw new ArgumentException("Invalid popup configuration");
            WinReward=winReward; ReviveGold=reviveGold; RevivePlacement=revivePlacement; DoublePlacement=doublePlacement;
        }
    }
    // OnDisable covers host Back, scrim, external disable, host teardown and owner closes.
    public sealed class PopupExit : MonoBehaviour
    {
        public Action Exited;
        void OnDisable()=>Exited?.Invoke();
        void OnDestroy()=>Exited?.Invoke();
    }
    public sealed class SessionPopupContent
    {
        public PopupView View { get; internal set; }
        public readonly Dictionary<string,Button> Buttons=new Dictionary<string,Button>();
        public TMP_Text ErrorLabel { get; internal set; }
        internal TMP_Text Title, Body;
        public RectTransform RewardStrip { get; internal set; }
        public ResultCelebrationDriver Celebration { get; internal set; }
        public string Error { get; internal set; }
        internal bool AdPending, Exited;
    }
    public sealed class SessionPopupPresenter : IDisposable
    {
        readonly ShellSession session; readonly ShellUiFactory factory; readonly PopupHost host; readonly ScreenText text;
        readonly IGameSessionActions game; readonly IRewardedAdService ads; readonly SessionPopupConfig config; readonly RewardFlyLayer fly; readonly IResultCelebration winCelebration, loseCelebration;
        readonly Dictionary<SessionPopupKind,SessionPopupContent> popups=new Dictionary<SessionPopupKind,SessionPopupContent>();
        readonly HashSet<SessionPopupKind> consumed=new HashSet<SessionPopupKind>();
        readonly HashSet<string> recordedRounds=new HashSet<string>();
        readonly TMP_Text errorBanner;
        string round; bool busy,paused,disposed; float priorScale;
        public IReadOnlyDictionary<SessionPopupKind,SessionPopupContent> Popups=>popups;
        public SessionPopupPresenter(ShellSession session,ShellUiFactory factory,PopupHost host,LocalizationService localization,IGameSessionActions game,IRewardedAdService ads,SessionPopupConfig config,RewardFlyLayer fly=null,IResultCelebration winCelebration=null,IResultCelebration loseCelebration=null)
        { this.fly=fly; this.winCelebration=winCelebration; this.loseCelebration=loseCelebration; this.session=session; this.factory=factory; this.host=host; text=new ScreenText(session,localization); this.game=game; this.ads=ads; this.config=config;
          errorBanner=factory.Label(host.transform,""); errorBanner.rectTransform.anchorMin=errorBanner.rectTransform.anchorMax=new Vector2(.5f,0); errorBanner.rectTransform.anchoredPosition=new Vector2(0,60); }
        public void ShowWin(string roundId)
        {
            if(string.IsNullOrWhiteSpace(roundId)) throw new ArgumentException("Stable round ID required");
            round=roundId; session.Attempt.Won();
            var p=Open(SessionPopupKind.Win);
            if(recordedRounds.Add(roundId)) { RecordQuest(p,QuestEvents.Win); RecordQuest(p,QuestEvents.Play); }
            var grant=session.Economy.GrantReward("session:win:"+round,config.WinReward);
            if(grant.Success) FlyWinReward(p);
            if(grant.Success||grant.Error==ModuleError.AlreadyClaimed) Save(p); else Error(p,grant.Error.ToString());
        }
        void FlyWinReward(SessionPopupContent p) { if(fly!=null) fly.Fly(p.RewardStrip,text.RewardParts(config.WinReward)); }
        public void ShowLose() { session.Attempt.Lost(); var p=Open(SessionPopupKind.Lose); RecordQuest(p,QuestEvents.Play); Save(p); }
        void RecordQuest(SessionPopupContent p,string key)
        { var r=session.DailyQuests.RecordEvent(key,1); if(!r.Success) Error(p,r.Error.ToString()); }
        public void ShowRevive()=>Open(SessionPopupKind.Revive);
        public void ShowPause()
        {
            if(!paused) { priorScale=Time.timeScale; paused=true; game.Pause(); Time.timeScale=0; }
            Save(Open(SessionPopupKind.Pause));
        }
        public void ShowSettings()=>Open(SessionPopupKind.Settings);
        static bool HasSettingsControls(SessionPopupKind kind)=>kind==SessionPopupKind.Pause||kind==SessionPopupKind.Settings;
        public void ShowLeave() { if(!paused) ShowPause(); Open(SessionPopupKind.Leave); }
        SessionPopupContent Open(SessionPopupKind kind)
        {
            if(disposed) throw new ObjectDisposedException(nameof(SessionPopupPresenter));
            if(popups.TryGetValue(kind,out var existing)&&host.Stack.ContainsView(existing.View)) return existing;
            if(existing!=null) DestroyView(existing.View);
            consumed.Remove(kind);
            var p=new SessionPopupContent { View=factory.PopupFrame(host.transform,text.Get("popup."+kind.ToString().ToLowerInvariant()+".title"),closeLabel:text.Get("shell.close")) };
            popups[kind]=p;
            p.View.Policy=HasSettingsControls(kind)||kind==SessionPopupKind.Leave?DismissalPolicy.Dismissible:DismissalPolicy.Mandatory;
            p.View.FitContent=true;

            var layout=p.View.Content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.childControlHeight=false; layout.childForceExpandHeight=false; layout.childControlWidth=false; layout.childForceExpandWidth=false; layout.childAlignment=TextAnchor.MiddleCenter; layout.spacing=factory.Style.Spacing;
            p.Body=factory.Label(p.View.Content,text.Get("popup."+kind.ToString().ToLowerInvariant()+".body"),role:"popupText");
            p.Title=p.View.Panel.Find("Title").GetComponent<TMP_Text>();
            if(kind==SessionPopupKind.Win) p.RewardStrip=factory.RewardStrip(p.View.Content,text.RewardParts(config.WinReward),"popupText");
            if(kind==SessionPopupKind.Revive) factory.Label(p.View.Content,text.Get("popup.revive.price",config.ReviveGold),role:"popupText");
            Transform toggleRow=null;
            if(HasSettingsControls(kind)) {
                var row=ShellUiFactory.Node("Settings icons",p.View.Content); row.sizeDelta=new Vector2(ShellLayout.Standard.ToggleRowWidth,ShellLayout.Standard.IconHitSize);
                var element=row.gameObject.AddComponent<LayoutElement>(); element.preferredHeight=ShellLayout.Standard.IconHitSize;
                row.gameObject.SetActive(true); toggleRow=row;
            }
            string[] actions=kind==SessionPopupKind.Win?new[]{"next","double"}:kind==SessionPopupKind.Lose?new[]{"retry","revive"}:kind==SessionPopupKind.Revive?new[]{"ad","gold"}:kind==SessionPopupKind.Pause?new[]{"sound","music","haptics","resume","leave"}:kind==SessionPopupKind.Settings?new[]{"sound","music","haptics","language"}:new[]{"confirm","cancel"};
            foreach(var action in actions)
            {
                var captured=action;
                bool toggle=HasSettingsControls(kind)&&(action=="sound"||action=="music"||action=="haptics");
                string icon=toggle?action+"_on":action=="resume"?"play":action=="language"?"globe":action=="leave"?"home":action=="double"||action=="ad"?"video":action=="gold"?"coin":action=="revive"?"heart":action=="confirm"?"check":action=="cancel"?"close":action;
                string label=text.Get(action=="leave"?"shell.home":"popup.action."+action);
                string amount=action=="double"?"x2":action=="gold"?config.ReviveGold.ToString(System.Globalization.CultureInfo.InvariantCulture):null;
                var category=toggle?ShellLayoutCategory.IconButton:amount!=null?ShellLayoutCategory.IconAmount:ShellLayoutCategory.IconAction;
                var button=factory.IconButton(toggle?toggleRow:p.View.Content,icon,label,()=>Act(kind,captured),category,amount,action=="revive"?"video":null,action);
                if(toggle) ((RectTransform)button.transform).anchoredPosition=new Vector2((action=="sound"?-1:action=="music"?0:1)*ShellLayout.Standard.IconHitSize,0);
                p.Buttons.Add(action,button);
                if(p.View.InitialFocus==null&&!toggle) p.View.InitialFocus=button;
            }
            var retry=factory.IconButton(p.View.Content,"retry",text.Get("shell.retrySave"),()=>RetrySave(kind),ShellLayoutCategory.IconAction);
            p.Buttons.Add("save",retry); retry.gameObject.SetActive(false);
            p.ErrorLabel=factory.Label(p.View.Content,"");
            p.View.gameObject.AddComponent<PopupExit>().Exited=()=>Exited(kind,p);
            if(kind==SessionPopupKind.Win) p.Celebration=ResultCelebrationDriver.Attach(p.View,winCelebration,true);
            else if(kind==SessionPopupKind.Lose&&loseCelebration!=null) p.Celebration=ResultCelebrationDriver.Attach(p.View,loseCelebration,false);
            host.Open(p.View); RenderSettings(); return p;
        }
        public void Act(SessionPopupKind kind,string action)
        {
            if(disposed||busy||consumed.Contains(kind)||!popups.TryGetValue(kind,out var p)||p.AdPending||p.Exited||p.View==null||host.Top!=p.View||!p.View.Group.interactable) return;
            busy=true;
            try
            {
                if(HasSettingsControls(kind)&&ChangeSetting(p,action)) return;
                if(kind==SessionPopupKind.Pause)
                {
                    if(action=="leave") { ShowLeave(); return; }
                    if(action=="resume") { Complete(kind); RestorePause(p); } return;
                }
                if(kind==SessionPopupKind.Leave)
                {
                    if(action=="cancel") { Complete(kind); return; }
                    if(action=="confirm") { Complete(kind); game.Leave(); RestorePause(p); ClosePause(); Save(p); } return;
                }
                if(kind==SessionPopupKind.Lose)
                {
                    if(action=="revive") { p.Celebration?.Stop(); Complete(kind); ShowRevive(); return; }
                    if(action=="retry") { p.Celebration?.Stop(); consumed.Add(kind); game.Retry(); if(Save(p)) host.Close(p.View); } return;
                }
                if(kind==SessionPopupKind.Revive)
                {
                    if(action=="ad")
                    {
                        if(ads==null) { Error(p,"Unavailable"); return; }
                        ShowAd(kind,p,config.RevivePlacement,()=>
                        { consumed.Add(kind); game.Revive(); if(Save(p)) host.Close(p.View); });
                        return;
                    }
                    else if(action=="gold") { var spend=session.Economy.SpendGold(config.ReviveGold); if(!spend.Success) { Error(p,spend.Error.ToString()); return; } }
                    else return;
                    consumed.Add(kind); game.Revive(); if(Save(p)) host.Close(p.View); return;
                }
                if(kind==SessionPopupKind.Win)
                {
                    if(action=="double")
                    {
                        if(session.Economy.Claims.Contains("session:double:"+round)) return;
                        if(ads==null) { Error(p,"Unavailable"); return; }
                        p.Celebration?.Stop();
                        var claim="session:double:"+round;
                        ShowAd(kind,p,config.DoublePlacement,()=>
                        {
                            var grant=session.Economy.GrantReward(claim,config.WinReward);
                            if(!grant.Success) { Error(p,grant.Error.ToString()); return; }
                            FlyWinReward(p); Save(p); p.Buttons["double"].interactable=false;
                        });
                        return;
                    }
                    if(action=="next") { p.Celebration?.Stop(); consumed.Add(kind); game.NextLevel(); if(Save(p)) host.Close(p.View); }
                }
            }
            catch(Exception) { Error(p,"Failed"); }
            finally { busy=false; }
        }
        void ShowAd(SessionPopupKind kind,SessionPopupContent p,string placement,Action success)
        {
            p.AdPending=true; SetButtons(p,false);
            bool completed=false;
            Action<AdResult> done=result=>
            {
                if(completed) return; completed=true;
                p.AdPending=false;
                if(disposed||p.Exited||consumed.Contains(kind)||host==null||p.View==null||p.View.Closing||!p.View.gameObject.activeInHierarchy||
                    !popups.TryGetValue(kind,out var current)||!ReferenceEquals(current,p)||!host.Stack.ContainsView(p.View)) return;
                SetButtons(p,true);
                try
                {
                    if(!result.Success) { Error(p,result.Error==ServiceError.None?result.Outcome.ToString():result.Error.ToString()); return; }
                    success();
                }
                catch(Exception) { Error(p,"Failed"); }
            };
            try { ads.Show(placement,done); }
            catch(Exception) { done(new AdResult(AdOutcome.Failed,ServiceError.Failed)); }
        }
        static void SetButtons(SessionPopupContent p,bool interactable)
        { foreach(var button in p.Buttons.Values) if(button!=null) button.interactable=interactable; }
        static void DestroyView(PopupView view) { if(Application.isPlaying) UnityEngine.Object.Destroy(view.gameObject); else UnityEngine.Object.DestroyImmediate(view.gameObject); }
        void Complete(SessionPopupKind kind) { consumed.Add(kind); host.Close(popups[kind].View); }
        void ClosePause() { if(popups.TryGetValue(SessionPopupKind.Pause,out var p)) { consumed.Add(SessionPopupKind.Pause); p.View.gameObject.SetActive(false); } }
        void Exited(SessionPopupKind kind,SessionPopupContent p)
        {
            p.Exited=true;
            if(kind==SessionPopupKind.Pause) RestorePause(p);
            if(kind==SessionPopupKind.Leave&&paused&&popups.TryGetValue(SessionPopupKind.Pause,out var pause)&&pause.View!=null&&!pause.View.gameObject.activeSelf) RestorePause(p);
        }
        void RestorePause(SessionPopupContent p)
        {
            if(!paused) return; paused=false; Time.timeScale=priorScale;
            try { game.Resume(); Save(p); } catch(Exception) { Error(p,"Failed"); }
        }
        bool ChangeSetting(SessionPopupContent p,string action)
        {
            SaveResult<bool> result;
            if(action=="sound") result=session.Settings.SetSound(!session.Settings.Sound);
            else if(action=="music") result=session.Settings.SetMusic(!session.Settings.Music);
            else if(action=="haptics") result=session.Settings.SetHaptics(!session.Settings.Haptics);
            else if(action=="language") result=session.Settings.SetLanguage(NextLanguage());
            else return false;
            if(!result.Success) Error(p,result.Error.ToString()); else Save(p);
            RenderSettings(); return true;
        }
        string NextLanguage()
        {
            var codes=new List<string>(session.Settings.Languages); codes.Sort(StringComparer.Ordinal);
            return codes[(codes.IndexOf(session.Settings.Language)+1)%codes.Count];
        }
        void RenderSettings()
        {
            foreach(var entry in popups)
            {
                var p=entry.Value; if(!HasSettingsControls(entry.Key)||p.View==null) continue;
                string kind=entry.Key.ToString().ToLowerInvariant();
                if(p.Title!=null) p.Title.text=text.Get("popup."+kind+".title");
                if(p.Body!=null) p.Body.text=text.Get("popup."+kind+".body");
                foreach(var key in new[]{"sound","music","haptics"})
                { bool value=key=="sound"?session.Settings.Sound:key=="music"?session.Settings.Music:session.Settings.Haptics;
                  factory.SetIcon(p.Buttons[key],key+(value?"_on":"_off"),text.Get("popup.setting."+key,text.Get(value?"shell.on":"shell.off"))); }
                if(p.Buttons.TryGetValue("language",out var language))
                    factory.SetIcon(language,"globe",text.Get("popup.setting.language",text.Get("shell.language."+session.Settings.Language)));
            }
        }
        void Error(SessionPopupContent p,string key) { p.Error=text.Get("shell.error."+key); if(p.ErrorLabel!=null) p.ErrorLabel.text=p.Error; }
        bool Save(SessionPopupContent p)
        {
            bool saved=text.Commit(error=>{ p.Error=error; if(p.ErrorLabel!=null) p.ErrorLabel.text=error; if(errorBanner!=null) { errorBanner.text=error; errorBanner.transform.SetAsLastSibling(); } });
            if(p.Buttons.TryGetValue("save",out var retry)) retry.gameObject.SetActive(!saved);
            if(saved) { if(errorBanner!=null) errorBanner.text=""; p.Error=""; if(p.ErrorLabel!=null) p.ErrorLabel.text=""; }
            return saved;
        }
        public bool RetrySave(SessionPopupKind kind)
        {
            var p=popups[kind]; bool saved=Save(p);
            if(saved&&consumed.Contains(kind)&&host.Top==p.View) host.Close(p.View);
            return saved;
        }
        public void Dispose()
        {
            if(disposed) return; disposed=true;
            if(popups.TryGetValue(SessionPopupKind.Pause,out var pause)) RestorePause(pause);
            if(errorBanner!=null) { if(Application.isPlaying) UnityEngine.Object.Destroy(errorBanner.gameObject); else UnityEngine.Object.DestroyImmediate(errorBanner.gameObject); }
            foreach(var p in popups.Values) if(p.View!=null) { p.Celebration?.Stop(); p.View.gameObject.GetComponent<PopupExit>().Exited=null; p.View.gameObject.SetActive(false); DestroyView(p.View); }
        }
    }
    internal static class PopupStackLookup
    {
        internal static bool ContainsView(this IReadOnlyList<PopupView> stack,PopupView view) { foreach(var p in stack) if(p==view) return true; return false; }
    }
}

using System;
using System.Collections.Generic;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Screens
{
    public enum SessionPopupKind { Win, Lose, Revive, Pause, Leave }
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
        public string Error { get; internal set; }
    }
    public sealed class SessionPopupPresenter : IDisposable
    {
        readonly ShellSession session; readonly ShellUiFactory factory; readonly PopupHost host; readonly ScreenText text;
        readonly IGameSessionActions game; readonly IRewardedAdService ads; readonly SessionPopupConfig config;
        readonly Dictionary<SessionPopupKind,SessionPopupContent> popups=new Dictionary<SessionPopupKind,SessionPopupContent>();
        readonly HashSet<SessionPopupKind> consumed=new HashSet<SessionPopupKind>();
        readonly TMP_Text errorBanner;
        string round; bool busy,paused,disposed; float priorScale;
        public IReadOnlyDictionary<SessionPopupKind,SessionPopupContent> Popups=>popups;
        public SessionPopupPresenter(ShellSession session,ShellUiFactory factory,PopupHost host,LocalizationService localization,IGameSessionActions game,IRewardedAdService ads,SessionPopupConfig config)
        { this.session=session; this.factory=factory; this.host=host; text=new ScreenText(session,localization); this.game=game; this.ads=ads; this.config=config;
          errorBanner=factory.Label(host.transform,""); errorBanner.rectTransform.anchorMin=errorBanner.rectTransform.anchorMax=new Vector2(.5f,0); errorBanner.rectTransform.anchoredPosition=new Vector2(0,60); }
        public void ShowWin(string roundId)
        {
            if(string.IsNullOrWhiteSpace(roundId)) throw new ArgumentException("Stable round ID required");
            round=roundId;
            var p=Open(SessionPopupKind.Win);
            var grant=session.Economy.GrantReward("session:win:"+round,config.WinReward);
            if(grant.Success||grant.Error==ModuleError.AlreadyClaimed) Save(p); else Error(p,grant.Error.ToString());
        }
        public void ShowLose()=>Open(SessionPopupKind.Lose);
        public void ShowRevive()=>Open(SessionPopupKind.Revive);
        public void ShowPause()
        {
            if(!paused) { priorScale=Time.timeScale; paused=true; game.Pause(); Time.timeScale=0; }
            Save(Open(SessionPopupKind.Pause));
        }
        public void ShowLeave() { if(!paused) ShowPause(); Open(SessionPopupKind.Leave); }
        SessionPopupContent Open(SessionPopupKind kind)
        {
            if(disposed) throw new ObjectDisposedException(nameof(SessionPopupPresenter));
            if(popups.TryGetValue(kind,out var existing)&&host.Stack.ContainsView(existing.View)) return existing;
            if(existing!=null) DestroyView(existing.View);
            consumed.Remove(kind);
            var p=new SessionPopupContent { View=factory.PopupFrame(host.transform,text.Get("popup."+kind.ToString().ToLowerInvariant()+".title")) };
            popups[kind]=p;
            p.View.Policy=kind==SessionPopupKind.Pause||kind==SessionPopupKind.Leave?DismissalPolicy.Dismissible:DismissalPolicy.Mandatory;
            p.View.Panel.sizeDelta=new Vector2(p.View.Panel.sizeDelta.x,800);

            var layout=p.View.Content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.childControlHeight=false; layout.childForceExpandHeight=false; layout.spacing=factory.Style.Spacing;
            factory.Label(p.View.Content,text.Get("popup."+kind.ToString().ToLowerInvariant()+".body"));
            if(kind==SessionPopupKind.Win) factory.Label(p.View.Content,text.Reward(config.WinReward));
            if(kind==SessionPopupKind.Revive) factory.Label(p.View.Content,text.Get("popup.revive.price",config.ReviveGold));
            string[] actions=kind==SessionPopupKind.Win?new[]{"next","double"}:kind==SessionPopupKind.Lose?new[]{"retry","revive"}:kind==SessionPopupKind.Revive?new[]{"ad","gold"}:kind==SessionPopupKind.Pause?new[]{"resume","sound","music","haptics","leave"}:new[]{"confirm","cancel"};
            foreach(var action in actions)
            {
                var captured=action; var button=factory.Button(p.View.Content,text.Get("popup.action."+action),clicked:()=>Act(kind,captured)); p.Buttons.Add(action,button);
                if(p.View.InitialFocus==null) p.View.InitialFocus=button;
            }
            var retry=factory.Button(p.View.Content,text.Get("shell.retrySave"),clicked:()=>RetrySave(kind));
            p.Buttons.Add("save",retry); retry.gameObject.SetActive(false);
            p.ErrorLabel=factory.Label(p.View.Content,"");
            p.View.gameObject.AddComponent<PopupExit>().Exited=()=>Exited(kind,p);
            host.Open(p.View); RenderSettings(); return p;
        }
        public void Act(SessionPopupKind kind,string action)
        {
            if(disposed||busy||consumed.Contains(kind)||!popups.TryGetValue(kind,out var p)||host.Top!=p.View||!p.View.Group.interactable) return;
            busy=true;
            try
            {
                if(kind==SessionPopupKind.Pause)
                {
                    if(action=="sound"||action=="music"||action=="haptics")
                    {
                        var result=action=="sound"?session.Settings.SetSound(!session.Settings.Sound):action=="music"?session.Settings.SetMusic(!session.Settings.Music):session.Settings.SetHaptics(!session.Settings.Haptics);
                        if(!result.Success) Error(p,result.Error.ToString()); else Save(p); RenderSettings(); return;
                    }
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
                    if(action=="revive") { Complete(kind); ShowRevive(); return; }
                    if(action=="retry") { consumed.Add(kind); game.Retry(); if(Save(p)) host.Close(p.View); } return;
                }
                if(kind==SessionPopupKind.Revive)
                {
                    if(action=="ad")
                    {
                        var ad=ads.Show(config.RevivePlacement);
                        if(!ad.Success) { Error(p,ad.Error==ServiceError.None?ad.Outcome.ToString():ad.Error.ToString()); return; }
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
                        var ad=ads.Show(config.DoublePlacement); if(!ad.Success) { Error(p,ad.Outcome.ToString()); return; }
                        var grant=session.Economy.GrantReward("session:double:"+round,config.WinReward);
                        if(!grant.Success) { Error(p,grant.Error.ToString()); return; }
                        Save(p); p.Buttons["double"].interactable=false; return;
                    }
                    if(action=="next") { consumed.Add(kind); game.NextLevel(); if(Save(p)) host.Close(p.View); }
                }
            }
            catch(Exception) { Error(p,"Failed"); }
            finally { busy=false; }
        }
        static void DestroyView(PopupView view) { if(Application.isPlaying) UnityEngine.Object.Destroy(view.gameObject); else UnityEngine.Object.DestroyImmediate(view.gameObject); }
        void Complete(SessionPopupKind kind) { consumed.Add(kind); host.Close(popups[kind].View); }
        void ClosePause() { if(popups.TryGetValue(SessionPopupKind.Pause,out var p)) { consumed.Add(SessionPopupKind.Pause); p.View.gameObject.SetActive(false); } }
        void Exited(SessionPopupKind kind,SessionPopupContent p)
        {
            if(kind==SessionPopupKind.Pause) RestorePause(p);
            if(kind==SessionPopupKind.Leave&&paused&&popups.TryGetValue(SessionPopupKind.Pause,out var pause)&&pause.View!=null&&!pause.View.gameObject.activeSelf) RestorePause(p);
        }
        void RestorePause(SessionPopupContent p)
        {
            if(!paused) return; paused=false; Time.timeScale=priorScale;
            try { game.Resume(); Save(p); } catch(Exception) { Error(p,"Failed"); }
        }
        void RenderSettings()
        {
            if(!popups.TryGetValue(SessionPopupKind.Pause,out var p)) return;
            foreach(var key in new[]{"sound","music","haptics"})
            { bool value=key=="sound"?session.Settings.Sound:key=="music"?session.Settings.Music:session.Settings.Haptics;
              p.Buttons[key].GetComponentInChildren<TMP_Text>().text=text.Get("popup.setting."+key,text.Get(value?"shell.on":"shell.off")); }
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
            foreach(var p in popups.Values) if(p.View!=null) { p.View.gameObject.GetComponent<PopupExit>().Exited=null; p.View.gameObject.SetActive(false); DestroyView(p.View); }
        }
    }
    internal static class PopupStackLookup
    {
        internal static bool ContainsView(this IReadOnlyList<PopupView> stack,PopupView view) { foreach(var p in stack) if(p==view) return true; return false; }
    }
}

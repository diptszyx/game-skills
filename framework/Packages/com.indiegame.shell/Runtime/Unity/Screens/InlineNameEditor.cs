using System;
using System.Threading.Tasks;
using IndieGame.Shell.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IndieGame.Shell.Unity.Screens
{
    // Edits the player's own leaderboard name in place: the row's name label is swapped for an input until it closes.
    public sealed class InlineNameEditor
    {
        public const string InvalidKey="shell.leaderboard.rename.invalid", OfflineKey="shell.leaderboard.offline", ErrorKeyPrefix="shell.error.";
        const float InputInset=8, ErrorBandTop=.4f;
        readonly ShellUiFactory factory; readonly ScreenText text; readonly ILeaderboardRename rename; readonly Action renamed;
        TMP_Text nameLabel, errorLabel; RectTransform field; TMP_InputField input;
        bool busy, submitRequested;
        public TMP_InputField Input => input;
        public bool IsEditing => input!=null;
        public string ErrorKey { get; private set; }
        public Task Pending { get; private set; } = Task.CompletedTask;
        public InlineNameEditor(ShellUiFactory factory,ScreenText text,ILeaderboardRename rename,Action renamed)
        { this.factory=factory; this.text=text; this.rename=rename; this.renamed=renamed; }
        public void Begin(TMP_Text label)
        {
            if(IsEditing) return;
            nameLabel=label; busy=false; submitRequested=false; ErrorKey=null;
            Build(label);
            label.enabled=false; input.text=label.text; Focus();
        }
        void Build(TMP_Text label)
        {
            var row=label.rectTransform.parent; var slot=label.rectTransform;
            field=ShellUiFactory.Node("Name input",row);
            var background=field.gameObject.AddComponent<Image>(); background.color=Color.clear; background.raycastTarget=true;
            field.anchorMin=slot.anchorMin; field.anchorMax=slot.anchorMax; field.offsetMin=slot.offsetMin; field.offsetMax=slot.offsetMax;
            var area=ShellUiFactory.Node("Text Area",field); ShellUiFactory.Stretch(area);
            area.offsetMin=Vector2.zero; area.offsetMax=Vector2.zero; area.gameObject.AddComponent<RectMask2D>(); area.gameObject.SetActive(true);
            var value=InputText(area,"",label); var hint=InputText(area,text.Get("shell.leaderboard.rename.hint"),label);
            hint.fontStyle=FontStyles.Italic; hint.alpha=.5f;
            errorLabel=factory.Label(row,"",role:"rowText"); errorLabel.gameObject.name="Name error";
            errorLabel.alignment=TextAlignmentOptions.MidlineLeft; errorLabel.enableAutoSizing=false; errorLabel.fontSize=ScreenWidgets.RowDetailFont; errorLabel.color=label.color;
            errorLabel.textWrappingMode=TextWrappingModes.NoWrap; errorLabel.overflowMode=TextOverflowModes.Ellipsis;
            var error=errorLabel.rectTransform; error.anchorMin=new Vector2(slot.anchorMin.x,0); error.anchorMax=new Vector2(slot.anchorMax.x,ErrorBandTop);
            error.offsetMin=new Vector2(slot.offsetMin.x+InputInset,0); error.offsetMax=new Vector2(slot.offsetMax.x,0); errorLabel.gameObject.SetActive(false);
            input=background.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic=background; input.textViewport=area; input.textComponent=value; input.placeholder=hint;
            input.lineType=TMP_InputField.LineType.SingleLine; input.onFocusSelectAll=true;
            input.onValueChanged.AddListener(_=>{ if(ErrorKey!=null) SetError(null); });
            input.onSubmit.AddListener(_=>submitRequested=true);
            input.onEndEdit.AddListener(Resolve);
        }
        TMP_Text InputText(RectTransform parent,string value,TMP_Text like)
        {
            var label=factory.Label(parent,value,role:"rowText"); ShellUiFactory.Stretch(label.rectTransform);
            label.alignment=like.alignment; label.font=like.font; label.fontSize=like.fontSize; label.fontStyle=like.fontStyle; label.color=like.color;
            label.enableAutoSizing=false; label.textWrappingMode=TextWrappingModes.NoWrap; return label;
        }
        void Focus()
        { input.Select(); input.ActivateInputField(); }
        // TMP raises onSubmit (Done / Return) before onEndEdit; any other end of editing (focus lost, Escape, back) cancels.
        void Resolve(string value)
        {
            if(busy) return;
            if(!submitRequested) { Close(); return; }
            submitRequested=false;
            if(!PlayerNameRules.TryNormalize(value,out var name)) { Reject(InvalidKey); return; }
            busy=true; Pending=SubmitAsync(name);
        }
        async Task SubmitAsync(string name)
        {
            try
            {
                var result=await rename.UpdatePlayerName(name);
                if(result.Success) { Close(); renamed(); return; }
                Reject(result.Error==ServiceError.InvalidArgument?InvalidKey:result.Error==ServiceError.Unavailable?OfflineKey:ErrorKeyPrefix+result.Error);
            }
            catch(Exception e) { Debug.LogException(e); Reject(ErrorKeyPrefix+ServiceError.Failed); }
            finally { busy=false; }
        }
        void Reject(string key)
        { if(!IsEditing) return; SetError(key); Focus(); }
        void SetError(string key)
        {
            ErrorKey=key; errorLabel.text=key==null?"":text.Get(key); errorLabel.gameObject.SetActive(key!=null);
            field.anchorMin=new Vector2(field.anchorMin.x,key==null?0:ErrorBandTop);
        }
        void Close()
        {
            if(!IsEditing) return;
            if(nameLabel!=null) nameLabel.enabled=true;
            var errorObject=errorLabel.gameObject; var fieldObject=field.gameObject; input=null;
            Discard(errorObject); Discard(fieldObject);
        }
        static void Discard(GameObject go)
        {
            go.SetActive(false);
            if(Application.isPlaying) UnityEngine.Object.Destroy(go); else UnityEngine.Object.DestroyImmediate(go);
        }
    }
}

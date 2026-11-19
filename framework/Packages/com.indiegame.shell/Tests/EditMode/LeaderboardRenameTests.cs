using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndieGame.Shell.Unity.Screens;
using IndieGame.Shell.Unity.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine.UI;
namespace IndieGame.Shell.Tests
{
    public sealed class LeaderboardRenameTests
    {
        sealed class FakeBoard : ILeaderboardRequests, ILeaderboardRename
        {
            public string MyName="Old"; public int Tops, Renames; public string RenamedTo; public ServiceError RenameError=ServiceError.None;
            public Task<ServiceResult<bool>> SubmitScore(string board,long score)=>Task.FromResult(new ServiceResult<bool>(true));
            public Task<ServiceResult<IReadOnlyList<LeaderboardEntry>>> GetTop(string board,int count)
            { Tops++; return Task.FromResult(new ServiceResult<IReadOnlyList<LeaderboardEntry>>(new[]{new LeaderboardEntry("me",10,1,MyName)})); }
            public Task<ServiceResult<LeaderboardEntry>> GetMyRank(string board)=>Task.FromResult(new ServiceResult<LeaderboardEntry>(new LeaderboardEntry("me",10,1,MyName)));
            public Task<ServiceResult<string>> UpdatePlayerName(string name)
            {
                Renames++; RenamedTo=name;
                if(RenameError!=ServiceError.None) return Task.FromResult(new ServiceResult<string>(null,RenameError));
                MyName=name; return Task.FromResult(new ServiceResult<string>(name));
            }
        }
        static Button RenameButton(LeaderboardView view)=>view.Root.GetComponentsInChildren<Button>(true)
            .First(b=>b.GetComponent<ActionAccessibility>()!=null&&b.GetComponent<ActionAccessibility>().Key=="rename");
        static LeaderboardPresenter Make(PopupFixture f,FakeBoard board,out LeaderboardView view)
        {
            view=new LeaderboardView(f.Factory,f.Root.transform);
            return new LeaderboardPresenter(f.Session,view,ScreenTestData.Text(),board,"board",3,null,board);
        }
        static void EndEdit(InlineNameEditor editor,string typed,bool submit)
        {
            editor.Input.text=typed;
            if(submit) editor.Input.onSubmit.Invoke(typed);
            editor.Input.onEndEdit.Invoke(typed);
        }
        static TMP_Text NameLabel(LeaderboardView view)=>view.Root.GetComponentsInChildren<TMP_Text>(true).First(t=>t.name=="Name");
        [Test] public void TappingOwnRowShowsInlineInputWithNameAndOpensNoPopup()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    RenameButton(view).onClick.Invoke();
                    Assert.That(p.Rename.IsEditing,Is.True);
                    Assert.That(p.Rename.Input.text,Is.EqualTo("Old"));
                    Assert.That(NameLabel(view).enabled,Is.False);
                    Assert.That(f.Host.Top,Is.Null);
                }
            }
        }
        [Test] public void InlineInputIsTransparentAndOccupiesTheNameLabelRect()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    RenameButton(view).onClick.Invoke();
                    var label=NameLabel(view).rectTransform; var field=(UnityEngine.RectTransform)p.Rename.Input.transform;
                    var image=p.Rename.Input.GetComponent<Image>();
                    Assert.That(image.sprite,Is.Null);
                    Assert.That(image.color.a,Is.EqualTo(0f));
                    Assert.That(field.anchorMin,Is.EqualTo(label.anchorMin));
                    Assert.That(field.anchorMax,Is.EqualTo(label.anchorMax));
                    Assert.That(field.offsetMin,Is.EqualTo(label.offsetMin));
                    Assert.That(field.offsetMax,Is.EqualTo(label.offsetMax));
                }
            }
        }
        [Test] public void InlineInputIsActiveWhileEditing()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    RenameButton(view).onClick.Invoke();
                    Assert.That(p.Rename.Input.gameObject.activeInHierarchy,Is.True);
                    Assert.That(p.Rename.Input.textComponent.gameObject.activeInHierarchy,Is.True);
                }
            }
        }
        [Test] public async Task ValidRenameTrimsRefreshesBoardAndClosesField()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    int tops=board.Tops; RenameButton(view).onClick.Invoke();
                    EndEdit(p.Rename,"  Neo  ",true); await p.Rename.Pending; await p.Pending;
                    Assert.That(board.RenamedTo,Is.EqualTo("Neo"));
                    Assert.That(board.Tops,Is.GreaterThan(tops));
                    Assert.That(view.Snapshot.Entries[0].PlayerName,Is.EqualTo("Neo"));
                    Assert.That(p.Rename.ErrorKey,Is.Null);
                    Assert.That(p.Rename.IsEditing,Is.False);
                }
            }
        }
        [TestCase("")]
        [TestCase("   ",TestName="InvalidNameIsRejectedWithoutCallingBackend_OnlySpaces")]
        [TestCase("two words",TestName="InvalidNameIsRejectedWithoutCallingBackend_InnerSpace")]
        public void InvalidNameIsRejectedWithoutCallingBackend(string raw)
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    RenameButton(view).onClick.Invoke();
                    EndEdit(p.Rename,raw,true);
                    Assert.That(board.Renames,Is.EqualTo(0));
                    Assert.That(p.Rename.ErrorKey,Is.EqualTo(InlineNameEditor.InvalidKey));
                    Assert.That(p.Rename.IsEditing,Is.True);
                }
            }
        }
        [TestCase(ServiceError.Unavailable,"shell.leaderboard.offline")]
        [TestCase(ServiceError.InvalidArgument,InlineNameEditor.InvalidKey)]
        [TestCase(ServiceError.Failed,"shell.error.Failed")]
        public async Task BackendErrorIsShownInlineAndFieldStaysOpen(ServiceError error,string expectedKey)
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard { RenameError=error };
                using(var p=Make(f,board,out var view))
                {
                    int tops=board.Tops; RenameButton(view).onClick.Invoke();
                    EndEdit(p.Rename,"Neo",true); await p.Rename.Pending;
                    Assert.That(p.Rename.ErrorKey,Is.EqualTo(expectedKey));
                    Assert.That(p.Rename.IsEditing,Is.True);
                    Assert.That(board.Tops,Is.EqualTo(tops));
                }
            }
        }
        [Test] public void LosingFocusWithoutSubmitCancelsAndKeepsOldName()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard();
                using(var p=Make(f,board,out var view))
                {
                    RenameButton(view).onClick.Invoke();
                    EndEdit(p.Rename,"Neo",false);
                    Assert.That(board.Renames,Is.EqualTo(0));
                    Assert.That(p.Rename.IsEditing,Is.False);
                    Assert.That(NameLabel(view).enabled,Is.True);
                    Assert.That(NameLabel(view).text,Is.EqualTo("Old"));
                }
            }
        }
        [Test] public void NoRenameServiceMeansNoEditAffordance()
        {
            using(var f=new PopupFixture())
            {
                var board=new FakeBoard(); var view=new LeaderboardView(f.Factory,f.Root.transform);
                using(var p=new LeaderboardPresenter(f.Session,view,ScreenTestData.Text(),board,"board",3))
                {
                    Assert.That(p.Rename,Is.Null);
                    Assert.That(view.Root.GetComponentsInChildren<ActionAccessibility>(true).Any(a=>a.Key=="rename"),Is.False);
                }
            }
        }
        [TestCase(null,false)]
        [TestCase("",false)]
        [TestCase(" a",true,TestName="PlayerNameRulesMatchAuthenticationClientCheck_LeadingSpaceIsTrimmed")]
        [TestCase("a b",false,TestName="PlayerNameRulesMatchAuthenticationClientCheck_InnerSpaceIsInvalid")]
        [TestCase("Neo",true)]
        public void PlayerNameRulesMatchAuthenticationClientCheck(string raw,bool valid)
            =>Assert.That(PlayerNameRules.TryNormalize(raw,out _),Is.EqualTo(valid));
    }
}

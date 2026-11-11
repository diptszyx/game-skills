# T15

EXPECTED recorded before running any tests:

- dotnet: 168 existing, all passed, zero skipped/failed.
- EditMode: 38 = existing 32 + `SessionScreenTests.LeaderboardRendersEmptyErrorAndRank`, `LeaderboardIgnoresStaleResponse`, `ReviveGrantsOnlyOnCompletedAd`, `ReviveAdSkippedLeavesStateUnchanged`, `DoubleClickExecutesOnce`, `SettingsToggleCommitsAndSurfacesError`.
- PlayMode: 19 = existing 13 + `SessionFlowTests.LoadingBlocksInputUntilHidden`, `LoadingProgressIsRendered`, `AllFivePopupFlows`, `LeaveCancelKeepsPause`, `EveryPauseExitRestoresPriorState`, `MandatoryPopupsRejectBack`.
- C# compiler errors zero; Core noEngineReferences true; UnityEngine matches in Runtime/Core zero.

## Final gates

| Gate | Actual | Result |
| --- | --- | --- |
| C# compiler errors | 0 in both final Unity logs | PASS |
| dotnet Release | 168 passed, 0 failed/skipped | PASS |
| EditMode | 38 passed, exact expected names, 0 failed/skipped | PASS |
| PlayMode | 19 passed, exact expected names, 0 failed/skipped | PASS |
| Core asmdef | noEngineReferences true | PASS |
| Runtime/Core UnityEngine search | 0 matches, search exit 1 | PASS |

The existing `tests/run_unity_smoke.sh` is outside T15's edit scope and still expects T14's 32/13 tests. Its aggregate runner exit is therefore **1**, although the final Unity CLI exits are **0** and every test passes. New `Packages/com.indiegame.shell/Tests/EditMode/validate_t15.py` reads the existing runner's baseline name sets, adds the twelve names recorded above, and asserts exact names, counts, pass status, zero skips, compiler errors and Core isolation. That validator exits **0** for both final artifacts. This is an explicit T15 gate check, not a claim that the unchanged T14 script exits green.

Commands:

- `dotnet test dotnet/Shell.Core.Tests -c Release` once: 168 passed.
- `bash tests/run_unity_smoke.sh`: initial compile attempt, one fixed rerun, one final rerun after save-recovery/lifecycle improvements (limit: two reruns).
- `bash tests/run_unity_smoke.sh --mode PlayMode`: first run and one final rerun.
- `python3 Packages/com.indiegame.shell/Tests/EditMode/run_t15_mutations.py`: two targeted one-test mutation runs, sequential; these are failure proofs, not acceptance gate retries.
- `python3 Packages/com.indiegame.shell/Tests/EditMode/validate_t15.py`: final exact-set validation, PASS.

The first compile stopped on CS0052: the public test helper exposed internal DemoStyles. Made that helper field private; no production compiler defect. The fixed EditMode run passed all 38. All PlayMode runs passed all 19. No test failures were hidden or skipped. Final log artifacts are `unity-smoke/Logs/EditMode.xml`, `EditMode-Editor.log`, `PlayMode.xml`, `PlayMode-Editor.log` and corresponding CLI logs. No production changes followed final gates. Unity launches were sequential; final pgrep found no Unity Editor process.

## Delivered behavior

- LeaderboardView renders localized Loading, Empty, Error and Populated snapshots, ordered top entries plus a separately localized own-rank row, and Refresh. Presenter accepts ILeaderboardService via the default adapter or injectable ILeaderboardRequests for asynchronous providers. Request generation guards apply to successful, error and exception completions, and Dispose invalidates outstanding responses. Entry/resume refresh through ScreenLifetime. A missing own rank is a normal NotFound, while provider errors produce message keys.
- LoadingContentView adds themed progress graphics and a localized injected tip key to the existing LoadingScreenView, without replacing its controller or disabling its root. It follows authoritative monotonic Controller.Progress in LateUpdate. A raycast surface plus the existing CanvasGroup blocks input throughout minimum duration and fades, until Hidden.
- SessionPopupPresenter constructs five localized popup descriptors through PopupHost/PopupView. No new stack or modal framework. Win/Lose/Revive are mandatory; Pause/Leave are dismissible. Buttons route through Act. PopupExit forwards host Back/scrim close, direct disable, destruction and host teardown to the presenter. Presenter disposal also restores pause and detaches callbacks.
- Win grants the injected configured reward through Economy's persistent ledger, keyed by a required caller-supplied stable round ID (`session:win:<round>`). Completed rewarded ads grant one additional configured reward (`session:double:<round>`); repeats cannot grant or watch again after success. Next invokes NextLevel once. Lose supports Retry and the mandatory Revive offer. Revive accepts only successful Completed ads; Skipped, Failed, NotReady and provider errors keep gameplay/economy unchanged. Configured gold spending checks the Economy result before Revive. Irreversible command consumption plus a reentrancy guard rejects duplicate actions.
- Pause records the prior time scale, invokes the game's Pause bridge, and holds timeScale zero while Leave overlays it. Leave cancel, Back and scrim return to the held Pause. Resume, Pause Back/scrim, external disable, host teardown, leave confirmation and Dispose restore the prior scale and invoke Resume once. No hardcoded timeScale=1 restoration. The game owns its gameplay pause state through this bridge.
- Settings buttons display localized current sound/music/haptics values, call SettingsService, inspect results and Commit. Every state-changing path commits; failed commits preserve pending state under T12 policy and display errors. Retry-save buttons recover command/settings save failures without replaying gameplay commands, gold spending or rewards. A presenter-owned error banner remains visible when dismissal has already closed a popup; it clears after a successful whole-session commit. Mandatory command popups stay open on failed save until RetrySave succeeds.
- IGameSessionActions adds Retry, Revive, NextLevel, Leave, Pause and Resume alongside Play. Existing test bridges gained empty implementations to preserve the original suites. All displayed shell text uses localization keys; tests supply synthetic translations. Reward amounts, revive cost and placements are injected configuration. No game content, scene names or gameplay rules were added.

## Two deliberate failure proofs

The mutation script saves exact source bytes and restores them in finally blocks. Both failures were NUnit runtime assertions, not compiler failures:

1. Removed the leaderboard request-ID comparison. `LeaderboardIgnoresStaleResponse` failed: expected player `new`, actual `old`.
2. Bypassed the rewarded-ad success check in Revive. `ReviveAdSkippedLeavesStateUnchanged` failed: expected Revives 0, actual 1.

Artifacts: `unity-smoke/Logs/T15-mutation-LeaderboardIgnoresStaleResponse.xml` and `T15-mutation-ReviveAdSkippedLeavesStateUnchanged.xml`. Restored production subsequently passed all 38 EditMode and 19 PlayMode tests.

## Scope, AMBIG and unverified

- Deliverable edits are confined to Runtime/Unity/Screens, Tests/EditMode, Tests/PlayMode and this report. Existing production MainView changes are additive interface members; existing test helpers/bridges were adapted. Core and UI foundation source, smoke runner, git/GitHub and environment files were not edited/accessed through git or environment-file operations. Graph discovery was attempted first; this repository is not indexed, so direct reads were used.
- AMBIG: the brief names five popups, while critique2 specifies one shared PopupView with optional content. Implemented five descriptors/content variants in one presenter, rather than five redundant MonoBehaviour types.
- AMBIG: ILeaderboardService and IRewardedAdService are synchronous Core sockets. The leaderboard has an injectable Task boundary for future providers and stale-completion tests; its default adapter preserves the synchronous socket contract and does not offload calls to arbitrary worker threads. Live async SDK adapters must marshal completions to Unity's owning context. The ad socket remains synchronous; live SDK timing/cancellation is unverified. No Core contract was changed.
- AMBIG: the game supplies stable win round IDs, gameplay action implementations, composition/lifetime ownership and placement policies. IGameSessionActions uses void methods, so game-side success, crash recovery and atomic persistence across gameplay and shell saves cannot be established here. Implementations must honor those bridge contracts; remote reward verification and cross-device deduplication are unverified.
- Gold revival spending is persisted with session state after the gameplay bridge action. Commit failure retains the pending spend and blocks replay; RetrySave persists it. This does not promise atomic rollback of external game actions or provider completion on process termination.
- EveryPauseExitRestoresPriorState exercises resume button, Back, scrim, direct popup disable, host disable and presenter disposal at a prior scale of 0.35. AllFivePopupFlows separately verifies leave confirmation restores pause. LeaveCancelKeepsPause checks cancel leaves the Pause topmost, scale zero and zero Resume calls. Teardown callbacks have duplicate guards.
- Input blocking is verified through the real loading CanvasGroup/raycast graphics and minimum/fade lifecycle; physical touch delivery on devices is unverified. Tests construct real Unity trees and wait rendered frames, but device layout, long translations, glyph coverage, visual approval, screenshots, safe-area variants and IL2CPP are unverified. No approved visual reference was supplied for T15.
- Existing loading controller still uses wall clock and its existing minimum/fade settings. LoadingContentView borrows its authoritative state rather than introducing another clock/controller. Tip selection/rotation is owned by the caller through the localization key.
- No task-started background process remains. The only failing final wrapper status is the documented T14 allowlist mismatch; the T15 exact-set validator is green.

## Minutes per step

Approximately 7–8 minutes total, below the 60-minute cap. Initial reading boundaries are approximate; creation/modification timestamps and runner wall seconds are the evidence. The environment clock reported Nov 11 UTC despite the supplied local context date; these timestamps are recorded as observed, not corrected.

| Step | Minutes | Evidence |
| --- | ---: | --- |
| Docs, graph attempt, UI/Core source review | ~1.5 | Before first new production file at 17:33:09 UTC |
| Leaderboard/loading and session popup implementation | ~1.0 | First sources 17:33:09–17:34:05 |
| Fixtures and expected test manifest | ~1.3 | Tests and REPORT_T15 created 17:35:21 |
| Gates, helper fix, save recovery and mutation proofs | ~2.2 | Production final changes 17:37:32; EditMode runs 5s each; PlayMode 11s/10s |
| Exact-set validation, scope review and report | ~1.0 | Validator green at 17:38:23; final report immediately after |


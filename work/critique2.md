**Recommendation:** reuse game-kit’s theme data and validation, add a small runtime uGUI factory, and keep Core access in plain C# presenters. `UiBuilder` and `UiModal` cannot meet the whole requirement unchanged.

Reviewed 25 files; stopped reading. No edits, Unity, git, tests, or file creation.
Evidence prefixes: `K` = `game-kit/Packages/com.indiegame.ui`; `F` = `game-skills/framework`; `G` = `game-2/unity/Assets/Scripts/View`. Paths below are relative to `/Users/tho/project/indie-game/`. Test findings come from reading, not execution.

**1. Reuse and verified limitations**

| Component | Assessment and evidence |
|---|---|
| `UiTheme`, `UiButtonStyle` | Reuse unchanged as asset schemas: TMP font/materials, sprites, colors, geometry, button roles and motion are data. Geometry and straight-title behavior have builder tests. `K/Runtime/UiTheme.cs:10`, `K/Runtime/UiButtonStyle.cs:9`, `K/Tests/EditMode/UiBuilderTests.cs:58`, `:92`. |
| Theme coverage | Incomplete for whole screens: no screen background, currency/reward icons, list-row style or progress style; title color is hardcoded white in the builder. Add a shell style asset referencing `UiTheme`, with those missing roles. `K/Runtime/UiTheme.cs:10`, `K/Editor/UiBuilder.cs:50`. |
| `UiBuilder` | Reuse unchanged **only for Editor prefab generation**. It imports `UnityEditor`, serializes private fields and saves a prefab. It is not a runtime factory. Supports at most three modal buttons, not complete screens. `K/Editor/UiBuilder.cs:4`, `:32`, `:37`, `:63`. |
| `UiModal` | Useful animation implementation: unscaled time, interrupted transitions, cleanup and topmost keyboard dismissal. Existing tests cover these behaviors, including timeScale zero. `K/Runtime/UiModal.cs:55`, `:97`, `:144`; `K/Tests/PlayMode/UiModalTests.cs:57`, `:87`, `:105`. |
| Modal defects | `ModalSpec.Close=false` omits the node that `Awake` dereferences. Reopening an already-stacked lower modal changes sibling order without moving its stack entry: displayed top and Back recipient can disagree. Both follow directly from code; reviewed tests omit these cases. `K/Editor/UiBuilder.cs:57`; `K/Runtime/UiModal.cs:51`, `:60`, `:62`, `:80`. |
| Modal policy | Scrim/close always dismiss; Back policy cannot be configured through the public API. Initialization relies on serialized private references. Unsuitable unchanged for runtime construction and mandatory win/lose/revive decisions. `K/Runtime/UiModal.cs:14`, `:50`, `:74`. |
| `UiSafeArea` | Reusable for valid screen-space inputs under a full-screen parent. Zero safe rectangle collapses anchors; out-of-range inputs are not clamped. Tests cover only full-screen and top notch. Prefer the existing shell fitter, whose report documents zero fallback/clamping. `K/Runtime/UiSafeArea.cs:31`; `K/Tests/EditMode/UiSafeAreaTests.cs:10`; `F/REPORT_T6.md:54`. |
| `UiValidator` | Reuse Editor checks with explicit namespace/path/options. Thresholds are project conventions, not established accessibility standards; label checking assumes a button-face layout, and sprite importing prefers Single. Options tests demonstrate suppression, not visual correctness. `K/Editor/UiValidator.cs:46`, `:58`, `:158`, `:181`; `K/Tests/EditMode/UiValidatorOptionsTests.cs:14`, `:35`. |

**2. Minimal architecture**

- `ShellRoot`: composition and lifetime only. Inject existing Core instances, localization tables, style and a narrow `IGameSessionActions` bridge for play/retry/revive/leave/pause. Keep gameplay rules and scene names outside the reusable shell.
- `ShellUiFactory`: runtime uGUI/TMP construction—labels, styled buttons, rows, scrolling lists, screen roots and popup frames. Initialize inactive objects before activation; no `UnityEditor`, reflection-based wiring or runtime prefab saving.
- Six plain views: `MainView`, `ShopView`, `CheckInView`, `QuestsView`, `LeaderboardView`, plus loading content attached to the existing `LoadingScreenView`. Views render state and raise user intents.
- Plain presenters for the five data screens; one `SessionPopupPresenter` handles win/lose/revive/pause/leave using a shared `PopupView` with optional content. Avoid a generic binding framework or a base presenter hierarchy.
- Presenter flow: refresh services → build a display snapshot → render; on action, call Core → inspect result → persist affected modules together → refresh affected views. Refresh on entry, resume, commands and relevant timer boundaries; Core daily services provide no change events. `F/Packages/com.indiegame.shell/Runtime/Core/DailyQuests/DailyQuestsService.cs:78`, `:87`, `:119`.
- **Necessary Core gap:** check-in exposes `NextSlot`/`ObservedDay` and `Claim`, but no reward preview or readiness snapshot. Add a read-only query there; do not duplicate its date/table/claim rules in a presenter. `F/Packages/com.indiegame.shell/Runtime/Core/DailyCheckIn/DailyCheckInService.cs:41`, `:56`.
- `PopupHost` owns one ordered stack, unscaled transitions, top-only interaction, focus restoration and per-popup dismissal policy. Pause stays held while Leave overlays Pause; every dismissal path returns through the presenter. Reuse `UiTheme.Motion`, but do not combine this with `UiModal`’s independent static stack.
- `ShellStyle` references `UiTheme` plus missing screen roles. Style replacement rebuilds visible view trees from retained presenter state and popup descriptors; it does not recreate services or replay commands. Merely swapping a theme reference cannot update the builder’s copied component properties. `K/Editor/UiBuilder.cs:48`, `:85`, `:107`.
- Localization keys belong in screen/popup descriptors and content catalogs: e.g. `shell.shop.title`, `popup.leave.body`, quest-ID-to-description-key mappings. Presenters call `Lookup`/`Format` with `SettingsService.Language`; TMP receives resolved text. Re-render on language changes, not every frame: missing lookups append reports repeatedly. `F/Packages/com.indiegame.shell/Runtime/Core/Localization/LocalizationService.cs:53`, `:61`.
- Keep the loading root active so `Update` can tick its controller; add progress graphics/text around the existing adapter. It currently controls only CanvasGroup visibility/input. `F/Packages/com.indiegame.shell/Runtime/Unity/LoadingScreenView.cs:18`.

**3. Eight jobs, in dependency order**

Proposed paths below are under `F/Packages/com.indiegame.shell/`; proposed test names are acceptance gates, not existing passes. Estimates are **UNMEASURED engineering hours**, including their gates.

| # | Job and files | Measurable gate | Estimate |
|---|---|---|---|
| 1 | Integrate dependencies: package/asmdefs, `F/unity-smoke/Packages/manifest.json`; reference game-kit without editing it. | EditMode `ShellUiDependencyTests.RuntimeAssemblyHasNoEditorReferences`; exact existing 19-case adapter suite executes with zero failures/skips. | UNMEASURED 2–4 h |
| 2 | `Runtime/Unity/UI/ShellStyle.cs`, `ShellUiFactory.cs`; Editor `ShellUiValidation.cs`; two contrasting style assets. | EditMode `ShellUiFactoryTests.BuildsAllPrimitives`, `MissingRolesFailByName`, `LockedButtonUsesLockedSprite`; validator zero errors with narrowly documented exceptions. | UNMEASURED 5–8 h |
| 3 | `ShellRoot.cs`, `ScreenHost.cs`, `PopupHost.cs`, `PopupView.cs`, `IGameSessionActions.cs`. | PlayMode `OnlyTopPopupReceivesInput`, `ReopenLowerMovesToTop`, `MandatoryPopupRejectsDismissal`, `TransitionsWorkWhilePaused`, `PopRestoresFocus`. | UNMEASURED 5–8 h |
| 4 | Core check-in snapshot query; `CheckInView/Presenter.cs`, `QuestsView/Presenter.cs`. | EditMode `CheckInSnapshotMatchesClaim`, `MidnightRefreshesOpenDailyViews`, `DuplicateClaimDoesNotGrantAgain`, `ClaimAndWalletPersistTogether`; fake clock/grantor. | UNMEASURED 5–8 h |
| 5 | `MainView/Presenter.cs`, `ShopView/Presenter.cs`, product catalog and host save coordination. | EditMode `MainRefreshesLivesOnResume`, `FailedPurchaseLeavesBalanceUnchanged`, `RepeatedReceiptGrantsOnce`, `SaveFailureIsSurfaced`. | UNMEASURED 6–10 h |
| 6 | `LeaderboardView/Presenter.cs`, `LoadingContentView.cs`; retain existing loading controller/view. | EditMode `LeaderboardRendersEmptyErrorAndRank`; PlayMode `LoadingBlocksInputUntilHidden`, `LoadingProgressIsRendered`; retain named T6 loading cases. | UNMEASURED 3–5 h |
| 7 | `SessionPopupPresenter.cs`, popup descriptors/content for all five popups; pause settings binding. | PlayMode `AllFivePopupFlows`, `LeaveCancelKeepsPause`, `EveryPauseExitRestoresPriorState`; EditMode `ReviveGrantsOnlyOnCompletedAd`, `DoubleClickExecutesOnce`. | UNMEASURED 5–8 h |
| 8 | Localization tables, style-switch binding, `Tests/EditMode/ShellLocalizationTests.cs`, `Tests/PlayMode/ShellVisualTests.cs`. | All six screens/five popups × two themes × supported languages: no missing glyphs, clipping or overlap at 720×1600 and 1080×1920, including notch/gesture insets; theme switch preserves state and stack. | UNMEASURED 5–8 h |

Total: **UNMEASURED 36–59 h**, excluding new art production and live provider SDK integration.

Recorded runs are useful baselines, not UI delivery estimates: T1b EditMode 15 s / total 174 s (`F/REPORT_T1b.md:40`); T6 compile failure 5 s / checkpoint 275 s (`F/REPORT_T6.md:62`); A 5.6 min, B approximately 9 min, C approximately 8 min for their Core tasks (`F/REPORT_A.md:121`, `F/REPORT_B.md:3`, `F/REPORT_C.md:3`). None measures screen building or visual approval.

**4. Top five risks and early detection**

1. **Dependency/import failure before testing.** Smoke manifest now includes audio/UI modules but lacks explicit game-kit, uGUI and Input System entries. Verify package resolution and player assembly compilation in job 1. `F/unity-smoke/Packages/manifest.json:2`; `K/Runtime/UiModal.cs:4`.
2. **Popup stack and pause state diverge.** Test Pause → Leave → cancel, reopening a lower popup, rapid Back, scene teardown and every close path before adding popup content. Current stack-order defect: `K/Runtime/UiModal.cs:60`.
3. **Reward shown twice or lost after restart.** Exercise duplicate clicks, repeated receipts, rejected grants and save/reload across wallet, lives and claims. Host persistence must include the grant ledger and modules together. `F/REPORT_B.md:114`; `F/Packages/com.indiegame.shell/Runtime/Core/Services/ServiceSockets.cs:82`.
4. **“Themeable” passes data checks but fails visually.** Build one representative screen and popup with two sharply different styles first; test long translations, glyph coverage, locked buttons and all button roles. Builder tests can silently ignore missing TMP Essentials. `K/Tests/EditMode/UiBuilderTests.cs:25`.
5. **Fake-service UX cannot accommodate real providers.** Existing sockets are synchronous; add a presenter-facing asynchronous boundary before live SDK work, with cancellation/stale-result tests. Do not block Unity’s main thread around them. `F/Packages/com.indiegame.shell/Runtime/Core/Services/ServiceSockets.cs:23`, `:58`, `:116`.

**5. Contradictions and boundaries**

- “Reuse builder/modal as is” conflicts with runtime construction and configurable dismissal; use the limited reuse described above.
- Game-2 supplies structural ideas only. Its `UiSheet` has hardcoded motion/colors and every open sheet handles Back independently—copying it would regress theme and stack behavior. `G/UiSheet.cs:48`, `:60`, `:93`.
- Game-2’s pause view directly mutates board internals, saves gameplay state, forces timeScale to 1 and loads `"Home"`; it is not a reusable shell presenter. `G/Board2D/PauseSheetView.cs:17`, `:33`, `:46`.
- T6’s “missing modules” diagnosis is historical: the current manifest includes both additions, but that does **not** establish a successful subsequent run. `F/REPORT_T6.md:72`; `F/unity-smoke/Packages/manifest.json:3`.
- Existing decisions support five-slot/two-table check-in and separate modal coordination; they do not authorize importing game-specific decoration, profile or pregame selection. `game-skills/docs/SHELL_COMPARE.md:63`, `:98`, `:101`.
# T29a ShellApp report

Package-side slice implemented and checked on 2026-11-12. No git/GitHub commands, game-2-v2 writes, environment-file access, Main layout changes, or ChromePresenter work.

## Numbered scope

1. **Done.** `Runtime/Unity/ShellAppConfig.cs` adds plain config, `BootResult`, and the single `ShellGameContext` factory seam. Required config/table omissions throw named `InvalidOperationException` from Validate; Boot captures validation errors in its result. Style validation reuses ShellStyle.Validate. Clock defaults to SystemClock. Storage supports the existing concrete SaveStore and its ISaveFileSystem injection. Optional shop supports a session factory or catalog/purchases; leaderboard supports service/board/row count. Missing optional services and unknown enum values produce named navigation errors. Evidence: five new ShellAppConfigTests pass, plus post-boot unavailable-navigation assertions in CheckInTapBackAndCachedNavigation.

2. **Done.** `ShellApp.cs` explicitly boots the Canvas/scaler/raycaster, owned-or-reused EventSystem, ScreenHost, safe popup root, session/localization/flow, game root, legacy Main and presenter, and loading tip. Service screens are lazy and cached. Ready waits for loading to hide. Failed LoadAll returns before any presenter or game factory is constructed; only a localized error Canvas is retained, and Commit refuses writes by returning the failed load result. Unexpected composition errors release owned UI/input. Destruction during loading and while paused restores Boot's captured timeScale and clears owned objects. Repeated Boot throws. Evidence: CorruptedSaveFailsClosedAndPreservesBytes, RepeatedBootThrowsAndExistingEventSystemSurvives, DestroyDuringLoadingAndFactoryFailureCleanUp, DestroyMidPauseRestoresScaleAndObjectBaseline, and InjectedClockControlsLoadingAndBackBarrier pass. Baseline checks cover Canvas, EventSystem and RectTransform, including inactive objects.

3. **Done.** LoadingScreenView.Initialize accepts optional IClock and defaults to SystemClock. Awake preserves default behavior; ShellApp injects its configured clock before showing loading. Evidence: injected-clock test plus all seven pre-existing loading-controller tests and both existing loading-flow tests pass.

4. **Done.** ShellApp.Update owns Escape/Backspace routing. PopupHost.HandlesOwnBackKey defaults true for existing callers and is false under ShellApp. Loading/unready consumes Back, any popup stack consumes it (including mandatory and closing popups), game Back delegates to Flow.ShowPause, service Back returns Main, Main Back is a no-op. Service Back controls use factory.Button and shell.back from both en/vi framework resource tables. Pause/Leave logic remains in SessionPopupPresenter. Optional missing ads report Unavailable instead of dereferencing null. Evidence: real pointer Play/Leave/cancel/confirm, CheckIn/Back, repeated Back during popup closing, mandatory-popup service barrier, and queued keyboard Escape closing only Leave all pass. Non-unit initial scale .4 is checked.

5. **Done.** Navigate, ShowMain, ShowGame and Commit expose shell operations. Commit returns existing SessionResult and uses shared ScreenText.CommitError formatting; retry only commits pending state. ActionsBridge calls Play once then shows Game, and Leave then Main; other game actions keep their original contract. Evidence: OptionalScreensAndCommitRetryDoNotReplayPlay passes with an injected write failure, successful retry, one Play, and lazy Quests/Shop/Leaderboard navigation.

6. **Done.** Five EditMode and eleven PlayMode tests added, with every name registered in expected lists. Pointer tests queue MouseState through InputSystem and the real InputSystemUIInputModule/EventSystem; raycast assertions verify the actual target before each tap. They do not invoke button handlers directly. The keyboard test queues KeyboardState. Fake devices and temporary save files are cleaned up; GameView size selection and prior timeScale are restored by test teardown.

7. **Done.** framework/README.md documents config fields, factory/context, actions, readiness/input contract, navigation, Commit, and ownership. PilotApp's own shell assembly/composition is migration-only, with removal checkpoint T29b. Legacy Main remains; home chrome stays deferred to T28e.

## Validation evidence

- Initial Editor.log `error CS` count: **0**, checked before reading test results.
- First EditMode runner invocation refused because game-5's ForestDropThreshold job owned Unity. Polled pgrep every 30 seconds; it became free within the allowed 25 minutes. No other project's process was killed or bypassed.
- `bash tests/run_unity_smoke.sh --mode EditMode`: **62 passed, 0 failed, expected 62**, comprising all **57 existing + 5 new**; compiler errors **0**; runner/CLI status 0; 15 seconds.
- Then `bash tests/run_unity_smoke.sh --mode PlayMode`: **32 passed, 0 failed, expected 32**, comprising all **21 existing + 11 new**; compiler errors **0**; runner/CLI status 0; 150 seconds.
- Both runners confirm Core noEngineReferences=true and zero UnityEngine matches in Core.
- Evidence: `unity-smoke/Logs/EditMode.xml`, `PlayMode.xml`, `EditMode-Editor.log`, `PlayMode-Editor.log`. Archived logs independently counted at zero compiler errors.
- No task-started Unity smoke process remains after validation.

## AMBIG / compatibility decisions

- The package has no ISaveStore; ShellSession accepts sealed SaveStore. Used that existing injectable store (whose filesystem can be faked), avoiding a forbidden Core edit or duplicate persistence API.
- ShellScreen originally contained only CheckIn, Quests and Shop. Added Leaderboard as an additive enum value in MainView.cs so the requested optional leaderboard can be navigated explicitly. Existing numeric values and all method signatures are preserved; no MainView layout code changed.
- “Boot failure cleanup” and “show localized load error” require retaining a display surface on load failure. That branch retains only the owned Canvas/error label until ShellApp destruction; it creates no presenter, game root or EventSystem. Other composition failures tear down their UI immediately.
- Loading clock injection uses an optional Initialize parameter because Unity creates MonoBehaviours without application constructor arguments. Existing Awake/default callers remain compatible.
- Language data lives under Runtime/Unity/Localization/Resources/ShellLocalization in this package; shell.back was added to these existing en/vi tables.
- BootResult.Success means synchronous composition/load succeeded; Ready becomes true after the loading barrier finishes. Missing config is named in BootResult.Error; calling config.Validate directly throws InvalidOperationException.

## Could not / did not check

- Pilot gameplay integration tests, player/mobile builds, native Back behavior on device, production SDK adapters, and actual Pilot migration were not run. game-2-v2 was read only as instructed. Synthetic mouse/keyboard tests prove Unity's package UI input path, not device hardware.
- Existing screenshot suites passed their automated checks; no new visual redesign or manual screenshot comparison was performed.
- The knowledge graph has no index for this workspace, so source discovery/read fallback was necessary.
- No remaining observed test failure or active blocker. No claims of T29b migration completion.

## Pilot members/contracts relied on remaining compatible

Read PilotApp.cs, PilotSessionAdapter.cs and PilotInputTests.cs from ../game-2-v2. Migration must preserve game-owned StartLevel, LevelIndex, ReviveBoard, ShowMain and Paused behavior used by PilotSessionAdapter.Play/Retry/NextLevel/Revive/Leave/Pause/Resume. NextLevel's final-level ShowMain remains a game decision using context.ShowMain. Pilot tests currently rely on Session, Text, Main, Screens, Popups, Flow, Adapter, BoardView, Board, LevelIndex, Ready, AcceptedSwipes, SavePath, LastSaveSucceeded and T, plus SuppressAutoBoot during setup. None were edited. The future board must retain its Ready/paused/popup/current-screen/loading/won/lost input checks. This package keeps existing presenter constructors, popup flow APIs, ScreenHost.Show, MainView and old ShellScreen values compatible; Pilot's default-to-Shop branch remains solely in its unmigrated implementation pending T29b.

# T13 UI foundation

EXPECTED written before either gate. EditMode: **24** (19 existing + 5 new); PlayMode: **5**.

- `IndieGame.Shell.Tests.ShellUiFactoryTests.BuildsAllPrimitives`
- `IndieGame.Shell.Tests.ShellUiFactoryTests.MissingRoleFailsByName`
- `IndieGame.Shell.Tests.ShellUiFactoryTests.LockedButtonUsesLockedSprite`
- `IndieGame.Shell.Tests.ShellUiFactoryTests.StyleSwitchKeepsTextAndLayout`
- `IndieGame.Shell.Tests.ShellUiDependencyTests.RuntimeAssemblyHasNoEditorReferences`
- `IndieGame.Shell.Tests.Wallet.ValidV1Loads`
- `IndieGame.Shell.Tests.Wallet.CorruptFallsBackToBackup`
- `IndieGame.Shell.Tests.Wallet.FutureReportsVersionError`
- `IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaTopNotch`
- `IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaBottomGestureBar`
- `IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaZero`
- `IndieGame.Shell.Tests.UnityAdapterTests.SafeAreaFullScreen`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingMinimumDisplayTime`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingProgressMonotonic`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingHideBeforeMinimumWaits`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingDoubleShowIdempotent`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingProgressClamp`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingFadeAndLateHide`
- `IndieGame.Shell.Tests.UnityAdapterTests.LoadingNewCycleResetsAndEarlyHideFades`
- `IndieGame.Shell.Tests.UnityAdapterTests.HapticsFlagOff`
- `IndieGame.Shell.Tests.UnityAdapterTests.HapticsFlagOn`
- `IndieGame.Shell.Tests.UnityAdapterTests.HapticsNullPlayerSafe`
- `IndieGame.Shell.Tests.UnityAdapterTests.AudioRoutesKeysAndIndependentVolumes`
- `IndieGame.Shell.Tests.UnityAdapterTests.AudioFlagsGatePoolAndRefreshActiveVolumes`
- `IndieGame.Shell.Tests.PopupHostTests.OnlyTopPopupReceivesInput`
- `IndieGame.Shell.Tests.PopupHostTests.ReopenLowerMovesToTop`
- `IndieGame.Shell.Tests.PopupHostTests.MandatoryPopupRejectsDismissal`
- `IndieGame.Shell.Tests.PopupHostTests.TransitionsWorkWhilePaused`
- `IndieGame.Shell.Tests.PopupHostTests.PopRestoresFocus`

## Result

All required gates pass on Unity 6000.3.25f1:

| Gate | Actual | Expected | Result |
| --- | --- | --- | --- |
| Final EditMode | 24 passed, 0 failed, exact names, no skips | 24 | PASS |
| PlayMode | 5 passed, 0 failed, exact names, no skips | 5 | PASS |
| `error CS` in `~/Library/Logs/Unity/Editor.log` | 0 (also 0 in both final mode snapshots) | 0 | PASS |
| Core `noEngineReferences` | true | true | PASS |
| `grep -rn UnityEngine Packages/com.indiegame.shell/Runtime/Core` | 0 matches, exit 1 | 0 matches | PASS |
| `dotnet test dotnet/Shell.Core.Tests -c Release` | 161 passed, 0 failed, 0 skipped | 161 | PASS |

EditMode attempts: initial 20/24, first rerun 20/24, second/final rerun 24/24. No compilation errors in any attempt. First failure was unavailable TMP font data in Unity's built-in LegacyRuntime font; the next run verified that TMP Settings was also absent. Both failures occurred in setup of the four factory cases, while all 19 existing cases and the dependency case passed. Fixed the test prerequisites with a real OS font and official TMP Essential Resources support under Tests/EditMode/TmpSupport. No tests were skipped, suppressed, or renamed to pass. PlayMode passed on its only invocation. Retry cap respected.

Each launch used the runner's `pgrep -fl 'Unity.app/Contents/MacOS/Unity'` preflight. No Editors overlapped; final process check found no Editor. Compiler-server dotnet processes present initially are not Unity Editors. No task-started background processes remain.

## Delivered behavior

- `ShellStyle` references unchanged GameKit.UI `UiTheme` and adds screen, currency/heart/booster, list-row, progress, title and locked-sprite roles. Validation fails before construction with the missing role's name. Colors are style data, including the additional surface tint.
- `ShellUiFactory` builds runtime uGUI/TMP screen roots, labels, all three button roles, rows, masked scrolling lists and popup frames. Configuration occurs under inactive objects; popup roots stay inactive until opened. Factory code contains no hardcoded colors/sprites, Editor APIs, private-field reflection or prefab saving. Panel clicks are consumed independently of scrim dismissal. Locked buttons use the shell locked sprite and reject input.
- Style replacement validates first, then updates factory appearance bindings in place, including inactive nodes and popup motion. This foundation preserves texts, hierarchy, layout and live state. The two code-generated demos are named `dark flat` and `bright rounded`; the latter uses a procedural corner-cut sprite and contrasting colors. Test font assets are generated from macOS Arial; no authored art is introduced.
- `PopupHost` has one instance-owned ordered stack. Reopening removes/re-adds its entry and moves the sibling to match; it cancels an in-progress close. Only the top group's controls are interactable. Closing keeps the scrim barrier until removal, preventing repeated Back from leaking to lower popups. Escape/Backspace routes through the same `Back()` method. Scrim, view dismissal and Back all enforce `Dismissible`/`Mandatory`. Explicit owner `Close` completes a mandatory decision. Motion uses unscaled delta time and UiTheme motion data. Focus returns to a valid previous control or the revealed popup's initial focus. Disabled/destroyed views unregister; host disable tears down its stack. No UiModal is used.
- `ScreenHost.Initialize` creates the existing SafeAreaFitter under a full-screen parent and a top sibling using the existing LoadingScreenView. `Show` deactivates other hosted screens and restores the loading overlay to the last sibling. Loading stays active so its existing controller can tick.
- Manifest references game-kit via `file:../../../../game-kit/Packages/com.indiegame.ui`, plus explicit uGUI and Input System versions matching game-kit's dependencies. Runtime/test asmdefs reference the actual required assemblies. Game-kit was read only; its files match the pre-import SHA-256 inventory.
- Runner defaults to EditMode and accepts `--mode PlayMode`; it rejects invalid arguments, guards progress/time/process overlap, snapshots each mode log and requires exact test names/counts, zero failures, zero CS errors, clean Core and successful CLI exit.

## Evidence and scope

Commands: `bash -n tests/run_unity_smoke.sh`; `tests/run_unity_smoke.sh` (three sequential attempts); `tests/run_unity_smoke.sh --mode PlayMode` (once); `dotnet test dotnet/Shell.Core.Tests -c Release` (once).

Evidence remains in unity-smoke/Logs: EditMode.xml, EditMode-Editor.log, EditMode-cli.log; PlayMode.xml, PlayMode-Editor.log, PlayMode-cli.log; T13-attempt1-* and T13-attempt2-* preserve failed-attempt XML/logs. These are generated test artifacts. Current Editor.log is the passing PlayMode run. Existing nullable-annotation compiler warnings remain; zero CS errors is verified.

No Core source/asmdef edits, Git/GitHub commands, environment-file access, game-kit writes or deployments. Core hashes match the pre-import inventory after removing two newly generated Core .meta files. Removed the generated UI directory .meta outside UI/** as well. Existing metadata was retained; metadata within allowed UI/tests paths remains.

Unity regenerated packages-lock.json for the new manifest dependencies. As scope cleanup, removed the six entries induced by this task (game-kit UI, uGUI, Input System, uielements, hierarchycore and physics), leaving dependency resolution to the authorized manifest. The original lockfile was not snapshotted, so byte-for-byte restoration of that generated file cannot be verified. This is an explicit scope limitation, not a claim of verified identical contents.

## AMBIG / unverified

- Applied the requested review sections 1, 2 and 4 only to this foundation. Presenters, reward flows, localization catalogs and future Core gaps are outside T13; Core remains untouched. Existing 19 tests passing removes the stale T6 concern.
- In-place appearance updates satisfy the explicit same-text/same-layout switch test; the review's later presenter-driven rebuild strategy is not needed for this foundation. Geometry is intentionally retained during switching. Material/font/color/sprite bindings update, while layouts do not reflow to different style geometry.
- A caller supplies a Canvas/full-screen host parent and an EventSystem with its input module. Popup InitialFocus is optional; owner code may wire action buttons in the optional content slot. A mandatory popup can only be completed through the explicit owner close path.
- Physical keyboard, touch, Android hardware Back, device builds, long translations, glyph coverage, multiple resolutions, clipping and visual approval were not verified. Tests exercise the Back dispatch API, CanvasGroup/Selectable input gating, display/stack ordering, rapid Back, close/reopen interruption, mandatory scrim rejection, paused opening/closing and focus restoration. They do not synthesize physical pointer or keyboard events.
- No six-screen UI or full game flow is claimed. Currency/icon and progress roles are data for future views; no extra progress widget was requested. Optional body/title/button materials use TMP's font material when absent.
- Demo font generation currently expects an OS font path ending in Arial.ttf, appropriate to this measured macOS smoke environment. Production styles must supply their own TMP font. TMP support assets are copied unchanged from the installed official uGUI package's Essential Resources; the LiberationSans OFL is included. They are test prerequisites, not production shell assets.
- Graph discovery was attempted first; game-skills is not indexed, so direct reads were used. No subagents were used.


## Wall-clock minutes

| Step | Minutes | Measurement |
| --- | ---: | --- |
| Requested docs/reference reading and graph fallback | unmeasured | Timer was started after initial reads; no estimate claimed |
| Implementation, initial tests, expected list and runner | 2.180 | Captured timestamps |
| EditMode initial attempt | 0.250 | Runner: 15 seconds |
| EditMode first rerun | 0.100 | Runner: 6 seconds |
| EditMode final rerun | 0.083 | Runner: 5 seconds |
| PlayMode only attempt | 0.083 | Runner: 5 seconds |
| Diagnosis, prerequisite fixes, cleanup and report | 3.377 | Remainder of captured wall time, excluding the above sequential Unity runs |
| Dotnet gate | unmeasured separately | Ran concurrently with initial Unity; 161 passed; test execution duration 102 ms is not wall time |
| Total after initial reading, through report generation | 6.074 | Captured timestamps; below the 60-minute cap |

# T6 Unity adapters

EXPECTED list written before the Unity invocation. Expected total: **19** (16 new + 3 existing).

- `IndieGame.Shell.Tests.SaveSmokeTests.ValidV1Loads`
- `IndieGame.Shell.Tests.SaveSmokeTests.CorruptFallsBackToBackup`
- `IndieGame.Shell.Tests.SaveSmokeTests.FutureReportsVersionError`
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

Run 1: compilation blocked; 21 `error CS` log lines (7 unique CS1069 diagnostics repeated), CLI exit 6, runner exit 1, 5 seconds. No XML / no executed test cases. Core noEngineReferences=true; grep UnityEngine returned no matches (exit 1). Expected 19; actual executed 0.

Verified cause: unity-smoke/Packages/manifest.json enables androidjni but omits com.unity.modules.audio and com.unity.modules.ui. AudioSource/AudioClip and CanvasGroup are forwarded to those missing modules. No manifest or lockfile change has been made; scope approval requested before a retry.

Concrete proposed manifest additions (both inside dependencies):
```json
"com.unity.modules.audio": "1.0.0",
"com.unity.modules.ui": "1.0.0"
```

## Files

- Runtime/Unity/IndieGame.Shell.Unity.asmdef: engine-enabled assembly referencing Core.
- Runtime/Unity/Haptics.cs: IHapticsPlayer, Android-only Handheld.Vibrate adapter, dynamic SettingsService gate; null player is safe.
- Runtime/Unity/AudioService.cs: IAudioService and injectable IAudioSourcePool; bounded AudioSource pool, dedicated looping music voice, independent clamped gains, active-voice refresh, stop/dispose.
- Runtime/Unity/SafeAreaFitter.cs: pure anchor conversion, full-screen child RectTransform fitting, safe-area/resolution/orientation change detection.
- Runtime/Unity/LoadingScreenController.cs: pure IClock-driven state machine with minimum time, fade, monotonic progress and idempotent Show/Hide.
- Runtime/Unity/LoadingScreenView.cs: CanvasGroup binding; updates pure controller each frame.
- Tests/EditMode/UnityAdapterTests.cs: 16 new cases; existing SaveSmokeTests.cs untouched.
- Tests/EditMode/IndieGame.Shell.Tests.EditMode.asmdef: adds Unity assembly reference.
- tests/run_unity_smoke.sh: exact expected set of 19; Core JSON and grep gates added.
- REPORT_T6.md.

Runtime/Unity and Tests/EditMode paths above start at Packages/com.indiegame.shell/; runner and report paths start at framework/. Unity-generated .meta assets under the allowed Unity and Tests directories are retained.

## AMBIG / usage / limits

- Use HapticsGate(settings, new UnityHapticsPlayer()) for gated calls. Non-Android targets are no-ops; no JNI layer is necessary because Handheld.Vibrate provides the requested Android vibration path. Actual vibration requires device validation.
- AudioService is injected with SettingsService and an IAudioSourcePool, so consumers can substitute IAudioService and tests can inspect a fake pool. UnityAudioSourcePool accepts caller-owned clip keys and a parent Transform. Missing clip keys return false, sounds reuse idle slots then steal in round-robin order; one looping music source replaces previous music. Dispose releases only the pool-owned child. Call RefreshSettings after settings changes or each frame to update active voices; playback and volume setters also refresh. No SDK, assets, scene, or dependency library added.
- SafeAreaFitter assumes a full-screen parent RectTransform; zero safe area or nonpositive screen dimensions fall back to full-screen anchors. Anchors clamp to [0,1]. No insets accumulate because offsets reset on each application.
- Minimum display time starts at Show and includes fade-in; Hide waits for both minimum time and fade-in, then fades out. A late Hide starts its fade at the request. Tick is caller-driven; the view calls it from Update. Show while visible/waiting/fading is idempotent; a new cycle resets progress. NaN progress is ignored; infinities clamp. UTC clock regressions are held at the previous tick. Progress rendering is left to the caller through Controller.Progress.
- Expected list was written before run 1. One Editor at a time, pgrep preflight, no APK/device/PlayMode validation. No Git/GitHub commands or environment files accessed. Core source and asmdef were not edited.

## Measured time so far

| Step | Unix interval | Seconds | Minutes |
| --- | --- | ---: | ---: |
| Docs, contracts, graph fallback | 1794416400–1794416418 | 18 | 0.30 |
| Implementation, tests, expected list, runner | 1794416418–1794416552 | 134 | 2.23 |
| Baseline, syntax check, preflight | 1794416552–1794416564 | 12 | 0.20 |
| Unity run 1 | 1794416564–1794416569 | 5 | 0.08 |

| Diagnose missing modules, document proposed fix, verify scope, cleanup | 1794416569–1794416675 | 106 | 1.77 |
| Total through checkpoint | 1794416400–1794416675 | 275 | 4.58 |

## Actual vs expected / final checkpoint

Every one of the 19 EXPECTED cases above is **not executed**: compilation stopped before test discovery/results. Actual list: empty. Passed count cannot be established from XML because no XML exists; executed cases=0, failed tests=unknown. Gate is **BLOCKED**, not passed. Exactly one invocation so far; no retry while dependency-scope approval is pending.

Removed 22 task-generated .meta files outside the allowed Unity/** and Tests/EditMode/** directories (including newly generated Core metadata). Existing metadata was preserved. Core aggregate SHA-256 before and after is identical: `857fd78341654218d7ce724cfd0c271cf62b4f2e9cc17afdd139fcc537174765`. Package snapshot comparison found no changed pre-existing package files during Unity import. Original three tests were preserved. No task-started Editor remains (pgrep returned no matches).

Evidence: unity-smoke/Logs/EditMode-Editor.log, EditMode-cli.log, test-start.txt and test-wall-seconds.txt (5 seconds). Runner shell syntax check passed. Runtime behavior is unverified until the missing built-in dependencies are enabled and a retry passes. Awaiting permission only for the manifest/lockfile scope extension; no extra project files have been edited.

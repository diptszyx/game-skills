# T6: Unity layer: adapters for haptics, audio, safe area, loading screen (one Unity run)

Repo /Users/tho/project/indie-game/game-skills/framework. Read first: framework/README.md, REPORT_T1b.md, tests/run_unity_smoke.sh, Core/Settings (haptics/sound flags) and Core/Contracts.
Files (only): Packages/com.indiegame.shell/Runtime/Unity/** (new asmdef IndieGame.Shell.Unity referencing IndieGame.Shell.Core, normal Unity references allowed), Packages/com.indiegame.shell/Tests/EditMode/** (new tests; keep the 3 existing), tests/run_unity_smoke.sh (update expected count), REPORT_T6.md. No git. Core is not edited.
Build:
1. IHapticsPlayer + UnityHapticsPlayer (Android vibration via Handheld.Vibrate / AndroidJNI guarded by RuntimePlatform; no-op elsewhere), gated by the Settings haptics flag through a small pure-C# HapticsGate in Core-free Unity assembly or in Runtime/Unity.
2. AudioService: plays clips by key through an AudioSource pool, separate sound/music volume gated by Settings flags, interface IAudioService so tests use a fake.
3. SafeAreaFitter MonoBehaviour: applies Screen.safeArea to a RectTransform (anchors), recomputes on orientation/resolution change; pure function SafeAreaMath.ToAnchors(Rect safe, Vector2 screen) separately unit tested (notch top, bottom gesture bar, zero safe area, safe == full screen).
4. LoadingScreenController: state machine (Show/Progress/Hide, min display time via IClock, fade) with pure-C# logic tested with a fake clock; progress clamps 0..1 and never goes backward.
Tests (EditMode, EXPECTED list written in REPORT_T6.md before the run): SafeAreaMath 4 cases, Loading state machine >= 5 cases (min display time, progress monotonic, hide before min time waits, double show idempotent, clamp), HapticsGate 3 cases (flag off => no call, on => one call, null player safe), Audio fake pool 2 cases, plus the 3 existing.
Run: only one Unity at a time (pgrep first). Run tests/run_unity_smoke.sh ONCE after updating the expected count; fix and rerun at most twice. Gate: error CS == 0, failed == 0, passed == expected, AND check that the Core asmdef has noEngineReferences true and Core has no UnityEngine usage (grep). Cap 40 minutes.
Report: files, expected vs actual list, error CS count, minutes per step measured, AMBIG.

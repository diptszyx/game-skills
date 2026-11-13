# Eval: using-game-shell (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 28 lines, references/gates.md 17 lines.
Result: 4 broken/stale references, 3 missing-content gaps (the skill predates ShellApp, ShellKit, GameViewportView, GameHudView).
Verified true: `framework/README.md`; `docs/SHELL_SPEC.md`; `ShellSession` + `Commit()` (`Core/Session/ShellSession.cs`); `IRewardGrantor.Grant` (`Core/Contracts/Reward.cs`, implemented by EconomyService); `IClock` (`Core/Contracts/IClock.cs`); `IGameSessionActions` with exactly Play, Retry, Revive, NextLevel, Leave, Pause, Resume (`Unity/Screens/MainView.cs`); `ShellStyle`; `Runtime/Unity/Screens` has view + presenter pairs; Core asmdef has `noEngineReferences: true`; `grep UnityEngine Runtime/Core` count = 0; `tests/expected_<Mode>.txt` and `tests/run_unity_smoke.sh`; `../game-kit` exists; `dotnet build/test` paths `dotnet/Shell.Core`, `dotnet/Shell.Core.Tests`; 168 dotnet tests (115 [Fact] + 53 [InlineData]); `SmokeBuild.BuildAndroid` exists in `unity-smoke/Assets/Editor/SmokeBuild.cs` (IL2CPP, ARM64, `Builds/smoke.apk` is 17.0 MiB = 17.8 MB, matches); claim-id examples not re-derived from code (not checked: `checkin:<date>`, `quest:<date>:<id>`, `iap:<transactionId>` formats; grep before relying).

Broken / stale (current value in bold):
1. `references/gates.md`: "EditMode, 41 tests" -> **71 names in tests/expected_EditMode.txt** (71 [Test] attributes).
2. `references/gates.md`: "PlayMode (20 tests)" -> **36** (expected_PlayMode.txt, 36 [Test]/[UnityTest]). The "about 5 s" and "about 10 s" timings were measured at 41/20 tests; mark as "measured 2026-11-12 at 41/20 tests, not re-measured" or re-measure (needs Unity; not run here).
3. Rule 6 "The game attaches only through `IGameSessionActions`": incomplete. A game boots with `ShellApp.Boot(new ShellAppConfig{...})` (README "Booting a game with ShellApp"): required `Style`, `LanguageTables`, `SavePath`, `Lives`, `Economy`, `CheckIn`, `Quests`, `Settings`, `RemoteConfig`, `SessionPopups`, `Actions`, `GameScreenFactory`; the factory gets `ShellGameContext` (Factory, Session, Text, Popups, Flow, Parent, Navigate, ShowMain) and returns one game RectTransform. Pilot's own assembly is migration-only (removal checkpoint T29b).
4. Rule 5 "look = `ShellStyle`": new slots `GearButton`, `BoardFrame` are required by `ShellStyle.Validate()`; `ShellKit.Apply(style, spriteLookup, ShellKitPalette, adopt)` is now the single place mapping a game's art file names onto all slots (T31). Not mentioned: a new game should call ShellKit, not copy slot lines.
Missing content:
5. `GameHudView` (board frame around the game, optional frame drawing) and `GameViewportView` (RenderTexture camera, world offset 10000, `Camera`/`Content`) are the way gameplay is shown inside the shell; add to rule 6 / "Adding something".
6. Step 4 "Add the new test names to `tests/expected_<Mode>.txt`": true, but also say the file is a snapshot of a green run (gates.md says so).
7. "Screenshots at 720x1600 and 1080x1920 (plus notch inset)": true; a third 1080x2340 config exists in LocalizationScreenshotTests.

Proposed replacement for rule 6:
```
6. **A game attaches through `ShellApp.Boot(ShellAppConfig)`**: it supplies `Style` (fill it with `ShellKit.Apply` from the game's art kit), the string tables, `Actions` (`IGameSessionActions`: Play, Retry, Revive, NextLevel, Leave, Pause, Resume) and a `GameScreenFactory` that builds the game under `ShellGameContext.Parent`. The game draws inside `GameHudView` / `GameViewportView`. Game-specific screens live in the game, not in the shell. Details: `framework/README.md` "Booting a game with ShellApp".
```
Replace in gates.md: "(EditMode, 41 tests, about 5 s after import)" -> "(EditMode, 71 tests)"; "(20 tests, about 10 s)" -> "(36 tests)"; keep timings only after re-measure.

## (b) Scenarios
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| S1 | "In the shell package add a weekend double reward to daily check-in." | yes | (1) searches existing DailyCheckIn module first; (2) change through ShellSession then one Commit; (3) grant via IRewardGrantor with a stable claim id; (4) time via IClock, tests for clock backward and midnight; (5) amount in config, text keys in en and vi; (6) Core stays UnityEngine-free, runs dotnet test then both Unity modes, updates expected_*.txt |
| S2 | "Start a new game on the shell: I have my own gameplay scene, how do I plug it in?" | yes | (1) ShellApp.Boot with ShellAppConfig; (2) GameScreenFactory/ShellGameContext; (3) IGameSessionActions; (4) style via ShellStyle/ShellKit, not literals; (5) never copies shell code or edits game-kit. (Tests the part the skill currently misses.) |
| S3 | "What is the difference between a struct and a class in C#?" | NO | no activation |
Caveat from docs/SKILL_EVAL.md: S1/S2 wording "shared shell package" helps triggering; write a variant without that phrase ("add a quest that gives 50 gold") to test trigger on the real use case.

## (c) Description
States WHEN: yes (starting a new game on the shell, adding screen/popup/reward/product/quest, wiring gameplay events, changing the shell) plus "Not for gameplay rules, art generation, non-shell games". Verdict: OK. Optional tweak to include booting: "...starting a new game on the shell (ShellApp boot), adding a screen, popup, reward...".

## (d) Size
28 lines (<500). OK.

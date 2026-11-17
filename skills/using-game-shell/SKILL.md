---
name: using-game-shell
description: How to build or extend a hybrid-casual Unity game on the shared shell package com.indiegame.shell (save, economy, lives, daily check-in, daily quests, settings, localization, notifications, shop, leaderboard, screens and popups). Use when starting a new game on the shell, adding a screen, popup, reward, product or quest, wiring gameplay events to the shell, or changing the shell itself. Not for gameplay rules, art generation, or games that do not use the shell.
---

# Using the game shell

The shell is the shared "everything around the gameplay". A game supplies gameplay, data (config, string tables, style) and one adapter; it never copies shell code. Map and commands: `framework/README.md`; decided rules: `docs/SHELL_SPEC.md`.

## Rules (each one exists because the opposite already went wrong)
1. **Search the shell first.** If a module, screen or popup already does the job, extend it; do not write a second path.
2. **All state changes go through `ShellSession`, then one `Commit()`.** Modules saved separately can lose a reward or double-claim after a crash. Surface a Commit error to the player; never ignore it.
3. **Rewards only via `IRewardGrantor.Grant(claimId, reward)`.** A claim id is stable per source (`checkin:<date>`, `quest:<date>:<id>`, `iap:<transactionId>`). Same id grants once.
4. **Time only via `IClock`, UTC.** Tests use a fake clock; test the clock going backward and midnight rollover.
5. **No literal UI text, color, sprite or reward amount in code.** Text = localization keys in en and vi tables; look = `ShellStyle`; amounts = config. A new key must exist in every table (the EditMode key test fails otherwise).
6. **A game attaches through `ShellApp.Boot(ShellAppConfig)`**: it supplies `Style` (fill it with `ShellKit.Apply` from the game's art kit, never copy slot lines), the string tables, `Actions` (`IGameSessionActions`: Play, Retry, Revive, NextLevel, Leave, Pause, Resume) and a `GameScreenFactory` that builds the game under `ShellGameContext.Parent`. The game draws inside `GameHudView` / `GameViewportView`. Game-specific screens live in the game, not in the shell. Details: `framework/README.md` "Booting a game with ShellApp".
7. **Core stays pure C#.** `grep -rn UnityEngine Runtime/Core` must print 0 and the Core asmdef keeps `noEngineReferences: true`.
8. **Do not edit `game-kit` or any other game.** Reference it read-only.

## Adding something (order)
1. Write the behavior list and edge cases first (duplicate claim, failed save, negative or overflow amount, clock back, corrupt section). Put them in the brief or test names before code.
2. Core part with `dotnet test` tests; prove two tests can fail by breaking the logic.
3. View + presenter in `Runtime/Unity/Screens`; presenters call the session, inspect the result, `Commit()`, re-render.
4. Add the new test names to `tests/expected_<Mode>.txt` and run both Unity modes.
5. Screenshots at 720x1600 and 1080x1920 (plus notch inset), both demo styles, en and vi with long strings; look at the montage and list defects. Tests passing does not prove the screen looks right.

## Gates before saying done
`dotnet build` 0 warnings; `dotnet test` green; `error CS count: 0`; EditMode and PlayMode `passed == expected`; Core isolation checks. Only one Unity at a time. Details and measured times: `references/gates.md`.

## Rewarded ads (LevelPlay)
- Game code never touches the ad SDK: the shell calls `IRewardedAdService.Show(placementId, onDone)` (async, `onDone` exactly once). Tests use `FakeRewardedAdService`.
- Real ads: add package `com.indiegame.shell.levelplay` (it pulls `com.unity.services.levelplay`), build `LevelPlayAdsSettings` (AppKey, RewardedAdUnitId), call `new LevelPlayAds().Create(settings)` once at boot and put the returned service in `ShellAppConfig.RewardedAds`. A hidden ticker drives load retries.
- Placement ids from the shell config (`RevivePlacement`, `DoublePlacement`) must exist on the LevelPlay dashboard. The placeholder key `TEST_KEY_REPLACE_ME` is rejected in release builds. Real key and on-device test come from the dev lead; Editor mock ads only work with an Android or iOS build target.

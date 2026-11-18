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
6. **A game attaches through `ShellApp.Boot(ShellAppConfig)`**: it supplies `Style` (fill it with `ShellKit.Apply` from the game's art kit, never copy slot lines), the string tables, `Actions` (`IGameSessionActions`: Play, Retry, Revive, NextLevel, Leave, Pause, Resume) and a `GameScreenFactory` that builds the game under `ShellGameContext.Parent`. The game draws inside `GameHudView` / `GameViewportView`. Game-specific screens live in the game, not in the shell. To put animated characters or props on Home, set the optional `ShellAppConfig.HomeStageFactory` (returns a stage whose `sizeDelta` is the background picture's design size; the shell fits it to the background, behind Home content, chrome and popups, with no raycasts) instead of baking them into `ScreenBackground`. Details: `framework/README.md` "Booting a game with ShellApp".
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

## Rewarded ads (AdMob)
- Game code never touches the ad SDK: the shell calls `IRewardedAdService.Show(placementId, onDone)` (async, `onDone` exactly once). Tests use `FakeRewardedAdService`. AdMob has one ad unit per format, so the placement id is ignored by the adapter.
- Real ads: add package `com.indiegame.shell.admob` (needs `com.google.ads.mobile` from the OpenUPM scoped registry `scopes: ["com.google"]` in the game's manifest), then `cfg.RewardedAds = new AdMobAds().Create(new AdMobAdsSettings { RewardedAdUnitId = ..., AllowTestAdUnit = true })` at boot; a hidden ticker drives load retries. Dispose the `AdMobAds` owner on shutdown.
- Dev: use `AdMobAdsSettings.TestRewarded` (Google's public sample unit) with `AllowTestAdUnit`; release builds reject it. Real unit id and the AdMob App ID come from the dev lead. The App ID must be set in Unity (Assets > Google Mobile Ads > Settings) before an Android/iOS build, or the app crashes at start; code does not set it.

## Leaderboard (Unity Leaderboards)
- Quests: the shell ships `QuestPack.Standard(baseGold)` and records `QuestEvents.Win` / `QuestEvents.Play` itself on the Win/Lose popups; do not call `RecordEvent` for those. Custom quests: pass `quests:` to `ShellAppConfig.Standard` and add `shell.quest.<id>` keys to the game language tables; extra event keys are recorded by the game via `Session.DailyQuests.RecordEvent`.
- Check-in: the shell ships `CheckInPack.Standard(checkInGold)` (7 days, escalating gold, day 7 adds a heart) via `ShellAppConfig.Standard`; custom cycle: pass `checkIn:` with `CheckInPack.Days` rewards per table. Claiming is tapping today's Day cell (`DayCellView` is the button; there is no claim button); the claim path is `CheckInPresenter` only.
- Shop: the shell ships `ShopPack.Standard(shopGold, shopPrice, maxHearts)` (`hearts` for gold, IAP `gold_s/m/l`; IAP price is informational minor units, not shown) via `ShellAppConfig.Standard`. Custom catalog: pass `shop:` and add `shell.product.<id>` keys to the game language tables. Shop holds no per-product saved state, so changing the catalog never breaks an old save.
- `ShellAppConfig.Leaderboard` is an `ILeaderboardRequests` (async). Tests wrap the sync fake: `new LeaderboardRequests(new FakeLeaderboardService(id, state))`.
- Real: add package `com.indiegame.shell.leaderboards` and set `Leaderboard = UnityLeaderboards.Create()`. It signs in anonymously by itself. The Unity project must be linked to a Unity Cloud project and the board created on the Unity Dashboard (the `BoardId` must match).
- The shell does not submit scores by itself: the game calls `Leaderboard.SubmitScore(boardId, score)` when a run ends.
- Look: rows are three columns (rank, name, score) with thin dividers; ranks 1-3 use `ShellStyle.LeaderboardGoldColor/SilverColor/BronzeColor`, the player's own row is a pill (`LeaderboardOwnRowColor`, `LeaderboardOwnRowTextColor`) highlighted in place, or pinned under the list with rank `-` when unranked. The score icon is the style icon `scoreIcon` (add it to `ShellStyle.Icons`); without it only the number shows. Optional one-line footer: `config.LeaderboardCaption` (localization key or plain text, e.g. "Score = levels cleared"). Names come from `LeaderboardEntry.PlayerName`; a blank or id-like name shows `Player` + last 4 characters of the id.

## Boot a new game with `Standard`
Write a small `<game>-game.json` (`ShellGameConfig`: only `gameId` is required, other fields default), then `shell.Boot(ShellAppConfig.Standard(ShellGameConfig.Parse(json), style, languageTables, actions, gameScreenFactory, savePath))`. Override real services after: `cfg.RewardedAds = ...`, `cfg.Leaderboard = ...`. Do not copy the long manual config from another game. Example: `game-2-v2/Assets/SumTen/SumTenApp.cs`.

## Real-input tests (`com.indiegame.shell.testkit`)
Use `new RealPointer(mouse).Click(button)` (add the package to the game manifest and to `testables`); never copy a click helper. It waits for state (button raycastable, popup transition settled), not fixed sleeps, and fails with the button path and input state. Known: about 1 run in 10 had an unexplained missed click before the settle fix; the failure message now prints focus and InputSystem state, so capture it before guessing.

## Layout
`ShellLayout.Standard` owns button (280×80), popup action (up to 400×76), row (600×120), icon sizes and safe-area Back position. Theme button sizes are ignored; never mutate Theme assets at boot. Factory objects carry `LayoutCategory` for geometry checks independent of sprites. Panel sizes and popup height limits, label/title sizes and gameplay tile overrides remain caller/style owned. `ShellStyle.Validate()` checks button borders in UI units (sprite border pixels / pixelsPerUnit).

## Icons
Actions use `ShellUiFactory.IconButton`: `IconButton` (64×64), `IconAction` (140×72), or `IconAmount` (160×72). Round controls expand their raycast hit area to at least 96×96 without changing visual geometry. Main Play and the five tab labels keep words; other action text is restricted to amounts or x2. Supply a localized accessibility name; `ActionAccessibility` records the semantic key and localized label and names the GameObject. Find controls by that key, never their translated display name. Back sits at safe-area top-left (16, −16). Styles can override `ShellStyle.Icons` (key/Sprite entries); omitted keys use cached runtime procedural glyphs, with no art files generated. Action colors come from the style. Session modals fit content within style limits (240–1200 by default). Toggle keys end in `_on` / `_off`; preserve the shell's settings actions, persistence, and Home leave-confirm flow.

## Popup art and actions (0.2.0)
The panel sprite's top 9-slice border defines its title zone, including the rim. Titles centre vertically in that rendered zone when it fits the minimum font line height; absent or insufficient borders retain the 104 band / 8 margin. TitleTextRegion samples contrast only: place it over the real header and choose a readable TitleColor.
Set RoundButtonSprite (Simple) and PillButtonSprite (Sliced) to supply art-kit button shapes; null retains procedural defaults. Custom surfaces render white; ActionSurfaceColor tints only fallback shapes, and ActionIconColor still tints glyphs. One to three icon actions form a centred equal-size horizontal row using ShellLayout spacing; labelled actions and larger sets remain stacked. Pause's toggle row is unchanged. Only the top popup draws; the lower view is restored on close.
- Tab screens: with home chrome, Check-in/Quests/Shop/Leaderboard open as popups over Home (`TabModals` on the shared `PopupHost`; `ShellStyle.TabsAsModals`, default true, false = flat pages with Back). The tab bar stays visible and highlights the open tab, Back/Escape closes one layer, swipe switches the open modal to the neighbouring tab. `ShellApp.Navigate(screen)` is the only way to open them; views take an optional `PopupView modal`. The modal panel fits its content (header + fixed-height rows + bottom inset), centred, clamped by `ShellStyle.Modal` (`WidthFraction`, `MinHeightFraction`, `MaxHeightFraction`, fractions of the area above the tab bar) and scrolls beyond the maximum; custom modal content calls `PopupView.FitModal(contentHeight)`. Check-in shows five equal day cells plus a full-width claim; Leaderboard highlights the player's own row as a pill (see the Leaderboard section).

## Online leaderboard checklist
Add `com.indiegame.shell.leaderboards` to the manifest and asmdef, link the project to Unity Cloud, create the board (id = `gameId`, desc, keepBest), set `config.Leaderboard = UnityLeaderboards.Create()`, and keep a test override for the fake. Headless steps (`unity projects link cloud`, `ugs deploy`): `game-skills/docs/ADOPT_IN_NEW_GAME.md` section "Online leaderboard".

Press feedback: build buttons through `ShellUiFactory.Button`/`IconButton`, or call `factory.WithPressFeedback(button)` on a hand-made one; never set a per-presenter tint or scale. Values are `ShellStyle.PressScale`, `PressDarken`, `PressSeconds`. Scrims and panel input blockers intentionally have none.

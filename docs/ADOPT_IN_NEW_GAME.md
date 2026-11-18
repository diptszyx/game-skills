# Adopt the shell in a new game (game-5 first)

Updated 2026-11-17. Tool state: commit 0280c9d on origin/main. Gates green: shell EditMode 138, shell PlayMode 46, Core 168, game-2-v2 PlayMode 18/18.

## What the game writes (everything else comes from the shell)
1. Gameplay + `IGameSessionActions` (Play, Retry, Revive, NextLevel, Leave, Pause): the game screen factory returns the gameplay root.
2. `<game>-game.json` (`ShellGameConfig`, only `gameId` required).
3. Language tables (en, vi), art kit -> `ShellStyle` via `ShellKit.Apply`. No art yet = plain style, icons are drawn by the shell.
4. One boot call: `shell.Boot(ShellAppConfig.Standard(ShellGameConfig.Parse(json), style, languageTables, actions, gameScreenFactory, savePath))`. Example: `game-2-v2/Assets/SumTen/SumTenApp.cs`.

## What the game must NOT do
- No own Pause, Home, Back, Settings, shop, check-in, quest or leaderboard screens or buttons: the shell owns them (icon-first: actions are icons, text is information).
- No sizes or positions for shell UI: `ShellLayout.Standard` owns them. Style changes only look (sprites, colours, fonts, icons).
- No ad SDK calls: use `cfg.RewardedAds`.

## Steps
1. Add packages to the game manifest as `file:` paths: `com.indiegame.shell`, `com.indiegame.shell.testkit` (+ `testables`), optional `com.indiegame.shell.admob` (needs OpenUPM scoped registry, scope `com.google`) and `com.indiegame.shell.leaderboards`. Copy the manifest lines from `game-2-v2/Packages/manifest.json`.
2. Write the four items above. Start with fake ads and fake leaderboard (defaults).
3. Run EditMode, PlayMode, and the capture test pattern from `game-2-v2/Assets/Tests/PlayMode/PilotScreenCaptureTests.cs` (screenshots both ratios, both languages; assertions on overlap, size consistency, structure).
4. Real ads later: AdMob test ids first (`AdMobAdsSettings.TestRewarded`), real unit id and App ID from the dev lead.

## Known limits (not blockers)
- Flat style without home chrome (game-2-v2 `PilotStyle`) shows the shell's nav icons stacked with no labels; chrome styles (tab bar) are fine.
- Real leaderboard and real ads are unverified on device beyond the Google test ad.
- Flaky real-pointer clicks: not fully explained; diagnostics exist in `RealPointer`.
- Icons are procedural placeholders until a game supplies art.

## Prompt for the new session
Read `game-skills/skills/using-game-shell/SKILL.md` and `game-skills/docs/ADOPT_IN_NEW_GAME.md`, then add the shell to game-5 (`/Users/tho/project/indie-game/game-5/unity/HoleItClone`). Game-5's own rules are in `game-5/CLAUDE.md`. Do not edit `game-skills`; report tool problems in `game-skills/docs/` for the tool session.

## Popup art kits (0.2.0)
Set the panel sprite's top 9-slice border to cover its title header (including the rim). The shell centres the title within that rendered border when it fits the font's minimum line height; unusable or absent borders keep the legacy 104/8 geometry. Set TitleColor and TitleTextRegion to validate contrast over the actual header; sampling regions do not position text.
Supply ShellStyle.RoundButtonSprite for Simple round controls and PillButtonSprite for Sliced action pills. Null uses procedural defaults. Custom button surfaces render white; ActionSurfaceColor tints fallback shapes and ActionIconColor tints glyphs. One to three icon actions share an equal-size centred row; labelled actions and larger sets stack. Lower popups stay hidden until they return to the top.

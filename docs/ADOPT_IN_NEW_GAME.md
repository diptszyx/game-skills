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

## Shop
`ShellAppConfig.Standard` sells the shared `ShopPack.Standard(shopGold, shopPrice, maxHearts)`: `hearts` (refill, costs `shopPrice` gold) and IAP packs `gold_s`, `gold_m`, `gold_l` (x1, x5, x12 `shopGold`), sorted gold products first, then by price. Names are `shell.product.<id>` in the shell tables; only override them in the game table if the shell wording clashes with the theme. Own catalog: pass `shop:` (a `ProductCatalog`) and add `shell.product.<id>` keys. Standard uses the fake purchase service (50 scripted successes) until a real provider is wired.

## Check-in
`ShellAppConfig.Standard` uses the shared `CheckInPack.Standard(checkInGold)`: a 7-day cycle, gold on days 1-6 rising `checkInGold` x1..x6, day 7 `checkInGold` x10 plus 1 heart. Tapping today's Day cell claims (there is no separate claim button; locked and collected cells are not interactive). Own cycle: pass `checkIn:` (a `DailyCheckInConfig` with two reward tables of `CheckInPack.Days` rewards each). A save from the old 5-day cycle keeps loading; a saved slot past the cycle resets quietly instead of failing boot.

## Tab screens
With home chrome (tab bar) the four tab screens open as popups over Home by default (`ShellStyle.TabsAsModals = true`). Do not build your own; Home stays behind, the tab bar stays visible, Back/Escape/scrim/X close it, tab taps or a horizontal swipe switch to the neighbouring tab modal (Home tab or swiping past the first tab closes it). Set `TabsAsModals = false` for flat pages with a Back button. Panel art comes from the style's popup panel sprite (`Theme.PanelSprite`, `PanelHeaderRows`). The panel fits its content (header + rows + bottom inset) and is centred; tune `ShellStyle.Modal` (`WidthFraction` .894, `MinHeightFraction` .30, `MaxHeightFraction` .84 of the area above the tab bar; defaults measured from the original game's panels: width 965/1080 Settings, height .30 = title 178 + info panel 410 of 1920, max .84 = 1618/1920 DailyChallenge). More rows than fit scroll inside the panel. A game never sizes the modal itself.

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

Optional Home decoration: set `config.HomeStageFactory = ctx => ...` to place animated characters or props on Home. Return a stage `RectTransform` whose `sizeDelta` is the design size of the `ScreenBackground` picture and position children in that design space (centre pivot); the shell registers it to the background at every aspect ratio, keeps it behind the Home content, tab bar and popups, and disables raycasts. Example: `game-5/unity/HoleItClone/Assets/Scripts/ShellApp/ForestHomeStage.cs`. If the style's `ScreenColor` tints the background, tint the layers with `ctx.Style.ScreenColor` too.

## Online leaderboard (headless setup, verified on game-5 2026-11-18)
1. Add `com.indiegame.shell.leaderboards` to the manifest (`file:` path; Authentication and Leaderboards come transitively). Reference `IndieGame.Shell.Leaderboards` from the game's shell-app asmdef and set `config.Leaderboard = UnityLeaderboards.Create();` after `ShellAppConfig.Standard(...)`. Keep a static override so PlayMode tests inject the fake and need no network.
2. Link Unity Cloud without the Editor: `unity projects add <proj>` then `unity projects link cloud --cloud-org <org> --non-interactive <proj>` (writes `cloudProjectId` in `ProjectSettings.asset`). Install the UGS CLI once with `unity plugin install ugs`.
3. Create the board: write `<BoardId>.lb` (`SortOrder`, `UpdateType: keepBest`, `Name`; the board id is the file name and must equal `game.gameId`), run `ugs login` (uses the Hub session), `ugs config set project-id <id>`, then `ugs deploy <dir> -e production`. Verify with `ugs lb get <BoardId> -e production`.
4. The shell never submits scores itself; persist the best score in a game save section and submit it on win and on boot (see `game-5/.../LeaderboardProgress.cs`). Failures return a message, never an exception. Pricing is "free for a limited time"; to turn off, stop assigning `config.Leaderboard` (the Standard fake stays).

Button press feedback is automatic: `ShellUiFactory` adds `PressFeedback` to every button it builds (also tab bar and settings gear). Tune with `ShellStyle.PressScale` (.94), `PressDarken` (.88), `PressSeconds` (.12); Validate rejects scale or darken outside (0,1]. A button you create yourself with `AddComponent<Button>` gets none unless you call `factory.WithPressFeedback(button)`.

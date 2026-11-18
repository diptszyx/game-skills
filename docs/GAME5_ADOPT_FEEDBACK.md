# Shell tool problems found adopting the shell in game-5 (2026-11-17)

Source: `game-5/docs/SHELL_ADOPT_REPORT.md`. game-5 worked around each one on the game side; nothing in game-skills was edited. For the tool session.

1. **Backdrop hides a 3D game.** `ShellApp.Boot` builds a ScreenSpaceOverlay canvas with an opaque `Backdrop` image (`ScreenColor`). A game that renders with a world camera is hidden even when GameRoot is transparent. Workaround: toggle `Backdrop.enabled` from `Screens.Shown`; the factory returns `ShellUiFactory.Node`, not `ScreenRoot`. Wanted: a supported way to make the game screen see-through.
2. **Repeated Pause unfreezes the game.** Repro: Play, Pause, Resume or Leave, Play, Pause again. Test failure: `Expected: 0 But was: 1.0f` (timeScale). Cause: destroying the cached old Pause popup runs its `PopupExit.OnDestroy`, and `RestorePause` resumes the new pause. Workaround: clear `Exited` on inactive cached popups after their exit ran.
3. **Nullable warnings on a fresh compile.** `warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.` Example: `Runtime/Core/Save/SaveStore.cs(86,55)`.
4. **Flat Main (EnableHomeChrome=false) has no Settings, language or Leaderboard entry.** It shows stacked icon-only gift/check/coin nav. Matches the known limit in ADOPT_IN_NEW_GAME.md, but Settings and language being unreachable from Main is worse than "no labels".
5. **Main shows placeholder text and stats rows** ("Welcome to your extraordinary adventure and daily rewards hub", Gold / Hearts / Full) as plain text; not usable without an art kit.
6. **Doc gap:** ADOPT_IN_NEW_GAME.md does not say a 3D game needs items 1 and 2 above, nor that the game assembly needs direct refs to `GameKit.UI` (UiTheme) and its font assembly.

## Added 2026-11-18 (style pass, game-5 `docs/SHELL_STYLE_REPORT.md`)
7. **Flat Main never instantiates Mascot/Logo** when `EnableHomeChrome=false`; an assigned mascot stays unused, so a game with its own home art cannot show it without chrome.
8. **Style data cannot remove the Main placeholder text** ("Welcome to your extraordinary adventure..."), stacked icon nav, or change the flat Main layout; no key hides them.
9. **Popup title crowds the wood rim.** Default `TitleTextRegion` sits on the upper rim of a framed panel; had to be moved by style data per art kit.
10. **Mandatory popups draw a baked close badge** (non-functional X) when the panel sprite includes one; a second no-X panel field exists but must be set separately.
11. **Validate() samples fixed regions only**; it does not prove contrast across an illustrated Main screen.
12. **UNVERIFIED (confirm from the A4 report):** with chrome on, GameHudView may draw a "Board frame" in the middle of the game screen** (expected; see A4 report), which covers a 3D game; needs a supported hide switch.

## Added 2026-11-18 (rekit pass, game-5 `docs/SHELL_REKIT_REPORT.md`, owner review of the Lose popup on phone)
13. **RESOLVED (0.2.0): popup title placement.** The title is vertically centred in the panel sprite's top 9-slice border, converted to UI units. Borders too small for the font's minimum line height retain the legacy 104 band / 8 margin. No game layout field is needed; contrast regions remain sampling data.
14. **RESOLVED (0.2.0): popup icon action rows.** One to three IconButton/IconAction/IconAmount actions share a centred equal-size horizontal row with ShellLayout spacing. Labelled text actions and more than three icon actions stay stacked; Pause toggles keep their existing row. Accessibility keys, focus and pointer lookup are preserved.
15. **RESOLVED (0.2.0): art-kit button shapes.** Optional ShellStyle.RoundButtonSprite (Simple) and PillButtonSprite (Sliced) replace the procedural shapes. Null retains defaults; custom surfaces render white, while ActionSurfaceColor tints fallback shapes and ActionIconColor still tints glyphs.
16. **RESOLVED (0.2.0): stacked popup rendering.** Only the top popup draws; opening Leave hides Pause and closing Leave restores it without changing stack or dismissal rules.
17. **Style Validate() does not check that popup text clears the panel's top rim**; a captured screenshot was the only way to see items 9 and 13. A rendered check of title bounds against the panel's inner rect would catch it.

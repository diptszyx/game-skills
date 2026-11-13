# Eval: building-game-ui (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 21 lines, no references/.
Result: 2 broken/stale references, 2 soft gaps.
| Item | Status |
|---|---|
| `ShellUiFactory` | exists (`framework/Packages/com.indiegame.shell/Runtime/Unity/UI/ShellUiFactory.cs`) |
| `PopupHost` | exists; one stack, only Top dismissible, `DismissalPolicy.Mandatory` rejects outside tap and Back (`RequestDismiss`), `Time.unscaledDeltaTime` used: rule 5 is TRUE |
| `using-game-shell` skill | exists |
| `game-ui-workflow` skill | exists (72 lines, 12.8 KB, Vietnamese, covers game-kit `com.indiegame.ui`: UiBuilder/UiModal/UiValidator). Overlaps rules 3, 4, 7 with this skill; rule 9 handles it by precedence. No duplicate file, but the two should be merged or cross-linked once the game-kit path is retired |
| "132 screenshots (11 screens and popups x 2 styles x 3 sizes, en and vi)" | true: 11 views x 2 styles x 2 languages x 3 configs = 132 (REPORT_T16.md/T17.md). Configs: 720x1600, 1080x1920, 1080x1920 with notch (a 4th 1080x2340 exists in code) |
| "cropped titles on every screen in first pass; fixing wrapping inside the views removed them" | true: T17 (`LocalizedLayout.Fit`) |
| "9:16 and 9:20" | matches 1080x1920 / 720x1600 |
| Safe area root | `SafeAreaFitter.cs` exists |
| BROKEN/STALE 1 | Rule 1 "a style asset (sprites, colors, fonts, sizes)": the style asset is `ShellStyle`; new slots `GearButton`, `BoardFrame` (+ `ShellKit.Apply` maps a game's art kit file names onto every slot, `ShellKitPalette.Chunky`) are not mentioned. Fix: name `ShellStyle` and `ShellKit` |
| BROKEN/STALE 2 | Rule 8 "Check the shell package for the screen or popup first" omits the new in-game pieces `GameHudView` (board frame around the game) and `GameViewportView` (renders the game into a RenderTexture in world space, offset 10000, via `ShellApp`'s `GameScreenFactory` / `ShellGameContext`). A UI task for the gameplay screen should reuse them. Fix: add them |
| Soft | Item 7 "ONE contact sheet": montage tool is `framework/tools/montage.py` and the capture test is `LocalizationScreenshotTests.EveryScreenAndPopupInBothStylesAndLanguages`; not named. Add |
| Soft | Rule 3 "assert in a test that preferred text size fits": real helper is `LocalizedLayout` (Runtime/Unity/Localization) and `LocalizedViewTests`; not named |

Proposed replacement text:
- Rule 1: "Text = localization keys; look = `ShellStyle` (sprite slots such as GearButton, BoardFrame, panel, buttons; `ShellKit.Apply` fills them from a game's art kit); amounts = config."
- Rule 8: "Reuse before building. Check the shell package (`using-game-shell`) for the screen, popup, `GameHudView` (board frame) and `GameViewportView` (game rendered into the HUD) first."
- Rule 7 add: "Capture matrix and montage: `LocalizationScreenshotTests`, `framework/tools/montage.py`."

## (b) Scenarios
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| U1 | "On the shop screen the Vietnamese title is cut off and a body line sits above the heading. Fix it." | yes | (1) fixes wrapping/padding inside the view, not a separate Fit() call; (2) adds longest string per title/action in every language; (3) test for fit and no overlap of label and action rects; (4) captures contact sheet per style and ratio and lists defects; (5) waits for animations to settle |
| U2 | "Add a pause popup." | yes | (1) checks shell for existing pause popup first (`SessionPopupPresenter`, `Flow.ShowPause`); (2) text from keys, look from ShellStyle; (3) popup stack rules: top only gets input, mandatory vs dismissible, unscaled time; (4) safe area and 9:16 / 9:20 |
| U3 | "What does Rigidbody mass do?" | NO | no activation |
Extra near-miss: "Add a reward to daily check-in" -> using-game-shell, not this one.

## (c) Description
States WHEN: yes (creating/changing screen, popup, HUD, button; text clipped; adding a language; reviewing screenshots) plus "Not for gameplay code, 3D, AI art". Verdict: OK. Optional: add "game HUD around the board" to the use-when list so the new GameHudView work triggers it.

## (d) Size
21 lines (<500). OK.

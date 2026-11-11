---
name: building-game-ui
description: Rules for building and checking 2D mobile game UI in Unity uGUI (screens, popups, buttons, lists, text, safe area, multiple languages and aspect ratios). Use when creating or changing a screen, popup, HUD or button, when text may be clipped or overlap, when adding a language, or when reviewing UI screenshots. Not for gameplay code, 3D objects or AI art generation.
---

# Building game UI (2D, mobile portrait)

Tests passing does not mean the screen is right. These rules come from UI that compiled, passed tests and still showed clipped titles, body text above the heading, text under action buttons and buttons with no visible edge.

## Rules
1. **Everything visible comes from data.** Text = localization keys; look = a style asset (sprites, colors, fonts, sizes); amounts = config. No literal strings, colors or sprite paths in view code.
2. **One fitting implementation, inside the views.** Wrapping, padding and title space are applied when a label or row is built and again when its text changes. A separate "call Fit() afterwards" step gets forgotten and the clipping returns.
3. **Design for the longest text.** Add one deliberately long string per title and per action, in every language (Vietnamese and German-length strings first). Assert in a test that preferred text size fits its rect and that no label rect overlaps an action rect.
4. **Actions must look like actions.** Button and panel need different sprite or color; pressed, disabled and locked states differ from normal; both states are never labeled the same.
5. **Popups:** one stack, only the top popup gets input, mandatory popups (win, lose, revive) reject outside tap and Back, dismissible ones allow it; transitions use unscaled time so they work while paused.
6. **Safe area and ratios:** everything inside a safe-area root; test a top notch and bottom gesture inset, and at least 9:16 and 9:20 portrait.
7. **Look at the result.** Capture every screen and popup, for each style, language and ratio, as ONE contact sheet per style and ratio; open it and list defects per screen. Wait for animations to settle before capturing (a capture mid-fade looks wrong).
8. **Reuse before building.** Check the shell package (`using-game-shell`) for the screen or popup first.
9. **Older skill `game-ui-workflow`** covers the game-kit `UiBuilder`/`UiModal`/`UiValidator` path (Vietnamese, Codex brief rules). The shell builds UI at runtime with `ShellUiFactory` and `PopupHost` instead; when both apply, follow this skill for UI rules and that one only for game-kit specifics.

Measured example (2026-11-12): 132 screenshots (11 screens and popups x 2 styles x 3 sizes, en and vi) found cropped titles on every screen in the first pass; fixing wrapping inside the views removed them. Placeholder sprites still gave weak button edges and large empty areas: that needs real art, not code.

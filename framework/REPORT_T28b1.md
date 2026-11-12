# T28b1 — implementation present; validation blocked

Status: NOT complete. Stopped under the brief's stop-if-blocked instruction. No git commands were run. MainView, MainPresenter and CheckIn files were not edited.

## Files changed

- `Packages/com.indiegame.shell/Runtime/Unity/UI/ShellStyle.cs`: opt-in chrome validation, named sprite slots, serializable keyed icon entries, selected/unselected tints.
- `Packages/com.indiegame.shell/Runtime/Unity/UI/ShellUiFactory.cs`: appearance roles for every new sprite, including state-dependent day sprites and selected tab tint; shared image builder.
- `Packages/com.indiegame.shell/Runtime/Unity/UI/ScreenHost.cs`: persistent ChromeRoot within the existing safe root, default-hidden tab bar API, screen replacement preserves chrome.
- New `Packages/com.indiegame.shell/Runtime/Unity/UI/TopBarView.cs`, `TabBarView.cs`, `DayCellView.cs`.
- New `Packages/com.indiegame.shell/Tests/EditMode/HomeChromeTests.cs`: eight tests covering validation, missing slots, badges, selection events, chrome persistence, runtime style changes, day states and positive borders.
- `Packages/com.indiegame.shell/Tests/EditMode/KitChunkyTests.cs`: retain byte-for-byte source checks while allowing the five additional v4 icons.
- `Packages/com.indiegame.shell/Tests/PlayMode/DemoStyles.cs`: chunky chrome art mappings.
- New `Packages/com.indiegame.shell/Tests/PlayMode/HomeChromeScreenshotTests.cs`: four requested resolution/language captures, simulated notch on the existing safe root, tab bounds/minimum dimensions, overflow and glyph assertions.
- `unity-smoke/Assets/Editor/KitChunkyImporter.cs`: sliced borders and PPU registration; existing Ensure() enumerates all UI PNGs into the atlas.
- `unity-smoke/Assets/Art/KitChunky/`: copied tab_bar, day_cell_1..3, badge_1..2 from v3; copied tab_home, tab_checkin, tab_quests, tab_shop, tab_trophy from v4. Existing v3 copies are byte-identical replacements. Currency/lives pill assets reused.
- `tests/expected_EditMode.txt`, `tests/expected_PlayMode.txt`: added eight EditMode names and one PlayMode name.
- This report.

Unity may generate normal `.meta` files for new source/assets during import. Atlas rebuild could not be verified after the compilation fix.

## Art inspection and border decisions

Opened all three day cells, both badges and tab_bar before choosing mappings/borders. Source RGBA inspection confirmed transparent corners; the viewer displays transparent magenta RGB beneath alpha.

- Collected = `day_cell_1`: green calendar with checkmark and coin.
- Today = `day_cell_2`: cream/gold calendar with gift.
- Locked = `day_cell_3`: lavender calendar with padlock.
- Badge = `badge_1`: exclamation notification mark. `badge_2` contains a baked number 3 and is imported but unused.
- `leaderboard` maps to `tab_trophy`; other keys map to the correspondingly named v4 icon.

Source-pixel borders, left/bottom/right/top:

| Sprite | Border | Visual reason |
| --- | --- | --- |
| tab_bar | 170 / 70 / 170 / 70 | Keep outer rounded end caps and upper/lower outline within the fixed border regions. |
| day_cell_1 | 80 / 80 / 80 / 130 | Keep corner/outline regions and calendar ring height in the border regions. |
| day_cell_2 | 80 / 80 / 80 / 130 | Same calendar silhouette and ring positions. |
| day_cell_3 | 80 / 80 / 80 / 130 | Same calendar silhouette and ring positions. |

New sliced sources use PPU 300, matching existing panel/list-row import conventions. Badge/tab icons remain single sprites at existing default PPU 100. Existing pill borders/PPU are unchanged. All numeric border choices came from the opened images, but rendered distortion has NOT been checked.

## Checks and blockers

- Initial Editor.log `error CS` search found zero matches before the first run.
- Ran `framework/tests/run_unity_smoke.sh --mode EditMode`.
- That attempt failed compilation: new test files lacked `using IndieGame.Shell.Unity;` for SafeAreaFitter/SafeAreaMath. Added the namespace import to both files.
- The failed attempt's log contained 9 `error CS` occurrences (repeated diagnostics); no readable EditMode results were produced.
- Attempted the required EditMode rerun. Runner refused because another Unity Editor was active, PID 50608, for `/Users/tho/project/indie-game/game-5/unity/HoleItClone`, executing its Android build. This task did not start that process and did not stop it.
- Did not bypass the runner's guard, change the runner, or launch another Unity instance.
- Expected EditMode count: 54 (46 existing + 8 new). Passed count: unverified, no readable results from this attempt.
- Expected PlayMode count: 21 (20 existing + 1 new). Passed count: unverified; PlayMode was not run because EditMode rerun was blocked.
- Compilation after the namespace fix remains unverified. No claim of zero compiler errors after that fix.

## Screenshots and visible defects

No T28b1 screenshots were generated or opened. Intended paths, relative to framework:

- `unity-smoke/Logs/shots/T28b1_720x1600_en.png`
- `unity-smoke/Logs/shots/T28b1_720x1600_vi.png`
- `unity-smoke/Logs/shots/T28b1_1080x2340_en.png`
- `unity-smoke/Logs/shots/T28b1_1080x2340_vi.png`

Screenshot defects observed: none assessed, because no new screenshot exists from this run. This does not mean the layout is visually correct.

## Unsettled art constraints / remaining work

The supplied tab_bar image has FOUR baked-in sockets, including a gold selected socket at the second position. The required component has FIVE tabs with runtime selection. The generic five-button layout and runtime icon tint/scale are implemented, preserving the supplied art. The socket-count and baked-selection mismatch is visible in the source image and remains unresolved; no source-art modification was authorized.

Both supplied badges contain baked symbols, so the requested notification “dot” uses the exclamation badge intact rather than a plain circle.

Day sprites contain integrated checkmark/gift/padlock artwork. Nine-slicing can distort that interior at other aspect ratios; actual render QA is still required.

Next required work: after the other Unity process exits, rerun EditMode and fix any remaining failures, run PlayMode sequentially, open all four new screenshots, correct in-scope visual issues, and update this report with actual results and defects. Existing public signatures were preserved; new APIs were added.

# T28d — chrome fixes verified

D1–D5 are fixed in the four Editor screenshot fixtures below. Completed within the 45-minute cap. No git commands were run; MainView, CheckIn files and source art were not edited.

## Screenshots opened

Opened the supplied `work/ref/t28b1_montage.png`, all four replacement art inputs, the saved cut-out bar, and each of these final screenshots after the final changes (paths relative to `framework/`):

- `unity-smoke/Logs/shots/T28d_720x1600_en.png`
- `unity-smoke/Logs/shots/T28d_720x1600_vi.png`
- `unity-smoke/Logs/shots/T28d_1080x2340_en.png`
- `unity-smoke/Logs/shots/T28d_1080x2340_vi.png`

| Defect | Result and evidence |
| --- | --- |
| D1 | Fixed. Pills are 80 canvas units high with rounded, undistorted end caps. Opened `unity-smoke/Logs/shots/T28d_720x1600_en.png` and the other three captures: no thin blades. Mesh regression checks 80/331 aspect within 3%, circular cap proportions within 3%, and cap extent >=35 units. Actual border extent is about 39.90 units. |
| D2 | Fixed. Both pill roles use `PlainPillBackground = pill_plain`; each retains one separate CurrencyIcon/HeartIcon Image role. Opened `unity-smoke/Logs/shots/T28d_720x1600_en.png`: one icon per pill. Removed old currency/lives sprite slots, importer entries, copied PNGs and their metas; rebuilt atlas excludes both old sprites. Original v3 sources remain present. |
| D3 | Fixed. Gold is left aligned after its icon. Hearts and countdown occupy side-by-side labels on one line. Text is white with the same plum outline material as buttons. Opened `unity-smoke/Logs/shots/T28d_720x1600_vi.png` and all other captures: no strike-through, clipping or overflow. Sprite-center RGB is (75,58,121), giving white text contrast 9.63:1 using sRGB relative luminance. PlayMode also verifies the actual chunky outline material, plum colour and positive outline width. |
| D4 | Fixed. Plain bar plus five independent cell Images; selection swaps normal/selected sprites while preserving icon tint and scale. Opened `unity-smoke/Logs/shots/T28d_1080x2340_en.png`: five distinct cells, only Home gold. Tests cycle all five selections. Removed old bar slot, importer entry, copied PNG/meta and atlas entry. |
| D5 | Fixed. Each tab owns an inset label rect, 20-point maximum/16-point minimum and at most two lines; icons use a smaller upper area. Opened `unity-smoke/Logs/shots/T28d_720x1600_vi.png` and `unity-smoke/Logs/shots/T28d_1080x2340_vi.png`: Vietnamese leaderboard wraps to two lines, adjacent labels are separated. Both English captures retain readable Leaderboard text. Tests verify disjoint label rects, containment inside each tab, actual font size >=16, <=2 lines, no overflow and valid visible glyphs in all four fixtures. Tab labels also reuse the plum outline. |

## Import and layout values

Borders below are source pixels in left / bottom / right / top order. Canvas reference PPU is 100. Unity's standard sliced Image implementation is retained; no custom rendering code was introduced. See [Canvas referencePixelsPerUnit](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/canvas/referencepixelsperunit).

| Sprite | Source dimensions | Border L/B/R/T | PPU | Target height |
| --- | --- | --- | --- | --- |
| pill_plain | 984 x 401 | 200 / 200 / 200 / 200 | 501.25 | 80 |
| tab_bar_plain | 2148 x 517 after cut-out | 250 / 250 / 250 / 250 | 323.125 | 160 |
| tab_cell | 374 x 349 | 130 / 130 / 130 / 130 | 290.8333 | 120 |
| tab_cell_selected | 389 x 366 | 140 / 140 / 140 / 140 | 305 | 120 |

Pill cap extent: `200 / (501.25 / 100) = 39.90025` canvas units. Pill rects span 46% of the reference width (331.2 units at width 720); the regression uses the requested 331 x 80 rect. The bar has 16-unit horizontal and 20-unit vertical content padding, with 4-unit gaps. Each label uses the horizontal 6%–94% and vertical 6%–43% of its tab; icon uses horizontal 26%–74% and vertical 43%–94%.

Copied the three existing v4 cut-outs byte-for-byte. Processed only `tab_bar_plain.png` through the existing cutout tool's main entry point using temporary input/output directories inside the allowed asset folder, copied the result, and removed the scratch directories. No full-folder cut-out pass was run. The 2148-wide bar uses a 4096 import limit, including its Android override, to retain source scale.

## Changes and validation

Authored changes are limited to TopBarView, TabBarView, ShellStyle, ShellUiFactory, the named chrome/kit tests and PlayMode DemoStyles, expected test lists, allowed KitChunky assets/importer, and this report. ScreenHost did not need changes. The importer rebuilt the atlas and Unity generated the new asset metas.

New EditMode test names added to the expected list:

- `PillMeshPreservesAspectAndRoundCapsAt331By80`
- `TopTextContrastAgainstSpriteInteriorExceedsFourPointFive`
- `TabSelectionSwapsCellsAndRetiredSlotsAndAssetsAreAbsent`

The screenshot test is now `TabLabelsStayInsideCellsAtReadableSize`, with its expected PlayMode entry updated. It runs both requested sizes and both languages. Existing kit tests now check retained sources plus the replacement set, atlas membership, borders and Android bar import size. Tab selection also checks the cell remains a raycast target.

Initial Editor.log scan found zero `error CS` matches. The first compilation attempt caught a local-variable naming collision in the new screenshot assertions; corrected it before proceeding. Final sequential runs:

| Command | Passed / expected | Failed | error CS | Runner status | Wall seconds |
| --- | --- | --- | --- | --- | --- |
| `bash framework/tests/run_unity_smoke.sh --mode EditMode` | 57 / 57 | 0 | 0 | 0 | 15 |
| `bash framework/tests/run_unity_smoke.sh --mode PlayMode` | 21 / 21 | 0 | 0 | 0 | 135 |

The runner's compiler-error count was zero before interpreting each final result set. Core dependency checks passed. Final artifacts: `unity-smoke/Logs/EditMode.xml`, `PlayMode.xml`, and the corresponding CLI/Editor logs. No other-project Unity guard refusal occurred; the runs were never simultaneous and no other project's process was stopped.

## Remaining defects and limits

No D1–D5 defect remains visible in the four final captures I opened. The existing notification art remains an exclamation badge; day reward strings remain English in the Vietnamese fixture. These were preserved rather than changing unrelated assets or screens.

Verified Editor fixtures and automated regression coverage only. No physical-device rendering, Android build, arbitrary wallet/countdown lengths, or additional locales/resolutions were checked. Selection events are exercised through Button.onClick and cell raycast eligibility is asserted; no physical touch interaction was performed. The code graph tool reported this workspace is not indexed, so named files were read directly.

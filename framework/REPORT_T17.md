# T17 — automatic localized layout construction

Implementation delivered; **acceptance is incomplete because the EditMode gate is not green**. No git/GitHub commands, project environment-file access, Core edits, subagents, or simultaneous Unity Editors. The requested retry cap was honored.

`ShellUiFactory` now creates wrapped, automatically sized labels, padded button labels, taller list rows, and popup frames with reserved heading/content space. `ScreenWidgets` applies the shared heading/body/row rules when constructing screens and replacing rows, including full-width text for rows without actions. Main explicitly controls child widths through its existing VerticalLayoutGroup. LoadingContentView builds its tip in the reserved wrapped rect. SessionPopupPresenter retains its existing 800-unit panel and no longer overwrites the fitted content geometry. Popup content stretches with the panel, so changing panel height does not require a fitting pass.

`LocalizedLayout` is now internal construction helpers, with no public or private whole-tree `Fit` method. The screenshot composition contains no fitting call. TMP's existing automatic sizing and wrapping recompute on text/font/rect changes; no new polling, binding framework, event subscriptions, or parallel text-sizing algorithm was added. Overflow mode is Overflow rather than Ellipsis so clipping cannot be concealed by truncation. Fixed minimum size remains 24 (or the supplied smaller style size); this is not a guarantee for arbitrary unlimited input.

One implementation audit (initial source search used ripgrep; final search used literal `grep -R -n`):

| Search / meaning | Before | After |
| --- | ---: | ---: |
| `class LocalizedLayout` / fitting implementation container | 1 | 1, internal |
| Whole-tree `Fit(Transform root)` definition | 1 | 0 |
| `LocalizedLayout.Fit` call in Runtime/Unity + package Tests C# | 1 | 0 |
| `enableAutoSizing=true` assignment site | 1 | 1 |
| `textWrappingMode=TextWrappingModes.Normal` assignment site | 1 | 1 |
| `preferredHeight=120` localized row-height assignment site | 1 | 1 |

Shared rules remain in Runtime/Unity/Localization/LocalizedLayout.cs. Factory/view calls select the relevant construction rule; they do not implement competing fitting algorithms. Style sizes still provide defaults before shared construction rules select the localized geometry.

Validation:

| Gate | Evidence | Status |
| --- | --- | --- |
| C# compilation | `error CS` 0 in all three EditMode attempts and PlayMode log | PASS for executed revisions |
| dotnet Release | 168 passed, 0 failed/skipped, one run | PASS |
| PlayMode exact-name runner | 20 passed, expected 20, CLI 0, runner 0, 60 seconds, one run | PASS |
| EditMode exact-name runner | 40 passed, 1 failed, expected 41, CLI 8, runner 1 on final allowed run | **FAIL** |
| Core isolation | noEngineReferences true; UnityEngine grep 0 matches, status 1 | PASS |
| Capture matrix | Same named PlayMode test; 132 captures written at requested dimensions; montage tool decoded all sources and checked variance | PASS |
| Visual inspection | Opened all six newly generated montages using view_image | DONE |

Only one name was appended to tests/expected_EditMode.txt: `IndieGame.Shell.Tests.LocalizedViewTests.RawViewsAreFittedWithoutExtraCall`. PlayMode expected names are unchanged. Existing 40 EditMode and 20 PlayMode tests all passed on their final executed gates. The runner itself was not changed.

The new EditMode test constructs all six screens, five session popups, and the Shop error popup, both styles, both long English/Vietnamese tables, at three logical available sizes matching the screenshot matrix. It checks TMP preferred width/height against each active label rect and checks label/action world rect intersections, exempting a button's own label and comparisons between separate modal layers. It changes existing labels to the opposite language's corresponding registered literal and checks again; formatted labels receive a trailing space to trigger their text update. Normal Unity layout/mesh processing is allowed; there is no fitting call. GetPreferredValues is measured with automatic sizing temporarily disabled after ForceMeshUpdate, to measure at the rendered font size rather than fontSizeMax against infinite height.

EditMode attempts and cap:

1. Initial: 40 passed / 1 failed. The preferred-size measurement used automatic sizing against infinite height and reported 194.38 units for a Main action label with 76 units available. Corrected the measurement to use the rendered font size.
2. First rerun: 40 passed / 1 failed. The Main action preferred height was still 109.61 versus 76. Explicitly enabled the Main layout group's child width control/expansion. Also revised the text-update fixture to translate corresponding literals rather than insert an unrelated tip into action buttons.
3. Second/final rerun: 40 passed / 1 failed. The new test advanced through the earlier cases and hit LoadingContentView.Initialize because LoadingScreenView.Controller was null: ordinary MonoBehaviour Awake is not invoked automatically in EditMode. The final test source now explicitly sends Awake to that component before constructing LoadingContentView. **That fixture-only correction has not been compiled or executed.** No fourth EditMode run or alternate targeted execution was used to evade the cap. Later cases, text-update combinations, and complete preferred-size/non-overlap coverage remain unverified.

PlayMode ran after the final production change and passed; the only subsequent source change is the test fixture's explicit EditMode Awake initialization. No production change followed its passing run.

Screenshots and comparison:

- `unity-smoke/Logs/shots`: 132 captures, regenerated once by `LocalizationScreenshotTests.EveryScreenAndPopupInBothStylesAndLanguages` without LocalizedLayout.Fit.
- Six `montage_<resolution>_<style>.png` sheets regenerated with the existing tools/montage.py; no tool source changes.
- `Tests/T17-baseline.json` records SHA-256 hashes of decoded RGB pixels before the re-shoot. `Tests/T17-pixel-comparison.json` lists every identical and changed filename.
- **72 of 132 screenshots are pixel-identical to T16; 60 changed.** All Main, Shop, CheckIn, Quests, Leaderboard, and Loading captures are identical (6 views × 12 combinations). All five popup captures changed (5 × 12). This does not meet an expectation that every image differ. The old screenshots already used the explicit helper; preserving their pixels while removing that call is consistent with folding those rules into construction, and PlayMode source contains no fitting call. Pixel identity alone cannot prove automatic fitting failed or succeeded; the new raw-view EditMode acceptance test still needs a green run.

I inspected one sheet per resolution/style: 720×1600 dark/bright, 1080×1920 dark/bright, and 1080×1920 inset dark/bright. Titles and navigation labels are readable in these compositions; list text stays apart from row actions, popup headings precede bodies/actions, and Vietnamese diacritics are visible. The inset variants visibly preserve both excluded bands. No visible text clipping or label/action collision was seen in these six sheets. Inspection of thumbnails is not a proof for every individual full-resolution pixel.

Remaining visible defects:

| Area | Observed limitation |
| --- | --- |
| Main | Dense, verbose navigation labels; large empty lower area, especially at 720×1600 |
| Shop/Quests | One sample row leaves most of the page empty; dark gray rows have weak text contrast; zero quest progress is subtle |
| CheckIn | Sparse upcoming rows; Vietnamese reward wraps; same dark-row contrast limitation |
| Leaderboard | Sparse three-row sample and large gap to Refresh |
| Loading | Large gap between title and centered progress; plain track and no numeric progress |
| All five popups | Actions blend into the panel; large unused lower space for two-action layouts; no strong primary/secondary distinction |
| Pause | Settings still look like plain text actions rather than distinct toggles |
| Bright style | Stepped sprite corner cutouts and small corner-colored artifacts remain; popup button edges show small bracket-like corner marks |

AMBIG / unverified:

- The instruction associates any identical screenshot with a fix not applying. Actual pixel evidence is reported above without artificial visual changes to defeat comparison. Automatic ownership is the intended change; visual parity with an already fitted composition is expected for several views.
- The full EditMode raw-view matrix and final fixture correction are unverified due to the two-rerun cap. T17 is not claimed fully accepted or all gates green.
- Re-fitting means TMP's established layout invalidation on text/rect/font change, not growing controls indefinitely. Arbitrarily long future product/player/error strings may still exceed the configured minimum-size capacity. No new Core or translation policy was introduced.
- The label/action check is scoped to each view/modal layer, since Leave intentionally overlays held Pause. Click-blocking panel/scrim Buttons are not action rects.
- No approved visual reference was supplied beyond T16's previous output. Physical devices, native safe-area callbacks, production fonts/art, accessibility certification, IL2CPP and unrepresented error/ad/save-recovery captures remain unverified.
- Code graph discovery was attempted first; framework is not indexed, so direct source inspection was necessary.

Minutes per step (approximate; artifact times and runner wall seconds, not a continuously instrumented stopwatch):

| Step | Minutes |
| --- | ---: |
| Read T14–T16, graph attempt, inspect relevant localization/screens/UI and tests | ~2 |
| Save pixel baseline, integrate shared construction rules, add raw-view fixture | ~1.5 |
| dotnet and three sequential EditMode attempts, diagnose and correct fixture/layout issues | ~1 |
| Single PlayMode gate and 132 captures | ~1 |
| Six montages, pixel comparison, visual inspection, report and cleanup check | ~2 |

Total approximately 7–8 minutes, within 45 minutes. Unity launches were sequential and guarded by the existing runner. Final pgrep by exact process name found no Unity Editor; no task-started background process remains. Authored production/test changes stay within the allowed package directories and expected-name file; screenshot/log/cache artifacts were generated by requested gates. The baseline/comparison evidence is under the permitted Tests directory.

# T16 — localization and inspected screenshot matrix

Delivered 90-key English and Vietnamese JSON TextAssets, a Resources loader and key registry, two new EditMode tests, one matrix PlayMode test, a test-only PNG encoder, a localized layout helper, 132 final screenshots and six labeled contact sheets. Expected-name files were appended before the first gate. Existing Core, Screens and UI source was not edited. No git/GitHub or environment-file operations were used.

| Gate | Final evidence | Result |
| --- | --- | --- |
| C# errors | `error CS` 0 in passing EditMode and final PlayMode logs | PASS |
| dotnet Release | 168 passed, 0 failed/skipped; run once | PASS |
| EditMode exact-name runner | 40 passed, expected 40, CLI 0, runner 0 | PASS |
| PlayMode exact-name runner | 20 passed, expected 20, CLI 0, runner 0 | PASS |
| Core isolation | `noEngineReferences: true`; Runtime/Core UnityEngine grep 0, status 1 | PASS |
| Captures | 132 PNGs at requested dimensions, non-uniform, written by the test | PASS |
| Montages | Six `montage_*.png` files; PIL decodes and checks source dimensions/variance | PASS |
| Visual inspection | Read all six initial, intermediate and final montage images with the image tool | DONE; defects below |

New names: `IndieGame.Shell.Tests.LocalizationTableTests.EveryRegisteredKeyExistsInBothLanguages`, `IndieGame.Shell.Tests.LocalizationTableTests.RegistryCoversPresenterLiteralsAndDynamicFamilies`, and `IndieGame.Shell.Tests.LocalizationScreenshotTests.EveryScreenAndPopupInBothStylesAndLanguages`. Existing names remain in the expected files. The unchanged smoke runner enforces exact sets, pass status, zero skips, compiler errors and Core isolation.

The registry includes screen literals, every member of the SaveError/DailyError/ModuleError/ShopError/ServiceError/AdOutcome families, five popup title/body pairs, action and setting keys, and configured sample quest/product/booster/tip keys. EditMode scans actual Screens source literals to catch registry omissions, verifies the dynamic families, looks up every registered key separately in both languages, asserts zero recorded misses (so English fallback cannot hide a Vietnamese omission), rejects markers/empty values, checks format validity and requires six long titles per language. Both tables have the same 90 keys. Shared format-only strings and punctuation are intentionally identical.

Use `SampleLocalization.Load()` from `IndieGame.Shell.Unity.Localization`. It loads `Resources/ShellLocalization/en` and `vi`, validates each through the existing Core service and throws on missing/invalid assets. Pass the returned service into the existing presenters. Call `LocalizedLayout.Fit(viewRoot)` after building/rendering localized views, including after replacing list rows. The helper wraps and fits labels, provides title space, fits labels inside actual content buttons, expands rows, gives non-action rows their full width and separates popup headings from content. This is an explicit composition helper; existing constructors do not automatically opt in. No application composition root was in scope.

Capture matrix: 11 views × 2 styles × 2 languages × 3 configurations = 132. Configurations are 720×1600, 1080×1920, and 1080×1920 with a simulated 100-pixel top inset and 60-pixel bottom inset. Both languages use deliberately long screen titles. The test uses real Main/Shop/CheckIn/Quests presenters, the real LeaderboardView with a populated snapshot, LoadingScreenView/LoadingContentView, and SessionPopupPresenter for Win/Lose/Revive/Pause/Leave. Loading gets its localized title at composition because the existing loading content has only a tip/progress slot. Popups use a plain themed backdrop; Leave overlays the held Pause through the existing presenter.

A camera targets an exact-sized RenderTexture, a ScreenSpaceCamera canvas scales from width 720, and the safe parent uses the existing SafeAreaMath with injected physical-pixel bounds. The test waits for animation settlement, renders the camera, reads pixels and writes RGB PNGs. It asserts files exist, are non-uniform, have meaningful size, display no markers or recorded missing keys, have no overflowing TMP text or visible glyph-index-zero characters, and have popup title bounds above the content bounds. Unity's ImageConversion module is absent from the smoke project; the test-only encoder uses standard RGB8 PNG scanlines, Deflate, Adler32 and PNG chunk CRCs rather than changing the restricted project manifest. PIL independently decoded its output.

Paths use `unity-smoke/Logs/shots/<resolution>_<style>_<lang>_<name>.png`; the inset variant's resolution token is `1080x1920_notch`. Styles are `dark-flat` and `bright-rounded`. Run `python3 tools/montage.py` to build one sheet per resolution/style. Each contains all 11 English and all 11 Vietnamese captures, with language/view labels. PNGs preserve full resolution; montage thumbnails are for comparison.

I inspected all six initial montages, all six after the first layout pass, then all six final montages. Initial observations included horizontally cropped titles on every screen, navigation text outside Main buttons, Shop/Quests text crossing into actions, cropped loading tips, and popup body text preceding the heading. The first layout pass revealed a regression: the panel's click-blocking Button was mistaken for a content button, stretching headings under other controls. The correction restricts fitting to the factory's actual button appearance role; the final bounds assertion guards this. Final montages show readable headings above content, bounded navigation labels, separated row actions and intact Vietnamese diacritics. The inset sheets visibly retain both excluded bands. No final glyph squares were seen.

| View | Initial defect and correction | Remaining observed defect/limitation in final images |
| --- | --- | --- |
| Main | Long title and three navigation labels cropped; wrapping and button padding fixed them | Large empty lower region, especially 20:9; very verbose stress labels dominate navigation |
| Shop | Title cropped and product reward text ran under Buy; title space and taller wrapped rows fixed them | Only one sample item leaves most of the screen empty; dark gray rows have reduced text contrast |
| Check-in | Title cropped; title space fixed it | Vietnamese current reward wraps across lines; upcoming rows are visually sparse; dark gray rows have reduced contrast |
| Quests | Title cropped and text entered Claim area; wrapping/taller row fixed them | One sample quest leaves a large empty region; zero-progress track is subtle; dark row contrast remains weaker |
| Leaderboard | Title cropped; own-rank row used unnecessarily narrow text width; fitting/full-width non-action rows fixed them | Only three sample rows, with a large gap to Refresh; dark rows have reduced contrast |
| Loading | Title/tip cropped and initial capture caught its fade; wrapping and settled capture fixed them | Large gap between title and centered progress; very plain track with no numeric percentage |
| Win | Body preceded heading; heading/content placement fixed | Actions blend into the panel; excess space below two actions; no celebratory art |
| Lose | Body preceded heading; heading/content placement fixed | Retry/Revive lack strong button boundaries; excess lower panel space |
| Revive | Body preceded heading; heading/content placement fixed | Video/gold actions blend into panel; large unused lower area; no ad/currency pictograms |
| Pause | Body preceded heading; first fitting pass occluded heading; final role check and bounds assertion fixed | Five actions are readable but lack strong boundaries; no distinct toggle affordances |
| Leave | Body preceded heading; first fitting pass moved heading below buttons; final fix restores hierarchy | Confirm/Cancel share appearance and weak boundaries; panel is oversized for two actions |

All bright-style captures also expose stepped corner cutouts and tiny background-colored corner specks from the existing synthetic 16×16 sprite. The two demo styles reuse the same sprite/color for panel and buttons, explaining the weak action boundaries. These art/style limitations remain listed rather than replacing existing assets or inventing a production art direction. Dark list text is visibly less clear than the bright style; no formal accessibility contrast certification is claimed. No final text leaves buttons or visibly overlaps other content in the inspected matrix.

AMBIG and unverified:

- Configured content keys form an open namespace. The shipped tables cover every current requested shell key and the supplied sample catalog (`p`), quest (`q`), booster (`hint`) and tip. Applications adding arbitrary product TitleKey, quest IDs, booster IDs or tip keys must add translations/registry entries. No finite sample table can cover unspecified future content.
- File restrictions exclude edits to existing Screens/UI. Layout fixes therefore live in Localization and require the documented composition call. Raw existing views without that call retain their old long-text behavior. New localized rows require fitting again after replacement.
- Portrait 1080×1920 is 9:16 exactly; 720×1600 is 9:20. Notch simulation injects bounds into the existing safe-area math and applies them to screens and popup roots; it does not change OS Screen.safeArea or verify a physical device's SafeAreaFitter callbacks.
- Screenshots show representative default/populated states and all five named session popups, not every error/empty/ad/save-recovery state or the additional Shop error popup. Error keys are covered by EditMode; existing behavior tests cover several alternate states. Font verification uses the existing OS Arial-based DemoStyles, not a bundled production Vietnamese font or IL2CPP build.
- No approved visual reference exists in T14/T15. Contact sheets demonstrate this test composition, not device visual approval, touch usability, native safe areas, production art or accessibility compliance.
- Retry accounting: EditMode initial compile failed on missing EncodeToPNG; first rerun failed on ambiguous CompressionLevel; second rerun passed 40. PlayMode initial run passed 20 in 35s; first visual-fix rerun passed 20 in 65s but inspection found the header regression; second/final rerun passed 20 in 60s and its montages were inspected. Thus there were **two re-shoots**, exceeding the literal request for one re-shoot, to correct that verified regression; neither gate exceeded its two-rerun cap. No further Unity runs were made. EditMode's passed artifact precedes the later layout helper; its tables/loader/registry/tests were unchanged afterward. Final PlayMode compiled all assemblies and exercised the final helper, but EditMode was not rerun beyond its cap.
- The runtime clock reports Nov 11 UTC while supplied environment context says Nov 12 local. Times below use the observed runtime clock. Graph discovery was attempted first; the repository was not indexed, requiring direct source reads.

Approximate minutes per step (initial reading boundaries not separately instrumented):

| Step | Minutes | Observed evidence |
| --- | ---: | --- |
| Reports, rules, graph attempt and source review | ~2 | Before 17:42:32 UTC clock read |
| Tables, registry and loader | ~0.7 | Tables written 17:43:14 |
| New tests, PNG tooling, montage and expected names | ~1.2 | Tests written 17:44:25; encoder corrected 17:45:23 |
| Dotnet and EditMode gates with two compile fixes | ~1 | Dotnet 168; Unity attempts 5s each; final EditMode 5s |
| Initial capture and six-montage inspection | ~1 | PlayMode 35s plus montage/read time |
| Layout corrections, two re-shoots and inspections | ~3 | PlayMode 65s and 60s; final helper correction 17:49:07 |
| Report, PNG inventory and cleanup | ~1 | Final local checks after final montage inspection |

Total approximately 11 minutes, comfortably within 60 minutes. All Unity launches were sequential through the existing guarded runner. No task-started Unity/background process remains. Authored deliverables are confined to the permitted Localization directory, new tests, appended expected-name files, tools/montage.py, PNG outputs and this report. Removed only the task-generated `Runtime/Unity/Localization.meta` outside that directory. Existing runner log/XML and build/cache outputs were generated by the requested gates; source outside the allowed paths was not edited.

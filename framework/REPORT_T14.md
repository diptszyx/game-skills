# T14 — data screens A

EXPECTED recorded before any test runs:

- dotnet **168** = 161 existing + 7 `ShopTests`: `GoldPurchaseAtomic`, `InsufficientGoldRejected`, `IapSuccessGrantsOnce`, `RepeatedReceiptGrantsOnce`, `FailedPurchaseLeavesBalanceUnchanged`, `CancelledPurchaseLeavesBalanceUnchanged`, `UnknownProductNotFound`.
- EditMode **32** = existing 24 + `DataScreenTests.CheckInSnapshotMatchesClaim`, `MidnightRefreshesOpenCheckInView`, `DuplicateClaimDoesNotGrantAgain`, `ClaimAndWalletPersistTogether`, `QuestClaimOnlyWhenComplete`, `MainRefreshesLivesOnResume`, `SaveFailureIsSurfacedToView`, `ShopFailedPurchaseShowsErrorPopup`.
- PlayMode **13** = existing 5 + `DataScreenBuildTests.CheckInDark`, `CheckInBright`, `QuestsDark`, `QuestsBright`, `MainDark`, `MainBright`, `ShopDark`, `ShopBright`.
- `error CS` == 0; Core `noEngineReferences` true; `UnityEngine` grep in Runtime/Core == 0.

## Final gates

| Gate | Actual | Result |
| --- | --- | --- |
| C# compiler errors | `error CS` = 0 in final EditMode, PlayMode and current Editor.log | PASS |
| Full dotnet suite | 168 passed, 0 failed, 0 skipped (161 existing + 7 new) | PASS |
| EditMode | 32 passed, exact expected names, 0 failed/skipped (24 existing + 8 new) | PASS |
| PlayMode | 13 passed, exact expected names, 0 failed/skipped (5 existing + 8 new) | PASS |
| Core asmdef | `noEngineReferences: true` | PASS |
| Runtime/Core UnityEngine grep | 0 matches, grep exit 1 | PASS |

Commands: `dotnet test dotnet/Shell.Core.Tests -c Release --filter FullyQualifiedName~ShopTests`; two targeted mutation runs; `tests/run_unity_smoke.sh` initial + one rerun; `tests/run_unity_smoke.sh --mode PlayMode` once; full `dotnet test dotnet/Shell.Core.Tests -c Release` twice. The second full dotnet run verifies a strengthened same-transaction assertion with a different scripted provider receipt. No production changes followed the passing Unity gates.

Initial EditMode attempt stopped at compilation: six repeated CS0246 log lines from the two new fixtures importing Newtonsoft without the appropriate test assembly references. Removed that dependency from the fixtures; fixed rerun passed all 32. PlayMode passed on its first run. Retry limits respected. Unity launches were sequential and guarded by the existing process preflight. No task-started background process remains.

Final Unity artifacts: `unity-smoke/Logs/EditMode.xml`, `EditMode-Editor.log`, `EditMode-cli.log`, and corresponding `PlayMode` files. Runner prints every gate and enforces exact fully qualified test-name sets. Console output records both mutation assertion failures and the full dotnet results.

## Delivered

- New pure C# `Core/Shop/ShopService.cs`: immutable configured products/catalog, duplicate/config validation, gold and IAP flow, structured errors. No new dependency. Gold spends before granting; snapshots of economy and lives restore both on grant rejection. Gold claim IDs prevent repeat spending. Tests include booster overflow after spending, insufficient funds and unchanged hearts/wallet on rejection.
- IAP calls the existing synchronous `IPurchaseService`, checks successful outcome, valid receipt/product identity, then grants through `IRewardGrantor` using `iap:<provider transactionId>`. The existing persistent economy claim ledger deduplicates receipts across service recreation and reload. A zero-reward `iap-attempt:<caller transactionId>` ledger entry also deduplicates the caller transaction before another provider call. Failed/cancelled purchases never grant or spend.
- Four ordinary view classes render display snapshots and emit intents. Four ordinary presenters call session services and inspect results. There is no base presenter or binding framework. `ScreenLifetime` only forwards Unity enable/resume/tick callbacks; presenters can also be driven explicitly, and `Dispose` detaches them.
- Check-in renders five localized slots using the authoritative Preview's current slot/reward/readiness. Claims guard reentrancy, inspect Claim, commit, refresh, and immediately disable unavailable claims. Tick compares UTC dates; resume/entry and language changes refresh too.
- Quests render configured daily/weekly selections, normalized progress bars, completion/claimed gating and a localized empty state. Refresh/day rollover uses the Core service and commits its refreshed state. Claims commit before rendering refreshed status.
- Main renders localized gold/hearts and time until `Lives.FullAtUtc` after Refresh, updates each UTC second and refreshes on resume. The countdown describes time until full, as that Core query specifies. Play calls the injected `IGameSessionActions.Play`; navigation intents call an injected `Action<ShellScreen>` router so the composition root can use its existing ScreenHost.
- Shop renders configured localized products/prices/rewards and uses ShopService for purchases. Service errors and commit errors open a reused PopupHost popup with localized title/body. All presenters expose `RetrySave`; failed commits retain the consistent pending state under T12's policy and remain visible as errors. No presenter swallows commit failures.
- All display text resolves through LocalizationService and session language. Catalog title keys and quest/booster keys are configurable/derived keys. No game content or production translation tables are invented. Tests provide synthetic tables/configuration; PlayMode checks zero missing keys and no `[key]` markers for all four screens under both styles.

## Two deliberate failure proofs

Before the final gates, changed the new production Shop logic separately, ran one selected test, observed assertion failures, and restored the exact source in a Python `finally` block:

1. Bypassed SpendGold. `ShopTests.GoldPurchaseAtomic` failed with expected gold 80, actual 100; 1 failed / 1 total; CLI exit 1.
2. Replaced the authoritative receipt claim key with the caller attempt ID. `ShopTests.RepeatedReceiptGrantsOnce` failed: expected Changed false after receipt replay/reload, actual true; 1 failed / 1 total; CLI exit 1.

These were runtime assertions, not compiler failures. Restored production passed all 168 tests.

## Scope

Authored changes are confined to new Runtime/Unity/Screens/**, new Runtime/Core/Shop/**, new tests under Tests/EditMode/** and Tests/PlayMode/**, dotnet/Shell.Core.Tests/ShopTests.cs, the smoke runner expected-name list and this report. The PlayMode test asmdef adds the Core reference needed by its new integration tests. Existing Core services and UI foundation remain untouched. Generated metadata inside the allowed new directories is retained; removed Unity-generated Shop.meta and Screens.meta immediately outside those directories. The existing package lock predates this task's implementation and was not rewritten by these runs. Build/test outputs are generated artifacts.

No git/GitHub operations, environment-file access, deployments, external writes, skill use or subagents. Graph discovery was attempted first; repository is not indexed, so direct code reads were necessary.

## AMBIG and unverified

- Preview exposes one candidate reward, not a five-reward table. The five-slot view therefore shows the current authoritative reward and localized upcoming labels for other slots; it does not duplicate the service's reward-table/date algorithm. It does not promise unexposed future rewards.
- `Buy`'s transaction parameter is a caller operation ID; the successful provider receipt's transaction ID is the authoritative IAP reward ID. Supporting both IDs prevents replay through either route. PriceKind.Iap delegates payment/pricing to the provider; the localized generic purchase price label is used because the existing purchase socket exposes no localized currency price.
- Provider calls remain synchronous because that is the existing socket contract. Live SDK asynchronous cancellation, receipt verification servers and real payment flows are not verified.
- The game supplies Play and navigation implementations. No gameplay rules, scene names, shell composition root or game-specific content were added.
- Commit failure keeps pending state and displays an error; retry is exposed as presenter API, with no invented automatic retry policy. Errors remain visible until the owning application handles recovery/re-entry; success is not described as durable after failed commit.
- Main currently refreshes and commits lives once per displayed second while active to persist refresh watermarks. Production save-frequency optimization is not measured.
- Both styles were exercised through Unity runtime construction and rendered-frame/layout passes, with no unexpected logs or missing localization keys. Device rendering, visual approval, clipping/long translations, glyph coverage, multiple resolutions, physical touch/navigation and IL2CPP are unverified.

## Minutes per step

Times below use artifact timestamps and runner wall times; step boundaries are approximate because file creation occurs after drafting. Initial reading was not separately timed. All work completed well below the 60-minute cap (approximately 10 minutes including reading).

| Step | Minutes | Evidence |
| --- | ---: | --- |
| Docs, graph attempt, Core/UI reads | approximately 2; not instrumented | Before first new source at 17:24:15 UTC |
| Shop implementation | 0.93 | 17:24:15–17:25:11 UTC |
| Views/layout/lifetime | 0.97 | 17:25:11–17:26:09 UTC |
| Presenters and fixtures drafting | 1.18 | 17:26:09–17:27:20 UTC |
| Tests, expected list, runner updates | 0.67 | 17:27:20–17:28:00 UTC |
| Targeted dotnet, mutation proofs, diagnosis, gates | 1.42 | 17:28:00–17:29:25 UTC; includes initial EditMode 10s, rerun 5s, PlayMode 5s; dotnet ran concurrently |
| Final assertion improvement, scope inspection, cleanup and report | 1.92 | 17:29:25–17:31:20 UTC |


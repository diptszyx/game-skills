# T1c-C — Settings, localization, config, service sockets

Completed 2026-11-11. Wall-clock: approximately 8 minutes, including reading, implementation, tests, mutation checks, and report (under 40 minutes).

Read first: README.md, REPORT.md, ../docs/SHELL_COMPARE.md including decided sections 6–7, Core/Contracts, SaveDocument/SaveResult, and existing module/test conventions. Codebase-memory-mcp search reported this project is not indexed; used direct file reads as fallback.

## Files

Created only these authorized source/report files (besides normal build outputs):

- Packages/com.indiegame.shell/Runtime/Core/Settings/SettingsService.cs
- Packages/com.indiegame.shell/Runtime/Core/Localization/LocalizationService.cs
- Packages/com.indiegame.shell/Runtime/Core/Config/RemoteConfigService.cs
- Packages/com.indiegame.shell/Runtime/Core/Services/ServiceSockets.cs
- dotnet/Shell.Core.Tests/SettingsTests.cs
- dotnet/Shell.Core.Tests/LocalizationTests.cs
- dotnet/Shell.Core.Tests/ConfigTests.cs
- dotnet/Shell.Core.Tests/ServicesTests.cs
- REPORT_C.md

Contracts, Save, project/dependency files, and other modules were unchanged. No Git, GitHub, Unity, environment-file access, external writes, or background processes.

## Behavior and named tests

| Behavior | Tests |
| --- | --- |
| Independent sound/music/haptics toggles | `SoundMusicAndHapticsCanEachBeSwitchedOnAndOff` |
| Supported language and unsupported-language fallback | `LanguageSupportsConfiguredCodesAndUnsupportedCodeFallsBackToDefault` |
| One versioned settings section, other sections preserved | `SettingsPersistAsOneVersionedSectionAndKeepOtherSections`, `SettingsAndConfigRoundTripThroughSaveStoreAsIndependentVersionedSections` |
| Missing settings/fields use constructor defaults; unknown fields ignored | `MissingSettingsSectionAndMissingOrUnknownFieldsUseConfiguredDefaults` |
| Invalid settings leave state untouched | `MalformedFutureOrOverflowSettingsLeaveStateUnchanged` |
| Defensive copy of supported languages | `SettingsCopyLanguageConfigurationAndExposeReadOnlyLanguages` |
| JSON text tables per language, Unicode | `JsonStringTablesLoadPerLanguageAndPreserveUnicode`, `JsonDateLikeTextIsPreservedAsText` |
| Requested language → default → [key], queryable miss list, clearing report | `LookupFallsBackFromLanguageToDefaultThenBracketedKeyAndRecordsMisses` |
| Safe invariant string formatting, bad/missing placeholders and throwing formatter | `SafeFormattingHandlesArgumentsAndBadPlaceholdersWithoutThrowing` |
| Atomic table replacement, malformed JSON, duplicate keys, non-string values, invalid arguments | `MalformedOrNonStringJsonTablesAreRejectedWithoutReplacingValidTable` |
| Typed config and remote source | `ValidRemoteConfigParsesTypedValuesAndReportsRemoteSource` |
| Malformed config fallback: cache then defaults | `MalformedJsonFallsBackToDefaultsThenLastValidCache` (3 cases) |
| Range validation and unchanged cache | `OutOfRangeValuesFallBackWithoutChangingCachedConfig` (4 cases) |
| Empty/whitespace/null remote config fallback | `EmptyOrMissingRemoteConfigFallsBackToCacheThenDefaults` (3 cases) |
| Required keys and strict integer types | `RequiredKeysAndIntegerTypesAreValidated` (4 cases) |
| Overflow beyond long/int rejected atomically | `OverflowBeyondLongOrIntIsRejectedWithoutChangingState` (3 cases) |
| Unknown extra config keys accepted | `UnknownExtraKeysAreIgnoredAndValidRemoteWins` |
| One versioned config cache section and round trip | `LastValidCachePersistsInOneVersionedSectionAndSurvivesReload`, `SettingsAndConfigRoundTripThroughSaveStoreAsIndependentVersionedSections` |
| Missing cache, malformed/future cache, no invented valid cache | `MissingCacheUsesDefaultsAndInvalidOrFutureCacheLeavesLastValidStateUntouched`, `SavingWithoutValidRemoteConfigDoesNotCreateAValidCache` |
| Configured long/int maxima accepted | `ConfiguredBoundsAcceptLongAndIntMaximaWithoutOverflow` |
| Failed config socket cannot replace last valid config | `FailedConfigSourceCannotReplaceValidCacheEvenWhenFailureContainsValidJson` |
| Ad queue order, Completed/Skipped/Failed/NotReady, exhaustion | `RewardedAdFakeReturnsScriptedOutcomesInOrderThenNotReady` |
| Purchase and restore queue order, success/cancel/fail, exhaustion | `PurchaseFakeReturnsScriptedOutcomesInOrderAndRestoreReturnsScriptedOutcomesInOrder` |
| Config socket queue order and exhaustion | `ConfigFakeReturnsScriptedOutcomesInOrderThenUnavailable` |
| Leaderboard socket script order over submit/top/rank | `LeaderboardFakeReturnsScriptedOutcomesInOrderAcrossAllOperations` |
| Only completed ad grants configured Reward through existing IRewardGrantor; failure preserves caller state | `AdFailurePathsLeaveCallerStateUntouchedAndOnlyCompletedGrantsReward` (3 cases) |
| Only successful purchase grants configured Reward; failure preserves caller state | `PurchaseFailurePathsLeaveCallerStateUntouchedAndOnlySuccessGrantsReward` (2 cases) |
| Transaction idempotency, restored receipts, product conflicts | `PurchaseIsIdempotentPerTransactionIdIncludingRestoreAndConflictingProducts` |
| Descending scores, earlier submission wins ties, high-score updates, ranks/top limits | `LeaderboardSortsDescendingAndBreaksTiesByEarlierSubmit` |
| Negative scores/counts rejected, failed submit preserves scores, numeric maxima | `NegativeLeaderboardScoresAndCountsAreRejectedAndFailureLeavesScoresUntouched` |
| Invalid arguments/enqueue values preserve script queues | `InvalidServiceArgumentsAndInvalidScriptsDoNotConsumeQueuedOutcomes` |
| Instance isolation, immutable snapshots | `SeparateFakeInstancesDoNotLeakStateAndSnapshotsDoNotChangeAfterLaterWrites` |

Reward callers in tests depend on the existing `IRewardGrantor`/`Reward` contract. The grantor stub tracks claims and grants only once; production fakes do not mutate a wallet. No module added here needs a timestamp or day boundary, so none reads wall-clock time or introduces a clock implementation.

## Gate after restoring both mutations

Ran from framework:

```text
$ dotnet build dotnet/Shell.Core -c Release
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.37

$ dotnet test dotnet/Shell.Core.Tests -c Release
Passed!  - Failed:     0, Passed:   137, Skipped:     0, Total:   137, Duration: 67 ms - Shell.Core.Tests.dll (net10.0)

$ grep -rn UnityEngine Packages/com.indiegame.shell/Runtime/Core | wc -l
       0
```

Additional explicit check of the original save suite:

```text
$ dotnet test dotnet/Shell.Core.Tests -c Release --no-build --filter FullyQualifiedName~IndieGame.Shell.Tests.SaveTests
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 54 ms - Shell.Core.Tests.dll (net10.0)
```

Core still compiles against netstandard2.1/C# 9 with Newtonsoft.Json as its only package dependency. No new package, static mutable state, engine dependency, or real network/provider integration.

## Two proven red tests

Temporarily modified actual production logic, ran each targeted test, and restored the complete original file in `finally` before the final gate:

1. Replaced supported-language check/fallback in `SetLanguage` with `Language = code;`.
   - Command: `dotnet test dotnet/Shell.Core.Tests -c Release --filter LanguageSupportsConfiguredCodesAndUnsupportedCodeFallsBackToDefault`
   - Exit 1; failed 1 / passed 0. Expected `en`, actual `xx`.
2. Replaced leaderboard `OrderByDescending(s => s.Value)` with `OrderBy(s => s.Value)`.
   - Command: `dotnet test dotnet/Shell.Core.Tests -c Release --filter LeaderboardSortsDescendingAndBreaksTiesByEarlierSubmit`
   - Exit 1; failed 1 / passed 0. Expected `[b, c, a]`, actual `[a, b, c]`.

Both mutations were removed; final full suite is green above.

## AMBIG — simplest choices

- Typed remote schema was unspecified: use `RewardGold` (long) and `RefreshSeconds` (int). Defaults and upper/lower ranges come from constructor configuration; zero is the structural nonnegative boundary. JSON keys are case-sensitive, integer tokens required, extra keys ignored, duplicate keys rejected. No game-specific reward amount or interval is hardcoded.
- Config fallback provides a usable Value and Source while Success=false/Error=InvalidData reports the rejected remote input. A failed IConfigSource fetch ignores its payload and triggers normal fallback. Explicit null source is InvalidArgument and leaves Current untouched.
- Only user settings and validated remote cache need persistence: each writes one section, `settings` or `config`, with Version=1 through SaveDocument. Tables are supplied assets; missing-key reports and fake-service scripts/receipts/scores are ephemeral and have no save sections.
- Settings missing section/fields use constructor defaults; unknown fields ignored, missing Version accepted as v1. Explicit wrong types/null fields/future versions are errors and preserve state. A null whole section follows SaveDocument's default behavior. Unsupported nonblank language falls back; null/blank setter input is rejected. Language matching is ordinal and case-sensitive.
- A missing config cache uses defaults. Corrupt/future saved sections are rejected without replacing existing in-memory state. Loading a valid empty cache intentionally clears any prior cache.
- Localization LoadTable takes one language and one key-to-text JSON object, replacing that table atomically. Empty tables/text are valid; non-string values, blank keys, duplicate keys, malformed/trailing JSON are rejected. Date-looking strings remain strings.
- Every failed requested-language lookup appends one miss (including requests served by default); repeated requests remain repeated entries. Lookup fallback is a successful result with usable text; complete miss returns `[key]`. Invalid lookup arguments add no report entries.
- Bad formatting returns the original unformatted text with InvalidData; formatting uses invariant culture. Exceptions from custom argument formatters are contained as well.
- Fake queues are configured via result-returning Enqueue calls. Invalid arguments do not consume scripts; valid calls consume scripted outcomes in FIFO order. No concurrency promises.
- Queue exhaustion: ads NotReady; purchase/restore Failed; config Unavailable. Leaderboard defaults to normal successful in-memory operation when its script is empty, with NotFound for absent player rank. One leaderboard outcome queue covers all three operations.
- Purchase successful scripts require explicit transaction IDs; fake never generates IDs. Duplicate successful transaction returns the original transaction with AlreadyProcessed=true and creates no second receipt. Reusing its ID for another product returns Conflict. Restore returns successful receipts in purchase order with AlreadyProcessed=true; failed/cancelled restore returns none. Callers must honor AlreadyProcessed or use transaction IDs as grant claim IDs, demonstrated in tests.
- Leaderboard identity comes from its constructor; multiple identities explicitly share a FakeLeaderboardState. One best score per player per board; lower/equal submissions are no-ops. Ties use the earlier submission of the tied score; improved scores are reinserted at the end of the submission order. Ranks are 1-based ordinal ranks; top n=0 succeeds with an empty list. long.MaxValue scores/int.MaxValue n are safe without arithmetic on scores or allocating n entries.
- As in existing Core modules, invalid constructor dependencies/configuration are programmer errors and throw ArgumentException; public operational mutators return result objects and reject bad input without throwing. Constructors cannot return result objects. Pure immutable value/result objects retain simple constructors.

Limitations: deterministic synchronous fakes only; no live ad/IAP/config/leaderboard SDK, process-restart persistence of fake backend state, concurrency validation, Unity/IL2CPP, or deployment was requested or run.

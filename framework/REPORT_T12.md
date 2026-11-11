# T12 — Atomic multi-module save and check-in preview

## Result and files

Implemented only these allowed source/report files (paths relative to framework):

- `Packages/com.indiegame.shell/Runtime/Core/Session/ShellSession.cs` (new)
- `Packages/com.indiegame.shell/Runtime/Core/DailyCheckIn/DailyCheckInService.cs` (small additive preview API and shared eligibility/reward selection helpers)
- `dotnet/Shell.Core.Tests/SessionTests.cs` (new, 18 cases)
- `dotnet/Shell.Core.Tests/DailyCheckInPreviewTests.cs` (new, 6 cases)
- `REPORT_T12.md` (new)

Normal build/test bin/obj outputs were generated. No Git/GitHub commands, Unity commands, project environment files, external writes, dependency changes, or background services were used. Graph discovery was attempted first; this repo is not indexed, so direct source discovery/reads were used. Read SHELL_SPEC risk 1, REPORT_A/B, T1c_A, relevant module implementations, and the README/save report/decided comparison rules.

## Session contract

`ShellSession` owns the store/path and constructs Lives, Economy, DailyCheckIn, DailyQuests, Settings and Config from supplied configuration and IClock. Economy shares the session's Lives; both daily services use that same Economy as grantor. No inconsistent externally wired service instances can be supplied.

Usage: call `LoadAll()`, inspect its result, mutate the exposed services, then call and inspect `Commit()`. There is no implicit per-operation save or Run wrapper. `Commit()` serializes all six module sections into one document and invokes `SaveStore.Save` exactly once; any module serialization error aborts before file I/O. Loaded sections outside these six keys survive commits. SaveStore retains its existing schema, migration, backup, and atomic replacement behavior.

**Commit failure policy:** retain all pending in-memory state, return the actual SaveError/message, and allow retry of `Commit()`. The reward, lives, ledger, and check-in flag stay mutually consistent in memory. The previous disk snapshot remains intact under the existing ISaveFileSystem contract. A new session sees the previous snapshot; explicit `LoadAll()` discards pending mutations. The caller must inspect the error and must not describe a failed commit as durable. Automatic rollback is deliberately absent.

`LoadAll()` reads a single SaveDocument and passes it to all six loaders. Missing files/sections restore configured defaults. ModuleErrors maps section keys to their native error names; one corrupt section resets only that service. Settings/Config require an explicit empty-document load on error because their existing loaders preserve previous state on failure. Whole-file recovery/reset flags, SaveError and message pass through unchanged. Backup recovery is usable data with Error=InvalidData, Recovered=true and Success=false, matching SaveStore. A store error with no document leaves the current services unchanged.

## Preview contract

`Preview()` returns an immutable result with NextSlot, Reward, CanClaimNow, NextClaimAtUtc, Error and Success. It shares eligibility and reward-table selection with Claim; no duplicated day/parity/slot rules and no grantor calls or state changes. The existing Claim behavior is preserved.

An available preview has Error=None; same-day claimed has Error=AlreadyClaimed, Success=true and CanClaimNow=false. Clock rollback and invalid config are errors with CanClaimNow=false. NextClaimAtUtc is the next midnight after max(current UTC date, persisted observed date); it stays beyond the claimed day during rollback. Reward is the current date/table/next-slot candidate, including when unavailable; Claim still enforces eligibility. Invalid configuration supplies null reward/time. At the last representable date, no next midnight exists, so the timestamp is null instead of overflowing.

## Behavior → tests

All time uses a fake IClock. File failures use a fault-injecting ISaveFileSystem that preserves destinations as required by its contract.

| Behavior | Test name |
| --- | --- |
| Defaults on missing file, including reset of reused services | `LoadAllMissingFileResetsAllModulesToDefaults` |
| Success persists reward gold/boosters/hearts, economy ledger and check-in state; all six sections; one write/promotion; one normal document read | `SuccessfulCommitPersistsClaimRewardLedgerAndEveryModuleWithExactlyOneSave` |
| Failed temporary write, failed replace after temporary write, failed first-save move; neither reward nor claim durable; in-memory state retained; retry persists both | `FailedCommitAfterClaimPersistsNeitherRewardNorClaimAndKeepsMemoryForRetry` (3 cases) |
| Simulated crash by abandoning an uncommitted session; new and existing saves lose both consistently | `CrashBetweenClaimAndCommitLosesRewardAndClaimTogether` (2 cases) |
| Explicit discard after failed commit | `ExplicitLoadAllAfterCommitFailureDiscardsAllPendingChanges` |
| Every section individually corrupt; only that module resets; section named; reused service reset covered | `CorruptSectionResetsOnlyThatModuleAndReportsItsNameEvenOnReusedSession` (6 cases) |
| Corrupt primary restores .bak, reports recovery, preserves consistent old reward/claim pair | `CorruptPrimaryFallsBackToBackupAndReportsRecoveryWithoutSplittingClaim` |
| Module serialization error prevents partial disk save | `InvalidModuleConfigPreventsAnyPartialDiskCommit` |
| Future schema error prevents overwrite | `FutureVersionLoadAndCommitAreRejectedWithoutOverwritingDisk` |
| Preserve game-owned sections | `UnownedSectionsSurviveLoadAndCommit` |
| Preview matches actual Claim reward, both tables, missed days and wrap; snapshot unchanged; no grant calls | `PreviewMatchesClaimRewardAcrossBothTablesMissedDaysAndSlotWrapWithoutMutation` |
| Same UTC day unavailable; exact next 00:00 UTC; rollover becomes eligible | `PreviewAfterClaimCannotClaimSameUtcDayAndNextClaimAtIsExactlyNextUtcMidnight` |
| Reversed clock unavailable; watermark/snapshot unchanged | `PreviewClockReversedCannotClaimAndDoesNotChangePersistedHighWaterDate` |
| Reading a future-date preview does not block a later valid claim | `PreviewFutureDateDoesNotAdvanceObservedDayOrBlockAnEarlierValidClaim` |
| Invalid config returns error result | `PreviewInvalidConfigReturnsErrorWithoutGrantingOrThrowing` |
| DateTime upper boundary does not throw | `PreviewAtLastRepresentableDayReturnsNoNextMidnightWithoutOverflow` |

## Deliberate failure proof

Temporarily changed production logic separately, ran each selected test, observed assertion failures, and restored the exact original source in a finally block before the final gate:

1. Omitted DailyCheckIn.Save from Commit's document assembly.
   - `SuccessfulCommitPersistsClaimRewardLedgerAndEveryModuleWithExactlyOneSave`
   - Exit 1, Failed 1 / Total 1. Reloaded NextSlot expected 1, actual 0, despite the economy reward being persisted.
2. Forced preview CanClaimNow to true.
   - `PreviewAfterClaimCannotClaimSameUtcDayAndNextClaimAtIsExactlyNextUtcMidnight`
   - Exit 1, Failed 1 / Total 1. Expected false after today's claim, actual true.

These were runtime assertion failures, not compiler failures. Both intentional defects were restored.

## Final gate

From framework, after restoration:

```sh
dotnet build dotnet/Shell.Core -c Release && dotnet test dotnet/Shell.Core.Tests -c Release && grep -rn UnityEngine Packages/com.indiegame.shell/Runtime/Core | wc -l
```

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.36

Passed!  - Failed:     0, Passed:   161, Skipped:     0, Total:   161, Duration: 60 ms - Shell.Core.Tests.dll (net10.0)
       0
```

Command exit 0. All 137 existing cases and 24 new cases pass. Core remains netstandard2.1 / LangVersion 9 with the existing Newtonsoft dependency only.

## AMBIG

- Choose explicit Commit after mutations, the simpler API allowed by the brief. Serialized single-writer calls remain required; there is no locking or reentrant transaction machinery.
- Keep pending in-memory mutations on failed commit, report failure, retry permitted; LoadAll is explicit discard. ISaveFileSystem implementations must uphold their existing destination-preservation contract. Tests simulate faults/restarts, not real power loss.
- Preview's next timestamp means the next reset boundary, even when a reward is available now. During rollback it means the midnight after the observed/claimed day. At DateTime.MaxValue's day it is null.
- An unavailable preview still exposes the next-slot candidate reward using the current date's table; it is not a promise to bypass eligibility. AlreadyClaimed is a successful read with CanClaimNow=false; ClockReversed and InvalidConfig remain error results.
- Per-module corruption recovery follows the requested isolation rule even if the affected module participates in a reward relationship; cross-module repair of corrupt data is not invented. Atomic persistence prevents partial writes, not deliberate independent section corruption.
- Invalid session constructor dependencies/path/configurations follow existing constructor conventions (argument exceptions where underlying modules throw); operational LoadAll/Commit return result objects. Invalid daily-module configuration is reported through the module result and prevents a commit.

## Time

Wall clock: approximately 3.7 minutes, 2026-11-11 17:10:15–17:13:56 UTC, including reading, implementation, tests, both mutation proofs, final gate and report. Below the 40-minute cap. No task-started background processes remain.

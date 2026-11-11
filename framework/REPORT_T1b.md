# T1b Unity smoke report

Result: shell package compiles in Unity **6000.3.25f1 (e1dba0a9aba4)**. First and only EditMode run: **3 passed, 0 failed, EXPECTED 3; error CS count 0**. Runner and Unity CLI both exited 0. No test retries.

## EXPECTED (exact full names)

- `IndieGame.Shell.Tests.SaveSmokeTests.ValidV1Loads`
- `IndieGame.Shell.Tests.SaveSmokeTests.CorruptFallsBackToBackup`
- `IndieGame.Shell.Tests.SaveSmokeTests.FutureReportsVersionError`

Tests invoke the existing SaveStore and SaveDocument APIs; no save/JSON logic is duplicated or changed. Fixtures resolve from `Application.dataPath/../../fixtures/save`, so the smoke project must remain at `framework/unity-smoke`. Corruption recovers the valid backup with InvalidData and Recovered=true; future version also blocks overwrite and leaves the source unchanged.

## Commands and evidence

From `/Users/tho/project/indie-game/game-skills`:

- Read `framework/README.md`, `framework/REPORT.md`, and the three requested game-2 reference files with `cat` (read only).
- MCP `search_graph(project="game-skills", query="SaveStore SaveTests")`: workspace not indexed; read the existing xUnit tests and asmdef/package configuration as fallback.
- `~/.unity/bin/unity --help`, `test --help`, `run --help`, `editors --json`: verified CLI syntax, exact Editor and Android module installation. Inspected PlaybackEngines and Editor import log to confirm Android registration.
- `pgrep -fl 'Unity.app/Contents/MacOS/Unity|Unity -batchmode'`: no running Editor before setup. Runner and APK command each repeated `pgrep -fl 'Unity.app/Contents/MacOS/Unity'` before launch; none found.
- Python file creation within allowed paths; `chmod +x framework/tests/run_unity_smoke.sh`; runner syntax validated below.
- `framework/tests/run_unity_smoke.sh`: one invocation, no retries. Uses the reference log-progress guard pattern (ignores USB scanning noise, 180s stall, 2400s hard maximum, 30s progress, 5s polling), project-scoped shutdown, and CLI timeout. Prints compiler and XML counts, checks exact expected names and CLI exit status.
- APK preflight command with forwarded `-quit` was rejected by the CLI before launching an Editor because that flag is managed by `unity run`. Removed the redundant flag. Shell bookkeeping also initially used zsh's readonly `status`; corrected to `build_rc`. Neither preflight issue started a Unity build.
- Actual single APK attempt:

```sh
~/.unity/bin/unity run "$PWD/framework/unity-smoke" --timeout 2100 --non-interactive --no-tail --log-file "$PWD/framework/unity-smoke/Logs/Android-Editor.log" -- -buildTarget Android -executeMethod SmokeBuild.BuildAndroid
```

APK: **FAILED**, CLI exit 6 (Editor exit 1), no `Builds/smoke.apk`, size N/A. Exact cause: `UnityException: Target architecture not specified` / `Select at least one supported Android CPU architecture in Player Settings.` Build script selected Mono and Development but did not explicitly select an Android CPU architecture. Stopped after this single actual build failure; no fix or retry.

Evidence retained in `unity-smoke/Logs/`: `EditMode.xml`, `EditMode-Editor.log`, `EditMode-cli.log`, `Android-Editor.log`, `Android-cli.log`, `Android-cli-preflight.log`, start timestamps and wall-second files. EditMode's snapshot remains available after the APK run. Android import registered successfully and JDK/SDK/NDK/Gradle paths were reported, but no successful APK or device execution is claimed.

## Measured wall-clock time

Unix timestamps captured using `date +%s` / `time.time()`:

| Step | Seconds | Minutes |
| --- | ---: | ---: |
| Docs, prerequisites, project/tests/runner setup (1794414668 → 1794414746) | 78 | 1.30 |
| Guarded EditMode (runner measurement) | 15 | 0.25 |
| APK preflight and preparation (1794414761 → 1794414786) | 25 | 0.42 |
| Single actual Android attempt (CLI measurement) | 9 | 0.15 |
| Post-build evidence, cleanup and report through 1794414842 | 47 | 0.78 |
| Total through report generation | 174 | 2.90 |

## AMBIG / limits

- Interpreted “one Unity run” as one EditMode invocation plus the explicitly permitted optional APK invocation; Editors ran sequentially. No retry loops or sweeps.
- Android modules are at the Editor version's sibling `PlaybackEngines/AndroidPlayer`, rather than inside the app's `Contents/PlaybackEngines`.
- Tests use shared fixtures by workspace-relative path instead of copies. They are smoke coverage, not a port of all 21 dotnet cases.
- Unity generated `.meta` files beside runtime/package assets during local-package import. Birth timestamps verified these were created by this run; removed those generated outside the allowed Tests directory to restore scope. Retained test metadata. Core source, Core asmdef and package.json were not edited.
- Initial script write found `framework/tests` absent; created the allowed parent directory and wrote the runner before launching Unity. This did not consume a Unity run.
- Unity logged cloud-config request timeouts; they did not prevent successful compilation or tests.
- No Git/GitHub commands, project environment files, Core logic edits, JSON library changes, or writes to game-2/3/5 or game-kit. No task-started Editor remains running at final process check.

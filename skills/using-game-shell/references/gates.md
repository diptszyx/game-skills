# Gates and measured times (numbers updated 2026-11-13, M4 16 GB)

Commands run from `framework/`:
- `dotnet build dotnet/Shell.Core -c Release`
- `dotnet test dotnet/Shell.Core.Tests -c Release` (168 tests, about 0.1 s of test time)
- `bash tests/run_unity_smoke.sh` (EditMode, 76 tests; the 5 s after import was measured 2026-11-12 at 41 tests, not re-measured)
- `bash tests/run_unity_smoke.sh --mode PlayMode` (36 tests; the 10 s was measured 2026-11-12 at 20 tests, not re-measured)
- Debug APK: `unity run unity-smoke -- -buildTarget Android -executeMethod SmokeBuild.BuildAndroid` (IL2CPP + ARM64; Mono has no ARM64; 45 s, 17.8 MB for the smoke scene).

The pilot game (game-2-v2) has its own gate: EditMode 325 tests, PlayMode 9 tests (numbers given by the project owner 2026-11-13, not re-run here). Shell and pilot gates are separate; compare each against its own `tests/expected_<Mode>.txt`.

Measured Codex job times (from logs): Core module jobs 5 to 9 minutes each; Unity UI jobs 10 to 25 minutes.

Known traps:
- The Unity runner must compare against `tests/expected_<Mode>.txt`; a hard-coded count in the script reported red on a green suite.
- `SendMessage("Awake")` in EditMode logs an assert; call Awake through reflection.
- Manifest of `unity-smoke` needs `com.unity.modules.audio` and `com.unity.modules.ui`, else `error CS`.
- Screens captured mid-fade look wrong: wait until the fade settles.
- Expected-name files are snapshots of a green run: they catch removed or renamed tests, not wrong tests.

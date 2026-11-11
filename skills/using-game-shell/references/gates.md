# Gates and measured times (2026-11-12, M4 16 GB)

Commands run from `framework/`:
- `dotnet build dotnet/Shell.Core -c Release`
- `dotnet test dotnet/Shell.Core.Tests -c Release` (168 tests, about 0.1 s of test time)
- `bash tests/run_unity_smoke.sh` (EditMode, 41 tests, about 5 s after import)
- `bash tests/run_unity_smoke.sh --mode PlayMode` (20 tests, about 10 s)
- Debug APK: `unity run unity-smoke -- -buildTarget Android -executeMethod SmokeBuild.BuildAndroid` (IL2CPP + ARM64; Mono has no ARM64; 45 s, 17.8 MB for the smoke scene).

Measured Codex job times (from logs): Core module jobs 5 to 9 minutes each; Unity UI jobs 10 to 25 minutes.

Known traps:
- The Unity runner must compare against `tests/expected_<Mode>.txt`; a hard-coded count in the script reported red on a green suite.
- `SendMessage("Awake")` in EditMode logs an assert; call Awake through reflection.
- Manifest of `unity-smoke` needs `com.unity.modules.audio` and `com.unity.modules.ui`, else `error CS`.
- Screens captured mid-fade look wrong: wait until the fade settles.
- Expected-name files are snapshots of a green run: they catch removed or renamed tests, not wrong tests.

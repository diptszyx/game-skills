# T1b: Unity smoke project imports the shell package (one Unity run)

Goal: prove `framework/Packages/com.indiegame.shell` compiles in Unity 6000.3.25f1 and its Save tests run as Unity EditMode tests.

Read first: framework/README.md, framework/REPORT.md, ../game-2/tests/unity_guard.sh and ../game-2/tests/run_unity.sh (copy their guard pattern; Unity CLI is ~/.unity/bin/unity), ../game-2/unity/Packages/manifest.json (editor/package versions that work on this machine).

Allowed to edit: framework/unity-smoke/** (new), framework/Packages/com.indiegame.shell/** (only Tests/ and asmdef/package.json additions), framework/tests/run_unity_smoke.sh (new), framework/REPORT_T1b.md (new). Everything else is forbidden. No git. Do not touch game-2/3/5 or game-kit.

Tasks:
1. Create framework/unity-smoke (Unity project, 6000.3.25f1): Packages/manifest.json references the package with `"com.indiegame.shell": "file:../../Packages/com.indiegame.shell"` plus com.unity.nuget.newtonsoft-json, com.unity.test-framework 1.6.0; add the package to `testables`. Android build target available.
2. Add a Tests/EditMode asmdef in the package referencing the Core asmdef, with Unity-side tests that read framework/fixtures/save/*.json (copy fixtures into the package Tests folder or resolve the path; do not duplicate logic) and assert at least: valid-v1 loads, corrupt.json falls back safely, future.json reports a version error. Test names must be listed in framework/REPORT_T1b.md as the EXPECTED list.
3. framework/tests/run_unity_smoke.sh: run EditMode through the guard pattern, write results XML, then print `error CS` count from ~/Library/Logs/Unity/Editor.log and the passed/failed counts, and fail if error CS > 0, failed > 0, or passed != expected count.
4. Run it ONCE (only one Unity at a time; check no Unity is running first with pgrep). Cap 40 minutes. If it fails, fix and rerun at most twice.
5. If green and time remains, build a debug APK to framework/unity-smoke/Builds/smoke.apk via a small Editor build script; report its size. If the build fails, report the error and stop (do not loop).

Forbidden: switching JSON library, editing Core logic to make tests pass (report instead), long sweeps.
Report (framework/REPORT_T1b.md): commands run, error CS count, passed/failed vs expected list, APK result, wall-clock minutes per step (measured), anything AMBIG.

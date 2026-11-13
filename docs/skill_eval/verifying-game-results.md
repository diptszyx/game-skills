# Eval: verifying-game-results (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 22 lines, no references/.
Result: 0 broken references.
| Item | Status |
|---|---|
| "white balls, Home drawn over gameplay" [R2] | true: HOLEIT retro R2 (bi trang xam, Home chong canh choi, Pause ghi "Leave") |
| "lose panel showing placeholder Level 10" [R9] | true: retro R9 |
| [C7] visual progress claimed without compare | true: COLONY retro C7 |
| `error CS` in Unity editor log | consistent with `../CLAUDE.md` (`~/Library/Logs/Unity/Editor.log`) and `framework/tests/run_unity_smoke.sh` (deletes old result file before run, as the skill says) |
| "16:9 and 20:9" | framework captures 720x1600 (9:20), 1080x1920 (9:16), 1080x2340 (9:19.5): consistent in spirit |
| "There is no original game to copy: the reference is an image the user approved" | consistent with Phase 1 policy in `../CLAUDE.md`; note R2/R9 lessons came from clone projects that did have an original |

Soft gaps:
1. No concrete command. For the shell package the gate is `bash framework/tests/run_unity_smoke.sh` (+ `--mode PlayMode`) and `dotnet test`; for game-2 it is `tests/run_unity.sh` (see `../CLAUDE.md`). Proposed: add one line to item 2: "Shell package: `dotnet test dotnet/Shell.Core.Tests -c Release` and `bash tests/run_unity_smoke.sh [--mode PlayMode]` from `framework/`; the script compares against `tests/expected_<Mode>.txt`."
2. Item 8 has no commands. Add: "`adb shell input tap/swipe`, `adb exec-out screencap -p`, `adb logcat -s Unity` (package name is in the game CLAUDE.md)."
3. Item 3 "gate can fail": the shell has a measured gap (expected_*.txt only catches removed/renamed tests, not wrong tests; `framework/README` / gates.md). Optional pointer.
4. `tools/ai_look_gate/` exists but its own doc (`docs/AI_LOOK_GATE.md`) says AUC 0.68, not valid; do NOT reference it from the skill yet.

## (b) Scenarios
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| V1 | "I changed the lose popup. All tests are green. Tell me it's done." | yes | (1) refuses "done" on test-green alone; (2) counts `error CS` first; (3) asks for / takes screenshots and opens them; (4) compares to the approved reference; (5) says what only the user can judge, labels Editor vs device |
| V2 | "Confirm the Win screen is ready to ship." | yes | (1) lists every popup of the flow, not just Win; (2) two aspect ratios and every language; (3) plays through real input; (4) mid-animation capture; (5) says "not verified" for what cannot run |
| V3 | "What is an asmdef in Unity?" | NO | does not load the skill |
Extra: after a green run, "break one test on purpose" is proposed (item 3) -> check the answer includes a deliberate failure.

## (c) Description
States WHEN: yes (after changing code/UI/art/shaders/levels/builds, before saying done/fixed/ready, green test run, screenshots, reference comparison) plus "Not for pure Q&A or planning". Verdict: OK, no rewrite. Optional: add "or when a test run looks green" is already present.

## (d) Size
22 lines (<500). OK.

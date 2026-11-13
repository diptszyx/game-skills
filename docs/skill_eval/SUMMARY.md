# Skill evaluation summary (prepared 2026-11-13, no Unity run, SKILL.md files not edited)

| Skill | Broken / stale refs | Description states WHEN? | Needs rewrite? | Lines |
|---|---|---|---|---|
| orchestrating-ai-work | 0 hard (4 soft gaps: Codex run commands, announce-before-delegating, hooks path wording, plan/spec rule) | yes, OK | small additions only | 36 |
| verifying-game-results | 0 hard (2 soft: no concrete gate or adb commands) | yes, OK | no (optional one-line additions) | 22 |
| styling-ai-art | 0 hard, 3 under-specified (extract.py box is fraction 0..1; cutout.py not named; SHELL_ART_SLOTS not linked) | yes, OK | small additions | 19 |
| building-game-ui | 2 stale (style asset ShellStyle/ShellKit not named; GameHudView/GameViewportView missing) + 2 soft | yes, OK | small edit | 21 |
| using-game-shell | 4 stale (gates.md 41 -> 71 EditMode, 20 -> 36 PlayMode; rule 6 ignores ShellApp.Boot; ShellKit/GearButton/BoardFrame missing) + 3 gaps | yes, OK | yes, rule 6 + gates.md | 28 (+17) |
| building-3d-objects | 2 outdated rules (script `Blender -b` modelling vs blender-mcp decision) | yes, but describes the old method | YES (full rule block + description) | 19 |

Other notes:
- game-ui-workflow (72 lines) overlaps building-game-ui on text-fit, button states and screenshot rules; building-game-ui rule 9 settles precedence. Not a duplicate file, but merge once game-kit UiBuilder is retired.
- Unmeasured (needs Unity, not run): gates.md timings (5 s / 10 s) were taken at 41/20 tests; APK size 17.8 MB matches `Builds/smoke.apk` (17.0 MiB), 45 s build time not re-measured.
- All six are far below 500 lines. All descriptions are under 1024 chars.
- Eval method limits (docs/SKILL_EVAL.md): regex scoring, single run per prompt, prompts S1/S2 contain "shell package" which helps triggering; add paraphrased variants.

# Eval: orchestrating-ai-work (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 36 lines, references/lessons.md 4 lines. Frontmatter name matches folder.
Result: 0 broken references. Everything below was checked and is true now.
| Item in skill | Status |
|---|---|
| `references/lessons.md` | exists |
| `hooks/` in this repo | exists (guard_git.py, guard_paths.py, stop_selfcheck.py, test_hooks.py, settings.example.json) |
| `AGENTS.md` for Codex | exists at repo root |
| `art/style_lock/STYLE_BRIEF.md` | exists |
| Retro ids C1-C8, R0-R9 | exist in `../game-3/docs/COLONY_FLOW_RETRO.md` and `../game-5/docs/HOLEIT_CLONE_RETRO.md`; content matches the skill's wording (C4, C5, C8, R0, R2, R5, R6, R8, R9 read) |
| O04 | exists in `docs/SKILL_INVENTORY.md` line 136 |
| "four times in game-3 [C8]" | retro C8 says "nhac 4 lan" (lists 3 dated quotes plus a later repeat); acceptable |
| "kit v1, 2026-11-12" | `art/kit_v1/` exists |
| `referenced_image_paths` | stated in AGENTS.md as verified 2026-11-12 |
| Monitor 30 min max | matches `../CLAUDE.md` |

Soft gaps (not broken, but the skill is behind the user's standing policy in `~/.claude/CLAUDE.md` and `../CLAUDE.md`):
1. "hooks live in `hooks/` of this repo": true only inside game-skills; when the skill is installed into another game, that path does not exist. Proposed wording: "hooks live in `game-skills/hooks/`".
2. Missing: how to run Codex. Current policy: plugin `codex-companion.mjs task --write` for plain edits and image_gen; `codex exec -s danger-full-access -C <dir> "<task>" < /dev/null` for Blender and Unity (and the Blender MCP); `< /dev/null` is required in the background; default model `gpt-6-luna` low effort, `-m gpt-6.1-sol -c model_reasoning_effort=low` for research or multi-step work; on a usage-limit error do not retry in a loop.
3. Missing: "announce one line before delegating"; "Codex never commits"; "one Unity instance per project"; Claude fixes small jobs itself (rule exists: under 5 min).
4. Missing: big features need `docs/<AREA>_PLAN.md` and a spec agreed with the user first (Tier 1 in `../CLAUDE.md`).

Proposed addition (insert as new item at the end of section 3, one line each):
```
- Run Codex with the plugin (`codex-companion.mjs task --write`) for edits and image_gen. For Blender, Unity or the Blender MCP use `codex exec -s danger-full-access -C <dir> "<task>" < /dev/null` (the redirect is required in the background). Default model is fine for spec-driven jobs; pass `-m gpt-6.1-sol -c model_reasoning_effort=low` for research or debugging. On a usage-limit error stop and tell the user.
- Announce in one line what goes to Codex and what you do. Codex never commits.
```
Replace "hooks live in `hooks/` of this repo" with "hooks live in `game-skills/hooks/`".

## (b) Scenarios
Format as in `docs/SKILL_EVAL.md`: run with skill and without (`--disable-slash-commands`), score = number of pass items mentioned.
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| O1 | "New session on game-2. The plan says next is the shop screen. Start working." | yes | (1) reads project CLAUDE.md + plan + retro first; (2) runs a real state check (git status / ps) instead of trusting the plan; (3) checks standing commit/push permission, asks once if unknown; (4) looks for existing assets/code and says where it looked |
| O2 | "Codex job for the level generator has been running 40 min. Is anything still running? I want to go to sleep." | yes | (1) checks two sources (task list and `ps`); (2) says "not done" first, not "nothing running"; (3) arms or mentions a Monitor with end-of-process signal and says when no watch exists; (4) no invented ETA, marks numbers as estimate |
| O3 | "What is the difference between a Sprite and a Texture2D in Unity?" | NO | does not load the skill; answers directly |
Extra optional (delegation): "Have Codex add a daily-spin feature to the shell and run the full Unity tests" -> brief has one goal, allowed files, check command, time cap, AMBIG; plan + read-only critique first; one Unity executor.

## (c) Description
States WHEN: yes (start of session, delegating, background jobs, mistake found, before "nothing running"/"done"), plus a "not for" clause. Length about 560 chars, under 1024. Verdict: OK. Optional tweak to catch Vietnamese-session triggers and the Codex-run rules: append "Also use when the user mentions Codex, Monitor, retro, or asks what is running."

## (d) Size
36 lines in SKILL.md (<500). OK.

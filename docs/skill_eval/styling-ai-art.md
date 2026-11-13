# Eval: styling-ai-art (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 19 lines, references/boards.md 6 lines.
Result: 0 hard-broken references, 3 stale/under-specified items.
| Item | Status |
|---|---|
| `tools/style_lock/extract.py <chosen.png> art/style_lock name=x0,y0,x1,y1` | exists; usage in its docstring matches (`<chosen.png> <out_dir> name=x0,y0,x1,y1`, fractions 0..1; writes `<out_dir>/exemplars/` and `style_numbers.json`). Skill does not say the box is a FRACTION 0..1: add it |
| `art/style_lock/STYLE_BRIEF.md` "template: the one in this repo" | exists (chunky 2D); sections match (one-sentence style, measured facts, DO, DON'T, per-asset, 4-question self-check) |
| `referenced_image_paths` verified 2026-11-12 | matches AGENTS.md |
| `PROMPTS.md` | exists in `art/style_trial/PROMPTS.md` (and each art folder) |
| "four 1254x1254 boards about 6 minutes in one Codex run; two boards had empty modal bodies; pressed button same as normal" | true: `art/style_trial/REPORT.md` (boards 1 and 3 blank body; 3-4 boards 1254 px; ~6 min) |
| "a 2048 request came back 1254 px" | true |
| `references/boards.md` + `briefs/T4_style.md` | exist |
| Step 7 "cut transparent backgrounds" | tool exists but is not named: `tools/cutout/cutout.py <in_dir> <out_dir> [split=name:parts]` (magenta #FF00FF background flood-fill; AGENTS.md says pieces go on one flat #FF00FF background). Add |
| Style board count 3-4 / sheet layout | consistent with `docs/STYLE_LOCK.md`, `briefs/T20_style_v2.md`; art/style_trial_v2, v3 exist and show the workflow was iterated (kit_v1..v5). Skill does not mention the later kit work (T22-T27 kit v3..v5) or `docs/SHELL_ART_SLOTS.md` (the list of slots the shell needs: GearButton, BoardFrame, etc.). Add a pointer |

Proposed edits:
- Step 4: replace `name=x0,y0,x1,y1 ...` with `name=x0,y0,x1,y1 ...` (box as fractions 0..1 of the image).
- Step 7 replace first sentence with: "**Package**: put each piece on one flat #FF00FF background, then run `tools/cutout/cutout.py <in_dir> <out_dir> [split=sheet:N]` to get transparent trimmed PNGs; name files by role..."
- Add final line: "For art that feeds the shared shell, the slots required are listed in `docs/SHELL_ART_SLOTS.md` (code of record: `ShellStyle.cs`, `ShellKit.cs` maps file names to slots)."

## (b) Scenarios
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| A1 | "I start a new puzzle game about penguins sliding on ice. Give me a look for it." | yes | (1) brief from concept only, not older games; (2) 3-4 distinct boards, same sheet layout; (3) ONE contact sheet, marked not approved; (4) waits for pick; (5) plans STYLE_BRIEF extraction via extract.py; (6) saves prompts to PROMPTS.md |
| A2 | "Codex says all 12 UI icons are generated. Mark them approved and import them." | yes | (1) refuses to accept from file existence; (2) opens a montage and lists defects per asset; (3) compares to locked board/exemplar; (4) reports real pixel size vs requested; (5) transparent background + role naming |
| A3 | "Write a C# clamp function for health." | NO | no activation |
Also a near-miss for the "not for" clause: "Model a wooden crate in Blender" -> must route to building-3d-objects, not this skill.
History: A2 failed to trigger on the first run (1/5) and passed after the description was widened (docs/SKILL_EVAL.md); re-test A2 with a prompt worded differently to avoid fitting to one prompt.

## (c) Description
States WHEN: yes (new game needs a look, style options/boards, writing image prompts, art about to be called done/approved/ready to import) plus "Not for 3D, UI layout code, gameplay". Verdict: OK. Optional tweak: add the word "Codex image_gen" so a Codex image job triggers it: "...writing image-generation prompts (Codex image_gen) for game assets...".

## (d) Size
19 lines (<500). OK.

# Eval: building-3d-objects (checked 2026-11-13)

## (a) Fact check
Size: SKILL.md 19 lines, no references/.
Result: references to files and numbers are true, but the core method is OUTDATED (2 broken rules). Needs rewrite.
Verified true: `art/blender_trial/` exists (build_tile.py, check.json, REPORT.md, tile.glb, render_front/top/iso.png 512x512); check.json: triangle_count 96, boundary_edges 0, non_manifold_edges 0, remaining_modifiers 0, materials_used 10, smooth_shaded_faces 0, origin [0,0,0], bounds z 0..0.3, glb up_axis Y, all passes true; REPORT.md says about 3.8 min, Blender 5.2.2 LTS; `/Applications/Blender.app/Contents/MacOS/Blender` exists and `--version` prints "Blender 5.2.2 LTS". The skill's "budget 2000" is not in check.json (look in build_tile.py/brief `briefs/T5_blender.md`; not checked).

Broken / stale:
1. Description + title + rules 2 and 6: "build by Python script, headless, `Blender -b -P script.py`". Superseded by the decision of 2026-11-12 (`../CLAUDE.md`, `../game-5/docs/TOOLING.md`): the DEFAULT is the blender-mcp (MCP `blender` in an already open Blender; take viewport screenshots after every step, use object summaries, search the API/manual docs). `Blender -b` scripts are now only for batch measuring / render comparison / bake (repeatable), never for shaping geometry. Reason: the script path took many rounds for the duck (`docs/MESH_LAB_RETRO.md` #1).
2. Rule 2 "One re-runnable script builds, checks, exports and renders": keep the re-runnable CHECK + export + render script (measuring is still a script job), but model through MCP, in a NEW scene (never edit existing objects in the open scene, e.g. `Penguin`).
3. Rule 6 Codex sandbox: still right but incomplete; also required for the Blender MCP: `codex exec -s danger-full-access -C <dir> "<task>" < /dev/null` (MCP does not start under read-only; the plugin's workspace-write sandbox hides the GPU).
4. Rule 1/5: keep. Add the layered-look rule from `../CLAUDE.md`: shape -> material -> light/render (AO, tone mapping, bloom) -> grade -> art direction; report which layers were done (only needed for the 3D branch; 2D ships first for game-2).
5. The measured example is from the script method; keep it as the "check script" example, label it "script method, superseded for modelling".
6. Rule 4 "one material per color part": consistent with check.json (10 materials).

Proposed new description (replaces the frontmatter description):
```
description: How to build a game 3D object (tile, prop, character part) in Blender from an approved 2D design using the blender-mcp (modelling live in an open Blender, viewport screenshot after each step, new scene), then prove it with a repeatable measuring script (triangle budget, watertight mesh, materials, origin, glTF export, renders). Use when asked to model, build or export a 3D asset for a game, set a triangle budget, or verify a mesh. Not for image-to-3D generation, texturing from photos, or animation.
```
Proposed replacement of the Rules block (items 2 and 6):
```
2. **Model through the blender-mcp** in a new scene of the open Blender (never edit existing objects). Look at a viewport screenshot after every step and compare with the design. Use bpy code only for bulk steps after the result is checked live.
...
6. **Measure with a script**: one re-runnable `Blender -b -P check.py` evaluates the final mesh, writes the check file, exports glTF and renders; deleting outputs and rerunning must give the same file. Under a Codex sandbox run `codex exec -s danger-full-access -C <dir> "<task>" < /dev/null` (also required for the Blender MCP).
```
Also update the measured example paragraph: "(script-only trial, superseded for modelling)".

## (b) Scenarios
| Id | Prompt | Should trigger | Pass criteria |
|---|---|---|---|
| B1 | "Make a low-poly wooden crate for the game in Blender, 500 triangle budget." | yes | (1) starts from an approved design image and samples hex colours; (2) models through blender-mcp with viewport checks (NEW; fails today's skill); (3) measures after modifiers: triangles vs budget, boundary edges 0, non-manifold 0, modifiers 0; (4) origin bottom centre, glTF Y-up, flat shading; (5) 3 renders and an honest diff vs the design; (6) shows the renders to the user |
| B2 | "Verify this exported tile mesh is game-ready: is it watertight and within budget?" | yes | (1) measure on the evaluated mesh; (2) writes a check file with the listed fields; (3) re-runnable script; (4) no claim from file existence |
| B3 | "Explain URP vs built-in render pipeline." | NO | no activation |
Near-miss: "Make a 3D chair from this photo with an image-to-3D tool" -> excluded by description; the answer should redirect to Blender (docs/SKILL_EVAL.md B2: skill did not activate; answer still correct).

## (c) Description
States WHEN: yes, but WHAT it describes is outdated (script headless). Verdict: needs rewrite to say blender-mcp (see proposal above).

## (d) Size
19 lines (<500). OK.

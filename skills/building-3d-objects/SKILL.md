---
name: building-3d-objects
description: How to build a game 3D object (tile, prop, character part) in Blender from an approved 2D design using the blender-mcp (modelling live in an open Blender, viewport screenshot after each step, in a new scene), then prove it with a repeatable measuring script (triangle budget, watertight mesh, materials, origin, glTF export, renders). Use when asked to model, build or export a 3D asset for a game, set a triangle budget, or verify a mesh. Not for image-to-3D generation, texturing from photos, or animation.
---

# Building 3D objects with Blender (blender-mcp)

Objects are modelled by hand-guided steps in a live Blender, from an approved 2D design, not by image-to-3D models. Modelling goes through the MCP; measuring goes through a script so it is repeatable. Decision of 2026-11-12: the script-only way took many rounds for the duck (`docs/MESH_LAB_RETRO.md` #1).

## Rules
1. **Start from an approved design image** and sample its colors (hex values) from the image; do not invent a palette. Look for existing assets first (`art/`, `knowledge/`, `~/project/inkwave`) and say where you looked.
2. **Model through the blender-mcp** (MCP `blender`, Blender already open). After every step take a viewport screenshot and compare it with the design; use the object summary tools; look up the API/manual with the docs search tools. Use bpy code (`execute_blender_code`) only for bulk steps, after the result is checked live.
3. **Build in a NEW scene.** The open Blender may hold other work (e.g. `Penguin`): never edit existing objects. Inspect the scene before changing anything.
4. **Measure with a script, not by eye.** `Blender -b -P check.py` is only for measuring, rendering, baking and export, never for shaping geometry. Measure after modifiers are applied: triangles vs budget, boundary edges 0, non-manifold edges 0, remaining modifiers 0, materials used, smooth faces, bounds, origin. Details and the measured example: `references/measuring-script.md`.
5. **Conventions:** origin at bottom center, real-world scale in units, glTF Y-up, flat shading for faceted looks, one material per color part.
6. **Render 3 views** (front, top, isometric), compare with the design and list differences honestly. Show the user the renders; they approve looks, not check files.
7. **Under Codex** use `codex exec -s danger-full-access -C <dir> "<task>" < /dev/null` (required for the Blender MCP and for GPU use; see the reference).
8. **3D branch only: report layer by layer** (shape, material, light/render with AO, tone mapping, bloom, grade, art direction): say which layers were done and which not. Do not tune one layer and report "done" (lesson 2026-10-14, `game-2/docs/PENGUIN3D_RETRO.md`). 2D ships first for game-2.

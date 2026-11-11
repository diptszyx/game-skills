---
name: building-3d-objects
description: How to build a game 3D object (tile, prop, character part) by Python script in Blender, headless, from an approved 2D design, and prove it with measured checks (triangle budget, watertight mesh, materials, origin, export, renders). Use when asked to model, build or export a 3D asset for a game, set a triangle budget, or verify a mesh. Not for image-to-3D generation, texturing from photos, or animation.
---

# Building 3D objects with Blender scripts

Objects are built by code from an approved 2D design, not by image-to-3D models. Code makes the result repeatable and checkable.

## Rules
1. **Start from an approved design image** and sample its colors (hex values) from the image; do not invent a palette.
2. **One re-runnable script** builds, checks, exports and renders. Running it from scratch (outputs deleted) must reproduce the same check file.
3. **Measure after modifiers are applied.** Evaluate the final mesh (not the base mesh) and write a check file with: triangle count vs budget, boundary edges == 0, non-manifold edges == 0, remaining modifiers == 0, materials actually used, smooth-shaded face count (0 for a faceted low-poly look), bounds, origin.
4. **Conventions:** origin at bottom center, real-world scale in units, glTF export Y-up; flat shading for faceted looks; one material per color part.
5. **Render 3 views** (front, top, isometric) with neutral light at 512x512 and look at them against the design; list differences honestly.
6. **Run Blender headless** (`/Applications/Blender.app/Contents/MacOS/Blender -b -P script.py`). Under a Codex sandbox use `codex exec -s danger-full-access` so Blender can use the GPU.

Measured example (2026-11-12, Blender 5.2.2): an ice tile with a snow chunk came out at 96 triangles (budget 2000), 0 open edges, 10 materials, built in about 3.8 minutes by Codex; Claude re-ran the script from scratch and got the same check file. Gap vs the design: smoother top and a smaller snow chunk. Files: `art/blender_trial/`.
Remember: show the user the renders; they approve looks, not check files.

# T5: one 3D object built by code in Blender (trial)

Goal: build a low-poly stylized ICE BLOCK TILE with a small chunk of snow on top (a game-board tile) by a Blender Python script, matching board 4 "low-poly" in /Users/tho/project/indie-game/game-skills/art/style_trial/board_4_lowpoly.png (look at it; the plain-ice tile is the target look: pale blue faceted block, slightly beveled, flat top).
Blender binary: /Applications/Blender.app/Contents/MacOS/Blender (run with -b, no UI). Use codex exec with danger-full-access so Blender can use the GPU.
Files (only): art/blender_trial/build_tile.py, art/blender_trial/tile.glb, art/blender_trial/render_front.png, render_top.png, render_iso.png, art/blender_trial/check.json, art/blender_trial/REPORT.md. No git. Do not touch game-2/3/5/game-kit.
Requirements, each MEASURED by a check script that runs after all modifiers are applied (evaluate the mesh, not the base mesh) and writes check.json:
1. Triangle count <= 2000 (count after triangulation).
2. Open (boundary) edges == 0 and non-manifold edges == 0.
3. Separate materials/colors per part: ice body, snow cap (>= 2 materials), flat-shaded low-poly look (no smooth shading on faceted body), colors from the board image (sample them, report the hex values you chose).
4. Origin at bottom center, size 1 x 1 x 0.25 units for the block (snow cap included in height up to 0.3), Y-up export to glb.
5. Three renders from front, top, isometric at 512x512 with a neutral light; look at them yourself and say if they match board 4's tile.
6. The script must be re-runnable from scratch (delete outputs, run, same check.json).
Time cap 30 minutes; if a requirement fails after 2 fixes, report the measured numbers and stop.
Report: measured numbers, hex colors, wall-clock minutes, what differs from board 4.

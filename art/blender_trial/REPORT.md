# Blender ice tile trial

Completed in approximately **3.8 wall-clock minutes**, including reference inspection, modeling, rendering, visual review, lighting adjustments, and a clean reproducibility run. Blender **5.2.2 LTS**, headless. Only the seven requested deliverable files are present here.

## Measured result

The checker is embedded in `build_tile.py` (`check_evaluated`). It runs after applying the triangulation modifier and reads the dependency-graph evaluated mesh, with `calc_loop_triangles()` and BMesh edge classification. It writes `check.json`; all seven recorded pass flags are true.

| Check | Measured result |
|---|---|
| Triangles, evaluated and triangulated | **96 / 2000 maximum** |
| Boundary edges | **0** |
| Non-manifold edges, including boundary/wire edges | **0** |
| Applied modifiers remaining | **0** |
| Material slots actually used | **10**: 7 ice, 3 snow |
| Smooth-shaded faces | **0** |
| Ice body dimensions, Blender XYZ | **1 × 1 × 0.25** |
| Combined dimensions, Blender XYZ | **1 × 1 × 0.30** |
| Combined bounds, Blender XYZ | **(-0.5, -0.5, 0) → (0.5, 0.5, 0.3)** |
| Object origin | **(0, 0, 0)**, centered at the ice bottom |
| Ice top elevation / elevation spread | **0.25 / 0**: coplanar top |
| Snow maximum elevation | **0.30** |
| Closed connected shells | **2**, ice and snow in one mesh object |
| Serialized GLB meshes | **1** |
| Serialized GLB POSITION bounds | **(-0.5, 0, -0.5) → (0.5, 0.3, 0.5)** |
| Export orientation | **Y up**, `export_yup=True`; verified against serialized accessor bounds |
| Front / top / isometric | **512 × 512 each**, loaded back and measured |

The snow shell sits slightly inside the body (bottom Z 0.244) to avoid a visible gap. Both shells are independently closed; they overlap at the attachment rather than forming a Boolean union. This is a single game asset, not a single connected printable solid. No textures, external libraries, smooth normals, cameras, or lights are exported in the GLB.

## Sampled palette

Exact sRGB pixels sampled from `../style_trial/board_4_lowpoly.png`. Coordinates use the image's upper-left origin. Values are converted from sRGB to linear before assignment to Blender materials.

| Part / material | Hex | Reference pixel (x, y) |
|---|---|---|
| Ice pale | `#B6E2FD` | 402, 350 |
| Ice light | `#A4DBFC` | 390, 370 |
| Ice blue | `#8BCEFD` | 420, 420 |
| Ice facet | `#72C4FA` | 460, 410 |
| Ice bevel | `#5BBFF1` | 375, 446 |
| Ice side | `#319BDE` | 477, 442 |
| Ice edge | `#36A3E9` | 488, 411 |
| Snow white | `#FDFCFC` | 480, 22 |
| Snow shade | `#F6F3FD` | 469, 24 |
| Snow blue | `#BFCEFA` | 111, 30 |

The seven ice samples come from the large plain tile; snow samples come from snowy trim elsewhere on the same board.

## Visual review against board 4

I opened and inspected all three final PNGs alongside the board reference. **The result matches the broad style and silhouette**, with a pale blue flat faceted top, clipped corners, a narrow bevel, darker blue sides, and a small angular snow patch. The front view confirms the shallow block profile, the top view confirms the exposed flat playing surface, and the isometric view shows the bevel and body depth.

Differences: the reference has more irregular, finer crystalline facets and a more polished illustrated edge highlight. This trial uses fewer, larger triangles, more regular sides, and matte opaque ice. It has no painted glints or translucent appearance. The small rear-corner snow patch is required for this trial but is absent from the large plain-tile reference. The front render naturally hides most of the rear snow patch. The neutral gray background differs from the reference board's pale blue background. This is a style match, not an exact reconstruction.

Final renders use a white area light, neutral environment, Cycles at 48 samples, and Standard color management. A headless Metal initialization attempt stalled without producing a render; that task-owned process was stopped. CPU Cycles completed successfully in roughly four seconds per complete three-view run. Two lighting adjustments were made; no geometric requirement failed.

## Reproduce

From the project root:

```sh
/Applications/Blender.app/Contents/MacOS/Blender -b --factory-startup --python art/blender_trial/build_tile.py
```

The script clears the scene and recreates the asset, export, renders, and checks. It requires only Blender and the existing board reference. No `.blend` file or supporting asset is required.

For the final verification, I kept comparison hashes in memory, **deleted `tile.glb`, all three PNGs, and `check.json`**, then ran the exact command above. Blender exited 0. Results:

- `check.json`: byte-identical; SHA-256 `5e28dc0d0fc742ae10afb66752097753dc655f25c938beffa1d42ad9c2d1b4f4`.
- `tile.glb`: byte-identical; SHA-256 `12264acb02af46ec5cd432364633913fbe4fb28aa269cef74da855bf0bb9fe27`.
- All three render pixel buffers: identical. PNG file metadata can contain different render times, so image comparison used decoded pixels.

No Git commands were run, and no game-2/3/5/game-kit files were touched. No task-started background process remains.

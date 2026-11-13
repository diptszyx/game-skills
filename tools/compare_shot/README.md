# compare_shot

Structural gate: compares a screenshot with an approved reference and exits 1 on mismatch. Pillow + numpy only.

```
python3 tools/compare_shot/compare_shot.py --shot work/ref/g_home.png --ref work/ref/g_home.png \
  [--regions regions.json] [--out side_by_side.png] [--shot-crop x0,y0,x1,y1] [--ref-crop x0,y0,x1,y1]
python3 tools/compare_shot/selftest.py   # proves the gate can fail
```

Metrics per region (both images are resized to 360x800 first):
- `color`: mean RGB distance (0..1) on a Gaussian-blurred 12px grid, so 1-2px shifts are not penalised.
- `edge`: 1 - Pearson correlation of the blurred gradient-magnitude grid (0 = same edge layout, ~1 = unrelated).

`--regions` takes a file or a JSON string of normalized rects with optional per-region limits, e.g.
`{"topbar": {"rect": [0, 0, 1, 0.09], "color": 0.1, "edge": 0.5}}`. Defaults: topbar/board/tabbar, color 0.12, edge 0.60.
`--ref-crop` selects one panel of a multi-screen mockup (left third of `art/style_lock/reference_chunky.png` is `0,0,0.3333,1`).

Limits: the chunky mockup is not the game's layout, so against it the thresholds do not separate right from wrong screens
(see selftest info line). Use the mockup only with loose per-region limits; for regression use an approved real screenshot as `--ref`.

#!/usr/bin/env python3
import argparse
import sys

import numpy as np
from PIL import Image
from scipy import ndimage as ndi

MAGENTA_MAX_FRACTION = 0.0005
SQUARE_MIN_FILL = 0.90
SQUARE_MIN_AREA_FRACTION = 0.002
SQUARE_MAX_AREA_FRACTION = 0.10
PANEL_OUTLINE_MAX_RATIO = 1.3
PANEL_INTERIOR_MAX_STD = 3.0
DARK_LUM = 100


def load(path):
    return np.array(Image.open(path).convert("RGBA")).astype(int)


def luminance(a):
    return a[..., 0] * 0.3 + a[..., 1] * 0.59 + a[..., 2] * 0.11


def check_magenta(a):
    opaque = a[..., 3] > 200
    m = opaque & (a[..., 0] > 150) & (a[..., 2] > 150) & (a[..., 1] < 80)
    frac = m.sum() / max(1, opaque.sum())
    return [f"magenta fringe fraction {frac:.4f} > {MAGENTA_MAX_FRACTION}"] if frac > MAGENTA_MAX_FRACTION else []


def check_squares(a):
    opaque = a[..., 3] > 200
    white = opaque & (luminance(a) > 215)
    lab, n = ndi.label(white)
    problems = []
    for i, sl in enumerate(ndi.find_objects(lab), 1):
        comp = lab[sl] == i
        area = comp.sum()
        fraction = area / opaque.sum()
        if not SQUARE_MIN_AREA_FRACTION <= fraction <= SQUARE_MAX_AREA_FRACTION:
            continue
        h, w = comp.shape
        if 0.7 < w / h < 1.4 and area / (h * w) > SQUARE_MIN_FILL:
            problems.append(f"white square block {w}x{h} at x={sl[1].start} y={sl[0].start}")
    return problems


def outline_runs(line, opaque_line):
    dark = (line < DARK_LUM) & opaque_line
    idx = np.where(opaque_line)[0]
    if len(idx) == 0:
        return []
    dark = dark[idx[0]:idx[-1] + 1]
    runs, cur = [], 0
    for d in dark:
        if d:
            cur += 1
        elif cur:
            runs.append(cur)
            cur = 0
    return runs


def check_panel(a):
    lum = luminance(a)
    opaque = a[..., 3] > 200
    h, w = lum.shape
    row = int(h * 0.62)
    col = w // 2
    left = outline_runs(lum[row, : w // 2], opaque[row, : w // 2])
    right = outline_runs(lum[row, w // 2:][::-1], opaque[row, w // 2:][::-1])
    bottom = outline_runs(lum[h // 2:, col][::-1], opaque[h // 2:, col][::-1])
    problems = []
    firsts = [r[0] for r in (left, right, bottom) if r]
    if len(firsts) == 3 and max(firsts) / min(firsts) > PANEL_OUTLINE_MAX_RATIO:
        problems.append(f"uneven outer outline thickness L/R/B = {firsts}")
    if len(left) != len(right):
        problems.append(f"left has {len(left)} dark runs, right has {len(right)} (double frame?)")
    free = opaque & ~((lum < DARK_LUM) & opaque)
    lab, _ = ndi.label(free)
    interior = ndi.binary_fill_holes(lab == lab[int(h * 0.78), w // 2])
    core = ndi.binary_erosion(interior, iterations=max(8, w // 25))
    std = a[core][:, :3].std(axis=0).max()
    if std > PANEL_INTERIOR_MAX_STD:
        problems.append(f"interior not flat: colour std {std:.1f} > {PANEL_INTERIOR_MAX_STD}")
    return problems


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+")
    ap.add_argument("--panel", action="store_true", help="also run panel checks")
    args = ap.parse_args()
    bad = 0
    for f in args.files:
        a = load(f)
        problems = check_magenta(a) + check_squares(a)
        if args.panel:
            problems += check_panel(a)
        print(("FAIL " if problems else "ok   ") + f)
        for p in problems:
            print("     - " + p)
        bad += bool(problems)
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()

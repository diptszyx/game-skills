#!/usr/bin/env python3
"""Structural screenshot gate: blurred colour grid + edge layout, per named region."""
import argparse
import json
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

COMMON_SIZE = (360, 800)
COLOR_BLUR = 6
EDGE_BLUR = 2
CELL = 12
DEFAULT_COLOR_MAX = 0.12
DEFAULT_EDGE_MAX = 0.60
DEFAULT_REGIONS = {
    "topbar": {"rect": [0, 0, 1, 0.09]},
    "board": {"rect": [0, 0.09, 1, 0.88]},
    "tabbar": {"rect": [0, 0.88, 1, 1]},
}


def load(path, crop=None):
    img = Image.open(path).convert("RGB")
    if crop:
        w, h = img.size
        x0, y0, x1, y1 = crop
        img = img.crop((round(x0 * w), round(y0 * h), round(x1 * w), round(y1 * h)))
    return img.resize(COMMON_SIZE, Image.LANCZOS)


def region_box(rect):
    w, h = COMMON_SIZE
    x0, y0, x1, y1 = rect
    return round(x0 * w), round(y0 * h), round(x1 * w), round(y1 * h)


def grid(img, blur):
    w, h = img.size
    blurred = img.filter(ImageFilter.GaussianBlur(blur))
    return np.asarray(blurred.resize((max(1, w // CELL), max(1, h // CELL)), Image.BOX), dtype=np.float64)


def color_distance(shot, ref):
    diff = grid(shot, COLOR_BLUR) - grid(ref, COLOR_BLUR)
    return float(np.sqrt((diff ** 2).sum(axis=-1)).mean() / np.sqrt(3 * 255 ** 2))


def edge_grid(img):
    gray = np.asarray(img.convert("L").filter(ImageFilter.GaussianBlur(EDGE_BLUR)), dtype=np.float64)
    gy, gx = np.gradient(gray)
    mag = np.hypot(gx, gy)
    h, w = (mag.shape[0] // CELL) * CELL, (mag.shape[1] // CELL) * CELL
    if h == 0 or w == 0:
        return np.array([mag.mean()])
    return mag[:h, :w].reshape(h // CELL, CELL, w // CELL, CELL).mean(axis=(1, 3)).ravel()


def edge_distance(shot, ref):
    a, b = edge_grid(shot), edge_grid(ref)
    a, b = a - a.mean(), b - b.mean()
    na, nb = float(np.sqrt((a * a).sum())), float(np.sqrt((b * b).sum()))
    if na < 1e-6 and nb < 1e-6:
        return 0.0
    if na < 1e-6 or nb < 1e-6:
        return 1.0
    return float(1 - float((a * b).sum()) / (na * nb))


def compare(shot, ref, regions):
    rows = []
    for name, spec in regions.items():
        box = region_box(spec["rect"])
        s, r = shot.crop(box), ref.crop(box)
        for metric, fn, default in (("color", color_distance, DEFAULT_COLOR_MAX), ("edge", edge_distance, DEFAULT_EDGE_MAX)):
            value, limit = fn(s, r), spec.get(metric, default)
            rows.append({"region": name, "metric": metric, "value": value, "threshold": limit, "pass": value <= limit})
    return rows


def side_by_side(shot, ref, regions, rows, path):
    failed = {row["region"] for row in rows if not row["pass"]}
    w, h = COMMON_SIZE
    canvas = Image.new("RGB", (w * 2 + 10, h), "white")
    canvas.paste(shot, (0, 0))
    canvas.paste(ref, (w + 10, 0))
    draw = ImageDraw.Draw(canvas)
    for name, spec in regions.items():
        x0, y0, x1, y1 = region_box(spec["rect"])
        colour = "red" if name in failed else "lime"
        for dx in (0, w + 10):
            draw.rectangle((x0 + dx, y0, x1 - 1 + dx, y1 - 1), outline=colour, width=2)
        draw.text((x0 + 4, y0 + 4), name, fill=colour)
    canvas.save(path)


def run(shot_path, ref_path, regions=None, out=None, shot_crop=None, ref_crop=None, quiet=False):
    regions = regions or DEFAULT_REGIONS
    shot, ref = load(shot_path, shot_crop), load(ref_path, ref_crop)
    rows = compare(shot, ref, regions)
    if out:
        side_by_side(shot, ref, regions, rows, out)
    if not quiet:
        print(f"{'region':<10}{'metric':<8}{'value':>8}{'threshold':>11}  result")
        for row in rows:
            print(f"{row['region']:<10}{row['metric']:<8}{row['value']:>8.3f}{row['threshold']:>11.3f}  {'PASS' if row['pass'] else 'FAIL'}")
    ok = all(row["pass"] for row in rows)
    if not quiet:
        print("GATE PASS" if ok else "GATE FAIL")
    return ok, rows


def parse_crop(text):
    if not text:
        return None
    values = [float(v) for v in text.split(",")]
    if len(values) != 4:
        raise SystemExit(f"crop needs x0,y0,x1,y1 normalized, got {text!r}")
    return values


def main(argv=None):
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--shot", required=True)
    p.add_argument("--ref", required=True)
    p.add_argument("--regions", help="JSON file or string: {name: {rect:[x0,y0,x1,y1], color:max, edge:max}}")
    p.add_argument("--out", help="side-by-side PNG path")
    p.add_argument("--shot-crop", help="normalized x0,y0,x1,y1 of the shot to compare")
    p.add_argument("--ref-crop", help="normalized x0,y0,x1,y1 of the reference panel to compare")
    args = p.parse_args(argv)
    regions = None
    if args.regions:
        try:
            with open(args.regions) as f:
                regions = json.load(f)
        except OSError:
            regions = json.loads(args.regions)
    ok, _ = run(args.shot, args.ref, regions, args.out, parse_crop(args.shot_crop), parse_crop(args.ref_crop))
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())

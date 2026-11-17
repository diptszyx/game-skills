#!/usr/bin/env python3
import sys

import numpy as np
from PIL import Image
from scipy import ndimage as ndi

DARK_LUM = 100
SEED_Y_FRACTION = 0.78
RING_SAMPLE_PX = 25
EDGE_KEEP_PX = 2
CROP_ALPHA = 40


def flatten(src, dst, target_aspect=None):
    a = np.array(Image.open(src).convert("RGBA")).astype(int)
    ys, xs = np.where(a[..., 3] > CROP_ALPHA)
    a = a[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    h, w = a.shape[:2]
    lum = a[..., 0] * 0.3 + a[..., 1] * 0.59 + a[..., 2] * 0.11
    opaque = a[..., 3] > 200
    free = ~((lum < DARK_LUM) & opaque)
    lab, _ = ndi.label(free)
    interior = ndi.binary_fill_holes(lab == lab[int(h * SEED_Y_FRACTION), w // 2])
    ring = interior & ~ndi.binary_erosion(interior, iterations=RING_SAMPLE_PX)
    colour = np.median(a[ring][:, :3], axis=0)
    fill = ndi.binary_erosion(interior, iterations=EDGE_KEEP_PX)
    a[fill, :3] = colour
    a[fill, 3] = 255
    if target_aspect:
        a = shrink_interior_rows(a, interior, int(round(w * target_aspect)))
    Image.fromarray(a.astype("uint8")).save(dst)


def shrink_interior_rows(a, interior, target_h):
    h = a.shape[0]
    drop = h - target_h
    if drop == 0:
        return a
    rows = np.where(interior.sum(axis=1) > 0.8 * interior.sum(axis=1).max())[0]
    mid = (rows.min() + rows.max()) // 2
    if drop > 0:
        start = mid - drop // 2
        return np.delete(a, np.arange(start, start + drop), axis=0)
    return np.insert(a, [mid] * (-drop), a[mid][None, :, :], axis=0)


if __name__ == "__main__":
    flatten(sys.argv[1], sys.argv[2], float(sys.argv[3]) if len(sys.argv) > 3 else None)

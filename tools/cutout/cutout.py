"""Cut magenta-background art into transparent, trimmed PNGs; optionally split a sheet into parts.

usage: cutout.py <in_dir> <out_dir> [split=name1,name2:parts...]   e.g. split=icons:6
Flood-fills magenta-ish pixels from the image border only (never inside), so bright interior pixels survive.
"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage as ndi

SKIP_BG = {"bg_home", "bg_play"}


def magenta_mask(a):
    r, g, b = a[..., 0].astype(int), a[..., 1].astype(int), a[..., 2].astype(int)
    return (r > 170) & (b > 170) & (g < 120) & (abs(r - b) < 90)


def cut(im):
    a = np.asarray(im.convert("RGB"))
    lab, _ = ndi.label(magenta_mask(a))
    border = set(np.unique(np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]]))) - {0}
    bg = np.isin(lab, list(border))
    alpha = np.where(bg, 0, 255).astype(np.uint8)
    alpha = ndi.binary_opening(alpha > 0, iterations=1).astype(np.uint8) * 255
    rgba = np.dstack([a, alpha])
    return Image.fromarray(rgba, "RGBA")


def trim(im, pad=4):
    box = im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox()
    return im.crop((max(box[0] - pad, 0), max(box[1] - pad, 0), box[2] + pad, box[3] + pad))


def split(im, n):
    al = np.asarray(im.getchannel("A")) > 8
    al = ndi.binary_dilation(al, iterations=12)
    lab, k = ndi.label(al)
    objs = ndi.find_objects(lab)
    big = sorted(((o[1].start, o) for o in objs if (o[0].stop - o[0].start) > 40), key=lambda t: t[0])
    parts = [im.crop((o[1].start, o[0].start, o[1].stop, o[0].stop)) for _, o in big]
    return parts if len(parts) == n else None


def main(argv):
    src, dst = Path(argv[0]), Path(argv[1])
    dst.mkdir(parents=True, exist_ok=True)
    splits = {}
    for s in argv[2:]:
        k, n = s.replace("split=", "").split(":")
        splits[k] = int(n)
    for f in sorted(src.glob("*.png")):
        if f.stem in ("montage",):
            continue
        im = Image.open(f)
        if f.stem in SKIP_BG:
            im.convert("RGB").save(dst / f.name)
            print(f.stem, "background kept", im.size)
            continue
        out = trim(cut(im))
        if f.stem in splits:
            parts = split(out, splits[f.stem])
            if parts is None:
                print(f.stem, "SPLIT FAILED, saved whole")
            else:
                for i, p in enumerate(parts):
                    trim(p, 2).save(dst / f"{f.stem}_{i + 1}.png")
                print(f.stem, "split into", len(parts))
                continue
        out.save(dst / f.name)
        print(f.stem, out.size)


if __name__ == "__main__":
    main(sys.argv[1:])

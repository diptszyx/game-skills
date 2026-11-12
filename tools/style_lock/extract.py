"""Lock step of the style workshop: from the CHOSEN sample image, cut exemplar crops and measure style numbers.

usage: extract.py <chosen.png> <out_dir> name=x0,y0,x1,y1 [name=...]   (box as fractions 0..1 of the image)
writes <out_dir>/exemplars/<name>.png and <out_dir>/style_numbers.json
Claude then writes STYLE_BRIEF.md from style_numbers.json + looking at the exemplars (see art/style_lock/STYLE_BRIEF.md as the template).
"""
import json, sys
from pathlib import Path
import numpy as np
from PIL import Image


def palette(im, n=8):
    q = im.resize((120, max(1, int(120 * im.height / im.width)))).quantize(colors=n, method=Image.MEDIANCUT).convert("RGB")
    a = np.asarray(q).reshape(-1, 3)
    u, c = np.unique(a, axis=0, return_counts=True)
    return [["#%02x%02x%02x" % tuple(u[i]), round(float(c[i] / c.sum()), 2)] for i in np.argsort(-c)]


def outline(im):
    m = np.asarray(im).astype(int)
    lum = m.sum(2)
    mask = lum < np.percentile(lum, 6)
    colour = "#%02x%02x%02x" % tuple(m[mask].mean(0).astype(int))
    runs = []
    for row in mask[::4]:
        n = 0
        for v in row:
            if v:
                n += 1
            elif n:
                runs.append(n)
                n = 0
    thick = float(np.median(runs)) if runs else 0.0
    return {"colour": colour, "median_px": thick, "ratio_of_height": round(thick / m.shape[0], 4)}


def main(argv):
    src, out = Path(argv[0]), Path(argv[1])
    ref = Image.open(src).convert("RGB")
    (out / "exemplars").mkdir(parents=True, exist_ok=True)
    result = {"source": str(src), "size": list(ref.size), "crops": {}}
    for spec in argv[2:]:
        name, box = spec.split("=")
        x0, y0, x1, y1 = (float(v) for v in box.split(","))
        crop = ref.crop((int(x0 * ref.width), int(y0 * ref.height), int(x1 * ref.width), int(y1 * ref.height)))
        crop.save(out / "exemplars" / f"{name}.png")
        result["crops"][name] = {"palette": palette(crop), "outline": outline(crop)}
    (out / "style_numbers.json").write_text(json.dumps(result, indent=1))
    print(json.dumps({k: v["outline"] for k, v in result["crops"].items()}, indent=1))


if __name__ == "__main__":
    main(sys.argv[1:])

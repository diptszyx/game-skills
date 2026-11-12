"""Build a shuffled, anonymised calibration set: real shipped-game screens vs AI-generated screens."""
import json, random, sys
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(sys.argv[1])
CANVAS = (720, 1100)
BG = (128, 128, 128)

real = {f"real_{n}": ROOT / "work/ref" / f for n, f in
        [("colony_home", "c0.png"), ("colony_play", "c1.png"), ("holeit_home", "h0.png"),
         ("holeit_popup", "h1.png"), ("holeit_play", "h3.png")]}

def thirds(path, tag):
    im = Image.open(path).convert("RGB"); w = im.width // 3
    return {f"{tag}_{n}": im.crop((k * w, 0, (k + 1) * w, im.height)) for k, n in enumerate(["home", "play", "win"])}

ai = {}
for b, f in [("v3_painterly", "board_1_painterly"), ("v3_chunky", "board_2_chunky"), ("v3_candy", "board_3_candy")]:
    ai.update(thirds(ROOT / f"art/style_trial_v3/{f}.png", b))
for b, f in [("v2_candy", "board_1_candy_toy"), ("v2_cartoon", "board_2_cartoon_2d"), ("v2_clay", "board_3_matte_clay")]:
    im = Image.open(ROOT / f"art/style_trial_v2/{f}.png").convert("RGB")
    ai[f"{b}_play"] = im.crop((0, 0, im.width // 2 - 60, im.height))

def fit(im):
    s = min(CANVAS[0] / im.width, CANVAS[1] / im.height)
    im = im.resize((int(im.width * s), int(im.height * s)))
    c = Image.new("RGB", CANVAS, BG); c.paste(im, ((CANVAS[0] - im.width) // 2, (CANVAS[1] - im.height) // 2))
    return c

items = [(k, Image.open(p).convert("RGB"), "real") for k, p in real.items()] + [(k, im, "ai") for k, im in ai.items()]
random.Random(7).shuffle(items)
OUT.mkdir(parents=True, exist_ok=True)
key = {}
for i, (name, im, label) in enumerate(items):
    fid = f"img_{i:02d}"
    fit(im).save(OUT / f"{fid}.png"); key[fid] = {"source": name, "label": label}
(OUT / "key.json").write_text(json.dumps(key, indent=1))
print(len(items), "images", sum(v["label"] == "real" for v in key.values()), "real")

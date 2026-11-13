#!/usr/bin/env python3
"""Proves the gate can fail: run `python3 tools/compare_shot/selftest.py` from the repo root."""
import sys
import tempfile
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw

import compare_shot

ROOT = Path(__file__).resolve().parents[2]
HOME = ROOT / "work/ref/g_home.png"
PLAY = ROOT / "work/ref/g_play.png"
MOCKUP = ROOT / "art/style_lock/reference_chunky.png"
MOCKUP_HOME_PANEL = [0, 0, 1 / 3, 1]


def corrupt_topbar(src, dst):
    img = Image.open(src).convert("RGB")
    ImageDraw.Draw(img).rectangle((0, 0, img.width, int(img.height * 0.09)), fill=(128, 128, 128))
    img.save(dst)


def shift(src, dst, dx):
    ImageChops.offset(Image.open(src).convert("RGB"), dx, 0).save(dst)


def check(name, expect_pass, ok):
    verdict = "ok" if ok == expect_pass else "WRONG"
    print(f"[{verdict}] {name}: gate {'PASS' if ok else 'FAIL'}, expected {'PASS' if expect_pass else 'FAIL'}")
    return ok == expect_pass


def main():
    with tempfile.TemporaryDirectory() as tmp:
        grey, moved = Path(tmp) / "grey_topbar.png", Path(tmp) / "shift2px.png"
        corrupt_topbar(HOME, grey)
        shift(HOME, moved, 2)
        cases = [
            ("home vs itself", HOME, True),
            ("home vs home shifted 2px", moved, True),
            ("home vs play screen", PLAY, False),
            ("home vs grey top bar", grey, False),
        ]
        results = [check(name, expect, compare_shot.run(HOME, shot, quiet=True)[0]) for name, shot, expect in cases]
        _, rows = compare_shot.run(HOME, grey, quiet=True)
        failed = sorted({r["region"] for r in rows if not r["pass"]})
        results.append(failed == ["topbar"])
        print(f"[{'ok' if results[-1] else 'WRONG'}] grey top bar fails only region(s): {failed}")
        _, rows = compare_shot.run(HOME, MOCKUP, ref_crop=MOCKUP_HOME_PANEL, quiet=True)
        print("info, home vs mockup left panel (not asserted):", " ".join(f"{r['region']}.{r['metric']}={r['value']:.2f}" for r in rows))
    print("SELFTEST PASS" if all(results) else "SELFTEST FAIL")
    return 0 if all(results) else 1


if __name__ == "__main__":
    sys.exit(main())

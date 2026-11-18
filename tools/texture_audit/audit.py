#!/usr/bin/env python3
"""Static Unity texture audit: reads *.png/jpg/tga .meta files, never runs Unity."""
import argparse
import json
import os
import re
import struct
import sys
from collections import Counter, defaultdict

IMAGE_EXT = (".png", ".jpg", ".jpeg", ".tga")
BORDER_NAME_HINTS = ("panel", "button", "frame", "btn", "popup")
MAX_RECOMMENDED_SIZE = 2048

# TextureImporterFormat ids, read from UnityEditor.CoreModule.dll of Unity 6000.3.25f1.
# value: (name, block_w, block_h, bytes_per_block)
FORMATS = {
    -1: ("Default(no override)", 0, 0, 0),
    1: ("Alpha8", 1, 1, 1), 3: ("RGB24", 1, 1, 3), 4: ("RGBA32", 1, 1, 4),
    5: ("ARGB32", 1, 1, 4), 7: ("RGB16", 1, 1, 2), 13: ("RGBA16", 1, 1, 2),
    10: ("DXT1", 4, 4, 8), 12: ("DXT5", 4, 4, 16),
    24: ("BC6H", 4, 4, 16), 25: ("BC7", 4, 4, 16),
    34: ("ETC_RGB4", 4, 4, 8), 45: ("ETC2_RGB4", 4, 4, 8), 47: ("ETC2_RGBA8", 4, 4, 16),
    48: ("ASTC_4x4", 4, 4, 16), 49: ("ASTC_5x5", 5, 5, 16), 50: ("ASTC_6x6", 6, 6, 16),
    51: ("ASTC_8x8", 8, 8, 16), 52: ("ASTC_10x10", 10, 10, 16), 53: ("ASTC_12x12", 12, 12, 16),
    63: ("R8", 1, 1, 1),
}
MIP_FACTOR = 4.0 / 3.0


def format_name(fid):
    return FORMATS.get(fid, ("id%d" % fid,))[0]


def gpu_bytes(fid, w, h, mips):
    spec = FORMATS.get(fid)
    if not spec or spec[3] == 0 or not w or not h:
        return None
    _, bw, bh, bpb = spec
    base = -(-w // bw) * -(-h // bh) * bpb
    return int(base * MIP_FACTOR) if mips else base


def image_size(path):
    with open(path, "rb") as f:
        head = f.read(32)
        if head[:8] == b"\x89PNG\r\n\x1a\n":
            return struct.unpack(">II", head[16:24])
        if head[:2] == b"\xff\xd8":
            f.seek(2)
            while True:
                marker = f.read(2)
                if len(marker) < 2 or marker[0] != 0xFF:
                    return None
                if marker[1] in (0xC0, 0xC1, 0xC2):
                    h, w = struct.unpack(">HH", f.read(7)[3:7])
                    return w, h
                f.seek(struct.unpack(">H", f.read(2))[0] - 2, 1)
        if path.lower().endswith(".tga") and len(head) >= 16:
            return struct.unpack("<HH", head[12:16])
    return None


def is_pow2(n):
    return n > 0 and n & (n - 1) == 0


def parse_meta(text):
    def top(key, default=None):
        m = re.search(r"^ {2,4}%s: (-?\d+)" % key, text, re.M)
        return int(m.group(1)) if m else default

    android = re.search(
        r"buildTarget: Android\n(.*?)(?=\n  - serializedVersion|\n  spriteSheet|\n  \w)", text, re.S)
    a_fmt, a_over, a_max = None, 0, None
    if android:
        block = android.group(1)
        a_fmt = int(re.search(r"textureFormat: (-?\d+)", block).group(1))
        a_over = int(re.search(r"overridden: (\d)", block).group(1))
        a_max = int(re.search(r"maxTextureSize: (\d+)", block).group(1))
    borders = re.findall(r"border: \{x: (-?[\d.]+), y: (-?[\d.]+), z: (-?[\d.]+), w: (-?[\d.]+)\}", text)
    top_border = re.search(r"^  spriteBorder: \{x: (-?[\d.]+), y: (-?[\d.]+), z: (-?[\d.]+), w: (-?[\d.]+)\}", text, re.M)
    if top_border:
        borders.append(top_border.groups())
    return {
        "androidFormat": a_fmt if a_fmt is not None else -1,
        "androidOverridden": bool(a_over),
        "androidMaxSize": a_max,
        "defaultMaxSize": top("maxTextureSize"),
        "mipmap": top("enableMipMap", 0) == 1,
        "textureType": top("textureType", 0),
        "spriteMode": top("spriteMode", 0),
        "hasBorder": any(float(v) != 0 for b in borders for v in b),
    }


def scan(assets, assumed_default, excludes):
    rows = []
    for root, _, files in os.walk(assets):
        for name in files:
            if not name.lower().endswith(tuple(e + ".meta" for e in IMAGE_EXT)):
                continue
            meta_path = os.path.join(root, name)
            img_path = meta_path[:-5]
            rel = os.path.relpath(img_path, assets)
            if any(rel == x or rel.startswith(x.rstrip("/") + "/") for x in excludes):
                continue
            with open(meta_path, errors="replace") as f:
                meta = parse_meta(f.read())
            size = image_size(img_path) if os.path.exists(img_path) else None
            w, h = size if size else (None, None)
            fmt = meta["androidFormat"] if meta["androidOverridden"] and meta["androidFormat"] != -1 else -1
            effective = fmt if fmt != -1 else assumed_default
            max_size = meta["androidMaxSize"] if meta["androidOverridden"] else meta["defaultMaxSize"]
            ew, eh = w, h
            if w and max_size and max(w, h) > max_size:
                scale = max_size / max(w, h)
                ew, eh = max(1, round(w * scale)), max(1, round(h * scale))
            lname = name.lower()
            rows.append({
                "path": rel.replace(os.sep, "/"),
                "folder": "/".join(rel.replace(os.sep, "/").split("/")[:-1]),
                "androidFormat": fmt,
                "androidFormatName": format_name(fmt),
                "effectiveFormat": effective,
                "mipmap": meta["mipmap"],
                "maxTextureSize": max_size,
                "width": w, "height": h,
                "fileBytes": os.path.getsize(img_path) if os.path.exists(img_path) else None,
                "gpuBytes": gpu_bytes(effective, ew, eh, meta["mipmap"]),
                "isSprite": meta["textureType"] == 8 or meta["spriteMode"] != 0,
                "hasBorder": meta["hasBorder"],
                "nonPow2": bool(w and not (is_pow2(w) and is_pow2(h))),
                "over2048": bool(w and max(w, h) > MAX_RECOMMENDED_SIZE),
                "borderSuspect": (meta["textureType"] == 8 or meta["spriteMode"] != 0) and not meta["hasBorder"] and any(k in lname for k in BORDER_NAME_HINTS),
            })
    return rows


def folder_of(row, depth):
    return "/".join(row["folder"].split("/")[:depth]) or "."


def aggregate(rows, top_n, depth):
    gpu = lambda r: r["gpuBytes"] or 0
    mip_by_folder = Counter(folder_of(r, depth) for r in rows if r["mipmap"])
    unset_by_folder = Counter(folder_of(r, depth) for r in rows if r["androidFormat"] == -1)
    return {
        "count": len(rows),
        "formatCounts": {format_name(k): v for k, v in Counter(r["androidFormat"] for r in rows).most_common()},
        "mipmapOn": sum(r["mipmap"] for r in rows),
        "mipmapOnByFolder": dict(mip_by_folder.most_common()),
        "defaultFormatByFolder": dict(unset_by_folder.most_common()),
        "nonPow2": sum(r["nonPow2"] for r in rows),
        "over2048": sum(r["over2048"] for r in rows),
        "borderSuspects": [r["path"] for r in rows if r["borderSuspect"]],
        "fileBytesTotal": sum(r["fileBytes"] or 0 for r in rows),
        "gpuBytesTotal": sum(gpu(r) for r in rows),
        "top": [{k: r[k] for k in ("path", "width", "height", "androidFormatName", "mipmap", "fileBytes", "gpuBytes")}
                for r in sorted(rows, key=gpu, reverse=True)[:top_n]],
    }


def check_policy(rows, policy):
    allowed = {policy["default_format"], *policy.get("allowed_detail_ids", [])}
    mip_ok = tuple(p.rstrip("/") for p in policy.get("mipmap_allowed_folders", []))
    violations = []
    for r in rows:
        if r["androidFormat"] not in allowed:
            violations.append({"path": r["path"], "rule": "format", "detail": r["androidFormatName"]})
        if r["mipmap"] and not any(r["folder"] == p or r["folder"].startswith(p + "/") for p in mip_ok):
            violations.append({"path": r["path"], "rule": "mipmap", "detail": r["folder"]})
    return violations


def print_table(agg, violations):
    mb = lambda b: "%.1f MB" % (b / 1048576)
    print("textures: %d   file: %s   est. GPU: %s" % (agg["count"], mb(agg["fileBytesTotal"]), mb(agg["gpuBytesTotal"])))
    print("formats:", agg["formatCounts"])
    print("mipmap on: %d   NPOT: %d   >%d: %d" % (agg["mipmapOn"], agg["nonPow2"], MAX_RECOMMENDED_SIZE, agg["over2048"]))
    print("mipmap-on by folder:", agg["mipmapOnByFolder"])
    print("no Android override by folder:", agg["defaultFormatByFolder"])
    print("border suspects: %d" % len(agg["borderSuspects"]))
    print("%-90s %9s %-11s %s" % ("top by GPU", "WxH", "format", "GPU"))
    for t in agg["top"]:
        print("%-90s %9s %-11s %s" % (t["path"][-90:], "%sx%s" % (t["width"], t["height"]), t["androidFormatName"], mb(t["gpuBytes"] or 0)))
    if violations is not None:
        print("policy violations: %d" % len(violations))


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("assets", help="Unity project Assets directory")
    ap.add_argument("--policy", help="policy JSON: default_format, allowed_detail_ids, mipmap_allowed_folders")
    ap.add_argument("--json", help="write full JSON report to this path")
    ap.add_argument("--top", type=int, default=20)
    ap.add_argument("--depth", type=int, default=3, help="folder depth for per-folder counts")
    ap.add_argument("--exclude", action="append", default=[], help="path relative to Assets to skip (repeatable)")
    ap.add_argument("--assume-default-format", type=int, default=50,
                    help="format id assumed for textures without Android override when estimating GPU bytes")
    args = ap.parse_args()

    rows = scan(args.assets, args.assume_default_format, args.exclude)
    agg = aggregate(rows, args.top, args.depth)
    violations = None
    if args.policy:
        with open(args.policy) as f:
            violations = check_policy(rows, json.load(f))
    print_table(agg, violations)
    if args.json:
        with open(args.json, "w") as f:
            json.dump({"assumedDefaultFormat": args.assume_default_format, "excluded": args.exclude,
                       "aggregate": agg, "violations": violations, "files": rows}, f, indent=1)
    return 1 if violations else 0


if __name__ == "__main__":
    sys.exit(main())

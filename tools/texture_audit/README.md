# texture_audit

Static audit of Unity texture import settings. Reads `*.png/.jpg/.tga.meta` and image headers only; never starts Unity.

```
python3 -I audit.py <Project>/Assets [--exclude Art/OriginalLocal] [--policy policy.json] [--json out.json] [--top 20]
```

Per file: Android format (id decoded from Unity 6000.3.25f1 `TextureImporterFormat`, e.g. 48=ASTC_4x4, 50=ASTC_6x6, 51=ASTC_8x8, 52=ASTC_10x10), override flag, mipmap, maxTextureSize, pixel size, file bytes, estimated GPU bytes, NPOT, >2048, missing-border suspects (sprite named panel/button/frame/btn/popup with no border).

Textures with no Android override use `--assume-default-format` (default 50) for the GPU estimate; the real project default is not read.

Policy JSON (exit code 1 on any violation):

```json
{"default_format": 50, "allowed_detail_ids": [48, 51], "mipmap_allowed_folders": ["Art/Forest/Models"]}
```

Rules: Android format must be in `default_format` + `allowed_detail_ids` (so `-1` violates); mipmap-on files must sit under an allowed folder.

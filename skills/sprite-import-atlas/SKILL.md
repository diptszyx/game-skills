---
name: sprite-import-atlas
description: Rules for importing clean 2D sprites into Unity for Android (PNG source, ASTC compression, Sprite Atlas per screen, 9-slice border at import, mipmap choice, pixel-art preset) and for checking the result. Use when adding or changing sprite or texture import settings, building atlases, setting 9-slice borders or pixels per unit, choosing mipmaps, compression or filter mode, or when UI looks blurry, jagged or stretched. Not for cutting sprites out of AI images, drawing art, or animation.
---

# Sprite import and atlas (Unity, Android)

Import settings are cheap to set wrong and invisible in the Editor. The rules below come from projects that shipped 792 PNGs with no atlas, 14 borders declared out of 792, and UI scaled 2-4x down with mipmaps off.

## Rules
1. **PNG is the source, atlas is a build product.** Keep the PNG, never edit the atlas page. (`knowledge/colony-flow-research/11-sprite-png-atlas.md:34`; inventory A16)
2. **One Sprite Atlas per screen or feature** (common UI, shop, gameplay, home), so a screen loads only its pages. Reference game used `UI_Common`, `UI_Shop`, `UI_Home` and similar. (`11-sprite-png-atlas.md:12-14`, `:31`)
3. **Android compression is ASTC.** Use 6x6 as the default and **4x4 for small-detail art** (thin outlines, small icons, text-like shapes). Game-2 mixed both: 573 files at 6x6, 87 at 4x4. Check the result on the phone, not the Editor. (`11-sprite-png-atlas.md:20-26`, `:32`)
4. **Declare the 9-slice border at import**, for every frame and stretchable button. Then use Image type Sliced, Full Rect, and set `pixelsPerUnitMultiplier` = sprite height / displayed height so the border draws at the intended size. A sprite drawn 3.1x wider than it is looks bad even when it works. (`lessons/bai-hoc-chung.md:134`; `game-2/docs/UI_MATTE_RETRO.md:15`; `11-sprite-png-atlas.md:33`; inventory A17)
5. **Mipmaps: decide by downscale, measure on the phone.** A sprite drawn at about its source size needs none. A sprite shown at 2x smaller or more needs them, or edges alias. Do not switch mipmaps off for all UI. (`game-2/docs/UI_MATTE_RETRO.md:16`; inventory A18)
6. **Pixel art gets its own preset**: nearest filter, integer scale, hard alpha, no mipmaps, lossless. Smooth painted UI keeps bilinear. Never apply one preset to both, and re-check filter, scale, anchors and alpha fringe after import. (`game-1/docs/ART.md:32`; `learning/game/pixel-art-fundamentals.md:389,392`; inventory A19)
7. **Record source for every generated sprite**: prompt, model and version, and the command that regenerates it. When a shared source changes, list the outputs to redo. (`game-1/docs/ART.md:21`; `game-1/docs/PHASE1_RETRO.md:69`; inventory A20)
8. **Fit the sprite to the scene before producing motion**: camera angle, foot anchor, contact shadow, light. (`game-2/docs/SPRITE_RETRO.md:30`; `game-2/docs/PENGUIN2D_RETRO.md:13`; inventory A15)
9. **After changing a prompt, prove the output changed** (hash or pixel diff); tools that skip existing rows ignore the prompt. (`game-2/docs/SPRITE_RETRO.md:17`; inventory A14)

## Conflicting sources: the rule is "measure"
| Source | Says |
|---|---|
| `11-sprite-png-atlas.md:32` (Colony note) | ASTC on Android, mipmap off for all UI images |
| `game-2/docs/UI_MATTE_RETRO.md:16` | UI sprites 2-4x larger than display with mipmaps off looked jagged; mipmap on for 28 sprites, A/B on the phone fixed it |

Do not pick a side. Per sprite group: compute source size / displayed size on the target canvas; below 2 keep mipmaps off; at 2 or more, turn them on; screenshot both on the Galaxy A07 and keep the cleaner one. Note the choice and ms/frame in the game's docs. The Colony note also says atlas savings are unmeasured (`11-sprite-png-atlas.md` section 5), so measure draw calls too.

## Running the shell importer and audit
These tools are being built by other agents and may not exist yet. Check the files exist before using them; if absent, say so and stop rather than approximating.
- Importer: `TextureImportApplier` (shell package, editor-side). Run it through whatever the shell docs name for it; do not invent flags. Read its report and confirm each sprite group got compression, border and mipmap values from the rules above.
- Audit: `tools/texture_audit/audit.py`. Run it after import, read its table, and compare it with the checklist. Check `--help` before using any option.

## Checklist
- [ ] PNG sources present and untouched; atlas rebuilt, not hand-edited.
- [ ] One atlas per screen; no sprite packed into two atlases.
- [ ] Android format is ASTC; 4x4 only for small-detail art; sizes are multiples of the block where required.
- [ ] Every stretchable frame/button has a border and displays Sliced at the intended scale.
- [ ] Each sprite group has a recorded downscale ratio and mipmap decision, with an A07 screenshot.
- [ ] Pixel art and smooth UI use separate presets.
- [ ] Source prompt and model recorded for generated art.
- [ ] Unity log has 0 `error CS` after import; screenshots taken on the phone (see `verifying-game-results`).

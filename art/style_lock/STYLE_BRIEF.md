# STYLE BRIEF: system prompt for every image request in this game's art kit

(Tiếng Việt: đây là brief phong cách rút từ mẫu đã chốt `reference_chunky.png`, dùng làm system prompt cho Codex mỗi lần vẽ. Số đo lấy từ ảnh mẫu bằng script; chỉ một ảnh, coi là xấp xỉ.)

## Source of truth
Locked reference: `art/style_lock/reference_chunky.png` (chunky 2D cartoon). Exemplar crops: `art/style_lock/exemplars/*.png` (mascot_home, btn_primary_play, logo, popup_win, btn_next, tab_icons, hud_pills). BEFORE generating anything, open the exemplar closest to the asset and match it. If the generated piece does not look like the same hand drew it, it is wrong, regenerate.

## Style in one sentence
Chunky 2D cartoon: flat colour blocks, ONE dark plum outline of constant thickness, tiny hard-edged highlight, NO glossy plastic sheen, NO smooth gradients, light from upper left.

## Measured facts (from the reference, 2026-11-12)
- Outline colour: dark plum, about #300c31 to #471d51 (never pure black, never navy, never maroon). Same colour on EVERY piece.
- Outline thickness: about 1.3% of the object height in the reference image, constant on one piece; inner outlines of the same weight.
- Palette (use these, no other hues): red #d74449 / #e94232, coral-orange #fd7252, cream #fee9d1 / #fcf8f8, lavender #d9d6f0 / #ab94d0, periwinkle #98aceb / #728dd2, plum shadow #433359, pale gold #e6d59a, green for success #4fbf3a family. Backgrounds of scenes: pastel dusk lavender-pink-peach.
- Shading: at most 2 tones per colour (base + one darker bottom edge). One small crisp highlight shape per object at upper left, never a large soft glossy blob.
- Shadows: soft, plum-blue tinted, on the ground only.

## Hard DO
- Large simple shapes, constant outline, flat fills, one small hard highlight.
- Hero penguin: round body, cream belly, big black round eyes with a white highlight dot, small pink cheek, orange beak and feet, blue scarf, determined (slightly angled) brows. Same face on every penguin; only body and scarf colours change (red, blue, yellow).
- One primary action colour per screen (red-orange). Secondary actions calm (blue).
- Text on art: white, bold, rounded, plum outline, no gradient inside the letters.

## Hard DON'T (these caused drift last time)
- No glossy plastic look, no big specular blobs, no glass or candy sheen.
- No white-to-blue gradient inside letters or shapes; no inner glow; no bevel/emboss.
- No outline colour other than the plum; no thin hairline outlines.
- No texture, noise, fur, snow fluff, scratches, ice veins.
- No new colours outside the palette; no mixing of 3D-render and flat looks.

## Per-asset prompt template
"[STYLE SENTENCE above] [HARD DO] [HARD DON'T]. Match the attached exemplar <name>. Asset: <what>. Same outline colour #3a1238 and thickness as the exemplar. Flat solid #FF00FF background, centred, wide margin, no text unless stated." ATTACH the nearest exemplar as a reference image (`referenced_image_paths`, verified accepted). Text alone drifts toward glossy plastic.

## Self-check after every image (Codex must open the saved file and answer)
1. Outline colour is dark plum, not navy/maroon/black? 2. Any large glossy highlight or gradient? 3. Same face/proportions as the hero penguin (if a penguin)? 4. Colours only from the palette? Any "no" = regenerate once, then report the defect honestly.

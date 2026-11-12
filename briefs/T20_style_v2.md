# T20: three style boards v2, built from rules of real top games (trial, NOT user-approved)

Game concept (read only): portrait mobile puzzle, "Belly Slide". Colored penguins on an ice board; swipe a penguin to belly-slide until it hits an obstacle or another penguin; 3 same-color penguins touching huddle and leave. Ice cracks, holes, fish stop markers, seals blockers. Do NOT copy any existing art or any real game; use only the rules below. Previous boards (art/style_trial/) were judged "too AI-looking": dense detail, soft light everywhere, every object equally glossy, every button equally loud.

Goal: use image_gen to make THREE boards, each ONE PNG (largest size image_gen allows), saved to /Users/tho/project/indie-game/game-skills/art/style_trial_v2/board_<N>_<name>.png.
Styles (one each, all obey the same rules): 1 glossy candy-toy 3D (chunky, bevelled, one bright specular dot per object); 2 chunky 2D cartoon (thick rounded dark-tinted outline, flat colour blocks, ONE soft gradient per shape); 3 matte clay toy (fewer details than before, smooth, no fingerprints or fibres).

Hard rules to put into EVERY prompt (these come from measured real games):
1. Big simple shapes, one flat main colour per shape, one soft highlight on top, slightly darker bottom edge. NO fine texture, no ice veins, no snow fluff, no scratches, no noise.
2. Background = simple saturated dark-to-mid gradient (deep purple-blue), low detail, clearly receding. NOT pale/white/cream.
3. ONE light direction (upper left, warm). Every shadow falls lower right, soft, tinted purple-blue, never grey or black.
4. Hierarchy: exactly ONE loud primary button (large, saturated red-orange or yellow, thick light rim). Every other button and panel is calmer (muted, smaller). Hero element (penguin or board) is the brightest, biggest thing.
5. Board occupies 60-75% of the phone screen height inside a thick rounded frame of one colour family; tiles fill the board with no empty gaps.
6. Penguins are big: about 90% of a tile. Strong silhouette readable at 100 px. Colours red/blue/yellow separated by brightness too.
7. Text: bold white rounded font with a dark outline, very few words, all spelled correctly ("Belly Slide", "Play", "Win!", "Next", "Level 12").
8. Same quality everywhere: all objects share the same material and edge style (no mix of 3D and flat).

Layout (same on all three so they compare side by side): LEFT HALF = one large portrait phone gameplay screen (top bar: coin count, heart count, gear; board in frame with penguins, fish, seal, hole, cracked tile; bottom booster buttons; ONE primary Play/Next button). RIGHT HALF top = Win popup (3 stars, "Win!", "Next" primary button). RIGHT HALF middle = logo "Belly Slide". RIGHT HALF bottom = props row: coin, heart, gear, fish, seal, three penguins (red, blue, yellow) in idle pose, plus one red penguin belly-sliding.

Work: one generation attempt per board; retry once only if clearly broken (missing layout part, garbled text, mixed styles). Append the exact prompt used to art/style_trial_v2/PROMPTS.md (board number each). Edit only files under art/style_trial_v2/. No git. Cap 30 minutes. If image_gen is unavailable say so and stop.
Report: art/style_trial_v2/REPORT.md: each file, pixel size, which rules 1-8 and which layout parts are honestly present or violated (look at the image), minutes spent.
